using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.Xml;

namespace SSI_Lab1_Grupowanie.Core
{
    public class DaneProbek
    {
        public double[] probkiX1;
        public double[] probkiX2;
        public int M; // liczba probek z CSVki

        public bool WczytajZPliku(string sicezkaCSV)
        {
            if (!File.Exists(sicezkaCSV)) { return false; }

            string[] wszystkieLinie = File.ReadAllLines(sicezkaCSV);
            List<double> ListaX1 = new List<double>();
            List<double> ListaX2 = new List<double>();

            for (int i = 0; i < wszystkieLinie.Length; i++)
            {
                string linia = wszystkieLinie[i].Trim();
                if (linia.Length == 0) continue;

                string[] czesci = linia.Split(new char[] { ',', ';' });

                double x1 = double.Parse(czesci[0].Trim(), CultureInfo.InvariantCulture);  // `CultureInfo.InvariantCulture` to robi, że kropka ozancza liczby zmiennoprzecinkowe :D
                double x2 = double.Parse(czesci[1].Trim(), CultureInfo.InvariantCulture);  

                ListaX1.Add(x1);
                ListaX2.Add(x2);
            }

            M = ListaX1.Count;
            probkiX1 = ListaX1.ToArray();
            probkiX2 = ListaX2.ToArray();
            return true;

        }
    }
}