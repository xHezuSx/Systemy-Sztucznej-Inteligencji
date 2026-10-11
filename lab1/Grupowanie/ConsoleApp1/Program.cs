using System;
using System.IO;
using System.Globalization;

namespace SSI_Lab1_Grupowanie
{
    class Program
    {
        // ---------------------------------------------------------
        // FUNKCJA GŁÓWNA (MAIN)
        // ---------------------------------------------------------
        static void Main(string[] args)
        {
            string sciezkaCsv = "spiralka.csv";

            // 1. Sprawdzamy czy plik istnieje
            if (!File.Exists(sciezkaCsv))
            {
                Console.WriteLine("BŁĄD: Nie znaleziono pliku " + sciezkaCsv);
                Console.WriteLine("Umiesc go w folderze z plikiem .exe lub w projekcie!");
                Console.WriteLine("Wcisnij dowolny klawisz, aby zamknac...");
                Console.ReadKey();
                return;
            }

            // 2. Wczytujemy wszystkie linie z pliku
            string[] wszystkieLinie = File.ReadAllLines(sciezkaCsv);
            int M = wszystkieLinie.Length; // Liczba próbek (w PDF oznaczone jako M)

            // Tworzymy tablice na dane: 2 kolumny (x1 i x2)
            double[] probkiX1 = new double[M];
            double[] probkiX2 = new double[M];

            for (int i = 0; i < M; i++)
            {
                string linia = wszystkieLinie[i].Trim();
                if (linia.Length == 0) continue;

                // Rozdzielamy po przecinku lub sredniku
                string[] czesci = linia.Split(new char[] { ',', ';' });

                // CultureInfo.InvariantCulture dba o to, by kropka "." byla separatorem dziesietnym
                probkiX1[i] = double.Parse(czesci[0].Trim(), CultureInfo.InvariantCulture);
                probkiX2[i] = double.Parse(czesci[1].Trim(), CultureInfo.InvariantCulture);
            }

            Console.WriteLine("Wczytano " + M + " probek z bazy spiralka.csv.");

            // Przygotowujemy plik raportu do zapisu
            StreamWriter raport = new StreamWriter("raport_wyniki.txt");

            // =========================================================================
            // ZADANIE 1: K-SREDNIE (metryka euklidesowa, 3 grupy, 10 iteracji)
            // =========================================================================
            Console.WriteLine("\n[1/3] Liczenie Zadania 1: K-srednie (metryka euklidesowa)...");
            raport.WriteLine("==================================================================");
            raport.WriteLine("RAPORT 1: K-SREDNIE (Euklidesowa, 3 grupy, 10 iteracji)");
            raport.WriteLine("==================================================================");
            WykonajKMeans(probkiX1, probkiX2, M, 3, 10, false, "zadanie1", raport);

            // =========================================================================
            // ZADANIE 2: K-SREDNIE (metryka tylko po X1, 4 grupy, 10 iteracji)
            // =========================================================================
            Console.WriteLine("[2/3] Liczenie Zadania 2: K-srednie (tylko parametr X1)...");
            raport.WriteLine("\n==================================================================");
            raport.WriteLine("RAPORT 2: K-SREDNIE (tylko parametr X1, 4 grupy, 10 iteracji)");
            raport.WriteLine("==================================================================");
            WykonajKMeans(probkiX1, probkiX2, M, 4, 10, true, "zadanie2", raport);

            // =========================================================================
            // ZADANIE 3: FUZZY C-MEANS (FCM) - na ocene bdb (3 grupy, 20 iteracji)
            // =========================================================================
            Console.WriteLine("[3/3] Liczenie Zadania 3: FCM (na ocene bardzo dobra)...");
            raport.WriteLine("\n==================================================================");
            raport.WriteLine("RAPORT 3: FUZZY C-MEANS (FCM) (3 grupy, 20 iteracji, fcm_m = 2)");
            raport.WriteLine("==================================================================");
            WykonajFCM(probkiX1, probkiX2, M, 3, 20, 2.0, "zadanie3_fcm", raport);

            // Zamykamy zapis raportu
            raport.Close();

            Console.WriteLine("\nGOTOWE!");
            Console.WriteLine("Wszystkie wyniki zostaly zapisane do pliku: raport_wyniki.txt");
            Console.WriteLine("Wygenerowano rowniez pliki .csv z danymi do wykresow.");
            Console.WriteLine("\nWcisnij dowolny klawisz, aby zakonczyc program...");
            Console.ReadKey();
        }

