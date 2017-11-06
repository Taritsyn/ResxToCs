using System;
using System.IO;
using System.Reflection;

using ResxToCs.Core;

namespace ResxToCs.DotNet
{
	/// <summary>
	/// Command-line application for converting the <code>.resx</code> files to the <code>.Designer.cs</code> files
	/// </summary>
	class Program
	{
		/// <summary>
		/// Application configuration settings
		/// </summary>
		private readonly AppConfiguration _appConfig;


		/// <summary>
		/// Constructs an instance of the command-line applicationfor converting the <code>.resx</code> files
		/// to the <code>.Designer.cs</code> files
		/// </summary>
		/// <param name="args">Command-line arguments</param>
		private Program(string[] args)
		{
			_appConfig = GetAppConfigurationFromArguments(args);
		}


		/// <summary>
		/// The main entry point for command-line application
		/// </summary>
		public static int Main(string[] args)
		{
			int returnCode = 0;

			try
			{
				var app = new Program(args);
				returnCode = app.Run();
			}
			catch (Exception e)
			{
				WriteErrorLine(e.Message);
				returnCode = -1;
			}

			return returnCode;
		}


		/// <summary>
		/// Runs a command-line application
		/// </summary>
		private int Run()
		{
			int returnCode = 0;
			AppMode appMode = _appConfig.Mode;

			switch (appMode)
			{
				case AppMode.Help:
					WriteHelp();
					break;
				case AppMode.Version:
					WriteVersion();
					break;
				case AppMode.Conversion:
					returnCode = Convert(_appConfig.ResourceDirectory) ? 0 : -1;
					break;
				default:
					throw new AppUsageException("Unknown application mode.");
			}

			return returnCode;
		}

		/// <summary>
		/// Gets a application configuration settings from command-line arguments
		/// </summary>
		/// <param name="args">Command-line arguments</param>
		/// <returns>Application configuration settings</returns>
		private static AppConfiguration GetAppConfigurationFromArguments(string[] args)
		{
			var appConfig = new AppConfiguration
			{
				Mode = AppMode.Unknown
			};

			if (args != null && args.Length > 0)
			{
				string firstArgument = args[0];

				if (firstArgument.StartsWith("-") && firstArgument.Length > 1)
				{
					string firstSwitchName = firstArgument.Substring(1).ToUpperInvariant();

					switch (firstSwitchName)
					{
						case "-HELP":
						case "H":
						case "?":
							appConfig.Mode = AppMode.Help;
							break;
						case "-VERSION":
						case "V":
							appConfig.Mode = AppMode.Version;
							break;
						default:
							throw new AppUsageException(
								string.Format("Unknown command switch `{0}`.", firstArgument));
					}
				}
				else if (firstArgument == "/?")
				{
					appConfig.Mode = AppMode.Help;
				}
				else
				{
					appConfig.Mode = AppMode.Conversion;
					appConfig.ResourceDirectory = TrimQuotes(firstArgument);
				}
			}
			else
			{
				appConfig.Mode = AppMode.Conversion;
			}

			return appConfig;
		}

