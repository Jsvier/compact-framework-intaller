//-----------------------------------------------------------------------
// <copyright file="UserSession.cs" company="Makro">
//     Makro Supermayorista S.A.
// </copyright>
//-----------------------------------------------------------------------

namespace HHT_Base
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using prototype.request;

    /// <summary>
    /// clase de session del usuario
    /// </summary>
    public static class UserSession
    {
        /// <summary>
        /// Token de servicio
        /// </summary>
        private static ServiceProxy _serviceproxy { get; set; }

        /// <summary>
        /// Gets or sets del servicio
        /// </summary>
        public static ServiceProxy ServiceProxy
        {
            get
            {
                if (_serviceproxy == null)
                {
                    using (var file = new StreamReader(HHT_Helper.HHT_PATH + "\\config.txt"))
                    {
                        var _lines = file.ReadToEnd().Replace("\r", "").Split("\n".ToCharArray());

                        foreach (string line in _lines)
                        {                       
                            _serviceproxy = new ServiceProxy();
                            _serviceproxy.URL = line;
                            break;
                        }
                    }
                }
                return _serviceproxy; 
            }
            set
            { _serviceproxy = value; }
        }

    }
}
