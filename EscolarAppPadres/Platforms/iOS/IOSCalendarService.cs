using EscolarAppPadres.Interface;
using EventKit;
using Foundation;
using UIKit;
using EscolarAppPadres.Platforms.iOS;
using EventKitUI;
using EscolarAppPadres.Helpers;

[assembly: Dependency(typeof(IOSCalendarService))]
namespace EscolarAppPadres.Platforms.iOS
{
    public class IOSCalendarService : ICalendarService
    {
        private EKEventStore eventStore;

        public IOSCalendarService()
        {
            eventStore = new EKEventStore();
        }

        public async Task AddEventAsync(string Url, string Title, string Description, DateTime FechaHoraInicio, DateTime FechaHoraFin)
        {
            var (status, error) = await eventStore.RequestAccessAsync(EKEntityType.Event);

            if (status)
            {
                try
                {
                    string fullDescription = string.IsNullOrEmpty(Url) ? Description : $"{Url}\n\n{Title}\n\n{Description}";

                    EKEvent newEvent = EKEvent.FromStore(eventStore);
                    newEvent.Title = Title;
                    newEvent.Notes = fullDescription;
                    newEvent.StartDate = (NSDate)FechaHoraInicio;
                    newEvent.EndDate = (NSDate)FechaHoraFin;
                    newEvent.Calendar = eventStore.DefaultCalendarForNewEvents;

                    var eventController = new EKEventEditViewController
                    {
                        Event = newEvent,
                        EventStore = eventStore
                    };

                    eventController.Completed += async (sender, e) =>
                    {
                        if (e.Action == EKEventEditViewAction.Saved)
                        {
                            NSError err;
                            eventStore.SaveEvent(newEvent, EKSpan.ThisEvent, out err);

                            if (err != null)
                            {
                                var errorMessage = $"Error al guardar el evento LocalizedDescription: {err.LocalizedDescription}";
                                var errorStackTrace = $"Error al guardar el evento UserInfo: {err.UserInfo}";

                                Console.WriteLine("=== ERROR DETECTADO ===");
                                Console.WriteLine(errorMessage);
                                Console.WriteLine(errorStackTrace);
                                Console.WriteLine("=============================================");

                                await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema al guardar el evento. Por favor, intenta más tarde.");
                            }
                        }
                        eventController.DismissViewController(true, null);
                    };

                    var viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
                    viewController.PresentViewController(eventController, true, null);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                    var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";

                    Console.WriteLine("=== ERROR DETECTADO ===");
                    Console.WriteLine(errorMessage);
                    Console.WriteLine(errorStackTrace);
                    Console.WriteLine("=======================");

                    await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema. Por favor, intenta nuevamente más tarde.");
                }
            }
            else
            {
                var result = await DialogsHelper.ShowConfirmationMessage("Permisos Denegados",
                    "No se concedieron permisos para acceder al calendario.\n\n" +
                    "¿Deseas ir a la configuración de la aplicación para habilitarlos?");

                if(result)
                {
                    AppInfo.ShowSettingsUI();
                }
            }

            await Task.CompletedTask;
        }

        public async Task AddEventSilentlyAsync(string Url, string Title, string Description, DateTime FechaHoraInicio, DateTime FechaHoraFin)
        {
            var (status, errors) = await eventStore.RequestAccessAsync(EKEntityType.Event);

            if (status)
            {
                try
                {
                    string fullDescription = string.IsNullOrEmpty(Url) ? Description : $"{Url}\n\n{Title}\n\n{Description}";

                    EKEvent newEvent = EKEvent.FromStore(eventStore);
                    newEvent.Title = Title;
                    newEvent.Notes = fullDescription;
                    newEvent.StartDate = (NSDate)FechaHoraInicio;
                    newEvent.EndDate = (NSDate)FechaHoraFin;
                    newEvent.Calendar = eventStore.DefaultCalendarForNewEvents;

                    NSError error;
                    eventStore.SaveEvent(newEvent, EKSpan.ThisEvent, out error);

                    if (error != null)
                    {
                        var errorMessage = $"Error al guardar el evento LocalizedDescription: {error.LocalizedDescription}";
                        var errorStackTrace = $"Error al guardar el evento UserInfo: {error.UserInfo}";

                        Console.WriteLine("=== ERROR DETECTADO ===");
                        Console.WriteLine(errorMessage);
                        Console.WriteLine(errorStackTrace);
                        Console.WriteLine("=============================================");

                        await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema al guardar el evento. Por favor, intenta nuevamente más tarde.");
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                    var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";

                    Console.WriteLine("=== ERROR DETECTADO ===");
                    Console.WriteLine(errorMessage);
                    Console.WriteLine(errorStackTrace);
                    Console.WriteLine("=======================");

                    await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema. Por favor, intenta nuevamente más tarde.");
                }
            }
            else
            {
                var result = await DialogsHelper.ShowConfirmationMessage("Permisos Denegados",
                    "No se concedieron permisos para acceder al calendario.\n\n" +
                    "¿Deseas ir a la configuración de la aplicación para habilitarlos?");

                if (result)
                {
                    AppInfo.ShowSettingsUI();
                }
            }

            await Task.CompletedTask;
        }
    }
}