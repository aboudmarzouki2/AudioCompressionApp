using System.Collections.Generic;

namespace AudioCompressionApp.CompressionAlgorithms
{
    public static class DeltaModulation
    {
        public static byte[] Encode(float[] samples, int stepSize)
        {
            List<byte> encoded = new List<byte>();
            float predicted = 0;
            foreach (float sample in samples)
            {
                byte bit = sample >= predicted ? (byte)1 : (byte)0;
                predicted += (bit == 1) ? stepSize : -stepSize;
                encoded.Add(bit);
            }
            return encoded.ToArray();
        }

        public static float[] Decode(byte[] encoded, int stepSize)
        {
            List<float> decoded = new List<float>();
            float predicted = 0;
            foreach (byte bit in encoded)
            {
                predicted += (bit == 1) ? stepSize : -stepSize;
                decoded.Add(predicted);
            }
            return decoded.ToArray();
        }
    }
}