using System.Net.Mime;

using Microsoft.AspNetCore.Mvc;

using Microsoft.SemanticKernel;

namespace NetCoreConf2023.Demo.Api.Controller.Api.V1;

[ApiController]
[Route(@"api/[controller]")]
[Route(@"api/v{version:apiVersion}/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class DemoController : ControllerBase
{
    private readonly IKernel kernel;
    private readonly ILogger logger;

    public DemoController(IKernel kernel, ILogger<DemoController> logger)
    {
        this.kernel = kernel;
        this.logger = logger;
    }
}
