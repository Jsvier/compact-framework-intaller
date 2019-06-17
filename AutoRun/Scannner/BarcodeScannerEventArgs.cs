using System;

public class BarcodeScannerEventArgs : EventArgs
{
    public BarcodeScannerEventArgs(string data)
    {
        this.data = data;
    }
    private string data;
    public string Data
    {
        get { return data; }
    }
}
