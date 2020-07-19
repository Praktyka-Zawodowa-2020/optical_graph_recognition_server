using Api.Entities;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Helpers
{
    public class GoogleAuthHandler
    {
        private readonly AppSettings _appSettings;
        public GoogleAuthHandler(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public User Authenticate(string idToken, string authCode)
        {
            // validate google id token
            var paylodedUser = this.VerififyIdToken(idToken).Result;
            // return null if token invalid
            if (paylodedUser == null) return null;

            // obtain google credentails
            var userCredentials = this.ObtainApiCredentails(authCode, paylodedUser.GoogleId).Result;
            // return null if authcode invalid
            if (userCredentials == null) return null;

            return paylodedUser;
        }

        private async Task<User> VerififyIdToken(string idToken)
        {
            try
            {
                var validPayload = await GoogleJsonWebSignature.ValidateAsync(idToken);

                var clientId = _appSettings.Secrets.ClientId;
                var aud = validPayload.Audience;
                if (!clientId.Equals(aud)) return null;

                var user = new User()
                {
                    GoogleId = validPayload.Subject,
                    Mail = validPayload.Email,
                    RefreshTokens = new List<RefreshToken>()
                };
                return user;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<UserCredential> ObtainApiCredentails(string authorizationCode, string userId)
        {
            // authorization code is sent by the client (web browser)
            string dataStoreFolder = _appSettings.Secrets.StorageFile;

            // create authorization code flow with clientSecrets
            GoogleAuthorizationCodeFlow authorizationCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                DataStore = new FileDataStore(dataStoreFolder),
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = _appSettings.Secrets.ClientId,
                    ClientSecret = _appSettings.Secrets.ClientSecret
                }
            });

            FileDataStore fileStore = new FileDataStore(dataStoreFolder);
            TokenResponse tokenResponse = await fileStore.GetAsync<TokenResponse>(userId);

            if (tokenResponse == null)
            {
                try
                {
                    // token data does not exist for this user
                    tokenResponse = await authorizationCodeFlow.ExchangeCodeForTokenAsync(
                      userId, // user for tracking the userId on our backend system
                      authorizationCode,
                      _appSettings.Secrets.RedirectUri, // redirect_uri can not be empty. Must be one of the redirects url listed in your project in the api console
                      CancellationToken.None);
                }
                catch (Exception e)
                {
                    return null;
                };
            }

            UserCredential userCredential = new UserCredential(authorizationCodeFlow, userId, tokenResponse);
            return userCredential;

            // If access token expires, the UserCredential automatically makes request with refresh_token
            // and gets new access_token
            // bool complete = await userCredential.RefreshTokenAsync(CancellationToken.None);
        }
        public async Task<UserCredential> GetUserCredentials(string userId)
        {
            string dataStoreFolder = _appSettings.Secrets.StorageFile;

            // create authorization code flow with clientSecrets
            GoogleAuthorizationCodeFlow authorizationCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                DataStore = new FileDataStore(dataStoreFolder),
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = _appSettings.Secrets.ClientId,
                    ClientSecret = _appSettings.Secrets.ClientSecret
                }
            });

            FileDataStore fileStore = new FileDataStore(dataStoreFolder);
            TokenResponse tokenResponse = await fileStore.GetAsync<TokenResponse>(userId);

            if (tokenResponse == null)
            {
                return null;
            }

            UserCredential userCredential = new UserCredential(authorizationCodeFlow, userId, tokenResponse);
            return userCredential;
        }
    }

}
