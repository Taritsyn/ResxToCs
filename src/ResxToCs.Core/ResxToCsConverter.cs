using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using ResxToCs.Core.Constants;
using ResxToCs.Core.Helpers;
using ResxToCs.Core.Utilities;

namespace ResxToCs.Core
{
	/// <summary>
	/// Converter which produces transformation of Resx code into C# code
	/// </summary>
	public sealed class ResxToCsConverter
	{
		private static Regex _cultureRegex = new Regex("[a-zA-Z]{2}-[a-zA-Z]{2}$");


		/// <summary>
		/// Converts a Resx code to C# code
		/// </summary>
		/// <param name="code">Resx code</param>
		/// <param name="resourceName">Name of resource</param>
		/// <param name="resourceNamespace">Namespace of resource</param>
		/// <param name="internalAccessModifier">Flag for whether to set the access modifier of
		/// resource class to internal</param>
		/// <returns>C# code</returns>
		public static string ConvertCode(string code, string resourceName, string resourceNamespace,
			bool internalAccessModifier = false)
		{
			if (code == null)
			{
				throw new ArgumentNullException(nameof(code));
			}

			if (resourceName == null)
			{
				throw new ArgumentNullException(nameof(resourceName));
			}

			if (resourceNamespace == null)
			{
				throw new ArgumentNullException(nameof(resourceNamespace));
			}

			if (string.IsNullOrWhiteSpace(code))
			{
				throw new ArgumentException(
					string.Format(ErrorMessages.ArgumentIsEmpty, nameof(code)),
					nameof(code)
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format(ErrorMessages.ArgumentIsEmpty, nameof(resourceName)),
					nameof(resourceName)
				);
			}

			if (string.IsNullOrWhiteSpace(resourceNamespace))
			{
				throw new ArgumentException(
					string.Format(ErrorMessages.ArgumentIsEmpty, nameof(resourceNamespace)),
					nameof(resourceNamespace)
				);
			}

			string output = string.Empty;
			bool isCultureSpecified = ResourceNameContainsCulture(resourceName);

			if (!isCultureSpecified)
			{
				using (var xmlReader = XmlReader.Create(new StringReader(code)))
				{
					output = ConvertXmlReader(xmlReader, resourceName, resourceNamespace,
						internalAccessModifier);
				}
			}

			return output;
		}

		/// <summary>
		/// Converts a <c>.resx</c> file to <c>.Designer.cs</c> file
		/// </summary>
		/// <param name="inputFile">Path to input resource file</param>
		/// <param name="outputFile">Path to output C#-file</param>
		/// <param name="resourceNamespace">Namespace of resource</param>
		/// <param name="internalAccessModifier">Flag for whether to set the access modifier of
		/// resource class to internal</param>
		/// <returns>File conversion result</returns>
		public static FileConversionResult ConvertFile(string inputFile, string outputFile = null,
			string resourceNamespace = null, bool internalAccessModifier = false)
		{
			if (inputFile == null)
			{
				throw new ArgumentNullException(nameof(inputFile));
			}

			if (string.IsNullOrWhiteSpace(inputFile))
			{
				throw new ArgumentException(
					string.Format(ErrorMessages.ArgumentIsEmpty, nameof(inputFile)),
					nameof(inputFile)
				);
			}

			string inputFileExtension = Path.GetExtension(inputFile);

			if (!string.Equals(inputFileExtension, FileExtension.Resx, StringComparison.OrdinalIgnoreCase))
			{
				throw new ResxConversionException(
					string.Format("The {0} file is not a resource.", inputFile));
			}

			string processedInputFile = PathHelpers.GetCanonicalPath(inputFile);
			string processedOutputFile;

			if (!string.IsNullOrWhiteSpace(outputFile))
			{
				string outputFileExtension = Path.GetExtension(outputFile);

				if (!string.Equals(outputFileExtension, ".cs", StringComparison.OrdinalIgnoreCase))
				{
					throw new ResxConversionException(
						string.Format("The {0} file is not a C#-file.", outputFile));
				}

				processedOutputFile = PathHelpers.GetCanonicalPath(outputFile);
			}
			else
			{
				processedOutputFile = ConvertFilePath(processedInputFile);
			}

			string outputDir = Path.GetDirectoryName(processedOutputFile);

			if (string.IsNullOrWhiteSpace(resourceNamespace))
			{
				string projectDir = GetProjectDirectoryName(outputDir);
				if (projectDir == null)
				{
					throw new ResxConversionException("Project file not exist.");
				}
				resourceNamespace = GenerateResourceNamespace(projectDir, outputDir);
			}

			string output = string.Empty;
			string resourceName = GenerateResourceName(processedOutputFile);
			bool isCultureSpecified = ResourceNameContainsCulture(resourceName);

			if (!isCultureSpecified)
			{
				try
				{
					using (var xmlReader = XmlReader.Create(inputFile))
					{
						output = ConvertXmlReader(xmlReader, resourceName, resourceNamespace,
							internalAccessModifier);
					}
				}
				catch (IOException e)
				{
					throw new ResxConversionException(ErrorMessages.FileNotFoundOrUnreadable, e);
				}
				catch (UnauthorizedAccessException e)
				{
					throw new ResxConversionException(ErrorMessages.FileNotFoundOrUnreadable, e);
				}
				catch (SecurityException e)
				{
					throw new ResxConversionException(ErrorMessages.FileNotFoundOrUnreadable, e);
				}
				catch
				{
					throw;
				}
			}

			var result = new FileConversionResult
			{
				ConvertedContent = output,
				InputPath = processedInputFile,
				OutputPath = processedOutputFile
			};

			return result;
		}

