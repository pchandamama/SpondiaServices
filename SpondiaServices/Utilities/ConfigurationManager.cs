using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;

namespace UserMedias.Utilities
{
	public  class  ConfigurationManager
	{
		 IConfigurationRoot _config;
		 static ConfigurationManager _instance;
		private  ConfigurationManager(ExecutionContext context)
		{
			try
			{
				var config = new ConfigurationBuilder()
			   .SetBasePath(context.FunctionAppDirectory)
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
				this._config = config;

			}
			catch(Exception ex)
			{
				throw new ApplicationException($"Error loading configuration {ex.Message}");
			}

		}
		public string GetValue(string key)
		{
			return _config[key];
		}
		public static ConfigurationManager GetInstance(ExecutionContext context)
		{
			if(_instance == null)
				_instance = new ConfigurationManager(context);
			 return _instance;
		}
	}
}
