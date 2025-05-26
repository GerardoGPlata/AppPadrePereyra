using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using System.Globalization;
using Syncfusion.Maui.Core.Hosting;
using CommunityToolkit.Maui.Views;

namespace EscolarAppPadres
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzg1NTU5M0AzMjM5MmUzMDJlMzAzYjMyMzkzYldCVjZqREdJVDBWdjZ5TVNTcHNPcUFBMzFNQ3lLMHpHOXZNOFBIdkhRbFk9");

            CultureInfo cultureInfo = new CultureInfo("es-MX");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMopups()
                .UseMauiCommunityToolkit();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
