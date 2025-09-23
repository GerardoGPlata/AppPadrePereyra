using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using EscolarAppPadres.Views.Login;
using EscolarAppPadres.Interface;
using System.Text.Json;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using EscolarAppPadres.Views.Account;

namespace EscolarAppPadres.ViewModels.Login
{
    public class LogoutViewModel : INotifyPropertyChanged
    {
        //private readonly StudentLoginService _studentLoginService;
        private readonly FatherLoginService _fatherLoginService;
        private readonly EncryptionService _encryptionService;

        private bool _isNavigating;
        private bool _isLoggingIn;

        private string _nombreCompleto;
        private string _matricula;
        private string _correo;

        private Color _logoutButtonBackgroundColor = Colors.Transparent;
        private Color _logoutButtonTextColor = Colors.Black;

        private string _imageButtonSource = "sunny_outline.svg";
        private double _brightness = 1.0;
        private string _brightnessMessage;

        public ObservableCollection<HijoConColor> Hijos { get; set; } = new ObservableCollection<HijoConColor>();

        public LogoutViewModel()
        {
            _encryptionService = new EncryptionService();
            _fatherLoginService = new FatherLoginService();
            LoadUserData();
            LoadChildrenData();
        }
        public string NombreCompleto
        {
            get => _nombreCompleto;
            set
            {
                _nombreCompleto = value;
                OnPropertyChanged(nameof(NombreCompleto));
            }
        }

        public string Correo
        {
            get => _correo;
            set
            {
                _correo = value;
                OnPropertyChanged(nameof(Correo));
            }
        }

