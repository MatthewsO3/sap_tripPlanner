using System;
using System.Windows.Forms;

namespace tripPlanner
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            TripPlanner oAddon = null;
            oAddon = new TripPlanner();

            Application.Run();

            globalD.oCompany = null;
        }
    }

    sealed class globalD
    {
        public static SAPbobsCOM.Company oCompany = null;
        public static SAPbouiCOM.Application oSBOApp = null;
    }
}