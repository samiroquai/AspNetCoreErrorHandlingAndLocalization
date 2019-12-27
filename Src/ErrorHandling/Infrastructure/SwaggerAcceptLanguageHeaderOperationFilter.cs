using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Infrastructure
{
    public class SwaggerAcceptLanguageHeaderOperationFilter : IOperationFilter
    {
       
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();
            var examples = new Dictionary<string, OpenApiExample>();
            examples.Add("French", new OpenApiExample() { Value = new OpenApiString("fr-BE") });
            examples.Add("English", new OpenApiExample() { Value = new OpenApiString("en-US") });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Examples= examples,
                Description = "One of the supported cultures. Will help the API to provide localized content to the client app.",
            }); ;
        }
    }
}
