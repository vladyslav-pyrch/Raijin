using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Raijin.AppHost")]

namespace Raijin.Constants;

public static class EnvironmentVariables
{
    public static class Cryptominisat
    {
        public static readonly string ContainerName = "CRYPTOMINISAT_CONTAINER_NAME";

        public static readonly string FileExchangeLocalPath = "CRYPTOMINISAT_FILE_EXCHANGE_LOCAL_PATH";

        public static readonly  string FileExchangeContainerPath = "CRYPTOMINISAT_FILE_EXCHANGE_CONTAINER_PATH";

        public static readonly  string TimeoutSeconds = "CRYPTOMINISAT_TIMEOUT_SECONDS";
    }
}
