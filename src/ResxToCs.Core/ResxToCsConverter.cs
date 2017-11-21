using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
		/// <param name="resourceNamespace">Namespace of resource</param>
		/// <param name="resourceName">Name of resource</param>
		/// <returns>C# code</returns>
		public static string ConvertCode(string code, string resourceNamespace, string resourceName)
		{
			if (code == null)
			{
				throw new ArgumentNullException(nameof(code));
			}

			if (resourceNamespace == null)
			{
				throw new ArgumentNullException(nameof(resourceNamespace));
			}

			if (resourceName == null)
			{
				throw new ArgumentNullException(nameof(resourceName));
			}

			if (string.IsNullOrWhiteSpace(code))
			{
				throw new ArgumentException(
					string.Format("The parameter '{0}' must be a non-empty string.", nameof(code)),
					nameof(code)
				);
			}

			if (string.IsNullOrWhiteSpace(resourceNamespace))
			{
				throw new ArgumentException(
					string.Format("The parameter '{0}' must be a non-empty string.", nameof(resourceNamespace)),
					nameof(resourceNamespace)
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format("The parameter '{0}' must be a non-empty string.", nameof(resourceName)),
					nameof(resourceName)
				);
			}

			string output = string.Empty;
			bool isCultureSpecified = ResourceNameContainsCulture(resourceName);

			if (!isCultureSpecified)
			{
				XDocument xmlDoc = XDocument.Parse(code);
				output = ConvertXmlDocument(xmlDoc, resourceNamespace, resourceName);
			}

			return output;
		}

		/// <summary>
		/// Converts a <code>.resx</code> file to <code>.Designer.cs</code> file
		/// </summary>
		/// <param name="filePath">Path to resource file</param>
		/// <returns>File conversion result</returns>
		public static FileConversionResult ConvertFile(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException(
					string.Format("The parameter '{0}' must be a non-empty string.", nameof(filePath)),
					nameof(filePath)
				);
			}

			string inputFilePath = PathHelpers.GetCanonicalPath(filePath);
			string inputFileExtension = Path.GetExtension(inputFilePath);

			if (!string.Equals(inputFileExtension, ".resx", StringComparison.OrdinalIgnoreCase))
			{
				throw new ResxConversionException(
					string.Format("The {0} file is not a resource.", inputFilePath));
			}

			string resourceDirPath = Path.GetDirectoryName(inputFilePath);
			string projectDirPath;

			try
			{
				projectDirPath = GetProjectDirectoryName(resourceDirPath);
			}
			catch (FileNotFoundException e)
			{
				throw new ResxConversionException(e.Message, e);
			}

			string assemblyName = GetAssemblyName(projectDirPath);
			string resourceNamespace = GenerateResourceNamespace(assemblyName, projectDirPath, resourceDirPath);
			string resourceName = Path.GetFileNameWithoutExtension(inputFilePath);

			string output = string.Empty;
			string outputFilePath = Path.Combine(resourceDirPath, resourceName + ".Designer.cs");
			bool isCultureSpecified = ResourceNameContainsCulture(resourceName);

			if (!isCultureSpecified)
			{
				XDocument xmlDoc;

				try
				{
					xmlDoc = XDocument.Load(inputFilePath);
				}
				catch (IOException e)
				{
					throw new ResxConversionException("The '{0}' file not found or unreadable.", e);
				}
				catch (UnauthorizedAccessException e)
				{
					throw new ResxConversionException("The '{0}' file not found or unreadable.", e);
				}
				catch (SecurityException e)
				{
					throw new ResxConversionException("The '{0}' file not found or unreadable.", e);
				}
				catch (InvalidOperationException e)
				{
					throw new ResxConversionException(e.Message, e);
				}

				output = ConvertXmlDocument(xmlDoc, resourceNamespace, resourceName);
			}

			var result = new FileConversionResult
			{
				ConvertedContent = output,
				InputPath = inputFilePath,
				OutputPath = outputFilePath
			};

			return result;
		}

		public static string ConvertXmlDocument(XDocument xmlDoc, string resourceNamespace,
			string resourceName)
		{
			var resourceStrings = new List<ResourceData>();

			foreach (XElement xmlElem in xmlDoc.Descendants("data"))
			{
				string name = xmlElem.Attribute("name").Value;
				string value = xmlElem.Element("value").Value;

				resourceStrings.Add(
					new ResourceData
					{
						Name = name,
						Value = value
					}
				);
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
namespace {0}
{{
	using System;
	using System.Globalization;
	using System.Reflection;
	using System.Resources;

	/// <summary>
	/// A strongly-typed resource class, for looking up localized strings, etc.
	/// </summary>
	public class {1}
	{{
		private static Lazy<ResourceManager> _resourceManager =
			new Lazy<ResourceManager>(() => new ResourceManager(
				""{0}.{1}"",
#if NET40
				typeof({1}).Assembly
#else
				typeof({1}).GetTypeInfo().Assembly
#endif
			));

		private static CultureInfo _resourceCulture;

		/// <summary>
		/// Returns a cached ResourceManager instance used by this class
		/// </summary>
		public static ResourceManager ResourceManager
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
		public static CultureInfo Culture
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
", resourceNamespace, resourceName);

			foreach (ResourceData resourceString in resourceStrings)
			{
				outputBuilder.AppendLine();
				RenderProperty(outputBuilder, resourceString);
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

		private static string GetProjectDirectoryName(string resourceDirPath)
		{
			string rootPath = PathHelpers.RemoveLastSlash(Path.GetPathRoot(resourceDirPath));
			string projectDirPath = PathHelpers.RemoveLastSlash(resourceDirPath);

			while (true)
			{
				if (string.Equals(projectDirPath, rootPath, StringComparison.OrdinalIgnoreCase))
				{
					throw new FileNotFoundException("Project file not exist.");
				}

				if (Directory.EnumerateFiles(projectDirPath, "*.csproj", SearchOption.TopDirectoryOnly).Any())
				{
					break;
				}

				int lastSlashIndex = projectDirPath.LastIndexOf(Path.DirectorySeparatorChar);
				if (lastSlashIndex != -1)
				{
					projectDirPath = projectDirPath.Substring(0, lastSlashIndex);
				}
			}

			return projectDirPath;
		}

		private static string GetAssemblyName(string projectDirPath)
		{
			string assemblyName = Path.GetFileName(PathHelpers.RemoveLastSlash(projectDirPath));

			return assemblyName;
		}

		private static string GenerateResourceNamespace(string assemblyName, string projectDirPath,
			string resourceDirPath)
		{
			string resourceNamespace = assemblyName;

			if (!string.Equals(projectDirPath, resourceDirPath, StringComparison.OrdinalIgnoreCase))
			{
				string resourceNamespacePart = resourceDirPath.Substring(projectDirPath.Length);
				resourceNamespacePart = resourceNamespacePart.Trim(Path.DirectorySeparatorChar);
				resourceNamespacePart = resourceNamespacePart.Replace(Path.DirectorySeparatorChar, '.');

				resourceNamespace = string.Join(".", assemblyName, resourceNamespacePart);
			}

			return resourceNamespace;
		}

		private static bool ResourceNameContainsCulture(string resourceName)
		{
			return _cultureRegex.IsMatch(resourceName);
		}

		private static void RenderProperty(StringBuilder builder, ResourceData resourceString)
		{
			builder
				.AppendLine("		/// <summary>")
				.AppendFormat("		/// Looks up a localized string similar to \"{0}\"",
					Utils.XmlEncode(Utils.CutShortByWords(resourceString.Value, 100)))
				.AppendLine()
				.AppendLine("		/// </summary>")
				.AppendFormat("		public static string {0}", resourceString.Name)
				.AppendLine()
				.AppendLine("		{")
				.AppendFormat(@"			get {{ return GetString(""{0}""); }}", resourceString.Name)
				.AppendLine()
				.AppendLine("		}")
				;
		}
	}
}