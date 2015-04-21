﻿#region
using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using Owin;
using VirtoCommerce.Web.Models.Security;
using System.Configuration;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Facebook;
using System.Security.Claims;

#endregion

namespace VirtoCommerce.Web
{
    public partial class Startup
    {
        #region Public Methods and Operators
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseSiteContext();
            app.UseStageMarker(PipelineStage.PostAuthorize);

            // Configure the db context, user manager and role manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationUserStore.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(
                new CookieAuthenticationOptions
                {
                    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                    LoginPath = new PathString("/Account/Login"),
                    Provider =
                        new CookieAuthenticationProvider
                        {
                            // Enables the application to validate the security stamp when the user logs in.
                            // This is a security feature which is used when you change a password or add an external login to your account.  
                            OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                                    TimeSpan.FromMinutes(30),(manager, user) => user.GenerateUserIdentityAsync(manager))
                        }
                });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            var facebookOptions = new FacebookAuthenticationOptions();
            facebookOptions.AppId = ConfigurationManager.AppSettings["OAuth2.Facebook.AppId"];
            facebookOptions.AppSecret = ConfigurationManager.AppSettings["OAuth2.Facebook.Secret"];
            app.UseFacebookAuthentication(facebookOptions);

            var googleOptions = new GoogleOAuth2AuthenticationOptions();
            googleOptions.ClientId = ConfigurationManager.AppSettings["OAuth2.Google.ClientId"];
            googleOptions.ClientSecret = ConfigurationManager.AppSettings["OAuth2.Google.Secret"];
            app.UseGoogleAuthentication(googleOptions);
        }
        #endregion

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
    }
}