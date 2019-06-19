//-----------------------------------------------------------------------
// <copyright file="AutorunException.cs" company="Makro">
//     Makro Supermayorista S.A.
// </copyright>
//-----------------------------------------------------------------------

namespace HHT_Base
{
    using System;

    /// <summary>
    /// Clase de Excepcion de Autorun
    /// </summary>
    public class AutorunException : Exception
    {
        public AutorunException()
        {
        }

        public AutorunException(string message)
            : base(message)
        {
        }

        public AutorunException(int message)
            : base(message.ToString())
        {
        }

        public AutorunException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}