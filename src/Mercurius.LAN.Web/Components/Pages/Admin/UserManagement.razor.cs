using Mercurius.LAN.Web.Models.Auth;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Components.Shared;
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components.Forms;
using Mercurius.LAN.Web.Services;
using System.ComponentModel.DataAnnotations;
using Mercurius.LAN.Web.DTOs.Users;

namespace Mercurius.LAN.Web.Components.Pages.Admin;

public partial class UserManagement
{
    private List<UserDTO> _users = new();
    private List<UserDTO> _displayUsers = new();
    private UserDTO _selectedUser = new();
    private bool _isCreateMode = true;
    private EditContext? _editContext;
    private CustomAutocomplete<UserDTO> _autoCompleteComponent = null!;
    private RegisterUserDTO _registerModel = new();
    private string _newRole = string.Empty;

    [Inject] private IAuthenticationClient AuthenticationClient { get; set; } = null!;
    [Inject] private IUserClient UserClient { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _users = (await UserClient.GetAllUsersAsync()).ToList();
                _displayUsers = _users;
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception)
            {
                ToastService.ShowError("Users could not be loaded.");
            }
        }
    }

    protected override void OnInitialized() => ReInitEditContext();
    private void ReInitEditContext()
    {
        _editContext = new(_registerModel);
        if(_isCreateMode)
        {
            _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
            _editContext.OnFieldChanged += (sender, args) =>
            {
                _editContext.Validate();
            };
        }
    }

    private void OnUserSelected(UserDTO user)
    {
        _selectedUser = user;
        _isCreateMode = false;
        _registerModel.Username = user.Username;
        _registerModel.Password = string.Empty;
        ReInitEditContext();
    }

    private void ClearForm()
    {
        _selectedUser = new UserDTO();
        _isCreateMode = true;
        _registerModel = new RegisterUserDTO();
        _displayUsers = _users;
        _autoCompleteComponent.ClearSearchField();
        ReInitEditContext();
        StateHasChanged();
    }

    private async Task HandleSubmit()
    {
        try
        {
            if (_isCreateMode)
            {
                await AuthenticationClient.RegisterAsync(new LoginRequest
                {
                    Username = _registerModel.Username,
                    Password = _registerModel.Password
                });
                ToastService.ShowSuccess("User created successfully.");
                _users = (await UserClient.GetAllUsersAsync()).ToList();
                _displayUsers = _users;
                ClearForm();
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
        }
    }

    private async Task AddRole()
    {
        if (!_isCreateMode && !string.IsNullOrWhiteSpace(_newRole))
        {
            try
            {
                await UserClient.AddRoleToUserAsync(_selectedUser.Username, new AddUserRoleRequest
                {
                    Username = _selectedUser.Username,
                    RoleName = _newRole
                });
                ToastService.ShowSuccess($"Role '{_newRole}' added.");
                _users = (await UserClient.GetAllUsersAsync()).ToList();
                _selectedUser = _users.First(u => u.Username == _selectedUser.Username);
                _displayUsers = _users;
                _newRole = string.Empty;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ToastService.ShowError(ex.Message);
            }
        }
    }

    private async Task RemoveRole(string role)
    {
        if (!_isCreateMode && !string.IsNullOrWhiteSpace(role))
        {
            try
            {
                // Assuming API supports removing a role by sending AddUserRoleRequest with role to remove
                // If a specific RemoveRole API exists, use that instead
                await UserClient.DeleteRoleFromUserAsync(_selectedUser.Username, role);
                ToastService.ShowSuccess($"Role '{role}' removed.");
                _users = (await UserClient.GetAllUsersAsync()).ToList();
                _selectedUser = _users.First(u => u.Username == _selectedUser.Username);
                _displayUsers = _users;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ToastService.ShowError(ex.Message);
            }
        }
    }

    private async Task DeleteUser()
    {
        if (!_isCreateMode && _selectedUser != null)
        {
            try
            {
                await UserClient.DeleteUserAsync(_selectedUser.Username);
                ToastService.ShowSuccess("User deleted successfully.");
                _users.RemoveAll(u => u.Username == _selectedUser.Username);
                ClearForm();
            }
            catch (Exception ex)
            {
                ToastService.ShowError(ex.Message);
            }
        }
    }
}
