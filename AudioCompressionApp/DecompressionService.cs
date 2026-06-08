using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using AudioCompressionApp.CompressionAlgorithms;

namespace AudioCompressionApp.mohalali
{
    public static class DecompressionService
    {
        public static (string decompressedPath, double elapsedSeconds) Decompress(
            string inputPath,
            string algorithm,
            int quantizationLevels,
            int stepSize,
            float alpha,
            CancellationToken token,
            IProgress<(int percentage, double ratio, double speed)> progress)
        {
            var startTime = DateTime.Now;
            string dir = Path.GetDirectoryName(inputPath);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(inputPath).Replace("_compressed", "");
            string outputPath = Path.Combine(dir, nameWithoutExt + "_restored.wav");

            if (File.Exists(outputPath)) try { File.Delete(outputPath); } catch { }

            byte[] compressedData = null;
            int sampleRate = 44100;
            int channels = 1;
            int bitsPerSample = 8;

            // 1. READ THE CUSTOM COMPRESSED WAV FILE
            using (var fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                reader.ReadBytes(4); // RIFF
                reader.ReadInt32();  // File Size
                reader.ReadBytes(4); // WAVE
                reader.ReadBytes(4); // fmt 
                reader.ReadInt32();  // Subchunk1Size
                reader.ReadInt16();  // AudioFormat
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32();  // ByteRate
                reader.ReadInt16();  // BlockAlign
                bitsPerSample = reader.ReadInt16();
                reader.ReadBytes(4); // data
                int dataSize = reader.ReadInt32();
                compressedData = reader.ReadBytes(dataSize);
            }

            if (token.IsCancellationRequested) return (string.Empty, 0);

            float[] decodedSamples;

            // 2. ROUTE TO THE CORRECT DECODER
            switch (algorithm)
            {
                case "DPCM":
                    decodedSamples = DPCM.Decode(compressedData, quantizationLevels: quantizationLevels);
                    break;
                case "Delta Modulation":
                    byte[] unpackedDM = UnpackBits(compressedData, compressedData.Length * 8);
                    decodedSamples = DeltaModulation.Decode(unpackedDM, stepSize: stepSize);
                    break;
                case "Adaptive Delta Modulation":
                    byte[] unpackedADM = UnpackBits(compressedData, compressedData.Length * 8);
                    decodedSamples = AdaptiveDeltaModulation.Decode(unpackedADM, initialStep: stepSize, alpha: alpha);
                    break;
                case "Predictive Differential Coding":
                    decodedSamples = PredictiveDifferentialCoding.Decode(compressedData, quantizationLevels: quantizationLevels);
                    break;
                case "Nonlinear Quantization":
                    decodedSamples = NonlinearQuantization.Decode(compressedData, quantizationLevels: quantizationLevels);
                    break;
                default:
                    throw new ArgumentException($"Unknown algorithm: {algorithm}");
            }

            // 3. WRITE BACK TO STANDARD 16-BIT UNCOMPRESSED WAV
            WriteStandardWav(outputPath, decodedSamples, sampleRate, channels);

            double finalElapsed = (DateTime.Now - startTime).TotalSeconds;
            progress?.Report((100, 0, 0)); // Update UI Progress

            return (outputPath, finalElapsed);
        }

        // Reverses the bit-packing done during DM and ADM compression
        private static byte[] UnpackBits(byte[] packed, int totalBits)
        {
            byte[] unpacked = new byte[totalBits];
            for (int i = 0; i < totalBits; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = 7 - (i % 8);
                bool isSet = (packed[byteIndex] & (1 << bitIndex)) != 0;
                unpacked[i] = isSet ? (byte)1 : (byte)0;
            }
            return unpacked;
        }

        // Reconstructs a fully standard, playable WAV file
        private static void WriteStandardWav(string path, float[] samples, int sampleRate, int channels)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var writer = new BinaryWriter(fs))
            {
                int bitsPerSample = 16;
                int byteRate = sampleRate * channels * bitsPerSample / 8;
                short blockAlign = (short)(channels * bitsPerSample / 8);
                int dataSize = samples.Length * 2; // 16-bit = 2 bytes per sample

                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataSize);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1); // 1 = PCM format
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write(blockAlign);
                writer.Write((short)bitsPerSample);
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(dataSize);

                // Convert floating point [-1.0, 1.0] back to 16-bit audio [-32768, 32767]
                foreach (float sample in samples)
                {
                    float clamped = Math.Max(-1.0f, Math.Min(1.0f, sample));
                    short val = (short)(clamped * 32767f);
                    writer.Write(val);
                }
            }
        }
    }
}