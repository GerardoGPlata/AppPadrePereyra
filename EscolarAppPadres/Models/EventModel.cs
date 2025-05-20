namespace EscolarAppPadres.Models
{
   public class EventModel
    {
        public string? TipoEvento { get; set; }
        public string? Nivel { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFin { get; set; }
        public string? Imagen { get; set; }
        public string? ColorIndicador { get; set; }

        public string FechaHoraInicio => FechaInicio.HasValue && HoraInicio.HasValue
            ? $"{FechaInicio.Value.ToString("ddd. dd MMM, yyyy")} {HoraInicio.Value.Hours:D2}:{HoraInicio.Value.Minutes:D2}"
            : string.Empty;

        public string FechaHoraFin => FechaFin.HasValue && HoraFin.HasValue
            ? $"{FechaFin.Value.ToString("ddd. dd MMM, yyyy")} {HoraFin.Value.Hours:D2}:{HoraFin.Value.Minutes:D2}"
            : string.Empty;

        public string FechaHoraRango =>
            FechaInicio.HasValue && HoraInicio.HasValue && FechaFin.HasValue && HoraFin.HasValue
            ? $"{FechaInicio.Value:ddd. dd MMM, yyyy} {HoraInicio.Value.Hours:D2}:{HoraInicio.Value.Minutes:D2}, {FechaFin.Value:ddd. dd MMM, yyyy} {HoraFin.Value.Hours:D2}:{HoraFin.Value.Minutes:D2}"
            : string.Empty;
    }
}
