using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace FunctionApp1
{
    public static class usermedias
    {
        static ILogger _logger;
        [FunctionName("usermedias")]
        public static async Task<IActionResult> Run([HttpTrigger] HttpRequest req, ILogger log, ExecutionContext context)
        {
            _logger = log;
            log.LogInformation("C# HTTP trigger function processed a request......");
            string auth_code = req.Query["auth_code"];
            log.LogInformation("parsed query string.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (auth_code == null)
            {
                return new BadRequestObjectResult("authcode is missing in  query string parameter");
            }
            log.LogInformation("building config..");

            var config = new ConfigurationBuilder()
           .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
            log.LogInformation("loading.. config..");

            var clientId = config["instagram.clientid"].ToString();
            var clientSecret = config["instagram.clientsecret"].ToString();
            var grantType = "authorization_code";
            var redirectUri = config["redirect_uri"].ToString();

            log.LogInformation("loaded.. config..");


            HttpClient client = new HttpClient();

            var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret",clientSecret),
                    new KeyValuePair<string, string>("grant_type",grantType),
                    new KeyValuePair<string, string>("redirect_uri",redirectUri),
                    new KeyValuePair<string, string>("code", auth_code)

                });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.instagram.com/oauth/access_token");
            request.Content = formContent;
            log.LogInformation($"Requesting auth token from instagram..");
            var result = await client.SendAsync(request);
            var response = await result.Content.ReadAsStringAsync();
            log.LogInformation($"Resposne received from instagram API..");

            if (result.StatusCode != HttpStatusCode.OK)
            {
                log.LogInformation($"Error !! {response}");
                return new BadRequestObjectResult($"An error occured while aquiring auth token,please see logs for details {response}");

            }

            var jsResult = (JObject)JsonConvert.DeserializeObject(response);
            string accessToken = (string)jsResult["access_token"];
            var user_id = jsResult["user_id"].ToString();

            var mediaLiast = await GetSelfMediaRecent(user_id, accessToken);

            return (ActionResult)new OkObjectResult(mediaLiast.ToString());

            }

        public static async Task<JArray> GetSelfMediaRecent(string user_id, string accessToken)
        {
            string url = "https://graph.instagram.com/me";
            JArray  mediaObjects = new JArray();
            string urlRequest = "?fields=media&access_token=";
            //string accesstoken = this.access_token;
            string fullUrl = url + urlRequest + accessToken;
            try
            {
                _logger.LogInformation("Fetching media from instagram..");
                HttpClient client = new HttpClient();
                var result = await client.GetAsync(fullUrl);
                var response = await result.Content.ReadAsStringAsync();

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError($"Error !! {response}");
                    throw new ApplicationException($"Error fetching media. {response}");

                }

                var jsResult = (JObject)JsonConvert.DeserializeObject(response);
                var media = (JArray)jsResult["media"]["data"];

                foreach(var item in media)
                {
                    var mediaId = item["id"].ToString();

                    var mediaQueryUri = "https://graph.instagram.com/" + mediaId + "?fields=id,media_type,media_url&access_token="+accessToken;

                    result = await client.GetAsync(mediaQueryUri);
                    response = await result.Content.ReadAsStringAsync();

                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError($"Error !! {response}");
                        throw new ApplicationException($"Error fetching media info. {response}");

                    }

                    jsResult = (JObject)JsonConvert.DeserializeObject(response);
                    mediaObjects.Add(jsResult);

                }

                return mediaObjects;


            }
            catch (Exception e)
            {
                throw new ApplicationException($"Error fetching media. {e.Message}");

            }

        }
    }
}
