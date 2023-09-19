using System;
using System.IO;
#if !NET40
using System.Reflection;
#endif

using ResxToCs.Core.FileSystem;
using ResxToCs.Core.Loggers;

namespace ResxToCs.DotNet
{
	/// <summary>
	/// Command-line application for converting the <c>.resx</c> files to the <c>.Designer.cs</c> files
	/// </summary>
	internal class App
	{
		/// <summary>
		/// Application configuration settings
		/// </summary>
		private readonly AppConfiguration _appConfig;

		/// <summary>
		/// Converter which produces transformation of Resx file into C# file
		/// </summary>
		private readonly ResxToCsFileConverter _fileConverter;


		/// <summary>
		/// Constructs an instance of the command-line application for converting the <c>.resx</c> files
		/// to the <c>.Designer.cs</c> files
		/// </summary>
		/// <param name="args">Command-line arguments</param>
		/// <param name="logger">Logger</param>
		public App(string[] args, ILogger logger)
		{
			_appConfig = GetAppConfigurationFromArguments(args);
			_fileConverter = new ResxToCsFileConverter(logger);
		}


		/// <summary>
		/// Runs a command-line application
		/// </summary>
		public int Run()
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
					bool result = _fileConverter.Convert(_appConfig.InputDirectory, _appConfig.OutputDirectory,
						_appConfig.Namespace, _appConfig.InternalAccessModifier);
					returnCode = result ? 0 : -1;
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
							case "-OUT-DIR":
							case "O":
								SetAppMode(appConfig, AppMode.Conversion);
								if (argIndex < lastArgIndex)
								{
									appConfig.OutputDirectory = TrimQuotes(args[++argIndex]);
								}
								else
								{
									throw new AppUsageException(
										"`-o` or `--out-dir` switch must be followed by path to the output directory.");
								}

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
			outputStream.WriteLine("  -o, --out-dir <DIRECTORY>        The output directory for all generated files.");
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