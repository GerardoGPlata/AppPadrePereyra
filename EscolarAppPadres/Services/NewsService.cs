using System.Text;
using System.Text.Json;
using EscolarAppPadres.Constants;
using System.Net.Http.Headers;
using EscolarAppPadres.Models.Response;
using EscolarAppPadres.Models;
using System.Net;

namespace EscolarAppPadres.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;

        public NewsService()
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

        public async Task<ResponseModel<NotificacionesLeidas>?> GetNewsAsync(int id, string token)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{ApiRoutes.BaseUrl}{ApiRoutes.StudentNews.GetNews}/{id}";

                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();

                var GetNewsResponse = JsonSerializer.Deserialize<ResponseModel<NotificacionesLeidas>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return GetNewsResponse;

                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.InternalServerError:
                        return GetNewsResponse;

                    default:
                        return GetNewsResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<NotificacionesLeidas> { IsClientError = true, Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo." };
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout alcanzado: {ex.Message}");
                return new ResponseModel<NotificacionesLeidas> { IsClientError = true, Message = "El servidor está tardando mucho en responder. Intente nuevamente más tarde." };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error de deserialización: {ex.Message}");
                return new ResponseModel<NotificacionesLeidas> { IsClientError = true, Message = "No se pudo procesar la respuesta del servidor." };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                return new ResponseModel<NotificacionesLeidas> { IsClientError = true, Message = "Fallo al procesar la solicitud. Por favor, intenta más tarde." };
            }
        }

        public async Task<ResponseModel<NotificacionesLeidas>?> UpdateNewsReadAsync(int id, NotificacionesLeidas notificacionesLeidas, string token)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(notificacionesLeidas);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{ApiRoutes.BaseUrl}{ApiRoutes.StudentNews.UpdateNews}/{id}";

                var response = await _httpClient.PutAsync(url, content, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();

                var UpdateNewsNotificationResponse = JsonSerializer.Deserialize<ResponseModel<NotificacionesLeidas>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return UpdateNewsNotificationResponse;

                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.InternalServerError:
                        return UpdateNewsNotificationResponse;

                    default:
                        return UpdateNewsNotificationResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<NotificacionesLeidas> { IsClientError = true, Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo." };
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine($"Timeout alcanzado: {ex.Message}");
                return new ResponseModel<NotificacionesLeidas> { IsClientError = true, Message = "El servidor está tardando mucho en responder. Intente nuevamente más tarde." };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error de deserialización: {ex.Message}");
                return new ResponseModel<NotificacionesLeidas> { IsClientError = true, Message = "No se pudo procesar la respuesta del servidor." };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                return new ResponseModel<NotificacionesLeidas> { IsClientError = true, Message = "Fallo al procesar la solicitud. Por favor, intenta más tarde." };
            }
        }
    }
}
