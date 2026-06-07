using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AudioCompressionApp.mohalali;

namespace AudioCompressionApp
{
    public partial class Form1 : Form
    {
        private AudioEngine engine = new AudioEngine();
        private CompressionContext compressionCtx = new CompressionContext();
        private CancellationTokenSource cts;

        // UI Controls
        private TextBox txtFilePath;
        private Button btnBrowse, btnPlay, btnStop, btnReset, btnCompress, btnCancel, btnDecompress;
        private ComboBox cmbAlgorithms;
        private NumericUpDown numSampleRate;
        private Label lblProps;
        private ProgressBar progressBar;
        private Chart performanceChart;

        // Requirement 6 Controls
        private Label lblQuantization, lblStepSize, lblAlpha;
        private NumericUpDown numQuantization, numStepSize, numAlpha;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Audio Compression Studio - 2026";
            this.Size = new Size(850, 650);
            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;

            // --- 1. File Input & Preview ---
            GroupBox grpFile = new GroupBox { Text = "1. File Input & Preview", Bounds = new Rectangle(20, 20, 500, 80) };
            txtFilePath = new TextBox { Bounds = new Rectangle(20, 30, 350, 25), ReadOnly = true };
            btnBrowse = new Button { Text = "Browse", Bounds = new Rectangle(380, 28, 100, 25) };
            btnPlay = new Button { Text = "▶ Play", Bounds = new Rectangle(20, 60, 80, 25) };
            btnStop = new Button { Text = "■ Stop", Bounds = new Rectangle(110, 60, 80, 25) };
            btnReset = new Button { Text = "↺ Reset", Bounds = new Rectangle(200, 60, 80, 25) };

            btnBrowse.Click += BtnBrowse_Click;
            btnPlay.Click += BtnPlay_Click;
            btnStop.Click += BtnStop_Click;
            btnReset.Click += BtnReset_Click;

            grpFile.Controls.AddRange(new Control[] { txtFilePath, btnBrowse, btnPlay, btnStop, btnReset });

            // --- 2. File Properties ---
            GroupBox grpProps = new GroupBox { Text = "2. File Properties", Bounds = new Rectangle(540, 20, 270, 180) };
            lblProps = new Label
            {
                Bounds = new Rectangle(15, 25, 240, 140),
                Text = "Size: --\nDuration: --\nSample Rate: --\nChannels: --\nBit Rate: --\nEncoding: --",
                Font = new Font("Consolas", 10)
            };
            grpProps.Controls.Add(lblProps);

            // --- 3. Compression Settings ---
            GroupBox grpSettings = new GroupBox { Text = "3. Compression Settings", Bounds = new Rectangle(20, 120, 500, 105) };

            Label lblAlgo = new Label { Text = "Algorithm:", Bounds = new Rectangle(20, 30, 70, 25) };
            cmbAlgorithms = new ComboBox { Bounds = new Rectangle(90, 28, 200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAlgorithms.Items.AddRange(new string[] { "DPCM", "Delta Modulation", "Adaptive Delta Modulation" });

            Label lblRate = new Label { Text = "Target Rate:", Bounds = new Rectangle(310, 30, 80, 25) };
            numSampleRate = new NumericUpDown { Bounds = new Rectangle(390, 28, 90, 25), Maximum = 48000, Minimum = 8000, Value = 44100 };

            lblQuantization = new Label { Text = "Quant. Levels:", Bounds = new Rectangle(20, 65, 80, 25) };
            numQuantization = new NumericUpDown { Bounds = new Rectangle(100, 63, 60, 25), Maximum = 65536, Minimum = 2, Value = 256 };

            lblStepSize = new Label { Text = "Step Size:", Bounds = new Rectangle(180, 65, 60, 25) };
            numStepSize = new NumericUpDown { Bounds = new Rectangle(240, 63, 60, 25), Maximum = 100, Minimum = 1, Value = 1 };

            lblAlpha = new Label { Text = "Alpha:", Bounds = new Rectangle(320, 65, 40, 25) };
            numAlpha = new NumericUpDown { Bounds = new Rectangle(360, 63, 60, 25), Maximum = 1, Minimum = 0, DecimalPlaces = 2, Increment = 0.05M, Value = 0.2M };

            cmbAlgorithms.SelectedIndexChanged += CmbAlgorithms_SelectedIndexChanged;
            cmbAlgorithms.SelectedIndex = 0;

            grpSettings.Controls.AddRange(new Control[] { lblAlgo, cmbAlgorithms, lblRate, numSampleRate, lblQuantization, numQuantization, lblStepSize, numStepSize, lblAlpha, numAlpha });

            // --- 4. Real-Time Tracking ---
            GroupBox grpTracking = new GroupBox { Text = "4. Real-Time Monitoring", Bounds = new Rectangle(20, 235, 790, 310) };
            progressBar = new ProgressBar { Bounds = new Rectangle(20, 30, 750, 25) };

            performanceChart = new Chart { Bounds = new Rectangle(20, 70, 750, 220) };
            ChartArea chartArea = new ChartArea("MainArea");
            performanceChart.ChartAreas.Add(chartArea);

            Series ratioSeries = new Series("Progress/Ratio") { ChartType = SeriesChartType.Spline, BorderWidth = 2 };
            Series speedSeries = new Series("Speed (MB/s)") { ChartType = SeriesChartType.Spline, BorderWidth = 2 };
            performanceChart.Series.Add(ratioSeries);
            performanceChart.Series.Add(speedSeries);

            grpTracking.Controls.AddRange(new Control[] { progressBar, performanceChart });

            // --- 5. Action Buttons ---
            btnCompress = new Button { Text = "Compress", Bounds = new Rectangle(400, 560, 130, 35), BackColor = Color.LightGreen };
            btnDecompress = new Button { Text = "Decompress", Bounds = new Rectangle(540, 560, 130, 35), BackColor = Color.LightSkyBlue };
            btnCancel = new Button { Text = "Cancel", Bounds = new Rectangle(680, 560, 130, 35), BackColor = Color.LightCoral };

            btnCompress.Click += BtnCompress_Click;
            btnDecompress.Click += BtnDecompress_Click;
            btnCancel.Click += BtnCancel_Click;

            this.Controls.AddRange(new Control[] { grpFile, grpProps, grpSettings, grpTracking, btnCompress, btnDecompress, btnCancel });
        }

        // ==========================================
        // UI BEHAVIOR LOGIC
        // ==========================================
        private void CmbAlgorithms_SelectedIndexChanged(object sender, EventArgs e)
        {
            string algo = cmbAlgorithms.Text;
            lblQuantization.Visible = numQuantization.Visible = (algo == "DPCM");
            lblStepSize.Visible = numStepSize.Visible = (algo == "Delta Modulation" || algo == "Adaptive Delta Modulation");
            lblAlpha.Visible = numAlpha.Visible = (algo == "Adaptive Delta Modulation");
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Audio Files|*.wav;*.mp3" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                    lblProps.Text = engine.LoadFileAndGetProperties(ofd.FileName);
                    compressionCtx.Load(ofd.FileName);
                }
            }
        }

