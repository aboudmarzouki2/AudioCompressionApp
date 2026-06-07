using System;
using System.Collections.Generic;

namespace AudioCompressionApp.CompressionAlgorithms
{
    public static class AdaptiveDeltaModulation
    {
        // We use float stepSize to match the [-1.0, 1.0] audio range
        public static byte[] Encode(float[] samples, int initialStep, int maxStep, float alpha)
        {
            List<byte> encoded = new List<byte>();
            float predicted = 0;
            // Normalize step: if user inputs 1, effective step is 0.01f
            float stepSize = initialStep / 100.0f;
            float maxStepFloat = maxStep / 100.0f;

            for (int i = 0; i < samples.Length; i++)
            {
                byte bit = samples[i] >= predicted ? (byte)1 : (byte)0;
                predicted += (bit == 1) ? stepSize : -stepSize;

                // Clamp predicted value to stay within valid audio range
                predicted = Math.Max(-1.0f, Math.Min(1.0f, predicted));

                encoded.Add(bit);

                // Adaptive step size adjustment
                if (i > 0 && bit == encoded[i - 1])
                    stepSize = Math.Min(stepSize * (1 + alpha), maxStepFloat);
                else
                    stepSize = Math.Max(initialStep / 100.0f, stepSize / (1 + alpha));
            }
            return encoded.ToArray();
        }

        public static float[] Decode(byte[] encoded, int initialStep, float alpha)
        {
            float[] decoded = new float[encoded.Length];
            float predicted = 0;
            float stepSize = initialStep / 100.0f;
            float maxStepFloat = 10f / 100f; // Matches the maxStep in DecompressionService

            for (int i = 0; i < encoded.Length; i++)
            {
                predicted += (encoded[i] == 1) ? stepSize : -stepSize;
                predicted = Math.Max(-1.0f, Math.Min(1.0f, predicted));
                decoded[i] = predicted;

                // Same adaptive logic as Encode
                if (i > 0 && encoded[i] == encoded[i - 1])
                    stepSize = Math.Min(stepSize * (1 + alpha), maxStepFloat);
                else
                    stepSize = Math.Max(initialStep / 100.0f, stepSize / (1 + alpha));
            }
            return decoded;
        }
    }
}