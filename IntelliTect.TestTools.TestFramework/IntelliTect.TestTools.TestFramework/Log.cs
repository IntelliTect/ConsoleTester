namespace IntelliTect.TestTools.TestFramework
{
    public class Log : ILogger
    {
        public void Debug(string testCaseName, string testBlockName, string message)
        {
            LogToDebug($"Debug: {testCaseName} - {testBlockName} - {message}");
        }

        public void Error(string testCaseName, string testBlockName, string message)
        {
            LogToDebug($"Error: {testCaseName} - {testBlockName} - {message}");
        }

        public void Info(string testCaseName, string testBlockName, string message)
        {
            LogToDebug($"Info: {testCaseName} - {testBlockName} - {message}");
        }

        public void LogTestBlockArguments(string testCaseName, string testBlockName, string argumentsAsJson)
        {
            LogToDebug($"{testCaseName} - {testBlockName} - Arguments: {argumentsAsJson}");
        }

        public void LogTestBlockReturn(string testCaseName, string testBlockName, string returnedValue)
        {
            LogToDebug($"Return: {testCaseName} - {testBlockName} - Returned: {returnedValue}");
        }

        private void LogToDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
