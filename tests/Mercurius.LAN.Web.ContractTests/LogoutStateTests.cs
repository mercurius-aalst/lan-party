using Mercurius.LAN.Web.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class LogoutStateTests
{
    [Fact]
    public void ProviderCallbackIsFixedAndPurposeIsVersioned()
    {
        Assert.Equal("/account/logout/callback", LogoutState.CallbackPath);
        Assert.Equal("Mercurius.LAN.Web.Account.LogoutState.v1", LogoutState.ProtectorPurpose);
    }

    [Fact]
    public void StateCookieUsesDedicatedScopedSettings()
    {
        var options = LogoutState.CreateCookieOptions(isDevelopment: false, isHttps: false);

        Assert.Equal("mercurius-logout-state", LogoutState.CookieName);
        Assert.Equal("/account/logout", options.Path);
        Assert.Null(options.Domain);
        Assert.True(options.HttpOnly);
        Assert.Equal(SameSiteMode.Lax, options.SameSite);
        Assert.True(options.Secure);
        Assert.True(options.IsEssential);
        Assert.Equal(LogoutState.Lifetime, options.MaxAge);
        Assert.NotNull(options.Expires);
        Assert.InRange(options.Expires.Value - DateTimeOffset.UtcNow, TimeSpan.Zero, LogoutState.Lifetime);
    }

    [Fact]
    public void StateCookieIsSecureForHttpsDevelopmentAndAllNonDevelopmentRequests()
    {
        Assert.False(LogoutState.CreateCookieOptions(isDevelopment: true, isHttps: false).Secure);
        Assert.True(LogoutState.CreateCookieOptions(isDevelopment: true, isHttps: true).Secure);
        Assert.True(LogoutState.CreateCookieOptions(isDevelopment: false, isHttps: false).Secure);
    }

    [Fact]
    public void ValidStatePreservesTargetAndDeletesCookieOnRead()
    {
        using var services = CreateServices();
        const string target = "/tournaments/abc?tab=matches#details";
        var cookie = IssueState(services, target);
        var callback = CreateContext(services, cookie);

        Assert.Equal(target, LogoutState.Consume(callback, GetProvider(services), isDevelopment: false));
        AssertStateCookieDeleted(callback);
    }

    [Fact]
    public void ExpiredStateFallsBackToHomeAndDeletesCookie()
    {
        using var services = CreateServices();
        var cookie = ProtectState(services, "/tournaments/abc", TimeSpan.FromMilliseconds(1));
        Thread.Sleep(50);
        var callback = CreateContext(services, cookie);

        Assert.Equal("/", LogoutState.Consume(callback, GetProvider(services), isDevelopment: false));
        AssertStateCookieDeleted(callback);
    }

    [Fact]
    public void TamperedStateFallsBackToHomeAndDeletesCookie()
    {
        using var services = CreateServices();
        var cookie = IssueState(services, "/tournaments/abc");
        var tamperedCookie = ReplaceMiddleCharacter(cookie);
        var callback = CreateContext(services, tamperedCookie);

        Assert.Equal("/", LogoutState.Consume(callback, GetProvider(services), isDevelopment: false));
        AssertStateCookieDeleted(callback);
    }

    [Fact]
    public void MalformedStateFallsBackToHomeAndDeletesCookie()
    {
        using var services = CreateServices();
        var callback = CreateContext(services, "not-a-protected-state");

        Assert.Equal("/", LogoutState.Consume(callback, GetProvider(services), isDevelopment: false));
        AssertStateCookieDeleted(callback);
    }

    [Fact]
    public void MissingStateFallsBackToHomeAndStillDeletesCookie()
    {
        using var services = CreateServices();
        var callback = CreateContext(services, cookieValue: null);

        Assert.Equal("/", LogoutState.Consume(callback, GetProvider(services), isDevelopment: false));
        AssertStateCookieDeleted(callback);
    }

    [Fact]
    public void OverLimitTargetFallsBackBeforeWritingLargeState()
    {
        using var services = CreateServices();
        var context = CreateContext(services, cookieValue: null);
        var overLimitTarget = "/" + new string('x', LogoutState.MaxReturnUrlLength);
        var provider = GetProvider(services);

        LogoutState.Store(context, provider, overLimitTarget, isDevelopment: false);
        var cookie = ReadIssuedCookie(context);
        var callback = CreateContext(services, cookie);
        var storedTarget = provider.CreateProtector(LogoutState.ProtectorPurpose)
            .ToTimeLimitedDataProtector()
            .Unprotect(cookie);

        Assert.True(cookie.Length <= LogoutState.MaxProtectedStateLength);
        Assert.Equal("/", storedTarget);
        Assert.Equal("/", LogoutState.Consume(callback, provider, isDevelopment: false));
    }

    [Fact]
    public void StateContextsRemainIndependent()
    {
        using var services = CreateServices();
        var firstContext = CreateContext(services, cookieValue: null);
        var secondContext = CreateContext(services, cookieValue: null);
        var provider = GetProvider(services);

        LogoutState.Store(firstContext, provider, "/tournaments/first", isDevelopment: false);
        LogoutState.Store(secondContext, provider, "/tournaments/second", isDevelopment: false);

        var firstCallback = CreateContext(services, ReadIssuedCookie(firstContext));
        var secondCallback = CreateContext(services, ReadIssuedCookie(secondContext));

        Assert.Equal("/tournaments/first", LogoutState.Consume(firstCallback, provider, isDevelopment: false));
        Assert.Equal("/tournaments/second", LogoutState.Consume(secondCallback, provider, isDevelopment: false));
    }

    [Theory]
    [InlineData("/profile")]
    [InlineData("https://evil.example")]
    [InlineData("/foo/../profile")]
    public void RecoveredUnsafeStateIsRevalidated(string returnUrl)
    {
        using var services = CreateServices();
        var cookie = ProtectState(services, returnUrl, LogoutState.Lifetime);
        var callback = CreateContext(services, cookie);

        Assert.Equal("/", LogoutState.Consume(callback, GetProvider(services), isDevelopment: false));
    }

    [Fact]
    public void StateRejectsAProtectionPurposeFromAnotherFeature()
    {
        using var services = CreateServices();
        var provider = GetProvider(services);
        var cookie = provider.CreateProtector("Mercurius.LAN.Web.OtherState.v1")
            .ToTimeLimitedDataProtector()
            .Protect("/tournaments/abc", LogoutState.Lifetime);
        var callback = CreateContext(services, cookie);

        Assert.Equal("/", LogoutState.Consume(callback, provider, isDevelopment: false));
    }

    private static ServiceProvider CreateServices()
    {
        return new ServiceCollection()
            .AddLogging()
            .AddDataProtection()
            .SetApplicationName("Mercurius.LAN.Web.ContractTests")
            .Services
            .BuildServiceProvider();
    }

    private static IDataProtectionProvider GetProvider(ServiceProvider services) =>
        services.GetRequiredService<IDataProtectionProvider>();

    private static string IssueState(ServiceProvider services, string returnUrl)
    {
        var context = CreateContext(services, cookieValue: null);
        LogoutState.Store(context, GetProvider(services), returnUrl, isDevelopment: false);
        return ReadIssuedCookie(context);
    }

    private static string ProtectState(ServiceProvider services, string returnUrl, TimeSpan lifetime)
    {
        return GetProvider(services)
            .CreateProtector(LogoutState.ProtectorPurpose)
            .ToTimeLimitedDataProtector()
            .Protect(returnUrl, lifetime);
    }

    private static DefaultHttpContext CreateContext(ServiceProvider services, string? cookieValue)
    {
        var context = new DefaultHttpContext { RequestServices = services };
        if(cookieValue != null)
            context.Request.Headers.Cookie = $"{LogoutState.CookieName}={cookieValue}";

        return context;
    }

    private static string ReadIssuedCookie(DefaultHttpContext context)
    {
        var header = context.Response.Headers.SetCookie.ToString();
        var value = header.Split(';', 2)[0];
        return value[(value.IndexOf('=') + 1)..];
    }

    private static string ReplaceMiddleCharacter(string value)
    {
        var index = value.Length / 2;
        var replacement = value[index] == 'A' ? 'B' : 'A';
        return value[..index] + replacement + value[(index + 1)..];
    }

    private static void AssertStateCookieDeleted(DefaultHttpContext context)
    {
        var header = context.Response.Headers.SetCookie.ToString();
        Assert.Contains($"{LogoutState.CookieName}=;", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/account/logout", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("expires=Thu, 01 Jan 1970", header, StringComparison.OrdinalIgnoreCase);
    }
}
