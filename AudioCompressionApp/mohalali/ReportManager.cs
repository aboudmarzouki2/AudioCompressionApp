// الملف: mohalali/ReportManager.cs
using System;
using System.IO;
using System.Windows.Forms;

namespace AudioCompressionApp.mohalali
{
    public static class ReportManager
    {
        /// <summary>
        /// يحدث نص lblProps بخصائص الملف المضغوط
        /// </summary>
        public static void UpdatePropertiesAfterCompression(
            AudioEngine engine, string compressedPath, Label lblProps)
        {
            lblProps.Text = engine.LoadFileAndGetProperties(compressedPath);
        }

        /// <summary>
        /// يعرض نافذة تقرير الضغط
        /// </summary>
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