        public bool IsNavigating
        {
            get => _isNavigating;
            set
            {
                _isNavigating = value;
                OnPropertyChanged(nameof(IsNavigating));
            }
        }

        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                _isLoggingIn = value;
                OnPropertyChanged(nameof(IsLoggingIn));
            }
        }

        public Color LogoutButtonBackgroundColor
        {
            get => _logoutButtonBackgroundColor;
            set
            {
                _logoutButtonBackgroundColor = value;
                OnPropertyChanged(nameof(LogoutButtonBackgroundColor));
            }
        }

        public Color LogoutButtonTextColor
        {
            get => _logoutButtonTextColor;
            set
            {
                _logoutButtonTextColor = value;
                OnPropertyChanged(nameof(LogoutButtonTextColor));
            }
        }

        public string ImageButtonSource
        {
            get => _imageButtonSource;
            set
            {
                _imageButtonSource = value;
                OnPropertyChanged(nameof(ImageButtonSource));
            }
        }

        public string BrightnessMessage
        {
            get => _brightnessMessage;
            set
            {
                _brightnessMessage = value;
                OnPropertyChanged(nameof(BrightnessMessage));
            }
        }

        public ICommand LogoutCommand => new Command(async () => await LogoutAsync());
        public ICommand PrivacyPolicyCommand => new Command(async () => await NavigateToPrivacyPolicyAsync());
        public ICommand ChangePasswordCommand => new Command(async () => await NavigateToChangePasswordAsync());
        public ICommand GlowCommand => new Command(async () => await GlowAsync());
        public ICommand SeleccionarColorCommand => new Command<HijoConColor>(async (hijo) => await SeleccionarColorAsync(hijo));

        private async Task SeleccionarColorAsync(HijoConColor hijo)
        {
            var popup = new ColorPickerPopup();
            popup.SelectedColor = hijo.ColorHex;
            // Actualiza en vivo el color del hijo en cada tap (sin persistir aún)
            EventHandler<string> handler = (_, hex) =>
            {
                if (string.IsNullOrEmpty(hex)) return;
                hijo.ColorHex = hex;
            };
            popup.ColorChanged += handler;
            var selectedColorTask = popup.ResultSource.Task;
            popup.Show();

            var nuevoColor = await selectedColorTask;
            // Limpia suscripción
            popup.ColorChanged -= handler;
            if (string.IsNullOrEmpty(nuevoColor)) return;

            hijo.ColorHex = nuevoColor;

            // Actualiza y guarda
            var json = JsonSerializer.Serialize(Hijos.ToList());
            await SecureStorage.SetAsync("Hijos", json);

            LoadChildrenData(); // Refresca la lista
        }



        private async void LoadUserData()
        {
            NombreCompleto = await SecureStorage.GetAsync("NombreCompleto") ?? "Nombre no disponible";
            //Matricula = await SecureStorage.GetAsync("Matricula") ?? "Matrícula no disponible";
            Correo = await SecureStorage.GetAsync("Correo") ?? "Correo no disponible";
        }

        public async void LoadChildrenData()
        {
            try
            {
                var childrenJson = await SecureStorage.GetAsync("Hijos");

                if (!string.IsNullOrEmpty(childrenJson))
                {
                    var hijos = JsonSerializer.Deserialize<List<HijoConColor>>(childrenJson);

                    if (hijos != null)
                    {
                        Hijos.Clear();
                        foreach (var hijo in hijos)
                        {
                            Hijos.Add(hijo);
                            Console.WriteLine($"AlumnoID: {hijo.AlumnoId}, NombreCompleto: {hijo.NombreCompleto}, Matricula: {hijo.Matricula}, Color: {hijo.ColorHex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando hijos: {ex.Message}");
            }
        }

        private void UpdateLogoutButtonAppearance()
        {
            var greenColor = (Color)Application.Current!.Resources["StaticButtonColorGreen"];
            LogoutButtonBackgroundColor = greenColor;
            LogoutButtonTextColor = Colors.White;
        }
        private void ResetLogoutButtonAppearance()
        {
            LogoutButtonBackgroundColor = Colors.Transparent;
            LogoutButtonTextColor = Colors.Black;
        }

        public async Task LogoutAsync()
        {
            if (_isNavigating)
                return;

            _isNavigating = true;
            UpdateLogoutButtonAppearance();
            await Task.Delay(200);

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var usuarioIdString = await SecureStorage.GetAsync("Usuario_Id");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(usuarioIdString))
                {
                    await DialogsHelper2.ShowErrorMessage("No se encontró la información de usuario o token.");
                    _isNavigating = false;
                    return;
                }

                if (!int.TryParse(usuarioIdString, out var usuarioId))
                {
                    await DialogsHelper2.ShowErrorMessage("Usuario ID inválido.");
                    _isNavigating = false;
                    return;
                }

                DialogsHelper.ShowLoadingMessage("Cerrando sesión, por favor espere...");

                var logoutRequest = new Logout
                {
                    UsuarioId = usuarioId,
                    WebToken = token,
                };

                var logoutApiResponse = await _fatherLoginService.LogoutAsync(logoutRequest, token);

                if (logoutApiResponse != null && !logoutApiResponse.IsClientError)
                {
                    if (logoutApiResponse.Result && logoutApiResponse.Valoration)
                    {
                        SecureStorage.RemoveAll();
                        Preferences.Set("cache_flag", false);

                        await DialogsHelper2.ShowSuccessMessage(logoutApiResponse.Message);
                        Application.Current!.MainPage = new NavigationPage(new StudentLoginView());
                    }
                    else if (logoutApiResponse.Result && !logoutApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(logoutApiResponse.Message);
                    }
                    else if (!logoutApiResponse.Result && !logoutApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(logoutApiResponse.Log!);
                    }
                }
                else
                {
                    await DialogsHelper2.ShowErrorMessage(logoutApiResponse?.Message ?? "Error desconocido");
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
                await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema al cerrar sesión. Por favor, intenta nuevamente más tarde.");
            }
            finally
            {
                DialogsHelper.HideLoadingMessage();
                ResetLogoutButtonAppearance();
                _isNavigating = false;
            }
        }


        private async Task GlowAsync()
        {
            var BrightnessService = DependencyService.Get<IBrightnessService>();
            _brightness = BrightnessService.GetCurrentBrightness();

            if (_brightness >= 0.0901960784313725)
            {
                _brightness = 0.0862745098039216;
                BrightnessMessage = "Brillo ajustado al 50%";
            }
            else if (_brightness < 0.0862745098039216)
            {
                _brightness = 1.0;
                BrightnessMessage = "Brillo ajustado al 100%";
            }

            var success = await BrightnessService.ChangeBrightness(_brightness, BrightnessMessage);

            if (success)
            {
                ImageButtonSource = _brightness == 1.0 ? "sunny_outline.svg" : "partly_sunny_outline.svg";
            }
        }

        public async Task NavigateToPrivacyPolicyAsync()
        {
            if (IsNavigating) return;

            IsNavigating = true;

            await Shell.Current.GoToAsync("PrivacyView");
            IsNavigating = false;
        }

        public async Task NavigateToChangePasswordAsync()
        {
            if (IsNavigating) return;

            IsNavigating = true;

            await Shell.Current.GoToAsync("ChangePasswordView");
            IsNavigating = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
