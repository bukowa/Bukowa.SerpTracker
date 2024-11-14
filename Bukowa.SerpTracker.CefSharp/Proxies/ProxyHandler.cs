using CefSharp;
using CefSharp.Handler;

namespace Bukowa.SerpTracker.CefSharp.Proxies;

public class ProxyRequestHandler(string user, string password) : RequestHandler
{
    protected override bool GetAuthCredentials(
        IWebBrowser chromiumWebBrowser,
        IBrowser browser,
        string originUrl,
        bool isProxy,
        string host,
        int port,
        string realm,
        string scheme,
        IAuthCallback callback
    )
    {
        if (!isProxy)
            return false;

        using (callback)
        {
            callback.Continue(username: user, password: password);
        }

        return true;
    }
}
