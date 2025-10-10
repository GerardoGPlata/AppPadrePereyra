using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace EscolarAppPadres.Views.School;

public partial class BoletaPdfView : ContentPage
{
    private readonly byte[] _pdfBytes;
    private readonly Stream _viewerStream;
    private string? _sharedFilePath;

    public BoletaPdfView(Stream pdfStream)
    {
        InitializeComponent();

        if (pdfStream is null)
        {
            throw new ArgumentNullException(nameof(pdfStream));
        }

        _pdfBytes = ReadAllBytes(pdfStream);
        _viewerStream = new MemoryStream(_pdfBytes, writable: false);
        pdfViewer.DocumentSource = _viewerStream;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        pdfViewer.DocumentSource = null;
        _viewerStream?.Dispose();

        if (!string.IsNullOrWhiteSpace(_sharedFilePath) && File.Exists(_sharedFilePath))
        {
            try
            {
                File.Delete(_sharedFilePath);
            }
            catch
            {
                // Ignorado: archivo temporal.
            }
        }
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        await SharePdfAsync();
    }

    private async Task SharePdfAsync()
    {
        if (_pdfBytes.Length == 0)
        {
            await DisplayAlert("Compartir boleta", "No hay contenido para compartir.", "Aceptar");
            return;
        }

        try
        {
            var fileName = $"boleta_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var cacheDirectory = FileSystem.CacheDirectory;
            var filePath = Path.Combine(cacheDirectory, fileName);

            await File.WriteAllBytesAsync(filePath, _pdfBytes);
            _sharedFilePath = filePath;

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Compartir boleta",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Compartir boleta", $"No fue posible compartir la boleta. {ex.Message}", "Aceptar");
        }
    }

    private static byte[] ReadAllBytes(Stream source)
    {
        if (source is MemoryStream memoryStream)
        {
            if (memoryStream.CanSeek)
            {
                memoryStream.Position = 0;
            }

            var result = memoryStream.ToArray();
            memoryStream.Dispose();
            return result;
        }

        using var buffer = new MemoryStream();
        if (source.CanSeek)
        {
            source.Position = 0;
        }

        source.CopyTo(buffer);
        source.Dispose();
        return buffer.ToArray();
    }
}
