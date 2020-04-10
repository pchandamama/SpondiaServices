using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using UserMedias.Repository;

namespace UserMedias.Utilities
{
	internal static class CosmosService<T>
	{
		static ConfigurationManager _config;
		static Container _container;
		public static void Initalize(ConfigurationManager configuration)
		{
			if (_container != null)//only once intialize as it is static
				return;
			_config = configuration;
			var endpointUrl = _config.GetValue("CosmosEndpoint");
			var authorizationKey = _config.GetValue("CosmosAuthKey");
			var databaseName = _config.GetValue("CosmosDatabaseName");
			string containerName = _config.GetValue("CosmosContainerName");

			CosmosClientBuilder clientBuilder = new CosmosClientBuilder(endpointUrl, authorizationKey);
			CosmosClient client = clientBuilder
								.WithConnectionModeDirect()
								.Build();
			_container = client?.GetContainer(databaseName, containerName);

		}
		public static async Task<T> AddItemAsync(T item)
		{
			return await _container.CreateItemAsync<T>(item);
		}

		public static  async Task <dynamic>DeleteItemAsync(string id)
		{
			return await _container.DeleteItemAsync<dynamic>(id, new PartitionKey(id));
		}

		public static async Task<T> GetItemAsync<T>(string id)
		{
			try
			{
				ItemResponse<T> response = await _container.ReadItemAsync<T>(id, new PartitionKey(id));
				return response;
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				throw new ApplicationException("User not found");
			}

		}

	}
}
