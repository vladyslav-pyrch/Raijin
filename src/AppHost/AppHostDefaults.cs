namespace Raijin.AppHost;

internal static class AppHostDefaults
{
    public static class RabbitMq
    {
        public static readonly string Name = "rabbit-mq";
    }

    public static class SatSolver
    {
        public static readonly string Name = "sat-solver";

        public static class Database
        {
            public static readonly string Name = "sat-solver-db";
        }
    }

    public static class QueryService
    {
        public static readonly string Name = "query-service";

        public static readonly string HealthCheckPath = "/health";

        public static class Database
        {
            public static readonly string Name = "query-service-db";
        }

        public static class Redis
        {
            public static readonly string Name = "query-service-redis-db";
        }
    }

    public static class IdentityService
    {
        public static readonly string Name = "identity-service";

        public static readonly string HealthCheckPath = "/health";

        public static class Database
        {
            public static readonly string Name = "identity-service-db";
        }
    }

    public static class CombinatoricsService
    {
        public static readonly string Name = "combinatorics-service";

        public static readonly string HealthCheckPath = "/health";

        public static class Database
        {
            public static readonly string Name = "combinatorics-service-db";
        }
    }

    public static class ApiGateway
    {
        public static readonly string Name = "api-gateway";

        public static readonly string HealthCheckPath = "/health";
    }

    public static class SpaFrontend
    {
        public static readonly string Name = "spa-frontend";

        public static readonly string AppDirectory = "../Spa";

        public static readonly string RunScriptName = OperatingSystem.IsLinux() ? "start" : "win:start";
    }

    public static class Cryptominisat
    {
        public static readonly string Name = "cryptominisat";

        public static readonly string ContainerName = "cryptominisat";

        public static readonly string ContextPath = "../cryptominisat";

        public static readonly string MountLocalPath =
            Path.Combine(Directory.GetCurrentDirectory(), "../cryptominisat-mount");

        public static readonly string MountContainerPath = "/app/cryptominisat/problems";
    }

}