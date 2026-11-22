using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using SSI_Lab2_Ewolucyjne.Core;

namespace SSI_Lab2_Ewolucyjne
{
    public class OknoGlowne : Form
    {
        private List<StanIteracji> historiaIteracji = new List<StanIteracji>();
        private Bitmap mapa2DCache;

        // Kontrolki UI
        private ComboBox cbAlgorytm;
        private Panel panelParam1Plus1;
        private Panel panelParamPSO;
        private Panel panelParamMuLambda;

        // Parametry 1+1
        private NumericUpDown num1D_Rozrzut;
        private NumericUpDown num1D_Wsp;
        private NumericUpDown num1D_StartMin;
        private NumericUpDown num1D_StartMax;

        // Parametry PSO
        private NumericUpDown numPSO_N;
        private NumericUpDown numPSO_Inercja;
        private NumericUpDown numPSO_Glob;
        private NumericUpDown numPSO_Lok;
        private CheckBox chkPSO_Wektory;

        // Parametry mu + lambda
        private NumericUpDown numML_Mu;
        private NumericUpDown numML_Lambda;
        private NumericUpDown numML_Turniej;
        private NumericUpDown numML_Mutacja;

        // Wspolne
        private NumericUpDown numIteracje;
        private NumericUpDown numSeed;
        private TrackBar suwakIteracji;
        private Label lblAktualnaIteracja;
        private RichTextBox txtRaport;
        private Panel panelWykresu;

        public OknoGlowne()
        {
            Text = "SSI Lab 2 - Algorytmy Ewolucyjne (1+1, PSO, Strategia mu + lambda)";
            Size = new Size(1260, 820);
            MinimumSize = new Size(1050, 700);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);

            WygenerujMape2D();
            InicjalizujKomponenty();
            UruchomObliczenia();
        }

        private void WygenerujMape2D()
        {
            int res = 250;
            mapa2DCache = new Bitmap(res, res);
            double minF = -2.4, maxF = 2.4;

            for (int py = 0; py < res; py++)
            {
                double x2 = 100.0 * (res - 1 - py) / (res - 1);
                for (int px = 0; px < res; px++)
                {
                    double x1 = 100.0 * px / (res - 1);
                    double val = FunkcjePrzystosowania.Oblicz2D(x1, x2);
                    double norm = Math.Clamp((val - minF) / (maxF - minF), 0.0, 1.0);

                    // Kwantyzacja na 12 poziomow daje efekt linii poziomicowych jak w Octave/Matlab
                    double poziom = Math.Floor(norm * 12.0) / 12.0;
                    Color c = PobierzKolorMapy(poziom);
                    mapa2DCache.SetPixel(px, py, c);
                }
            }
        }

        private Color PobierzKolorMapy(double t)
        {
            // Paleta wzorowana na Viridis z PDF (granat -> morski -> zielony -> zolty)
            int r = (int)Math.Clamp(68 + t * 185, 0, 255);
            int g = (int)Math.Clamp(1 + Math.Sin(t * Math.PI * 0.85) * 215 + t * 35, 0, 255);
            int b = (int)Math.Clamp(84 + Math.Sin(t * Math.PI) * 110 - t * 60, 0, 255);
            return Color.FromArgb(r, g, b);
        }

