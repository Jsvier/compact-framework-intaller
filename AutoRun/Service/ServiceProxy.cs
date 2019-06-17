//-----------------------------------------------------------------------
// <copyright file="ServiceProxy.cs" company="Makro">
//     Makro Supermayorista S.A.
// </copyright>
//-----------------------------------------------------------------------

namespace prototype.request
{
    using System;
    using Newtonsoft.Json;

    public class ServiceProxy
    {
        public string URL { get; set; }

        /// <summary>
        /// Obtiene
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IRTD Get_demo(string data)
        {
            //string json = "{\"token\":\"" + UserSession.Operator.Token + "\",\"passport\":\"" + data + "\"}";

            //WebRequestCompact request = new WebRequestCompact(string.Concat(URL, @"/", FasttrackEnum.URLS.Customer_Passport.Value), FasttrackEnum.HttpMethod.POST.Value, json);
            //return Get_Customer_Passport<object>(request.GetResponse());
            return (IRTD)new Generic();
        }

            
    }
}