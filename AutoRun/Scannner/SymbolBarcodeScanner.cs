using System;
using OpenNETCF.Windows.Forms;
using Symbol;
using Symbol.Barcode;

public class SymbolBarcodeScanner : BarcodeScanner
{
    private Reader symbolReader = null;
    private ReaderData symbolReaderData = null;

    public override bool Initialize()
    {
        // If the scanner is already present, fail to initialize
        if (symbolReader != null)
            return false;

        // Create a new scanner; use the first available scanner
        symbolReader = new Reader();

        // Create the scanner data
        symbolReaderData = new ReaderData(ReaderDataTypes.Text,
            ReaderDataLengths.MaximumLabel);

        // Create the event handler delegate
        symbolReader.ReadNotify += new EventHandler(symbolReader_ReadNotify);

        // Enable the scanner with a wait cursor
        symbolReader.Actions.Enable();

        // Set up the scanner
        //symbolReader.Parameters.Feedback.Success.BeepTime = 0;
        //symbolReader.Parameters.Feedback.Success.WaveFile =Application2.StartupPath + "\\beep.wav";

        return true;
    }
    public override void Start()
    {
        // If you have both a scanner and data
        if ((symbolReader != null) && (symbolReaderData != null))
            // Submit a scan
            symbolReader.Actions.Read(symbolReaderData);
    }

    private void symbolReader_ReadNotify(object sender, EventArgs e)
    {
        if(symbolReader!=null)
        {
            ReaderData readerData = symbolReader.GetNextReaderData();

            // If successful, scan
            if (readerData.Result == Results.SUCCESS)
            {
                // Raise the scan event to the caller (with data)
                OnBarcodeScan(new BarcodeScannerEventArgs(readerData.Text));

                // Start the next scan
                Start();
            }
        }
    }
    public override void Stop()
    {
        // If you have a scanner
        if (symbolReader != null)
            // Cancel all pending scans
            symbolReader.Actions.Flush();
        Terminate();
    }

    public override void Terminate()
    {
        symbolReader = null;
        symbolReaderData = null;
    }
}