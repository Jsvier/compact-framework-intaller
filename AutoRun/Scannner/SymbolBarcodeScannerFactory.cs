public class SymbolBarcodeScannerFactory : BarcodeScannerFactory
{
    public override BarcodeScanner GetBarcodeScanner()
    {
        return new SymbolBarcodeScanner();
    }
}