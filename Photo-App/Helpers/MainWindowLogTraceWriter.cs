namespace Photo_Detect_Catalogue_Search_WPF_App.Helpers
{
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Diagnostics;

    class MainWindowLogTraceWriter : ITraceWriter
    {
        private TraceLevel _levelFilter;

        public MainWindowLogTraceWriter(TraceLevel levelFilter = TraceLevel.Verbose)
        {
            _levelFilter = levelFilter;
        }

        public TraceLevel LevelFilter
        {
            get { return _levelFilter; }
        }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            MainWindow.Log(message);
        }
    }
}
