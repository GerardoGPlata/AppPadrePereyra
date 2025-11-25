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
using System.Threading; // ← Agregar esta referencia

namespace EscolarAppPadres.ViewModels.Payments
{
    public class PendingPaymentsViewModel : INotifyPropertyChanged
    {
        private readonly PaymentsService _paymentsService;
        private readonly SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1, 1); // ← Agregar semáforo

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

        public decimal SubTotal
        {
            get
            {
                return SelectedPayments?.Sum(p => p.ImporteBase) ?? 0;
            }
        }

        public decimal TotalDescuento
        {
            get
            {
                return SelectedPayments?.Sum(p => p.DescuentoDoc) ?? 0;
            }
        }

        public decimal TotalRecargos
        {
            get
            {
                return SelectedPayments?.Sum(p => p.InteresDoc) ?? 0;
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
        public ICommand ShowFilterCommand { get; }
        public ICommand CloseFilterPopupCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand AcceptOverdueSelectionCommand { get; }

        public PendingPaymentsViewModel()
        {
            _paymentsService = new PaymentsService();
            PendingPayments = new ObservableCollection<PaymentItem>();
            SelectedPayments = new ObservableCollection<PaymentItem>();
            LoadPaymentsCommand = new Command(async () => await RefreshPaymentsAsync());
            PagarCommand = new Command(ShowPaymentPopup);
            ConfirmPaymentCommand = new Command(async () => await ProcessPaymentAsync());
            ClosePopupCommand = new Command(() => IsPopupOpen = false);
            ShowFilterCommand = new Command(ShowFilter);
            CloseFilterPopupCommand = new Command(CloseFilterPopup);
            ApplyFilterCommand = new Command(ApplyFilter);
            AcceptOverdueSelectionCommand = new Command(AcceptOverdueSelection);
            PopupHeader = "DETALLE DE PAGO";
            _displayTitle = "Pagos Pendientes";
        }

        private string _displayTitle;
        public string DisplayTitle
        {
            get => _displayTitle;
            set
            {
                if (_displayTitle != value)
                {
                    _displayTitle = value;
                    OnPropertyChanged(nameof(DisplayTitle));
                }
            }
        }

        private void ShowFilter()
        {
            IsFilterPopupOpen = true;
        }

        private bool _isFilterPopupOpen;
        public bool IsFilterPopupOpen
        {
            get => _isFilterPopupOpen;
            set { _isFilterPopupOpen = value; OnPropertyChanged(nameof(IsFilterPopupOpen)); }
        }

        private bool _isPendingFilter = true; // default seleccionado
        public bool IsPendingFilter
        {
            get => _isPendingFilter;
            set
            {
                if (_isPendingFilter == value) return;
                _isPendingFilter = value;

                if (value)
                {
                    // Desactivar el otro
                    if (_isPaidFilter)
                    {
                        _isPaidFilter = false;
                        OnPropertyChanged(nameof(IsPaidFilter));
                    }
                    DisplayTitle = "Pagos Pendientes";
                }
                else
                {
                    // Garantizar que al menos uno esté activo
                    if (!_isPaidFilter)
                    {
                        _isPaidFilter = true;
                        OnPropertyChanged(nameof(IsPaidFilter));
                        DisplayTitle = "Pagos Realizados";
                    }
                }
                OnPropertyChanged(nameof(IsPendingFilter));
            }
        }

        private bool _isPaidFilter; // inicia false
        public bool IsPaidFilter
        {
            get => _isPaidFilter;
            set
            {
                if (_isPaidFilter == value) return;
                _isPaidFilter = value;

                if (value)
                {
                    if (_isPendingFilter)
                    {
                        _isPendingFilter = false;
                        OnPropertyChanged(nameof(IsPendingFilter));
                    }
                    DisplayTitle = "Pagos Realizados";
                }
                else
                {
                    if (!_isPendingFilter)
                    {
                        _isPendingFilter = true;
                        OnPropertyChanged(nameof(IsPendingFilter));
                        DisplayTitle = "Pagos Pendientes";
                    }
                }
                OnPropertyChanged(nameof(IsPaidFilter));
            }
        }

        private async void ApplyFilter()
        {
            IsFilterPopupOpen = false;
            if (IsPaidFilter)
            {
                DisplayTitle = "Pagos Realizados";
                await LoadPaidPaymentsAsync();
            }
            else
            {
                DisplayTitle = "Pagos Pendientes";
                await LoadPendingPaymentsAsync();
            }
        }

        private async Task RefreshPaymentsAsync()
        {
            if (IsPaidFilter)
                await LoadPaidPaymentsAsync();
            else
                await LoadPendingPaymentsAsync();
        }

        // Método público para uso desde la vista al aparecer
        public Task RefreshCurrentAsync() => RefreshPaymentsAsync();

        private void CloseFilterPopup()
        {
            IsFilterPopupOpen = false;
        }

        private async Task LoadPaidPaymentsAsync()
        {
            if (!await _loadingSemaphore.WaitAsync(100))
                return;
            try
            {
                IsRefreshing = true;
                SinResultados = false;
                PendingPayments.Clear();
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Token de autenticación no encontrado.");
                    return;
                }
                var response = await _paymentsService.GetPaidPaymentsAsync(token);
                if (response?.Result == true && response.Data != null)
                {
                    _paymentsData = response.Data.FirstOrDefault() ?? new PendingPaymentsResponse();
                    var paymentItems = new List<PaymentItem>();
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
                            TipoDocumento = 2,
                            ColegiaturaOriginal = colegiatura,
                            IsPaid = true,
                            IsSelected = false
                        };
                        paymentItem.PropertyChanged += Payment_PropertyChanged;
                        paymentItems.Add(paymentItem);
                    }
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
                            TipoDocumento = 1,
                            InscripcionOriginal = inscripcion,
                            IsPaid = true,
                            IsSelected = false
                        };
                        paymentItem.PropertyChanged += Payment_PropertyChanged;
                        paymentItems.Add(paymentItem);
                    }
                    foreach (var otroDocumento in _paymentsData.OtrosDocumentos)
                    {
                        var paymentItem = new PaymentItem
                        {
                            DocumentoPorPagarId = otroDocumento.DocumentoPorPagarId.ToString(),
                            Documento = otroDocumento.Documento,
                            Concepto = otroDocumento.Concepto,
                            Alumno = otroDocumento.Alumno,
                            Matricula = otroDocumento.Matricula,
                            Grado = otroDocumento.Grado,
                            Grupo = otroDocumento.Grupo ?? "",
                            ImporteCalculado = otroDocumento.ImporteCalculado,
                            FechaLimiteFormato = otroDocumento.FechaLimiteFormato,
                            TipoDocumentoTexto = otroDocumento.TipoDocumentoTexto,
                            TipoDocumento = 3,
                            OtroDocumentoOriginal = otroDocumento,
                            IsPaid = true,
                            IsSelected = false
                        };
                        paymentItem.PropertyChanged += Payment_PropertyChanged;
                        paymentItems.Add(paymentItem);
                    }
                    foreach (var item in paymentItems)
                        PendingPayments.Add(item);
                    SinResultados = !PendingPayments.Any();
                    CalcularTotal();
                }
                else
                {
                    SinResultados = true;
                }
            }
            catch (Exception ex)
            {
                await DialogsHelper2.ShowErrorMessage($"Error al cargar pagos completados: {ex.Message}");
                SinResultados = true;
            }
            finally
            {
                IsRefreshing = false;
                OnPropertyChanged(nameof(PendingPayments));
                _loadingSemaphore.Release();
            }
        }

        private void ShowPaymentPopup()
        {
            SelectedPayments.Clear();

            foreach (var payment in PendingPayments.Where(p => p.IsSelected))
            {
                SelectedPayments.Add(payment);
            }

            PopupHeader = $"DETALLE DE PAGO";
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

                Console.WriteLine($"[DEBUG] === INICIANDO PROCESO DE PAGO ===");
                Console.WriteLine($"[DEBUG] Número de pagos seleccionados: {SelectedPayments.Count}");

                // Debug detallado de cada pago seleccionado
                for (int i = 0; i < SelectedPayments.Count; i++)
                {
                    var payment = SelectedPayments[i];
                    Console.WriteLine($"[DEBUG] Pago #{i + 1}:");
                    Console.WriteLine($"  - Concepto: {payment.Concepto}");
                    Console.WriteLine($"  - Alumno: {payment.Alumno}");
                    Console.WriteLine($"  - Tipo Documento: {payment.TipoDocumento} ({payment.TipoDocumentoTexto})");
                    Console.WriteLine($"  - Importe: {payment.ImporteCalculado:C2}");
                }

                // Agrupar pagos por tipo para determinar el tipoPago
                var tiposPago = DeterminarTiposPago(SelectedPayments.ToList());
                var tipoPago = tiposPago.Count == 1 ? tiposPago.First() : "MX"; // MX para mixto

                Console.WriteLine($"[DEBUG] Tipos de pago detectados: [{string.Join(", ", tiposPago)}]");
                Console.WriteLine($"[DEBUG] Tipo de pago final: {tipoPago}");

                // Obtener IDs de documentos con debug detallado
                var documentosIds = new List<string>();
                Console.WriteLine($"[DEBUG] === OBTENIENDO IDs DE DOCUMENTOS ===");

                foreach (var payment in SelectedPayments)
                {
                    string? documentoId = null;

                    if (payment.ColegiaturaOriginal != null)
                    {
                        documentoId = payment.ColegiaturaOriginal.DocumentoPorPagarId;
                        documentosIds.Add(documentoId);
                        Console.WriteLine($"[DEBUG] Colegiatura - ID: {documentoId}, Concepto: {payment.Concepto}");
                    }
                    else if (payment.InscripcionOriginal != null)
                    {
                        documentoId = payment.InscripcionOriginal.DocumentoPorPagarId.ToString();
                        documentosIds.Add(documentoId);
                        Console.WriteLine($"[DEBUG] Inscripción - ID: {documentoId}, Concepto: {payment.Concepto}");
                    }
                    else if (payment.OtroDocumentoOriginal != null)
                    {
                        documentoId = payment.OtroDocumentoOriginal.DocumentoPorPagarId.ToString();
                        documentosIds.Add(documentoId);
                        Console.WriteLine($"[DEBUG] Otro Documento - ID: {documentoId}, Concepto: {payment.Concepto}");
                    }
                    else
                    {
                        Console.WriteLine($"[WARNING] Pago sin documento original válido: {payment.Concepto}");
                    }
                }

                var documentoPorPagarId = string.Join("/", documentosIds);
                Console.WriteLine($"[DEBUG] IDs de documentos concatenados: {documentoPorPagarId}");

                var totalImporte = SelectedPayments.Sum(p => p.ImporteCalculado);
                Console.WriteLine($"[DEBUG] Importe total calculado: {totalImporte:C2}");

                // Generar concepto con debug mejorado
                var conceptoGeneral = GenerarConceptoGeneral(SelectedPayments.ToList());
                Console.WriteLine($"[DEBUG] === CONCEPTOS SELECCIONADOS ===");
                var todosLosConceptos = SelectedPayments.Select(p => p.Concepto).ToList();
                for (int i = 0; i < todosLosConceptos.Count; i++)
                {
                    Console.WriteLine($"[DEBUG] Concepto #{i + 1}: {todosLosConceptos[i]}");
                }
                Console.WriteLine($"[DEBUG] Concepto general generado: {conceptoGeneral}");

                var request = new CreateChargeMovilRequestDto
                {
                    DocumentoPorPagarId = documentoPorPagarId,
                    TipoPago = tipoPago,
                    Importe = totalImporte,
                    Concepto = conceptoGeneral
                };

                Console.WriteLine($"[DEBUG] === DATOS FINALES DEL REQUEST ===");
                Console.WriteLine($"[DEBUG] DocumentoPorPagarId: {request.DocumentoPorPagarId}");
                Console.WriteLine($"[DEBUG] TipoPago: {request.TipoPago}");
                Console.WriteLine($"[DEBUG] Importe: {request.Importe:C2}");
                Console.WriteLine($"[DEBUG] Concepto: {request.Concepto}");
                Console.WriteLine($"[DEBUG] === ENVIANDO REQUEST A OPENPAY ===");

                var response = await _paymentsService.CreateChargeMovilAsync(request, token);

                if (response?.Result == true && response.Data != null && response.Data.Any())
                {
                    var chargeData = response.Data.First();
                    Console.WriteLine($"[DEBUG] Cargo creado exitosamente - TransactionId: {chargeData.TransactionId}");

                    if (!string.IsNullOrEmpty(chargeData.PaymentUrl))
                    {
                        // Guardar ID de transacción para verificación posterior
                        if (!string.IsNullOrEmpty(chargeData.TransactionId))
                        {
                            Preferences.Set("LastTransactionId", chargeData.TransactionId);
                            Console.WriteLine($"[DEBUG] TransactionId guardado: {chargeData.TransactionId}");
                        }

                        IsPopupOpen = false;
                        Console.WriteLine($"[DEBUG] Navegando a URL de pago: {chargeData.PaymentUrl}");

                        // Navegar a la página de pago
                        var paymentPage = new PaymentWebViewPage(chargeData.PaymentUrl);
                        await Application.Current.MainPage.Navigation.PushAsync(paymentPage);
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] URL de pago vacía en respuesta");
                        await DialogsHelper2.ShowErrorMessage("No se pudo obtener la URL de pago.");
                    }
                }
                else
                {
                    var errorMsg = response?.Message ?? "Error desconocido al crear el cargo.";
                    Console.WriteLine($"[ERROR] Error en respuesta de CreateChargeMovil: {errorMsg}");
                    await DialogsHelper2.ShowErrorMessage($"Error: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en ProcessPaymentAsync: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                await DialogsHelper2.ShowErrorMessage($"Error inesperado: {ex.Message}");
            }
        }

        private string GenerarConceptoGeneral(List<PaymentItem> payments)
        {
            // Obtener todos los conceptos únicos
            var conceptosUnicos = payments.Select(p => p.Concepto).Distinct().ToList();

            Console.WriteLine($"[DEBUG] === GENERANDO CONCEPTO GENERAL ===");
            Console.WriteLine($"[DEBUG] Conceptos únicos encontrados: {conceptosUnicos.Count}");

            foreach (var concepto in conceptosUnicos)
            {
                Console.WriteLine($"[DEBUG] - {concepto}");
            }

            // Si son 3 o menos conceptos, mostrarlos todos
            if (conceptosUnicos.Count <= 3)
            {
                var conceptoFinal = string.Join(", ", conceptosUnicos);
                Console.WriteLine($"[DEBUG] Concepto final (≤3): {conceptoFinal}");
                return conceptoFinal;
            }
            else
            {
                // Si son más de 3, mostrar los primeros 3 y agregar "y X más"
                var primerosTres = conceptosUnicos.Take(3);
                var conceptoFinal = string.Join(", ", primerosTres) + $" y {conceptosUnicos.Count - 3} más";
                Console.WriteLine($"[DEBUG] Concepto final (>3): {conceptoFinal}");
                return conceptoFinal;
            }
        }
        public async Task LoadPendingPaymentsAsync()
        {
            // Usar semáforo para evitar ejecuciones simultáneas
            if (!await _loadingSemaphore.WaitAsync(100)) // Esperar máximo 100ms
            {
                Console.WriteLine("[DEBUG] LoadPendingPaymentsAsync ya se está ejecutando, saltando...");
                return;
            }

            try
            {
                Console.WriteLine("[DEBUG] Iniciando LoadPendingPaymentsAsync");
                IsRefreshing = true;
                SinResultados = false;

                // Limpiar la colección para evitar duplicados
                PendingPayments.Clear();

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Token de autenticación no encontrado.");
                    return;
                }

                Console.WriteLine("[DEBUG] Haciendo petición al servidor...");
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
                    foreach (var otroDocumento in _paymentsData.OtrosDocumentos)
                    {
                        var paymentItem = new PaymentItem
                        {
                            DocumentoPorPagarId = otroDocumento.DocumentoPorPagarId.ToString(),
                            Documento = otroDocumento.Documento,
                            Concepto = otroDocumento.Concepto,
                            Alumno = otroDocumento.Alumno,
                            Matricula = otroDocumento.Matricula,
                            Grado = otroDocumento.Grado,
                            Grupo = otroDocumento.Grupo ?? "",
                            ImporteCalculado = otroDocumento.ImporteCalculado,
                            FechaLimiteFormato = otroDocumento.FechaLimiteFormato,
                            TipoDocumentoTexto = otroDocumento.TipoDocumentoTexto,
                            TipoDocumento = 3, // Otro Documento
                            OtroDocumentoOriginal = otroDocumento
                        };
                        paymentItem.PropertyChanged += Payment_PropertyChanged;
                        paymentItems.Add(paymentItem);
                    }

                    Console.WriteLine($"[DEBUG] Agregando {paymentItems.Count} elementos a la colección");

                    // Agregar todos los items a la colección de una vez
                    foreach (var item in paymentItems)
                    {
                        PendingPayments.Add(item);
                    }

                    // Actualizar estado
                    SinResultados = !PendingPayments.Any();
                    // UpdateSelectableStatus(); // Ya no se usa esta estrategia
                    CalcularTotal();

                    Console.WriteLine($"[DEBUG] Total de elementos en PendingPayments: {PendingPayments.Count}");
                }
                else
                {
                    SinResultados = true;
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[ERROR] Error de red en LoadPendingPaymentsAsync: {httpEx.Message}");
                await DialogsHelper2.ShowErrorMessage("No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo.");
                SinResultados = true;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[ERROR] Timeout en LoadPendingPaymentsAsync");
                await DialogsHelper2.ShowErrorMessage("La solicitud ha expirado. Intente nuevamente.");
                SinResultados = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error inesperado en LoadPendingPaymentsAsync: {ex.Message}");
                await DialogsHelper2.ShowErrorMessage($"Error al cargar pagos: {ex.Message}");
                SinResultados = true;
            }
            finally
            {
                IsRefreshing = false;
                OnPropertyChanged(nameof(PendingPayments));
                _loadingSemaphore.Release(); // ← Liberar el semáforo
                Console.WriteLine("[DEBUG] LoadPendingPaymentsAsync completado");
            }
        }

        private void Payment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PaymentItem.IsSelected))
            {
                if (sender is PaymentItem item)
                {
                    if (item.IsSelected)
                    {
                        // Obtener todos los pagos pendientes del alumno ordenados por fecha
                        // Asumimos que PendingPayments ya contiene solo pagos pendientes o filtramos por !IsPaid si es necesario
                        // Pero la lista se llama PendingPayments, así que asumimos que son pendientes.
                        // Sin embargo, para estar seguros usamos la fecha límite.
                        var studentPayments = PendingPayments
                            .Where(p => p.Matricula == item.Matricula)
                            .OrderBy(p => p.FechaLimite)
                            .ToList();

                        var oldestPayment = studentPayments.FirstOrDefault();

                        // Si el pago seleccionado NO es el más antiguo
                        if (oldestPayment != null && item.DocumentoPorPagarId != oldestPayment.DocumentoPorPagarId)
                        {
                            // Prevenir selección
                            item.IsSelected = false;

                            // Identificar pagos anteriores al seleccionado (que deberían pagarse antes)
                            var previousPayments = studentPayments
                                .Where(p => p.FechaLimite < item.FechaLimite)
                                .ToList();

                            // Configurar popup
                            OverdueStudentName = item.Alumno;
                            OverduePaymentsList = new ObservableCollection<PaymentItem>(previousPayments);
                            _oldestPaymentToSelect = oldestPayment;
                            OverdueWarningMessage = $"Se seleccionará el adeudo más antiguo: {oldestPayment.Concepto}";

                            IsOverdueWarningPopupOpen = true;
                            return; // Detener cálculo
                        }
                    }
                }
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

        public async Task InitializeAsync()
        {
            await LoadPendingPaymentsAsync();
        }

        private void UpdateSelectableStatus()
        {
            if (PendingPayments == null) return;

            var groupedByStudent = PendingPayments.GroupBy(p => p.Matricula);

            foreach (var group in groupedByStudent)
            {
                // Verificar si el estudiante tiene algún pago vencido
                bool hasOverdue = group.Any(p => p.EsFechaVencida);

                foreach (var payment in group)
                {
                    if (hasOverdue)
                    {
                        // Si tiene vencidos, solo puede seleccionar los vencidos
                        payment.IsSelectable = payment.EsFechaVencida;

                        // Si un pago no vencido estaba seleccionado, deseleccionarlo
                        if (!payment.IsSelectable && payment.IsSelected)
                        {
                            payment.IsSelected = false;
                        }
                    }
                    else
                    {
                        // Si no tiene vencidos, puede seleccionar cualquiera
                        payment.IsSelectable = true;
                    }
                }
            }
        }

        private bool _isOverdueWarningPopupOpen;
        public bool IsOverdueWarningPopupOpen
        {
            get => _isOverdueWarningPopupOpen;
            set { _isOverdueWarningPopupOpen = value; OnPropertyChanged(nameof(IsOverdueWarningPopupOpen)); }
        }

        private ObservableCollection<PaymentItem> _overduePaymentsList;
        public ObservableCollection<PaymentItem> OverduePaymentsList
        {
            get => _overduePaymentsList;
            set { _overduePaymentsList = value; OnPropertyChanged(nameof(OverduePaymentsList)); }
        }

        private string _overdueStudentName;
        public string OverdueStudentName
        {
            get => _overdueStudentName;
            set { _overdueStudentName = value; OnPropertyChanged(nameof(OverdueStudentName)); }
        }

        private PaymentItem _oldestPaymentToSelect;

        private string _overdueWarningMessage;
        public string OverdueWarningMessage
        {
            get => _overdueWarningMessage;
            set { _overdueWarningMessage = value; OnPropertyChanged(nameof(OverdueWarningMessage)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void AcceptOverdueSelection()
        {
            IsOverdueWarningPopupOpen = false;
            if (_oldestPaymentToSelect != null)
            {
                // Buscar el objeto real en PendingPayments para asegurar que actualizamos la lista principal
                var target = PendingPayments.FirstOrDefault(p => p.DocumentoPorPagarId == _oldestPaymentToSelect.DocumentoPorPagarId);
                if (target != null)
                {
                    target.IsSelected = true;
                }
                _oldestPaymentToSelect = null;
                CalcularTotal();
            }
        }
    }
}