using System;
using System.Collections.Generic;

namespace AudioCompressionApp.CompressionAlgorithms
{
    public static class AdaptiveDeltaModulation
    {
        public static byte[] Encode(float[] samples, int initialStep, int maxStep, float alpha)
        {
            List<byte> encoded = new List<byte>();
            float predicted = 0;
            int stepSize = initialStep;

            for (int i = 0; i < samples.Length; i++)
            {
                byte bit = samples[i] >= predicted ? (byte)1 : (byte)0;
                predicted += (bit == 1) ? stepSize : -stepSize;
                encoded.Add(bit);

                if (i > 0 && encoded[i] == encoded[i - 1])
                    stepSize = Math.Min(stepSize + (int)(stepSize * alpha), maxStep);
                else
                    stepSize = Math.Max(initialStep, stepSize / 2);
            }
            return encoded.ToArray();
        }

        public static float[] Decode(byte[] encoded, int initialStep, int maxStep, float alpha)
        {
            List<float> decoded = new List<float>();
            float predicted = 0;
            int stepSize = initialStep;

            for (int i = 0; i < encoded.Length; i++)
            {
                predicted += (encoded[i] == 1) ? stepSize : -stepSize;
                decoded.Add(predicted);

                if (i > 0 && encoded[i] == encoded[i - 1])
                    stepSize = Math.Min(stepSize + (int)(stepSize * alpha), maxStep);
                else
                    stepSize = Math.Max(initialStep, stepSize / 2);
            }
            return decoded.ToArray();
        }
    }
}