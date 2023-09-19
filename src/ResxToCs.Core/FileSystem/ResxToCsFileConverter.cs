using System;
using System.IO;

using ResxToCs.Core.Constants;
using ResxToCs.Core.Helpers;
using ResxToCs.Core.Loggers;

namespace ResxToCs.Core.FileSystem
{
	/// <summary>
	/// Converter which produces transformation of Resx file into C# file in specified directory
	/// </summary>
	public class ResxToCsFileConverter
	{
		/// <summary>
		/// Logger
		/// </summary>
		private readonly ILogger _logger;


		/// <summary>
		/// Constructs instance of the converter which produces transformation of Resx file into C# file in specified directory
		/// </summary>
		/// <param name="logger">Logger</param>
		public ResxToCsFileConverter(ILogger logger)
		{
			_logger = logger;
		}


		/// <summary>
		/// Converts a <c>.resx</c> files in specified directory
		/// </summary>
		/// <param name="inputDirectory">The directory containing input <c>.resx</c> files</param>
		/// <param name="outputDirectory">The directory containing output <c>.Designer.cs</c> files</param>
		/// <param name="resourceNamespace">Namespace of resource</param>
		/// <param name="internalAccessModifier">Flag for whether to set the access modifier of
		/// resource class to internal</param>
		/// <returns>Result of conversion (<c>true</c> - success; <c>false</c> - failure)</returns>
		public bool Convert(string inputDirectory, string outputDirectory, string resourceNamespace,
			bool internalAccessModifier)
		{
			bool result = true;
			string processedInputDirectory;
			string processedOutputDirectory;
			string currentDirectory = Directory.GetCurrentDirectory();

			if (!string.IsNullOrWhiteSpace(inputDirectory))
			{
				processedInputDirectory = PathHelpers.ToAbsolutePath(currentDirectory, inputDirectory);

				if (!Directory.Exists(processedInputDirectory))
				{
					_logger.Error(ErrorMessages.DirectoryNotExist, processedInputDirectory);
					return false;
				}
			}
			else
			{
				processedInputDirectory = currentDirectory;
			}

			if (!string.IsNullOrWhiteSpace(outputDirectory))
			{
				processedOutputDirectory = PathHelpers.ToAbsolutePath(currentDirectory, outputDirectory);
			}
			else
			{
				processedOutputDirectory = string.Empty;
			}

			_logger.Info("{1}Starting conversion of `.resx` files in the '{0}' directory:{1}",
				processedInputDirectory, Environment.NewLine);

			int processedFileCount = 0;
			int сonvertedFileCount = 0;
			int failedFileCount = 0;

			foreach (string inputFilePath in Directory.EnumerateFiles(processedInputDirectory, "*.resx", SearchOption.AllDirectories))
			{
				string relativeInputFilePath = inputFilePath.Substring(processedInputDirectory.Length);
				string outputFilePath = !string.IsNullOrWhiteSpace(processedOutputDirectory) ?
					ResxToCsConverter.ConvertFilePath(inputFilePath, processedOutputDirectory)
					:
					string.Empty
					;

				try
				{
					FileConversionResult conversionResult = ResxToCsConverter.ConvertFile(inputFilePath, outputFilePath,
						resourceNamespace, internalAccessModifier);
					outputFilePath = conversionResult.OutputPath;
					string convertedContent = conversionResult.ConvertedContent;
					bool changesDetected = FileHelpers.HasFileContentChanged(outputFilePath, convertedContent);

					if (changesDetected)
					{
						processedOutputDirectory = Path.GetDirectoryName(outputFilePath);
						if (!Directory.Exists(processedOutputDirectory))
						{
							Directory.CreateDirectory(processedOutputDirectory);
						}

						File.WriteAllText(outputFilePath, convertedContent);

						_logger.Info("	* '{0}' file has been successfully converted", relativeInputFilePath);
						сonvertedFileCount++;
					}
					else
					{
						_logger.Info("	* '{0}' file has not changed", relativeInputFilePath);
					}
				}
				catch (ResxConversionException e)
				{
					_logger.Info("	* '{0}' file failed to convert", relativeInputFilePath);
					_logger.Error(e.Message);
					failedFileCount++;
				}

				processedFileCount++;
			}

			if (processedFileCount > 0)
			{
				_logger.Info("{3}Total files: {0}. Converted: {1}. Failed: {2}.", processedFileCount,
					сonvertedFileCount, failedFileCount, Environment.NewLine);

				result = failedFileCount == 0;
				if (result)
				{
					_logger.Success("Conversion is successfull.");
				}
				else
				{
					_logger.Error("Conversion is failed.");
				}
			}
			else
			{
				_logger.Warn("There are no resx files found in the '{0}' directory.", processedInputDirectory);
			}

			_logger.Info(string.Empty);

			return result;
		}
	}
}