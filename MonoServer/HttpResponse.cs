using System;
using System.Net;
using System.Net.Sockets;

namespace MonoServer
{
	public class HttpResponse
	{
		public HttpHeader Header { get; set; }
		private NetworkStream Stream;
		private System.Text.Encoding Encoding;
		private string EncodingName;

		public HttpResponse (NetworkStream stream)
		{
			this.Stream = stream;
			this.Header = new HttpHeader ();
			this.Encoding = System.Text.Encoding.UTF8;
			this.EncodingName = "utf-8";
		}

		public void Status(int status, string message) {
			this.Header.Status (status, message);
		}

		public void Send(string message) {
			this.Header.Status (200, "OK");
			this.Header.SetHeaders ("Content-Length", this.Encoding.GetBytes(message).Length.ToString (), true);
			this.Header.SetHeaders ("Content-Type", "text/plain; " + "charset=" + this.EncodingName);
			byte[] response = this.Encoding.GetBytes(HttpHeader.Serialize (this.Header) + message);
			this.Stream.Write (response, 0, response.Length);
			this.Stream.Flush();
		}

		public void SetEncoding(string encoding) {
			encoding = encoding.ToLower ();

			this.EncodingName = encoding;

			switch (encoding) {
			case "utf-8":
				this.Encoding = System.Text.Encoding.UTF8;
				break;
			case "ascii":
				this.Encoding = System.Text.Encoding.ASCII;
				break;
			case "utf-7":
				this.Encoding = System.Text.Encoding.UTF7;
				break;
			case "utf-32":
				this.Encoding = System.Text.Encoding.UTF32;
				break;
			case "unicode":
				this.Encoding = System.Text.Encoding.Unicode;
				break;
			default:
				this.Encoding = System.Text.Encoding.UTF8;
				this.EncodingName = "utf-8";
				break;
			}
		}

		public void SetHeader(string key, string value) {
			this.Header.SetHeaders (key, value);
		}
	}
}

