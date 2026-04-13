using Microsoft.AspNetCore.HttpOverrides;
using ForwardedIpNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;
using System.Net;

namespace PortfolioSite.Models.Options;

public sealed class ReverseProxyOptions
{
    public bool TrustAllProxies { get; set; }
    public string KnownProxies { get; set; } = "";
    public string KnownNetworks { get; set; } = "";

    public IReadOnlyList<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (TrustAllProxies)
        {
            errors.Add("ReverseProxy:TrustAllProxies is not supported. Use KnownProxies or KnownNetworks instead.");
        }

        foreach (var proxy in SplitEntries(KnownProxies))
        {
            if (!IPAddress.TryParse(proxy, out _))
            {
                errors.Add($"ReverseProxy:KnownProxies contains an invalid IP address: {proxy}");
            }
        }

        foreach (var network in SplitEntries(KnownNetworks))
        {
            if (!TryParseNetwork(network, out _))
            {
                errors.Add($"ReverseProxy:KnownNetworks contains an invalid CIDR block: {network}");
            }
        }

        return errors;
    }

    public void ApplyTo(ForwardedHeadersOptions options)
    {
        var parsedProxies = SplitEntries(KnownProxies)
            .Select(IPAddress.Parse)
            .ToList();
        var parsedNetworks = SplitEntries(KnownNetworks)
            .Select(entry =>
            {
                TryParseNetwork(entry, out var network);
                return network;
            })
            .ToList();

        if (parsedProxies.Count == 0 && parsedNetworks.Count == 0)
        {
            return;
        }

        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();

        foreach (var proxy in parsedProxies)
        {
            options.KnownProxies.Add(proxy);
        }

        foreach (var network in parsedNetworks)
        {
            options.KnownNetworks.Add(network);
        }
    }

    private static List<string> SplitEntries(string? rawValue)
    {
        return (rawValue ?? string.Empty)
            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }

    private static bool TryParseNetwork(string rawValue, out ForwardedIpNetwork network)
    {
        network = null!;

        var parts = rawValue.Split(new[] { '/' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2
            || !IPAddress.TryParse(parts[0], out var prefix)
            || !int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        var maxPrefixLength = prefix.AddressFamily switch
        {
            System.Net.Sockets.AddressFamily.InterNetwork => 32,
            System.Net.Sockets.AddressFamily.InterNetworkV6 => 128,
            _ => -1
        };

        if (maxPrefixLength < 0 || prefixLength < 0 || prefixLength > maxPrefixLength)
        {
            return false;
        }

        network = new ForwardedIpNetwork(prefix, prefixLength);
        return true;
    }
}
