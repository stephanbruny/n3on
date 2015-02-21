using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MonoServer
{
	public class HttpServer
	{
		private TcpListener Listener;
		private IPEndPoint  EndPoint;
		public  IPAddress   Address;
		private Func<HttpHeader, HttpResponse, HttpHeader> OnRequest;

		public HttpServer (string address, int port, Func<HttpHeader, HttpResponse, HttpHeader> onRequest)
		{
			this.Address = IPAddress.Parse(address);
			this.EndPoint = new IPEndPoint (this.Address, port);
			this.Listener = new TcpListener (this.EndPoint);
			this.Listener.Start ();
			this.OnRequest = onRequest;
		}

		private void Close() 
		{
			this.Listener.Stop ();
		}

		public void WaitForConnection() {
			this.Listener.BeginAcceptTcpClient (new AsyncCallback (ProcessClient), new object());
		}

		private void WaitForRequest(TcpClient tcpClient, NetworkStream stream, AsyncCallback callback) 
		{
			byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
			stream.BeginRead (buffer, 0, buffer.Length, callback, new ServerRequest { Client = tcpClient, Message = buffer });
		}

		private void RequestCallback(IAsyncResult asyncResult)
		{
			ServerRequest req = (ServerRequest)asyncResult.AsyncState;
			NetworkStream stream = req.Client.GetStream ();
			int read = stream.EndRead (asyncResult);
			if (read == 0) {
				stream.Close ();
				req.Client.Close ();
				return;
			}

			string data = System.Text.Encoding.UTF8.GetString(req.Message, 0, read);
			HttpHeader requestHeader = HttpHeader.Deserialize (data);

			HttpResponse response = new HttpResponse (stream);

			try {
				this.OnRequest(requestHeader, response);
			} catch (Exception) {
				response.Status (500, "Internal Server Error");
				response.Send ("500 - Internal Server Error");
			}
			WaitForRequest (req.Client, req.Client.GetStream(), new AsyncCallback (RequestCallback));
		}

		private void ProcessClient(IAsyncResult asyncResult) 
		{
			try {
				TcpClient client = default(TcpClient);
				client = this.Listener.EndAcceptTcpClient(asyncResult);

				NetworkStream clientStream = client.GetStream ();
				WaitForRequest(client, clientStream, new AsyncCallback(RequestCallback));

			} catch (Exception ex) {
				ServerUtils.addError (new ServerError { Message = "Error in TCP-Connection", Ex = ex });
			}
			WaitForConnection ();
		}
	}
}

