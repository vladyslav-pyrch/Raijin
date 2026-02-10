using Microsoft.EntityFrameworkCore;

namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems;

public class SatProblemsDbContext(DbContextOptions<SatProblemsDbContext> options) : DbContext(options)
{
    public DbSet<SatProblem> SatProblems { get; set; }
}