namespace IntelliTect.TestTools.TestFramework
{
    public interface ILogger
    {
        void LogTestBlockArguments(string testCaseName, string testBlockName, string argumentsAsJson);
        void LogTestBlockReturn(string testCaseName, string testBlockName, string returnedValue);
        void Debug(string testCaseName, string testBlockName, string message);
        void Info(string testCaseName, string testBlockName, string message);
        void Error(string testCaseName, string testBlockName, string message);
    }
}
