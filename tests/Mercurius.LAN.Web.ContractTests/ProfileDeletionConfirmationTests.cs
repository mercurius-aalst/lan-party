using System.Reflection;
using System.Net;
using System.Net.Http;
using System.Text;
using Blazored.Toast.Services;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Components.Pages.Users;
using Mercurius.LAN.Web.DTOs.Users;
using Refit;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class ProfileDeletionConfirmationTests
{
    [Theory]
    [InlineData("captainone")]
    [InlineData(" CAPTAINONE ")]
    [InlineData("CaptainOne")]
    public void ConfirmedUsername_AllowsTrimmedCaseInsensitiveMatch(string confirmation)
    {
        var page = new Profile();
        SetField(page, "_originalUsername", "CaptainOne");
        SetField(page, "_deleteConfirmation", confirmation);

        Assert.True(ReadCanDelete(page));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("DifferentPlayer")]
    public void EmptyOrIncorrectConfirmation_KeepsDeletionDisabled(string confirmation)
    {
        var page = new Profile();
        SetField(page, "_originalUsername", "CaptainOne");
        SetField(page, "_deleteConfirmation", confirmation);

        Assert.False(ReadCanDelete(page));
    }

    [Fact]
    public void UnsavedUsernameEdit_DoesNotChangeExpectedConfirmation()
    {
        var page = new Profile();
        SetField(page, "_originalUsername", "CaptainOne");
        SetField(page, "_deleteConfirmation", "UnsavedName");
        ReadModel(page).Username = "UnsavedName";

        Assert.False(ReadCanDelete(page));

        SetField(page, "_deleteConfirmation", " captainone ");

        Assert.True(ReadCanDelete(page));
    }

    [Fact]
    public async Task SuccessfulSave_UsesBackendUsernameForDeletionConfirmation()
    {
        var page = CreateSavePage(out var userClient);
        userClient.UpdatedProfile = new UserProfileDTO { Username = " SavedName " };

        await InvokeSaveAsync(page);

        SetField(page, "_deleteConfirmation", " savedname ");
        Assert.True(ReadCanDelete(page));

        SetField(page, "_deleteConfirmation", "EditedName");
        Assert.False(ReadCanDelete(page));
    }

    [Fact]
    public async Task FailedSave_ContinuesUsingPriorBackendUsernameForDeletionConfirmation()
    {
        var page = CreateSavePage(out var userClient);
        userClient.UpdateException = await CreateApiException(HttpStatusCode.Conflict);

        await InvokeSaveAsync(page);

        SetField(page, "_deleteConfirmation", "editedname");
        Assert.False(ReadCanDelete(page));

        SetField(page, "_deleteConfirmation", " captainone ");
        Assert.True(ReadCanDelete(page));
    }

    private static Profile CreateSavePage(out RecordingUserClientProxy userClient)
    {
        var page = new Profile();
        SetField(page, "_originalUsername", "CaptainOne");
        ReadModel(page).Username = "EditedName";

        var userClientProxy = DispatchProxy.Create<IUserClient, RecordingUserClientProxy>();
        userClient = (RecordingUserClientProxy)(object)userClientProxy;
        SetPrivateProperty(page, "UserClient", userClientProxy);

        var toastProxy = DispatchProxy.Create<IToastService, RecordingToastServiceProxy>();
        SetPrivateProperty(page, "ToastService", toastProxy);

        return page;
    }

    private static async Task InvokeSaveAsync(Profile page)
    {
        var method = typeof(Profile).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Profile save method was not found.");

        await (Task)(method.Invoke(page, null) ?? throw new InvalidOperationException("Profile save did not return a task."));
    }

    private static void SetPrivateProperty(object instance, string name, object? value)
    {
        var property = typeof(Profile).GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Profile property '{name}' was not found.");
        property.SetValue(instance, value);
    }

    private static UpdateUserProfileRequest ReadModel(Profile page) =>
        (UpdateUserProfileRequest)(GetField("_model").GetValue(page)
            ?? throw new InvalidOperationException("Profile model was not initialized."));

    private static bool ReadCanDelete(Profile page) =>
        (bool)(typeof(Profile).GetProperty("_canDelete", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(page)
            ?? throw new InvalidOperationException("Deletion confirmation state was not found."));

    private static void SetField(Profile page, string name, object value) =>
        GetField(name).SetValue(page, value);

    private static FieldInfo GetField(string name) =>
        typeof(Profile).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Profile field '{name}' was not found.");

    private static async Task<ApiException> CreateApiException(HttpStatusCode statusCode)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, "https://example.test/users/me");
        using var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent("{\"message\":\"Profile could not be saved.\"}", Encoding.UTF8, "application/json")
        };

        return await ApiException.Create(request, HttpMethod.Patch, response, new RefitSettings(), innerException: null);
    }

    public class RecordingUserClientProxy : DispatchProxy
    {
        public UserProfileDTO? UpdatedProfile { get; set; }
        public ApiException? UpdateException { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            return targetMethod?.Name switch
            {
                nameof(IUserClient.CheckUsernameAvailabilityAsync) =>
                    Task.FromResult(new UsernameAvailabilityResponse { IsAvailable = true }),
                nameof(IUserClient.UpdateCurrentUserProfileAsync) when UpdateException is not null =>
                    throw UpdateException,
                nameof(IUserClient.UpdateCurrentUserProfileAsync) =>
                    Task.FromResult(UpdatedProfile ?? new UserProfileDTO { Username = "EditedName" }),
                _ => throw new NotSupportedException($"Unexpected user client call: {targetMethod?.Name}")
            };
        }
    }

    public class RecordingToastServiceProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => null;
    }
}
