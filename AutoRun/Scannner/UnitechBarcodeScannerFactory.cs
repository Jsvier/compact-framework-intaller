namespace MbsControls.Footer.Scannner
{
    public class UnitechBarcodeScannerFactory : BarcodeScannerFactory
    {
        public override BarcodeScanner GetBarcodeScanner()
        {
            return new UnitechBarcodeScanner();
        }
    }
}