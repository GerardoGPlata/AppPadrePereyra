using EscolarAppPadres.ViewModels.Login;

namespace EscolarAppPadres.Views.Login;

public partial class StudentLoginView : ContentPage
{
    private readonly LoginViewModel _loginViewModel;
    private bool _isPasswordVisible = false;

    public StudentLoginView()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _loginViewModel = new LoginViewModel();
        BindingContext = _loginViewModel;
    }

    private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;

        PasswordEntry.IsPassword = !_isPasswordVisible;

        TogglePasswordVisibilityButton.Source = _isPasswordVisible
            ? "eye_outline.svg"
            : "eye_off_outline.svg";
    }
}