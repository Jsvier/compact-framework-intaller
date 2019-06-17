namespace MbsControls.Footer.Scannner
{
    using System.Windows.Forms;
    using USICF;

    public static class Unitech
    {
        private static USIClass _reader;

        public static USIClass Reader
        {
            get
            {
                if (_reader == null)
                {
                    _reader = new USIClass(new Form());
                }
                return _reader;
            }
        }
        //   _unitechReader = new USIClass(new Form()) 
    }
    public class UnitechBarcodeScanner : BarcodeScanner
    {


        public override bool Initialize()
        {

                USIClass.PromptMessage = false;
                // But we want to show USI error popup windows
                USIClass.ErrorMessage = true;


                Unitech.Reader.DataReady += symbolReader_ReadNotify;
                //Reader.ErrorEvent += new USIClass.ErrorEventHandler(myUSI_ErrorEvent);

                Unitech.Reader.SetWorkingMode(USIClass.WorkingMode.SWM_BARCODE);	// barcode mode

                return true;
        }
        public override void Start()
        {

            // If you have both a scanner and data
            Unitech.Reader.EnableScanner(true);
        }

        private void symbolReader_ReadNotify(object sender, USIEventArgs e)
        {
            // Raise the scan event to the caller (with data)
            OnBarcodeScan(new BarcodeScannerEventArgs(e.BarcodeData.Replace("\r", "")));
        }
        public override void Stop()
        {
            // If you have both a scanner and data
            Unitech.Reader.EnableScanner(false);
            Unitech.Reader.DataReady -= symbolReader_ReadNotify;
        }

        public override void Terminate()
        {

        }
    }
}