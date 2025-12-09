using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class GameEventArgs : EventArgs
    {
        public string Message { get; }
        public object Data { get; }
        public DateTime Timestamp { get; }

        public GameEventArgs(string message, object data = null)
        {
            Message = message;
            Data = data;
            Timestamp = DateTime.Now;
        }
    }
}
