using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class NotificacionesLeidas
    {
        public int NotificacionLeidaId { get; set; }
        public int NotificacionDestinatarioId { get; set; }
        public int? Id { get; set; }
        public bool? Leido { get; set; }
        public bool? Hecho { get; set; }
        public int? Tipo { get; set; }
        public int? NotificacionId { get; set; }
        public int? AlumnoId { get; set; }

        public Notificacion? Notificacion { get; set; }
        public string TextoLeido => Leido == true ? "LEÍDO" : "NO LEÍDO";
        public string EstadoImagen => Leido == true ? "notifications_active_outline.svg" : "notifications_off_outline.svg";
    }
}
