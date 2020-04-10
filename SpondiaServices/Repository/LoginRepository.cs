using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Configuration;
using UserMedias.Utilities;
using Microsoft.Extensions.Logging;


namespace UserMedias.Repository
{

	public class LoginRepository
	{
		Container _container;
		public  LoginRepository(ConfigurationManager config)
		{
			try
			{
				 CosmosService<LoginDetail>.Initalize(config);

			}catch(Exception ex)
			{
					throw new ApplicationException($"Database not initialized, {ex.Message}");
			}
		}
		public async Task<LoginDetail> GetLoginDetailsAsync(LoginDetail login)
		{
			

			LoginDetail loginDetail = null;
			try
			{
				loginDetail = await CosmosService<LoginDetail>.GetItemAsync<LoginDetail>(login.Email);
			}
			catch (Exception ex) 
			{
				if (ex.Message == "User not found")
					return null;
				else throw;
			}
			return loginDetail;

		}
	}
}
