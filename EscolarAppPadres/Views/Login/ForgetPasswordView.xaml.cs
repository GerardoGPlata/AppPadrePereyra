namespace EscolarAppPadres.Views.Login;

public partial class ForgetPasswordView : ContentPage
{
	public ForgetPasswordView()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void OnForgetPassButtonClicked(object sender, EventArgs e)
    {
        var greenColor = (Color)Application.Current!.Resources["StaticButtonColorGreen"];
        ForgetPassButton.BackgroundColor = greenColor;
        ForgetPassButton.TextColor = Colors.White;
        await Task.Delay(200);
        ForgetPassButton.BackgroundColor = Colors.Transparent;
        ForgetPassButton.TextColor = Colors.Black;
        //Application.Current!.MainPage = new AppShell();
    }
}