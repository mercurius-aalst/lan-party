using Mercurius.LAN.Web.Middleware;
using System.Net;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class ApiUnavailableAndLogoutContractTests
{
    [Fact]
    public void LiveClientsBoundReadRequestsWithoutApplyingShortTimeoutToMutations()
    {
        var code = ReadRepositoryFile("src/Mercurius.LAN.Web/Extensions/DependencyExtensions.cs");

        Assert.Equal(TimeSpan.FromSeconds(5), ApiReadTimeoutHandler.Timeout);
        Assert.Equal(2, CountOccurrences(code, ".AddHttpMessageHandler<ApiReadTimeoutHandler>()"));
        Assert.DoesNotContain("client.Timeout = ApiRequestTimeout", code);
        Assert.DoesNotContain("AddTransientHttpErrorPolicy", code);
        Assert.DoesNotContain("WaitAndRetryAsync", code);
    }

    [Fact]
    public async Task ReadTimeoutHandler_CancelsGetsButLeavesMutationsUnboundedByReadDeadline()
    {
        var readHandler = new ApiReadTimeoutHandler
        {
            InnerHandler = new BlockingHandler()
        };
        using var readRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.example.test/read");
        using var readInvoker = new HttpMessageInvoker(readHandler);

        var readTask = readInvoker.SendAsync(readRequest, CancellationToken.None);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => readTask);

        var mutationHandler = new ApiReadTimeoutHandler
        {
            InnerHandler = new RecordingHandler()
        };
        using var mutationRequest = new HttpRequestMessage(HttpMethod.Put, "https://api.example.test/mutation");
        using var mutationInvoker = new HttpMessageInvoker(mutationHandler);

        using var response = await mutationInvoker.SendAsync(mutationRequest, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(((RecordingHandler)mutationHandler.InnerHandler!).ReceivedCancellation);
    }

    private static int CountOccurrences(string value, string term) =>
        value.Split(term, StringSplitOptions.None).Length - 1;

    private sealed class BlockingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => WaitForCancellationAsync(cancellationToken);

        private static async Task<HttpResponseMessage> WaitForCancellationAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public bool ReceivedCancellation { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            ReceivedCancellation = cancellationToken.IsCancellationRequested;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while(directory != null)
        {
            var repositoryPath = Path.Combine(directory.FullName, relativePath);
            if(File.Exists(repositoryPath))
                return File.ReadAllText(repositoryPath);

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Could not locate repository file '{relativePath}'.");
    }
}
