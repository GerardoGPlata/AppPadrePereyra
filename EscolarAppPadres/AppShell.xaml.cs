using EscolarAppPadres.Views.News;
using EscolarAppPadres.Views.School;
using EscolarAppPadres.Views.Account;
using EscolarAppPadres.Views.Calendar;

namespace EscolarAppPadres
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Rutas de Account
            Routing.RegisterRoute("PrivacyView", typeof(PrivacyView));
            Routing.RegisterRoute("ChangePasswordView", typeof(ChangePasswordView));

            // Rutas de News
            Routing.RegisterRoute("VinculoView", typeof(VinculoView));
        }
    }
}
