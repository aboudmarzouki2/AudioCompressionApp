using System;
using System.IO;
using System.Windows.Forms;

namespace AudioCompressionApp.mohalali
{
    public static class ReportManager
    {
        public static void UpdatePropertiesAfterCompression(
            AudioEngine engine, string compressedPath, Label lblProps,
            long originalSize, int sampleRate, int channels, int bitsPerSample, string algorithm)
        {
            try
            {
                // محاولة قراءة الملف بالطريقة العادية (لـ DPCM وغيره)
                if (bitsPerSample >= 8)
                {
                    lblProps.Text = engine.LoadFileAndGetProperties(compressedPath);
                }
                else
                {
                    // للملفات 1-bit: حساب الخصائص يدوياً
                    FileInfo fileInfo = new FileInfo(compressedPath);
                    long fileSizeBytes = fileInfo.Length;
                    double fileSizeMB = fileSizeBytes / (1024.0 * 1024.0);
                    
                    // حساب المدة بناءً على عدد البتات
                    long totalBits = fileSizeBytes * 8;
                    int totalSamples = (int)(totalBits / channels);
                    double durationSeconds = (double)totalSamples / sampleRate;
                    TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
                    
                    int bitRate = sampleRate * channels * bitsPerSample;
                    
                    lblProps.Text = $"Size: {fileSizeMB:F2} MB\n" +
                                   $"Duration: {duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}\n" +
                                   $"Sample Rate: {sampleRate} Hz\n" +
                                   $"Channels: {channels}\n" +
                                   $"Bit Rate: {bitRate} bps\n" +
                                   $"Bits Per Sample: {bitsPerSample} bit\n" +
                                   $"Algorithm: {algorithm}";
                }
            }
            catch (Exception ex)
            {
                lblProps.Text = $"Error reading compressed file:\n{ex.Message}";
            }
        }

        public static void ShowReport(
            long originalSizeBytes,
            string compressedPath,
            double elapsedSeconds,
            string algorithm,
            int targetSampleRate)
        {
            long compressedSize = new FileInfo(compressedPath).Length;

            double originalMB = originalSizeBytes / (1024.0 * 1024.0);
            double compressedMB = compressedSize / (1024.0 * 1024.0);
            double reduction = (1 - (double)compressedSize / originalSizeBytes) * 100;

            string report =
                $"📊 تقرير الضغط\n\n" +
                $"الخوارزمية: {algorithm}\n" +
                $"معدل العيّنات المستهدف: {targetSampleRate} Hz\n" +
                $"⏱ الزمن المستغرق: {elapsedSeconds:F2} ثانية\n\n" +
                $"حجم الملف الأصلي: {originalMB:F2} MB\n" +
                $"حجم الملف المضغوط: {compressedMB:F2} MB\n" +
                $"نسبة التخفيض: {reduction:F1}%\n\n" +
                $"مسار الملف المضغوط:\n{compressedPath}";

            MessageBox.Show(report, "تقرير الضغط", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}