using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ResxToCs.MSBuild
{
	/// <summary>
	/// Task logger
	/// </summary>
	internal class TaskLogger : ResxToCs.Core.Loggers.ILogger
	{
		/// <summary>
		/// Task logging helper
		/// </summary>
		private readonly TaskLoggingHelper _taskLoggingHelper;


		/// <summary>
		/// Constructs instance of the task logger
		/// </summary>
		/// <param name="taskLoggingHelper">Task logging helper</param>
		public TaskLogger(TaskLoggingHelper taskLoggingHelper)
		{
			_taskLoggingHelper = taskLoggingHelper;
		}


		/// <inheritdoc/>
		public void Error(string message)
		{
			_taskLoggingHelper.LogError(message);
		}

		/// <inheritdoc/>
		public void Error(string message, params object[] args)
		{
			_taskLoggingHelper.LogError(message, args);
		}

		/// <inheritdoc/>
		public void Warn(string message)
		{
			_taskLoggingHelper.LogWarning(message);
		}

		/// <inheritdoc/>
		public void Warn(string message, params object[] args)
		{
			_taskLoggingHelper.LogWarning(message, args);
		}

		/// <inheritdoc/>
		public void Info(string message)
		{
			_taskLoggingHelper.LogMessage(MessageImportance.High, message);
		}

		/// <inheritdoc/>
		public void Info(string message, params object[] args)
		{
			_taskLoggingHelper.LogMessage(MessageImportance.High, message, args);
		}

		/// <inheritdoc/>
		public void Success(string message)
		{
			_taskLoggingHelper.LogMessage(MessageImportance.High, message);
		}

		/// <inheritdoc/>
		public void Success(string message, params object[] args)
		{
			_taskLoggingHelper.LogMessage(MessageImportance.High, message,args);
		}
	}
}