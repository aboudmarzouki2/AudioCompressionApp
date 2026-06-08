using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using AudioCompressionApp.CompressionAlgorithms;

namespace AudioCompressionApp.mohalali
{
    public static class CompressionService
    {
        public static (string compressedPath, double elapsedSeconds, int bitsPerSample, int sampleRate, int channels) Compress(
            string inputPath,
            string algorithm,
            int targetSampleRate,
            int quantizationLevels,
            int stepSize,
            float alpha,
            CancellationToken token,
            IProgress<(int percentage, double ratio, double speed)> progress)
        {
            var startTime = DateTime.Now;
            string dir = Path.GetDirectoryName(inputPath);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
            string outputPath = Path.Combine(dir, nameWithoutExt + "_compressed.wav");

            if (File.Exists(outputPath)) try { File.Delete(outputPath); } catch { }

            float[] samples;
            int originalSampleRate;
            int channels;

            // 1. SAFE READING: Reads exact buffer chunks to prevent the MP3 empty-silence bug
            using (var reader = new AudioFileReader(inputPath))
            {
                originalSampleRate = reader.WaveFormat.SampleRate;
                channels = reader.WaveFormat.Channels;

                var sampleList = new System.Collections.Generic.List<float>();
                var buffer = new float[8192];
                int read;

                while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        sampleList.Add(buffer[i]);
                    }
                }
                samples = sampleList.ToArray();
            }

            if (token.IsCancellationRequested) return (string.Empty, 0, 0, 0, 0);

            byte[] compressedData;
            int bitsPerSample;
            long originalSizeByte = new FileInfo(inputPath).Length;

            // Adjust original size calculation if testing an MP3 for accurate graphs
            if (inputPath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                originalSizeByte = samples.Length * 2;
            }

            // 2. FULL ENGINE ROUTING (All 5 Algorithms)
            switch (algorithm)
            {
                case "DPCM":
                    compressedData = DPCM.Encode(samples, quantizationLevels: quantizationLevels);
                    bitsPerSample = 8;
                    break;
                case "Delta Modulation":
                    byte[] dmBits = DeltaModulation.Encode(samples, stepSize: stepSize);
                    compressedData = PackBits(dmBits);
                    bitsPerSample = 1;
                    break;
                case "Adaptive Delta Modulation":
                    byte[] admBits = AdaptiveDeltaModulation.Encode(samples, initialStep: stepSize, maxStep: 10, alpha: alpha);
                    compressedData = PackBits(admBits);
                    bitsPerSample = 1;
                    break;
                case "Predictive Differential Coding":
                    compressedData = PredictiveDifferentialCoding.Encode(samples, quantizationLevels: quantizationLevels);
                    bitsPerSample = 8;
                    break;
                case "Nonlinear Quantization":
                    compressedData = NonlinearQuantization.Encode(samples, quantizationLevels: quantizationLevels);
                    bitsPerSample = 8;
                    break;
                default:
                    throw new ArgumentException($"Unknown algorithm: {algorithm}");
            }

            // Write using original sample rate to guarantee 1:1 file size matching later
            WriteCustomWav(outputPath, compressedData, originalSampleRate, channels, bitsPerSample);

            double finalElapsed = (DateTime.Now - startTime).TotalSeconds;
            double finalRatio = 100.0 * (1.0 - ((double)compressedData.Length / originalSizeByte));
            progress?.Report((100, finalRatio, 0));

            return (outputPath, finalElapsed, bitsPerSample, originalSampleRate, channels);
        }

        private static byte[] PackBits(byte[] bits)
        {
            byte[] packed = new byte[(bits.Length + 7) / 8];
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] != 0) packed[i / 8] |= (byte)(1 << (7 - (i % 8)));
            }
            return packed;
        }

        private static void WriteCustomWav(string path, byte[] data, int sampleRate, int channels, int bitsPerSample)
        {
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);

            using (var fs = new FileStream(path, FileMode.Create))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + data.Length);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write(blockAlign);
                writer.Write((short)bitsPerSample);
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(data.Length);
                writer.Write(data);
            }
        }
    }
}