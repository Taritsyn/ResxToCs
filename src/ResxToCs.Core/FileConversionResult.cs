namespace ResxToCs.Core
{
	/// <summary>
	/// File conversion result
	/// </summary>
	public sealed class FileConversionResult
	{
		/// <summary>
		/// Gets or sets a converted code
		/// </summary>
		public string ConvertedContent
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a path of input file
		/// </summary>
		public string InputPath
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a path of output file
		/// </summary>
		public string OutputPath
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the file conversion result
		/// </summary>
		public FileConversionResult()
		{
			ConvertedContent = string.Empty;
			InputPath = string.Empty;
			OutputPath = string.Empty;
		}
	}
}