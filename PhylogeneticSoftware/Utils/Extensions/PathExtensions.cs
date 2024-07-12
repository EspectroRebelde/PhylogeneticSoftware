// Extension of the original GetExtension method

using System.Diagnostics.CodeAnalysis;

namespace PathExtensions
{
    public static class PathExtensions
    {
        /// <summary>
        /// Returns the extension of the given path. The returned value includes the period (".") character of the
        /// extension except when you have a terminal period when you get string.Empty, such as ".exe" or ".cpp".
        /// The returned value is null if the given path is null or empty if the given path does not include an
        /// extension.
        /// </summary>
        [return: NotNullIfNotNull("path")]
        public static string? GetExtension(string? path)
        {
            if (path == null)
                return null;

            return GetExtension(path.AsSpan()).ToString();
        }
    
        /// <summary>
        /// Returns the extension of the given path.
        /// </summary>
        /// <remarks>
        /// The returned value is an empty ReadOnlySpan if the given path does not include an extension.
        /// </remarks>
        public static ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
        {
            int length = path.Length;

            for (int i = length - 1; i >= 0; i--)
            {
                char ch = path[i];
                if (ch == '.')
                {
                    if (i != length - 1)
                    {
                        return path.Slice(i, length - i);
                    }
                    else
                    {
                        return ReadOnlySpan<char>.Empty;
                    }
                }
                if (ch == System.IO.Path.DirectorySeparatorChar || ch == System.IO.Path.AltDirectorySeparatorChar || ch == System.IO.Path.VolumeSeparatorChar)
                    break;
            }
            return ReadOnlySpan<char>.Empty;
        }
        
        [return: NotNullIfNotNull("path")]
        public static string? GetExtensionReversed(string? path)
        {
            if (path == null)
                return null;

            return GetExtensionReversed(path.AsSpan()).ToString();
        }
        
        public static ReadOnlySpan<char> GetExtensionReversed(ReadOnlySpan<char> path)
        {
            int length = path.Length;

            for (int i = 0; i < length; i++)
            {
                char ch = path[i];
                if (ch == '.' && i != length - 1 )
                {
                    char nextChar = path[i + 1];
                    if (nextChar != System.IO.Path.DirectorySeparatorChar &&
                        nextChar != System.IO.Path.AltDirectorySeparatorChar &&
                        nextChar != System.IO.Path.VolumeSeparatorChar)
                    {
                        return path.Slice(i, length - i);
                    }
                }
            }
            return ReadOnlySpan<char>.Empty;
        }
    }
}