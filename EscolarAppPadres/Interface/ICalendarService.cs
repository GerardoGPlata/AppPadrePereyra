using System;
using System.Threading.Tasks;

namespace EscolarAppPadres.Interface
{
    public interface ICalendarService
    {
        Task AddEventAsync(string Url, string Title, string Description, DateTime FechaHoraInicio, DateTime FechaHoraFin);

        Task AddEventSilentlyAsync(string Url, string Title, string Description, DateTime FechaHoraInicio, DateTime FechaHoraFin);
    }
}
