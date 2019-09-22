using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleTests
{
    public class LoggingDatabase
    {
        public LoggingDatabase(string testCaseName)
        {
            TestCaseName = testCaseName;
        }

        public string TestCaseName { get; }

        public void LogTestBlockStart()
        {

        }

        public void LogTestBlockEnd()
        {

        }
    }
}
