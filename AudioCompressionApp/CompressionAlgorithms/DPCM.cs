using System;

namespace multiMediaProject.CompressionAlgorithms
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
            short[] encoded = new short[samples.Length];
            float predicted = 0;
            float step = 2f / quantizationLevels;

            for (int i = 0; i < samples.Length; i++)
            {
                float error = samples[i] - predicted;
                int quantizedError = (int)((error + 1) / step);
                quantizedError = Clamp(quantizedError, 0, quantizationLevels - 1);
                encoded[i] = (short)quantizedError;
                predicted += (quantizedError * step) - 1;
            }

            byte[] result = new byte[encoded.Length * 2];
            Buffer.BlockCopy(encoded, 0, result, 0, result.Length);
            return result;
        }

        public static float[] Decode(byte[] compressedData, int quantizationLevels)
        {
            short[] encoded = new short[compressedData.Length / 2];
            Buffer.BlockCopy(compressedData, 0, encoded, 0, compressedData.Length);

            float[] decoded = new float[encoded.Length];
            float predicted = 0;
            float step = 2f / quantizationLevels;

            for (int i = 0; i < encoded.Length; i++)
            {
                float error = (encoded[i] * step) - 1;
                predicted += error;
                decoded[i] = predicted;
            }
            return decoded;
        }
    }
}