//-----------------------------------------------------------------------
// <copyright file="RTD_CABs.cs" company="Makro">
//     Makro Supermayorista S.A.
// </copyright>
//-----------------------------------------------------------------------

namespace HHT_Base
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json.Serialization;
    using Newtonsoft.Json;
    using prototype.request;

    public class RTD_CABs : IRTD
    {
        public int r { get; set; }
        public int t { get; set; }
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<CAB> d { get; set; }
    }
}