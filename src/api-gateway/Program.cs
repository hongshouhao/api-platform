using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            new WebHostBuilder()
                .UseKestrel()
                .UseIISIntegration()
                .UseNLog()
                .UseStartup<Startup>();
    }
}