        private void InicjalizujKomponenty()
        {
            Panel panelLewy = new Panel
            {
                Dock = DockStyle.Left,
                Width = 420,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            // 1. Presety pod raporty z PDF
            GroupBox gbPresety = new GroupBox
            {
                Text = "Szybkie presety (Raporty i Testy z PDF)",
                Dock = DockStyle.Top,
                Height = 195,
                Padding = new Padding(8)
            };

            DodajPreset(gbPresety, "1+1: Zad. 1 i 2 (rozrzut=10, wsp=1.1, 20 iter.)", 20, () =>
            {
                cbAlgorytm.SelectedIndex = 0;
                num1D_Rozrzut.Value = 10; num1D_Wsp.Value = 1.1M;
                num1D_StartMin.Value = 0; num1D_StartMax.Value = 100;
                numIteracje.Value = 20;
                UruchomObliczenia(5);
            });

            DodajPreset(gbPresety, "1+1: Zad. 3 (start [15;35], rozrzut=5, 20 iter.)", 48, () =>
            {
                cbAlgorytm.SelectedIndex = 0;
                num1D_Rozrzut.Value = 5; num1D_Wsp.Value = 1.1M;
                num1D_StartMin.Value = 15; num1D_StartMax.Value = 35;
                numIteracje.Value = 20;
                UruchomObliczenia(20);
            });

            DodajPreset(gbPresety, "PSO: Raport 1 (N=4, r_glob=1, r_inercji=0, r_lok=0)", 76, () =>
            {
                cbAlgorytm.SelectedIndex = 1;
                numPSO_N.Value = 4; numPSO_Glob.Value = 1.0M; numPSO_Inercja.Value = 0.0M; numPSO_Lok.Value = 0.0M;
                numIteracje.Value = 10; numSeed.Value = 123;
                UruchomObliczenia(5);
            });

            DodajPreset(gbPresety, "PSO: Raport 2 (N=4, r_glob=1, r_inercji=0.7, ten sam start)", 104, () =>
            {
                cbAlgorytm.SelectedIndex = 1;
                numPSO_N.Value = 4; numPSO_Glob.Value = 1.0M; numPSO_Inercja.Value = 0.7M; numPSO_Lok.Value = 0.0M;
                numIteracje.Value = 10; numSeed.Value = 123;
                UruchomObliczenia(5);
            });

            DodajPreset(gbPresety, "PSO: Raport 3 (N=20, r_glob=0.05, r_inercji=0, r_lok=0.8)", 132, () =>
            {
                cbAlgorytm.SelectedIndex = 1;
                numPSO_N.Value = 20; numPSO_Glob.Value = 0.05M; numPSO_Inercja.Value = 0.0M; numPSO_Lok.Value = 0.8M;
                numIteracje.Value = 15;
                UruchomObliczenia(13);
            });

            DodajPreset(gbPresety, "mu+lambda: Raport 1 (mu=4, lambda=3, turniej=2, mut=3)", 160, () =>
            {
                cbAlgorytm.SelectedIndex = 2;
                numML_Mu.Value = 4; numML_Lambda.Value = 3; numML_Turniej.Value = 2; numML_Mutacja.Value = 3;
                numIteracje.Value = 15;
                UruchomObliczenia(13);
            });

            // 2. Parametry algorytmu
            GroupBox gbParametry = new GroupBox
            {
                Text = "Konfiguracja algorytmu",
                Dock = DockStyle.Top,
                Height = 235,
                Padding = new Padding(10)
            };

            DodajEtykiete(gbParametry, "Algorytm:", 12, 24);
            cbAlgorytm = new ComboBox { Left = 135, Top = 21, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cbAlgorytm.Items.AddRange(new object[]
            {
                "1. Algorytm 1+1 (Problem 1D)",
                "2. Rój cząstek PSO (Problem 2D)",
                "3. Strategia mu + lambda (Problem 2D)"
            });
            cbAlgorytm.SelectedIndex = 1;
            cbAlgorytm.SelectedIndexChanged += (s, e) =>
            {
                AktualizujWidocznoscPaneli();
                UruchomObliczenia();
            };
            gbParametry.Controls.Add(cbAlgorytm);

            // Podpanel 1+1
            panelParam1Plus1 = new Panel { Left = 10, Top = 52, Width = 380, Height = 105, Visible = false };
            DodajEtykiete(panelParam1Plus1, "Rozrzut pocz.:", 2, 6);
            num1D_Rozrzut = StworzNum(125, 3, 70, 0.1M, 50, 10, 1);
            DodajEtykiete(panelParam1Plus1, "Wsp. przyrostu:", 205, 6);
            num1D_Wsp = StworzNum(310, 3, 65, 1.01M, 3.0M, 1.1M, 2);
            DodajEtykiete(panelParam1Plus1, "Start X min:", 2, 38);
            num1D_StartMin = StworzNum(125, 35, 70, 0, 100, 0, 0);
            DodajEtykiete(panelParam1Plus1, "Start X max:", 205, 38);
            num1D_StartMax = StworzNum(310, 35, 65, 0, 100, 100, 0);
            panelParam1Plus1.Controls.AddRange(new Control[] { num1D_Rozrzut, num1D_Wsp, num1D_StartMin, num1D_StartMax });

            // Podpanel PSO
            panelParamPSO = new Panel { Left = 10, Top = 52, Width = 380, Height = 105, Visible = true };
            DodajEtykiete(panelParamPSO, "Liczba cząstek N:", 2, 6);
            numPSO_N = StworzNum(125, 3, 65, 2, 100, 4, 0);
            DodajEtykiete(panelParamPSO, "r_inercji:", 205, 6);
            numPSO_Inercja = StworzNum(305, 3, 70, 0, 2, 0.2M, 2);
            DodajEtykiete(panelParamPSO, "r_glob:", 2, 36);
            numPSO_Glob = StworzNum(125, 33, 65, 0, 2, 0.3M, 2);
            DodajEtykiete(panelParamPSO, "r_lok:", 205, 36);
            numPSO_Lok = StworzNum(305, 33, 70, 0, 2, 0.3M, 2);
            chkPSO_Wektory = new CheckBox { Text = "Rysuj wektory przyciągania do X_glob", Left = 2, Top = 68, Width = 350, Checked = true };
            chkPSO_Wektory.CheckedChanged += (s, e) => panelWykresu.Invalidate();
            panelParamPSO.Controls.AddRange(new Control[] { numPSO_N, numPSO_Inercja, numPSO_Glob, numPSO_Lok, chkPSO_Wektory });

            // Podpanel mu + lambda
            panelParamMuLambda = new Panel { Left = 10, Top = 52, Width = 380, Height = 105, Visible = false };
            DodajEtykiete(panelParamMuLambda, "Rodzice (mu):", 2, 6);
            numML_Mu = StworzNum(125, 3, 65, 1, 50, 4, 0);
            DodajEtykiete(panelParamMuLambda, "Potomkowie (λ):", 205, 6);
            numML_Lambda = StworzNum(310, 3, 65, 1, 100, 10, 0);
            DodajEtykiete(panelParamMuLambda, "Rozmiar turnieju:", 2, 38);
            numML_Turniej = StworzNum(125, 35, 65, 1, 20, 2, 0);
            DodajEtykiete(panelParamMuLambda, "Poziom mutacji:", 205, 38);
            numML_Mutacja = StworzNum(310, 35, 65, 0.1M, 50, 10, 1);
            panelParamMuLambda.Controls.AddRange(new Control[] { numML_Mu, numML_Lambda, numML_Turniej, numML_Mutacja });

            gbParametry.Controls.Add(panelParam1Plus1);
            gbParametry.Controls.Add(panelParamPSO);
            gbParametry.Controls.Add(panelParamMuLambda);

            // Wspolne: iteracje, seed i przycisk uruchomienia
            DodajEtykiete(gbParametry, "Iteracje:", 12, 165);
            numIteracje = StworzNum(75, 162, 65, 1, 200, 30, 0);
            DodajEtykiete(gbParametry, "Seed:", 150, 165);
            numSeed = StworzNum(190, 162, 65, 1, 99999, 123, 0);

            Button btnStart = new Button
            {
                Text = "Uruchom ewolucję",
                Left = 265,
                Top = 160,
                Width = 122,
                Height = 28,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnStart.Click += (s, e) => UruchomObliczenia();

            Button btnLosujSeed = new Button
            {
                Text = "Nowe losowanie (zmień Seed)",
                Left = 12,
                Top = 195,
                Width = 375,
                Height = 28,
                BackColor = Color.White
            };
            btnLosujSeed.Click += (s, e) =>
            {
                numSeed.Value = new Random().Next(1, 99999);
                UruchomObliczenia();
            };

            gbParametry.Controls.AddRange(new Control[] { numIteracje, numSeed, btnStart, btnLosujSeed });

            // 3. Suwak iteracji
            GroupBox gbSuwak = new GroupBox
            {
                Text = "Oś czasu ewolucji (krok po kroku)",
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(8)
            };

            lblAktualnaIteracja = new Label
            {
                Text = "Iteracja: 0 / 30",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 22
            };

            suwakIteracji = new TrackBar
            {
                Dock = DockStyle.Top,
                Minimum = 0,
                Maximum = 30,
                Value = 30,
                TickFrequency = 1
            };
            suwakIteracji.Scroll += (s, e) => AktualizujWidokIteracji();

            gbSuwak.Controls.Add(suwakIteracji);
            gbSuwak.Controls.Add(lblAktualnaIteracja);

            // 4. Raport tekstowy
            GroupBox gbRaport = new GroupBox
            {
                Text = "Dane do raportu dla wybranej iteracji",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            txtRaport = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.2F),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            gbRaport.Controls.Add(txtRaport);

            panelLewy.Controls.Add(gbRaport);
            panelLewy.Controls.Add(gbSuwak);
            panelLewy.Controls.Add(gbParametry);
            panelLewy.Controls.Add(gbPresety);

            // Prawy panel wykresu
            panelWykresu = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, panelWykresu, new object[] { true });

            panelWykresu.Paint += PanelWykresu_Paint;
            panelWykresu.Resize += (s, e) => panelWykresu.Invalidate();

            Controls.Add(panelWykresu);
            Controls.Add(panelLewy);
        }

        private void DodajPreset(Control parent, string text, int top, Action akcja)
        {
            Button btn = new Button
            {
                Text = text,
                Left = 10,
                Top = top,
                Width = 375,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 8.5F)
            };
            btn.Click += (s, e) => akcja();
            parent.Controls.Add(btn);
        }

        private NumericUpDown StworzNum(int left, int top, int width, decimal min, decimal max, decimal val, int decimals)
        {
            NumericUpDown num = new NumericUpDown
            {
                Left = left,
                Top = top,
                Width = width,
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimals,
                Increment = decimals == 0 ? 1 : (decimals == 1 ? 0.5M : 0.05M),
                Value = val
            };
            num.ValueChanged += (s, e) => UruchomObliczenia();
            return num;
        }

        private void DodajEtykiete(Control parent, string tekst, int x, int y)
        {
            parent.Controls.Add(new Label { Text = tekst, Left = x, Top = y, AutoSize = true });
        }

        private void AktualizujWidocznoscPaneli()
        {
            panelParam1Plus1.Visible = cbAlgorytm.SelectedIndex == 0;
            panelParamPSO.Visible = cbAlgorytm.SelectedIndex == 1;
            panelParamMuLambda.Visible = cbAlgorytm.SelectedIndex == 2;
        }

        private void UruchomObliczenia(int? docelowaIteracja = null)
        {
            int iters = (int)numIteracje.Value;
            int seed = (int)numSeed.Value;

            if (cbAlgorytm.SelectedIndex == 0)
            {
                Algorytm1Plus1 alg = new Algorytm1Plus1();
                historiaIteracji = alg.Wykonaj(
                    (double)num1D_Rozrzut.Value,
                    (double)num1D_Wsp.Value,
                    iters,
                    (double)num1D_StartMin.Value,
                    (double)num1D_StartMax.Value,
                    seed);
            }
            else if (cbAlgorytm.SelectedIndex == 1)
            {
                AlgorytmPSO alg = new AlgorytmPSO();
                historiaIteracji = alg.Wykonaj(
                    (int)numPSO_N.Value,
                    (double)numPSO_Inercja.Value,
                    (double)numPSO_Glob.Value,
                    (double)numPSO_Lok.Value,
                    iters,
                    seed);
            }
            else
            {
                AlgorytmMuPlusLambda alg = new AlgorytmMuPlusLambda();
                historiaIteracji = alg.Wykonaj(
                    (int)numML_Mu.Value,
                    (int)numML_Lambda.Value,
                    (int)numML_Turniej.Value,
                    (double)numML_Mutacja.Value,
                    iters,
                    seed);
            }

            suwakIteracji.Maximum = iters;
            suwakIteracji.Value = docelowaIteracja.HasValue ? Math.Min(docelowaIteracja.Value, iters) : iters;
            AktualizujWidokIteracji();
        }

        private void AktualizujWidokIteracji()
        {
            if (historiaIteracji == null || historiaIteracji.Count == 0) return;

            int idx = suwakIteracji.Value;
            StanIteracji stan = historiaIteracji[idx];

            string info = idx == 0 ? " (Stan początkowy przed ewolucją)" : "";
            lblAktualnaIteracja.Text = $"Iteracja: {stan.NumerIteracji} / {suwakIteracji.Maximum}{info}";
            txtRaport.Text = stan.TekstRaportu;
            panelWykresu.Invalidate();
        }

        private void PanelWykresu_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (cbAlgorytm.SelectedIndex == 0)
            {
                RysujWykres1D(g);
            }
            else
            {
                RysujWykres2D(g);
            }
        }

