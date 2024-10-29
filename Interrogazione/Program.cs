using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace RandomStudentPicker
{
    public partial class MainForm : Form
    {
        private readonly Random random = new Random();
        private readonly HashSet<int> usedNumbers = new HashSet<int>();
        private readonly List<string> students = new List<string>
        {
            "Mario", "Luigi", "Anna", "Giulia", "Marco", "Sofia", "Matteo", "Luca", "Sara", "Francesco",
            "Alessandro", "Chiara", "Giovanni", "Elena", "Martina", "Davide", "Giorgia", "Nicola", "Federica", "Simone",
            "Carla", "Giovanni", "Valentina", "Emanuele", "Vittoria", "Stefano", "Rosa", "Pietro", "Simona", "Cristina"
        };

        private Timer blinkTimer;
        private Timer loadingTimer;
        private Timer fadeTimer;
        private int blinkCounter;
        private int loadingCounter;
        private float currentOpacity = 0;
        private bool isFadingIn = true;

        // UI Controls
        private Button btnPickStudent;
        private Label lblTitle, lblInstructions, lblResult, lblLoading;
        private Panel mainPanel;
        private PictureBox progressBar;
        private Label lblProgress;

        // Colori personalizzati
        private static readonly Color PrimaryColor = Color.FromArgb(63, 81, 181);    // Material Indigo
        private static readonly Color AccentColor = Color.FromArgb(255, 64, 129);    // Material Pink
        private static readonly Color BackgroundColor = Color.FromArgb(250, 250, 250);
        private static readonly Color TextColor = Color.FromArgb(33, 33, 33);
        private static readonly Color SecondaryTextColor = Color.FromArgb(117, 117, 117);

        public MainForm()
        {
            InitializeComponent();
            InitializeTimers();
            this.DoubleBuffered = true;
        }

        private void InitializeTimers()
        {
            blinkTimer = new Timer { Interval = 200 };
            loadingTimer = new Timer { Interval = 50 };
            fadeTimer = new Timer { Interval = 30 };

            blinkTimer.Tick += (s, e) => HandleBlinkEffect();
            loadingTimer.Tick += (s, e) => HandleLoadingEffect();
            fadeTimer.Tick += (s, e) => HandleFadeEffect();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            InitializeFormProperties();
            CreateMainPanel();
            CreateControls();
            LayoutControls();
            ResumeLayout(false);
        }

        private void InitializeFormProperties()
        {
            ClientSize = new Size(1000, 700);
            Text = "Estrazione Studente";
            BackColor = BackgroundColor;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void CreateMainPanel()
        {
            mainPanel = new Panel
            {
                Size = new Size(800, 600),
                BackColor = Color.White,
                AutoSize = false
            };

            mainPanel.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    mainPanel.ClientRectangle,
                    Color.FromArgb(5, PrimaryColor),
                    Color.FromArgb(15, PrimaryColor),
                    90F))
                {
                    e.Graphics.FillRectangle(brush, mainPanel.ClientRectangle);
                }
            };

            Controls.Add(mainPanel);
            mainPanel.Location = new Point(
                (ClientSize.Width - mainPanel.Width) / 2,
                (ClientSize.Height - mainPanel.Height) / 2
            );
        }

        private void CreateControls()
        {
            // Creazione e configurazione del titolo
            lblTitle = CreateStyledLabel("Estrazione Studente", new Font("Bell MT UI Light", 32, FontStyle.Regular), TextColor);
            lblTitle.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var shadow = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    e.Graphics.TranslateTransform(2, 2);
                    e.Graphics.DrawString(lblTitle.Text, lblTitle.Font, shadow, 0, 0);
                    e.Graphics.ResetTransform();
                }
                e.Graphics.DrawString(lblTitle.Text, lblTitle.Font, new SolidBrush(lblTitle.ForeColor), 0, 0);
            };

            // Creazione e configurazione delle istruzioni
            lblInstructions = CreateStyledLabel("Premi il pulsante per estrarre uno studente",
                new Font("Segoe UI", 14, FontStyle.Regular), SecondaryTextColor);

            // Creazione e configurazione del pulsante
            btnPickStudent = new Button
            {
                Text = "Estrai Studente",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.White,
                BackColor = PrimaryColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(250, 50),
                Cursor = Cursors.Hand
            };
            btnPickStudent.FlatAppearance.BorderSize = 0;
            btnPickStudent.Click += BtnPickStudent_Click;

            // Effetto hover sul pulsante
            btnPickStudent.MouseEnter += (s, e) => btnPickStudent.BackColor = ControlPaint.Light(PrimaryColor);
            btnPickStudent.MouseLeave += (s, e) => btnPickStudent.BackColor = PrimaryColor;

            // Progress Bar personalizzata
            progressBar = new PictureBox
            {
                Size = new Size(400, 4),
                BackColor = Color.FromArgb(224, 224, 224),
                Visible = false
            };

            // Label per il risultato
            lblResult = CreateStyledLabel("", new Font("Segoe UI Light", 24, FontStyle.Regular), TextColor);
            lblResult.Visible = false;

            // Label per il caricamento
            lblLoading = CreateStyledLabel("Estrazione in corso...",
                new Font("Segoe UI", 16, FontStyle.Regular), SecondaryTextColor);
            lblLoading.Visible = false;

            // Label per il progresso
            lblProgress = CreateStyledLabel("Studenti estratti: 0/" + students.Count,
                new Font("Segoe UI", 12, FontStyle.Regular), SecondaryTextColor);

            // Aggiunta dei controlli al panel principale
            mainPanel.Controls.AddRange(new Control[]
            {
                lblTitle, lblInstructions, btnPickStudent,
                progressBar, lblResult, lblLoading, lblProgress
            });
        }

        private Label CreateStyledLabel(string text, Font font, Color foreColor)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = foreColor,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
        }

        private void LayoutControls()
        {
            Load += (s, e) =>
            {
                int centerX = mainPanel.Width / 2;
                int currentY = 50;

                // Posizionamento dei controlli
                lblTitle.Location = new Point(centerX - lblTitle.Width / 2, currentY);
                currentY += lblTitle.Height + 40;

                lblInstructions.Location = new Point(centerX - lblInstructions.Width / 2, currentY);
                currentY += lblInstructions.Height + 40;

                btnPickStudent.Location = new Point(centerX - btnPickStudent.Width / 2, currentY);
                currentY += btnPickStudent.Height + 40;

                progressBar.Location = new Point(centerX - progressBar.Width / 2, currentY);
                currentY += progressBar.Height + 20;

                lblLoading.Location = new Point(centerX - lblLoading.Width / 2, currentY);
                currentY += lblLoading.Height + 40;

                lblResult.Location = new Point(centerX - lblResult.Width / 2, currentY);

                lblProgress.Location = new Point(
                    mainPanel.Width - lblProgress.Width - 20,
                    mainPanel.Height - lblProgress.Height - 20
                );
            };
        }

        private void BtnPickStudent_Click(object sender, EventArgs e)
        {
            if (usedNumbers.Count == students.Count)
            {
                MessageBox.Show("Tutti gli studenti sono stati interrogati!", "Completato",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            StartSelectionAnimation();
        }

        private void StartSelectionAnimation()
        {
            btnPickStudent.Enabled = false;
            lblResult.Visible = false;
            lblLoading.Visible = true;
            progressBar.Visible = true;
            loadingCounter = 0;
            loadingTimer.Start();
            currentOpacity = 0;
            isFadingIn = true;
        }

        private void HandleLoadingEffect()
        {
            loadingCounter++;
            double progress = loadingCounter / 40.0; // 2 secondi totali (40 * 50ms)

            if (progress <= 1)
            {
                // Aggiorna la barra di progresso
                progressBar.Invalidate();
                progressBar.CreateGraphics().FillRectangle(
                    new SolidBrush(AccentColor),
                    0, 0, (float)(progressBar.Width * progress), progressBar.Height
                );
            }
            else
            {
                loadingTimer.Stop();
                progressBar.Visible = false;
                lblLoading.Visible = false;
                SelectAndDisplayStudent();
                btnPickStudent.Enabled = true;
            }
        }

        private void SelectAndDisplayStudent()
        {
            var availableNumbers = Enumerable.Range(1, students.Count)
                                           .Except(usedNumbers)
                                           .ToList();
            var selectedNumber = availableNumbers[random.Next(availableNumbers.Count)];
            usedNumbers.Add(selectedNumber);

            lblResult.Text = $"Studente estratto:\n{students[selectedNumber - 1]}\n(N° {selectedNumber})";
            lblResult.ForeColor = PrimaryColor;
            lblResult.Visible = true;
            lblResult.Location = new Point(
                (mainPanel.Width - lblResult.Width) / 2,
                lblResult.Location.Y
            );

            // Aggiorna il conteggio degli studenti estratti
            lblProgress.Text = $"Studenti estratti: {usedNumbers.Count}/{students.Count}";

            fadeTimer.Start();
            blinkCounter = 0;
            blinkTimer.Start();
        }

        private void HandleFadeEffect()
        {
            if (isFadingIn)
            {
                currentOpacity += 0.1f;
                if (currentOpacity >= 1)
                {
                    currentOpacity = 1;
                    isFadingIn = false;
                    fadeTimer.Stop();
                }
            }
            lblResult.ForeColor = Color.FromArgb(
                (int)(currentOpacity * 255),
                PrimaryColor
            );
        }

        private void HandleBlinkEffect()
        {
            if (++blinkCounter >= 6)
            {
                blinkTimer.Stop();
                lblResult.ForeColor = PrimaryColor;
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}