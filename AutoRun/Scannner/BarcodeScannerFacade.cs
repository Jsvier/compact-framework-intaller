using System.Runtime.InteropServices;
using MbsControls.Footer.Scannner;

public class BarcodeScannerFacade
{
    [DllImport("coredll.dll")]
    private static extern int SystemParametersInfo(int uiAction, int uiParam, string pvParam, int fWinIni);
    private const int SPI_GETOEMINFO = 258;

    public static BarcodeScanner GetBarcodeScanner()
    {
            BarcodeScannerFactory BarcodeScannerFactory = null;
            BarcodeScanner BarcodeScanner = null;

            string oemInfo = GetOemInfo();

            // Is this a Symbol device?
            if (oemInfo.ToUpper().IndexOf("MOTOROLA") > -1)
                BarcodeScannerFactory = new SymbolBarcodeScannerFactory();

            // Is this an Intermec device?
            if (oemInfo.ToUpper().IndexOf("UNITECH") > -1)
                BarcodeScannerFactory = new UnitechBarcodeScannerFactory();

            // Is this an Intermec device?
            if (oemInfo.ToUpper().IndexOf("DATALOGIC") > -1)
                BarcodeScannerFactory = new DatalogicBarcodeScannerFactory();

            // Create a generic bar code reader object
            if (BarcodeScannerFactory != null)
                BarcodeScanner = BarcodeScannerFactory.GetBarcodeScanner();

            return BarcodeScanner;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private static string GetOemInfo()
    {
        string oemInfo = new string(' ', 50);
        int result = SystemParametersInfo(SPI_GETOEMINFO, 50, oemInfo, 0);
        oemInfo = oemInfo.Substring(0, oemInfo.IndexOf('\0'));
        return oemInfo;
    }
}