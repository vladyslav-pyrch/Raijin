namespace Raijin.AppHost;

internal static class AppHostDefaults
{
    public static class ProblemSolvingService
    {
        public static readonly string Name = "problem-solving-service";

        public static readonly string HealthCheckPath = "/health";
    }

    public static class Cryptominisat
    {
        public static readonly string Name = "cryptominisat";

        public static readonly string ContainerName = "cryptominisat";

        public static readonly string ContextPath = "../cryptominisat";

        public static readonly string FileExchangeLocalPath =
            Path.Combine(Directory.GetCurrentDirectory(), @"..\file_exchange\cryptominisat");

        public static readonly string FileExchangeContainerPath = "/app/cryptominisat/problems";

        public static readonly string TimeoutSeconds = "20";
    }

}