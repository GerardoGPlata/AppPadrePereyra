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

        // Propiedades calculadas para Scheduler AppointmentMapping
        public DateTime StartDateTime
        {
            get
            {
                var date = FechaInicio ?? DateTime.Today;
                var time = HoraInicio ?? new TimeSpan(8, 0, 0);
                return date.Date + time;
            }
        }

        public DateTime EndDateTime
        {
            get
            {
                var start = StartDateTime;
                var endDate = FechaFin ?? FechaInicio ?? start.Date;
                var endTime = HoraFin ?? TimeSpan.Zero;

                // Si no hay hora fin, usar 1 hora después del inicio
                var end = endTime == TimeSpan.Zero && (!FechaFin.HasValue || endDate.Date == start.Date)
                    ? start.AddHours(1)
                    : endDate.Date + (endTime == TimeSpan.Zero ? new TimeSpan(9, 0, 0) : endTime);

                if (end <= start)
                    end = start.AddHours(1);

                return end;
            }
        }

        public Brush BackgroundBrush
        {
            get
            {
                try
                {
                    var color = !string.IsNullOrWhiteSpace(ColorIndicador) ? Color.FromArgb(ColorIndicador) : Colors.LightGreen;
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return new SolidColorBrush(Colors.LightGreen);
                }
            }
        }

        public bool IsAllDay { get; set; } = false;
    }
}
