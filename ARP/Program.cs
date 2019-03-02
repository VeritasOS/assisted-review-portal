/*
Copyright (c) 2019 Veritas Technologies LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using garb.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;

namespace garb
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// NLog: setup the logger first to catch all errors
			var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

			try
			{
				logger.Debug("init main");

				var host = CreateWebHostBuilder(args).Build();

				using (var scope = host.Services.CreateScope())
				{
					var services = scope.ServiceProvider;

					try
					{
						var context = services.GetRequiredService<GarbContext>();
						context.Database.Migrate();
						DbInitializer.Initialize(context);
					}
					catch (Exception ex)
					{
						logger.Error(ex, "An error occurred seeding the DB.");
					}
				}

				host.Run();
			}
			catch (Exception ex)
			{
				//NLog: catch setup errors
				logger.Error(ex, "Stopped program because of exception");
				throw;
			}
			finally
			{
				// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
				NLog.LogManager.Shutdown();
			}

		}

		//public static IWebHost BuildWebHost(string[] args) =>
		//    WebHost.CreateDefaultBuilder(args)
		//        .UseSetting("detailedErrors", "true")
		//        .UseStartup<Startup>()
		//        .CaptureStartupErrors(true)
		//        .Build();

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			   WebHost.CreateDefaultBuilder(args)
				   .UseSetting("detailedErrors", "true")
				   .UseStartup<Startup>()
				   .CaptureStartupErrors(true).ConfigureLogging(logging =>
				   {
					   logging.ClearProviders();
					   logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
				   })
				   .UseNLog();
	}
}
