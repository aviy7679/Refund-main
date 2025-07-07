using System;

namespace RefundSystem_University.ViewModels
{
    public class ViewError
    {
        public ViewError(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        public string Message { get; set; }
        public Exception Exception { get; set; }
        public override string ToString()
        {
            var result = Message;
            if (Exception != null)
                result += $"\n{Exception}";
            return result;
        }
    }
}