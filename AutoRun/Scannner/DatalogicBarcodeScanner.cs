namespace MbsControls.Footer.Scannner
{
    using System.Windows.Forms;
    using USICF;
    using Datalogic.API;

    public class DatalogicBarcodeScanner : BarcodeScanner
    {
        private DecodeEvent dcdEvent;
        private DecodeHandle hDcd;


        public override bool Initialize()
        {

             hDcd = new DecodeHandle(DecodeDeviceCap.Exists | DecodeDeviceCap.Barcode);
             Datalogic.API.DecodeRequest reqType = (DecodeRequest)1 | DecodeRequest.PostRecurring;


             // Initialize all events possible
             dcdEvent = new DecodeEvent(hDcd, reqType);
             //dcdEvent = new DecodeEvent(hDcd, reqType, this);
          //  dcdEvent.ScanPress += new DecodeScanPress(dcdEvent_ScanPress);
          //  dcdEvent.ScanStart += new DecodeScanStart(dcdEvent_ScanStart);
            dcdEvent.Scanned += new DecodeScanned(symbolReader_ReadNotify);
          //  dcdEvent.TimeOut += new DecodeTimeOut(dcdEvent_TimeOut);
           // dcdEvent.ScanStop += new DecodeScanStop(dcdEvent_ScanStop);
            dcdEvent.ScanRelease += new DecodeScanRelease(symbolReader_ReadNotify);

            return true;
        }
        public override void Start()
        {
            // Initiate a scan attempt and try to get a good read for up to 5 seconds.
            //hDcd.SoftTrigger(DecodeInputType.Barcode, 5000); ;
        }

        private void symbolReader_ReadNotify(object sender, DecodeEventArgs e)
        {
            CodeId cID = CodeId.NoData;
			string dcdData = string.Empty;

			dcdData = hDcd.ReadString(e.RequestID, ref cID);
		    
            // Raise the scan event to the caller (with data)
            OnBarcodeScan(new BarcodeScannerEventArgs(dcdData.Replace("\r", "")));
        }
        public override void Stop()
        {
            // If our instance of DcdEvent is listening to the decoder, we need to make
            // sure we tell DcdEvent to stop listening.
            if (dcdEvent.IsListening)
            {
                dcdEvent.StopScanListener();
            }

            if (hDcd != null)
            {
                hDcd.Dispose();
            }
        }

        public override void Terminate()
        {

        }
    }
}