using EscolarAppPadres.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class Event
    {
        [JsonPropertyName("eventoid")]
        public long EventoId { get; set; }

        [JsonPropertyName("tipoeventoid")]
        public EventTypeEnum TipoEventoId { get; set; }

        [JsonPropertyName("tipoevento")]
        public string TipoEvento { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("nivelid")]
        public string? NivelId { get; set; }

        [JsonPropertyName("nivel")]
        public string Nivel { get; set; }

        [JsonPropertyName("gradoid")]
        public string GradoId { get; set; }

        [JsonPropertyName("dateinicio")]
        public DateTime DateInicio { get; set; }

        [JsonPropertyName("datefin")]
        public DateTime? DateFin { get; set; }

        [JsonPropertyName("fechainicio")]
        public string? FechaInicioTexto { get; set; }

        [JsonPropertyName("fechafin")]
        public string? FechaFinTexto { get; set; }

        [JsonPropertyName("horainicio")]
        public string? HoraInicioTexto { get; set; }

        [JsonPropertyName("horafin")]
        public string? HoraFinTexto { get; set; }

        [JsonIgnore]
        public TimeSpan? HoraInicio { get; set; }

        [JsonIgnore]
        public TimeSpan? HoraFin { get; set; }

        [JsonPropertyName("editable")]
        public byte Editable { get; set; }

        // These fields aren't in the API response - remove or mark as optional
        public long? ImagenPorEventoId { get; set; }
        public long? NotificacionId { get; set; }
        public long? TareaId { get; set; }
        public long AlumnoId { get; set; }
        public long? MateriaId { get; set; }
        public bool Visto { get; set; }
        public bool DiaEntero { get; set; }
        public string Alumno { get; set; }
        public string Imagen { get; set; }

        [JsonIgnore]
        public bool EsEditable => Editable == 1;

        // Computed properties
        public DateTime StartDateTimeInRange => DateInicio;
        public DateTime EndDateTimeInRange => DateFin ?? DateInicio.AddHours(1);
        public DateTime AgendaDate => TipoEventoId == EventTypeEnum.Homework ? DateFin ?? DateInicio : DateInicio;
    }
}
