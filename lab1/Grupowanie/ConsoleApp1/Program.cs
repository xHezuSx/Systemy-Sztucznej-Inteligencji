using System;
using System.Windows.Forms;

namespace SSI_Lab1_Grupowanie
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new OknoGlowne());
        }
    }
}