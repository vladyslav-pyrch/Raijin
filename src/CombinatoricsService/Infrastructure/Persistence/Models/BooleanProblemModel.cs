namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class BooleanProblemModel
{
    public Guid Id { get; set; }

    public string Formula { get; set; }

    public string Satisfiability { get; set; }

    public List<VariableAssignmentModel>? Solution { get; set; }
}