using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Openpay;
using EscolarAppPadres.Models.Payments;
using EscolarAppPadres.Services;
using EscolarAppPadres.Views.Service;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Windows.Input;

namespace EscolarAppPadres.ViewModels.Payments
{
    public class PendingPaymentsViewModel : INotifyPropertyChanged
    {
        private readonly PaymentsService _paymentsService;

        private ObservableCollection<PaymentItem> _pendingPayments;
        private bool _isRefreshing;
        private bool _sinResultados;
        private decimal _total;
        private bool _isPopupOpen;
        private ObservableCollection<PaymentItem> _selectedPayments;
        private string _popupHeader;
        private PendingPaymentsResponse _paymentsData;

        public ObservableCollection<PaymentItem> PendingPayments
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

        public ObservableCollection<PaymentItem> SelectedPayments
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
            PendingPayments = new ObservableCollection<PaymentItem>();
            SelectedPayments = new ObservableCollection<PaymentItem>();
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

            PopupHeader = $"DETALLE DE PAGO ({SelectedPayments.Count} elementos)";
            IsPopupOpen = true;
        }

        private async Task ProcessPaymentAsync()
        {
            if (SelectedPayments == null || !SelectedPayments.Any())
            {
                await DialogsHelper2.ShowErrorMessage("Selecciona al menos un pago.");
                return;
            }

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Token de autenticación no encontrado. Inicia sesión nuevamente.");
                    return;
                }

                // Agrupar pagos por tipo para determinar el tipoPago
                var tiposPago = DeterminarTiposPago(SelectedPayments.ToList());
                var tipoPago = tiposPago.Count == 1 ? tiposPago.First() : "MX"; // MX para mixto

                // Obtener IDs de documentos
                var documentosIds = new List<string>();
                foreach (var payment in SelectedPayments)
                {
                    if (payment.ColegiaturaOriginal != null)
                    {
                        documentosIds.Add(payment.ColegiaturaOriginal.DocumentoPorPagarId);
                    }
                    else if (payment.InscripcionOriginal != null)
                    {
                        documentosIds.Add(payment.InscripcionOriginal.DocumentoPorPagarId.ToString());
                    }
                    else if (payment.OtroDocumentoOriginal != null)
                    {
                        documentosIds.Add(payment.OtroDocumentoOriginal.DocumentoPorPagarId.ToString());
                    }
                }

                var documentoPorPagarId = string.Join("/", documentosIds);
                var totalImporte = SelectedPayments.Sum(p => p.ImporteCalculado);
                var conceptoGeneral = GenerarConceptoGeneral(SelectedPayments.ToList());

                var request = new CreateChargeMovilRequestDto
                {
                    DocumentoPorPagarId = documentoPorPagarId,
                    TipoPago = tipoPago,
                    Importe = totalImporte,
                    Concepto = conceptoGeneral
                };

                Console.WriteLine($"[DEBUG] Creando cargo con datos: {request.DocumentoPorPagarId}, {request.TipoPago}, {request.Importe}");

                var response = await _paymentsService.CreateChargeMovilAsync(request, token);

