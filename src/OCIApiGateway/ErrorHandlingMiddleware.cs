using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OCIApiGateway
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            IHostingEnvironment env)
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
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ILogger logger = null;
            ControllerActionDescriptor actionDescriptor = null;
            var routeHandler = context.GetRouteData().Routers.FirstOrDefault(s => s is MvcAttributeRouteHandler) as MvcAttributeRouteHandler;
            if (routeHandler != null)
                actionDescriptor = routeHandler.Actions.FirstOrDefault() as ControllerActionDescriptor;
            if (actionDescriptor != null)
            {
                Type comtrollerType = actionDescriptor.ControllerTypeInfo;
                logger = _loggerFactory.CreateLogger(comtrollerType);
            }
            else
            {
                logger = _loggerFactory.CreateLogger<ErrorHandlingMiddleware>();
            }

            logger.LogError(exception, exception.Message);

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
