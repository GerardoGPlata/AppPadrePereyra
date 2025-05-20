namespace EscolarAppPadres;

public partial class LoadingPage : ContentPage
{
	public LoadingPage()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartImageSlideAnimation();
        StartRotationAnimation();
        StartTextBlinkAnimation();
    }

    private async void StartRotationAnimation()
    {
        while (true)
        {
            await LoadingIcon.RotateTo(360, 1500);
            LoadingIcon.Rotation = 0;
        }
    }

    private async void StartTextBlinkAnimation()
    {
        while (true)
        {
            await LoadingText.FadeTo(0, 1500);
            await LoadingText.FadeTo(1, 1500);
        }
    }

    private async void StartImageSlideAnimation()
    {
        LoadingLogo.TranslationY = -300;
        LoadingLogo.Opacity = 0;

        await Task.Delay(1500);

        await Task.WhenAll(
            LoadingLogo.TranslateTo(0, 0, 2000, Easing.BounceOut),
            LoadingLogo.FadeTo(1, 2000)
        );
    }
}