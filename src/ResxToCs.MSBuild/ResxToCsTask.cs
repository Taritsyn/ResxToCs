using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using ResxToCs.Core;
using ResxToCs.Core.Helpers;

namespace ResxToCs.MSBuild
{
	/// <summary>
	/// An MSBuild task for converting the <code>.resx</code> files to the <code>.Designer.cs</code> files
	/// </summary>
	public sealed class ResxToCsTask : Task
	{
		/// <summary>
		/// The directory containing <code>.resx</code> files
		/// </summary>
		public string InputDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// The namespace into which the output of the converter is placed
		/// </summary>
		public string Namespace
		{
			get;
			set;
		}


		/// <summary>
		/// Execute the Task
		/// </summary>
		public override bool Execute()
		{
			bool result = true;
			string resourceDirectory = InputDirectory;
			string resourceNamespace = Namespace;
			string currentDirectory = Directory.GetCurrentDirectory();

			if (!string.IsNullOrWhiteSpace(resourceDirectory))
			{
				resourceDirectory = PathHelpers.ProcessSlashes(resourceDirectory.Trim());
				if (!Path.IsPathRooted(resourceDirectory))
				{
					resourceDirectory = Path.Combine(currentDirectory, resourceDirectory);
				}
				resourceDirectory = Path.GetFullPath(resourceDirectory);

				if (!Directory.Exists(resourceDirectory))
				{
					WriteErrorLine("The {0} directory does not exist.", resourceDirectory);
					return false;
				}
			}
			else
			{
				resourceDirectory = currentDirectory;
			}

			WriteInfoLine();
			WriteInfoLine("Starting conversion of `.resx` files in the '{0}' directory:", resourceDirectory);
			WriteInfoLine();

			int processedFileCount = 0;
			int сonvertedFileCount = 0;
			int unconvertedFileCount = 0;

			foreach (string filePath in Directory.EnumerateFiles(resourceDirectory, "*.resx", SearchOption.AllDirectories))
			{
				string relativeFilePath = filePath.Substring(resourceDirectory.Length);

				try
				{
					FileConversionResult conversionResult = ResxToCsConverter.ConvertFile(filePath,
						resourceNamespace);
					string outputFilePath = conversionResult.OutputPath;
					string outputDirectoryPath = Path.GetDirectoryName(outputFilePath);

					if (!Directory.Exists(outputDirectoryPath))
					{
						Directory.CreateDirectory(outputDirectoryPath);
					}

					File.WriteAllText(outputFilePath, conversionResult.ConvertedContent);

					WriteInfoLine("	* '{0}' file has been successfully converted", relativeFilePath);
					сonvertedFileCount++;
				}
				catch (ResxConversionException e)
				{
					WriteInfoLine("	* '{0}' file failed to convert");
					WriteErrorLine(e.Message);
					unconvertedFileCount++;
				}

				processedFileCount++;
			}

			if (processedFileCount > 0)
			{
				WriteInfoLine();
				WriteInfoLine("Total files: {0}. Converted: {1}. Failed: {2}.",
					processedFileCount, сonvertedFileCount, unconvertedFileCount);

				result = processedFileCount == сonvertedFileCount;
				if (result)
				{
					WriteSuccessLine("Conversion is successfull.");
				}
				else
				{
					WriteErrorLine("Conversion is failed.");
				}
			}
			else
			{
				WriteWarnLine("There are no resx files found in the '{0}' directory.", resourceDirectory);
			}

			WriteInfoLine();

			return result;
		}

		/// <summary>
		/// Writes a information about the error and a new line
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private void WriteErrorLine(string message, params object[] messageArgs)
		{
			Log.LogError(message, messageArgs);
		}

		/// <summary>
		/// Writes a information about the warning and a new line
		/// </summary>
		/// <param name="message">Warning message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private void WriteWarnLine(string message, params object[] messageArgs)
		{
			Log.LogWarning(message, messageArgs);
		}

		/// <summary>
		/// Writes a information and a new line
		/// </summary>
		/// <param name="message">Information message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private void WriteInfoLine(string message, params object[] messageArgs)
		{
			Log.LogMessage(MessageImportance.High, message, messageArgs);
		}

		/// <summary>
		/// Writes a line terminator
		/// </summary>
		private void WriteInfoLine()
		{
			Log.LogMessage(MessageImportance.High, string.Empty);
		}

		/// <summary>
		/// Writes a information about the success and a new line
		/// </summary>
		/// <param name="message">Success message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private void WriteSuccessLine(string message, params object[] messageArgs)
		{
			Log.LogMessage(MessageImportance.High, message, messageArgs);
		}
	}
}