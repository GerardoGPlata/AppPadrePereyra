using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using EscolarAppPadres.Helpers;
using EscolarAppPadres.Views.Login;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace EscolarAppPadres.ViewModels.Login
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        //private readonly StudentLoginService _studentLoginService;
        private readonly FatherLoginService _fatherLoginService;
        private readonly EncryptionService _encryptionService;

        string[] ColoresDisponibles = new[]
        {
            "#FF5733", // Rojo naranja
            "#33B5FF", // Azul celeste
            "#FF33A8", // Rosa fuerte
            "#33FF57", // Verde limón
            "#FFC300", // Amarillo brillante
            "#8E44AD", // Púrpura
            "#1ABC9C", // Turquesa
            "#E67E22", // Naranja
            "#2ECC71", // Verde esmeralda
            "#3498DB", // Azul claro
            "#F39C12", // Dorado
            "#C0392B"  // Rojo oscuro
        };


        private string _matricula;
        private string _email;
        private string _password;

        private bool _isNavigating;
        private bool _isLoggingIn;

        private Color _loginButtonBackgroundColor = Colors.Transparent;
        private Color _loginButtonTextColor = Colors.Black;

        public LoginViewModel()
        {
            _fatherLoginService = new FatherLoginService();
            _encryptionService = new EncryptionService();
        }

        public string Matricula
        {
            get => _matricula;
            set
            {
                _matricula = value;
                OnPropertyChanged(nameof(Matricula));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
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

        public Color LoginButtonBackgroundColor
        {
            get => _loginButtonBackgroundColor;
            set
            {
                _loginButtonBackgroundColor = value;
                OnPropertyChanged(nameof(LoginButtonBackgroundColor));
            }
        }

        public Color LoginButtonTextColor
        {
            get => _loginButtonTextColor;
            set
            {
                _loginButtonTextColor = value;
                OnPropertyChanged(nameof(LoginButtonTextColor));
            }
        }

        public ICommand LoginCommand => new Command(async () => await LoginAsync());
        public ICommand ForgotPasswordCommand => new Command(async () => await NavigateToForgotPasswordAsync());

        private void UpdateLoginButtonAppearance()
        {
            var greenColor = (Color)Application.Current!.Resources["StaticButtonColorGreen"];
            LoginButtonBackgroundColor = greenColor;
            LoginButtonTextColor = Colors.White;
        }
        private void ResetLoginButtonAppearance()
        {
            LoginButtonBackgroundColor = Colors.Transparent;
            LoginButtonTextColor = Colors.Black;
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            email = email.Trim();

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            return Regex.IsMatch(email, emailPattern);
        }
        public async Task LoginAsync()
        {
            if (IsLoggingIn) return;

            IsLoggingIn = true;
            UpdateLoginButtonAppearance();
            await Task.Delay(200);

            try
            {
                var errorMessages = new List<string>();
                if (string.IsNullOrEmpty(Email))
                    errorMessages.Add("Por favor ingresa el correo electrónico.");
                else if (!IsValidEmail(Email))
                    errorMessages.Add("Por favor ingresa un correo electrónico válido.");
                if (string.IsNullOrEmpty(Password))
                    errorMessages.Add("Por favor ingresa la contraseña.");

                if (errorMessages.Any())
                {
                    var generalMessage = errorMessages.Count == 3
                        ? "Por favor completa todos los campos."
                        : "Por favor verifica los siguientes campos:\n\n" + string.Join("\n", errorMessages);

                    await DialogsHelper2.ShowWarningMessage(generalMessage);
                    IsLoggingIn = false;
                    ResetLoginButtonAppearance();
                    return;
                }

                DialogsHelper.ShowLoadingMessage("Iniciando sesión, por favor espere...");

                var loginFather = new LoginFather
                {
                    Email = Email.Trim(),
                    Password = Password.Trim()
                };

                var LoginApiResponse = await _fatherLoginService.LoginFatherAsync(loginFather);

                if (LoginApiResponse != null && !LoginApiResponse.IsClientError)
                {
                    if (LoginApiResponse.Result && LoginApiResponse.Valoration && LoginApiResponse.Data.Any())
                    {
                        Preferences.Set("cache_flag", true);

                        var tokenResponse = LoginApiResponse.Data[0];
                        var nombreCompleto = $"{tokenResponse.Nombre}";

                        try
                        {
                            await SecureStorage.SetAsync("Usuario_Id", tokenResponse.UsuarioId.ToString());
                            await SecureStorage.SetAsync("Tipo_Usuario_Id", tokenResponse.TipoUsuarioId.ToString());
                            await SecureStorage.SetAsync("Padre_Id", tokenResponse.PadreId.ToString());
                            await SecureStorage.SetAsync("NombreCompleto", nombreCompleto);
                            await SecureStorage.SetAsync("Correo", tokenResponse.Correo!);
                            await SecureStorage.SetAsync("auth_token", tokenResponse.Token!);
                            await SecureStorage.SetAsync("toke_finaliza", tokenResponse.Finaliza.ToString());
                            await SecureStorage.SetAsync("refresh_token", tokenResponse.RefreshToken!);
                            await SecureStorage.SetAsync("refresh_token_finaliza", tokenResponse.RefreshTokenExpiration.ToString());
                        }
                        catch (Exception secureStorageEx)
                        {
                            Console.WriteLine($"Error en SecureStorage: {secureStorageEx.Message}");
                            // Fallback a Preferences si SecureStorage falla
                            Preferences.Set("Usuario_Id", tokenResponse.UsuarioId.ToString());
                            Preferences.Set("Tipo_Usuario_Id", tokenResponse.TipoUsuarioId.ToString());
                            Preferences.Set("Padre_Id", tokenResponse.PadreId.ToString());
                            Preferences.Set("NombreCompleto", nombreCompleto);
                            Preferences.Set("Correo", tokenResponse.Correo!);
                            Preferences.Set("auth_token", tokenResponse.Token!);
                            Preferences.Set("toke_finaliza", tokenResponse.Finaliza.ToString());
                            Preferences.Set("refresh_token", tokenResponse.RefreshToken!);
                            Preferences.Set("refresh_token_finaliza", tokenResponse.RefreshTokenExpiration.ToString());
                        }

                        // Declarar al inicio del bloque donde se usa tokenResponse
                        var hijosConColor = new List<HijoConColor>();

                        if (tokenResponse.Hijos != null && tokenResponse.Hijos.Count > 0)
                        {
                            var colores = ColoresDisponibles.ToList();
                            var random = new Random();

                            foreach (var hijo in tokenResponse.Hijos)
                            {
                                var colorIndex = random.Next(colores.Count);
                                var colorSeleccionado = colores[colorIndex];
                                colores.RemoveAt(colorIndex); // evitar repetir

                                hijosConColor.Add(new HijoConColor
                                {
                                    AlumnoId = hijo.AlumnoId,
                                    NombreCompleto = hijo.NombreCompleto,
                                    Matricula = hijo.Matricula,
                                    ColorHex = colorSeleccionado
                                });
                            }

                            var hijosJson = JsonSerializer.Serialize(hijosConColor);
                            try
                            {
                                await SecureStorage.SetAsync("Hijos", hijosJson);
                            }
                            catch (Exception secureStorageEx)
                            {
                                Console.WriteLine($"Error en SecureStorage para Hijos: {secureStorageEx.Message}");
                                Preferences.Set("Hijos", hijosJson);
                            }
                        }



                        Console.WriteLine("=== Info ===");
                        Console.WriteLine($"Usuario_Id: {tokenResponse.UsuarioId}");
                        Console.WriteLine($"Tipo_Usuario_Id: {tokenResponse.TipoUsuarioId}");
                        Console.WriteLine($"NombreCompleto: {nombreCompleto}");
                        Console.WriteLine($"Correo: {tokenResponse.Correo}");
                        Console.WriteLine($"Padre_Id: {tokenResponse.PadreId}");
                        Console.WriteLine($"Hijos: {tokenResponse.Hijos?.Count}");

                        // Mostrar la lista de hijos con color
                        foreach (var hijo in hijosConColor)
                        {
                            Console.WriteLine($"Hijo: {hijo.NombreCompleto} / Matricula: {hijo.Matricula} / AlumnoID: {hijo.AlumnoId} / Color: {hijo.ColorHex}");
                        }




                        await DialogsHelper2.ShowSuccessMessage(LoginApiResponse.Message);
                        Application.Current!.MainPage = new AppShell();
                    }
                    else if (LoginApiResponse.Result && !LoginApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(LoginApiResponse.Message);
                    }
                    else if (!LoginApiResponse.Result && !LoginApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(LoginApiResponse.Log!);
                    }
                }
                else
                {
                    await DialogsHelper2.ShowErrorMessage(LoginApiResponse?.Message ?? "Error desconocido");
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
                //await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema al iniciar sesión. Por favor, intenta nuevamente más tarde.");
            }
            finally
            {
                DialogsHelper.HideLoadingMessage();
                ResetLoginButtonAppearance();
                IsLoggingIn = false;
            }
        }


        public async Task NavigateToForgotPasswordAsync()
        {
            if (IsNavigating) return;

            IsNavigating = true;

            await App.Current!.MainPage!.Navigation.PushAsync(new ForgetPasswordView());
            IsNavigating = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}