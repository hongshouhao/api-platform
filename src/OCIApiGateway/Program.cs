using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;

namespace OCIApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .UseNLog()
                .UseStartup<Startup>();
    }
}
