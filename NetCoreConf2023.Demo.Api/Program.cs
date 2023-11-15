using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;

using Microsoft.Extensions.Options;

using Microsoft.SemanticKernel;

using NetCoreConf2023.Demo.Api;
using NetCoreConf2023.Demo.Api.Infrastructure;
using NetCoreConf2023.Demo.Api.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

/* Load Configuration */

var programType = typeof(Program);

var applicationName = programType.Assembly.FullName;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    ApplicationName = applicationName,
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot"),
});

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

if (Debugger.IsAttached)
{
    builder.Configuration.AddJsonFile(@"appsettings.debug.json", optional: true, reloadOnChange: true);
}

builder.Configuration.AddJsonFile($@"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                     .AddJsonFile($@"appsettings.{Environment.UserName}.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

var isDevelopment = builder.Environment.IsDevelopment();

if (isDevelopment)
{
    builder.Logging.AddConsole();

    if (Debugger.IsAttached)
    {
        builder.Logging.AddDebug();
    }
}

/* Load Options */

builder.Services.AddOptions<SmtpClientOptions>().ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<SemanticKernelOptions>().ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<VersionSwaggerGenOptions>().ValidateDataAnnotations().ValidateOnStart();

var applicationInsightsConnectionString = builder.Configuration.GetConnectionString(@"ApplicationInsights");

if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    builder.Logging.AddApplicationInsights((telemetryConfiguration) => telemetryConfiguration.ConnectionString = applicationInsightsConnectionString, (_) => { });
}

// Add application services...
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration)
                .AddHttpContextAccessor()
                .AddRouting()
                .AddApiVersioning(options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = ApiVersion.Neutral;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = ApiVersionReader.Combine(
                        new QueryStringApiVersionReader(Constants.Versioning.QueryStringVersion),
                        new UrlSegmentApiVersionReader(),
                        new HeaderApiVersionReader(Constants.Versioning.HeaderVersion));

                    options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
                })
                .AddMvc(options => options.Conventions.Add(new VersionByNamespaceConvention()))
                .AddApiExplorer(options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = ApiVersion.Neutral;
                    options.GroupNameFormat = @"'v'V";
                })
                ;

/* OpenAPI (Swagger) Configuration */

builder.Services.AddSwaggerGen(options =>
                 {
                     options.OperationFilter<DefaultValuesOperationFilter>();
                     options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $@"{programType.Assembly.GetName().Name}.xml"));
                     options.EnableAnnotations();
                 })
                .AddTransient<IConfigureOptions<SwaggerGenOptions>, VersionSwaggerGenConfigureOptions>()
                ;

/* MVC Configuration */

builder.Services.AddProblemDetails()
                .AddControllers(options =>
                {
                    options.RequireHttpsPermanent = true;
                    options.SuppressAsyncSuffixInActionNames = true;
                })
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)))
                ;

/* Semantic Kernel Configuration */

builder.Services.AddScoped(sp =>
{
    var options = sp.GetRequiredService<IOptions<SemanticKernelOptions>>().Value;

    var kernel = new KernelBuilder()
        .WithLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
        .WithAzureOpenAIChatCompletionService(options.ChatModelDeploymentName, options.Endpoint.AbsolutePath, options.Key, alsoAsTextCompletion: true)
        .WithAzureOpenAITextEmbeddingGenerationService(options.EmbeddingsModelDeploymentName, options.Endpoint.AbsoluteUri, options.Key)
        .Build();

    return kernel;
});

/* Application Middleware Configuration */

var app = builder.Build();

if (isDevelopment)
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app
        .UseDeveloperExceptionPage()
        .UseSwagger()
        .UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($@"/swagger/{description.GroupName}/swagger.json", description.ApiVersion == ApiVersion.Neutral ? @"Default" : description.GroupName.ToUpperInvariant());
            }

            options.RoutePrefix = @"swagger";
        })
        ;
}

app.UseDefaultFiles()
   .UseStaticFiles()
   .UseRouting()
   .UseAuthentication()
   .UseAuthorization()
   .UseExceptionHandler()
   .UseStatusCodePages()
   .UseEndpoints(endpoints =>
   {
       endpoints.MapControllers();
   })
   ;

app.Run();
