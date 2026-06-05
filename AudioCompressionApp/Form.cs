using System;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using multiMediaProject.CompressionAlgorithms;

namespace multiMediaProject
{
    public partial class Form1 : Form
    {
        private AudioFileReader audioFileReader;
        private WaveOutEvent outputDevice;
        private float[] originalSamples;
        private byte[] compressedData;
        private string currentAlgorithm;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnLoadAudio_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Audio Files|*.wav;*.mp3;*.m4a;*.flac";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
                        outputDevice.Stop();

                    audioFileReader = new AudioFileReader(openFileDialog.FileName);
                    outputDevice = new WaveOutEvent();
                    outputDevice.Init(audioFileReader);

                    originalSamples = new float[audioFileReader.Length / 4];
                    audioFileReader.Position = 0;
                    audioFileReader.Read(originalSamples, 0, originalSamples.Length);

                    MessageBox.Show("تم تحميل الملف بنجاح");
                }
            }
        }

        private void btnPlay2_Click(object sender, EventArgs e)
        {
            if (outputDevice != null)
                outputDevice.Play();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (outputDevice != null)
                outputDevice.Stop();
        }

        private void btnCompress_Click(object sender, EventArgs e)
        {
            if (originalSamples == null)
            {
                MessageBox.Show("الرجاء تحميل ملف صوتي أولاً");
                return;
            }

            currentAlgorithm = cmbAlgorithm.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(currentAlgorithm))
            {
                MessageBox.Show("الرجاء اختيار خوارزمية الضغط");
                return;
            }

            try
            {
                switch (currentAlgorithm)
                {
                    case "Delta Modulation (DM)":
                        compressedData = DeltaModulation.Encode(originalSamples, 10);
                        break;
                    case "Adaptive Delta Modulation (ADM)":
                        compressedData = AdaptiveDeltaModulation.Encode(originalSamples, 10, 50, 1.2f);
                        break;
                    case "DPCM":
                        compressedData = DPCM.Encode(originalSamples, 16);
                        break;
                    default:
                        MessageBox.Show("خوارزمية غير معروفة");
                        return;
                }

                MessageBox.Show($"تم الضغط بنجاح!\nالحجم بعد الضغط: {compressedData.Length / 1024} KB");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void cmbAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnDecompress_Click(object sender, EventArgs e)
        {
            if (compressedData == null)
            {
                MessageBox.Show("الرجاء ضغط ملف أولاً");
                return;
            }

            try
            {
                float[] decompressedSamples;

                switch (currentAlgorithm)
                {
                    case "Delta Modulation (DM)":
                        decompressedSamples = DeltaModulation.Decode(compressedData, 10);
                        break;
                    case "Adaptive Delta Modulation (ADM)":
                        decompressedSamples = AdaptiveDeltaModulation.Decode(compressedData, 10, 50, 1.2f);
                        break;
                    case "DPCM":
                        decompressedSamples = DPCM.Decode(compressedData, 16);
                        break;
                    default:
                        MessageBox.Show("خوارزمية غير معروفة");
                        return;
                }

                MessageBox.Show("تم فك الضغط بنجاح!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }
    }
}