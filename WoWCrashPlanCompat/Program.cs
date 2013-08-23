using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Workers;

namespace WoWCrashPlanCompat
{
    class Program
    {
        static void Main(string[] args) {
            new TimerWorker().Run(new OutputWorker());

            Console.ReadLine();
        }

        public class OutputWorker : IWorkerOutput
        {

            #region IWorkerOutput Members

            public void Message(string message) {
                Console.WriteLine(message);
            }

            #endregion
        }
    }
}
