using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserMedias.Utilities;
using UserMedias.Repository;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace UserMedias
{
    public static class AddUser
    {
        [FunctionName("AddUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            LoginDetail user = JsonConvert.DeserializeObject<LoginDetail>(requestBody);
            
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                return new BadRequestObjectResult(new { message = "Incomplete input data, Please provide all required fields.", code = 400 });
            
            if(user.Password?.Length < 6)
                return new BadRequestObjectResult(new { message = "Weak password.Minimum 6 characters required", code = 400 });

            user.id = user.Email;//neede by cosmos document.
            
            ConfigurationManager config = ConfigurationManager.GetInstance(context);
            LoginDetail newuser = null; ;
            try
            {
                UserRepository repo = new UserRepository(config);
                LoginRepository repository = new LoginRepository(config);

                newuser = await repo.AddUser(user);


            }
            catch (CosmosException ex)
            {
                log.LogInformation("Exception," + ex.StackTrace.ToString());
                if (ex.StatusCode == HttpStatusCode.Conflict)
                    return new BadRequestObjectResult(new { message = "User exists,Please supply diffrent user name", code = 409 });

            }
            catch (Exception ex)
            {
                log.LogInformation("Exception,"+ex.StackTrace.ToString());
                return new BadRequestObjectResult(new { message = "An error occured while creating user", code = 500 });

            }
            return new OkObjectResult(newuser);



        }
    }
}
