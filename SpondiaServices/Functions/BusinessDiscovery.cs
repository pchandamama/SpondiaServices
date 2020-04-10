using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Dynamic;

namespace UserMedias.Functions
{
    public static class BusinessDiscovery
    {
        [FunctionName("BusinessDiscovery")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string token = req.Query["token"];
            string businessName = req.Query["businessname"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            token = token ?? data?.token;

            string Access_token = token;
            string url = "https://graph.facebook.com/";
            string ver = "v4.0/";
            string user_id = "17841405375274306";//owner
            string urlRequest = "?fields=business_discovery.username(" + businessName + "){name,followers_count,media_count,media{comments_count,like_count,id}}&access_token=";
            string fullUrl = url + ver + user_id + urlRequest + Access_token;
            var result = string.Empty;
            dynamic responseDTO = new ExpandoObject();
            responseDTO.mediaList = new List<dynamic>();

            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(fullUrl);
                result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JObject bdiscoveryData = JObject.Parse(result);
                    var business_Name = bdiscoveryData["business_discovery"]["name"];
                    var follower_count = (long)bdiscoveryData["business_discovery"]["followers_count"];
                    var media_count = (int)bdiscoveryData["business_discovery"]["media_count"];
                    var caption = bdiscoveryData["business_discovery"]["caption"];
					responseDTO.businessInfo = new
					{
						businessName = business_Name,
						followerCount = follower_count,
						mediaCount = media_count,
                        caption
                    };
					//List<dynamic> mediaList = new List<dynamic>();

					if (bdiscoveryData["business_discovery"]["media"]["data"] != null)
                    {
                        int i = 0;
                        foreach (JObject datavalue in bdiscoveryData["business_discovery"]["media"]["data"])
                        {
                            var Comments_count = (long)datavalue["comments_count"];// comment
                            var likes_count = (long)datavalue["like_count"];//likes
                            var Id = (long)datavalue["id"];
                            //var link_url = datavalue["permalink"].ToString();//links
                            //var Med_id = datavalue["id"].ToString();//image id
                            //var times = datavalue["timestamp"].ToString();
                            //var mediaUrl = datavalue["media_url"];
                            //var mediaType = datavalue["media_type"];
                            var mediaData = new
                            {
                                commentsCount = Comments_count,
                                likesCount = likes_count,
                                Id
                               // linkUrl = link_url,
                                //mediaId = Med_id,
                                //timeStamp = times,
                                //mediaUrl,
                                //mediaType
                            };
                            responseDTO.mediaList.Add(mediaData);

                        }

                    }
                }   
               
            }
            catch(Exception ex)
            {

                return new BadRequestObjectResult($"An error occured.{ex?.Message}");

            }

            if (string.IsNullOrEmpty(token))
                return new BadRequestObjectResult("token is missing in te request.");
                

            return new OkObjectResult(responseDTO);
        }
    }
}