		/// <summary>
		/// Converts a <code>.resx</code> files in specified directory
		/// </summary>
		/// <param name="resourceDirectory">The directory containing <code>.resx</code> files</param>
		/// <returns>Result of conversion (true - success; false - failure)</returns>
		private static bool Convert(string resourceDirectory)
		{
			bool result = true;
			string processedResourceDirectory = resourceDirectory;
			string currentDirectory = Directory.GetCurrentDirectory();

			if (!string.IsNullOrWhiteSpace(resourceDirectory))
			{
				if (!Path.IsPathRooted(resourceDirectory))
				{
					processedResourceDirectory = Path.Combine(currentDirectory, resourceDirectory);
				}

				processedResourceDirectory = Path.GetFullPath(processedResourceDirectory);

				if (!Directory.Exists(processedResourceDirectory))
				{
					throw new AppUsageException(
						string.Format("The {0} directory does not exist.",
						processedResourceDirectory)
					);
				}
			}
			else
			{
				processedResourceDirectory = currentDirectory;
			}

			WriteInfoLine();
			WriteInfoLine("Starting conversion of `.resx` files in the '{0}' directory:", processedResourceDirectory);
			WriteInfoLine();

			int processedFileCount = 0;
			int сonvertedFileCount = 0;
			int unconvertedFileCount = 0;

			foreach (string filePath in Directory.EnumerateFiles(processedResourceDirectory, "*.resx", SearchOption.AllDirectories))
			{
				string relativeFilePath = filePath.Substring(processedResourceDirectory.Length);

				try
				{
					FileConversionResult conversionResult = ResxToCsConverter.ConvertFile(filePath);
					string outputFilePath = conversionResult.OutputPath;
					string outputDirPath = Path.GetDirectoryName(outputFilePath);

					if (!Directory.Exists(outputDirPath))
					{
						Directory.CreateDirectory(outputDirPath);
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
				WriteWarnLine("There are no resx files found in the '{0}' directory.", processedResourceDirectory);
			}

			WriteInfoLine();

			return result;
		}

		/// <summary>
		/// Gets a application version number
		/// </summary>
		/// <returns>Application version number</returns>
		private static string GetVersion()
		{
			string version = typeof(Program)
#if NET40
				.Assembly
#else
				.GetTypeInfo().Assembly
#endif
				.GetName().Version.ToString();

			return version;
		}

		/// <summary>
		/// Writes a information about the error
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private static void WriteError(string message, params object[] messageArgs)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.Write(message, messageArgs);
			Console.ResetColor();
		}

		/// <summary>
		/// Writes a information about the error and a new line
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private static void WriteErrorLine(string message, params object[] messageArgs)
		{
			WriteError(message, messageArgs);
			Console.Error.WriteLine();
		}

		/// <summary>
		/// Writes a information about the warning
		/// </summary>
		/// <param name="message">Warning message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private static void WriteWarn(string message, params object[] messageArgs)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Error.Write(message, messageArgs);
			Console.ResetColor();
		}

		/// <summary>
		/// Writes a information about the warning and a new line
		/// </summary>
		/// <param name="message">Warning message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private static void WriteWarnLine(string message, params object[] messageArgs)
		{
			WriteWarn(message, messageArgs);
			Console.Error.WriteLine();
		}

		/// <summary>
		/// Writes a information
		/// </summary>
		/// <param name="message">Information message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private static void WriteInfo(string message, params object[] messageArgs)
		{
			Console.Out.Write(message, messageArgs);
		}

		/// <summary>
		/// Writes a information and a new line
		/// </summary>
		/// <param name="message">Information message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private static void WriteInfoLine(string message, params object[] messageArgs)
		{
			WriteInfo(message, messageArgs);
			Console.Out.WriteLine();
		}

		/// <summary>
		/// Writes a line terminator
		/// </summary>
		private static void WriteInfoLine()
		{
			Console.Out.WriteLine();
		}

		/// <summary>
		/// Writes a information about the success
		/// </summary>
		/// <param name="message">Success message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private static void WriteSuccess(string message, params object[] messageArgs)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Out.Write(message, messageArgs);
			Console.ResetColor();
		}

		/// <summary>
		/// Writes a information about the success and a new line
		/// </summary>
		/// <param name="message">Success message</param>
		/// <param name="messageArgs">Optional arguments for formatting the message string</param>
		private static void WriteSuccessLine(string message, params object[] messageArgs)
		{
			WriteSuccess(message, messageArgs);
			Console.Out.WriteLine();
		}

		/// <summary>
		/// Writes a help information
		/// </summary>
		private void WriteHelp()
		{
			TextWriter outputStream = Console.Out;

			outputStream.WriteLine();
			outputStream.WriteLine("  Usage: dotnet resx2cs [options] <DIRECTORY>");
			outputStream.WriteLine();
			outputStream.WriteLine("Arguments:");
			outputStream.WriteLine("    <DIRECTORY>\t\t\tThe directory containing `.resx` files. Defaults to the current directory.");
			outputStream.WriteLine("  Options:");
			outputStream.WriteLine("    -h, --help\t\t\tShow help information.");
			outputStream.WriteLine("    -v, --version\t\tShow the version number.");
		}

		/// <summary>
		/// Writes a version number
		/// </summary>
		private void WriteVersion()
		{
			Console.Out.WriteLine(GetVersion());
		}

		private static string TrimQuotes(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return value.Trim('"', '\'');
		}
	}
}