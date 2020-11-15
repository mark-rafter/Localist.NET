using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Localist.Server.Middleware
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        readonly IWebHostEnvironment webHostEnvironment;
        readonly ILogger<GlobalExceptionFilter> logger;

        public GlobalExceptionFilter(
            IWebHostEnvironment webHostEnvironment,
            ILogger<GlobalExceptionFilter> logger)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
        }

        public void OnException(ExceptionContext? context)
        {
            try
            {
                if (context is not null)
                {
                    logger.LogError(context.Exception, "GLOBAL EXCEPTION");

                    var requestPath = context.HttpContext?.Request?.Path;

                    if (webHostEnvironment.IsProduction()
                        && requestPath?.StartsWithSegments("/api", System.StringComparison.InvariantCultureIgnoreCase) != true)
                    {
                        context.Result = new RedirectToPageResult("Index");
                    }
                }
            }
            catch
            {
            }
        }
    }
}