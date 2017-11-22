using System;
using System.IO;
#if !NETSTANDARD1_3
using System.Linq;
#endif
#if NETSTANDARD1_3 || NETSTANDARD2_0
using System.Runtime.InteropServices;
#endif
using System.Text;

namespace ResxToCs.Core.Utilities
{
	public static class Utils
	{
		/// <summary>
		/// Flag indicating whether the current operating system is Windows
		/// </summary>
		private static readonly bool _isWindows;

		/// <summary>
		/// Array of other whitespace characters
		/// </summary>
		private static readonly char[] _otherWhitespaceChars = { '\t', '\r', '\n', '\v', '\f' };

		/// <summary>
		/// Array of XML encoding chars
		/// </summary>
		private static readonly char[] _xmlEncodingChars = { '"', '&', '<', '>' };


		/// <summary>
		/// Static constructor
		/// </summary>
		static Utils()
		{
			_isWindows = InnerIsWindows();
		}


		/// <summary>
		/// Determines whether the current operating system is Windows
		/// </summary>
		/// <returns>true if the operating system is Windows; otherwise, false</returns>
		public static bool IsWindows()
		{
			return _isWindows;
		}

		private static bool InnerIsWindows()
		{
#if NETSTANDARD1_3 || NETSTANDARD2_0
			bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
			PlatformID[] windowsPlatformIDs =
			{
				PlatformID.Win32NT,
				PlatformID.Win32S,
				PlatformID.Win32Windows,
				PlatformID.WinCE
			};
			bool isWindows = windowsPlatformIDs.Contains(Environment.OSVersion.Platform);
#endif

			return isWindows;
		}

		/// <summary>
		/// Collapses a whitespace
		/// </summary>
		/// <param name="value">String value</param>
		/// <returns>String value without extra spaces</returns>
		internal static string CollapseWhitespace(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (value.Length == 0
				|| (value.IndexOfAny(_otherWhitespaceChars) == -1 && value.IndexOf("  ", StringComparison.Ordinal) == -1))
			{
				return value;
			}

			StringBuilder sb = null;
			bool previousWhitespace = false;
			int previousCharIndex = 0;
			int charCount = value.Length;

			for (int charIndex = 0; charIndex < charCount; charIndex++)
			{
				char charValue = value[charIndex];
				bool currentWhitespace = charValue.IsWhitespace();

				if (currentWhitespace)
				{
					if (previousWhitespace || charValue != ' ')
					{
						if (sb == null)
						{
							sb = StringBuilderPool.GetBuilder();
						}

						if (previousCharIndex < charIndex)
						{
							sb.Append(value, previousCharIndex, charIndex - previousCharIndex);
						}

						if (!previousWhitespace)
						{
							sb.Append(' ');
						}

						previousCharIndex = charIndex + 1;
					}
				}

				previousWhitespace = currentWhitespace;
			}

			if (sb == null)
			{
				return value;
			}

			if (previousCharIndex < charCount)
			{
				sb.Append(value, previousCharIndex, charCount - previousCharIndex);
			}

			string result = sb.ToString();
			StringBuilderPool.ReleaseBuilder(sb);

			return result;
		}

		internal static string CutShort(string value, int maxLength, string endSymbol = "...")
		{
			string result = value.Trim();

			if (result.Length > maxLength)
			{
				result = result.Substring(0, maxLength).Trim() + endSymbol;
			}

			return result;
		}

		internal static string CutShortByWords(string value, int maxLength, string endSymbol = "...",
			bool isAfterCut = false)
		{
			string result;
			string processedValue = CollapseWhitespace(value).Trim();

			if (processedValue.Length > maxLength)
			{
				if (processedValue.IndexOf(" ", StringComparison.Ordinal) != -1)
				{
					var sb = StringBuilderPool.GetBuilder();

					string[] wordList = processedValue.Split(new [] {' '},
						StringSplitOptions.RemoveEmptyEntries);

					for (int i = 0; i < wordList.Length; i++)
					{
						string word = wordList[i];

						if (!isAfterCut)
						{
							if ((sb.ToString() + " " + word).Length > maxLength)
							{
								break;
							}
						}

						if (sb.Length > 0)
						{
							sb.Append(" ");
						}
						sb.Append(word);

						if (isAfterCut)
						{
							if (sb.Length > maxLength)
							{
								break;
							}
						}
					}

					sb.Append(endSymbol);

					result = sb.ToString();
					StringBuilderPool.ReleaseBuilder(sb);
				}
				else
				{
					result = CutShort(processedValue, maxLength, endSymbol);
				}
			}
			else
			{
				result = processedValue;
			}

			return result;
		}

		/// <summary>
		/// Converts a string to an XML-encoded string
		/// </summary>
		/// <param name="value">The string to encode</param>
		/// <returns>The encoded string</returns>
		internal static string XmlEncode(string value)
		{
			if (string.IsNullOrWhiteSpace(value) || !ContainsXmlEncodingChars(value))
			{
				return value;
			}

			string result;
			StringBuilder sb = StringBuilderPool.GetBuilder();

			using (var writer = new StringWriter(sb))
			{
				int charCount = value.Length;

				for (int charIndex = 0; charIndex < charCount; charIndex++)
				{
					char charValue = value[charIndex];

					switch (charValue)
					{
						case '"':
							writer.Write("&quot;");
							break;
						case '&':
							writer.Write("&amp;");
							break;
						case '<':
							writer.Write("&lt;");
							break;
						case '>':
							writer.Write("&gt;");
							break;
						default:
							writer.Write(charValue);
							break;
					}
				}

				writer.Flush();

				result = writer.ToString();
			}

			StringBuilderPool.ReleaseBuilder(sb);

			return result;
		}

		private static bool ContainsXmlEncodingChars(string value)
		{
			bool result = value.IndexOfAny(_xmlEncodingChars) != -1;

			return result;
		}
	}
}