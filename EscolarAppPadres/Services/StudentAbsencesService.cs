using EscolarAppPadres.Constants;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EscolarAppPadres.Services
{
    class StudentAbsencesService
    {
        private readonly HttpClient _httpClient;

        public StudentAbsencesService()
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

        public async Task<ResponseModel<StudentAbsence>?> GetAbsencesAsync(string token, string studentId)
        {
            const int timeoutSeconds = 30;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Reemplazar {studentId} en la URL
                var url = $"{ApiRoutes.BaseUrl}{ApiRoutes.StudentAbsences.GetStudentAbsences.Replace("{studentId}", studentId)}";
                Console.WriteLine($"URL de la solicitud: {url}");

                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta del servidor: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel<StudentAbsence>
                    {
                        IsClientError = true,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return new ResponseModel<StudentAbsence>
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
                    return new ResponseModel<StudentAbsence>
                    {
                        IsClientError = true,
                        Message = "No se pudo interpretar la respuesta del servidor"
                    };
                }

                return new ResponseModel<StudentAbsence>
                {
                    Result = tempResponse.Result,
                    Valoration = tempResponse.Valoration,
                    Message = tempResponse.Message,
                    Log = tempResponse.Log?.ToString(),
                    Data = tempResponse.Data?.FirstOrDefault() ?? new List<StudentAbsence>()
                };
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error al deserializar: {jsonEx.Message}");
                return new ResponseModel<StudentAbsence>
                {
                    IsClientError = true,
                    Message = "Formato de respuesta inválido del servidor"
                };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                return new ResponseModel<StudentAbsence>
                {
                    IsClientError = true,
                    Message = "No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo."
                };
            }
            catch (TaskCanceledException)
            {
                return new ResponseModel<StudentAbsence>
                {
                    IsClientError = true,
                    Message = "La solicitud ha expirado. Intente nuevamente."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return new ResponseModel<StudentAbsence>
                {
                    IsClientError = true,
                    Message = "Ocurrió un error inesperado. Intente nuevamente."
                };
            }
        }

        private class TempApiResponse
        {
            public bool Result { get; set; }
            public bool Valoration { get; set; }
            public string Message { get; set; }
            public object Log { get; set; }
            public List<List<StudentAbsence>> Data { get; set; }
        }
    }
}
