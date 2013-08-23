using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Workers;

namespace ProcessMonitorAction
{
    public partial class StatusForm : Form
    {
        public StatusForm() {
            InitializeComponent();

            this.Resize += new EventHandler(StatusForm_Resize);
        }

        void StatusForm_Resize(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized) {
                ShowInTaskbar = false;
                notifyIcon.Visible = true;
            }
            else {
                ShowInTaskbar = true;
                notifyIcon.Visible = false;
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            WindowState = FormWindowState.Normal;
        }

        private void StatusForm_Load(object sender, EventArgs e) {
            WireMessage myLogger = new WireMessage(WriteMessage);

            output = new OutputWorker(myLogger);
            timerWorker = new TimerWorker();
            timerWorker.Run(output);
        }

        public void WriteMessage(string message) {
            if (txtOutput.InvokeRequired) {
                WireMessage d = new WireMessage(WriteMessage);
                this.Invoke(d, new object[] { message });
            }
            else {
                txtOutput.AppendText(message + "\n");
            }
        }

        //public override void Dispose() {
        //    if (timerWorker != null) {
        //        timerWorker = null;
        //    }
        //    if (output != null) {
        //        output = null;
        //    }

        //    base.Dispose();
        //}

        private TimerWorker timerWorker = null;
        private OutputWorker output = null;

        public delegate void WireMessage(string message);



        public class OutputWorker : IWorkerOutput
        {
            private WireMessage _logger;

            public OutputWorker(WireMessage logger) {
                _logger = logger;
            }

            public void Message(string message) {
                _logger.Invoke(message);
            }
        }
    }
}
