using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TwainDotNet;
using TwainDotNet.Win32;
using TwainDotNet.Wpf;

namespace TheDocManager.Services
{
    public class ScanService
    {
        private Twain _twain;
        private bool _isScanning;
        private readonly Window _ownerWindow;

        public ScanService(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
            _twain = new Twain(new WpfWindowMessageHook(ownerWindow));
            _twain.TransferImage += OnTransferImage;
            _twain.ScanningComplete += OnScanningComplete;
        }

        public void StartScan()
        {
            if (_isScanning) return;

            try
            {
                _twain.SelectSource(); // Optional: comment out to skip dialog
                _twain.StartScanning(new ScanSettings
                {
                    UseDocumentFeeder = false,
                    ShowTwainUI = true,
                    Resolution = new ResolutionSettings { Dpi = 300 },
                    ShouldTransferAllPages = true,
                    Area = null,
                    UseDuplex = false
                });

                _isScanning = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"TWAIN scan failed: {ex.Message}");
            }
        }

        private void OnTransferImage(object? sender, TransferImageEventArgs e)

        {
            if (e.Image == null)
            {
                MessageBox.Show("Failed to acquire image from scanner.");
                return;
            }

            string outputDir = AppPaths.DocumentsDirectory;
            Directory.CreateDirectory(outputDir);

            string filename = $"Scan_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string filePath = Path.Combine(outputDir, filename);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new JpegBitmapEncoder();
                var bitmapSource = ConvertBitmapToBitmapSource(e.Image);
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                encoder.Save(fs);
            }

            MessageBox.Show($"Scan saved: {filePath}");
        }

        private void OnScanningComplete(object? sender, ScanningCompleteEventArgs e)

        {
            _isScanning = false;
        }
        private static unsafe BitmapSource ConvertBitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {
                PixelFormat pixelFormat = PixelFormats.Bgr24;

                if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    pixelFormat = PixelFormats.Bgra32;
                }

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    pixelFormat,
                    null,
                    (nint)(byte*)bitmapData.Scan0,
                    bitmapData.Stride * bitmap.Height,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

    }
}
