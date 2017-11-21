using System;
using System.IO;

namespace ResxToCs.Core.Helpers
{
	/// <summary>
	/// Path helpers
	/// </summary>
	public static class PathHelpers
	{
		public static string ProcessSlashes(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				return path;
			}

			string result = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			return result;
		}

		public static string GetCanonicalPath(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				return path;
			}

			string result = Path.GetFullPath(ProcessSlashes(path.Trim()));

			return result;
		}

		public static string RemoveFirstSlash(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				return path;
			}

			string result = path.TrimStart(Path.DirectorySeparatorChar);

			return result;
		}

		public static string RemoveLastSlash(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				return path;
			}

			string result = path.TrimEnd(Path.DirectorySeparatorChar);

			return result;
		}
	}
}