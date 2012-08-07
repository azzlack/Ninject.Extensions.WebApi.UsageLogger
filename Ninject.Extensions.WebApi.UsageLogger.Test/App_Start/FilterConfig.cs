using System.Web;
using System.Web.Mvc;

namespace Ninject.Extensions.WebApi.UsageLogger.Test
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}