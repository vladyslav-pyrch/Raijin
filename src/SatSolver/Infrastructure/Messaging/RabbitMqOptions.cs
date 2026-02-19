using System.ComponentModel.DataAnnotations;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    [Required]
    public string Exchange { get; set; } = null!;

    [Required]
    public string QueuePrefix { get; set; } = "sat-solver";

}