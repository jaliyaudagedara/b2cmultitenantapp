using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Notifications;
using Microsoft.IdentityModel.Protocols;
using System.IdentityModel.Tokens;
using B2CMultiTenant.Utilities;

namespace B2CMultiTenant
{
    public partial class Startup
    {
        // App config settings
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];

        //private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

        // B2C policy identifiers
        public static string SignUpInPolicyId = "B2C_1_MTUserSignUpIn";
        public static string ProfilePolicyId = "B2C_1_MTEditProfile";

        private ApplicationDbContext db = new ApplicationDbContext();

        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseErrorPage();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            // Configure OpenID Connect middleware for each policy
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(Startup.ProfilePolicyId));
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(Startup.SignUpInPolicyId));
        }

        // Used for avoiding yellow-screen-of-death
        private Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            if (notification.Exception.Message == "access_denied")
            {
                notification.Response.Redirect("/");
            }
            else
            {
                notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
            }
            return Task.FromResult(0);
        }

        private OpenIdConnectAuthenticationOptions CreateOptionsFromPolicy(string policy)
        {
            var resp = new OpenIdConnectAuthenticationOptions
            {
                MetadataAddress = String.Format(aadInstance, tenant, policy),
                AuthenticationType = policy,

                // These are standard OpenID Connect parameters, with values pulled from web.config
                ClientId = clientId,
                //RedirectUri = redirectUri,
                //PostLogoutRedirectUri = redirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = async (ctx) =>
                    {
                        var uri = ctx.Request.Uri;
                        var url = String.Format("{0}://{1}/", uri.Scheme, uri.Authority);
                        ctx.ProtocolMessage.RedirectUri = url;
                        ctx.ProtocolMessage.PostLogoutRedirectUri = url;
                        await Task.FromResult(0);
                    },
                    AuthenticationFailed = AuthenticationFailed,
                    SecurityTokenValidated = async (ctx) =>
                    {
                        try
                        {
                            var userId = ctx.AuthenticationTicket.Identity.Claims.First(c => c.Type == Constants.ObjectIdClaim).Value;
                            var roles = db.UserRoles.Where(r => r.UserObjectId == userId);
                            if (roles.Count() > 0)
                            {
                                //TODO: use a persistent cookie to hold unto user tenant selection
                                var tenantId = roles.First().TenantId;
                                var tenantName = db.Tenants.First(t => t.Id == tenantId).Name;
                                ctx.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.TenantIdClaim, tenantId));
                                ctx.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.TenantNameClaim, tenantName));
                                foreach (var r in roles.Where(role => role.TenantId == tenantId))
                                    ctx.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.RoleClaim, r.Role));
                            } 
                            await Task.FromResult(0);
                        } catch(Exception ex)
                        {
                            throw new SecurityTokenValidationException();
                        }
                    },
                },
                Scope = "openid",
                ResponseType = "id_token",
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role",
                },
            };
            return resp;
        }
    }
}
