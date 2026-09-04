using Mercurius.LAN.Web.Extensions;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class LocalReturnUrlHelperTests
{
    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("//evil.example")]
    [InlineData("/\\evil.example")]
    [InlineData("/\n//evil.example")]
    [InlineData("/\t\\evil.example")]
    public void UnsafeReturnUrlsFallBackToHome(string returnUrl)
    {
        Assert.Equal("/", LocalReturnUrlHelper.GetSafeLocalReturnUrl(returnUrl));
    }

    [Fact]
    public void LocalReturnUrlIsPreserved()
    {
        Assert.Equal("/tournaments/abc?tab=matches#details",
            LocalReturnUrlHelper.GetSafeLocalReturnUrl("/tournaments/abc?tab=matches#details"));
    }
}
