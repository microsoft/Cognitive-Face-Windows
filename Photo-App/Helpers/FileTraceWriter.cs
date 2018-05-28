using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Photo_Detect_Catalogue_Search_WPF_App.Helpers
{
    public class FileTraceWriter : ITraceWriter
    {
        static string LogFileName = "IotHub_Log.txt";
        const string LogFolderName = "Logs";

        public static string LogFilePath { get; set; }

        public TraceLevel LevelFilter => throw new NotImplementedException();

        static int timeout = 1000;

        public static bool LoggingEnabled = true;
        private static ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static FileTraceWriter _singleton;
        private static TraceLevel _levelFilter;

        private FileTraceWriter()
        {
            Initialise();
        }

        internal static FileTraceWriter GetInstance()
        {
            if (_singleton == null)
            {
                _singleton = new FileTraceWriter();
            }
            return _singleton;
        }

        public static void Initialise(TraceLevel levelFilter = TraceLevel.Verbose)
        {
            _levelFilter = levelFilter;
            LogFilePath = System.IO.Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDoc‌​uments), "PhotoRecognizer");

            if (!Directory.Exists(LogFilePath))
            {
                Directory.CreateDirectory(LogFilePath);
            }

            LogFileName = "Log_" + GetTimeStamp() + ".txt";
            LogFilePath = Path.Combine(LogFilePath, LogFileName);

            if (!File.Exists(LogFilePath))
            {
                using (StreamWriter sw = File.CreateText(LogFilePath))
                {
                    sw.WriteLine($"{DateTime.Now}: File created");
                }
            }
        }

        private static string GetTimeStamp()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }

        public static void LogError(Exception exc, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (exc == null)
            {
                Debug.WriteLine("Error, but no exception to process");
                return;
            }

            string errMsg = message;
            try
            {
                errMsg = $"ERROR: {DateTime.Now}: {memberName}: Line {sourceLineNumber}: {exc}{Environment.NewLine}";
            }
            catch (Exception ex)
            {
                errMsg = "ERROR, Error: " + ex.Message;
            }

            Debug.WriteLine(errMsg);

            if (!LoggingEnabled)
                return;
            
            try
            {
                if (cacheLock.TryEnterWriteLock(timeout))
                {
                    if (LogFilePath == null)
                        FileTraceWriter.Initialise();

                    using (StreamWriter sw = File.AppendText(LogFilePath))
                    {
                        sw.WriteLine(errMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                // to do
            }
        }

        public static void LogMessage(string Message,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string sourceFilePath = "",
           [CallerLineNumber] int sourceLineNumber = 0)
        {
            Debug.WriteLine(Message);

            if (!LoggingEnabled)
                return;
            
            try
            {
                if (cacheLock.TryEnterWriteLock(timeout))
                {
                    if (LogFilePath == null)
                        FileTraceWriter.Initialise();

                    using (StreamWriter sw = File.AppendText(LogFilePath))
                    {
                        sw.WriteLine($"{DateTime.Now}: {memberName}: Line {sourceLineNumber}: " + Message);
                    }
                }
            }
            catch (Exception exc)
            {
                // to do
            }
        }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            if (ex != null)
            {
                LogError(ex, message);
            }

            LogMessage(message);
        }
    }

}
