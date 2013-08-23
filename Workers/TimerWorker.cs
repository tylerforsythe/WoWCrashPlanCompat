using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;

namespace Workers
{
    public class TimerWorker : IDisposable
    {
        GTimer gtimer = new GTimer();
        int timeToCycle = 15 * 1000;
        int timeToWaitAfterAction = 10 * 1000;
        Timer actionTimer = null;
        IWorkerOutput outputDelegate = null;

        public void Run(IWorkerOutput output) {
            Run(output, null);
        }

        public void Run(IWorkerOutput output, ISynchronizeInvoke objectToSyncWith) {
            if (output == null)
                throw new Exception("IWorkerOutput reference may not be null.");
            if (timeToCycle < timeToWaitAfterAction)
                throw new Exception("Waiting period is less than cycle period. Undefined and a bad idea.");

            if (CrashPlanService == null)
                return;

            outputDelegate = output;

            CheckForWow();

            actionTimer = new System.Timers.Timer();
            actionTimer.Interval = timeToCycle;
            actionTimer.Elapsed += new ElapsedEventHandler(tmrTimersTimer_Elapsed);
            if (objectToSyncWith != null)
                actionTimer.SynchronizingObject = objectToSyncWith; //Synchronize with...
            actionTimer.Start();
        }

        public void Dispose() {
            if (actionTimer != null) {
                actionTimer.Stop();
                actionTimer.Dispose();
                actionTimer = null;
            }
            if (gtimer != null)
                gtimer = null;
            if (_cachedCrashPlanService != null)
                _cachedCrashPlanService = null;
        }

        private void tmrTimersTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            // Do something on the UI thread (same thread the form was created on)...
            // If we didn't set SynchronizingObject we would be on a worker thread...

            CheckForWow();
        }

        private void CheckForWow() {
            Process[] processlist = Process.GetProcesses();
            bool foundWoW = false;

            foreach (Process p in processlist) {
                if (p.ProcessName.Equals("wow", StringComparison.CurrentCultureIgnoreCase)) {
                    foundWoW = true;
                    break;
                }
            }

            if (foundWoW) {
                outputDelegate.Message("Found WoW.exe!");
                EnsureCrashPlanIsHalted();
            }
            else {
                outputDelegate.Message("Negatory");
                EnsureCrashPlanIsRunning();
            }
        }

        private void EnsureCrashPlanIsRunning() {
            if (CrashPlanService.Status != ServiceControllerStatus.Running) {
                CrashPlanService.Start();
                gtimer.Reset();
                System.Threading.Thread.Sleep(timeToWaitAfterAction);
                RefreshCrashPlanServiceCache();
            }
        }

        private void EnsureCrashPlanIsHalted() {
            if (CrashPlanService.Status == ServiceControllerStatus.Running) {
                CrashPlanService.Stop();
                gtimer.Reset();
                System.Threading.Thread.Sleep(timeToWaitAfterAction);
                RefreshCrashPlanServiceCache();
            }
        }

        private ServiceController _cachedCrashPlanService = null;
        private ServiceController CrashPlanService {
            get {
                if (_cachedCrashPlanService == null) {
                    RefreshCrashPlanServiceCache();
                }

                return _cachedCrashPlanService;
            }
        }

        private void RefreshCrashPlanServiceCache() {
            ServiceController[] services = ServiceController.GetServices();

            // try to find service name
            foreach (ServiceController service in services) {
                if (service.ServiceName.Contains("CrashPlan"))
                    _cachedCrashPlanService = service;
            }
        }
    }
}
