using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace HHT_Base
{
    public class HHT_BASE
    {
        public delegate void EventHandler(String s);
        public event EventHandler Message;
        public const int MAX_ARGS_SUPPORTED = 4;

        /// <summary>
        /// Evento disparador de la mensajeria
        /// </summary>
        /// <param name="message"></param>
        public void OnMessage(String message)
        {
            if (Message != null)
                Message(message);
        }

    }
}
