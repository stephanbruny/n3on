using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MonoServer
{
	public class ServerError 
	{
		public string Message {get; set;}
		public DateTime Time {get; set;}
		public Exception Ex { get; set; }
	}

	public class ServerUtils
	{
		private static List<ServerError> _errorStack = new List<ServerError>();

		public ServerUtils ()
		{
			_errorStack = new List<ServerError>();
		}

		public static void addError(ServerError Error) 
		{
			_errorStack.Add(Error);
		}

		public static byte[] ReadFile(string path)
		{
			try
			{
				return File.ReadAllBytes(path);
			}
			catch (Exception e)
			{
				ServerUtils.addError(new ServerError { Message = "Could not read file " + path, Time = DateTime.Now, Ex = e });
				return null;
			}
		}

		public static string ReadTextFile(string path)
	    {
	        try
	        {
	            using (StreamReader sr = new StreamReader(path))
	            {
	                String content = sr.ReadToEnd();
					sr.Close();
					return content;
	            }
	        }
	        catch (Exception e)
	        {
				ServerUtils.addError(new ServerError { Message = "Could not read file " + path, Time = DateTime.Now, Ex = e });
				return null;
	        }
	    }

		public static T JsonFileToObject<T>(string path) 
		{
			try {
				string json = ServerUtils.ReadTextFile(path);
				T obj = JsonConvert.DeserializeObject<T>(json);
				return obj;
			} 
			catch (Exception ex) 
			{
				ServerUtils.addError(new ServerError { Message = "Could not find or parse json file " + path, Time = DateTime.Now, Ex = ex });
				return default(T);
			}
		}
	}
}

