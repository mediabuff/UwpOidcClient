﻿using Newtonsoft.Json;
using System;
using System.Security.Claims;
using Windows.Security.Credentials;

namespace Authentication
{
    public class LoginResult
    {
        private static JsonSerializerSettings settings;

        public bool Success { get; set; }
        public string Error { get; set; }

        public ClaimsPrincipal Principal { get; set; }
        public string AccessToken { get; set; }
        public string IdentityToken { get; set; }

        public DateTime AccessTokenExpiration { get; set; }
        public DateTime AuthenticationTime { get; set; }

        static LoginResult()
        {
            settings = new JsonSerializerSettings();
            settings.Converters.Add(new ClaimsPrincipalConverter());
        }

        public static LoginResult Retrieve()
        {
            var vault = new PasswordVault();
            PasswordCredential credential;

            try
            {
                credential = vault.Retrieve("oidc", "login_result");
            }
            catch (Exception)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<LoginResult>(credential.Password, settings);
        }

        [JsonIgnore]
        public bool IsAccessTokenValid
        {
            get
            {
                return DateTime.Now < AccessTokenExpiration;
            }
        }

        public void Store()
        {
            var vault = new PasswordVault();

            try
            {
                var creds = vault.FindAllByResource("oidc");
                foreach (var cred in creds)
                {
                    vault.Remove(cred);
                }
            }
            catch { }

            var credential = new PasswordCredential(
                "oidc", 
                "login_result",
                JsonConvert.SerializeObject(this, settings));

            vault.Add(credential);
        }
    }
}