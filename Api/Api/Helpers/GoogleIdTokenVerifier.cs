using Api.Entities;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Helpers
{
    public class GoogleIdTokenVerifier
    {
        private readonly AppSettings _appSettings;
        public GoogleIdTokenVerifier(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        public async Task<User> Verifify(string idToken, string authCode)
        {
            try
            {

                var validPayload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                var clientId = _appSettings.ClientId;
                var aud = validPayload.Audience;

                if (!clientId.Equals(aud)) return null;

                Test(authCode, validPayload.Subject);

                var user = new User()
                {
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

        public async Task Test(string authorizationCode, string userId)
        {
            // authorization code is sent by the client (web browser)
            const string dataStoreFolder = "googleApiStorageFile";

            // create authorization code flow with clientSecrets
            GoogleAuthorizationCodeFlow authorizationCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                DataStore = new FileDataStore(dataStoreFolder),
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = _appSettings.ClientId,
                    ClientSecret = _appSettings.ClientSecret
                },
                Prompt = "consent"
            });

            FileDataStore fileStore = new FileDataStore(dataStoreFolder);
            await fileStore.ClearAsync();
            TokenResponse tokenResponse = await fileStore.GetAsync<TokenResponse>(userId);

            if (tokenResponse == null)
            {
                try
                {

                    // token data does not exist for this user
                    tokenResponse = await authorizationCodeFlow.ExchangeCodeForTokenAsync(
                      userId, // user for tracking the userId on our backend system
                      authorizationCode,
                      "https://localhost:44329/signin-google", // redirect_uri can not be empty. Must be one of the redirects url listed in your project in the api console
                      CancellationToken.None);
                }
                catch (Exception e)
                {

                };
            }

            UserCredential userCredential = new UserCredential(authorizationCodeFlow, userId, tokenResponse);
            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Fields = "*";
            try
            {
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            }
            catch (Exception e)
            {

            }

            // If access token expires, the UserCredential automatically makes request with refresh_token
            // and gets new access_token
            // bool complete = await userCredential.RefreshTokenAsync(CancellationToken.None);
        }

    }
}
