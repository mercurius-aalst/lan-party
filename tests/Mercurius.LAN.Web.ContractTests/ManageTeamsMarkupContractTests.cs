using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class ManageTeamsMarkupContractTests
{
    [Fact]
    public void TeamManagementPreservesStableFragmentTargets()
    {
        var markup = ReadRepositoryFile("src/Mercurius.LAN.Web/Components/Pages/Teams/ManageTeams.razor");

        Assert.Contains("id=\"received-invites\"", markup);
        Assert.Contains("id=\"team-workspace\"", markup);
        Assert.Contains("id=\"@($\"team-{team.Id:N}\")\"", markup);
        Assert.Contains("class=\"team-workspace brand-section-anchor\"", markup);
        Assert.Contains("class=\"team-detail-header brand-section-anchor\"", markup);
    }

    [Fact]
    public void BrandTypographyLoadsTheFamiliesReferencedByTheStylesheet()
    {
        var stylesheet = ReadRepositoryFile("src/Mercurius.LAN.Web/wwwroot/app.css");

        Assert.Contains(
            "@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Saira:wght@600;700;800;900&display=swap');",
            stylesheet);
        Assert.Contains("font-family: \"Inter\"", stylesheet);
        Assert.Contains("font-family: \"Saira\"", stylesheet);
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
