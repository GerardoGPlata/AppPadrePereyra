using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mopups.Pages;
using Mopups.Services;

namespace EscolarAppPadres.Services
{
    public class PopupService
    {
        private static readonly Lazy<PopupService> _instance = new Lazy<PopupService>(() => new PopupService());
        public static PopupService Instance => _instance.Value;

        private readonly Stack<PopupPage> _openPopups = new Stack<PopupPage>();

        public async Task ShowPopupAsync(PopupPage popup)
        {
            _openPopups.Push(popup);
            await MopupService.Instance.PushAsync(popup);
        }

        public async Task ClosePopupAsync()
        {
            if (_openPopups.Count > 0)
            {
                var popup = _openPopups.Pop();
                await MopupService.Instance.PopAsync();
            }
        }

        public async Task CloseAllPopupsAsync()
        {
            while (_openPopups.Count > 0)
            {
                var popup = _openPopups.Pop();
                await MopupService.Instance.PopAsync();
            }
        }
    }
}
