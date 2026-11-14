using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace SSI_Lab1_Grupowanie.Core
{
    public class AlgorytmKMeans : IAlgorytmGrupowania
    {
        public List<StanIteracji> Wykonaj(DaneProbek dane, int m, int iters, TypMetryki metryka, int seed = 123)
        {
            List<StanIteracji> historia = new List<StanIteracji>();
            Random los = new Random(seed);

            int M = dane.M;
            double[] srodkiX1 = new double[m];
            double[] srodkiX2 = new double[m];

            List<int> wylosowaneIndexy = new List<int>();

            // losujemy środki 
            for(int j = 0; j < m;  j++)
            {
                int losowyIndex = los.Next(0, M);

                while(wylosowaneIndexy.Contains(losowyIndex) && wylosowaneIndexy.Count < M)
                {
                    losowyIndex = los.Next(0, M);
                }
                wylosowaneIndexy.Add(losowyIndex);

                srodkiX1[j] = dane.probkiX1[losowyIndex];
                srodkiX2[j] = dane.probkiX2[losowyIndex];
            }

            int[] przypisanaGrupa = new int[M];

            PrzypiszProbkiDoSrodkow(dane, srodkiX1, srodkiX2, przypisanaGrupa, m, metryka);
            historia.Add(ZbudujStan(0, dane, srodkiX1, srodkiX2, przypisanaGrupa, m));

            for (int iteracja = 1; iteracja <= iters; iteracja++)
            {
                PrzypiszProbkiDoSrodkow(dane, srodkiX1, srodkiX2, przypisanaGrupa, m, metryka);

                for (int j = 0; j < m; j++)
                {
                    double sumaX1 = 0.0;
                    double sumaX2 = 0.0;
                    int licznik = 0;

                    for (int s = 0; s < M; s++)
                    {
                        if (przypisanaGrupa[s] == j)
                        {
                            sumaX1 += dane.probkiX1[s];
                            sumaX2 += dane.probkiX2[s];
                            licznik++;
                        }
                    }

                    if (licznik > 0) // środki to średnie w końcu
                    {
                        srodkiX1[j] = sumaX1 / licznik;
                        srodkiX2[j] = sumaX2 / licznik;
                    }
                }

                historia.Add(ZbudujStan(iteracja, dane, srodkiX1, srodkiX2, przypisanaGrupa, m));
            }

            return historia;
        }

        private void PrzypiszProbkiDoSrodkow(DaneProbek dane, double[] srodkiX1, double[] srodkiX2, int[] przypisanaGrupa, int m, TypMetryki metryka) 
        {
            for (int s = 0; s < dane.M; s++)
            {
                double minOdleglosc = double.MaxValue;
                int najlepszaGrupa = 0;
                
                for (int j=0; j < m; j++)
                {
                    double odl = MiaryOdleglosci.Policz(dane.probkiX1[s], dane.probkiX2[s], srodkiX1[j], srodkiX2[j], metryka);
                    if (odl < minOdleglosc)
                    {
                        minOdleglosc = odl;
                        najlepszaGrupa = j;
                    }
                }
                przypisanaGrupa[s] = najlepszaGrupa;
            }
        }
        private StanIteracji ZbudujStan(int iteracja, DaneProbek dane, double[] srodkiX1, double[] srodkiX2, int[] przypisanaGrupa, int m)
        {
            StanIteracji stan = new StanIteracji();
            stan.NumerIteracji = iteracja;
            stan.SrodkiX1 = (double[])srodkiX1.Clone();
            stan.SrodkiX2 = (double[])srodkiX2.Clone();
            stan.PrzypisanaGrupa = (int[])przypisanaGrupa.Clone();

            StringWriter raport = new StringWriter();
            raport.WriteLine($"=== K-SREDNIE: STAN PO {iteracja} ITERACJACH ===");

            for (int j = 0; j<m; j++)
            {
                int liczbaPunktow = 0;
                double minX1 = double.MaxValue, maxX1 = double.MinValue;
                double minX2 = double.MaxValue, maxX2 = double.MinValue;

                for(int s = 0; s < dane.M; s++)
                {
                    if(przypisanaGrupa[s] == j)
                    {
                        liczbaPunktow++;
                        if (dane.probkiX1[s] < minX1) minX1 = dane.probkiX1[s];
                        if (dane.probkiX1[s] > maxX1) maxX1 = dane.probkiX1[s];
                        if (dane.probkiX2[s] < minX2) minX2 = dane.probkiX2[s];
                        if (dane.probkiX2[s] > maxX2) maxX2 = dane.probkiX2[s];
                    }
                }

                raport.WriteLine($"\nGrupa {j + 1}:");
                raport.WriteLine($"  Srodek grupy:       (x1 = {srodkiX1[j]:F4}, x2 = {srodkiX2[j]:F4})");
                raport.WriteLine($"  Liczba probek:      {liczbaPunktow}");
                if (liczbaPunktow > 0)
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