namespace MbsControls.Footer.Scannner
{
    public class DatalogicBarcodeScannerFactory : BarcodeScannerFactory
    {
        public override BarcodeScanner GetBarcodeScanner()
        {
            return new DatalogicBarcodeScanner();
        }
    }
}