using Android.Content;
using Android.Provider;
using EscolarAppPadres.Platforms.Android;
using EscolarAppPadres.Services;
using EscolarAppPadres.Interface;
using Java.Util;
using AndroidApp = Android.App;
using AndroidNet = Android.Net;
using AndroidProvider = Android.Provider;
using EscolarAppPadres.Helpers;

[assembly: Dependency(typeof(AndroidCalendarService))]
namespace EscolarAppPadres.Platforms.Android
{
    public class AndroidCalendarService : ICalendarService
    {
        public async Task AddEventAsync(string Url, string Title, string Description, DateTime FechaHoraInicio, DateTime FechaHoraFin)
        {
            var readStatus = await Permissions.CheckStatusAsync<Permissions.CalendarRead>();
            var writeStatus = await Permissions.CheckStatusAsync<Permissions.CalendarWrite>();

            if (readStatus != PermissionStatus.Granted || writeStatus != PermissionStatus.Granted)
            {
                readStatus = await Permissions.RequestAsync<Permissions.CalendarRead>();
                writeStatus = await Permissions.RequestAsync<Permissions.CalendarWrite>();
            }

            if (readStatus == PermissionStatus.Granted && writeStatus == PermissionStatus.Granted)
            {
                try
                {
                    string fullDescription = string.IsNullOrEmpty(Url) ? Description : $"{Url}\n\n{Title}\n\n{Description}";

                    var intent = new Intent(Intent.ActionInsert);
                    intent.SetData(CalendarContract.Events.ContentUri);

                    intent.PutExtra(CalendarContract.Events.InterfaceConsts.Title, Title);
                    intent.PutExtra(CalendarContract.Events.InterfaceConsts.Description, fullDescription);
                    intent.PutExtra(CalendarContract.Events.InterfaceConsts.Dtstart, (long)(FechaHoraInicio - new DateTime(1970, 1, 1)).TotalMilliseconds);
                    intent.PutExtra(CalendarContract.Events.InterfaceConsts.Dtend, (long)(FechaHoraFin - new DateTime(1970, 1, 1)).TotalMilliseconds);
                    intent.PutExtra(CalendarContract.Events.InterfaceConsts.EventTimezone, Java.Util.TimeZone.Default!.ID);

                    intent.AddFlags(ActivityFlags.NewTask);
                    AndroidApp.Application.Context.StartActivity(intent);
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

        public async Task AddEventSilentlyAsync(string Url, string Title, string Description, DateTime FechaHoraInicio, DateTime FechaHoraFin)
        {
            var readStatus = await Permissions.CheckStatusAsync<Permissions.CalendarRead>();
            var writeStatus = await Permissions.CheckStatusAsync<Permissions.CalendarWrite>();

            if (readStatus != PermissionStatus.Granted || writeStatus != PermissionStatus.Granted)
            {
                readStatus = await Permissions.RequestAsync<Permissions.CalendarRead>();
                writeStatus = await Permissions.RequestAsync<Permissions.CalendarWrite>();
            }

            if (readStatus == PermissionStatus.Granted && writeStatus == PermissionStatus.Granted)
            {
                try
                {
                    long CalendarId = GetPrimaryCalendarId();

                    if (CalendarId == -1)
                    {
                        var errorMessage = "Error al agregar el evento al calendario. La URI resultante es nula.";
                        var errorStackTrace = "No se pudo obtener la URI del evento agregado.";

                        Console.WriteLine("=== ERROR DETECTADO ===");
                        Console.WriteLine(errorMessage);
                        Console.WriteLine(errorStackTrace);
                        Console.WriteLine("=======================");

                        await DialogsHelper2.ShowErrorMessage("No se encontró un calendario válido.");
                        return;
                    }

                    string fullDescription = string.IsNullOrEmpty(Url) ? Description : $"{Url}\n\n{Title}\n\n{Description}";

                    ContentValues eventValues = new ContentValues();
                    eventValues.Put(CalendarContract.Events.InterfaceConsts.CalendarId, CalendarId);
                    eventValues.Put(CalendarContract.Events.InterfaceConsts.Title, Title);
                    eventValues.Put(CalendarContract.Events.InterfaceConsts.Description, fullDescription);
                    eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtstart, (long)(FechaHoraInicio - new DateTime(1970, 1, 1)).TotalMilliseconds);
                    eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtend, (long)(FechaHoraFin - new DateTime(1970, 1, 1)).TotalMilliseconds);
                    eventValues.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, Java.Util.TimeZone.Default!.ID);

                    var uri = AndroidApp.Application.Context.ContentResolver!.Insert(CalendarContract.Events.ContentUri!, eventValues);

                    if (uri == null)
                    {
                        await DialogsHelper2.ShowErrorMessage("Fallo al guardar el evento. Por favor, intenta más tarde.");
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

        private long GetPrimaryCalendarId()
        {
            var uri = CalendarContract.Calendars.ContentUri;
            string[] projection = {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName,
                CalendarContract.Calendars.InterfaceConsts.CalendarAccessLevel,
                CalendarContract.Calendars.InterfaceConsts.IsPrimary
            };

            var cursor = AndroidApp.Application.Context.ContentResolver!.Query(uri!, projection, null, null, null);

            if (cursor == null || !cursor.MoveToFirst())
            {
                cursor?.Close();
                return -1;
            }

            long calendarId = -1;
            bool foundPrimary = false;

            do
            {
                var calId = cursor.GetLong(cursor.GetColumnIndex(projection[0]));
                var accessLevel = cursor.GetInt(cursor.GetColumnIndex(projection[2]));
                var isPrimary = cursor.GetInt(cursor.GetColumnIndex(projection[3])) == 1;

                if (accessLevel == 700)
                {
                    if (isPrimary)
                    {
                        calendarId = calId;
                        foundPrimary = true;
                        break;
                    }
                    else if (!foundPrimary)
                    {
                        calendarId = calId;
                    }
                }

            } while (cursor.MoveToNext());

            cursor.Close();

            if (calendarId == -1)
            {
                return -1;
            }

            return calendarId;
        }
    }
}