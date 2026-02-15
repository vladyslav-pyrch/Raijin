namespace Raijin.AppHost;

internal static class AppHostDefaults
{
    public static class RabbitMq
    {
        public const string Name = "rabbit-mq";
    }

    public static class SatSolver
    {
        public const string Name = "sat-solver";

        public const string ContextPath = "../SatSolver";

        public const string DockerfilePath = "./Worker/Dockerfile";

        public static class Database
        {
            public const string Name = "sat-solver-db";
        }

        public static class DatabaseServer
        {
            public const string Name = "sat-solver-db-server";
        }

        public static class DomainTests
        {
            public const string Name = "sat-solver-domain-tests";
        }

        public static class ApplicationTests
        {
            public const string Name = "sat-solver-application-tests";
        }

    }

    public static class QueryService
    {
        public const string Name = "query-service";

        public const string HealthCheckPath = "/health";

        public static class Database
        {
            public const string Name = "query-service-db";
        }

        public static class DatabaseServer
        {
            public const string Name = "query-service-db-server";
        }

        public static class Redis
        {
            public const string Name = "query-service-redis-db";
        }
    }

    public static class IdentityService
    {
        public const string Name = "identity-service";

        public const string HealthCheckPath = "/health";

        public static class Database
        {
            public const string Name = "identity-service-db";
        }

        public static class DatabaseServer
        {
            public const string Name = "identity-service-db-server";
        }
    }

    public static class CombinatoricsService
    {
        public const string Name = "combinatorics-service";

        public const string HealthCheckPath = "/health";

        public static class Database
        {
            public const string Name = "combinatorics-service-db";
        }

        public static class DatabaseServer
        {
            public const string Name = "combinatorics-service-db-server";
        }
    }

    public static class ApiGateway
    {
        public const string Name = "api-gateway";

        public const string HealthCheckPath = "/health";
    }

    public static class SpaFrontend
    {
        public const string Name = "spa-frontend";

        public const string AppDirectory = "../Spa";

        public static readonly string RunScriptName = OperatingSystem.IsLinux() ? "start" : "win:start";
    }
}