        // ---------------------------------------------------------
        // METODA DLA ZADANIA 1 I 2: K-SREDNIE
        // ---------------------------------------------------------
        static void WykonajKMeans(double[] probkiX1, double[] probkiX2, int M, int m, int iters, bool tylkoX1, string prefiksCsv, StreamWriter raport)
        {
            Random los = new Random(123); // stale ziarno, zeby wyniki byly powtarzalne

            // Tablice przechowujace wspolrzedne srodkow grup (V)
            double[] srodkiX1 = new double[m];
            double[] srodkiX2 = new double[m];

            // Krok 1 z PDF: losujemy m roznych probek i ustawiamy jako srodki poczatkowe
            for (int j = 0; j < m; j++)
            {
                int losowyIndeks = los.Next(0, M);
                srodkiX1[j] = probkiX1[losowyIndeks];
                srodkiX2[j] = probkiX2[losowyIndeks];
            }

            // Tablica przypisan: pod indeksem 's' przechowuje numer grupy dla danej probki
            int[] przypisanaGrupa = new int[M];

            // Krok 2 z PDF: glowna petla iteracji
            for (int iteracja = 1; iteracja <= iters; iteracja++)
            {
                // 2.1 Dla kazdej probki wyznaczamy najblizszy srodek
                for (int s = 0; s < M; s++)
                {
                    double minOdleglosc = double.MaxValue;
                    int najlepszaGrupa = 0;

                    for (int j = 0; j < m; j++)
                    {
                        double odl = 0.0;
                        if (tylkoX1)
                        {
                            // Zadanie 2: metryka odleglosci = |pktA_1 - pktB_1|
                            odl = Math.Abs(probkiX1[s] - srodkiX1[j]);
                        }
                        else
                        {
                            // Zadanie 1: metryka euklidesowa
                            double dx = probkiX1[s] - srodkiX1[j];
                            double dy = probkiX2[s] - srodkiX2[j];
                            odl = Math.Sqrt(dx * dx + dy * dy);
                        }

                        if (odl < minOdleglosc)
                        {
                            minOdleglosc = odl;
                            najlepszaGrupa = j;
                        }
                    }
                    przypisanaGrupa[s] = najlepszaGrupa;
                }

                // 2.2 Wyznaczamy nowe srodki grup jako srednie arytmetyczne probek
                for (int j = 0; j < m; j++)
                {
                    double sumaX1 = 0.0;
                    double sumaX2 = 0.0;
                    int licznik = 0;

                    for (int s = 0; s < M; s++)
                    {
                        if (przypisanaGrupa[s] == j)
                        {
                            sumaX1 += probkiX1[s];
                            sumaX2 += probkiX2[s];
                            licznik++;
                        }
                    }

                    // Jesli do grupy wpadly jakies punkty, liczymy nowa srednia
                    if (licznik > 0)
                    {
                        srodkiX1[j] = sumaX1 / licznik;
                        srodkiX2[j] = sumaX2 / licznik;
                    }
                }

                // Sprawdzamy czy to iteracja wymagana w PDF (dla k-srednich: iteracja 4 i 10)
                if (iteracja == 4 || iteracja == iters)
                {
                    raport.WriteLine("\n--- WYNIKI PO " + iteracja + " ITERACJACH ---");

                    for (int j = 0; j < m; j++)
                    {
                        int liczbaPunktow = 0;
                        double minX1 = double.MaxValue;
                        double maxX1 = double.MinValue;
                        double minX2 = double.MaxValue;
                        double maxX2 = double.MinValue;

                        for (int s = 0; s < M; s++)
                        {
                            if (przypisanaGrupa[s] == j)
                            {
                                liczbaPunktow++;
                                if (probkiX1[s] < minX1) minX1 = probkiX1[s];
                                if (probkiX1[s] > maxX1) maxX1 = probkiX1[s];
                                if (probkiX2[s] < minX2) minX2 = probkiX2[s];
                                if (probkiX2[s] > maxX2) maxX2 = probkiX2[s];
                            }
                        }

                        raport.WriteLine("Grupa " + (j + 1) + ":");
                        raport.WriteLine("  Srodek grupy:       (x1 = " + srodkiX1[j].ToString("F4") + ", x2 = " + srodkiX2[j].ToString("F4") + ")");
                        raport.WriteLine("  Liczba probek:      " + liczbaPunktow);
                        if (liczbaPunktow > 0)
                        {
                            raport.WriteLine("  Wartosci param. x1: min = " + minX1.ToString("F4") + ", max = " + maxX1.ToString("F4"));
                            raport.WriteLine("  Wartosci param. x2: min = " + minX2.ToString("F4") + ", max = " + maxX2.ToString("F4"));
                        }
                    }

                    // Eksport do prostego pliku CSV na potrzeby wykresu
                    ZapiszPlikWykresu(prefiksCsv + "_iter_" + iteracja + ".csv", probkiX1, probkiX2, przypisanaGrupa, M, srodkiX1, srodkiX2, m);
                }
            }
        }

