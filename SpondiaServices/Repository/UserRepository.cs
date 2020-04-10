using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using UserMedias.Utilities;
using ConfigurationManager = UserMedias.Utilities.ConfigurationManager;

namespace UserMedias.Repository
{
	public class UserRepository
	{

		public UserRepository(ConfigurationManager config)
		{

			try
			{
				CosmosService<LoginDetail>.Initalize(config);

			}
			catch (Exception ex)
			{
				throw new ApplicationException($"Database not initialized, {ex.Message}");
			}

		}
		public async Task<LoginDetail> AddUser(LoginDetail user)
		{

			var newuser =  await CosmosService<LoginDetail>.AddItemAsync(user);
			return newuser;
		}
	}
}
