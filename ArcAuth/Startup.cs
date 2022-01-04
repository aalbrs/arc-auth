using ArcAuth.ServiceExtensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace ArcAuth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Inject settings by binding the appsettings.json file to a class instance
            var appSettings = new AppSettings();
            Configuration.Bind(appSettings);
            services.AddSingleton<AppSettings>(appSettings);

            // OAuth implementation
            services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = ArcGisOauth.AuthenticationSchema;

                })
                .AddCookie(setup => setup.ExpireTimeSpan = TimeSpan.FromMinutes(60 * 24 * 14))
                .AddArcGisOAuth(appSettings);
            // Add group-based authorisation policies
            services.AddArcGisGroupAuthorisation(appSettings);

            services.AddControllersWithViews();
        }

        // Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            app.UseHttpsRedirection();

            app.UseRouting();

            // Serve a static app on root URL. Does not have authorisation applied itself, will make API calls. 
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-5.0#static-file-authorization
            app.UseFileServer(new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                RequestPath = "",
                EnableDefaultFiles = true
            });

            // Auth applied here and implemented in controllers 
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("account", "{controller=Account}/{action=Index}");
            });
        }
    }
}
