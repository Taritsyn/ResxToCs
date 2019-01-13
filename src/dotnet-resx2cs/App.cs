using System;
using System.IO;
#if !NET40
using System.Reflection;
#endif

using ResxToCs.Core;
using ResxToCs.Core.Helpers;

namespace ResxToCs.DotNet
{
	/// <summary>
	/// Command-line application for converting the <code>.resx</code> files to the <code>.Designer.cs</code> files
	/// </summary>
	class App
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
		private App(string[] args)
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
				var app = new App(args);
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
					returnCode = Convert(_appConfig.InputDirectory, _appConfig.Namespace,
						_appConfig.InternalAccessModifier) ? 0 : -1;
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
				int argCount = args.Length;
				int lastArgIndex = argCount - 1;

				for (int argIndex = 0; argIndex < argCount; argIndex++)
				{
					string arg = args[argIndex];

					if (arg.StartsWith("-") && arg.Length > 1)
					{
						string switchName = arg.Substring(1).ToUpperInvariant();

						switch (switchName)
						{
							case "-HELP":
							case "H":
							case "?":
								SetAppMode(appConfig, AppMode.Help);
								break;
							case "-VERSION":
							case "V":
								SetAppMode(appConfig, AppMode.Version);
								break;
							case "-NAMESPACE":
							case "N":
								SetAppMode(appConfig, AppMode.Conversion);
								if (argIndex < lastArgIndex)
								{
									appConfig.Namespace = TrimQuotes(args[++argIndex]);
								}
								else
								{
									throw new AppUsageException(
										"`-n` or `--namespace` switch must be followed by resource namespace.");
								}

								break;
							case "-INTERNAL-ACCESS-MODIFIER":
							case "I":
								SetAppMode(appConfig, AppMode.Conversion);
								appConfig.InternalAccessModifier = true;
								break;
							default:
								SetAppMode(appConfig, AppMode.Conversion);
								appConfig.InputDirectory = TrimQuotes(arg);
								break;
						}
					}
					else if (arg == "/?")
					{
						SetAppMode(appConfig, AppMode.Help);
					}
					else
					{
						SetAppMode(appConfig, AppMode.Conversion);
						appConfig.InputDirectory = TrimQuotes(arg);
					}
				}
			}
			else
			{
				SetAppMode(appConfig, AppMode.Conversion);
			}

			return appConfig;
		}

		/// <summary>
		/// Sets a application mode to configuration settings
		/// </summary>
		/// <param name="appConfig">Application configuration settings</param>
		/// <param name="mode">Application mode</param>
		private static void SetAppMode(AppConfiguration appConfig, AppMode mode)
		{
			if (appConfig.Mode == AppMode.Unknown)
			{
				appConfig.Mode = mode;
			}
		}

		/// <summary>
		/// Converts a <code>.resx</code> files in specified directory
		/// </summary>
		/// <param name="resourceDirectory">The directory containing <code>.resx</code> files</param>
		/// <param name="resourceNamespace">Namespace of resource</param>
		/// <param name="internalAccessModifier">Flag for whether to set the access modifier of
		/// resource class to internal</param>
		/// <returns>Result of conversion (true - success; false - failure)</returns>
		private static bool Convert(string resourceDirectory, string resourceNamespace,
			bool internalAccessModifier)
		{
			bool result = true;
			string processedResourceDirectory;
			string currentDirectory = Directory.GetCurrentDirectory();

			if (!string.IsNullOrWhiteSpace(resourceDirectory))
			{
				processedResourceDirectory = PathHelpers.ProcessSlashes(resourceDirectory.Trim());
				if (!Path.IsPathRooted(processedResourceDirectory))
				{
					processedResourceDirectory = Path.Combine(currentDirectory, processedResourceDirectory);
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
			int failedFileCount = 0;

			foreach (string filePath in Directory.EnumerateFiles(processedResourceDirectory, "*.resx", SearchOption.AllDirectories))
			{
				string relativeFilePath = filePath.Substring(processedResourceDirectory.Length);

				try
				{
					FileConversionResult conversionResult = ResxToCsConverter.ConvertFile(filePath,
						resourceNamespace, internalAccessModifier);
					string outputFilePath = conversionResult.OutputPath;
					string convertedContent = conversionResult.ConvertedContent;
					bool changesDetected = FileHelpers.HasFileContentChanged(outputFilePath, convertedContent);

					if (changesDetected)
					{
						string outputDirPath = Path.GetDirectoryName(outputFilePath);
						if (!Directory.Exists(outputDirPath))
						{
							Directory.CreateDirectory(outputDirPath);
						}

						File.WriteAllText(outputFilePath, convertedContent);

						WriteInfoLine("	* '{0}' file has been successfully converted", relativeFilePath);
						сonvertedFileCount++;
					}
					else
					{
						WriteInfoLine("	* '{0}' file has not changed", relativeFilePath);
					}
				}
				catch (ResxConversionException e)
				{
					WriteInfoLine("	* '{0}' file failed to convert");
					WriteErrorLine(e.Message);
					failedFileCount++;
				}

				processedFileCount++;
			}

			if (processedFileCount > 0)
			{
				WriteInfoLine();
				WriteInfoLine("Total files: {0}. Converted: {1}. Failed: {2}.",
					processedFileCount, сonvertedFileCount, failedFileCount);

				result = failedFileCount == 0;
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
			string version = typeof(App)
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
			outputStream.WriteLine("Usage: dotnet resx2cs [options] <DIRECTORY>");
			outputStream.WriteLine();
			outputStream.WriteLine("Arguments:");
			outputStream.WriteLine("  <DIRECTORY>   The directory containing `.resx` files.");
			outputStream.WriteLine("                Defaults to the current directory.");
			outputStream.WriteLine();
			outputStream.WriteLine("Options:");
			outputStream.WriteLine("  -h, --help                       Show help information.");
			outputStream.WriteLine("  -v, --version                    Show the version number.");
			outputStream.WriteLine("  -n, --namespace <NAMESPACE>      Namespace into which the resource class is placed.");
			outputStream.WriteLine("  -i, --internal-access-modifier   Set the access modifier of resource class to internal.");
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