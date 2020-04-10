using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System.Collections.Generic;
using System.Linq;
using UserMedias.Repository;
using UserMedias.Utilities;

namespace UserMedias
{
	public static class Login
	{
		[FunctionName("Login")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
			ILogger log, ExecutionContext context)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			dynamic input = JsonConvert.DeserializeObject(requestBody);
			
			LoginDetail result = null;
			try
			{
				ConfigurationManager config =  ConfigurationManager.GetInstance(context);
				var user = new LoginDetail { Email = input.email, Password = input.password };
				LoginRepository repository = new LoginRepository(config);
				result = await repository.GetLoginDetailsAsync(user);
			}
			catch (Exception ex)
			{

				log.LogInformation(ex.Message);
				return new BadRequestObjectResult(new { message ="A server error occured, please retry or contact the support if problem persists.",exception = ex.Message});
			}

			return result?.Email != null
				? (ActionResult)new OkObjectResult(new {status ="succes"})
				: new BadRequestObjectResult(new { status="user not found"});
		}
	}
}
