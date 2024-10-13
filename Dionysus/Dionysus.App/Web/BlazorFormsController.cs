using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace Dionysus.App.Web;

public class BlazorFormsController
{
    public static void Activate(Control.ControlCollection controls)
    {
        var _webView = new BlazorWebView();
        _webView.Dock = DockStyle.Fill;
        
        var _services = new ServiceCollection();
        _services.AddWindowsFormsBlazorWebView();
        _webView.HostPage = "Web\\wwwroot\\index.html";
        _webView.Services = _services.BuildServiceProvider();
        _webView.RootComponents.Add<Dionysus.Web.App>("#app");
        _webView.RootComponents.Add<HeadOutlet>("head::after");
        _webView.BackColor = ColorTranslator.FromHtml("#1b1b1b");
        
        controls.Add(_webView);
    }
}