using Acr.UserDialogs;
using Foundation;
using UIKit;
using EscolarAppPadres.Interface;
using EscolarAppPadres.Platforms.iOS;
using EscolarAppPadres.Services;

namespace EscolarAppPadres
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            UserDialogs.Init();
            DependencyService.RegisterSingleton<ICalendarService>(new IOSCalendarService());
            DependencyService.RegisterSingleton<IBrightnessService>(new iOSBrightnessService());

            // (versión 13 de iso y superior)
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                // Cambiar el color de la barra de estado (parte superior)
                var window = UIApplication.SharedApplication.KeyWindow;
                var statusBarFrame = window?.WindowScene?.StatusBarManager?.StatusBarFrame;

                if (statusBarFrame.HasValue)
                {
                    var statusBarView = new UIView(statusBarFrame.Value)
                    {
                        BackgroundColor = UIColor.White
                    };
                    window?.AddSubview(statusBarView);
                }

                // Cambiar el color de los íconos y el texto de la barra de estado a oscuro
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.DarkContent;
            }
            // (versión 12 de iso y inferior)
            else
            {
                // Cambiar el color de fondo de la barra de estado
                var statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
                if (statusBar != null && statusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
                {
                    statusBar.BackgroundColor = UIColor.White;
                }

                // Cambiar el color de los íconos y el texto de la barra de estado a oscuro
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.BlackOpaque;
            }

            // (versión 11 de iso y superior)
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                var toolbarBackgroundColor = UIColor.FromRGB(0x00, 0x83, 0x47);
                var toolbarForegroundColor = UIColor.White;

#pragma warning disable CA1416
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    var navigationBarAppearance = new UINavigationBarAppearance();
                    navigationBarAppearance.ConfigureWithOpaqueBackground();
                    navigationBarAppearance.BackgroundColor = toolbarBackgroundColor;
                    navigationBarAppearance.TitleTextAttributes = new UIStringAttributes
                    {
                        ForegroundColor = toolbarForegroundColor
                    };
                    navigationBarAppearance.LargeTitleTextAttributes = new UIStringAttributes
                    {
                        ForegroundColor = toolbarForegroundColor
                    };

                    UINavigationBar.Appearance.StandardAppearance = navigationBarAppearance;
                    UINavigationBar.Appearance.CompactAppearance = navigationBarAppearance;
                    UINavigationBar.Appearance.ScrollEdgeAppearance = navigationBarAppearance;
                }
                else
                {
                    UINavigationBar.Appearance.BarTintColor = toolbarBackgroundColor;
                    UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
                    {
                        ForegroundColor = toolbarForegroundColor
                    };
                    UINavigationBar.Appearance.LargeTitleTextAttributes = new UIStringAttributes
                    {
                        ForegroundColor = toolbarForegroundColor
                    };
                }

                UINavigationBar.Appearance.TintColor = toolbarForegroundColor;
                UIBarButtonItem.Appearance.TintColor = toolbarForegroundColor;
                UINavigationBar.Appearance.PrefersLargeTitles = false;
#pragma warning restore CA1416
            }

            return base.FinishedLaunching(application, launchOptions);
        }
    }
}
