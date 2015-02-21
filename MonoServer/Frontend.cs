using System;
using Mustache;

namespace MonoServer
{
	public class Frontend
	{
		private Mustache.FormatCompiler Compiler;

		public Frontend ()
		{
			this.Compiler = new FormatCompiler ();
		}

		public string Compile(string template, object context) {
			Generator gen = this.Compiler.Compile (template);
			return gen.Render (context);
		}
	}
}

