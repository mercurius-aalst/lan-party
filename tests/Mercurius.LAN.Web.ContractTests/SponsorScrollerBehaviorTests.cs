using System.Reflection;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.Models.Sponsors;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class SponsorScrollerBehaviorTests
{
    [Fact]
    public void SponsorCountUsesDistinctPersistedSponsorIds()
    {
        var component = CreateComponent(
        [
            new Sponsor { Id = 1, Name = "One" },
            new Sponsor { Id = 1, Name = "One duplicate" },
            new Sponsor { Id = 2, Name = "Two" },
            new Sponsor { Id = 3, Name = "Three" }
        ]);

        var countClass = typeof(SponsorScroller)
            .GetProperty("SponsorCountClass", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(component);

        Assert.Equal("sponsor-scroller--count-3", countClass);
    }

    [Fact]
    public void FourDistinctSponsorsRemainInMarqueeCount()
    {
        var component = CreateComponent(
        [
            new Sponsor { Id = 1, Name = "One" },
            new Sponsor { Id = 2, Name = "Two" },
            new Sponsor { Id = 3, Name = "Three" },
            new Sponsor { Id = 4, Name = "Four" }
        ]);

        var countClass = typeof(SponsorScroller)
            .GetProperty("SponsorCountClass", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(component);

        Assert.Equal("sponsor-scroller--count-4", countClass);
    }

    private static SponsorScroller CreateComponent(IReadOnlyList<Sponsor> sponsors)
    {
        var component = new SponsorScroller();
        typeof(SponsorScroller)
            .GetProperty(nameof(SponsorScroller.Sponsors))!
            .SetValue(component, sponsors);
        return component;
    }
}
