using EscolarAppPadres.ViewModels.Login;

namespace EscolarAppPadres.Views.Account;

public partial class ChangePasswordView : ContentPage
{
    private bool _isCurrentPasswordVisible = false;
    private bool _isNewPasswordVisible = false;
    private bool _isConfirmPasswordVisible = false;
    private readonly ChangePasswordViewModel _changePasswordViewModel;

    public ChangePasswordView()
	{
		InitializeComponent();
        _changePasswordViewModel = new ChangePasswordViewModel();
        BindingContext = _changePasswordViewModel;
    }

    private void OnToggleCurrentPasswordVisibilityClicked(object sender, EventArgs e)
    {
        _isCurrentPasswordVisible = !_isCurrentPasswordVisible;
        CurrentPasswordEntry.IsPassword = !_isCurrentPasswordVisible;
        ToggleCurrentPasswordVisibilityButton.Source = _isCurrentPasswordVisible
            ? "eye_outline.svg"
            : "eye_off_outline.svg";
    }

    private void OnToggleNewPasswordVisibilityClicked(object sender, EventArgs e)
    {
        _isNewPasswordVisible = !_isNewPasswordVisible;
        NewPasswordEntry.IsPassword = !_isNewPasswordVisible;
        ToggleNewPasswordVisibilityButton.Source = _isNewPasswordVisible
            ? "eye_outline.svg"
            : "eye_off_outline.svg";
    }

    private void OnToggleConfirmPasswordVisibilityClicked(object sender, EventArgs e)
    {
        _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;
        ToggleConfirmPasswordVisibilityButton.Source = _isConfirmPasswordVisible
            ? "eye_outline.svg"
            : "eye_off_outline.svg";
    }
}