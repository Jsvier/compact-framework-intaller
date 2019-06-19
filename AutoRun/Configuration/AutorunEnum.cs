//-----------------------------------------------------------------------
// <copyright file="AutorunEnum.cs" company="Makro">
//     Makro Supermayorista S.A.
// </copyright>
//-----------------------------------------------------------------------

namespace HHT_Base
{
    public class AutorunEnum
    {
        public enum RTD
        {
            ERROR = 0,
            OK = 1
        }

        public class HttpMethod
        {
            private HttpMethod(string value) { Value = value; }

            public string Value { get; set; }

            public static HttpMethod GET { get { return new HttpMethod("GET"); } }
            public static HttpMethod POST { get { return new HttpMethod("POST"); } }
            public static HttpMethod PATCH { get { return new HttpMethod("PATCH"); } }
            public static HttpMethod PUT { get { return new HttpMethod("PUT"); } }
            public static HttpMethod DELETE { get { return new HttpMethod("DELETE"); } }
        }
        
        public class URLS
        {
            private URLS(string value) { Value = value; }

            public string Value { get; set; }

            public static URLS GetCABs { get { return new URLS(@"GetCABs "); } }

        }
    }
}
