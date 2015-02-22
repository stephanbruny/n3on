using System;
using MongoDB;
using MongoDB.Driver;
using MonoServer;

namespace MonoServer.Models
{
	public class Article
	{
		public string Id { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime PublishDate { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }

		public bool Published { 
			get { 
				if (this.PublishDate <= DateTime.Now)
					return true;
				else
					return false; 
		    }
		}

		public static Article  New(string id, string title, string content, DateTime publish) {
			return new Article { Id = id, Title = title, Content = content, CreatedDate = DateTime.Now, PublishDate = publish };
		}
	}
}