        // ---------------------------------------------------------
        // METODA DLA ZADANIA 3: FUZZY C-MEANS (FCM)
        // ---------------------------------------------------------
        static void WykonajFCM(double[] probkiX1, double[] probkiX2, int M, int m, int iters, double fcm_m, string prefiksCsv, StreamWriter raport)
        {
            Random los = new Random(123);

            // Tablice o rozmiarach z PDF: U (przynaleznosci), D (odleglosci), V (srodki)
            double[,] U = new double[m, M];
            double[,] D = new double[m, M];
            double[] srodkiX1 = new double[m];
            double[] srodkiX2 = new double[m];

            // Krok 1.3 z PDF: wypelnienie D losowymi wartosciami z (0.1, 1.1)
            for (int j = 0; j < m; j++)
            {
                for (int s = 0; s < M; s++)
                {
                    D[j, s] = 0.1 + (los.NextDouble() * 1.0);
                }
            }

            // Krok 1.4: wyliczenie poczatkowego U
            PrzeliczPrzynaleznosciU(U, D, m, M, fcm_m);

            // Krok 1.5: wyliczenie poczatkowych srodkow V
            PrzeliczSrodkiFCM(srodkiX1, srodkiX2, probkiX1, probkiX2, U, m, M, fcm_m);

            // Krok 2: Glowna petla iteracji FCM
            for (int iteracja = 1; iteracja <= iters; iteracja++)
            {
                // 2.1 Obliczenie kwadratow odleglosci euklidesowych: D_j,s = d(x_s, v_j)^2
                for (int j = 0; j < m; j++)
                {
                    for (int s = 0; s < M; s++)
                    {
                        double dx = probkiX1[s] - srodkiX1[j];
                        double dy = probkiX2[s] - srodkiX2[j];
                        double d2 = (dx * dx) + (dy * dy);

                        // 2.2 Zabezpieczenie przed dzieleniem przez zero: jesli < 1e-5 to wstaw 1e-5
                        if (d2 < 0.00001)
                        {
                            d2 = 0.00001;
                        }
                        D[j, s] = d2;
                    }
                }

                // 2.3 Wyliczenie stopni przynaleznosci U
                PrzeliczPrzynaleznosciU(U, D, m, M, fcm_m);

                // 2.5 Obliczenie nowych polozen srodkow V
                PrzeliczSrodkiFCM(srodkiX1, srodkiX2, probkiX1, probkiX2, U, m, M, fcm_m);

                // Sprawdzamy stan po 4 i po 20 iteracjach (zgodnie z PDF)
                if (iteracja == 4 || iteracja == iters)
                {
                    raport.WriteLine("\n--- WYNIKI FCM PO " + iteracja + " ITERACJACH ---");

                    // Tablica do wyznaczenia dominującej grupy pod wykres
                    int[] przypisanaGrupaFCM = new int[M];

                    for (int j = 0; j < m; j++)
                    {
                        int liczbaPrzynaleznychPowyzej06 = 0;
                        double minX1 = double.MaxValue;
                        double maxX1 = double.MinValue;
                        double minX2 = double.MaxValue;
                        double maxX2 = double.MinValue;

                        for (int s = 0; s < M; s++)
                        {
                            // Wytyczna z PDF: bierzemy probki o przynaleznosci U > 0.6
                            if (U[j, s] > 0.6)
                            {
                                liczbaPrzynaleznychPowyzej06++;
                                if (probkiX1[s] < minX1) minX1 = probkiX1[s];
                                if (probkiX1[s] > maxX1) maxX1 = probkiX1[s];
                                if (probkiX2[s] < minX2) minX2 = probkiX2[s];
                                if (probkiX2[s] > maxX2) maxX2 = probkiX2[s];
                            }
                        }

                        raport.WriteLine("Grupa " + (j + 1) + ":");
                        raport.WriteLine("  Srodek grupy:       (x1 = " + srodkiX1[j].ToString("F4") + ", x2 = " + srodkiX2[j].ToString("F4") + ")");
                        raport.WriteLine("  Probki z U > 0.6:   " + liczbaPrzynaleznychPowyzej06);
                        if (liczbaPrzynaleznychPowyzej06 > 0)
                        {
                            raport.WriteLine("  Wartosci param. x1: min = " + minX1.ToString("F4") + ", max = " + maxX1.ToString("F4"));
                            raport.WriteLine("  Wartosci param. x2: min = " + minX2.ToString("F4") + ", max = " + maxX2.ToString("F4"));
                        }
                    }

                    // Na potrzeby wykresu przypisujemy punkt do grupy o najwyzszym U
                    for (int s = 0; s < M; s++)
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
                        przypisanaGrupaFCM[s] = bestJ;
                    }

                    ZapiszPlikWykresu(prefiksCsv + "_iter_" + iteracja + ".csv", probkiX1, probkiX2, przypisanaGrupaFCM, M, srodkiX1, srodkiX2, m);
                }
            }
        }

