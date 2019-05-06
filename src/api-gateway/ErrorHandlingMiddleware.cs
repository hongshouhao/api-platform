using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocelot.Configuration.Creator;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ApiGateway
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly ILoggerFactory _loggerFactory;

        public ErrorHandlingMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            IHostingEnvironment env,
            IInternalConfigurationCreator configCreator)
        {
            _next = next;
            _loggerFactory = loggerFactory;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

                if (context.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    await context.Response.WriteAsync(File.ReadAllText("404.html"));
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ILogger logger = _loggerFactory.CreateLogger("apigateway-exception");
            logger.LogError(exception, exception.Message);

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)500;

                string result;
                if (!_env.IsDevelopment())
                {
                    result = JsonConvert.SerializeObject(new { error = exception.Message });
                }
                else
                {
                    result = JsonConvert.SerializeObject(new
                    {
                        error = exception.Message,
                        stackTrace = exception.StackTrace
                    });
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
