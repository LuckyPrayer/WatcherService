using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Threading;
using System.Management;
using System.Runtime.InteropServices.WindowsRuntime;

namespace WatcherService
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        ManagementEventWatcher processStartEvent;
        ManagementEventWatcher processStopEvent;

        string processName = "CDViewer.exe";

        public Service1()
        {
            InitializeComponent();

            string creationQueryString =
        "SELECT TargetInstance" +
        "  FROM __InstanceCreationEvent " +
        "WITHIN  1 " +
        " WHERE TargetInstance ISA 'Win32_Process' " +
        "   AND TargetInstance.Name = '" + processName + "'";

            string deletionQueryString =
        "SELECT TargetInstance" +
        "  FROM __InstanceDeletionEvent " +
        "WITHIN  1 " +
        " WHERE TargetInstance ISA 'Win32_Process' " +
        "   AND TargetInstance.Name = '" + processName + "'";

            // The dot in the scope means use the current machine
            string scope = @"\\.\root\CIMV2";

            processStartEvent = new ManagementEventWatcher(scope, creationQueryString);
            processStopEvent = new ManagementEventWatcher(scope, deletionQueryString);




            processStartEvent.EventArrived += new EventArrivedEventHandler(processStartEvent_EventArrived);
            processStartEvent.Start();
            processStopEvent.EventArrived += new EventArrivedEventHandler(processStopEvent_EventArrived);
            processStopEvent.Start();
        }

        protected override void OnStart(string[] args)
        {
            /*
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif
            */
            WriteToFile("Service started at " + DateTime.Now.ToString());
            /*
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000;
            timer.Enabled = true;
            */
        }

        public bool StartProcess()
        {
            //Start service
            ServiceController sc = new ServiceController("CustomRPC");
            if (sc != null)
            {
                try
                {
                    if (sc.Status != ServiceControllerStatus.Running && sc.Status != ServiceControllerStatus.StartPending)
                    {
                        sc.Start();
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool StopProcess()
        {
            //Stop service
            ServiceController sc = new ServiceController("CustomRPC");
            if (sc != null)
            {
                try
                {
                    if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                    {
                        sc.Stop();
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }
            return false;

        }

        /*
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Process recalled at " + DateTime.Now.ToString());
        }
        */

        public void processStartEvent_EventArrived(object source, EventArrivedEventArgs e)
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            string processName = targetInstance.Properties["Name"].Value.ToString();
            string processID = targetInstance.Properties["ProcessID"].Value.ToString();

            WriteToFile("Process started - " + processName + processID + DateTime.Now.ToString());

            //Start Program
            if (!StartProcess())
            {
                WriteToFile("Service started - " + "CustomRPC" + DateTime.Now.ToString());
            }


        }

        public void processStopEvent_EventArrived(object source, EventArrivedEventArgs e)
        {

            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            string processName = targetInstance.Properties["Name"].Value.ToString();
            string processID = targetInstance.Properties["ProcessID"].Value.ToString();

            WriteToFile("Process ended - " + processName + processID + DateTime.Now.ToString());

            //Stop Program
            if(!StopProcess())
            {
                WriteToFile("Service killed - " + "CustomRPC" + DateTime.Now.ToString());
            }
                

        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                //Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }

        }

        protected override void OnStop()
        {
            WriteToFile("Service stopped at " + DateTime.Now.ToString());
        }



    }
}
