using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Threading.Tasks;

namespace EscolarAppPadres.Helpers
{
    public class DialogsHelper2
    {
        public static async Task ShowSuccessMessage(string message)
        {
            var snackbar = Snackbar.Make(message,
                                         duration: TimeSpan.FromSeconds(5),
                                         visualOptions: new SnackbarOptions
                                         {
                                             BackgroundColor = Colors.Green,
                                             TextColor = Colors.White,
                                             CornerRadius = new CornerRadius(10),
                                             Font = Microsoft.Maui.Font.SystemFontOfSize(16, FontWeight.Bold),
                                         });

            await snackbar.Show();
        }

        public static async Task ShowWarningMessage(string message)
        {
            var snackbar = Snackbar.Make(message,
                                         duration: TimeSpan.FromSeconds(5),
                                         visualOptions: new SnackbarOptions
                                         {
                                             BackgroundColor = Colors.Orange,
                                             TextColor = Colors.White,
                                             CornerRadius = new CornerRadius(10),
                                             Font = Microsoft.Maui.Font.SystemFontOfSize(16, FontWeight.Bold),
                                         });

            await snackbar.Show();
        }

        public static async Task ShowErrorMessage(string message)
        {
            var snackbar = Snackbar.Make(message,
                                         duration: TimeSpan.FromSeconds(5),
                                         visualOptions: new SnackbarOptions
                                         {
                                             BackgroundColor = Colors.Red,
                                             TextColor = Colors.White,
                                             CornerRadius = new CornerRadius(10),
                                             Font = Microsoft.Maui.Font.SystemFontOfSize(16, FontWeight.Bold),
                                         });

            await snackbar.Show();
        }
    }
}