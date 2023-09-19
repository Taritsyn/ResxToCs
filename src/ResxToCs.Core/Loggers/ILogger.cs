namespace ResxToCs.Core.Loggers
{
	/// <summary>
	/// Interface of logger
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Logs a information about the error
		/// </summary>
		/// <param name="message">Error message</param>
		void Error(string message);

		/// <summary>
		/// Logs a information about the error
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="args">Optional arguments for formatting the message string</param>
		void Error(string message, params object[] args);

		/// <summary>
		/// Logs a information about the warning
		/// </summary>
		/// <param name="message">Warning message</param>
		void Warn(string message);

		/// <summary>
		/// Logs a information about the warning
		/// </summary>
		/// <param name="message">Warning message</param>
		/// <param name="args">Optional arguments for formatting the message string</param>
		void Warn(string message, params object[] args);

		/// <summary>
		/// Logs a information
		/// </summary>
		/// <param name="message">Information message</param>
		void Info(string message);

		/// <summary>
		/// Logs a information
		/// </summary>
		/// <param name="message">Information message</param>
		/// <param name="args">Optional arguments for formatting the message string</param>
		void Info(string message, params object[] args);

		/// <summary>
		/// Logs a success
		/// </summary>
		/// <param name="message">Success message</param>
		void Success(string message);

		/// <summary>
		/// Logs a success
		/// </summary>
		/// <param name="message">Success message</param>
		/// <param name="args">Optional arguments for formatting the message string</param>
		void Success(string message, params object[] args);
	}
}