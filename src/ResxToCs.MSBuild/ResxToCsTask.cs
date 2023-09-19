using Microsoft.Build.Utilities;

using ResxToCs.Core.FileSystem;

namespace ResxToCs.MSBuild
{
	/// <summary>
	/// An MSBuild task for converting the <с>.resx</с> files to the <с>.Designer.cs</с> files
	/// </summary>
	public sealed class ResxToCsTask : Task
	{
		/// <summary>
		/// Converter which produces transformation of Resx file into C# file
		/// </summary>
		private readonly ResxToCsFileConverter _fileConverter;

		/// <summary>
		/// The directory containing input <с>.resx</с> files
		/// </summary>
		public string InputDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// The directory containing output <с>.Designer.cs</с> files
		/// </summary>
		public string OutputDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// The namespace into which the resource class is placed
		/// </summary>
		public string Namespace
		{
			get;
			set;
		}

		/// <summary>
		/// Flag for whether to set the access modifier of resource class to internal
		/// </summary>
		public bool InternalAccessModifier
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the <see cref="ResxToCsTask"/> class
		/// </summary>
		public ResxToCsTask()
		{
			InputDirectory = string.Empty;
			OutputDirectory = string.Empty;
			Namespace = string.Empty;
			InternalAccessModifier = false;

			_fileConverter = new ResxToCsFileConverter(new TaskLogger(Log));
		}


		/// <summary>
		/// Execute the Task
		/// </summary>
		public override bool Execute()
		{
			bool result = _fileConverter.Convert(InputDirectory, OutputDirectory, Namespace, InternalAccessModifier);

			return result;
		}
	}
}