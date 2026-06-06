using System;
using System.IO;
using NAudio.Wave;
using AudioCompressionApp.CompressionAlgorithms;

namespace AudioCompressionApp.mohalali
{
    public static class CompressionService
    {
        public static (string compressedPath, double elapsedSeconds, int bitsPerSample, int sampleRate, int channels) Compress(
    string inputPath, string algorithm, int targetSampleRate)
        {
            var startTime = DateTime.Now;

            string dir = Path.GetDirectoryName(inputPath);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
            string outputPath = Path.Combine(dir, nameWithoutExt + "_compressed.wav");

            // حذف الملف القديم إن وُجد
            if (File.Exists(outputPath))
            {
                try
                {
                    File.Delete(outputPath);
                }
                catch (IOException)
                {
                    outputPath = Path.Combine(dir, nameWithoutExt + "_compressed_" + DateTime.Now.Ticks + ".wav");
                }
            }

            // قراءة العينات من الملف الصوتي
            float[] samples;
            int originalSampleRate;
            int channels;
            using (var reader = new AudioFileReader(inputPath))
            {
                originalSampleRate = reader.WaveFormat.SampleRate;
                channels = reader.WaveFormat.Channels;

                var buffer = new float[reader.Length / 4];
                int read = reader.Read(buffer, 0, buffer.Length);
                Array.Resize(ref buffer, read);
                samples = buffer;
            }

            // تطبيق الخوارزمية المختارة (حفظ البيانات المشفرة مباشرة)
            byte[] compressedData;
            int bitsPerSample;
            
            switch (algorithm)
            {
                case "DPCM":
                    compressedData = DPCM.Encode(samples, quantizationLevels: 256);
                    bitsPerSample = 8; // 256 مستوى تكميم = 8 بت
                    break;
                case "Delta Modulation":
                    // Delta Modulation: 1 بت لكل عينة، لكن نجمعه في بايتات
                    byte[] dmBits = DeltaModulation.Encode(samples, stepSize: 1);
                    compressedData = PackBits(dmBits);
                    bitsPerSample = 1;
                    break;
                case "Adaptive Delta Modulation":
                    byte[] admBits = AdaptiveDeltaModulation.Encode(samples, initialStep: 1, maxStep: 10, alpha: 0.2f);
                    compressedData = PackBits(admBits);
                    bitsPerSample = 1;
                    break;
                default:
                    throw new ArgumentException($"خوارزمية غير معروفة: {algorithm}");
            }

            // كتابة ملف WAV مخصص بالبيانات المشفرة
            WriteCustomWav(outputPath, compressedData, originalSampleRate, channels, bitsPerSample);

            double elapsed = (DateTime.Now - startTime).TotalSeconds;
            return (outputPath, elapsed, bitsPerSample, originalSampleRate, channels);
        }

        // تجميع البتات في بايتات (للـ Delta Modulation)
        private static byte[] PackBits(byte[] bits)
        {
            byte[] packed = new byte[(bits.Length + 7) / 8];
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] != 0)
                    packed[i / 8] |= (byte)(1 << (7 - (i % 8)));
            }
            return packed;
        }

        // كتابة ملف WAV برأس صحيح للبيانات المشفرة
        private static void WriteCustomWav(string path, byte[] data, int sampleRate, int channels, int bitsPerSample)
        {
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);
            
            using (var fs = new FileStream(path, FileMode.Create))
            using (var writer = new BinaryWriter(fs))
            {
                // رأس RIFF
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + data.Length); // حجم الملف - 8
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });

                // رأس fmt
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16); // حجم رأس fmt
                writer.Write((short)1); // PCM (سنستخدمه حتى مع البيانات الخاصة)
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write(blockAlign);
                writer.Write((short)bitsPerSample);

                // رأس data
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(data.Length);
                writer.Write(data);
            }
        }
    }
}