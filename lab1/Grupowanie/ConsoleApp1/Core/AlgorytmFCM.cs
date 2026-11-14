using System;
using System.Collections.Generic;
using System.IO;

namespace SSI_Lab1_Grupowanie.Core
{
    public class AlgorytmFCM : IAlgorytmGrupowania
    {
        public double ParametrRozmycia { get; set; } = 2.0;

        public List<StanIteracji> Wykonaj(DaneProbek dane, int m, int iters, TypMetryki metryka, int seed = 123)
        {
            List<StanIteracji> historia = new List<StanIteracji>();
            Random los = new Random(seed);

            int M = dane.M;
            double fcm_m = ParametrRozmycia;

            double[,] U = new double[m, M];
            double[,] D = new double[m, M];
            double[] srodkiX1 = new double[m];
            double[] srodkiX2 = new double[m];

            for (int j = 0; j < m; j++)
            {
                for (int s = 0; s < M; s++)
                {
                    D[j, s] = 0.1 + (los.NextDouble() * 1.0);
                }
            }

            PrzeliczPrzynaleznosciU(U, D, m, M, fcm_m);
            PrzeliczSrodkiFCM(srodkiX1, srodkiX2, dane, U, m, M, fcm_m);

            historia.Add(ZbudujStan(0, dane, srodkiX1, srodkiX2, U, m));

            for (int iteracja = 1; iteracja <= iters; iteracja++)
            {
                for (int j = 0; j < m; j++)
                {
                    for (int s = 0; s < M; s++)
                    {
                        double d = MiaryOdleglosci.Policz(dane.probkiX1[s], dane.probkiX2[s], srodkiX1[j], srodkiX2[j], metryka);
                        double d2 = d * d;

                        // Zabezpieczenie przed dzieleniem przez zero
                        if (d2 < 0.00001)
                        {
                            d2 = 0.00001;
                        }
                        D[j, s] = d2;
                    }
                }

                PrzeliczPrzynaleznosciU(U, D, m, M, fcm_m);

                for (int j = 0; j < m; j++)
                {
                    for (int s = 0; s < M; s++)
                    {
                        if (double.IsNaN(U[j, s]) || double.IsInfinity(U[j, s]))
                        {
                            throw new Exception("Wykryto wartosci nieoznaczone (NaN) w tablicy U!");
                        }
                    }
                }

                PrzeliczSrodkiFCM(srodkiX1, srodkiX2, dane, U, m, M, fcm_m);

                historia.Add(ZbudujStan(iteracja, dane, srodkiX1, srodkiX2, U, m));
            }

            return historia;
        }

        private void PrzeliczPrzynaleznosciU(double[,] U, double[,] D, int m, int M, double fcm_m)
        {
            double wykladnik = 1.0 / (1.0 - fcm_m);

            for (int s = 0; s < M; s++)
            {
                double sumaMianownika = 0.0;
                for (int j = 0; j < m; j++)
                {
                    sumaMianownika += Math.Pow(D[j, s], wykladnik);
                }

                for (int j = 0; j < m; j++)
                {
                    U[j, s] = Math.Pow(D[j, s], wykladnik) / sumaMianownika;
                }
            }
        }

        private void PrzeliczSrodkiFCM(double[] srodkiX1, double[] srodkiX2, DaneProbek dane, double[,] U, int m, int M, double fcm_m)
        {
            for (int j = 0; j < m; j++)
            {
                double licznikX1 = 0.0;
                double licznikX2 = 0.0;
                double mianownik = 0.0;

                for (int s = 0; s < M; s++)
                {
                    double waga = Math.Pow(U[j, s], fcm_m);
                    licznikX1 += waga * dane.probkiX1[s];
                    licznikX2 += waga * dane.probkiX2[s];
                    mianownik += waga;
                }

                srodkiX1[j] = licznikX1 / mianownik;
                srodkiX2[j] = licznikX2 / mianownik;
            }
        }

        private StanIteracji ZbudujStan(int iteracja, DaneProbek dane, double[] srodkiX1, double[] srodkiX2, double[,] U, int m)
        {
            StanIteracji stan = new StanIteracji();
            stan.NumerIteracji = iteracja;
            stan.SrodkiX1 = (double[])srodkiX1.Clone();
            stan.SrodkiX2 = (double[])srodkiX2.Clone();
            stan.U = (double[,])U.Clone();
            stan.PrzypisanaGrupa = new int[dane.M];

            for (int s = 0; s < dane.M; s++)
            {
                double maxU = -1.0;
                int bestJ = 0;
                for (int j = 0; j < m; j++)
                {
                    if (U[j, s] > maxU)
                    {
                        maxU = U[j, s];
                        bestJ = j;
                    }
                }
                stan.PrzypisanaGrupa[s] = bestJ;
            }

            StringWriter raport = new StringWriter();
            raport.WriteLine($"=== FCM: STAN PO {iteracja} ITERACJACH ===");

            for (int j = 0; j < m; j++)
            {
                int liczbaPrzynaleznychPowyzej06 = 0;
                double minX1 = double.MaxValue, maxX1 = double.MinValue;
                double minX2 = double.MaxValue, maxX2 = double.MinValue;

                for (int s = 0; s < dane.M; s++)
                {
                    if (U[j, s] > 0.6)
                    {
                        liczbaPrzynaleznychPowyzej06++;
                        if (dane.probkiX1[s] < minX1) minX1 = dane.probkiX1[s];
                        if (dane.probkiX1[s] > maxX1) maxX1 = dane.probkiX1[s];
                        if (dane.probkiX2[s] < minX2) minX2 = dane.probkiX2[s];
                        if (dane.probkiX2[s] > maxX2) maxX2 = dane.probkiX2[s];
                    }
                }

                raport.WriteLine($"\nGrupa {j + 1}:");
                raport.WriteLine($"  Srodek grupy:       (x1 = {srodkiX1[j]:F4}, x2 = {srodkiX2[j]:F4})");
                raport.WriteLine($"  Probki z U > 0.6:   {liczbaPrzynaleznychPowyzej06}");
                if (liczbaPrzynaleznychPowyzej06 > 0)
                {
                    raport.WriteLine($"  Wartosci param. x1: min = {minX1:F4}, max = {maxX1:F4}");
                    raport.WriteLine($"  Wartosci param. x2: min = {minX2:F4}, max = {maxX2:F4}");
                }
            }

            stan.TekstRaportu = raport.ToString();
            return stan;
        }
    }
}