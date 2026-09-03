using Blazored.Toast.Services;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Refit;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace Mercurius.LAN.Web.Components.Pages.Users;

public partial class CompleteProfile
{
    private static readonly string[] AccountFields = [nameof(CompleteUserProfileRequest.Username)];
    private static readonly string[] AboutFields = [nameof(CompleteUserProfileRequest.Firstname), nameof(CompleteUserProfileRequest.Lastname)];
    private static readonly string[][] StepFields = [AccountFields, AboutFields, []];
    private static readonly string[] StepTitles = ["Account", "About you", "Gaming profiles"];
    private static readonly string[] StepDescriptions = ["Choose your player name", "Add your required details", "Optional gaming IDs"];
    private const int LastStepIndex = 2;

    private readonly CompleteUserProfileRequest _model = new();
    private EditContext? _editContext;
    private ValidationMessageStore? _validationMessageStore;
    private string? _email;
    private string? _usernameAvailabilityMessage;
    private string _usernameAvailabilityClass = "form-text";
    private bool? _usernameIsAvailable;
    private bool _isSaving;
    private bool _isCompleted;
    private int _activeStep;
    private string? _loadError;

    [Inject] private IUserClient UserClient { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Parameter, SupplyParameterFromQuery] public string? ReturnUrl { get; set; }
    [Parameter, SupplyParameterFromQuery] public bool Registration { get; set; }

    private bool IsRegistrationFlow => Registration;

    protected override async Task OnInitializedAsync()
    {
        _loadError = null;
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        PrefillFromClaims(authenticationState.User);

        try
        {
            var currentProfile = await UserClient.GetCurrentUserProfileAsync();
            _email = currentProfile.Email ?? currentProfile.User?.Email ?? _email;
            if(currentProfile.IsComplete)
            {
                if(IsRegistrationFlow) { _isCompleted = true; return; }
                NavigationManager.NavigateTo(GetSafeReturnUrl(ReturnUrl), true);
                return;
            }
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            NavigationManager.NavigateTo(GetLoginHref(), true);
            return;
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.Gone) { NavigationManager.NavigateTo("/account/logout", true); return; }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.NotFound) { }
        catch(ApiException) { SetLoadError(); return; }
        catch(HttpRequestException) { SetLoadError(); return; }
        catch(TaskCanceledException) { SetLoadError(); return; }
        catch(UnauthorizedAccessException)
        {
            NavigationManager.NavigateTo(GetLoginHref(), true);
            return;
        }