        private void RysujWykres1D(Graphics g)
        {
            int mL = 65, mP = 30, mG = 45, mD = 55;
            int szer = panelWykresu.Width - mL - mP;
            int wys = panelWykresu.Height - mG - mD;
            if (szer <= 50 || wys <= 50) return;

            double minX = 0, maxX = 100;
            double minY = -0.4, maxY = 0.45;

            Func<double, float> NaX = x => (float)(mL + (x - minX) / (maxX - minX) * szer);
            Func<double, float> NaY = y => (float)(mG + wys - (y - minY) / (maxY - minY) * wys);

            // Siatka
            using (Pen pSiatka = new Pen(Color.FromArgb(230, 230, 230)) { DashStyle = DashStyle.Dash })
            using (Font f = new Font("Segoe UI", 8.5f))
            {
                for (int x = 0; x <= 100; x += 10)
                {
                    float px = NaX(x);
                    g.DrawLine(pSiatka, px, mG, px, mG + wys);
                    g.DrawString(x.ToString(), f, Brushes.DimGray, px - 10, mG + wys + 6);
                }
                for (double y = -0.4; y <= 0.41; y += 0.1)
                {
                    float py = NaY(y);
                    g.DrawLine(pSiatka, mL, py, mL + szer, py);
                    g.DrawString(y.ToString("0.0"), f, Brushes.DimGray, mL - 38, py - 7);
                }
                // Os Y = 0
                g.DrawLine(Pens.Gray, mL, NaY(0), mL + szer, NaY(0));
                g.DrawRectangle(Pens.Black, mL, mG, szer, wys);
            }

            // Krzywa funkcji f(x) = sin(x/10)*sin(x/200)
            List<PointF> punktyFunkcji = new List<PointF>();
            for (double x = 0; x <= 100.0; x += 0.25)
            {
                punktyFunkcji.Add(new PointF(NaX(x), NaY(FunkcjePrzystosowania.Oblicz1D(x))));
            }
            using (Pen pFunkcja = new Pen(Color.FromArgb(37, 99, 235), 2.2f))
            {
                g.DrawLines(pFunkcja, punktyFunkcji.ToArray());
            }

            if (historiaIteracji.Count == 0) return;
            StanIteracji stan = historiaIteracji[suwakIteracji.Value];

            // Zakres rozrzutu [x - rozrzut; x + rozrzut]
            float xLewy = NaX(Math.Max(0, stan.X - stan.Rozrzut));
            float xPrawy = NaX(Math.Min(100, stan.X + stan.Rozrzut));
            using (Brush bZakres = new SolidBrush(Color.FromArgb(35, 220, 38, 38)))
            {
                g.FillRectangle(bZakres, xLewy, mG, Math.Max(2, xPrawy - xLewy), wys);
            }

            // Punkt kandydata x_pot (szare kolko)
            if (stan.NumerIteracji > 0)
            {
                float kpx = NaX(stan.Xpot);
                float kpy = NaY(stan.Ypot);
                g.DrawEllipse(new Pen(Color.DarkOrange, 2f), kpx - 5, kpy - 5, 10, 10);
            }

            // Aktualny najlepszy punkt (x, y) - czerwony punkt
            float cx = NaX(stan.X);
            float cy = NaY(stan.Y);
            g.FillEllipse(Brushes.Red, cx - 6, cy - 6, 12, 12);
            g.DrawEllipse(new Pen(Color.DarkRed, 1.5f), cx - 6, cy - 6, 12, 12);

            using (Font fBold = new Font("Segoe UI", 10f, FontStyle.Bold))
            {
                g.DrawString($"f(x) = sin(x/10)*sin(x/200)   |   x = {stan.X:F3}, y = {stan.Y:F4}, rozrzut = {stan.Rozrzut:F3}",
                    fBold, Brushes.Black, mL, 14);
            }
        }