		/// <summary>
		/// Converts a <c>.resx</c> file path to <c>.Designer.cs</c> file path
		/// </summary>
		/// <param name="inputFile">Path to input resource file</param>
		/// <param name="outputDir">Directory containing output <с>.Designer.cs</с> files</param>
		/// <returns>Path to output C#-file</returns>
		internal static string ConvertFilePath(string inputFile, string outputDir = null)
		{
			if (inputFile == null)
			{
				throw new ArgumentNullException(nameof(inputFile));
			}

			if (string.IsNullOrWhiteSpace(inputFile))
			{
				throw new ArgumentException(
					string.Format(ErrorMessages.ArgumentIsEmpty, nameof(inputFile)),
					nameof(inputFile)
				);
			}

			string outputFile;

			if (!string.IsNullOrWhiteSpace(outputDir))
			{
				string inputFileName = Path.GetFileName(inputFile);
				outputFile = Path.Combine(outputDir, Path.ChangeExtension(inputFileName, FileExtension.DesignerCs));
			}
			else
			{
				outputFile = Path.ChangeExtension(inputFile, FileExtension.DesignerCs);
			}

			return outputFile;
		}

		private static string ConvertXmlReader(XmlReader xmlReader, string resourceName,
			string resourceNamespace, bool internalAccessModifier)
		{
			string accessModifier = internalAccessModifier ? "internal" : "public";
			var resourceDataList = new List<ResourceData>();

			try
			{
				while (xmlReader.Read())
				{
					if (xmlReader.Depth == 1
						&& xmlReader.NodeType == XmlNodeType.Element
						&& xmlReader.Name == "data")
					{
						var resourceData = new ResourceData();
						int attrCount = xmlReader.AttributeCount;

						for (int attrIndex = 0; attrIndex < attrCount; attrIndex++)
						{
							xmlReader.MoveToAttribute(attrIndex);
							if (xmlReader.Name == "name")
							{
								resourceData.Name = xmlReader.Value;
								break;
							}
						}

						if (xmlReader.ReadToFollowing("value"))
						{
							resourceData.Value = xmlReader.ReadElementContentAsString();
						}

						resourceDataList.Add(resourceData);
					}
				}
			}
			catch (XmlException e)
			{
				throw new ResxConversionException(
					string.Format("During parsing the Resx code an error occurred: {0}", e.Message), e);
			}
			catch
			{
				throw;
			}

			var outputBuilder = StringBuilderPool.GetBuilder();
			outputBuilder.AppendFormat(
@"//------------------------------------------------------------------------------
// <auto-generated>
//	 This code was generated by a tool.
//
//	 Changes to this file may cause incorrect behavior and will be lost if
//	 the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace {1}
{{
	using System;
	using System.Globalization;
	using System.Reflection;
	using System.Resources;

	/// <summary>
	/// A strongly-typed resource class, for looking up localized strings, etc.
	/// </summary>
	{2} class {0}
	{{
		private static Lazy<ResourceManager> _resourceManager =
			new Lazy<ResourceManager>(() => new ResourceManager(
				""{1}.{0}"",
#if NET20 || NET30 || NET35 || NET40
				typeof({0}).Assembly
#else
				typeof({0}).GetTypeInfo().Assembly
#endif
			));

		private static CultureInfo _resourceCulture;

		/// <summary>
		/// Returns a cached ResourceManager instance used by this class
		/// </summary>
		{2} static ResourceManager ResourceManager
		{{
			get
			{{
				return _resourceManager.Value;
			}}
		}}

		/// <summary>
		/// Overrides a current thread's CurrentUICulture property for all
		/// resource lookups using this strongly typed resource class
		/// </summary>
		{2} static CultureInfo Culture
		{{
			get
			{{
				return _resourceCulture;
			}}
			set
			{{
				_resourceCulture = value;
			}}
		}}
", resourceName, resourceNamespace, accessModifier);

			foreach (ResourceData resourceData in resourceDataList)
			{
				outputBuilder.AppendLine();
				RenderProperty(outputBuilder, resourceData, accessModifier);
			}

			outputBuilder.Append(@"
		private static string GetString(string name)
		{
			string value = ResourceManager.GetString(name, _resourceCulture);

			return value;
		}
	}
}");

			string output = outputBuilder.ToString();
			StringBuilderPool.ReleaseBuilder(outputBuilder);

			return output;
		}

		private static string GetProjectDirectoryName(string resourceDir)
		{
			string rootDir = PathHelpers.RemoveLastSlash(Path.GetPathRoot(resourceDir));
			string projectDir = PathHelpers.RemoveLastSlash(resourceDir);

			while (!Directory.Exists(projectDir))
			{
				if (string.Equals(projectDir, rootDir, StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}

				projectDir = PathHelpers.RemoveLastSlash(PathHelpers.GetParentDirectoryName(projectDir));
			}

			while (true)
			{
				if (string.Equals(projectDir, rootDir, StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}

				if (Directory.EnumerateFiles(projectDir, "*.csproj", SearchOption.TopDirectoryOnly).Any())
				{
					break;
				}

				projectDir = PathHelpers.RemoveLastSlash(PathHelpers.GetParentDirectoryName(projectDir));
			}

			return projectDir;
		}

		private static string GetAssemblyName(string projectDir)
		{
			string assemblyName = Path.GetFileName(PathHelpers.RemoveLastSlash(projectDir));

			return assemblyName;
		}

		private static string GenerateResourceNamespace(string projectDir, string resourceDir)
		{
			string assemblyName = GetAssemblyName(projectDir);
			string resourceNamespace = assemblyName;

			if (!string.Equals(projectDir, resourceDir, StringComparison.OrdinalIgnoreCase))
			{
				string resourceNamespacePart = resourceDir.Substring(projectDir.Length);
				resourceNamespacePart = resourceNamespacePart.Trim(Path.DirectorySeparatorChar);
				resourceNamespacePart = resourceNamespacePart.Replace(Path.DirectorySeparatorChar, '.');

				resourceNamespace = string.Join(".", assemblyName, resourceNamespacePart);
			}

			return resourceNamespace;
		}

		private static string GenerateResourceName(string outputFile)
		{
			string resourceName;

			if (outputFile.EndsWith(FileExtension.DesignerCs, StringComparison.OrdinalIgnoreCase))
			{
				string outputFileName = Path.GetFileName(outputFile);
				resourceName = outputFileName.Substring(0, outputFileName.Length - FileExtension.DesignerCs.Length);
			}
			else
			{
				resourceName = Path.GetFileNameWithoutExtension(outputFile);
			}

			return resourceName;
		}

		private static bool ResourceNameContainsCulture(string resourceName)
		{
			return _cultureRegex.IsMatch(resourceName);
		}

		private static void RenderProperty(StringBuilder builder, ResourceData resourceData,
			string accessModifier)
		{
			builder
				.AppendLine("		/// <summary>")
				.AppendFormat("		/// Looks up a localized string similar to \"{0}\"",
					Utils.XmlEncode(Utils.CutShortByWords(resourceData.Value, 100)))
				.AppendLine()
				.AppendLine("		/// </summary>")
				.AppendFormat("		{1} static string {0}", resourceData.Name, accessModifier)
				.AppendLine()
				.AppendLine("		{")
				.AppendFormat(@"			get {{ return GetString(""{0}""); }}", resourceData.Name)
				.AppendLine()
				.AppendLine("		}")
				;
		}
	}
}