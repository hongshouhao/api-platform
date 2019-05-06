using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NLog;
using System;
using System.Threading.Tasks;

namespace ApiGatewayManager
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, IHostingEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            NLog.ILogger debugLogger = LogManager.GetCurrentClassLogger();
            debugLogger.Error(exception, exception.Message);

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)500;

                string result;
                if (_env.IsDevelopment())
                {
                    result = JsonConvert.SerializeObject(new { error = exception.Message });
                }
                else
                {
                    result = JsonConvert.SerializeObject(new { error = exception.Message, stackTrace = exception.StackTrace });
                }
                return context.Response.WriteAsync(result);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