        _editContext = new EditContext(_model);
        _validationMessageStore = new ValidationMessageStore(_editContext);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
    }

    private async Task HandleSubmitAsync(EditContext editContext)
    {
        if(_activeStep < LastStepIndex) { await NextStepAsync(); return; }
        await SaveAsync();
    }

    private async Task NextStepAsync()
    {
        if(_isSaving || _activeStep >= LastStepIndex || !ValidateCurrentStep()) return;
        if(_activeStep == 0)
        {
            await CheckUsernameAvailabilityAsync();
            if(_usernameIsAvailable == false) return;
        }
        _activeStep++;
    }

    private void PreviousStep() { if(!_isSaving && _activeStep > 0) _activeStep--; }
    private void CancelOnboarding()
    {
        if(_isSaving) return;
        if(IsRegistrationFlow)
        {
            NavigationManager.NavigateTo("/account/logout", true);
            return;
        }

        NavigationManager.NavigateTo(GetSafeReturnUrl(ReturnUrl));
    }

    private string GetRetryHref()
    {
        var returnUrl = GetSafeReturnUrl(ReturnUrl);
        var returnUrlQuery = $"returnUrl={Uri.EscapeDataString(returnUrl)}";
        return IsRegistrationFlow
            ? $"/complete-profile?registration=true&{returnUrlQuery}"
            : $"/complete-profile?{returnUrlQuery}";
    }

    private string GetRecoveryHref() => IsRegistrationFlow ? "/account/logout" : "/";

    private string GetLoginHref()
    {
        var loginReturnUrl = IsRegistrationFlow ? GetRetryHref() : GetSafeReturnUrl(ReturnUrl);
        return $"/account/login?returnUrl={Uri.EscapeDataString(loginReturnUrl)}";
    }

    private void SetLoadError() => _loadError = "Profile setup is unavailable right now. Please try again.";
    private void ContinueAfterCompletion() => NavigationManager.NavigateTo(GetSafeReturnUrl(ReturnUrl), true);

    private bool ValidateCurrentStep()
    {
        if(_editContext is null || _validationMessageStore is null) return false;
        _validationMessageStore.Clear();
        var isValid = true;
        foreach(var propertyName in StepFields[_activeStep])
        {
            var property = typeof(CompleteUserProfileRequest).GetProperty(propertyName);
            var field = new FieldIdentifier(_model, propertyName);
            if(property is null) continue;
            var results = new List<ValidationResult>();
            var context = new ValidationContext(_model) { MemberName = propertyName };
            if(Validator.TryValidateProperty(property.GetValue(_model), context, results)) continue;
            isValid = false;
            foreach(var result in results) _validationMessageStore.Add(field, result.ErrorMessage ?? "This field is invalid.");
        }
        _editContext.NotifyValidationStateChanged();
        return isValid;
    }

    private async Task SaveAsync()
    {
        if(_isSaving || _editContext is null) return;
        if(!_editContext.Validate()) { _activeStep = GetFirstInvalidStep(); return; }
        _isSaving = true;
        try
        {
            await CheckUsernameAvailabilityAsync();
            if(_usernameIsAvailable == false) { _activeStep = 0; return; }
            await UserClient.CompleteCurrentUserProfileAsync(_model);
            ToastService.ShowSuccess(IsRegistrationFlow ? "Account created. Welcome to Mercurius LAN." : "Profile completed.");
            if(IsRegistrationFlow) _isCompleted = true;
            else NavigationManager.NavigateTo(GetSafeReturnUrl(ReturnUrl), true);
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.BadRequest || exception.StatusCode == HttpStatusCode.Conflict) { ToastService.ShowError(await GetApiErrorAsync(exception)); }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.NotFound) { ToastService.ShowError("Your profile could not be created. Please sign in again and retry."); }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            NavigationManager.NavigateTo(GetLoginHref(), true);
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.Gone) { NavigationManager.NavigateTo("/account/logout", true); }
        catch(UnauthorizedAccessException)
        {
            NavigationManager.NavigateTo(GetLoginHref(), true);
        }
        catch(Exception) { ToastService.ShowError("Profile could not be saved. Please try again."); }
        finally { _isSaving = false; }
    }

    private async Task CheckUsernameAvailabilityAsync()
    {
        _usernameAvailabilityMessage = null;
        _usernameAvailabilityClass = "form-text";
        _usernameIsAvailable = null;
        if(string.IsNullOrWhiteSpace(_model.Username) || !System.Text.RegularExpressions.Regex.IsMatch(_model.Username.Trim(), "^[a-zA-Z0-9]{3,32}$")) return;
        try
        {
            var availability = await UserClient.CheckUsernameAvailabilityAsync(_model.Username);
            _usernameIsAvailable = availability.IsAvailable;
            _usernameAvailabilityMessage = availability.IsAvailable ? "Username is available." : availability.Reason ?? "Username is unavailable.";
            _usernameAvailabilityClass = availability.IsAvailable ? "form-text text-success" : "form-text text-danger";
        }
        catch(Exception) { _usernameAvailabilityMessage = "Username availability will be checked when you save."; _usernameAvailabilityClass = "form-text text-warning"; }
    }

    private int GetFirstInvalidStep()
    {
        foreach(var (fields, stepIndex) in StepFields.Select((fields, index) => (fields, index)))
            if(fields.Any(field => _editContext?.GetValidationMessages(new FieldIdentifier(_model, field)).Any() == true)) return stepIndex;
        return LastStepIndex;
    }

    private string GetStepClass(int stepIndex) => stepIndex < _activeStep ? "complete-profile-step complete-profile-step--complete" : stepIndex == _activeStep ? "complete-profile-step complete-profile-step--active" : "complete-profile-step";

    private void PrefillFromClaims(ClaimsPrincipal user)
    {
        _email = FindClaim(user, ClaimTypes.Email);
        _model.Username = FindClaim(user, "preferred_username", "nickname") ?? _model.Username;
        _model.Firstname = FindClaim(user, "given_name") ?? _model.Firstname;
        _model.Lastname = FindClaim(user, "family_name") ?? _model.Lastname;
        if(string.IsNullOrWhiteSpace(_model.Firstname) && string.IsNullOrWhiteSpace(_model.Lastname))
        {
            var name = FindClaim(user, "name");
            if(!string.IsNullOrWhiteSpace(name))
            {
                var nameParts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                _model.Firstname = nameParts.ElementAtOrDefault(0) ?? _model.Firstname;
                _model.Lastname = nameParts.ElementAtOrDefault(1) ?? _model.Lastname;
            }
        }
    }

    private static async Task<string> GetApiErrorAsync(ApiException exception, string fallback = "Profile could not be saved.")
    {
        try { var content = await exception.GetContentAsAsync<string>(); return string.IsNullOrWhiteSpace(content) ? fallback : content; }
        catch { return string.IsNullOrWhiteSpace(exception.Content) ? fallback : exception.Content.Trim('"'); }
    }

    private static string? FindClaim(ClaimsPrincipal user, params string[] claimTypes)
    {
        foreach(var claimType in claimTypes)
        {
            var claimValue = user.FindFirst(claimType)?.Value;
            if(!string.IsNullOrWhiteSpace(claimValue)) return claimValue;
        }
        return null;
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if(string.IsNullOrWhiteSpace(returnUrl) || !Uri.TryCreate(returnUrl, UriKind.Relative, out _)) return "/";
        if(!returnUrl.StartsWith("/", StringComparison.Ordinal) || returnUrl.StartsWith("//", StringComparison.Ordinal) || returnUrl.StartsWith("/\\", StringComparison.Ordinal)) return "/";
        return returnUrl;
    }
}
