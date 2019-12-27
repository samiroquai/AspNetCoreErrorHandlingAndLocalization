# Error handling, localization and documentation in ASP.NET Core

This project shows how to return consistent and localized error messages in an ASP.NET Core Web API. 

The following kind of errors were considered:
- Validation errors (handled by ASP.NET Core at model binding time). Transparent for the developer. Resulting in 400 status code.
- Business errors (depending on business logic). Raised by the developer. Resulting in 400 status code.
- Errors returned at the controller level, using Convenience methods (NotFound(), ...). Resulting in 4XX status code.
- Unexpected errors (e.g.: failed connection to a DB) resulting in 500 status code

The following resource has been used to build this project : 
https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-3.1#use-exceptions-to-modify-the-response

Most responses returned are under the form of https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails?view=aspnetcore-3.1, which implements RFC 7807 (https://tools.ietf.org/html/rfc7807). Validation errors are a bit more specific: They return a subclass of ProblemDetails: ValidationDetails (https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.validationproblemdetails?view=aspnetcore-3.1). But due to the fact that they share a common interface, client apps shouldn't have problems with these. 


## Localization middleware

It's required that client apps send an Accept-Language header along with their request. The [localization middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-3.1) will use this header to inform the request handlers what language the client prefers. The header must specify a value recognized by the __Supported Cultures__ (specified when the localization middleware is configured). To configure the localization middleware, update the Configure method of the Startup class as shown below. The default language is set to en-US.

```csharp
 public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var supportedCultures = new List<CultureInfo>() {
                new CultureInfo("en-US"),
                new CultureInfo("fr-BE") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures,
                //la propriété suivante semble nécessiter ASP.NET CORE 3.1
               // ApplyCurrentCultureToResponseHeaders = true

            });
        }
```

## Localization of error messages

Two approaches are used: 
- an IActionFilter : [BusinessExceptionFiter](./Src/ErrorHandling/Infrastructure/BusinessExceptionFilter.cs)
- custom ErrorMessages on validation DataAnnotations (shown below)

In both cases, localization middleware is involded

### Localization of errors messages caused by failed Model validation

Add an ErrorMessage property to the Data Annotations used for model validation. See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-3.1#dataannotations-localization for more info. 

```csharp
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTO
{
    public class City
    {
        [Required(ErrorMessage = "MandatoryField")]
        [MinLength(2,ErrorMessage = "FieldNotLongEnough")]
        [MaxLength(255, ErrorMessage = "FieldTooLong")]
        public string Name { get; set; }
        [Required(ErrorMessageResourceName = "MandatoryField")]
        [MinLength(2, ErrorMessage = "FieldNotLongEnough")]
        [MaxLength(3, ErrorMessage = "FieldTooLong")]
        public string CountryCode { get; set; }
    }
}
```

The ErrorMessage translations are then looked up by the [StringLocalizer](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-3.1) created at app startup (see Startup.cs). 

```csharp
  public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddControllers(options =>
                options.Filters.Add<BusinessExceptionFilter>()).AddDataAnnotationsLocalization(o=>
                {
                var type = typeof(LocalizedResources);
                var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
                var factory = services.BuildServiceProvider().GetService<IStringLocalizerFactory>();
                var localizer = factory.Create("ValidationResources", assemblyName.Name);
                o.DataAnnotationLocalizerProvider = (t, f) => localizer;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Error handling with Localization demo API", Version = "v1" });
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "WebApplication1.xml");
                c.IncludeXmlComments(filePath);
                c.OperationFilter<SwaggerAcceptLanguageHeaderOperationFilter>();
            });
        }
```

It requires that one resource file is created for every supported culture and that its prefixed with "ValidationResources". Note that the LocalizedResources class is just a helper class to help locate the resources files. In this project, see [ValidationResources.fr.resx](./src/ErrorHandling/Resources/ValidationResources.fr.resx) and [ValidationResources.en.resx](./src/ErrorHandling/Resources/ValidationResources.en.resx).

To see this demo in action, use the POST method on the WeatherForecastController with an invalid City model in the request body.

```http
DELETE /weatherforecast HTTP/1.1
Host: localhost:44382
Content-Type: application/json
Accept-language: fr-BE
User-Agent: PostmanRuntime/7.19.0
Accept: */*
Cache-Control: no-cache
Postman-Token: ae5f806f-ea3d-4623-8ee9-b215c1f17aa3,c2d9699f-b09b-4190-8cd7-12558b9c64fd
Host: localhost:44382
Accept-Encoding: gzip, deflate
Content-Length: 41
Connection: keep-alive
cache-control: no-cache

{
	"Name":"n",
	"CountryCode":"BEEEEEE"
}
```

Response:

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "traceId": "|fe7fc891-4d7791564da0f89b.",
    "errors": {
        "Name": [
            "La longueur du champ Name doit être supérieure ou égale à 2"
        ],
        "CountryCode": [
            "La longueur du champ CountryCode doit être inférieure ou égale à 3"
        ]
    }
}
```



### Localization of Exceptions raised by the developer

Suppose that your app uses a Domain Model which raises domain exceptions based on logic known by the domain. If your exceptions inherits from a common well-known exception type (e.g. BusinessException), then you can create an [IActionFilter implementation](https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-3.1#use-exceptions-to-modify-the-response) which returns a well-known and localized result to the client apps. This is demoed in this project. See the [BusinessExceptionFilter](./src/ErrorHandling/Infrastructure/BusinessExceptionFilter.cs) class.

This class gets injected a StringLocalizerFactory used to create an IStringLocalizer implementation, looking up the translations in resources files prefixed with __BusinessExceptions__. See [BusinessExceptions.en.resx](./src/ErrorHandling/Resources/BusinessExceptions.en.resx) and [BusinessExceptions.fr.resx](./src/ErrorHandling/Resources/BusinessExceptions.fr.resx).

For a demo of this mechanism, try the DELETE method on the WeatherForecastController. Try to delete a city with the name "namur". (this method requires a valid City model passed in the request body). 
Example call below:

```http
DELETE /weatherforecast HTTP/1.1
Host: localhost:44382
Content-Type: application/json
Accept-language: fr-BE
User-Agent: PostmanRuntime/7.19.0
Accept: */*
Cache-Control: no-cache
Postman-Token: 9e4cdd5f-9694-4792-85e7-990c107bac54,def46a03-7087-47c3-b78d-7492eaed7796
Host: localhost:44382
Accept-Encoding: gzip, deflate
Content-Length: 40
Connection: keep-alive
cache-control: no-cache

{
	"Name":"namur",
	"CountryCode":"BE"
}
```

Response:

```json
{
    "type": "https://apps.myapp.be/ordering/errors/business/#PersistentCity",
    "title": "Cette ville ne peut être supprimée",
    "status": 400
}
```
