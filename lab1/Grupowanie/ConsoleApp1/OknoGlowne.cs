using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SSI_Lab1_Grupowanie.Core;

namespace SSI_Lab1_Grupowanie
{
    public class OknoGlowne : Form
    {
        private DaneProbek dane = new DaneProbek();
        private List<StanIteracji> historiaIteracji = new List<StanIteracji>();

        // Kontrolki UI
        private ComboBox cbAlgorytm;
        private ComboBox cbMetryka;
        private NumericUpDown numGrupy;
        private NumericUpDown numIteracje;
        private NumericUpDown numFcmM;
        private NumericUpDown numSeed;
        private TrackBar suwakIteracji;
        private Label lblAktualnaIteracja;
        private RichTextBox txtRaport;
        private Panel panelWykresu;
        private CheckBox chkMieszajKoloryFCM;

        private readonly Color[] paletaKolorow = new Color[]
        {
            Color.FromArgb(220, 38, 38),   // Czerwony
            Color.FromArgb(22, 163, 74),   // Zielony
            Color.FromArgb(37, 99, 235),   // Niebieski
            Color.FromArgb(217, 119, 6),   // Pomaranczowy
            Color.FromArgb(147, 51, 234),  // Fioletowy
            Color.FromArgb(8, 145, 178),   // Cyjan
            Color.FromArgb(219, 39, 119),  // Rozowy
            Color.FromArgb(101, 163, 13)   // Limonkowy
        };

        public OknoGlowne()
        {
            Text = "SSI Lab 1 - Grupowanie (K-Średnich oraz Fuzzy C-Means)";
            Size = new Size(1220, 780);
            MinimumSize = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);

            InicjalizujKomponenty();
            WczytajDomyslnyPlik();
        }

