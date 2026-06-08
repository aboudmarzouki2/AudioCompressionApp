using System;

namespace AudioCompressionApp.CompressionAlgorithms
{
    public static class PredictiveDifferentialCoding
    {
        public static byte[] Encode(float[] samples, int quantizationLevels)
        {
            byte[] encoded = new byte[samples.Length];
            float prev1 = 0f;
            float prev2 = 0f;

            // Maximum possible error between a sample [-1, 1] and prediction [-1, 1] is 2.
            // So the error range is [-2, 2]. We shift by +2 to make it [0, 4].
            float step = 4.0f / quantizationLevels;

            for (int i = 0; i < samples.Length; i++)
            {
                // 2nd-order prediction: Extrapolate the trajectory using the last two samples
                float predicted = (2.0f * prev1) - prev2;

                // Clamp prediction to valid audio range
                predicted = Math.Max(-1.0f, Math.Min(1.0f, predicted));

                float error = samples[i] - predicted;

                // Quantize the error
                int quantizedError = (int)Math.Round((error + 2.0f) / step);
                quantizedError = Math.Max(0, Math.Min(quantizationLevels - 1, quantizedError));

                encoded[i] = (byte)quantizedError;

                // Reconstruct to keep the encoder and decoder perfectly synced (prevent drift)
                float reconstructedError = (quantizedError * step) - 2.0f;
                float reconstructedSample = predicted + reconstructedError;

                prev2 = prev1;
                prev1 = Math.Max(-1.0f, Math.Min(1.0f, reconstructedSample));
            }
            return encoded;
        }

        public static float[] Decode(byte[] encoded, int quantizationLevels)
        {
            float[] samples = new float[encoded.Length];
            float prev1 = 0f;
            float prev2 = 0f;
            float step = 4.0f / quantizationLevels;

            for (int i = 0; i < encoded.Length; i++)
            {
                // Mirror the exact same prediction logic
                float predicted = (2.0f * prev1) - prev2;
                predicted = Math.Max(-1.0f, Math.Min(1.0f, predicted));

                // De-quantize the error
                float reconstructedError = (encoded[i] * step) - 2.0f;
                float sample = predicted + reconstructedError;

                // Clamp the final sample to prevent audio clipping
                sample = Math.Max(-1.0f, Math.Min(1.0f, sample));

                samples[i] = sample;

                // Update history
                prev2 = prev1;
                prev1 = sample;
            }
            return samples;
        }
    }
}