        private void RysujWykres2D(Graphics g)
        {
            int mL = 60, mP = 30, mG = 45, mD = 55;
            int bok = Math.Min(panelWykresu.Width - mL - mP, panelWykresu.Height - mG - mD);
            if (bok <= 50) return;

            Func<double, float> NaX = x => (float)(mL + (x / 100.0) * bok);
            Func<double, float> NaY = y => (float)(mG + bok - (y / 100.0) * bok);

            // Tlo mapy warstwicowej funkcji F(x1, x2)
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.DrawImage(mapa2DCache, mL, mG, bok, bok);
            g.DrawRectangle(Pens.Black, mL, mG, bok, bok);

            using (Font f = new Font("Segoe UI", 8.5f))
            {
                for (int v = 0; v <= 100; v += 20)
                {
                    g.DrawString(v.ToString(), f, Brushes.Black, NaX(v) - 10, mG + bok + 6);
                    g.DrawString(v.ToString(), f, Brushes.Black, mL - 32, NaY(v) - 7);
                }
            }

            if (historiaIteracji.Count == 0) return;
            StanIteracji stan = historiaIteracji[suwakIteracji.Value];
            bool czyPSO = cbAlgorytm.SelectedIndex == 1;

            if (czyPSO)
            {
                float gx = NaX(stan.NajlepszyOsobnik.X1);
                float gy = NaY(stan.NajlepszyOsobnik.X2);

                // Przerywane linie przyciagania do optimum globalnego (jak na str. 14 PDF)
                if (chkPSO_Wektory.Checked)
                {
                    using (Pen pGlob = new Pen(Color.FromArgb(180, 220, 38, 38), 1.3f) { DashStyle = DashStyle.Dot })
                    {
                        foreach (var os in stan.PulaGlowna)
                        {
                            g.DrawLine(pGlob, NaX(os.X1), NaY(os.X2), gx, gy);
                        }
                    }
                }

                // Czastki jako niebieskie kwadraty
                using (Pen pCzastka = new Pen(Color.Blue, 2.2f))
                using (Brush bCzastka = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    foreach (var os in stan.PulaGlowna)
                    {
                        float px = NaX(os.X1);
                        float py = NaY(os.X2);
                        g.FillRectangle(bCzastka, px - 5, py - 5, 10, 10);
                        g.DrawRectangle(pCzastka, px - 5, py - 5, 10, 10);
                    }
                }

                // Najlepszy globalnie jako czerwona gwiazdka
                RysujGwiazdke(g, gx, gy, Color.Red);
            }
            else
            {
                // Strategia mu + lambda: pula potomna jako czerwone okregi (jak na str. 18 PDF)
                using (Pen pPot = new Pen(Color.Red, 2f))
                using (Brush bPot = new SolidBrush(Color.FromArgb(170, Color.White)))
                {
                    foreach (var os in stan.PulaPotomna)
                    {
                        float px = NaX(os.X1);
                        float py = NaY(os.X2);
                        g.FillEllipse(bPot, px - 5, py - 5, 10, 10);
                        g.DrawEllipse(pPot, px - 5, py - 5, 10, 10);
                    }
                }

                // Pula rodzicielska (mu) jako czerwone gwiazdki
                foreach (var os in stan.PulaGlowna)
                {
                    RysujGwiazdke(g, NaX(os.X1), NaY(os.X2), Color.DarkRed);
                }
            }

            using (Font fBold = new Font("Segoe UI", 10f, FontStyle.Bold))
            {
                string opis = czyPSO
                    ? $"PSO | Najlepszy wynik: {stan.NajlepszyOsobnik.Ocena:F5} w [{stan.NajlepszyOsobnik.X1:F2}; {stan.NajlepszyOsobnik.X2:F2}]"
                    : $"Strategia mu+lambda | Najlepszy wynik: {stan.NajlepszyOsobnik.Ocena:F5}  (* rodzice, o potomkowie)";
                g.DrawString(opis, fBold, Brushes.Black, mL, 14);
            }
        }

        private void RysujGwiazdke(Graphics g, float sx, float sy, Color kolor)
        {
            using (Pen pBialy = new Pen(Color.White, 4f))
            using (Pen pKolor = new Pen(kolor, 2.2f))
            {
                int r = 7;
                g.DrawLine(pBialy, sx - r, sy, sx + r, sy);
                g.DrawLine(pBialy, sx, sy - r, sx, sy + r);
                g.DrawLine(pBialy, sx - 5, sy - 5, sx + 5, sy + 5);
                g.DrawLine(pBialy, sx - 5, sy + 5, sx + 5, sy - 5);

                g.DrawLine(pKolor, sx - r, sy, sx + r, sy);
                g.DrawLine(pKolor, sx, sy - r, sx, sy + r);
                g.DrawLine(pKolor, sx - 5, sy - 5, sx + 5, sy + 5);
                g.DrawLine(pKolor, sx - 5, sy + 5, sx + 5, sy - 5);
            }
        }
    }
}