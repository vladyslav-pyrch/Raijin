using System.ComponentModel.DataAnnotations;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public sealed class RabbitMqEventBusOptions
{
    [Required]
    public string Exchange { get; set; } = null!;

}