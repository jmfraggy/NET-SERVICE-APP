using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics; // Added a using statement
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers; // Added a using statement
using System.Runtime.InteropServices; // Added a using statement

namespace MyNewService
{
    public partial class MyNewService : ServiceBase
    {

        //  Declaring SetServiceStatus function by using platform invoke.
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        // Summary:
        //      Services report their status to the Service Control Manager,
        //      so that a user can tell whether a service is functioning correctly.
        //      If a service takes a while to start up, 
        //      it's useful to report a SERVICE_START_PENDING status.

        public enum ServiceState
        {
            // Declare Service Status Values.
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

       [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            // Structure for the status, used in a platform invoke call.
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        private int eventId = 1; // ID of the next event to write into the event log.

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // Method to handle the Timer.Elapsed event
            // TODO: Insert monitoring activities here.
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }

        public MyNewService(string[] args)
        {
            // Summary: 
            //      Custom event.log edited to Add Features to the  Service.
            //      Sets the event source and log name according to the startup params
            //      that the user supplies. If no args are supplied, it uses default values.

            InitializeComponent();

            string eventSourceName = "MySource";
            string logName = "MyNewLog";

            if (args.Length > 0)
            {
                eventSourceName = args[0];
            }

            if (args.Length > 1)
            {
                logName = args[1];
            }

            eventLog1 = new EventLog();

            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }

            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // This code goes at the beginning to implement SERVICE_START_PENDING.

            // Define what occurs when the service  starts
            eventLog1.WriteEntry("In OnStart.");

            // Summary:
            //      Service app is designed to be long-running, 
            //      it usually polls or monitors the system. 

            // Set up a timer that triggers every minute. 
            Timer timer = new Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // This code goes at the end to set the status SERVICE_RUNNING.
        }

        protected override void OnStop()
        {
            // If OnStop is a long-running method. Implement the SERVICE_STOP_PENDING.
            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // This code goes at the beginning, it's OPTIONAL

            // Define what occurs when the service is stopped
            eventLog1.WriteEntry("In OnStop.");

            // Return SERVICE_STOPPED status before the OnStop method exits.
            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // This code goest at the end, OPTIONAL with SERVICE_STOP_PENDING.
        }

        protected override void OnContinue()
        {
            // Summary:
            //      You can  override the OnPause, OnContinue, and OnShutdown methods 
            //      to define additional processing for your component.
            eventLog1.WriteEntry("In OnContinue.");
        }

    }
}
