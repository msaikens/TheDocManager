using PdfiumViewer;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;

namespace TheDocManager.Views
{
    public partial class DocumentPreviewPage : Page
    {
        private readonly PdfDocument? _pdfDocument;
        private int _currentPage = 0;
        private readonly int _totalPages = 0;

        public DocumentPreviewPage()
        {
            InitializeComponent();
        }

        public DocumentPreviewPage(string filepath, string filename, DateTime uploadDate, string fileType)
        {
            FileNameTextBlock.Text = filename;
            UploadDateTextBlock.Text = $"Uploaded on: {uploadDate:g}";

            PreviewImage.Visibility = Visibility.Visible;
            UnsupportedTextBlock.Visibility = Visibility.Collapsed;

            PrevPageButton.IsEnabled = false;
            NextPageButton.IsEnabled = false;
            PageNumberTextBlock.Text = "";

            // Dispose previous document if any
            _pdfDocument?.Dispose();
            _pdfDocument = null;

            string ext = Path.GetExtension(filepath).ToLowerInvariant();

            if (ext == ".pdf")
            {
                try
                {
                    _pdfDocument = PdfDocument.Load(filepath);
                    _totalPages = _pdfDocument.PageCount;
                    _currentPage = 0;

                    RenderPdfPage(_currentPage);

                    // Enable navigation buttons if multiple pages
                    if (_totalPages > 1)
                    {
                        NextPageButton.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    ShowUnsupported($"Failed to load PDF file: {ex.Message}");
                }
            }
            else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp")
            {
                try
                {
                    var bitmapImage = new BitmapImage(new Uri(filepath));
                    PreviewImage.Source = bitmapImage;

                    // No pages to navigate
                    PrevPageButton.IsEnabled = false;
                    NextPageButton.IsEnabled = false;
                    PageNumberTextBlock.Text = "";
                }
                catch (Exception ex)
                {
                    ShowUnsupported($"Failed to load image file: {ex.Message}");
                }
            }
            else
            {
                ShowUnsupported();
            }
        }

        private void RenderPdfPage(int pageNumber)
        {
            if (_pdfDocument == null) return;

            Bitmap bmp = (Bitmap)_pdfDocument.Render(pageNumber, 96, 96, PdfRenderFlags.Annotations);
            PreviewImage.Source = ConvertBitmapToBitmapSource(bmp);
            bmp.Dispose();

            PageNumberTextBlock.Text = $"Page {pageNumber + 1} of {_totalPages}";

            PrevPageButton.IsEnabled = pageNumber > 0;
            NextPageButton.IsEnabled = pageNumber < _totalPages - 1;
        }

        private void ShowUnsupported(string? message = null)
        {
            PreviewImage.Source = null;
            PreviewImage.Visibility = Visibility.Collapsed;
            UnsupportedTextBlock.Text = message ?? "This file type cannot be previewed yet. Watch for updates, as this might change!";
            UnsupportedTextBlock.Visibility = Visibility.Visible;

            PrevPageButton.IsEnabled = false;
            NextPageButton.IsEnabled = false;
            PageNumberTextBlock.Text = "";
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pdfDocument == null || _currentPage <= 0) return;

            _currentPage--;
            RenderPdfPage(_currentPage);
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pdfDocument == null || _currentPage >= _totalPages - 1) return;

            _currentPage++;
            RenderPdfPage(_currentPage);
        }

        private static unsafe BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {
                var format = bitmap.PixelFormat switch
                {
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb => PixelFormats.Bgra32,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb => PixelFormats.Bgr24,
                    _ => throw new NotSupportedException("Unsupported pixel format: " + bitmap.PixelFormat),
                };
                {
                    return BitmapSource.Create(
                        bitmap.Width,
                        bitmap.Height,
                        bitmap.HorizontalResolution,
                        bitmap.VerticalResolution,
                        format,
                        null,
                        (nint)(void*)bmpData.Scan0,
                        bmpData.Stride * bitmap.Height,
                        bmpData.Stride);
                   
                }
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
                GC.Collect();
            }
        }
    }
}