                // ← CORRECCIÓN: Acceder al primer elemento de la lista
                if (response?.Result == true && response.Data != null && response.Data.Any())
                {
                    var chargeData = response.Data.First(); // ← Obtener el primer elemento de la lista

                    if (!string.IsNullOrEmpty(chargeData.PaymentUrl)) // ← Usar chargeData en lugar de response.Data
                    {
                        // Guardar ID de transacción para verificación posterior
                        if (!string.IsNullOrEmpty(chargeData.TransactionId)) // ← Usar chargeData
                        {
                            Preferences.Set("LastTransactionId", chargeData.TransactionId);
                        }

                        IsPopupOpen = false;

                        // Navegar a la página de pago
                        var paymentPage = new PaymentWebViewPage(chargeData.PaymentUrl); // ← Usar chargeData
                        await Application.Current.MainPage.Navigation.PushAsync(paymentPage);
                    }
                    else
                    {
                        await DialogsHelper2.ShowErrorMessage("No se pudo obtener la URL de pago.");
                    }
                }
                else
                {
                    var errorMsg = response?.Message ?? "Error desconocido al crear el cargo.";
                    await DialogsHelper2.ShowErrorMessage($"Error: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en ProcessPaymentAsync: {ex.Message}");
                await DialogsHelper2.ShowErrorMessage($"Error inesperado: {ex.Message}");
            }
        }
        public async Task LoadPendingPaymentsAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;
                PendingPayments.Clear();

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Token de autenticación no encontrado.");
                    return;
                }

                var response = await _paymentsService.GetStudentPaymentsAsync(token);

                if (response?.Result == true && response.Data != null)
                {
                    _paymentsData = response.Data.FirstOrDefault() ?? new PendingPaymentsResponse();

                    // Convertir todos los DTOs a PaymentItem unificado
                    var paymentItems = new List<PaymentItem>();

                    // Agregar Colegiaturas
                    foreach (var colegiatura in _paymentsData.Colegiaturas)
                    {
                        var paymentItem = new PaymentItem
                        {
                            DocumentoPorPagarId = colegiatura.DocumentoPorPagarId,
                            Documento = colegiatura.Documento,
                            Concepto = colegiatura.Concepto,
                            Alumno = colegiatura.Alumno,
                            Matricula = colegiatura.Matricula,
                            Grado = colegiatura.Grado,
                            Grupo = colegiatura.Grupo ?? "",
                            ImporteCalculado = colegiatura.ImporteCalculado,
                            FechaLimiteFormato = colegiatura.FechaLimiteFormato,
                            TipoDocumentoTexto = colegiatura.TipoDocumentoTexto,
                            TipoDocumento = 2, // Colegiatura
                            ColegiaturaOriginal = colegiatura
                        };
                        paymentItem.PropertyChanged += Payment_PropertyChanged;
                        paymentItems.Add(paymentItem);
                    }

                    // Agregar Inscripciones
                    foreach (var inscripcion in _paymentsData.Inscripciones)
                    {
                        var paymentItem = new PaymentItem
                        {
                            DocumentoPorPagarId = inscripcion.DocumentoPorPagarId.ToString(),
                            Documento = inscripcion.Documento,
                            Concepto = inscripcion.Concepto,
                            Alumno = inscripcion.Alumno,
                            Matricula = inscripcion.Matricula,
                            Grado = inscripcion.Grado,
                            Grupo = inscripcion.Grupo ?? "",
                            ImporteCalculado = inscripcion.ImporteCalculado,
                            FechaLimiteFormato = inscripcion.FechaLimiteFormato,
                            TipoDocumentoTexto = inscripcion.TipoDocumentoTexto,
                            TipoDocumento = 1, // Inscripción
                            InscripcionOriginal = inscripcion
                        };
                        paymentItem.PropertyChanged += Payment_PropertyChanged;
                        paymentItems.Add(paymentItem);
                    }

                    // Agregar Otros Documentos
                    foreach (var otroDoc in _paymentsData.OtrosDocumentos)
                    {
                        var paymentItem = new PaymentItem
                        {
                            DocumentoPorPagarId = otroDoc.DocumentoPorPagarId.ToString(),
                            Documento = otroDoc.Documento,
                            Concepto = otroDoc.Concepto,
                            Alumno = otroDoc.Alumno,
                            Matricula = otroDoc.Matricula,
                            Grado = otroDoc.Grado,
                            Grupo = otroDoc.Grupo ?? "",
                            ImporteCalculado = otroDoc.ImporteCalculado,
                            FechaLimiteFormato = otroDoc.FechaLimiteFormato,
                            TipoDocumentoTexto = otroDoc.TipoDocumentoTexto,
                            TipoDocumento = 3, // Otros
                            OtroDocumentoOriginal = otroDoc
                        };
                        paymentItem.PropertyChanged += Payment_PropertyChanged;
                        paymentItems.Add(paymentItem);
                    }

                    // Ordenar por fecha límite
                    paymentItems = paymentItems.OrderBy(p =>
                        DateTime.TryParse(p.FechaLimiteFormato?.Replace("/", "-"), out var fecha) ? fecha : DateTime.MaxValue
                    ).ToList();

                    foreach (var item in paymentItems)
                    {
                        PendingPayments.Add(item);
                    }

                    SinResultados = !PendingPayments.Any();

                    Console.WriteLine($"[INFO] Pagos cargados - Total: {PendingPayments.Count}, Colegiaturas: {_paymentsData.Colegiaturas.Count}, Inscripciones: {_paymentsData.Inscripciones.Count}, Otros: {_paymentsData.OtrosDocumentos.Count}");
                }
                else
                {
                    SinResultados = true;
                    var errorMsg = response?.Message ?? "Error desconocido al cargar los pagos.";
                    Console.WriteLine($"[ERROR] {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                SinResultados = true;
                Console.WriteLine($"[ERROR] Error cargando pagos: {ex.Message}");
                await DialogsHelper2.ShowErrorMessage($"Error cargando pagos: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void Payment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PaymentItem.IsSelected))
            {
                CalcularTotal();
            }
        }

        private void CalcularTotal()
        {
            Total = PendingPayments?.Where(p => p.IsSelected).Sum(p => p.ImporteCalculado) ?? 0;
            CanPagar = Total > 0;
        }

        private List<string> DeterminarTiposPago(List<PaymentItem> payments)
        {
            var tipos = new HashSet<string>();

            foreach (var payment in payments)
            {
                switch (payment.TipoDocumento)
                {
                    case 1: // Inscripción
                    case 2: // Colegiatura
                        tipos.Add("1");
                        break;
                    case 3: // Otros
                        tipos.Add("3");
                        break;
                }
            }

            return tipos.ToList();
        }

        private string GenerarConceptoGeneral(List<PaymentItem> payments)
        {
            var conceptos = payments.Select(p => p.Concepto).Distinct().Take(3);
            var conceptoGeneral = string.Join(", ", conceptos);

            if (payments.Count > 3)
            {
                conceptoGeneral += $" y {payments.Count - 3} más";
            }

            return conceptoGeneral;
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