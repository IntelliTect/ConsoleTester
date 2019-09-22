using IntelliTect.TestTools.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleTests
{
    public class DebugLogger : ILogger
    {
        public void Debug(string message)
        {
            LogToDebug($"DEBUG: {message}");
        }

        public void Error(string message)
        {
            LogToDebug($"ERROR: {message}");
        }

        public void Info(string message)
        {
            LogToDebug($"INFO: {message}");
        }

        private void LogToDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine($"TESTING CUSTOM LOGGER -- {message}");
        }
    }
}
