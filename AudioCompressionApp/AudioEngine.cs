using System;
using NAudio.Wave;

namespace AudioCompressionApp
{
    public class AudioEngine
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        // Extracts properties formatted for the UI Label
        public string LoadFileAndGetProperties(string filePath)
        {
            try
            {
                // Clean up previous file if one was loaded
                DisposeAudio();

                audioFile = new AudioFileReader(filePath);

                long fileSizeBytes = new System.IO.FileInfo(filePath).Length;
                double fileSizeMB = fileSizeBytes / (1024.0 * 1024.0);

                string duration = audioFile.TotalTime.ToString(@"hh\:mm\:ss");
                int sampleRate = audioFile.WaveFormat.SampleRate;
                int channels = audioFile.WaveFormat.Channels;
                int bitRate = audioFile.WaveFormat.AverageBytesPerSecond * 8;
                string encoding = audioFile.WaveFormat.Encoding.ToString();

                return $"Size: {fileSizeMB:F2} MB\n" +
                       $"Duration: {duration}\n" +
                       $"Sample Rate: {sampleRate} Hz\n" +
                       $"Channels: {channels}\n" +
                       $"Bit Rate: {bitRate} bps\n" +
                       $"Encoding: {encoding}";
            }
            catch (Exception ex)
            {
                return $"Error loading file: {ex.Message}";
            }
        }

        public void Play()
        {
            if (audioFile == null) return;

            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.Init(audioFile);
            }
            outputDevice.Play();
        }

        public void Stop()
        {
            outputDevice?.Stop();
            if (audioFile != null)
            {
                audioFile.Position = 0; // Reset to beginning
            }
        }

        // Prevents memory leaks
        public void DisposeAudio()
        {
            outputDevice?.Dispose();
            outputDevice = null;
            audioFile?.Dispose();
            audioFile = null;
        }
    }
}