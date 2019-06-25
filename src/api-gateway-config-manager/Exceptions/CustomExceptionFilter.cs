using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiGatewayManager.Exceptions
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is UserFriendlyException friendlyException)
            {
                context.Result = new BadRequestObjectResult(friendlyException.Message);
            }
        }
    }
}
