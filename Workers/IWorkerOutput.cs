using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workers
{
    public interface IWorkerOutput
    {
        void Message(string message);
    }
}
