using System.Diagnostics;
using Dionysus.App.Helpers;
using Dionysus.App.Renders;
using Dionysus.App.Web;
using Dionysus.WebScrap.GOGScrapper;
using Dionysus.WebScrap.XatabScrapper;

namespace Dionysus.App.Forms;

public partial class MainWindow : Form
{
    private bool _isExiting = false;
    private static Mutex _mutex;
    public MainWindow()
    {
        _mutex = new Mutex(false, "DionysusMutex");

        if (!_mutex.WaitOne(0, false))
        {
            MessageBox.Show("The application is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit(); 
            Close();
        }
        
        InitializeComponent();
        
        var _arguments = Environment.GetCommandLineArgs();
        if (_arguments.Contains("-console")) ConsoleHelper.ShowConsoleWindow();

        Xatab.GetStatus();
        GOG.GetStatus();
        
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
        this.Text = "Dionysus";
        this.Icon = new Icon(iconPath);
        BlazorFormsController.Activate(this.Controls);
        this.MinimumSize = new Size(786, 747);
        this.StartPosition = FormStartPosition.CenterScreen;
        
        VisibleChanged += (sender, args) =>
        { 
            if (Visible) WindowsHelper.SetMicaTitleBar(this.Handle);
        };
        
        AppHelper.Logic(true);
        var _notifyIcon = new NotifyIcon();
        var _trayContextMenu = new ContextMenuStrip();
        _notifyIcon.Icon = new Icon(iconPath);
        _notifyIcon.Text = "Dionysus";
        _notifyIcon.ContextMenuStrip = _trayContextMenu;
        _notifyIcon.DoubleClick += (sender, args) =>
        {
            BringToFront();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            Show();
        };
        var openMenuItem = new ToolStripMenuItem("Open");
        openMenuItem.Click += (sender, args) =>
        {
            BringToFront();
            WindowState = FormWindowState.Normal;
            WindowsHelper.SetMicaTitleBar(Handle);
            ShowInTaskbar = true;
            Show();
        };
        var exitMenuItem = new ToolStripMenuItem("Exit");
        exitMenuItem.Click += (sender, args) =>
        {
            _isExiting = true; 
            Application.Exit();
        };
        _trayContextMenu.Items.Add(openMenuItem);
        _trayContextMenu.Items.Add(exitMenuItem);
        _trayContextMenu.RenderMode = ToolStripRenderMode.Professional;
        _trayContextMenu.Renderer = new ContextMenuStripRender();
        _notifyIcon.Visible = true;
        
        FormClosing += (sender, e) =>
        {
            if (!_isExiting)
            {
                e.Cancel = true;
                _notifyIcon.BalloonTipTitle = "Dionysus";
                _notifyIcon.BalloonTipText = "The program was minimized to tray";
                _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                _notifyIcon.ShowBalloonTip(5000);

                ShowInTaskbar = false;
                Hide();
                AppHelper.HideFromAltTab(Handle);
            }
            else
            {
                _mutex.ReleaseMutex();
                _notifyIcon.Visible = false; 
            }
        };
    }
}