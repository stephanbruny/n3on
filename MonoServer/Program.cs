﻿using System;
using System.Web;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Collections.Generic;
using Mono.CSharp;

namespace MonoServer
{
	class ServerConfiguration {
		public int Port {get; set;}
		public string Address { get; set; }
		public string Static {get; set;}
	}

	class ApplicationLocals 
	{
		public string AppTitle { get; set; }
	}

	class ServerRequest 
	{
		public TcpClient Client { get; set; }
		public byte[] Message { get; set; }
	}

	class MainClass
	{
		private static HttpServer Server;
		private static ServerConfiguration Config;

		public static List<Func<HttpHeader, HttpResponse, Func<HttpHeader>, HttpHeader >> Middleware;

		public static ApplicationLocals _locals = new ApplicationLocals {AppTitle = "N3ON Server"};

		private static void Initialize() 
		{
			Config = ServerUtils.JsonFileToObject<ServerConfiguration>("./config.json");
			Server = new HttpServer (Config.Address, Config.Port, HandleRequest);
			Middleware = new List<Func<HttpHeader, HttpResponse, Func<HttpHeader>, HttpHeader>> ();
		}

		[MTAThread]
		public static void Main (string[] args)
		{
			Initialize ();
			Router router = new Router ();
			router.Add("/test/[0-9]+$", Test);
			Middleware.Add (router.Middleware);
			Middleware.Add (ServeStatic);
			Middleware.Add (NotFound404);

			new Thread (() => {
				Server.WaitForConnection ();
			}).Start ();

			while (true) 
			{
				Thread.Sleep (1000);
			}

		}

		public static HttpHeader RunMiddleware(HttpHeader header, HttpResponse response, int current = 0) {
			return Middleware[current] (header, response, () => {
				if (current < Middleware.Count - 1) {
					current ++;
					return RunMiddleware(header, response, current);
				}
				return header;
			});
		} 

		public static HttpHeader Test(HttpHeader header, HttpResponse response) {
			response.Send ("Just a Test. Page is " + header.UrlParts[2]);
			return header;
		}

		public static HttpHeader HandleRequest(HttpHeader header, HttpResponse response) 
		{
			return RunMiddleware (header, response);
		}

		public static HttpHeader ServeStatic(HttpHeader header, HttpResponse response, Func<HttpHeader> next) {
			try {
				string content = ServerUtils.ReadTextFile (Config.Static + header.Url);
				string ext = header.Url.Substring(header.Url.LastIndexOf('.'));
				string mime = MimeType.GetMimeType(ext);
				response.SetHeader("Content-Type", mime);
				response.Send(content);
			} catch (Exception ex) {
				string x = ex.Message;
				response.Status (404, "Not found");
				next ();
			}
			return header;
		}

		public static HttpHeader NotFound404(HttpHeader header, HttpResponse response, Func<HttpHeader> next) {
			response.Status (404, "Not found");
			response.Send ("404 - Not found");
			return header;
		}

		public static HttpHeader ServeDefault(HttpHeader header, HttpResponse response) {
			response.Send (GetDefaultHtml ());
			return header;
		}
			
		private static string GetDefaultHtml() 
		{
			string html = ServerUtils.ReadTextFile ("default.html");
			HttpHeader headers = new HttpHeader ();

			headers.Status (200, "OK");
			headers.SetHeaders ("Server", "N3ON Server Version 0.0.0.1");
			headers.SetHeaders ("Content-Length", html.Length.ToString());
			headers.SetHeaders ("Content-Type", "text/html");
			return HttpHeader.Serialize (headers) + html;
		}
	}
}