using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class Notificacion
    {
        public int NotificacionId { get; set; }
        public bool? EnviarPadres { get; set; }
        public bool? EnviarAlumnos { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string? Titulo { get; set; }
        public string? Mensaje { get; set; }
        public string? Vinculo { get; set; }
        public byte[]? Formato { get; set; }
        public int Estatus { get; set; }
        public int UsuarioId { get; set; }
        public int? TipoImagen { get; set; }
        public DateTime FechaModificacion { get; set; }

        public string FechaHora => $"{Fecha.ToString("ddd. dd MMM, yyyy")} {Hora.Hours:D2}:{Hora.Minutes:D2}";

        public string? NotificacionImageFormato {  get; set; }
    }
}
