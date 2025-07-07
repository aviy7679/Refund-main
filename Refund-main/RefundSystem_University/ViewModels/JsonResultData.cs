using System;

namespace RefundSystem_University.ViewModels
{
    public class JsonResultData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }

        public JsonResultData()
        {
            Success = false;
        }

        public JsonResultData(bool success)
        {
            Success = success;
        }

        public JsonResultData(string message, Exception e = null) : this()
        {
            Message = message;
            Exception = e?.ToString();
        }
    }
}