        private void BtnPlay_Click(object sender, EventArgs e) { engine.Play(); }
        private void BtnStop_Click(object sender, EventArgs e) { engine.Stop(); }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            engine.Stop();
            progressBar.Value = 0;
            performanceChart.Series[0].Points.Clear();
            performanceChart.Series[1].Points.Clear();

            if (!string.IsNullOrEmpty(compressionCtx.OriginalFilePath))
            {
                txtFilePath.Text = compressionCtx.OriginalFilePath;
                lblProps.Text = engine.LoadFileAndGetProperties(compressionCtx.OriginalFilePath);
                MessageBox.Show("The file and settings have been successfully reset to their original values.", "Reset Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                txtFilePath.Text = files[0];
                lblProps.Text = engine.LoadFileAndGetProperties(files[0]);
                compressionCtx.Load(files[0]);
            }
        }

        // ==========================================
        // ASYNC COMPRESSION & DECOMPRESSION
        // ==========================================
        private async void BtnCompress_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                MessageBox.Show("Please select an audio file first.", "Missing File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string algorithm = cmbAlgorithms.Text;
            int targetRate = (int)numSampleRate.Value;
            int qLevels = (int)numQuantization.Value;
            int step = (int)numStepSize.Value;
            float alpha = (float)numAlpha.Value;

            btnCompress.Enabled = btnDecompress.Enabled = false;
            cts = new CancellationTokenSource();

            var progress = new Progress<(int percentage, double ratio, double speed)>(data =>
            {
                progressBar.Value = data.percentage;
                performanceChart.Series[0].Points.AddY(data.ratio);
                performanceChart.Series[1].Points.AddY(data.speed);
            });

            try
            {
                var result = await Task.Run(() =>
                {
                    return CompressionService.Compress(txtFilePath.Text, algorithm, targetRate, qLevels, step, alpha, cts.Token, progress);
                });

                if (string.IsNullOrEmpty(result.compressedPath)) return;

                txtFilePath.Text = result.compressedPath;
                ReportManager.UpdatePropertiesAfterCompression(engine, result.compressedPath, lblProps, compressionCtx.OriginalSizeBytes, result.sampleRate, result.channels, result.bitsPerSample, algorithm);
                ReportManager.ShowReport(compressionCtx.OriginalSizeBytes, result.compressedPath, result.elapsedSeconds, algorithm, targetRate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCompress.Enabled = btnDecompress.Enabled = true;
                cts?.Dispose(); cts = null;
            }
        }

        private async void BtnDecompress_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFilePath.Text) || !txtFilePath.Text.EndsWith(".wav"))
            {
                MessageBox.Show("Please select a compressed .wav file to decompress.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string algorithm = cmbAlgorithms.Text;
            int qLevels = (int)numQuantization.Value;
            int step = (int)numStepSize.Value;
            float alpha = (float)numAlpha.Value;

            btnCompress.Enabled = btnDecompress.Enabled = false;
            cts = new CancellationTokenSource();

            var progress = new Progress<(int percentage, double placeholder, double speed)>(data =>
            {
                progressBar.Value = data.percentage;
                performanceChart.Series[0].Points.AddY(data.percentage);
                performanceChart.Series[1].Points.AddY(data.speed);
            });

            try
            {
                var resultPath = await Task.Run(() =>
                {
                    // Calls your new, dedicated decompression class
                    return DecompressionService.Decompress(txtFilePath.Text, algorithm, qLevels, step, alpha, cts.Token, progress);
                });

                if (string.IsNullOrEmpty(resultPath)) return;

                txtFilePath.Text = resultPath;
                lblProps.Text = engine.LoadFileAndGetProperties(resultPath);

                MessageBox.Show("Decompression completed successfully! The restored file is ready to play.", "Decompression Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Decompression failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCompress.Enabled = btnDecompress.Enabled = true;
                cts?.Dispose(); cts = null;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (cts != null && !cts.IsCancellationRequested) cts.Cancel();
            BtnReset_Click(sender, e);
        }
    }
}