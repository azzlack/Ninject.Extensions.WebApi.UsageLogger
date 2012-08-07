using Ninject.Extensions.WebApi.UsageLogger.App_Start;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Bootstrapper), "PreStart")]

namespace Ninject.Extensions.WebApi.UsageLogger.App_Start
{
    using System.Web.Http;

    using Ninject.Extensions.WebApi.UsageLogger.Handlers;

    /// <summary>
    /// Bootstrapper for the Usage Logger
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Executes right before the application is started
        /// </summary>
        public static void PreStart()
        {
            GlobalConfiguration.Configuration.MessageHandlers.Add(new UsageHandler());
        }
    }
}