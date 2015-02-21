using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MonoServer
{
	public class Router
	{
		private Dictionary<Regex, Func<HttpHeader, HttpResponse, HttpHeader>> Routes;

		public Router ()
		{
			this.Routes = new Dictionary<Regex, Func<HttpHeader, HttpResponse, HttpHeader>> ();
		}

		public HttpHeader Middleware(HttpHeader header, HttpResponse response, Func<HttpHeader> next) {
			foreach (KeyValuePair<Regex,  Func<HttpHeader, HttpResponse, HttpHeader>> keyValue in this.Routes) {
				if (keyValue.Key.IsMatch(header.Url)) {
					// TODO: Context (HttpRequest)
					keyValue.Value(header, response);
				}
			}

			return next ();
		}

		public void Add(string route, Func<HttpHeader, HttpResponse, HttpHeader> callback) {
			Routes.Add (new Regex(route), callback);
		}
	}
}

