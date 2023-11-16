using System.ComponentModel.DataAnnotations;

namespace NetCoreConf2023.Demo.Api.Controller.Api.V1.Models;

public class PlannerRequest
{
    [Required]
    public string Goal { get; init; }
}
