using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarAppPadres.ViewModels.Login
{
    public class RefreshTokenViewModel
    {
        private readonly StudentLoginService _studentLoginService;

        public RefreshTokenViewModel(StudentLoginService studentLoginService)
        {
            _studentLoginService = studentLoginService;
        }

        public async Task<string?> EnsureTokenValidAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var finalizaTokenStr = await SecureStorage.GetAsync("toke_finaliza");

                var refreshToken = await SecureStorage.GetAsync("refresh_token");
                var finalizaRefreshTokenExpirationStr = await SecureStorage.GetAsync("refresh_token_finaliza");

                var usuarioIdStr = await SecureStorage.GetAsync("Usuario_Id");
                if (usuarioIdStr == null) return null;
                int userId = int.Parse(usuarioIdStr!);

                DateTime finalizaToken = DateTime.UtcNow;
                DateTime finalizaRefreshTokenExpiration = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(finalizaTokenStr) && !string.IsNullOrEmpty(finalizaRefreshTokenExpirationStr))
                {
                    finalizaToken = DateTime.Parse(finalizaTokenStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    finalizaRefreshTokenExpiration = DateTime.Parse(finalizaRefreshTokenExpirationStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
                }

                if (DateTime.UtcNow > finalizaRefreshTokenExpiration)
                {
                    await DialogsHelper2.ShowWarningMessage("La sesión ha expirado. Vuelve a iniciar sesión.");
                    SecureStorage.RemoveAll();
                    Preferences.Clear();
                    return null;
                }

                if (DateTime.UtcNow.AddMinutes(5) > finalizaToken)
                {
                    var TokenData = new Token
                    {
                        UsuarioId = userId,
                        WebToken = token,
                        Finaliza = finalizaToken,
                    };

                    var RefreshTokenApiResponse = await _studentLoginService.RefreshTokenAsync(TokenData);

                    if (RefreshTokenApiResponse != null && RefreshTokenApiResponse.Data.Any())
                    {
                        var newTokenResponse = RefreshTokenApiResponse.Data[0];

                        await SecureStorage.SetAsync("Usuario_Id", newTokenResponse.UsuarioId.ToString());
                        await SecureStorage.SetAsync("auth_token", newTokenResponse.WebToken!);
                        await SecureStorage.SetAsync("toke_finaliza", newTokenResponse.Finaliza.ToString("o"));

                        token = newTokenResponse.WebToken;
                    }
                    else
                    {
                        SecureStorage.RemoveAll();
                        Preferences.Clear();
                        return null;
                    }
                }

                return token;
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

                return null;
            }
        }
    }
}