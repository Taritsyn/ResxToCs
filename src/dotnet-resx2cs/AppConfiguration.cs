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
		/// Gets or sets a namespace into which the resource class is placed
		/// </summary>
		public string Namespace
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to set the access modifier of resource class to internal
		/// </summary>
		public bool InternalAccessModifier
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
			InternalAccessModifier = false;
		}
	}
}