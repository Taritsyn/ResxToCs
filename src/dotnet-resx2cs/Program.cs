using System;

namespace ResxToCs.DotNet
{
	internal class Program
	{
		/// <summary>
		/// The main entry point for command-line application
		/// </summary>
		static int Main(string[] args)
		{
			int returnCode = 0;
			var logger = new ConsoleLogger();

			try
			{
				var app = new App(args, logger);
				returnCode = app.Run();
			}
			catch (Exception e)
			{
				logger.Error(e.Message);
				returnCode = -1;
			}

			return returnCode;
		}
	}
}