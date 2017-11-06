namespace ResxToCs.DotNet
{
	/// <summary>
	/// Application mode
	/// </summary>
	internal enum AppMode
	{
		/// <summary>
		/// Unknown
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Displays help information
		/// </summary>
		Help,

		/// <summary>
		/// Displays version number
		/// </summary>
		Version,

		/// <summary>
		/// Convert <code>.resx</code> file
		/// </summary>
		Conversion
	}
}