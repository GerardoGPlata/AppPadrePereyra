using Android.Content;
using Android.Provider;
using Android.Views;
using AndroidNet = Android.Net;
using AndroidApp = Android.App;
using AndroidProvider = Android.Provider;
using EscolarAppPadres.Interface;
using EscolarAppPadres.Platforms.Android;
using EscolarAppPadres.Helpers;

[assembly: Dependency(typeof(AndroidBrightnessService))]
namespace EscolarAppPadres.Platforms.Android
{
    public class AndroidBrightnessService : IBrightnessService
    {
        public async Task<bool> ChangeBrightness(double brightness, string message)
        {
            brightness = Math.Clamp(brightness, 0.0, 1.0);
            var context = AndroidApp.Application.Context;

            try
            {
                if (CanWriteSettings())
                {
                    Settings.System.PutInt(context.ContentResolver, Settings.System.ScreenBrightness, (int)(brightness * 255));

                    var activity = Platform.CurrentActivity;
                    if (activity != null)
                    {
                        var layoutParams = activity.Window!.Attributes;
                        layoutParams!.ScreenBrightness = (float)brightness;
                        activity.Window.Attributes = layoutParams;

                        await DialogsHelper2.ShowSuccessMessage(message);
                        return true;
                    }
                }
                else
                {
                    var result = await DialogsHelper.ShowConfirmationMessage(
                           "Permisos Denegados",
                           "No tienes permisos para modificar el brillo de la pantalla.\n\n" +
                           "¿Quieres ir a la configuración de la aplicación para habilitarlos?");

                    if (result)
                    {
                        RequestWriteSettingsPermission();
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";

                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");

                await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema. Por favor, intenta nuevamente más tarde.");
            }

            return false;
        }

        public double GetCurrentBrightness()
        {
            var context = AndroidApp.Application.Context;
            return Settings.System.GetInt(context.ContentResolver, Settings.System.ScreenBrightness, 255) / 255.0;
        }

        private bool CanWriteSettings()
        {
            return Settings.System.CanWrite(AndroidApp.Application.Context);
        }

        private void RequestWriteSettingsPermission()
        {
            var intent = new Intent(AndroidProvider.Settings.ActionManageWriteSettings);
            intent.SetData(AndroidNet.Uri.Parse("package:" + AndroidApp.Application.Context.PackageName));
            intent.AddFlags(ActivityFlags.NewTask);
            AndroidApp.Application.Context.StartActivity(intent);
        }
    }
}
