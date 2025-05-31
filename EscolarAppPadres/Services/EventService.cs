using EscolarAppPadres.Constants;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Response;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EscolarAppPadres.Services
{
    public class EventService
    {
        private readonly HttpClient _httpClient;

        public EventService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
        }

        /// <summary>
        /// Obtiene una lista de eventos asociados a un perfil y a uno o más estudiantes específicos.
        /// </summary>
        /// <param name="profileId">Identificador del perfil del usuario (por ejemplo, un padre de familia).</param>
        /// <param name="studentIds">Arreglo de identificadores de estudiantes asociados al perfil.</param>
        /// <param name="token">Token de autenticación Bearer para autorizar la solicitud HTTP.</param>
        /// <returns>
        /// Una tarea asincrónica que contiene un objeto <see cref="ResponseModel{Event}"/> con la lista de eventos
        /// o un mensaje de error en caso de falla.
        /// </returns>
        public async Task<ResponseModel<Event>> GetEventsAsync(long profileId, long[] studentIds, string token)
        {
            const int timeoutSeconds = 30;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                // Si el profileId es inválido, usar uno genérico
                if (profileId <= 0)
                {
                    Console.WriteLine("[GetEventsAsync] profileId inválido, usando valor por defecto 1.");
                    profileId = 1;
                }

                // Si no hay studentIds válidos, usar uno genérico
                if (studentIds == null || studentIds.Length == 0)
                {
                    Console.WriteLine("[GetEventsAsync] studentIds vacíos o nulos, usando valor por defecto [1000].");
                    studentIds = new long[] { 1000 };
                }

                var baseUrl = ApiRoutes.PereyraIdcUrl;
                Console.WriteLine($"[GetEventsAsync] Iniciando solicitud para profileId: {profileId}");

                // Configurar cliente HTTP con token válido
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Construir URL con parámetros
                var queryParams = new List<string>
                {
                    $"id={profileId}",
                    $"tipo={(short)UserTypeEnum.Parent}"
                };

                queryParams.AddRange(studentIds.Select(id => $"alumnoid[]={id}"));
                var url = $"{baseUrl}{ApiRoutes.StudentCalendar.ApiCalendar}?{string.Join("&", queryParams)}";

                Console.WriteLine($"[GetEventsAsync] URL construida: {url}");

                // Hacer solicitud
                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[GetEventsAsync] Respuesta recibida. Status: {response.StatusCode}");
                Console.WriteLine($"[GetEventsAsync] Contenido: {responseContent}");

                // Deserializar
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(), new EventJsonConverter() }
                };

                List<Event> events;
                try
                {
                    events = JsonSerializer.Deserialize<List<Event>>(responseContent, options);
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"[GetEventsAsync] Error de deserialización: {jsonEx.Message}");
                    throw new ApplicationException("No se pudo interpretar la respuesta del servidor", jsonEx);
                }

                // Validar respuesta HTTP
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => "No autorizado - Token inválido o expirado",
                        HttpStatusCode.NotFound => "Recurso no encontrado",
                        HttpStatusCode.BadRequest => "Solicitud mal formada",
                        _ => $"Error en el servidor: {response.StatusCode}"
                    };

                    return new ResponseModel<Event>
                    {
                        IsClientError = true,
                        Message = errorMessage,
                        Data = events ?? new List<Event>()
                    };
                }

                return new ResponseModel<Event>
                {
                    IsClientError = false,
                    Data = events ?? new List<Event>()
                };
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"[GetEventsAsync] Timeout después de {timeoutSeconds} segundos");
                return new ResponseModel<Event>
                {
                    IsClientError = true,
                    Message = $"La solicitud tardó demasiado (más de {timeoutSeconds} segundos)"
                };
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[GetEventsAsync] Error de conexión: {httpEx.Message}");
                return new ResponseModel<Event>
                {
                    IsClientError = true,
                    Message = "Error de conexión con el servidor. Verifique su conexión a Internet."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetEventsAsync] Error inesperado: {ex}");
                return new ResponseModel<Event>
                {
                    IsClientError = true,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }


        public class EventJsonConverter : JsonConverter<Event>
        {
            public override Event Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                return new Event
                {
                    EventoId = long.TryParse(root.GetProperty("eventoid").GetString(), out var eid) ? eid : 0,
                    TipoEventoId = Enum.TryParse<EventTypeEnum>(root.GetProperty("tipoeventoid").GetString(), out var tipo) ? tipo : default,
                    TipoEvento = root.GetProperty("tipoevento").GetString(),
                    Color = root.GetProperty("color").GetString(),
                    Descripcion = root.GetProperty("descripcion").GetString(),
                    Nombre = root.GetProperty("nombre").GetString(),
                    NivelId = Enum.TryParse<SchoolLevelEnum>(root.GetProperty("nivelid").GetString(), out var nivel) ? nivel : default,
                    Nivel = root.GetProperty("nivel").GetString(),
                    GradoId = root.GetProperty("gradoid").GetString(),
                    DateInicio = DateTime.TryParse(root.GetProperty("dateinicio").GetString(), out var inicio) ? inicio : default,
                    DateFin = DateTime.TryParse(root.GetProperty("datefin").GetString(), out var fin) ? fin : null,
                    Editable = byte.TryParse(root.GetProperty("editable").GetString(), out var editable) ? editable : (byte)0
                };
            }

            public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
            {
                throw new NotImplementedException("Solo lectura implementada.");
            }
        }

    }
}
