using EscolarAppPadres.Constants;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Openpay;
using EscolarAppPadres.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EscolarAppPadres.Services
{
    class PaymentsService
    {
        private readonly HttpClient _httpClient;

        public PaymentsService()
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

        public async Task<ResponseModel<PendingPayment>?> GetStudentPaymentsAsync(string token)
        {
            const int timeoutSeconds = 30;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{ApiRoutes.BaseUrl}{ApiRoutes.Payments.GetPendingPayments}";
                Console.WriteLine($"URL de la solicitud: {url}");

                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta del servidor: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel<PendingPayment>
                    {
                        IsClientError = true,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return new ResponseModel<PendingPayment>
                    {
                        IsClientError = true,
                        Message = "El servidor devolvió una respuesta vacía"
                    };
                }

                var tempResponse = JsonSerializer.Deserialize<TempApiResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (tempResponse == null)
                {
                    return new ResponseModel<PendingPayment>
                    {
                        IsClientError = true,
                        Message = "No se pudo interpretar la respuesta del servidor"
                    };
                }

                return new ResponseModel<PendingPayment>
                {
                    Result = tempResponse.Result,
                    Valoration = tempResponse.Valoration,
                    Message = tempResponse.Message,
                    Log = tempResponse.Log?.ToString(),
                    Data = tempResponse.Data ?? new List<PendingPayment>()
                };

            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error al deserializar: {jsonEx.Message}");
                return new ResponseModel<PendingPayment>
                {
                    IsClientError = true,
                    Message = "Formato de respuesta inválido del servidor"
                };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<PendingPayment>
                {
                    IsClientError = true,
                    Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo."
                };
            }
            catch (TaskCanceledException)
            {
                return new ResponseModel<PendingPayment>
                {
                    IsClientError = true,
                    Message = "La solicitud ha expirado. Intente nuevamente."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return new ResponseModel<PendingPayment>
                {
                    IsClientError = true,
                    Message = "Ocurrió un error inesperado. Intente nuevamente."
                };
            }
        }

        public async Task<ResponseModel<OpenpayChargeResponseDto>?> CreateOpenpayChargeAsync(OpenpayChargeRequestDto request, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = $"{ApiRoutes.BaseUrl}{ApiRoutes.Payments.CreateCharge}";
                var json = JsonSerializer.Serialize(request);
                Console.WriteLine("JSON ENVIADO DESDE APP:");
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel<OpenpayChargeResponseDto>
                    {
                        IsClientError = true,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }

                var tempResponse = JsonSerializer.Deserialize<ResponseModel<OpenpayChargeResponseDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return tempResponse;
            }
            catch (Exception ex)
            {
                return new ResponseModel<OpenpayChargeResponseDto>
                {
                    IsClientError = true,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel<OpenpayChargeResponseDto>?> CreateOpenpaySimpleChargeAsync(OpenpaySimpleChargeRequestDto request, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = $"{ApiRoutes.BaseUrl}{ApiRoutes.Payments.CreateCharge}";
                var json = JsonSerializer.Serialize(request);
                Console.WriteLine("JSON ENVIADO DESDE APP:");
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel<OpenpayChargeResponseDto>
                    {
                        IsClientError = true,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }

                var tempResponse = JsonSerializer.Deserialize<ResponseModel<OpenpayChargeResponseDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return tempResponse;
            }
            catch (Exception ex)
            {
                return new ResponseModel<OpenpayChargeResponseDto>
                {
                    IsClientError = true,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel<OpenpayChargeResponseDto>?> GetOpenpayChargeStatusAsync(string transactionId, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = $"{ApiRoutes.BaseUrl}{ApiRoutes.Payments.GetStatusCharge}/{transactionId}";

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel<OpenpayChargeResponseDto>
                    {
                        IsClientError = true,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }

                var tempResponse = JsonSerializer.Deserialize<ResponseModel<OpenpayChargeResponseDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return tempResponse;
            }
            catch (Exception ex)
            {
                return new ResponseModel<OpenpayChargeResponseDto>
                {
                    IsClientError = true,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        private class TempApiResponse
        {
            public bool Result { get; set; }
            public bool Valoration { get; set; }
            public string Message { get; set; }
            public object Log { get; set; }
            public List<PendingPayment> Data { get; set; }
        }

    }
}
