using Model.Domain.Foundation.Logging;
using ReactiveDomain.Logging;

// ReSharper disable UnusedVariable
// ReSharper disable MemberCanBePrivate.Global

namespace Domain
{
    public static class Bootstrap
    {
        private static readonly ILogger log = LogFactoryAdapter.GetLogger(typeof(Bootstrap));

        public static bool Loaded;

        public static void Load()
        {
            Loaded = true;
            log.Debug("Loaded");
        }

    }
}
