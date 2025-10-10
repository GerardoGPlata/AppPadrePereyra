using EscolarAppPadres.Constants;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                _ = studentIds;
                Console.WriteLine($"[GetEventsAsync] Solicitando calendario completo (perfil: {profileId}).");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return new ResponseModel<Event>
                    {
                        IsClientError = true,
                        Message = "Token de autenticación inválido."
                    };
                }

                var baseUrl = ApiRoutes.BaseUrl;
                var url = $"{baseUrl}{ApiRoutes.Calendar.getAllEventsCalendar}";

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(url, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => "No autorizado - Token inválido o expirado",
                        HttpStatusCode.NotFound => "Recurso no encontrado",
                        HttpStatusCode.BadRequest => "Solicitud mal formada",
                        _ => $"Error en el servidor: {response.StatusCode}"
                    };

                    Console.WriteLine($"[GetEventsAsync] Error HTTP: {response.StatusCode} - {responseContent}");

                    return new ResponseModel<Event>
                    {
                        IsClientError = true,
                        Message = errorMessage
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                CalendarEventsResponse? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<CalendarEventsResponse>(responseContent, options);
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"[GetEventsAsync] Error de deserialización: {jsonEx.Message}");
                    return new ResponseModel<Event>
                    {
                        IsClientError = true,
                        Message = "No se pudo interpretar la respuesta del servidor."
                    };
                }

                if (apiResponse == null)
                {
                    return new ResponseModel<Event>
                    {
                        IsClientError = true,
                        Message = "No se recibió información del servidor."
                    };
                }

                var mappedEvents = apiResponse.Data?
                    .Select(MapToEvent)
                    .Where(e => e != null)
                    .Cast<Event>()
                    .ToList() ?? new List<Event>();

                return new ResponseModel<Event>
                {
                    Result = apiResponse.Result,
                    Valoration = apiResponse.Valoration,
                    Message = apiResponse.Message ?? string.Empty,
                    Log = apiResponse.Log,
                    Data = mappedEvents,
                    IsClientError = !apiResponse.Result
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
        private static Event? MapToEvent(CalendarEventDto? dto)
        {
            if (dto == null)
            {
                return null;
            }

            var resolvedColor = dto.Color;
            if (string.IsNullOrWhiteSpace(resolvedColor))
            {
                resolvedColor = "#32CD32";
            }

            var evento = new Event
            {
                EventoId = long.TryParse(dto.EventoId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var eid) ? eid : 0,
                TipoEventoId = ParseEventType(dto.TipoEventoId),
                TipoEvento = dto.TipoEvento ?? string.Empty,
                Color = resolvedColor,
                Descripcion = dto.Descripcion ?? string.Empty,
                Nombre = dto.Nombre ?? string.Empty,
                NivelId = dto.NivelId,
                Nivel = dto.Nivel ?? string.Empty,
                GradoId = dto.GradoId ?? string.Empty,
                FechaInicioTexto = dto.FechaInicio,
                FechaFinTexto = dto.FechaFin,
                HoraInicioTexto = dto.HoraInicio,
                HoraFinTexto = dto.HoraFin,
                Editable = byte.TryParse(dto.Editable, NumberStyles.Integer, CultureInfo.InvariantCulture, out var editable) ? editable : (byte)0
            };

            var startDate = ParseDate(dto.DateInicio, dto.FechaInicio) ?? DateTime.Today;
            var startTime = ParseTime(dto.HoraInicio);
            evento.HoraInicio = startTime;
            evento.DateInicio = startTime.HasValue ? startDate.Date + startTime.Value : startDate.Date;

            var endDate = ParseDate(dto.DateFin, dto.FechaFin);
            var endTime = ParseTime(dto.HoraFin);
            evento.HoraFin = endTime;

            DateTime? endDateTime = null;
            if (endDate.HasValue)
            {
                endDateTime = endDate.Value.Date + (endTime ?? TimeSpan.Zero);
            }
            else if (endTime.HasValue)
            {
                endDateTime = evento.DateInicio.Date + endTime.Value;
            }

            if (endDateTime.HasValue)
            {
                if (endDateTime.Value <= evento.DateInicio)
                {
                    endDateTime = evento.DateInicio.AddHours(1);
                }

                evento.DateFin = endDateTime;
            }
            else
            {
                evento.DateFin = null;
            }

            evento.DiaEntero = !startTime.HasValue && !endTime.HasValue;

            return evento;
        }

        private static DateTime? ParseDate(string? value, string? alternative)
        {
            if (TryParseDate(value, out var primary))
            {
                return primary;
            }

            if (TryParseDate(alternative, out var secondary))
            {
                return secondary;
            }

            return null;
        }

        private static bool TryParseDate(string? value, out DateTime date)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                date = default;
                return false;
            }

            var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "MM/dd/yyyy" };
            return DateTime.TryParseExact(value,
                                          formats,
                                          CultureInfo.InvariantCulture,
                                          DateTimeStyles.None,
                                          out date);
        }

        private static TimeSpan? ParseTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result)
                ? result
                : null;
        }

        private static EventTypeEnum ParseEventType(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return EventTypeEnum.Undefined;
            }

            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericValue))
            {
                if (Enum.IsDefined(typeof(EventTypeEnum), numericValue))
                {
                    return (EventTypeEnum)numericValue;
                }
            }

            return Enum.TryParse<EventTypeEnum>(rawValue, true, out var parsed)
                ? parsed
                : EventTypeEnum.Undefined;
        }

        private sealed class CalendarEventsResponse
        {
            [JsonPropertyName("result")]
            public bool Result { get; set; }

            [JsonPropertyName("valoration")]
            public bool Valoration { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("log")]
            public string? Log { get; set; }

            [JsonPropertyName("data")]
            public List<CalendarEventDto>? Data { get; set; }
        }

        private sealed class CalendarEventDto
        {
            [JsonPropertyName("eventoid")]
            public string? EventoId { get; set; }

            [JsonPropertyName("tipoeventoid")]
            public string? TipoEventoId { get; set; }

            [JsonPropertyName("tipoevento")]
            public string? TipoEvento { get; set; }

            [JsonPropertyName("color")]
            public string? Color { get; set; }

            [JsonPropertyName("descripcion")]
            public string? Descripcion { get; set; }

            [JsonPropertyName("nombre")]
            public string? Nombre { get; set; }

            [JsonPropertyName("nivelid")]
            public string? NivelId { get; set; }

            [JsonPropertyName("nivel")]
            public string? Nivel { get; set; }

            [JsonPropertyName("gradoid")]
            public string? GradoId { get; set; }

            [JsonPropertyName("dateinicio")]
            public string? DateInicio { get; set; }

            [JsonPropertyName("datefin")]
            public string? DateFin { get; set; }

            [JsonPropertyName("fechainicio")]
            public string? FechaInicio { get; set; }

            [JsonPropertyName("fechafin")]
            public string? FechaFin { get; set; }

            [JsonPropertyName("horainicio")]
            public string? HoraInicio { get; set; }

            [JsonPropertyName("horafin")]
            public string? HoraFin { get; set; }

            [JsonPropertyName("editable")]
            public string? Editable { get; set; }
        }
    }
}
