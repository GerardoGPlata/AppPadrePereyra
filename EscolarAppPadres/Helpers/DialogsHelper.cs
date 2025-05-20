using Acr.UserDialogs;

namespace EscolarAppPadres.Helpers
{
    public class DialogsHelper
    {
        public static async Task ShowSuccessMessage(string title, string message)
        {
            var config = new ToastConfig(message)
            {
                BackgroundColor = System.Drawing.Color.Green,
                MessageTextColor = System.Drawing.Color.White,
                Duration = TimeSpan.FromSeconds(5),
                Position = ToastPosition.Bottom,
                Icon = "checked_icon.png",
            };

            UserDialogs.Instance.Toast(config);
            await Task.CompletedTask;
        }

        public static async Task ShowWarningMessage(string title, string message)
        {
            var config = new ToastConfig(message)
            {
                BackgroundColor = System.Drawing.Color.Orange,
                MessageTextColor = System.Drawing.Color.White,
                Duration = TimeSpan.FromSeconds(5),
                Position = ToastPosition.Bottom,
                Icon = "warning_icon.png",
            };

            UserDialogs.Instance.Toast(config);
            await Task.CompletedTask;
        }

        public static async Task ShowErrorMessage(string title, string message)
        {
            var config = new ToastConfig(message)
            {
                BackgroundColor = System.Drawing.Color.Red,
                MessageTextColor = System.Drawing.Color.White,
                Duration = TimeSpan.FromSeconds(5),
                Position = ToastPosition.Bottom,
                Icon = "error_icon.png",
            };

            UserDialogs.Instance.Toast(config);
            await Task.CompletedTask;
        }

        public static async Task<bool> ShowConfirmationMessage(string title, string message)
        {
            var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
            {
                Title = title,
                Message = message,
                OkText = "Ir a Configuración",
                CancelText = "Cancelar",
            });

            return result;
        }

        public static void ShowLoadingMessage(string message)
        {
            UserDialogs.Instance.ShowLoading(message);
        }

        public static void HideLoadingMessage()
        {
            UserDialogs.Instance.HideLoading();
        }
    }
}
