using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Dynamic;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;

namespace UserMedias.Functions
{
    public static class Recentmedia
    {
        [FunctionName("Recentmedia")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string token = req.Query["token"];
            string queryUser = req.Query["iguser"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            token = token ?? data?.token;
            queryUser = queryUser ?? data.iguser;
            dynamic result = null;
           
            try
            {
                result = await GetRecentMedia(token, queryUser);

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { ErrorInfo = "AneError occured, " + ex.Message.ToString() });

            }
            return new OkObjectResult(result);
        }
        private static async Task<dynamic> GetRecentMedia(string token, string businessName)
        {


            var igUser = await GetUserInfo(businessName, token);

            string url = "https://graph.facebook.com/"; 
            string ver = "v4.0/";
            string user_id = "17841405375274306";//owner
            string urlRequest = "/recent_media?fields=like_count,comments_count,media_type,permalink,caption,media_url&user_id=";
            string fullUrl = url + ver + igUser + urlRequest + user_id + "&access_token=" + token;

            var result = string.Empty;
            dynamic responseDTO = new ExpandoObject();
            responseDTO.MediaList = new List<dynamic>();

            HttpClient client = new HttpClient();
            var response = await client.GetAsync(fullUrl);
            result = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject media = JObject.Parse(result);

                foreach (JObject item in media["data"])
                {
                    var Comments_count = (int)item["comments_count"];// comment
                    var likes_count = (int)item["like_count"];//likes
                    var link_url = item["permalink"].ToString();//links
                    var Med_id = item["id"].ToString();//image id
                    var media_type = item["media_type"].ToString();
                    var media_url = item["media_url"]?.ToString();
                    var caption = item["caption"].ToString();


                    responseDTO.MediaList.Add(new
                    {
                        HashTag = igUser,
                        CommentsCount = Comments_count,
                        likesCount = likes_count,
                        LinkUrl = link_url,
                        Id = Med_id,
                        type = media_type,
                        mediaUrl = media_url,
                        Caption = caption,
                    });
                }
            }
            else
            {

                throw new ApplicationException(result.ToString());

            }

            return responseDTO;
        }
        public static async Task<string> GetUserInfo(string user, string token)
        {
            string ig_user = "";
            //  string access = "EAACV8WmSOJMBAP1BWjHJVDlRq3d0OTxWRKqOQZCQ4fxj2SIdyrvXdErXo8nl6X9NeWfOLEuun9OeYKXKVjTerrCAhjkRIFdCTB9WJJymDKv5HaSAkHtavve2RdBar1ClDoHcnIiqd6mCNjZBYxDiT4ZBSJvhSskmvmfqK0xpbe2ssXmRs6qe9kErzTFDRzi0B2uZByMIqgcZBELe8mRtOAgELWUxKZAtpvhSAoHQFOiJlFwfIvmC9T";
            string ver = "v4.0/";
            string url = "https://graph.facebook.com/";
            string userid = "17841405375274306";
            string urlRequest = "ig_hashtag_search?user_id=" + userid + "&q=" + user + "&access_token=" + token;
            var fullUrl = url + ver + urlRequest;
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(fullUrl);
            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var userData = JObject.Parse(result);
                foreach (JObject item in userData["data"])
                {
                    ig_user = item["id"].ToString();


                }

            }
            else
            {
                throw new ApplicationException(result.ToString()); ;

            }
            return ig_user;
        }
    }
}