using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using EscolarAppPadres.Constants;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Response;
using System.Net;

namespace EscolarAppPadres.Services
{
    public class EncryptionService
    {
        private readonly HttpClient _httpClient;

        public EncryptionService()
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

        public async Task<ResponseModel<Encrypted>?> EncryptDataAsync(Encrypted encrypted)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                var json = JsonSerializer.Serialize(encrypted);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiRoutes.StudentLogin.Encrypt, content, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                var EncrypResponse = JsonSerializer.Deserialize<ResponseModel<Encrypted>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return EncrypResponse;

                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.InternalServerError:
                        return EncrypResponse;

                    default:
                        return EncrypResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<Encrypted> { IsClientError = true, Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo." };
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout alcanzado: {ex.Message}");
                return new ResponseModel<Encrypted> {IsClientError = true, Message = "El servidor está tardando mucho en responder. Intente nuevamente más tarde." };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error de deserialización: {ex.Message}");
                return new ResponseModel<Encrypted> {IsClientError = true, Message = "No se pudo procesar la respuesta del servidor." };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                return new ResponseModel<Encrypted> {IsClientError = true, Message = "Fallo al procesar la solicitud. Por favor, intenta más tarde." };
            }
        }

        public async Task<ResponseModel<Decrypt>?> DecryptDataAsync(Decrypt decrypt)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", decrypt.token);

                var json = JsonSerializer.Serialize(decrypt);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiRoutes.StudentLogin.Decrypt, content, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                var DecryptResponse = JsonSerializer.Deserialize<ResponseModel<Decrypt>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return DecryptResponse;

                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.InternalServerError:
                        return DecryptResponse;

                    default:
                        return DecryptResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<Decrypt> {IsClientError = true, Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo." };
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout alcanzado: {ex.Message}");
                return new ResponseModel<Decrypt> {IsClientError = true, Message = "El servidor está tardando mucho en responder. Intente nuevamente más tarde." };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error de deserialización: {ex.Message}");
                return new ResponseModel<Decrypt> {IsClientError = true, Message = "No se pudo procesar la respuesta del servidor." };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                return new ResponseModel<Decrypt> {IsClientError = true, Message = "Fallo al procesar la solicitud. Por favor, intenta más tarde." };
            }
        }
    }
}
