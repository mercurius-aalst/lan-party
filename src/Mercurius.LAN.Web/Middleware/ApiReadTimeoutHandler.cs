namespace Mercurius.LAN.Web.Middleware;

internal sealed class ApiReadTimeoutHandler : DelegatingHandler
{
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if(request.Method != HttpMethod.Get)
            return base.SendAsync(request, cancellationToken);

        return SendReadRequestAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendReadRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(Timeout);
        return await base.SendAsync(request, timeoutCancellation.Token);
    }
}
