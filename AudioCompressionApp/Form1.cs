using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AudioCompressionApp
{
    public partial class Form1 : Form
    {
        // Our Audio Engine instance
        private AudioEngine engine = new AudioEngine();

        // UI Controls Declaration
        private TextBox txtFilePath;
        private Button btnBrowse, btnPlay, btnStop, btnCompress, btnCancel;
        private ComboBox cmbAlgorithms;
        private NumericUpDown numSampleRate;
        private Label lblProps;
        private ProgressBar progressBar;
        private Chart performanceChart;

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

            // --- 1. File Selection & Playback Zone ---
            GroupBox grpFile = new GroupBox { Text = "1. File Input & Preview", Bounds = new Rectangle(20, 20, 500, 80) };
            txtFilePath = new TextBox { Bounds = new Rectangle(20, 30, 350, 25), ReadOnly = true };
            btnBrowse = new Button { Text = "Browse", Bounds = new Rectangle(380, 28, 100, 25) };
            btnPlay = new Button { Text = "▶ Play", Bounds = new Rectangle(20, 60, 80, 25) };
            btnStop = new Button { Text = "■ Stop", Bounds = new Rectangle(110, 60, 80, 25) };

            btnBrowse.Click += BtnBrowse_Click;
            btnPlay.Click += BtnPlay_Click;
            btnStop.Click += BtnStop_Click; // Added Stop wiring

            grpFile.Controls.AddRange(new Control[] { txtFilePath, btnBrowse, btnPlay, btnStop });

            // --- 2. Audio Properties Zone ---
            GroupBox grpProps = new GroupBox { Text = "2. File Properties", Bounds = new Rectangle(540, 20, 270, 180) };
            lblProps = new Label
            {
                Bounds = new Rectangle(15, 25, 240, 140),
                Text = "Size: --\nDuration: --\nSample Rate: --\nChannels: --\nBit Rate: --\nEncoding: --",
                Font = new Font("Consolas", 10)
            };
            grpProps.Controls.Add(lblProps);

            // --- 3. Compression Settings Zone ---
            GroupBox grpSettings = new GroupBox { Text = "3. Compression Settings", Bounds = new Rectangle(20, 120, 500, 80) };
            Label lblAlgo = new Label { Text = "Algorithm:", Bounds = new Rectangle(20, 30, 70, 25) };
            cmbAlgorithms = new ComboBox { Bounds = new Rectangle(90, 28, 200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAlgorithms.Items.AddRange(new string[] { "Nonlinear Quantization", "DPCM", "Predictive Coding", "Delta Modulation" });
            cmbAlgorithms.SelectedIndex = 0;

            Label lblRate = new Label { Text = "Target Rate:", Bounds = new Rectangle(310, 30, 80, 25) };
            numSampleRate = new NumericUpDown { Bounds = new Rectangle(390, 28, 90, 25), Maximum = 48000, Minimum = 8000, Value = 44100 };

            grpSettings.Controls.AddRange(new Control[] { lblAlgo, cmbAlgorithms, lblRate, numSampleRate });

            // --- 4. Real-Time Tracking Zone ---
            GroupBox grpTracking = new GroupBox { Text = "4. Real-Time Monitoring", Bounds = new Rectangle(20, 220, 790, 320) };
            progressBar = new ProgressBar { Bounds = new Rectangle(20, 30, 750, 25) };

            // Set up Chart
            performanceChart = new Chart { Bounds = new Rectangle(20, 70, 750, 230) };
            ChartArea chartArea = new ChartArea("MainArea");
            performanceChart.ChartAreas.Add(chartArea);

            Series ratioSeries = new Series("Compression Ratio (%)") { ChartType = SeriesChartType.Spline, BorderWidth = 2 };
            Series speedSeries = new Series("Speed (MB/s)") { ChartType = SeriesChartType.Spline, BorderWidth = 2 };
            performanceChart.Series.Add(ratioSeries);
            performanceChart.Series.Add(speedSeries);

            grpTracking.Controls.AddRange(new Control[] { progressBar, performanceChart });

            // --- 5. Action Buttons ---
            btnCompress = new Button { Text = "Start Compression", Bounds = new Rectangle(540, 560, 130, 35), BackColor = Color.LightGreen };
            btnCancel = new Button { Text = "Cancel", Bounds = new Rectangle(680, 560, 130, 35), BackColor = Color.LightCoral };

            btnCompress.Click += BtnCompress_Click;
            btnCancel.Click += BtnCancel_Click;

            // Add everything to the form
            this.Controls.AddRange(new Control[] { grpFile, grpProps, grpSettings, grpTracking, btnCompress, btnCancel });
        }

        // ==========================================
        // ACTIVE EVENT HANDLERS (Wired to AudioEngine)
        // ==========================================

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Audio Files|*.wav;*.mp3" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                    lblProps.Text = engine.LoadFileAndGetProperties(ofd.FileName);
                }
            }
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            engine.Play();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            engine.Stop();
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
            }
        }

        // ==========================================
        // PENDING EVENT HANDLERS (For the rest of the team)
        // ==========================================

        private void BtnCompress_Click(object sender, EventArgs e)
        {
            // TODO: 'Multithreading Specialist' triggers BackgroundWorker here
            // TODO: 'Algorithm Engineers' execute selected algorithm from cmbAlgorithms.Text
            MessageBox.Show($"Starting {cmbAlgorithms.Text} at {numSampleRate.Value} Hz...");
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // TODO: 'Multithreading Specialist' triggers cancellation here
            MessageBox.Show("Operation Cancelled. Resetting file...");
        }
    }
}