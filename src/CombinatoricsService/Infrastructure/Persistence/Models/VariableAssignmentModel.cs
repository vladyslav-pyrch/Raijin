namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

public class VariableAssignmentModel
{
    public int Id { get; set; }

    public Guid BooleanProblemId { get; set; }

    public string VariableName { get; set; }

    public bool Value { get; set; }
}