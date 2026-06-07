using System;

namespace AudioCompressionApp.CompressionAlgorithms
{
    public static class DPCM
    {
        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static byte[] Encode(float[] samples, int quantizationLevels)
        {
            byte[] encoded = new byte[samples.Length];
            float predicted = 0;
            float step = 2.0f / quantizationLevels;

            for (int i = 0; i < samples.Length; i++)
            {
                // Calculate the error between current sample and our prediction
                float error = samples[i] - predicted;

                // Map the error into our quantized range
                int quantizedError = (int)((error + 1.0f) / step);
                quantizedError = Clamp(quantizedError, 0, quantizationLevels - 1);
                encoded[i] = (byte)quantizedError;

                // Update prediction based on the DE-QUANTIZED error
                // This must perfectly mirror the Decode logic
                predicted += (quantizedError * step) - 1.0f;
                predicted = Math.Max(-1.0f, Math.Min(1.0f, predicted)); // Prevent drift
            }
            return encoded;
        }

        public static float[] Decode(byte[] compressedData, int quantizationLevels)
        {
            float[] decoded = new float[compressedData.Length];
            float predicted = 0;
            float step = 2.0f / quantizationLevels;

            for (int i = 0; i < compressedData.Length; i++)
            {
                // De-quantize the received value back to the error space
                float error = (compressedData[i] * step) - 1.0f;

                // Update the state (mirroring the encoder's state)
                predicted += error;
                predicted = Math.Max(-1.0f, Math.Min(1.0f, predicted));

                decoded[i] = predicted;
            }
            return decoded;
        }
    }
}