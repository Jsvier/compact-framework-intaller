using System;

public abstract class BarcodeScanner : IDisposable
{
    public BarcodeScanner()
    {
        if (Initialize())
            Start();
    }

    public abstract bool Initialize();
    public abstract void Start();
    public abstract void Stop();
    public abstract void Terminate();

    // Event
    public delegate void BarcodeScanEventHandler(object sender,
        BarcodeScannerEventArgs e);
    public event BarcodeScanEventHandler BarcodeScan;
    protected virtual void OnBarcodeScan(BarcodeScannerEventArgs e)
    {
        if (BarcodeScan != null)
            BarcodeScan(this, e);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
