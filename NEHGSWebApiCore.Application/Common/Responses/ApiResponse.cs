using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Common.Responses
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? Message { get; set; }
        public dynamic? Data { get; set; }

        public static ApiResponse Success(dynamic data, string message = null)
        {
            return new ApiResponse { StatusCode = HttpStatusCode.OK, Data = data, Message = message };
        }

        public static ApiResponse Fail(string message, HttpStatusCode code)
        {
            return new ApiResponse { StatusCode = code, Data = null, Message = message };
        }
    }
}
