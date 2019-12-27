using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using WebApplication1.Business;

namespace WebApplication1.Infrastructure
{
    public class BusinessExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly IStringLocalizer _localizer;
        private readonly ILogger<BusinessExceptionFilter> _logger;

        //private readonly IStringLocalizer _localizer;

        public int Order { get; set; } = int.MaxValue - 10;
        public BusinessExceptionFilter(IStringLocalizerFactory localizerFactory, ILogger<BusinessExceptionFilter> logger)
        {
            var assemblyName = new AssemblyName(GetType().Assembly.FullName);
            _localizer = localizerFactory.Create("BusinessExceptions", assemblyName.Name);
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is BusinessException exception)
            {
                context.Result = new ObjectResult(
                    new ProblemDetails()
                    {
                        Title = Translate(exception.Code.ToString()),
                        Status = StatusCodes.Status400BadRequest,
                        Type = $"https://apps.myapp.be/ordering/errors/business/#{exception.Code}"
                    })
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                };
                context.ExceptionHandled = true;
            }
        }

        public string Translate(string errorCode)
        {
            LocalizedString translation = _localizer.GetString(errorCode);
            if (translation.ResourceNotFound)
                _logger.LogWarning($"BusinessExceptions {errorCode} couldn't be found in the resources files. Check that the client has specified an Accept-Language header with one of the supported cultures and that the appropriate file contains the errorCode {errorCode}.");
            return translation.Value;
        }
    }
}
