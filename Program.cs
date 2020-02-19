using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace oServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000", "http://192.168.0.107:5000")
                .Build();
    }
}
