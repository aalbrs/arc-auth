using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

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
            // ArcGIS Online or Enterprise base URL
            var portalUrl = Configuration["portalUrl"];
            // App details
            var portalAppId = Configuration["portalAppId"];
            var portalAppSecret = Configuration["portalAppSecret"];
            // Construct redirection and token endpoints
            var authEndpoint = $"{portalUrl}/sharing/rest/oauth2/authorize";
            var tokenEndpoint = $"{portalUrl}/sharing/rest/oauth2/token";
            var userInfoEndpoint = $"{portalUrl}/sharing/rest/community/self";

            // OAuth implementation
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "ArcGIS";

            })
            .AddCookie(setup => setup.ExpireTimeSpan = TimeSpan.FromMinutes(60 * 24))
            .AddOAuth("ArcGIS", options =>
            {
                options.ClientId = portalAppId;
                options.ClientSecret = portalAppSecret;
                // does not seem to matter what we put for callback, the middleware deals with the request
                options.CallbackPath = "/account/oauthcallback";
                options.AuthorizationEndpoint = authEndpoint;
                options.TokenEndpoint = tokenEndpoint;
                options.UserInformationEndpoint = userInfoEndpoint;

                // add scopes as needed, which can be applied from info in user info endpoint
                options.Scope.Add("username");
                options.Scope.Add("fullName");

                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        // Get user info from the /self endpoint and use it to populate user claims
                        var addToQuery = $"f=json&token={context.AccessToken}";
                        var builder = new UriBuilder(context.Options.UserInformationEndpoint);
                        builder.Query = addToQuery;
                        var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        var userObj = await response.Content.ReadAsStringAsync();
                        var user = JsonSerializer.Deserialize<dynamic>(userObj);
                        context.RunClaimActions(user);
                    }
                };
            });

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
