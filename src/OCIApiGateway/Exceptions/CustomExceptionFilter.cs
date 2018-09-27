using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OCIApiGateway.Exceptions
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is UserFriendlyException userFriendlyException)
            {
                context.Result = new BadRequestObjectResult(userFriendlyException.Message);
            }
        }
    }
}