        // Pomocnicze przeliczanie U ze wzoru z PDF: D^(1/(1-fcm_m)) / Suma
        static void PrzeliczPrzynaleznosciU(double[,] U, double[,] D, int m, int M, double fcm_m)
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

        // Pomocnicze przeliczanie srodkow V ze wzoru z PDF
        static void PrzeliczSrodkiFCM(double[] srodkiX1, double[] srodkiX2, double[] probkiX1, double[] probkiX2, double[,] U, int m, int M, double fcm_m)
        {
            for (int j = 0; j < m; j++)
            {
                double licznikX1 = 0.0;
                double licznikX2 = 0.0;
                double mianownik = 0.0;

                for (int s = 0; s < M; s++)
                {
                    double waga = Math.Pow(U[j, s], fcm_m);
                    licznikX1 += waga * probkiX1[s];
                    licznikX2 += waga * probkiX2[s];
                    mianownik += waga;
                }

                srodkiX1[j] = licznikX1 / mianownik;
                srodkiX2[j] = licznikX2 / mianownik;
            }
        }

        // Zapis danych do pliku CSV (kolumny gotowe do Excela)
        static void ZapiszPlikWykresu(string nazwaPliku, double[] pX1, double[] pX2, int[] grupy, int M, double[] sX1, double[] sX2, int m)
        {
            StreamWriter sw = new StreamWriter(nazwaPliku);
            sw.WriteLine("X1;X2;Kategoria");

            // Punkty
            for (int s = 0; s < M; s++)
            {
                sw.WriteLine(pX1[s].ToString(CultureInfo.InvariantCulture) + ";" +
                             pX2[s].ToString(CultureInfo.InvariantCulture) + ";Grupa_" + (grupy[s] + 1));
            }

            // Srodki
            for (int j = 0; j < m; j++)
            {
                sw.WriteLine(sX1[j].ToString(CultureInfo.InvariantCulture) + ";" +
                             sX2[j].ToString(CultureInfo.InvariantCulture) + ";SRODEK_" + (j + 1));
            }

            sw.Close();
        }
    }
}