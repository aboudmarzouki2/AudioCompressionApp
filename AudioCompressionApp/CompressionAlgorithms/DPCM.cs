using System;

namespace AudioCompressionApp.CompressionAlgorithms
{
    public static class DPCM
    {
        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static byte[] Encode(float[] samples, int quantizationLevels)
        {
            // تخزين كل عينة كـ byte واحد (8-bit)
            byte[] encoded = new byte[samples.Length];
            float predicted = 0;
            float step = 2f / quantizationLevels;

            for (int i = 0; i < samples.Length; i++)
            {
                float error = samples[i] - predicted;
                int quantizedError = (int)((error + 1) / step);
                quantizedError = Clamp(quantizedError, 0, quantizationLevels - 1);
                encoded[i] = (byte)quantizedError;
                predicted += (quantizedError * step) - 1;
            }

            return encoded;
        }

        public static float[] Decode(byte[] compressedData, int quantizationLevels)
        {
            float[] decoded = new float[compressedData.Length];
            float predicted = 0;
            float step = 2f / quantizationLevels;

            for (int i = 0; i < compressedData.Length; i++)
            {
                float error = (compressedData[i] * step) - 1;
                predicted += error;
                decoded[i] = predicted;
            }
            return decoded;
        }
    }
}