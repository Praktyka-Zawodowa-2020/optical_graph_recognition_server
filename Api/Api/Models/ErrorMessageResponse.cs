﻿namespace Api.Models
{
    public class ErrorMessageResponse
    {
        public string Message { get; set; }

        public ErrorMessageResponse(string message)
        {
            Message = message;
        }
    }
}
