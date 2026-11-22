using SSI_Lab2_Ewolucyjne.Core;

namespace SSI_Lab2_Ewolucyjne
{
    public class Osobnik
    {
        public double X1;
        public double X2;
        public double Ocena;

        // pola doalgorytmu roju (PSO) prędkość i ptimum
        public double Vx1;
        public double Vx2;
        public double NajlepszeX1;
        public double NajlepszeX2;
        public double NajlepszaOcena;

        public Osobnik(double x1, double x2)
        {
            X1 = x1;
            X2 = x2;

            Ocena = FunkcjePrzystosowania.Oblicz2D(x1, x2);
            NajlepszaOcena = Ocena;

            NajlepszeX1 = x1;
            NajlepszeX2 = x2;

            Vx1 = 0.0;
            Vx2 = 0.0;
        }

        public Osobnik Klonuj()
        {
            Osobnik kopia = new Osobnik(X1, X2);
            kopia.Ocena = Ocena;
            kopia.Vx1 = Vx1;
            kopia.Vx2 = Vx2;
            kopia.NajlepszeX1 = NajlepszeX1;
            kopia.NajlepszeX2 = NajlepszeX2;
            kopia.NajlepszaOcena = NajlepszaOcena;

            return kopia;
        }


    }
}