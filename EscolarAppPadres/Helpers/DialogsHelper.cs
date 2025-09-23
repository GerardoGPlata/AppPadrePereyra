using Acr.UserDialogs;
using System;
using System.Threading.Tasks;

namespace EscolarAppPadres.Helpers
{
    public class DialogsHelper
    {
        private static string ComposeMessage(string title, string message)
            => string.IsNullOrWhiteSpace(title) ? message : $"{title}\n{message}";

        public static async Task ShowSuccessMessage(string title, string message)
        {
            var text = ComposeMessage(title, message);
            var config = new ToastConfig(text)
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
            var text = ComposeMessage(title, message);
            var config = new ToastConfig(text)
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
            var text = ComposeMessage(title, message);
            var config = new ToastConfig(text)
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

        public static void ShowLoadingMessage(string message = "Cargando...", bool dimBackground = true)
        {
            var mask = dimBackground ? MaskType.Black : MaskType.Clear;
            UserDialogs.Instance.ShowLoading(message, mask);
        }

        public static void HideLoadingMessage()
        {
            UserDialogs.Instance.HideLoading();
        }

        // Info neutral
        public static async Task ShowInfoMessage(string title, string message)
        {
            var text = ComposeMessage(title, message);
            var config = new ToastConfig(text)
            {
                BackgroundColor = System.Drawing.Color.DodgerBlue,
                MessageTextColor = System.Drawing.Color.White,
                Duration = TimeSpan.FromSeconds(4),
                Position = ToastPosition.Bottom,
                Icon = "info_icon.png",
            };

            UserDialogs.Instance.Toast(config);
            await Task.CompletedTask;
        }

        // Toast personalizable rápido
        public static async Task ShowToast(string message, string? hexBg = null, string? hexText = null, int seconds = 3, ToastPosition position = ToastPosition.Bottom, string? icon = null)
        {
            var bg = string.IsNullOrWhiteSpace(hexBg) ? System.Drawing.Color.FromArgb(39, 56, 64) : System.Drawing.ColorTranslator.FromHtml(hexBg);
            var fg = string.IsNullOrWhiteSpace(hexText) ? System.Drawing.Color.White : System.Drawing.ColorTranslator.FromHtml(hexText);

            var config = new ToastConfig(message)
            {
                BackgroundColor = bg,
                MessageTextColor = fg,
                Duration = TimeSpan.FromSeconds(seconds),
                Position = position,
            };
            if (!string.IsNullOrWhiteSpace(icon))
                config.Icon = icon;

            UserDialogs.Instance.Toast(config);
            await Task.CompletedTask;
        }
    }
}
