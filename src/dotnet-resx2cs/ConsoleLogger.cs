using System;

using ResxToCs.Core.Loggers;

namespace ResxToCs.DotNet
{
	/// <summary>
	/// Console logger
	/// </summary>
	internal class ConsoleLogger : ILogger
	{
		/// <inheritdoc/>
		public void Error(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(message);
			Console.ResetColor();
		}

		/// <inheritdoc/>
		public void Error(string message, params object[] args)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(message, args);
			Console.ResetColor();
		}

		/// <inheritdoc/>
		public void Warn(string message)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Error.WriteLine(message);
			Console.ResetColor();
		}

		/// <inheritdoc/>
		public void Warn(string message, params object[] args)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Error.WriteLine(message, args);
			Console.ResetColor();
		}

		/// <inheritdoc/>
		public void Info(string message)
		{
			Console.Out.WriteLine(message);
		}

		/// <inheritdoc/>
		public void Info(string message, params object[] args)
		{
			Console.Out.WriteLine(message, args);
		}

		/// <inheritdoc/>
		public void Success(string message)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Out.WriteLine(message);
			Console.ResetColor();
		}

		/// <inheritdoc/>
		public void Success(string message, params object[] args)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Out.WriteLine(message, args);
			Console.ResetColor();
		}
	}
}