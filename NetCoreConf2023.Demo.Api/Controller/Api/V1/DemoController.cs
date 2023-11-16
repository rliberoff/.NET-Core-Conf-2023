using System.Net.Mime;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Options;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

using NetCoreConf2023.Demo.Api.Controller.Api.V1.Models;
using NetCoreConf2023.Demo.Api.Options;
using NetCoreConf2023.Demo.Api.Plugins;

using Swashbuckle.AspNetCore.Annotations;

namespace NetCoreConf2023.Demo.Api.Controller.Api.V1;

[ApiController]
[Route(@"api/[controller]")]
[Route(@"api/v{version:apiVersion}/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class DemoController : ControllerBase
{
    private const int MaxTokens = 2000;

    private static readonly string PluginsDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"Plugins");

    private readonly IKernel kernel;
    private readonly ISemanticTextMemory memory;

    public DemoController(IKernel kernel, ISemanticTextMemory memory, IOptions<BingOptions> bingOptions, IOptions<SmtpClientOptions> smptOptions)
    {
        this.kernel = kernel;
        this.memory = memory;

        InitKernel(kernel, bingOptions, smptOptions);
    }

    [HttpPost(@"planner/action")]
    [ActionName(nameof(ActionPlannerDemoAsync))]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerOperation(Summary = @"Shows how the Action Planner from Semantic Kernel works.", OperationId = nameof(ActionPlannerDemoAsync))]
    [SwaggerResponse(StatusCodes.Status200OK, @"Returns a response with the plan and its result.", ContentTypes = new[] { MediaTypeNames.Application.Json }, Type = typeof(ActionPlannerResponse))]
    public async Task<IActionResult> ActionPlannerDemoAsync(PlannerRequest request, CancellationToken cancellationToken)
    {
        var actionPlan = await new ActionPlanner(kernel).CreatePlanAsync(request.Goal, cancellationToken);

        if (actionPlan.Steps.Count == 0)
        {
            return BadRequest(new ActionPlannerResponse()
            {
                Output = @"Could not create a plan. Check that the goal just ask for one single action, and the action is supported by configured plug-ins and functions!",
                Plan = actionPlan.ToJson(true),
            });
        }

        var actionPlanResult = await actionPlan.InvokeAsync(kernel, cancellationToken: cancellationToken);

        return Ok(new ActionPlannerResponse()
        {
            Output = actionPlanResult.GetValue<string>(),
            Plan = actionPlan.ToJson(true),
        });
    }

    [HttpPost(@"planner/sequential")]
    [ActionName(nameof(SequentialPlannerDemoAsync))]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerOperation(Summary = @"Shows how the Sequential Planner from Semantic Kernel works.", OperationId = nameof(SequentialPlannerDemoAsync))]
    [SwaggerResponse(StatusCodes.Status200OK, @"Returns a response with the plan and its result.", ContentTypes = [MediaTypeNames.Application.Json], Type = typeof(SequentialPlannerResponse))]
    public async Task<IActionResult> SequentialPlannerDemoAsync(PlannerRequest request, CancellationToken cancellationToken)
    {
        var sequentialPlan = await new SequentialPlanner(kernel, new SequentialPlannerConfig() { MaxTokens = MaxTokens }).CreatePlanAsync(request.Goal, cancellationToken);

        if (sequentialPlan.Steps.Count == 0)
        {
            return BadRequest(new ActionPlannerResponse()
            {
                Output = @"Could not create a plan. Check that the goal is supported by configured plug-ins and functions!",
                Plan = sequentialPlan.ToJson(true),
            });
        }

        while (sequentialPlan.HasNextStep)
        {
            await kernel.StepAsync(sequentialPlan, cancellationToken);
        }

        return Ok(new SequentialPlannerResponse()
        {
            Outputs = sequentialPlan.Outputs.ToDictionary(output => output, output => sequentialPlan.State[output]),
            Plan = sequentialPlan.ToJson(true),
        });
    }

    [HttpPost(@"planner/stepwise")]
    [ActionName(nameof(StepwisePlannerDemoAsync))]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerOperation(Summary = @"Shows how the Stepwise Planner from Semantic Kernel works.", OperationId = nameof(StepwisePlannerDemoAsync))]
    [SwaggerResponse(StatusCodes.Status200OK, @"Returns a response with the plan and its result.", ContentTypes = [MediaTypeNames.Application.Json], Type = typeof(StepwisePlannerResponse))]
    public async Task<IActionResult> StepwisePlannerDemoAsync(PlannerRequest request, CancellationToken cancellationToken)
    {
        var plannerConfig = new StepwisePlannerConfig
        {
            MinIterationTimeMs = 1500,
            MaxIterations = 5,
            MaxTokens = MaxTokens,
        };

        var stepwisePlan = new StepwisePlanner(kernel, plannerConfig).CreatePlan(request.Goal);

        var kernelResult = await kernel.RunAsync(cancellationToken, stepwisePlan);
        var result = kernelResult.GetValue<string>()!;

        return Ok(new StepwisePlannerResponse()
        {
            Output = result,
            Plan = stepwisePlan.ToJson(true),
        });
    }

    private static void InitKernel(IKernel kernel, IOptions<BingOptions> bingOptions, IOptions<SmtpClientOptions> smptOptions)
    {
        kernel.ImportSemanticFunctionsFromDirectory(PluginsDirectory, @"TextPlugin", @"MealsPlugin");

        kernel.ImportFunctions(new TextPlugin(), nameof(TextPlugin));
        kernel.ImportFunctions(new WebSearchEnginePlugin(new BingConnector(bingOptions.Value.Key)), nameof(WebSearchEnginePlugin));
        kernel.ImportFunctions(new SendEmailPlugin(smptOptions), nameof(SendEmailPlugin));
    }
}
