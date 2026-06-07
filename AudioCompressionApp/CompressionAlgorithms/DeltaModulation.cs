using System;
using System.Collections.Generic;

namespace AudioCompressionApp.CompressionAlgorithms
{
    public static class DeltaModulation
    {
        public static byte[] Encode(float[] samples, int stepSize)
        {
            List<byte> encoded = new List<byte>();
            float predicted = 0;
            // Normalize: If user picks step size 1, actual step is 0.01f
            float step = stepSize / 100.0f;

            foreach (float sample in samples)
            {
                byte bit = sample >= predicted ? (byte)1 : (byte)0;
                predicted += (bit == 1) ? step : -step;

                // Clamp to prevent audio clipping
                predicted = Math.Max(-1.0f, Math.Min(1.0f, predicted));

                encoded.Add(bit);
            }
            return encoded.ToArray();
        }

        public static float[] Decode(byte[] encoded, int stepSize)
        {
            float[] decoded = new float[encoded.Length];
            float predicted = 0;
            float step = stepSize / 100.0f;

            for (int i = 0; i < encoded.Length; i++)
            {
                predicted += (encoded[i] == 1) ? step : -step;

                // Clamp to prevent audio clipping
                predicted = Math.Max(-1.0f, Math.Min(1.0f, predicted));

                decoded[i] = predicted;
            }
            return decoded;
        }
    }
}