using Microsoft.Extensions.Logging;
using Ocelot.Errors;
using System.Collections.Generic;
using System.Linq;

namespace OCIApiGateway
{
    public static class LogErrors
    {
        public static void LogWarning(this ILogger logger, IEnumerable<Error> errors)
        {
            string msg = string.Join("; ", errors.Select(s => s.Message));
            logger.LogWarning(msg);
        }
    }
}
