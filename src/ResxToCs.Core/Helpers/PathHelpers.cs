using System;
using System.IO;

using ResxToCs.Core.Utilities;

namespace ResxToCs.Core.Helpers
{
	/// <summary>
	/// Path helpers
	/// </summary>
	internal static class PathHelpers
	{
		/// <summary>
		/// Array of the directory separator characters
		/// </summary>
		private static readonly char[] _directorySeparatorChars = Utils.IsWindows() ?
			new char[] { '\\', '/' } : new char[] { '/', '\\' };


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

			char directorySeparatorChar = Path.DirectorySeparatorChar;
			char altDirectorySeparatorChar = Path.AltDirectorySeparatorChar;

			if (directorySeparatorChar == altDirectorySeparatorChar)
			{
				altDirectorySeparatorChar = directorySeparatorChar == '/' ? '\\' : '/';
			}

			string result = path.Replace(altDirectorySeparatorChar, directorySeparatorChar);

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

			string result = path.TrimStart(_directorySeparatorChars);

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

			string result = path.TrimEnd(_directorySeparatorChars);

			return result;
		}
		public static string GetParentDirectoryName(string dirPath)
		{
			if (dirPath == null)
			{
				throw new ArgumentNullException(nameof(dirPath));
			}

			if (string.IsNullOrWhiteSpace(dirPath))
			{
				return dirPath;
			}

			string parentDir = dirPath;
			int lastSlashIndex = dirPath.LastIndexOfAny(_directorySeparatorChars);

			if (lastSlashIndex != -1)
			{
				parentDir = dirPath.Substring(0, lastSlashIndex);
			}

			return parentDir;
		}

		/// <summary>
		/// Converts a relative path to an absolute path
		/// </summary>
		/// <param name="basePath">The base path</param>
		/// <param name="relativePath">The relative path to add to the base path</param>
		/// <returns>The absolute path</returns>
		public static string ToAbsolutePath(string basePath, string relativePath)
		{
			if (basePath == null)
			{
				throw new ArgumentNullException(nameof(basePath));
			}

			if (relativePath == null)
			{
				throw new ArgumentNullException(nameof(relativePath));
			}

			string absolutePath = PathHelpers.ProcessSlashes(relativePath.Trim());
			if (!Path.IsPathRooted(absolutePath))
			{
				absolutePath = Path.Combine(basePath, absolutePath);
			}
			absolutePath = Path.GetFullPath(absolutePath);

			return absolutePath;
		}
	}
}