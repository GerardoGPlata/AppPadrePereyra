using EscolarAppPadres.Handlers;
using EscolarAppPadres.Views.Login;
using EscolarAppPadres.ViewModels.Login;
using EscolarAppPadres.Services;
using EscolarAppPadres.Helpers;
using System.Globalization;

namespace EscolarAppPadres
{
    public partial class App : Application
    {
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isConnectionLost = false;
        private bool _successMessageShown = false;

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzg1NTU5M0AzMjM5MmUzMDJlMzAzYjMyMzkzYldCVjZqREdJVDBWdjZ5TVNTcHNPcUFBMzFNQ3lLMHpHOXZNOFBIdkhRbFk9");

            InitializeComponent();

            UserAppTheme = AppTheme.Light;

            ControlStylingMapper.ApplyAllControlStyles();

            var isCacheAvailable = CheckCacheAvailability();

            if (!isCacheAvailable)
            {
                MainPage = new NavigationPage(new StudentLoginView());
            }
            else
            {
                MainPage = new NavigationPage(new LoadingPage());
            }
        }

        protected override async void OnStart()
        {
            StartInternetCheckTimer();

            await EnsurePermissions();

            var isCacheAvailable = CheckCacheAvailability();

            if (!isCacheAvailable)
            {
                SecureStorage.RemoveAll();
                Preferences.Clear();

                MainPage = new NavigationPage(new StudentLoginView());
            }
            else
            {
                await Task.Delay(5000);

                // ✅ CAMBIO: Validación simplificada - solo verificar si hay datos guardados
                var UsuarioId = await SecureStorage.GetAsync("Usuario_Id");
                var TipoUsuarioId = await SecureStorage.GetAsync("Tipo_Usuario_Id");
                var authToken = await SecureStorage.GetAsync("auth_token");

                if (!string.IsNullOrEmpty(UsuarioId) &&
                    !string.IsNullOrEmpty(TipoUsuarioId) &&
                    !string.IsNullOrEmpty(authToken))
                {
                    // ✅ Si hay datos, ir directo al dashboard sin validaciones
                    MainPage = new AppShell();
                    // ✅ ELIMINADO: StartTokenValidationTask() - Ya no necesitamos validación periódica
                }
                else
                {
                    MainPage = new NavigationPage(new StudentLoginView());
                }
            }
        }

        private async Task CloseOpenPopupsAsync()
        {
            if (PopupManager.IsPopupOpen)
            {
                try
                {
                    await PopupService.Instance.CloseAllPopupsAsync();
                    PopupManager.IsPopupOpen = false;
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                    var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";

                    Console.WriteLine("=== ERROR DETECTADO ===");
                    Console.WriteLine(errorMessage);
                    Console.WriteLine(errorStackTrace);
                    Console.WriteLine("=======================");

                    //await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema. Por favor, intenta nuevamente más tarde.");
                }
            }
        }

        private async Task<bool> EnsurePermissions()
        {
            try
            {
                var permissionsToRequest = new List<(List<Permissions.BasePermission> Permissions, string FriendlyName)>
                {
                    (new List<Permissions.BasePermission>
                    {
                        new Permissions.CalendarRead(),
                        new Permissions.CalendarWrite()
                    },  "Calendario"),
                };

                var deniedPermissions = new List<string>();

                foreach (var (permissions, friendlyName) in permissionsToRequest)
                {
                    bool groupGranted = true;

                    foreach (var permission in permissions)
                    {
                        var status = await permission.CheckStatusAsync();

                        if (status != PermissionStatus.Granted)
                        {
                            status = await permission.RequestAsync();

                            if (status != PermissionStatus.Granted)
                            {
                                groupGranted = false;
                                break;
                            }
                        }
                    }

                    if (!groupGranted)
                    {
                        deniedPermissions.Add(friendlyName);
                    }
                }

                if (deniedPermissions.Any())
                {
                    var deniedMessage = $"Los siguientes permisos no fueron concedidos: {string.Join(", ", deniedPermissions)}.\n\n" +
                                        "Algunas funciones pueden no funcionar correctamente.\n\n" +
                                        "¿Quieres ir a la configuración de la aplicación para habilitarlos?";

                    var respuesta = await DialogsHelper.ShowConfirmationMessage("Permisos Denegados", deniedMessage);

                    if (respuesta)
                    {
                        AppInfo.ShowSettingsUI();
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud. Mensaje: {ex.Message}";
                var errorStackTrace = $"Pila de errores: {ex.StackTrace}";

                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");

                //await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema. Por favor, intenta nuevamente más tarde.");
                return false;
            }
        }

        private bool IsInternetAvailable()
        {
            var current = Connectivity.NetworkAccess;
            return current == NetworkAccess.Internet;
        }

        private bool CheckCacheAvailability()
        {
            try
            {
                return Preferences.ContainsKey("cache_flag") && Preferences.Get("cache_flag", false);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";

                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                return false;
            }
        }

        // ✅ ELIMINADO: StartTokenValidationTask() - Ya no necesitamos validación periódica

        private async void StartInternetCheckTimer()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var internetCheckTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await CheckInternetStatusPeriodically();
                    await Task.Delay(TimeSpan.FromSeconds(10), _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);

            await Task.CompletedTask;
        }

        private async Task CheckInternetStatusPeriodically()
        {
            if (IsInternetAvailable())
            {
                if (_isConnectionLost)
                {
                    _isConnectionLost = false;
                    _successMessageShown = false;
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        UnblockUI();
                        if (!_successMessageShown)
                        {
                            await DialogsHelper2.ShowSuccessMessage("Conexión restablecida.");
                            _successMessageShown = true;
                        }
                    });
                }
            }
            else
            {
                if (!_isConnectionLost)
                {
                    _isConnectionLost = true;
                    await MainThread.InvokeOnMainThreadAsync(() => BlockUI());
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DialogsHelper2.ShowErrorMessage("No hay conexión a Internet.");
                    });
                }
            }
        }

        private void BlockUI()
        {
            DialogsHelper.ShowLoadingMessage("Conexión perdida...");
        }

        private void UnblockUI()
        {
            DialogsHelper.HideLoadingMessage();
        }

        protected override void OnSleep()
        {
            _cancellationTokenSource?.Cancel();
        }

        protected override void OnResume()
        {
            // ✅ CAMBIO: Solo verificar conexión a internet, no tokens
            StartInternetCheckTimer();
        }
    }
}
