using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace MonoServer
{
	public class HttpHeader
	{
		public int      StatusCode { get; set; }
		public string   StatusMessage { get; set; }
		public string   Method { get; set; }
		public string   Url { get; set; }
		public string   Http { get; set; }
		public string   Body { get; set; }
		public string[] UrlParts { get; set; }

		public Dictionary<string, string> headers;

		public HttpHeader() {
			headers = new Dictionary<string, string> ();
		}

		public void SetHeaders(string key, string value, bool overwrite = false)
		{
			if (!this.headers.ContainsKey (key))
				this.headers.Add (key, value);
			else
				if (overwrite)
					this.headers [key] = value;
		}

		public string Get(string key) {
			if (headers.ContainsKey (key)) {
				return headers [key];
			}
			return null;
		}

		public void Status(int value, string message = "OK") 
		{
			this.StatusCode = value;
			this.StatusMessage = message;
		}
			
		public static string Serialize(HttpHeader header) {
			string result = "HTTP/1.1 " + header.StatusCode + " " + header.StatusMessage + "\n";
			foreach (KeyValuePair<string, string> pairs in header.headers) 
			{
				result += pairs.Key + ": " + pairs.Value + "\n";
			}

			return result + "\n";
		}

		public static HttpHeader Deserialize(string data) 
		{
			HttpHeader result = new HttpHeader ();
			List<string> lines = data.Split ('\n').ToList();

			string topHeader = lines [0];
			lines.RemoveAt (0);

			string httpRegexp = @"\S+";

			MatchCollection matches = Regex.Matches (topHeader, httpRegexp);
			result.Method = (matches.Count > 0) ? matches [0].Value : "GET";
			result.Url    = (matches.Count > 1) ? matches [1].Value : "/";
			result.Http   = (matches.Count > 2) ? matches [2].Value : "HTTP/1.1";

			result.UrlParts = result.Url.Split ('/');

			string headerKeyRegexp = @"^(.+)\:\s";
			string headerValRegexp = @"\:\s(.+)$";

			foreach (string line in lines) {
				string key = Regex.Match (line, headerKeyRegexp).Value.Replace (": ", " ");
				string value = Regex.Match(line, headerValRegexp).Value.Replace(": ", "");
				if (!String.IsNullOrEmpty (key)) {
					result.headers.Add (key, value);
				}
			}
			try {
				result.Body = data.Substring (data.Length - int.Parse (result.headers ["Content-Length"]));
			} catch (Exception ex) {
				ServerUtils.addError (new ServerError { Message = "Could not send body", Ex = ex });
				result.Body = null;
				result.SetHeaders ("Content-Length", "0");
			}
			return result;
		}
	}
}

