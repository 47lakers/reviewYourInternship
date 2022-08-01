using System;
namespace reviewYourInternship.Settings
{
	public class MongoDbConfig
	{
		public string Name { get; set; }

		public string Host { get; set; }

		public string Port { get; set; }

		public string ConnectionString => $"mongodb+srv://{Host}:{Port}@cluster0.0i4a8.mongodb.net/test";

	}
}

