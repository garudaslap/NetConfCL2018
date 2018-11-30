using System;
namespace RskManager.Models
{
    public class ErrorResultModel
    {
        public ErrorResultModel(string message, string stackTrace)
        {
            Message = message;
            StackTrace = stackTrace;
        }

        public string Message
        {
            get;
            set;      
        }

        public string StackTrace
        {
            get;
            set;
        }
    }
}