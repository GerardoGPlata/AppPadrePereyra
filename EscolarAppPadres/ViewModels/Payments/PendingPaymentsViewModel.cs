using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Openpay;
using EscolarAppPadres.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EscolarAppPadres.ViewModels.Payments
{
    public class PendingPaymentsViewModel : INotifyPropertyChanged
    {
        private readonly PaymentsService _paymentsService;

        private ObservableCollection<PendingPayment> _pendingPayments;
        private bool _isRefreshing;
        private bool _sinResultados;
        private decimal _total;
        private bool _isPopupOpen;
        private ObservableCollection<PendingPayment> _selectedPayments;
        private string _popupHeader;

        public ObservableCollection<PendingPayment> PendingPayments
        {
            get => _pendingPayments;
            set
            {
                _pendingPayments = value;
                OnPropertyChanged(nameof(PendingPayments));
            }
        }

        public decimal Total
        {
            get => _total;
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public bool SinResultados
        {
            get => _sinResultados;
            set
            {
                if (_sinResultados != value)
                {
                    _sinResultados = value;
                    OnPropertyChanged(nameof(SinResultados));
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        private bool _canPagar;
        public bool CanPagar
        {
            get => _canPagar;
            set
            {
                if (_canPagar != value)
                {
                    _canPagar = value;
                    OnPropertyChanged(nameof(CanPagar));
                }
            }
        }

        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                if (_isPopupOpen != value)
                {
                    _isPopupOpen = value;
                    OnPropertyChanged(nameof(IsPopupOpen));
                }
            }
        }

        public ObservableCollection<PendingPayment> SelectedPayments
        {
            get => _selectedPayments;
            set
            {
                _selectedPayments = value;
                OnPropertyChanged(nameof(SelectedPayments));
            }
        }

        public string PopupHeader
        {
            get => _popupHeader;
            set
            {
                _popupHeader = value;
                OnPropertyChanged(nameof(PopupHeader));
            }
        }

        public ICommand LoadPaymentsCommand { get; }
        public ICommand PagarCommand { get; }
        public ICommand ConfirmPaymentCommand { get; }
        public ICommand ClosePopupCommand { get; }



        public PendingPaymentsViewModel()
        {
            _paymentsService = new PaymentsService();
            PendingPayments = new ObservableCollection<PendingPayment>();
            SelectedPayments = new ObservableCollection<PendingPayment>();
            LoadPaymentsCommand = new Command(async () => await LoadPendingPaymentsAsync());
            PagarCommand = new Command(ShowPaymentPopup);
            ConfirmPaymentCommand = new Command(async () => await ProcessPaymentAsync());
            ClosePopupCommand = new Command(() => IsPopupOpen = false);
            PopupHeader = "DETALLE DE PAGO";
        }

        private void ShowPaymentPopup()
        {
            SelectedPayments.Clear();

            foreach (var payment in PendingPayments.Where(p => p.IsSelected))
            {
                SelectedPayments.Add(payment);
            }

            IsPopupOpen = true;
        }

        private async Task ProcessPaymentAsync()
        {
            if (SelectedPayments == null || !SelectedPayments.Any())
            {
                await DialogsHelper2.ShowErrorMessage("Selecciona al menos un pago.");
                return;
            }

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                await DialogsHelper2.ShowErrorMessage("Sesión expirada. Por favor inicie sesión nuevamente.");
                return;
            }

            // Datos fijos o puedes obtenerlos de un formulario
            var name = "Juan";
            var lastName = "Vazquez Juarez";
            var email = "juan.vazquez@empresa.com.mx";
            var phoneNumber = "4423456723";
            var total = SelectedPayments.Sum(p => p.ImporteCalculado);
            var description = $"Pago escolar de {SelectedPayments.Count} documento(s)";
            var orderId = $"oid-{DateTime.Now:yyyyMMddHHmmss}";
            var redirectUrl = "http://www.openpay.mx/index.html";

            var simpleRequest = new OpenpaySimpleChargeRequestDto
            {
                Name = name,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber,
                Amount = total,
                Description = description,
                OrderId = orderId,
                RedirectUrl = redirectUrl
            };

            IsPopupOpen = false;

            var response = await _paymentsService.CreateOpenpaySimpleChargeAsync(simpleRequest, token);

            if (response?.Result == true && response.Data != null && response.Data.Any())
            {
                var charge = response.Data.First();
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(charge));
                await DialogsHelper2.ShowSuccessMessage("Pago iniciado correctamente.");

                if (charge.PaymentMethod?.Url != null)
                {
                    await Shell.Current.Navigation.PushAsync(
                        new EscolarAppPadres.Views.Service.PaymentWebViewPage(charge.PaymentMethod.Url)
                    );
                }
            }
            else
            {
                await DialogsHelper2.ShowErrorMessage(response?.Message ?? "No se pudo iniciar el pago.");
            }
        }

        public async Task LoadPendingPaymentsAsync()
        {
            IsRefreshing = true;
            SinResultados = false;

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var padreId = await SecureStorage.GetAsync("Usuario_Id");

                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Sesión expirada. Por favor inicie sesión nuevamente.");
                    return;
                }

                if (!long.TryParse(padreId, out _))
                {
                    await DialogsHelper2.ShowErrorMessage("ID de usuario inválido.");
                    return;
                }

                var response = await _paymentsService.GetStudentPaymentsAsync(token);

                if (response?.Data != null && response.Data.Any())
                {
                    PendingPayments.Clear();

                    foreach (var payment in response.Data)
                    {
                        payment.PropertyChanged += Payment_PropertyChanged;
                        PendingPayments.Add(payment);
                    }

                    SinResultados = PendingPayments.Count == 0;
                }
                else
                {
                    SinResultados = true;
                }

                CalcularTotal(); // actualiza el total por si ya vienen seleccionados
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                await DialogsHelper2.ShowErrorMessage("No se pudo conectar al servidor. Verifique su conexión a Internet.");
            }
            catch (TaskCanceledException)
            {
                await DialogsHelper2.ShowErrorMessage("La solicitud ha expirado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                await DialogsHelper2.ShowErrorMessage("Ocurrió un error inesperado: " + ex.Message);
            }
            finally
            {
                IsRefreshing = false;
                OnPropertyChanged(nameof(PendingPayments));
            }
        }

        private void Payment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PendingPayment.IsSelected))
                CalcularTotal();
        }

        private void CalcularTotal()
        {
            Total = PendingPayments
                .Where(p => p.IsSelected)
                .Sum(p => p.ImporteCalculado);

            CanPagar = Total > 0;
        }

        public async Task InitializeAsync()
        {
            await LoadPendingPaymentsAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}