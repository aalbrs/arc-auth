using ArcAuth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ArcAuth.ServiceExtensions
{
    public static class ArcGisOauth
    {
        public readonly static string AuthenticationSchema = "ArcGISOAuth";

        public static AuthenticationBuilder AddArcGisOAuth(this AuthenticationBuilder serviceCollection, AppSettings settings)
        {
            serviceCollection.AddOAuth(AuthenticationSchema, options =>
            {
                options.ClientId = settings.PortalAppId;
                options.ClientSecret = settings.PortalAppSecret;
                // does not seem to matter what we put for callback, the middleware deals with the request
                options.CallbackPath = "/account/oauthcallback";
                options.AuthorizationEndpoint = settings.AuthEndpoint;
                options.TokenEndpoint = settings.TokenEndpoint;
                options.UserInformationEndpoint = settings.UserInfoEndpoint;

                // add scopes as needed, which can be applied from info in user info endpoint
                options.Scope.Add("username");
                options.Scope.Add("fullName");

                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
                
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        // get user info from the /self endpoint and use it to populate user claims
                        var addToQuery = $"f=json&token={context.AccessToken}";
                        var builder = new UriBuilder(context.Options.UserInformationEndpoint);
                        builder.Query = addToQuery;
                        var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        // deserialise info for populating claims
                        var userStr = await response.Content.ReadAsStringAsync();
                        var userObj = JsonSerializer.Deserialize<dynamic>(userStr);
                        var user = JsonSerializer.Deserialize<ArcGisUser>(userStr, new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                           
                        });
                        // apply general user info using main user object
                        context.RunClaimActions(userObj);

                        // Apply group membership claims by mapping group IDs as new claims.
                        // This is later used for applying access policies.
                        user.Groups.ForEach(group =>
                        {
                            context.Identity.AddClaim(new Claim(group.Id, "true"));
                        });
                    }
                };
            });

            return serviceCollection;
        }

        public static IServiceCollection AddArcGisGroupAuthorisation(this IServiceCollection services, AppSettings settings)
        {
            // Apply authorisation using app settings. Each policy requires group membership of 1 or more groups.

            services.AddAuthorization(options =>
            {
                foreach (var policySetting in settings.Authorisation.Policies)
                {
                    // A policy should require group membership, each group is added as a required claim. 
                    // User will have claims applied on sign in, represented by group ID as seen in ArcGIS Portal.
                    options.AddPolicy(policySetting.Name, policy =>
                    {
                        policySetting.Groups.ForEach((group) => policy.RequireClaim(group));
                    });
                }
            });
            return services;
        }

    }
}
