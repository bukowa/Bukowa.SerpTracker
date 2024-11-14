using System.Net;
using System.Net.Sockets;

namespace Bukowa.SerpTracker.CefSharp.Proxies;

public class ExceptionInvalidProxyFormat(string message) : Exception(message);

public class Proxy
{
    public string Scheme { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}


public static class ProxyFormatParser
{
    public static readonly string[] AllowedSchemes = new[] { "http", "https", "socks5", "socks4" };

    public static Proxy Parse(string proxy)
    {
        proxy = proxy.Trim();
        var splitOne = proxy.Split("://");
        if (splitOne.Length != 2)
        {
            throw new ExceptionInvalidProxyFormat("Invalid proxy format");
        }

        if (!AllowedSchemes.Contains(splitOne[0]))
        {
            throw new ExceptionInvalidProxyFormat($"Invalid proxy format: {proxy} allowed schemes: {AllowedSchemes}");
        }

        var scheme = splitOne[0];

        var splitTwo = splitOne[1].Split(':');
        if (splitTwo.Length < 2)
        {
            throw new ExceptionInvalidProxyFormat($"Invalid proxy format: {proxy}");
        }

        var host = splitTwo[0];

        IPAddress.TryParse(host, out var address);
        if (address == null || address.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new ExceptionInvalidProxyFormat($"Invalid host in proxy: {proxy}");
        }
        int port;
        try
        {
            port = int.Parse(splitTwo[1]);
        }
        catch (FormatException exc)
        {
            throw new ExceptionInvalidProxyFormat(exc.Message);
        }

        switch (splitTwo.Length)
        {
            case 2:
                return new Proxy
                {
                    Scheme = scheme,
                    Host = host,
                    Port = port,
                };
            case 4:
                var username = splitTwo[2];
                var password = splitTwo[3];
                return new Proxy
                {
                    Scheme = scheme,
                    Host = host,
                    Port = port,
                    Username = username,
                    Password = password,
                };
            default:
                throw new ExceptionInvalidProxyFormat($"Invalid proxy format: {proxy}");
        }
    }
}