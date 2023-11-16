namespace NetCoreConf2023.Demo.Api.Controller.Api.V1.Models;

public class SequentialPlannerResponse : PlannerResponseBase
{
    public IDictionary<string, string> Outputs { get; init; }
}
