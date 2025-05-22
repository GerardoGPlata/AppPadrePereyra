using EscolarAppPadres.Constants;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EscolarAppPadres.Services
{
    public class StudentConductualReportService
    {
        private readonly HttpClient _httpClient;
        public StudentConductualReportService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResponseModel<StudentConductualReport>> GetStudentConductualReportAsync(string token, string studentId)
        {
            const int timeoutSeconds = 30;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = $"{ApiRoutes.BaseUrl}{ApiRoutes.StudentReports.GetStudentConductualReports.Replace("{studentId}", studentId)}";
                Console.WriteLine($"URL de la solicitud: {url}");
                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta del servidor: {responseContent}");
                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel<StudentConductualReport>
                    {
                        IsClientError = true,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return new ResponseModel<StudentConductualReport>
                    {
                        IsClientError = true,
                        Message = "El servidor devolvió una respuesta vacía"
                    };
                }
                var apiResponse = JsonSerializer.Deserialize<TempApiResponse>(responseContent);
                if (apiResponse == null || !apiResponse.Result)
                {
                    return new ResponseModel<StudentConductualReport>
                    {
                        IsClientError = true,
                        Message = apiResponse?.Message ?? "Error desconocido"
                    };
                }
                return new ResponseModel<StudentConductualReport>
                {
                    IsClientError = false,
                    Data = apiResponse.Data
                };
            }
            catch (TaskCanceledException)
            {
                return new ResponseModel<StudentConductualReport>
                {
                    IsClientError = true,
                    Message = "La solicitud ha superado el tiempo de espera."
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<StudentConductualReport>
                {
                    IsClientError = true,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }


        private class TempApiResponse
        {
            public bool Result { get; set; }
            public bool Valoration { get; set; }
            public string Message { get; set; }
            public object Log { get; set; }
            public List<StudentConductualReport> Data { get; set; }
        }
    }
}
