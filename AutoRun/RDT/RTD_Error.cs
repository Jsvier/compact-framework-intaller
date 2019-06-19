//-----------------------------------------------------------------------
// <copyright file="RTD_Error.cs" company="Makro">
//     Makro Supermayorista S.A.
// </copyright>
//-----------------------------------------------------------------------

namespace HHT_Base
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using prototype.request;

    public class RTD_Error:IRTD
    {
        public RTD_Error()
        {
            r = (int)AutorunEnum.RTD.ERROR;
        }
        public int r { get; set; }
        public int t { get; set; }
        public int d { get; set; }
    }
}
