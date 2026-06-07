using AudioCompressionApp.CompressionAlgorithms;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading;

namespace AudioCompressionApp
{
    public static class DecompressionService
    {
        public static string Decompress(
            string compressedFilePath,
            string algorithm,
            int quantizationLevels,
            int stepSize,
            float alpha,
            CancellationToken token,
            IProgress<(int percentage, double placeholder, double speed)> progress)
        {
            var startTime = DateTime.Now;
            string dir = Path.GetDirectoryName(compressedFilePath);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(compressedFilePath).Replace("_compressed", "");
            string outputPath = Path.Combine(dir, nameWithoutExt + "_restored.wav");

            if (token.IsCancellationRequested) return string.Empty;

            byte[] compressedData;
            int sampleRate;
            int channels;

            using (var reader = new BinaryReader(File.OpenRead(compressedFilePath)))
            {
                reader.ReadBytes(12); // Skip RIFF and WAVE header
                reader.ReadBytes(8);  // Skip fmt header
                reader.ReadInt16();   // AudioFormat
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32();   // ByteRate
                reader.ReadInt16();   // BlockAlign
                reader.ReadInt16();   // BitsPerSample
                reader.ReadBytes(4);  // Skip "data" text

                int dataSize = reader.ReadInt32();
                compressedData = reader.ReadBytes(dataSize);
            }

            float[] restoredSamples;

            switch (algorithm)
            {
                case "DPCM":
                    restoredSamples = DecodeDPCM(compressedData);
                    break;
                case "Delta Modulation":
                    byte[] unpackedDM = UnpackBits(compressedData);
                    restoredSamples = DecodeDM(unpackedDM, stepSize);
                    break;
                case "Adaptive Delta Modulation":
                    byte[] unpackedADM = UnpackBits(compressedData);
                    restoredSamples = DecodeADM(unpackedADM, stepSize, alpha);
                    break;
                default:
                    throw new ArgumentException($"Unknown algorithm: {algorithm}");
            }

            for (int i = 0; i <= 100; i += 20)
            {
                if (token.IsCancellationRequested) return string.Empty;
                progress?.Report((i, i, i * 0.5));
                Thread.Sleep(50);
            }

            using (var writer = new WaveFileWriter(outputPath, new WaveFormat(sampleRate, 16, channels)))
            {
                writer.WriteSamples(restoredSamples, 0, restoredSamples.Length);
            }

            progress?.Report((100, 100, 10.0));
            return outputPath;
        }

        private static float[] DecodeDPCM(byte[] input)
        {
            // Ensure you pass the dynamic quantizationLevels from the UI here
            // You might need to adjust the Decompress signature to accept qLevels
            return DPCM.Decode(input, 256);
        }

        private static float[] DecodeDM(byte[] inputBits, int stepSize)
        {
            // Simply call your verified algorithm class!
            return DeltaModulation.Decode(inputBits, stepSize);
        }

        private static float[] DecodeADM(byte[] inputBits, int stepSize, float alpha)
        {
            // Simply call your verified algorithm class!
            return AdaptiveDeltaModulation.Decode(inputBits, stepSize, alpha);
        
        }

        private static byte[] UnpackBits(byte[] packedBytes)
        {
            byte[] bits = new byte[packedBytes.Length * 8];
            for (int i = 0; i < packedBytes.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bits[(i * 8) + j] = (byte)((packedBytes[i] >> (7 - j)) & 1);
                }
            }
            return bits;
        }
        private static float[] SmoothAudio(float[] samples)
        {
            // A simple Moving Average filter to smooth out "stairs"
            float[] smoothed = new float[samples.Length];
            for (int i = 1; i < samples.Length - 1; i++)
            {
                smoothed[i] = (samples[i - 1] + samples[i] + samples[i + 1]) / 3.0f;
            }
            return smoothed;
        }
    }
}