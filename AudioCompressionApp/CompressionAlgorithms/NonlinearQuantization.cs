using System;

namespace AudioCompressionApp.CompressionAlgorithms
{
    public static class NonlinearQuantization
    {
        // The compression factor controls the curve. Higher = more aggressive logarithmic compression.
        private const float CompressionFactor = 255.0f;

        public static byte[] Encode(float[] samples, int quantizationLevels)
        {
            byte[] encoded = new byte[samples.Length];
            // Range after logarithmic compression is [-1, 1]. Shifted to [0, 2].
            float step = 2.0f / quantizationLevels;

            for (int i = 0; i < samples.Length; i++)
            {
                float sample = Math.Max(-1.0f, Math.Min(1.0f, samples[i]));
                float sign = Math.Sign(sample);
                float magnitude = Math.Abs(sample);

                // 1. Apply Non-Linear Logarithmic Compression (Similar to A-Law/Mu-Law curves)
                float compressedSample = sign * (float)(Math.Log(1.0 + CompressionFactor * magnitude) / Math.Log(1.0 + CompressionFactor));

                // 2. Uniformly quantize the now-compressed signal
                int quantized = (int)Math.Round((compressedSample + 1.0f) / step);

                // Clamp and cast to byte
                encoded[i] = (byte)Math.Max(0, Math.Min(quantizationLevels - 1, quantized));
            }
            return encoded;
        }

        public static float[] Decode(byte[] encoded, int quantizationLevels)
        {
            float[] samples = new float[encoded.Length];
            float step = 2.0f / quantizationLevels;

            for (int i = 0; i < encoded.Length; i++)
            {
                // 1. De-quantize back to the compressed range [-1, 1]
                float compressedSample = (encoded[i] * step) - 1.0f;
                float sign = Math.Sign(compressedSample);
                float magnitude = Math.Abs(compressedSample);

                // 2. Apply Non-Linear Exponential Expansion to restore the original wave shape
                float expandedSample = sign * (float)((Math.Pow(1.0 + CompressionFactor, magnitude) - 1.0) / CompressionFactor);

                samples[i] = Math.Max(-1.0f, Math.Min(1.0f, expandedSample));
            }
            return samples;
        }
    }
}