//-----------------------------------------------------------------------
// <copyright file="ServiceProxy.cs" company="Makro">
//     Makro Supermayorista S.A.
// </copyright>
//-----------------------------------------------------------------------

namespace prototype.request
{
    using System;
    using Newtonsoft.Json;
    using HHT_Base;

    public class ServiceProxy
    {
        public string URL { get; set; }

        
        /// <summary>
        /// Obtiene
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal IRTD Get_CABs()
        {

            WebRequestCompact request = new WebRequestCompact(string.Concat(URL, @"/", AutorunEnum.URLS.GetCABs.Value), AutorunEnum.HttpMethod.POST.Value, string.Empty);
            return Get_CABs<object>(request.GetResponse());

        }

        /// <summary>
        /// Obtiene el el pasaporte del cliente en formato json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        private IRTD Get_CABs<T>(string json)
        {
            IRTD result;
            var root = Newtonsoft.Json.Linq.JObject.Parse(json);
            var serializer = new Newtonsoft.Json.JsonSerializer();

            var d = serializer.Deserialize(root["d"].CreateReader());

            if (d.GetType() == typeof(Int64))
                result = (RTD_Error)JsonConvert.DeserializeObject(json, typeof(RTD_Error));
            else
                result = (RTD_CABs)JsonConvert.DeserializeObject(json, typeof(RTD_CABs));

            return result;
        }
    }
}