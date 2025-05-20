using Mopups.Services;
using EscolarAppPadres.Models;
using EscolarAppPadres.ViewModels.News;

namespace EscolarAppPadres.Views.News;

public partial class VinculoView : ContentPage
{
    private readonly NewsViewModel _newsViewModel;
    private readonly NotificacionesLeidas _notificacionesLeidas;

    public VinculoView(NewsViewModel newsViewModel, NotificacionesLeidas notificacionesLeidas)
    {
        InitializeComponent();
        _newsViewModel = newsViewModel;
        _notificacionesLeidas = notificacionesLeidas;
        BindingContext = _newsViewModel;

        myWebView.Source = _notificacionesLeidas?.Notificacion?.Vinculo;
        Title = _notificacionesLeidas?.Notificacion?.Titulo;
    }

    private void OpenFilterCalendarReminderVinculoButtonClicked(object sender, EventArgs e)
    {
        _newsViewModel.OpenFilterCalendarReminderVinculoCommand.Execute(_notificacionesLeidas);
    }
}