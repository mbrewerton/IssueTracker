using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using Microsoft.Azure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Security.Claims;
using System.Linq;
using Microsoft.IdentityModel.Protocols;

[assembly: OwinStartup(typeof(PTLogger.Startup))]

namespace PTLogger
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureSSOLogin(app);
        }


        private void ConfigureSSOLogin(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active, //Ensure that check occurs on every request
                AuthenticationType = "Cookies",
                CookieManager = new SystemWebCookieManager(),
                SlidingExpiration = false,
                CookieSecure = CookieSecureOption.Always
            });

            var stsUrl = CloudConfigurationManager.GetSetting("Login.STSUrl");

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = "spider",
                Authority = stsUrl, //Constants.BaseAddress,  //STS Server Address
                RedirectUri = CloudConfigurationManager.GetSetting("Login.CallbackUrl"), //This site
                PostLogoutRedirectUri = CloudConfigurationManager.GetSetting("Login.PostLogoutUrl"),
                ResponseType = "code id_token",
                Scope = "openid",
                SignInAsAuthenticationType = "Cookies",
                UseTokenLifetime = true,
                ClientSecret = CloudConfigurationManager.GetSetting("Login.ClientSecret"),

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async n =>
                    {
                        var receivedIdentity = n.AuthenticationTicket.Identity;

                        var id = new ClaimsIdentity(receivedIdentity.AuthenticationType);

                        //Strip claims
                        var claims = new List<Claim>(from c in receivedIdentity.Claims
                                                     where c.Type != "iss" &&
                                                           c.Type != "aud" &&
                                                           c.Type != "nbf" &&
                                                           c.Type != "exp" &&
                                                           c.Type != "iat" &&
                                                           c.Type != "nonce" &&
                                                           c.Type != "c_hash" &&
                                                           c.Type != "at_hash"
                                                     select c);

                        claims.Add(new Claim("id_token", n.ProtocolMessage.IdToken));

                        //JP - Set this in config:
                        n.AuthenticationTicket.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddHours(10);
                        n.AuthenticationTicket.Properties.AllowRefresh = true;
                        n.AuthenticationTicket.Properties.IssuedUtc = DateTimeOffset.UtcNow;

                        n.AuthenticationTicket = new AuthenticationTicket(
                            new ClaimsIdentity(claims.Distinct(new ClaimComparer()),
                                n.AuthenticationTicket.Identity.AuthenticationType),
                                n.AuthenticationTicket.Properties);
                    },

                    RedirectToIdentityProvider = n =>
                    {
                        // if signing out, add the id_token_hint
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                            if (idTokenHint != null)
                            {
                                n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                            }

                        }

                        return Task.FromResult(0);
                    }
                }
            });
        }

    }
}
