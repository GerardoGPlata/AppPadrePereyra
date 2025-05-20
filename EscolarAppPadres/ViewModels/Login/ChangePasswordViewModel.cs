using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using EscolarAppPadres.Helpers;

namespace EscolarAppPadres.ViewModels.Login
{
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private readonly StudentLoginService _studentLoginService;
        private readonly EncryptionService _encryptionService;

        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;

        private Color _changePasswordButtonBackgroundColor = Colors.Transparent;
        private Color _changePasswordButtonTextColor = Colors.Black;

        public ChangePasswordViewModel()
        {
            _studentLoginService = new StudentLoginService();
            _encryptionService = new EncryptionService();
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set 
            {
                _currentPassword = value;
                OnPropertyChanged(nameof(CurrentPassword));
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public Color ChangePasswordButtonBackgroundColor
        {
            get => _changePasswordButtonBackgroundColor;
            set
            {
                _changePasswordButtonBackgroundColor = value;
                OnPropertyChanged(nameof(ChangePasswordButtonBackgroundColor));
            }
        }

        public Color ChangePasswordButtonTextColor
        {
            get => _changePasswordButtonTextColor;
            set
            {
                _changePasswordButtonTextColor = value;
                OnPropertyChanged(nameof(ChangePasswordButtonTextColor));
            }
        }

        public ICommand ChangePasswordCommand => new Command(async () => await ChangePasswordAsync());

        private void UpdateChangePasswordButtonAppearance()
        {
            var greenColor = (Color)Application.Current!.Resources["StaticButtonColorGreen"];
            ChangePasswordButtonBackgroundColor = greenColor;
            ChangePasswordButtonTextColor = Colors.White;
        }
        private void ResetChangePasswordButtonAppearance()
        {
            ChangePasswordButtonBackgroundColor = Colors.Transparent;
            ChangePasswordButtonTextColor = Colors.Black;
        }

        public async Task ChangePasswordAsync()
        {
            UpdateChangePasswordButtonAppearance();
            await Task.Delay(200);

            try
            {
                var errorMessages = new List<string>();

                if (string.IsNullOrEmpty(CurrentPassword))
                    errorMessages.Add("Por favor ingresa la contraseña actual.");

                if (string.IsNullOrEmpty(NewPassword))
                    errorMessages.Add("Por favor ingresa la nueva contraseña.");

                if (string.IsNullOrEmpty(ConfirmPassword))
                    errorMessages.Add("Por favor confirma la nueva contraseña.");

                if (errorMessages.Any())
                {
                    var generalMessage = errorMessages.Count == 3
                        ? "Por favor completa todos los campos."
                        : "Por favor verifica los siguientes campos:\n\n" + string.Join("\n", errorMessages);
                    await DialogsHelper2.ShowWarningMessage(generalMessage);
                    ResetChangePasswordButtonAppearance();
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    await DialogsHelper2.ShowWarningMessage("Las contraseñas nuevas no coinciden.");
                    ResetChangePasswordButtonAppearance();
                    return;
                }

                var token = await SecureStorage.GetAsync("auth_token");
                if (token == null)
                {
                    await DialogsHelper2.ShowErrorMessage("No se encontró el token de autenticación.");
                    return;
                }

                DialogsHelper.ShowLoadingMessage("Cambiando contraseña, por favor espere...");

                var encryptedData = new Encrypted
                {
                    IsMultiple = true,
                    DataList = new List<string>
                    {
                        CurrentPassword.Trim(),
                        NewPassword.Trim()
                    }
                };

                var EncryptionApiResponse = await _encryptionService.EncryptDataAsync(encryptedData);

                if (EncryptionApiResponse != null && !EncryptionApiResponse.IsClientError)
                {
                    if(EncryptionApiResponse.Result && EncryptionApiResponse.Valoration && EncryptionApiResponse.Data.Any())
                    {
                        var encryptedLoginData = EncryptionApiResponse.Data[0];

                        var changePasswordRequest = new ChangePassword
                        {
                            CurrentPassword = encryptedLoginData.DataList!.ElementAtOrDefault(0),
                            NewPassword = encryptedLoginData.DataList!.ElementAtOrDefault(1)
                        };

                        var ChangePasswordApiResponse = await _studentLoginService.ChangePasswordAsync(changePasswordRequest, token);

                        if (ChangePasswordApiResponse != null && !ChangePasswordApiResponse.IsClientError)
                        {
                            if (ChangePasswordApiResponse.Result && ChangePasswordApiResponse.Valoration)
                            {
                                await DialogsHelper2.ShowSuccessMessage(ChangePasswordApiResponse.Message);
                                await Shell.Current.GoToAsync("//AccountView", true);
                            }
                            else if (ChangePasswordApiResponse.Result && !ChangePasswordApiResponse.Valoration)
                            {
                                await DialogsHelper2.ShowErrorMessage(ChangePasswordApiResponse.Message);
                            }
                            else if (!ChangePasswordApiResponse.Result && !ChangePasswordApiResponse.Valoration)
                            {
                                await DialogsHelper2.ShowErrorMessage(ChangePasswordApiResponse.Log!);
                            }
                        }
                        else
                        {
                            await DialogsHelper2.ShowErrorMessage(ChangePasswordApiResponse?.Message ?? "Error desconocido");
                        }
                    }
                    else if (EncryptionApiResponse.Result && !EncryptionApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(EncryptionApiResponse.Message);
                    }
                    else if (!EncryptionApiResponse.Result && !EncryptionApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(EncryptionApiResponse.Log!);
                    }
                }
                else
                {
                    await DialogsHelper2.ShowErrorMessage(EncryptionApiResponse?.Message ?? "Error desconocido");
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
                await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema al cambiar contraseña. Por favor, intenta nuevamente más tarde.");
            }
            finally
            {
                DialogsHelper.HideLoadingMessage();
                ResetChangePasswordButtonAppearance();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
