using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

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
		/// <returns>C# code</returns>
		public static string ConvertCode(string code, string resourceName, string resourceNamespace)
		{
			return ConvertCode(code, resourceName, resourceNamespace, false);
		}

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
			bool internalAccessModifier)
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
					string.Format("The parameter '{0}' must be a non-empty string.", nameof(code)),
					nameof(code)
				);
			}

			if (string.IsNullOrWhiteSpace(resourceName))
			{
				throw new ArgumentException(
					string.Format("The parameter '{0}' must be a non-empty string.", nameof(resourceName)),
					nameof(resourceName)
				);
			}

			if (string.IsNullOrWhiteSpace(resourceNamespace))
			{
				throw new ArgumentException(
					string.Format("The parameter '{0}' must be a non-empty string.", nameof(resourceNamespace)),
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
		/// Converts a <code>.resx</code> file to <code>.Designer.cs</code> file
		/// </summary>
		/// <param name="filePath">Path to resource file</param>
		/// <returns>File conversion result</returns>
		public static FileConversionResult ConvertFile(string filePath)
		{
			return ConvertFile(filePath, string.Empty);
		}

		/// <summary>
		/// Converts a <code>.resx</code> file to <code>.Designer.cs</code> file
		/// </summary>
		/// <param name="filePath">Path to resource file</param>
		/// <param name="resourceNamespace">Namespace of resource</param>
		/// <returns>File conversion result</returns>
		public static FileConversionResult ConvertFile(string filePath, string resourceNamespace)
		{
			return ConvertFile(filePath, resourceNamespace, false);
		}

		/// <summary>
		/// Converts a <code>.resx</code> file to <code>.Designer.cs</code> file
		/// </summary>
		/// <param name="filePath">Path to resource file</param>
		/// <param name="internalAccessModifier">Flag for whether to set the access modifier of
		/// resource class to internal</param>
		/// <returns>File conversion result</returns>
		public static FileConversionResult ConvertFile(string filePath, bool internalAccessModifier)
		{
			return ConvertFile(filePath, string.Empty, internalAccessModifier);
		}

		/// <summary>
		/// Converts a <code>.resx</code> file to <code>.Designer.cs</code> file
		/// </summary>
		/// <param name="filePath">Path to resource file</param>
		/// <param name="resourceNamespace">Namespace of resource</param>
		/// <param name="internalAccessModifier">Flag for whether to set the access modifier of
		/// resource class to internal</param>
		/// <returns>File conversion result</returns>
		public static FileConversionResult ConvertFile(string filePath, string resourceNamespace,
			bool internalAccessModifier)
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

			string resourceName = Path.GetFileNameWithoutExtension(inputFilePath);
			string resourceDirPath = Path.GetDirectoryName(inputFilePath);
			if (string.IsNullOrWhiteSpace(resourceNamespace))
			{
				string projectDirPath = GetProjectDirectoryName(resourceDirPath);
				if (projectDirPath == null)
				{
					throw new ResxConversionException("Project file not exist.");
				}
				resourceNamespace = GenerateResourceNamespace(projectDirPath, resourceDirPath);
			}

			string output = string.Empty;
			string outputFilePath = Path.Combine(resourceDirPath, resourceName + ".Designer.cs");
			bool isCultureSpecified = ResourceNameContainsCulture(resourceName);

			if (!isCultureSpecified)
			{
				try
				{
					using (var xmlReader = XmlReader.Create(inputFilePath))
					{
						output = ConvertXmlReader(xmlReader, resourceName, resourceNamespace,
							internalAccessModifier);
					}
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
				catch
				{
					throw;
				}
			}

			var result = new FileConversionResult
			{
				ConvertedContent = output,
				InputPath = inputFilePath,
				OutputPath = outputFilePath
			};

			return result;
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
#if NET40
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

		private static string GetProjectDirectoryName(string resourceDirPath)
		{
			string rootPath = PathHelpers.RemoveLastSlash(Path.GetPathRoot(resourceDirPath));
			string projectDirPath = PathHelpers.RemoveLastSlash(resourceDirPath);

			while (true)
			{
				if (string.Equals(projectDirPath, rootPath, StringComparison.OrdinalIgnoreCase))
				{
					return null;
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

		private static string GenerateResourceNamespace(string projectDirPath, string resourceDirPath)
		{
			string assemblyName = GetAssemblyName(projectDirPath);
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