        private void InicjalizujKomponenty()
        {
            Panel panelLewy = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            GroupBox gbPresety = new GroupBox
            {
                Text = "Szybkie presety z instrukcji PDF",
                Dock = DockStyle.Top,
                Height = 115,
                Padding = new Padding(8)
            };

            Button btnZad1 = StworzPrzycisk("Zadanie 1: K-Średnie (Euklidesowa, 3 gr., 10 iter.)", 22);
            btnZad1.Click += (s, e) => UstawPreset(0, 0, 3, 10, 2.0, 4);

            Button btnZad2 = StworzPrzycisk("Zadanie 2: K-Średnie (Tylko X1, 4 gr., 10 iter.)", 52);
            btnZad2.Click += (s, e) => UstawPreset(0, 1, 4, 10, 2.0, 10);

            Button btnZad3 = StworzPrzycisk("Zadanie 3 (bdb): FCM (Euklidesowa, 3 gr., 20 iter.)", 82);
            btnZad3.Click += (s, e) => UstawPreset(1, 0, 3, 20, 2.0, 20);

            gbPresety.Controls.Add(btnZad1);
            gbPresety.Controls.Add(btnZad2);
            gbPresety.Controls.Add(btnZad3);

            GroupBox gbParametry = new GroupBox
            {
                Text = "Parametry algorytmu",
                Dock = DockStyle.Top,
                Height = 255,
                Padding = new Padding(10)
            };

            int y = 25;
            DodajEtykiete(gbParametry, "Algorytm:", 12, y);
            cbAlgorytm = new ComboBox { Left = 155, Top = y - 3, Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };
            cbAlgorytm.Items.AddRange(new object[] { "K-Średnich (K-Means)", "Fuzzy C-Means (FCM)" });
            cbAlgorytm.SelectedIndex = 0;
            cbAlgorytm.SelectedIndexChanged += (s, e) =>
            {
                bool czyFcm = cbAlgorytm.SelectedIndex == 1;
                numFcmM.Enabled = czyFcm;
                chkMieszajKoloryFCM.Enabled = czyFcm;
                UruchomObliczenia();
            };
            gbParametry.Controls.Add(cbAlgorytm);

            y += 32;
            DodajEtykiete(gbParametry, "Miara odległości:", 12, y);
            cbMetryka = new ComboBox { Left = 155, Top = y - 3, Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };
            cbMetryka.Items.AddRange(new object[]
            {
                "Euklidesowa",
                "Tylko parametr X1 (|A1-B1|)",
                "Manhattan (taksówkowa)",
                "Czebyszewa",
                "Minkowskiego (p=3)"
            });
            cbMetryka.SelectedIndex = 0;
            cbMetryka.SelectedIndexChanged += (s, e) => UruchomObliczenia();
            gbParametry.Controls.Add(cbMetryka);

            y += 32;
            DodajEtykiete(gbParametry, "Liczba grup (m):", 12, y);
            numGrupy = new NumericUpDown { Left = 155, Top = y - 3, Width = 80, Minimum = 2, Maximum = 8, Value = 3 };
            numGrupy.ValueChanged += (s, e) => UruchomObliczenia();
            gbParametry.Controls.Add(numGrupy);

            y += 32;
            DodajEtykiete(gbParametry, "Liczba iteracji:", 12, y);
            numIteracje = new NumericUpDown { Left = 155, Top = y - 3, Width = 80, Minimum = 1, Maximum = 200, Value = 10 };
            numIteracje.ValueChanged += (s, e) => UruchomObliczenia();
            gbParametry.Controls.Add(numIteracje);

            y += 32;
            DodajEtykiete(gbParametry, "Parametr fcm_m:", 12, y);
            numFcmM = new NumericUpDown { Left = 155, Top = y - 3, Width = 80, Minimum = 1.1M, Maximum = 5.0M, DecimalPlaces = 1, Increment = 0.1M, Value = 2.0M, Enabled = false };
            numFcmM.ValueChanged += (s, e) => UruchomObliczenia();
            gbParametry.Controls.Add(numFcmM);

            DodajEtykiete(gbParametry, "Seed:", 245, y);
            numSeed = new NumericUpDown { Left = 290, Top = y - 3, Width = 75, Minimum = 1, Maximum = 9999, Value = 123 };
            numSeed.ValueChanged += (s, e) => UruchomObliczenia();
            gbParametry.Controls.Add(numSeed);

            y += 32;
            chkMieszajKoloryFCM = new CheckBox
            {
                Text = "Mieszaj kolory RGB wg przynależności U (FCM)",
                Left = 12,
                Top = y,
                Width = 350,
                Checked = true,
                Enabled = false
            };
            chkMieszajKoloryFCM.CheckedChanged += (s, e) => panelWykresu.Invalidate();
            gbParametry.Controls.Add(chkMieszajKoloryFCM);

            y += 30;
            Button btnWczytajCsv = new Button
            {
                Text = "Wczytaj inny plik CSV...",
                Left = 12,
                Top = y,
                Width = 170,
                Height = 30,
                BackColor = Color.White
            };
            btnWczytajCsv.Click += BtnWczytajCsv_Click;
            gbParametry.Controls.Add(btnWczytajCsv);

            Button btnPrzelicz = new Button
            {
                Text = "Przelicz ponownie",
                Left = 195,
                Top = y,
                Width = 170,
                Height = 30,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPrzelicz.Click += (s, e) => UruchomObliczenia();
            gbParametry.Controls.Add(btnPrzelicz);

            GroupBox gbSuwak = new GroupBox
            {
                Text = "Podgląd krok po kroku (oś czasu iteracji)",
                Dock = DockStyle.Top,
                Height = 95,
                Padding = new Padding(10)
            };

            lblAktualnaIteracja = new Label
            {
                Text = "Iteracja: 10 / 10",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 22
            };

            suwakIteracji = new TrackBar
            {
                Dock = DockStyle.Top,
                Minimum = 0,
                Maximum = 10,
                Value = 10,
                TickFrequency = 1
            };
            suwakIteracji.Scroll += (s, e) => AktualizujWidokIteracji();

            gbSuwak.Controls.Add(suwakIteracji);
            gbSuwak.Controls.Add(lblAktualnaIteracja);

            GroupBox gbRaport = new GroupBox
            {
                Text = "Wyniki szczegółowe (Raport dla wybranej iteracji)",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            txtRaport = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.5F),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            gbRaport.Controls.Add(txtRaport);

            panelLewy.Controls.Add(gbRaport);
            panelLewy.Controls.Add(gbSuwak);
            panelLewy.Controls.Add(gbParametry);
            panelLewy.Controls.Add(gbPresety);

            panelWykresu = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, panelWykresu, new object[] { true });

            panelWykresu.Paint += PanelWykresu_Paint;
            panelWykresu.Resize += (s, e) => panelWykresu.Invalidate();

            Controls.Add(panelWykresu);
            Controls.Add(panelLewy);
        }

        private Button StworzPrzycisk(string tekst, int top)
        {
            return new Button
            {
                Text = tekst,
                Left = 10,
                Top = top,
                Width = 355,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.White
            };
        }

        private void DodajEtykiete(Control rodzic, string tekst, int x, int y)
        {
            rodzic.Controls.Add(new Label { Text = tekst, Left = x, Top = y, AutoSize = true });
        }

        private void WczytajDomyslnyPlik()
        {
            string sciezka = "spiralka.csv";
            if (!File.Exists(sciezka))
            {
                string alternatywna = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\spiralka.csv");
                if (File.Exists(alternatywna)) sciezka = alternatywna;
            }

            if (dane.WczytajZPliku(sciezka))
            {
                UruchomObliczenia();
            }
            else
            {
                MessageBox.Show("Nie znaleziono pliku spiralka.csv! Użyj przycisku 'Wczytaj inny plik CSV...', aby wskazać plik.",
                    "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnWczytajCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Pliki CSV (*.csv)|*.csv|Wszystkie pliki (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (dane.WczytajZPliku(ofd.FileName))
                    {
                        UruchomObliczenia();
                    }
                }
            }
        }

