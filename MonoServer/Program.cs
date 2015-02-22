using System;
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
	class ServerDatabase {
		public string Host { get; set; }
		public string Name { get; set; }
	}

	class ServerConfiguration {
		public int Port {get; set;}
		public string Address { get; set; }
		public string Static {get; set;}
		public ServerDatabase Database { get; set; } 
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

		private static MongoDBDatabaseProvider DatabaseProvider;

		private static void Initialize() 
		{
			Config = ServerUtils.JsonFileToObject<ServerConfiguration>("./config.json");
			Server = new HttpServer (Config.Address, Config.Port, HandleRequest);
			Middleware = new List<Func<HttpHeader, HttpResponse, Func<HttpHeader>, HttpHeader>> ();
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine ("N3ON Server initialized");

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine("Trying to connect MongoDB");
			DatabaseProvider = new MongoDBDatabaseProvider (Config.Database.Host, Config.Database.Name);
			try {
				DatabaseProvider.Initialize ();
				DatabaseProvider.Create<Models.Article> ("articles", Models.Article.New("test2", "Test Article 2", "This is a article for testing purposes.", DateTime.Now));
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Success.");
			} catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine ("Could not initialize database or insert document: " + ex.Message);
			}

		}

		[MTAThread]
		public static void Main (string[] args)
		{
			Initialize ();
			Router router = new Router ();
			router.Add("/test/[0-9]+$", Test);
			router.Add("/test/?$", TestFrontend);
			router.Add("/articles", Articles);
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

		public static HttpHeader Articles(HttpHeader header, HttpResponse response) {
			try {
				string template = ServerUtils.ReadTextFile ("templates/articles.html");
				Frontend fe = new Frontend ();
				response.SetHeader("Content-Type", "text/html");
				response.Send(fe.Compile(template, new { articles = DatabaseProvider.GetAll<Models.Article>("articles") } ));
				return header;
			} catch (Exception ex) {
				response.Send ("Error compiling template:\n" + ex.Message);
				return header;
			}
		}

		public static HttpHeader TestFrontend(HttpHeader header, HttpResponse response) {
			try {
				string template = ServerUtils.ReadTextFile ("templates/test.html");
				Frontend fe = new Frontend ();
				response.SetHeader("Content-Type", "text/html");
				response.Send(fe.Compile(template, new { content = "Test", pageTitle = "N3ON Testpage"}));
				return header;
			} catch (Exception ex) {
				response.Send ("Error compiling template:\n" + ex.Message);
				return header;
			}
		}


		public static HttpHeader HandleRequest(HttpHeader header, HttpResponse response) 
		{
			return RunMiddleware (header, response);
		}

		public static HttpHeader ServeStatic(HttpHeader header, HttpResponse response, Func<HttpHeader> next) {
			try {
				byte[] content = ServerUtils.ReadFile (Config.Static + header.Url);
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
