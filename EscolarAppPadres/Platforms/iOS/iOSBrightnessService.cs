using EscolarAppPadres.Platforms.iOS;
using EscolarAppPadres.Interface;
using UIKit;
using EscolarAppPadres.Helpers;

[assembly: Dependency(typeof(iOSBrightnessService))]
namespace EscolarAppPadres.Platforms.iOS
{
    public class iOSBrightnessService : IBrightnessService
    {
        public async Task<bool> ChangeBrightness(double brightness, string message)
        {
            brightness = Math.Clamp(brightness, 0.0, 1.0);

            try
            {
                UIScreen.MainScreen.Brightness = (float)brightness;
                await DialogsHelper2.ShowSuccessMessage(message);
                return true;
            }
            catch(Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";

                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");

                await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema. Por favor, intenta nuevamente más tarde.");
                return false;
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        public double GetCurrentBrightness()
        {
            return UIScreen.MainScreen.Brightness;
        }
    }
}
