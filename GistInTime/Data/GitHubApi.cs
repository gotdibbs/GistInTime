using GistInTime.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GistInTime.Data
{
    public class GitHubApi
    {
        public const string ClientId = "c0030de3ec5cfaff1542";
        public const string ClientSecret = "063c8d7df47fbc338bc19a17a43241df17443662";

        public const string BaseUrl = "https://api.github.com";

        public string AuthToken { get; set; }

        public GitHubApi(string authToken = null)
        {
            AuthToken = authToken;
        }

        #region Gists API

        public async Task<string> GetGistContents(string url)
        {
            try
            {
                var request = GetHttpRequest(HttpMethod.Get, url);

                request.Headers["Authorization"] = string.Concat("bearer ", AuthToken);

                return await GetResponseText(request);
            }
            catch (Exception ex)
            {
                MessageBox.Show("GistInTime Error: " + ex.Message);
                return null;
            }
        }

        public async Task<GistsResponse[]> GetMine()
        {
            try
            {
                var response = await ExecuteRequest<GistsResponse[]>(
                    HttpMethod.Get, BaseUrl + "/gists", null, AuthToken);

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show("GistInTime Error: " + ex.Message);
                return null;
            }
        }

        public async Task<GistsResponse[]> GetStarred()
        {
            try
            {
                var response = await ExecuteRequest<GistsResponse[]>(
                    HttpMethod.Get, BaseUrl + "/gists/starred", null, AuthToken);

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show("GistInTime Error: " + ex.Message);
                return null;
            }
        }

        #endregion

        public async Task<AuthorizationResponse> Authenticate(string username, string password)
        {
            try
            {
                var authRequest = new AuthorizationRequest
                {
                    client_id = ClientId,
                    client_secret = ClientSecret,
                    scopes = new List<string>
                    {
                        GithubScope.Gist
                    }
                };

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException("Both username and password are required.");
                }

                var request = GetHttpRequest(HttpMethod.Post, BaseUrl + "/authorizations");

                var authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
                request.Headers["Authorization"] = string.Concat("Basic ", authInfo);

                await WriteObject(request, authRequest);

                var json = await GetResponseText(request);
                var authResponse = Helpers.Json.Deserialize<AuthorizationResponse>(json);

                AuthToken = authResponse.token;

                return authResponse;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> RevokeToken(string username, string password, string tokenId)
        {
            try
            {
                var request = GetHttpRequest(HttpMethod.Delete, string.Concat(BaseUrl, "/authorizations/", tokenId));

                var authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
                request.Headers["Authorization"] = string.Concat("Basic ", authInfo);

                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.NoContent)
                    {
                        throw new Exception(string.Format("Server error (HTTP {0}: {1}).",
                            response.StatusCode,
                            response.StatusDescription));
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region Generic Request Helpers

        public async Task<T> ExecuteRequest<T>(
            HttpMethod method, 
            string requestUrl, 
            string body = "",
            string authToken = "",
            string username = "",
            string password = "")
        {
            var request = GetHttpRequest(method, requestUrl);

            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers["Authorization"] = string.Concat("bearer ", authToken);
            }
            else
            {
                throw new Exception("Authorization Token must be defined.");
            }

            var json = await GetResponseText(request);
            return Helpers.Json.Deserialize<T>(json);
        }

        private HttpWebRequest GetHttpRequest(HttpMethod method, string requestUrl)
        {
            var request = WebRequest.Create(requestUrl) as HttpWebRequest;
            request.Method = method.Method;

            // Required by GitHub API
            request.UserAgent = "GistInTime";

            return request;
        }

        private async Task<bool> WriteObject<T>(HttpWebRequest request, T obj)
        {
            var body = Helpers.Json.Serialize<T>(obj);

            if (!string.IsNullOrEmpty(body))
            {
                request.ContentType = "application/json";
                var bytes = Encoding.Default.GetBytes(body);
                using (var stream = await request.GetRequestStreamAsync())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                }
            }

            return true;
        }

        private async Task<string> GetResponseText(HttpWebRequest request)
        {
            using (var response = await request.GetResponseAsync() as HttpWebResponse)
            {
                if (response.StatusCode == HttpStatusCode.Forbidden ||
                    response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Authentication failure. Please try refreshing your auth token by logging out and then reauthorizing GistInTime.");
                }
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.Created &&
                    response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new Exception(string.Format("Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                }

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion
    }
}
