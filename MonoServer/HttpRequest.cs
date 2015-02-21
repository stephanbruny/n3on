using System;

namespace MonoServer
{
	public class HttpRequest
	{
		public HttpHeader Header {get; set;}
		public string Body { get; set; }

		public HttpRequest (HttpHeader header)
		{
			this.Header = header;
			// TODO: Parse Body
		}
	}
}

