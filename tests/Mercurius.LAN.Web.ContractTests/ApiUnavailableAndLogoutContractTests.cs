using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class ApiUnavailableAndLogoutContractTests
{
    [Fact]
    public void LiveClientsUseBoundedTimeoutWithoutAutomaticRetry()
    {
        var code = ReadRepositoryFile("src/Mercurius.LAN.Web/Extensions/DependencyExtensions.cs");

        Assert.Contains("ApiRequestTimeout = TimeSpan.FromSeconds(5)", code);
        Assert.Equal(2, CountOccurrences(code, "client.Timeout = ApiRequestTimeout"));
        Assert.DoesNotContain("AddTransientHttpErrorPolicy", code);
        Assert.DoesNotContain("WaitAndRetryAsync", code);
    }

    private static int CountOccurrences(string value, string term) =>
        value.Split(term, StringSplitOptions.None).Length - 1;

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
