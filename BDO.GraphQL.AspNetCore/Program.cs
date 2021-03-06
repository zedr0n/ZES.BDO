﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace BDO.GraphQL.AspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options => options.ConfigureEndpoints())
                .UseUrls("http://localhost:5000", "https://localhost:5001");
    }
}