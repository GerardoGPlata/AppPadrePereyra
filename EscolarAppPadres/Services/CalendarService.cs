using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Services
{
    class CalendarService
    {
        private readonly HttpClient _httpClient;

        public CalendarService()
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
        //public async Task<ResponseModel<Event>> GetEventsAsync(long profileId, long[] studentIds, string token)
        //{
        //    const int timeoutSeconds = 30;
        //    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        //    try
        //    {
        //        // Validaciones iniciales
        //        if (profileId <= 0)
        //        {
        //            return new ResponseModel<Event>
        //            {
        //                IsClientError = true,
        //                Message = "El identificador de usuario no es válido."
        //            };
        //        }

        //        if (studentIds == null || studentIds.Length == 0)
        //        {
        //            return new ResponseModel<Event>
        //            {
        //                IsClientError = true,
        //                Message = "No se han especificado alumnos en la petición",
        //                Data = new List<Event>()
        //            };
        //        }

        //        var baseUrl = ApiRoutes.PereyraIdcUrl;
        //        Console.WriteLine($"[GetEventsAsync] Iniciando solicitud para profileId: {profileId}");

        //        // Configurar cliente HTTP
        //        _httpClient.DefaultRequestHeaders.Authorization =
        //            new AuthenticationHeaderValue("Bearer", token);

        //        // Construir URL con parámetros
        //        var queryParams = new List<string>
        //        {
        //            $"id={profileId}",
        //            $"tipo={(short)UserTypeEnum.Parent}"
        //        };

        //        queryParams.AddRange(studentIds.Select(id => $"alumnoid[]={id}"));
        //        var url = $"{baseUrl}{ApiRoutes.StudentCalendar.ApiCalendar}?{string.Join("&", queryParams)}";

        //        Console.WriteLine($"[GetEventsAsync] URL construida: {url}");

        //        // Realizar la solicitud HTTP
        //        var response = await _httpClient.GetAsync(url, cts.Token);
        //        var responseContent = await response.Content.ReadAsStringAsync();

        //        Console.WriteLine($"[GetEventsAsync] Respuesta recibida. Status: {response.StatusCode}");
        //        Console.WriteLine($"[GetEventsAsync] Contenido de respuesta: {responseContent}");

        //        // Configuración avanzada para deserialización
        //        var options = new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true,
        //            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        //            Converters = { new JsonStringEnumConverter() }
        //        };

        //        // Intentar deserialización
        //        List<Event> events;
        //        try
        //        {
        //            events = JsonSerializer.Deserialize<List<Event>>(responseContent, options);
        //        }
        //        catch (JsonException jsonEx)
        //        {
        //            Console.WriteLine($"[GetEventsAsync] Error en deserialización primaria: {jsonEx.Message}");

        //            // Intento alternativo con clase temporal
        //            try
        //            {
        //                var tempEvents = JsonSerializer.Deserialize<List<EventTemp>>(responseContent, options);
        //                events = tempEvents?.Select(e => MapEventFromTemp(e)).ToList();

        //                Console.WriteLine($"[GetEventsAsync] Deserialización alternativa exitosa. Eventos: {events?.Count ?? 0}");
        //            }
        //            catch (Exception fallbackEx)
        //            {
        //                Console.WriteLine($"[GetEventsAsync] Error en deserialización alternativa: {fallbackEx.Message}");
        //                throw new ApplicationException("No se pudo interpretar la respuesta del servidor", fallbackEx);
        //            }
        //        }

        //        // Validar respuesta
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var errorMessage = response.StatusCode switch
        //            {
        //                HttpStatusCode.Unauthorized => "No autorizado - Token inválido o expirado",
        //                HttpStatusCode.NotFound => "Recurso no encontrado",
        //                HttpStatusCode.BadRequest => "Solicitud mal formada",
        //                _ => $"Error en el servidor: {response.StatusCode}"
        //            };

        //            return new ResponseModel<Event>
        //            {
        //                IsClientError = true,
        //                Message = errorMessage,
        //                Data = events ?? new List<Event>()
        //            };
        //        }

        //        return new ResponseModel<Event>
        //        {
        //            IsClientError = false,
        //            Data = events ?? new List<Event>()
        //        };
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        Console.WriteLine($"[GetEventsAsync] Timeout después de {timeoutSeconds} segundos");
        //        return new ResponseModel<Event>
        //        {
        //            IsClientError = true,
        //            Message = $"La solicitud tardó demasiado (más de {timeoutSeconds} segundos)"
        //        };
        //    }
        //    catch (HttpRequestException httpEx)
        //    {
        //        Console.WriteLine($"[GetEventsAsync] Error de conexión: {httpEx.Message}");
        //        return new ResponseModel<Event>
        //        {
        //            IsClientError = true,
        //            Message = "Error de conexión con el servidor. Verifique su conexión a Internet."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[GetEventsAsync] Error inesperado: {ex}");
        //        return new ResponseModel<Event>
        //        {
        //            IsClientError = true,
        //            Message = $"Error inesperado: {ex.Message}"
        //        };
        //    }
        //}

        //// Clase temporal para deserialización alternativa
        //private class EventTemp
        //{
        //    public string eventoid { get; set; }
        //    public string tipoeventoid { get; set; }
        //    public string tipoevento { get; set; }
        //    public string color { get; set; }
        //    public string descripcion { get; set; }
        //    public string nombre { get; set; }
        //    public string nivelid { get; set; }
        //    public string nivel { get; set; }
        //    public string gradoid { get; set; }
        //    public string dateinicio { get; set; }
        //    public string datefin { get; set; }
        //    public string fechainicio { get; set; }
        //    public string fechafin { get; set; }
        //    public string editable { get; set; }
        //}

        //// Mapeo desde la clase temporal
        //private Event MapEventFromTemp(EventTemp temp)
        //{
        //    return new Event
        //    {
        //        EventoId = long.TryParse(temp.eventoid, out var eventoId) ? eventoId : 0,
        //        TipoEventoId = Enum.TryParse<EventTypeEnum>(temp.tipoeventoid, out var tipoEvento) ? tipoEvento : default,
        //        TipoEvento = temp.tipoevento,
        //        Color = temp.color,
        //        Descripcion = temp.descripcion,
        //        Nombre = temp.nombre,
        //        NivelId = Enum.TryParse<SchoolLevelEnum>(temp.nivelid, out var nivel) ? nivel : default,
        //        Nivel = temp.nivel,
        //        GradoId = temp.gradoid,
        //        DateInicio = DateTime.TryParse(temp.dateinicio, out var inicio) ? inicio : default,
        //        DateFin = string.IsNullOrEmpty(temp.datefin) ? null :
        //                 DateTime.TryParse(temp.datefin, out var fin) ? fin : (DateTime?)null,
        //        Editable = byte.TryParse(temp.editable, out var editable) ? editable : (byte)0
        //    };
        //}
    }
}
