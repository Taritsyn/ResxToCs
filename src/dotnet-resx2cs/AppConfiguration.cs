namespace ResxToCs.DotNet
{
	/// <summary>
	/// Application configuration settings
	/// </summary>
	internal sealed class AppConfiguration
	{
		/// <summary>
		/// Gets or sets a application mode
		/// </summary>
		public AppMode Mode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a directory containing <code>.resx</code> files
		/// </summary>
		public string InputDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a namespace into which the output of the converter is placed
		/// </summary>
		public string Namespace
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of application configuration settings
		/// </summary>
		public AppConfiguration()
		{
			Mode = AppMode.Unknown;
			InputDirectory = string.Empty;
			Namespace = string.Empty;
		}
	}
}