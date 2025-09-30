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

        public async Task<ResponseModel<CreateChargeMovilResponseDto>?> CreateChargeMovilAsync(CreateChargeMovilRequestDto request, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiRoutes.Payments.CreateChargeMovil, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[DEBUG] CreateChargeMovil Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel<CreateChargeMovilResponseDto>
                    {
                        IsClientError = true,
                        Message = $"Error del servidor: {response.StatusCode}",
                        Data = new List<CreateChargeMovilResponseDto>() // Lista vacía
                    };
                }

                // Deserializar la respuesta del API
                var tempResponse = JsonSerializer.Deserialize<TempCreateChargeResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (tempResponse?.Result == true && tempResponse.Data?.Any() == true)
                {
                    var data = tempResponse.Data.First();

                    return new ResponseModel<CreateChargeMovilResponseDto>
                    {
                        Result = true,
                        Valoration = true,
                        Message = tempResponse.Message,
                        Data = new List<CreateChargeMovilResponseDto> // ← Crear lista con un elemento
                {
                    new CreateChargeMovilResponseDto
                    {
                        TransactionId = data.TransactionId,
                        PaymentUrl = data.PaymentUrl,
                        Referencia = data.Referencia
                    }
                }
                    };
                }

                return new ResponseModel<CreateChargeMovilResponseDto>
                {
                    IsClientError = true,
                    Message = tempResponse?.Message ?? "Error desconocido",
                    Data = new List<CreateChargeMovilResponseDto>() // Lista vacía
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CreateChargeMovil: {ex.Message}");
                return new ResponseModel<CreateChargeMovilResponseDto>
                {
                    IsClientError = true,
                    Message = $"Error: {ex.Message}",
                    Data = new List<CreateChargeMovilResponseDto>() // Lista vacía
                };
            }
        }
        // Clase temporal para deserializar la respuesta del API
        private class TempCreateChargeResponse
        {
            public bool Result { get; set; }
            public string Message { get; set; }
            public bool Valoration { get; set; }
            public List<CreateChargeMovilResponseData> Data { get; set; }
        }

        private class CreateChargeMovilResponseData
        {
            public string TransactionId { get; set; }
            public string PaymentUrl { get; set; }
            public string Referencia { get; set; }
        }

        /// <summary>
        /// Obtiene los pagos pendientes del estudiante.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ResponseModel<PendingPaymentsResponse>?> GetStudentPaymentsAsync(string token)
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
                    return new ResponseModel<PendingPaymentsResponse>
                    {
                        IsClientError = true,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return new ResponseModel<PendingPaymentsResponse>
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
                    return new ResponseModel<PendingPaymentsResponse>
                    {
                        IsClientError = true,
                        Message = "No se pudo interpretar la respuesta del servidor"
                    };
                }

                var pendingPaymentsResponse = tempResponse.Data?.FirstOrDefault() ?? new PendingPaymentsResponse();

                return new ResponseModel<PendingPaymentsResponse>
                {
                    Result = tempResponse.Result,
                    Valoration = tempResponse.Valoration,
                    Message = tempResponse.Message,
                    Log = tempResponse.Log?.ToString(),
                    Data = new List<PendingPaymentsResponse> { pendingPaymentsResponse }
                };

            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error al deserializar: {jsonEx.Message}");
                return new ResponseModel<PendingPaymentsResponse>
                {
                    IsClientError = true,
                    Message = "Formato de respuesta inválido del servidor"
                };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<PendingPaymentsResponse>
                {
                    IsClientError = true,
                    Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo."
                };
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Tiempo de espera agotado");
                return new ResponseModel<PendingPaymentsResponse>
                {
                    IsClientError = true,
                    Message = $"La solicitud tardó más de {timeoutSeconds} segundos. Verifique su conexión e intente de nuevo."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return new ResponseModel<PendingPaymentsResponse>
                {
                    IsClientError = true,
                    Message = "Ocurrió un error inesperado. Intente de nuevo."
                };
            }
        }

        /// <summary>
        /// Obtiene los pagos ya realizados del estudiante.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ResponseModel<PendingPaymentsResponse>?> GetPaidPaymentsAsync(string token)
        {
            const int timeoutSeconds = 30;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{ApiRoutes.BaseUrl}{ApiRoutes.Payments.GetPaidPayments}";
                Console.WriteLine($"URL de la solicitud: {url}");

                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta del servidor: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel<PendingPaymentsResponse>
                    {
                        IsClientError = true,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return new ResponseModel<PendingPaymentsResponse>
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
                    return new ResponseModel<PendingPaymentsResponse>
                    {
                        IsClientError = true,
                        Message = "No se pudo interpretar la respuesta del servidor"
                    };
                }

                var pendingPaymentsResponse = tempResponse.Data?.FirstOrDefault() ?? new PendingPaymentsResponse();

                return new ResponseModel<PendingPaymentsResponse>
                {
                    Result = tempResponse.Result,
                    Valoration = tempResponse.Valoration,
                    Message = tempResponse.Message,
                    Log = tempResponse.Log?.ToString(),
                    Data = new List<PendingPaymentsResponse> { pendingPaymentsResponse }
                };

            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error al deserializar: {jsonEx.Message}");
                return new ResponseModel<PendingPaymentsResponse>
                {
                    IsClientError = true,
                    Message = "Formato de respuesta inválido del servidor"
                };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<PendingPaymentsResponse>
                {
                    IsClientError = true,
                    Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo."
                };
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Tiempo de espera agotado");
                return new ResponseModel<PendingPaymentsResponse>
                {
                    IsClientError = true,
                    Message = $"La solicitud tardó más de {timeoutSeconds} segundos. Verifique su conexión e intente de nuevo."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return new ResponseModel<PendingPaymentsResponse>
                {
                    IsClientError = true,
                    Message = "Ocurrió un error inesperado. Intente de nuevo."
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
            public List<PendingPaymentsResponse> Data { get; set; }
        }

    }
}
