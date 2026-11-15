using System.Collections.Generic;

namespace SSI_Lab2_Ewolucyjne.Core
{
    public class StanIteracji
    {
        public int NumerIteracji;
        public string TekstRaportu;

        // 1+1
        public double X;
        public double Y;
        public double Xpot;
        public double Ypot;
        public double Rozrzut;

        // PSO
        public List<Osobnik> PulaGlowna = new List<Osobnik>();
        public List<Osobnik> PulaPotomna = new List<Osobnik>();
        public Osobnik NajlepszyOsobnik;

    }
}