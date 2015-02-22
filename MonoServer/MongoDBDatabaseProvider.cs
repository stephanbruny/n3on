using System;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;

namespace MonoServer
{
	public class MongoDBDatabaseProvider : IDatabaseProvider<MongoDatabase>
	{
		private MongoDatabase DB;
		private MongoServer DbServer;
		private string Host;
		private string DbName;

		public MongoDBDatabaseProvider (string host, string databaseName)
		{
			this.Host = host;
			this.DbName = databaseName;
		}

		public void Initialize() 
		{
			MongoServerSettings settings = new MongoServerSettings ();
			settings.ConnectionMode = ConnectionMode.Automatic;
			settings.Server = new MongoServerAddress (this.Host);
			this.DbServer = new MongoServer (settings);
			this.DB = this.DbServer.GetDatabase (this.DbName);
		}

		public MongoDatabase GetDatabase() 
		{
			return this.DB;
		}

		public void Create<M>(string collectionName, M data) {
			MongoCollection<M> collection = this.DB.GetCollection<M> (collectionName);
			collection.Insert<M> (data);
		}

		public M[] GetAll<M>(string collectionName) {
			MongoCollection<M> collection = this.DB.GetCollection<M> (collectionName);
			MongoCursor<M> cursor = collection.FindAll();
			List<M> result = new List<M> (cursor);
			return result.ToArray();
		}

		public void Close() 
		{
			this.DbServer.Disconnect ();
		}
	}
}

