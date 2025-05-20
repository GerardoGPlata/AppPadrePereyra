using System.Text;
using System.Text.Json;
using EscolarAppPadres.Constants;
using System.Net.Http.Headers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Response;
using System.Net;

namespace EscolarAppPadres.Services
{
    public class StudentLoginService
    {
        private readonly HttpClient _httpClient;

        public StudentLoginService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(ApiRoutes.BaseUrl)
            };
        }

        public async Task<ResponseModel<TokenResponse>?> LoginStudentAsync(LoginStudent loginStudent)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                var json = JsonSerializer.Serialize(loginStudent);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiRoutes.StudentLogin.LoginStudent, content, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                var LoginResponse = JsonSerializer.Deserialize<ResponseModel<TokenResponse>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return LoginResponse;

                    case HttpStatusCode.TooManyRequests:
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.InternalServerError:
                        return LoginResponse;

                    default:
                        return LoginResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<TokenResponse> {IsClientError = true, Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo." };
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout alcanzado: {ex.Message}");
                return new ResponseModel<TokenResponse> {IsClientError = true, Message = "El servidor está tardando mucho en responder. Intente nuevamente más tarde." };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error de deserialización: {ex.Message}");
                return new ResponseModel<TokenResponse> {IsClientError = true, Message = "No se pudo procesar la respuesta del servidor." };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                return new ResponseModel<TokenResponse> {IsClientError = true, Message = "Fallo al procesar la solicitud. Por favor, intenta más tarde." };
            }
        }

        public async Task<ResponseModel<object>?> ChangePasswordAsync(ChangePassword changePasswordRequest, string token)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(changePasswordRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiRoutes.StudentLogin.changePassword, content, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                var ChangePasswordResponse = JsonSerializer.Deserialize<ResponseModel<object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return ChangePasswordResponse;

                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.InternalServerError:
                        return ChangePasswordResponse;

                    default:
                        return ChangePasswordResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<object> {IsClientError = true, Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo." };
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout alcanzado: {ex.Message}");
                return new ResponseModel<object> {IsClientError = true, Message = "El servidor está tardando mucho en responder. Intente nuevamente más tarde." };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error de deserialización: {ex.Message}");
                return new ResponseModel<object> {IsClientError = true, Message = "No se pudo procesar la respuesta del servidor." };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                return new ResponseModel<object> {IsClientError = true, Message = "Fallo al procesar la solicitud. Por favor, intenta más tarde." };
            }
        }

        public async Task<ResponseModel<Token>?> RefreshTokenAsync(Token token)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                var json = JsonSerializer.Serialize(token);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiRoutes.StudentLogin.RefreshToken, content, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                var RefreshTokenResponse = JsonSerializer.Deserialize<ResponseModel<Token>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return RefreshTokenResponse;

                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.InternalServerError:
                        return RefreshTokenResponse;

                    default:
                        return RefreshTokenResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<Token> {IsClientError = true, Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo." };
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout alcanzado: {ex.Message}");
                return new ResponseModel<Token> {IsClientError = true, Message = "El servidor está tardando mucho en responder. Intente nuevamente más tarde." };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error de deserialización: {ex.Message}");
                return new ResponseModel<Token> {IsClientError = true, Message = "No se pudo procesar la respuesta del servidor." };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                return new ResponseModel<Token> { IsClientError = true, Message = "Fallo al procesar la solicitud. Por favor, intenta más tarde." };
            }
        }

        public async Task<ResponseModel<object>?> LogoutAsync(Logout logoutRequest, string token)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(logoutRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiRoutes.StudentLogin.Logout, content, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                var LogoutResponse = JsonSerializer.Deserialize<ResponseModel<object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return LogoutResponse;

                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.InternalServerError:
                        return LogoutResponse;

                    default:
                        return LogoutResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<object> {IsClientError = true, Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo." };
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout alcanzado: {ex.Message}");
                return new ResponseModel<object> { IsClientError = true, Message = "El servidor está tardando mucho en responder. Intente nuevamente más tarde." };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error de deserialización: {ex.Message}");
                return new ResponseModel<object> { IsClientError = true, Message = "No se pudo procesar la respuesta del servidor." };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                return new ResponseModel<object> {IsClientError = true, Message = "Fallo al procesar la solicitud. Por favor, intenta más tarde." };
            }
        }
    }
}