        private void UstawPreset(int algorytmIdx, int metrykaIdx, int grupy, int iteracje, double fcmM, int docelowaIteracjaSuwaka)
        {
            cbAlgorytm.SelectedIndex = algorytmIdx;
            cbMetryka.SelectedIndex = metrykaIdx;
            numGrupy.Value = grupy;
            numIteracje.Value = iteracje;
            numFcmM.Value = (decimal)fcmM;

            UruchomObliczenia();

            if (docelowaIteracjaSuwaka <= suwakIteracji.Maximum)
            {
                suwakIteracji.Value = docelowaIteracjaSuwaka;
                AktualizujWidokIteracji();
            }
        }

        private void UruchomObliczenia()
        {
            if (dane.M == 0) return;

            int m = (int)numGrupy.Value;
            int iters = (int)numIteracje.Value;
            TypMetryki metryka = (TypMetryki)cbMetryka.SelectedIndex;
            int seed = (int)numSeed.Value;

            try
            {
                IAlgorytmGrupowania algorytm;
                if (cbAlgorytm.SelectedIndex == 0)
                {
                    algorytm = new AlgorytmKMeans();
                }
                else
                {
                    algorytm = new AlgorytmFCM { ParametrRozmycia = (double)numFcmM.Value };
                }

                historiaIteracji = algorytm.Wykonaj(dane, m, iters, metryka, seed);

                suwakIteracji.Maximum = iters;
                suwakIteracji.Value = iters;

                AktualizujWidokIteracji();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd obliczeń: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AktualizujWidokIteracji()
        {
            if (historiaIteracji == null || historiaIteracji.Count == 0) return;

            int idx = suwakIteracji.Value;
            StanIteracji stan = historiaIteracji[idx];

            string dopisek = idx == 0 ? " (Stan początkowy przed pętlą)" : "";
            lblAktualnaIteracja.Text = $"Iteracja: {stan.NumerIteracji} / {suwakIteracji.Maximum}{dopisek}";
            txtRaport.Text = stan.TekstRaportu;

            panelWykresu.Invalidate();
        }

        private void PanelWykresu_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = panelWykresu.Width;
            int h = panelWykresu.Height;

            int marginesLewy = 65;
            int marginesPrawy = 35;
            int marginesGora = 45;
            int marginesDol = 60;

            int szerWykresu = w - marginesLewy - marginesPrawy;
            int wysWykresu = h - marginesGora - marginesDol;

            if (szerWykresu <= 50 || wysWykresu <= 50) return;

            double minX = -2.0, maxX = 1.6;
            double minY = -2.0, maxY = 2.5;

            if (dane.M > 0)
            {
                for (int i = 0; i < dane.M; i++)
                {
                    if (dane.probkiX1[i] < minX) minX = dane.probkiX1[i] - 0.2;
                    if (dane.probkiX1[i] > maxX) maxX = dane.probkiX1[i] + 0.2;
                    if (dane.probkiX2[i] < minY) minY = dane.probkiX2[i] - 0.2;
                    if (dane.probkiX2[i] > maxY) maxY = dane.probkiX2[i] + 0.2;
                }
            }

            Func<double, float> NaEkranX = (x) => (float)(marginesLewy + (x - minX) / (maxX - minX) * szerWykresu);
            Func<double, float> NaEkranY = (y) => (float)(marginesGora + wysWykresu - (y - minY) / (maxY - minY) * wysWykresu);

            using (Pen pioroSiatki = new Pen(Color.FromArgb(230, 230, 230), 1) { DashStyle = DashStyle.Dash })
            using (Pen pioroRamki = new Pen(Color.Black, 1.5f))
            using (Font fontOsi = new Font("Segoe UI", 8.5f))
            using (Brush pedzelTekstu = new SolidBrush(Color.FromArgb(60, 60, 60)))
            {
                for (double x = Math.Ceiling(minX * 2) / 2.0; x <= maxX; x += 0.5)
                {
                    float px = NaEkranX(x);
                    g.DrawLine(pioroSiatki, px, marginesGora, px, marginesGora + wysWykresu);
                    g.DrawString(x.ToString("0.0"), fontOsi, pedzelTekstu, px - 12, marginesGora + wysWykresu + 6);
                }

                for (double y = Math.Ceiling(minY * 2) / 2.0; y <= maxY; y += 0.5)
                {
                    float py = NaEkranY(y);
                    g.DrawLine(pioroSiatki, marginesLewy, py, marginesLewy + szerWykresu, py);
                    g.DrawString(y.ToString("0.0"), fontOsi, pedzelTekstu, marginesLewy - 38, py - 7);
                }

                g.DrawRectangle(pioroRamki, marginesLewy, marginesGora, szerWykresu, wysWykresu);

                using (Font fontNaglowka = new Font("Segoe UI", 10f, FontStyle.Bold))
                {
                    g.DrawString("atrybut 1 (x1)", fontNaglowka, Brushes.Black, marginesLewy + szerWykresu / 2 - 45, h - 28);
                    g.DrawString("atrybut 2 (x2)", fontNaglowka, Brushes.Black, 10, 12);
                }
            }

            if (historiaIteracji == null || historiaIteracji.Count == 0 || dane.M == 0) return;

            StanIteracji stan = historiaIteracji[suwakIteracji.Value];
            int m = stan.SrodkiX1.Length;
            bool czyFcm = (cbAlgorytm.SelectedIndex == 1) && (stan.U != null);

            for (int s = 0; s < dane.M; s++)
            {
                float px = NaEkranX(dane.probkiX1[s]);
                float py = NaEkranY(dane.probkiX2[s]);

                Color kolorPunktu;
                if (czyFcm && chkMieszajKoloryFCM.Checked)
                {
                    double r = 0, gr = 0, b = 0;
                    for (int j = 0; j < m; j++)
                    {
                        Color bazowy = paletaKolorow[j % paletaKolorow.Length];
                        double u = stan.U[j, s];
                        r += bazowy.R * u;
                        gr += bazowy.G * u;
                        b += bazowy.B * u;
                    }
                    kolorPunktu = Color.FromArgb(
                        Math.Clamp((int)r, 0, 255),
                        Math.Clamp((int)gr, 0, 255),
                        Math.Clamp((int)b, 0, 255));
                }
                else
                {
                    int grupa = stan.PrzypisanaGrupa[s];
                    kolorPunktu = paletaKolorow[grupa % paletaKolorow.Length];
                }

                using (Pen pioroKw = new Pen(kolorPunktu, 2.0f))
                using (Brush wypelnienie = new SolidBrush(Color.FromArgb(45, kolorPunktu)))
                {
                    g.FillRectangle(wypelnienie, px - 5, py - 5, 10, 10);
                    g.DrawRectangle(pioroKw, px - 5, py - 5, 10, 10);
                }
            }

            for (int j = 0; j < m; j++)
            {
                float sx = NaEkranX(stan.SrodkiX1[j]);
                float sy = NaEkranY(stan.SrodkiX2[j]);
                Color kolorSrodka = paletaKolorow[j % paletaKolorow.Length];

                using (Pen pioroBialeTlo = new Pen(Color.White, 4.5f))
                using (Pen pioroGwiazdki = new Pen(kolorSrodka, 2.5f))
                {
                    int r = 8;
                    g.DrawLine(pioroBialeTlo, sx - r, sy, sx + r, sy);
                    g.DrawLine(pioroBialeTlo, sx, sy - r, sx, sy + r);
                    g.DrawLine(pioroBialeTlo, sx - 6, sy - 6, sx + 6, sy + 6);
                    g.DrawLine(pioroBialeTlo, sx - 6, sy + 6, sx + 6, sy - 6);

                    g.DrawLine(pioroGwiazdki, sx - r, sy, sx + r, sy);
                    g.DrawLine(pioroGwiazdki, sx, sy - r, sx, sy + r);
                    g.DrawLine(pioroGwiazdki, sx - 6, sy - 6, sx + 6, sy + 6);
                    g.DrawLine(pioroGwiazdki, sx - 6, sy + 6, sx + 6, sy - 6);
                }
            }

            int legX = marginesLewy + 15;
            int legY = marginesGora + 15;
            using (Brush tloLegendy = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
            using (Pen ramkaLegendy = new Pen(Color.LightGray))
            using (Font fontLegendy = new Font("Segoe UI", 8.5f))
            {
                g.FillRectangle(tloLegendy, legX, legY, 155, 25 + m * 22);
                g.DrawRectangle(ramkaLegendy, legX, legY, 155, 25 + m * 22);

                for (int j = 0; j < m; j++)
                {
                    Color c = paletaKolorow[j % paletaKolorow.Length];
                    int iy = legY + 10 + j * 20;
                    using (Pen p = new Pen(c, 2f))
                    {
                        g.DrawRectangle(p, legX + 10, iy, 10, 10);
                    }
                    g.DrawString($"Grupa {j + 1} + Środek V{j + 1}", fontLegendy, Brushes.Black, legX + 28, iy - 3);
                }
            }
        }
    }
}