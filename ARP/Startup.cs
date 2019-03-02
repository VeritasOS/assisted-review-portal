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

using AutoMapper;
using garb.Data;
using garb.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using System.Text;

namespace garb
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();

			Configuration = builder.Build();
			//env.ConfigureNLog("nlog.config");
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Adding CORS
			services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
			{
				builder.AllowAnyOrigin()
					   .AllowAnyMethod()
					   .AllowAnyHeader()
					   .AllowCredentials();
			}));

			// Adding authentication services
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });

            // Add framework services.
            services.AddDbContext<GarbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("GarbDatabase")));

			//.AddUnitOfWork<GarbContext>();

			services.AddAutoMapper();

          	services.AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV");

			services.AddMvc();

			services.AddMvcCore().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			// In production, the Angular files will be served from this directory
			services.AddSpaStaticFiles(configuration =>
			{
				configuration.RootPath = "ClientApp/dist";
			});

			services.AddApiVersioning(o =>
			{
				o.AssumeDefaultVersionWhenUnspecified = true;
				o.DefaultApiVersion = new ApiVersion(1, 0);
			});

			services.AddSwaggerGen(options =>
			{
				var provider = services.BuildServiceProvider()
									   .GetRequiredService<IApiVersionDescriptionProvider>();

				foreach (var description in provider.ApiVersionDescriptions)
				{
					options.SwaggerDoc(description.GroupName, SwaggerHelper.CreateInfoForApiVersion(description));
				}

				// add a custom operation filter which sets default values
				options.OperationFilter<SwaggerDefaultValues>();

				// integrate xml comments
				options.IncludeXmlComments(SwaggerHelper.XmlCommentsFilePath);
			});

			// Added - uses IOptions<T> for your settings.
			services.AddOptions();

			// Added - Confirms that we have a home for our DemoSettings
			services.Configure<StorageHelper>(Configuration.GetSection("StorageHelperSettings"));

			//call this in case you need aspnet-user-authtype/aspnet-user-identity
			//services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApiVersionDescriptionProvider provider, GarbContext context)
		{
			//add CORS         
			app.UseCors("CorsPolicy");

            // Add Authentication
            app.UseAuthentication();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.Use(async (currentContext, next) =>
			{
				await next();

				// If there's no available file and the request doesn't contain an extension, we're probably trying to access a page.
				// Rewrite request to use app root
				if (currentContext.Response.StatusCode == 404 && !Path.HasExtension(currentContext.Request.Path.Value) && !currentContext.Request.Path.Value.StartsWith("/api") && !currentContext.Request.Path.Value.StartsWith("/api-docs") && !currentContext.Request.Path.Value.StartsWith("/store"))
				{
					currentContext.Request.Path = "/index.html";
					currentContext.Response.StatusCode = 200; // Make sure we update the status code, otherwise it returns 404
					await next();
				}
			});

			app.UseSwagger(options =>
			{
				options.RouteTemplate = "api-docs/{documentName}/swagger.json";
			});

			app.UseSwaggerUI(options =>
			{
				foreach (var description in provider.ApiVersionDescriptions)
				{
					options.SwaggerEndpoint(
						$"/api-docs/{description.GroupName}/swagger.json",
						description.GroupName.ToUpperInvariant());
					options.RoutePrefix = "api-docs";
				}
			});

			if (!env.IsDevelopment())
				app.UseHttpsRedirection();

			app.UseDefaultFiles();

			//app.UseStaticFiles();


			app.UseSpaStaticFiles();

			app.UseSpaStaticFiles(new StaticFileOptions()
			{
				FileProvider = new PhysicalFileProvider(
					Configuration.GetSection("StorageHelperSettings").GetValue<string>("StorageRootFolder")
					),
				RequestPath = new PathString("/store"),
				ServeUnknownFileTypes = true,
				DefaultContentType = "application/octet-stream",
				OnPrepareResponse = con =>
				{
					con.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
				}
			});

			app.UseMvc();

			//app.UseMvc(routes =>
			//{
			//	routes.MapRoute(
			//		name: "default",
			//		template: "{controller}/{action=Index}/{id?}");
			//});

			app.UseSpa(spa =>
			{
				// To learn more about options for serving an Angular SPA from ASP.NET Core,
				// see https://go.microsoft.com/fwlink/?linkid=864501

				spa.Options.SourcePath = "ClientApp";

				if (env.IsDevelopment())
				{
					spa.UseAngularCliServer(npmScript: "start");
				}
			});


			StorageHelper.StorageRootFolder = Configuration.GetSection("StorageHelperSettings").GetValue<string>("StorageRootFolder");

			DbInitializer.Initialize(context);
		}

	}
}
