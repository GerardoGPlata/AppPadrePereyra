using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using EscolarAppPadres.Interface;
using EscolarAppPadres.Platforms.Android;

namespace EscolarAppPadres
{
    [Activity(Theme = "@style/Maui.SplashTheme", 
        MainLauncher = true, 
        LaunchMode = LaunchMode.SingleTop, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            UserDialogs.Init(this);
            DependencyService.RegisterSingleton<ICalendarService>(new AndroidCalendarService());
            DependencyService.RegisterSingleton<IBrightnessService>(new AndroidBrightnessService());

            if (Window != null)
            {
                // (versión 6 de android y superior)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    // Cambiar el color de la barra de estado (parte superior)
                    Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#FFFFFF"));

                    // Cambiar el color de los íconos y el texto de la barra de estado a oscuro
                    Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.LightStatusBar);
                }

                // (versión 8 de android y superior)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    // Cambiar el color de la barra de navegación (parte inferior)
                    Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#FFFFFF"));

                    // Cambiar el color de los íconos y el texto de la barra de navegación a oscuro
                    Window.DecorView.SystemUiVisibility |= (StatusBarVisibility)SystemUiFlags.LightNavigationBar;
                }
                else
                {
                    // Cambiar el color de la barra de navegación (parte inferior)
                    Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#000000"));
                }
            }
        }
    }
}
