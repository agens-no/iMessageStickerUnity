using System.Globalization;

namespace Agens.Stickers
{
    public static class StickerEditorUtility
    {
        /// <summary>
        /// Checks the file extension of a filename
        /// </summary>
        /// <param name="fileName">filename with extension or full path of a file</param>
        /// <returns>True if the file type is supported for stickers</returns>
        public static bool HasValidFileExtension(string fileName)
        {
            var s = fileName.ToLower(CultureInfo.InvariantCulture);
            return s.EndsWith(".png") || s.EndsWith(".gif") || s.EndsWith(".jpg") || s.EndsWith(".jpeg");
        }

        /// <summary>
        /// Creates a readable string in Bytes or KiloBytes of a file
        /// 1 kB is 1000B in this case
        /// </summary>
        /// <param name="size">File size in byte</param>
        /// <returns>File size with fitting units</returns>
        public static string GetFileSizeString(long size)
        {
            if (size < 1000)
            {
                return size.ToString() + "B";
            }
            else
            {
                return (size / 1000L).ToString() + " kB";
            }
        }

        /// <summary>
        /// Checks if the filetype is an animated file or not.
        /// Supported file checks: .gif
        /// </summary>
        /// <param name="filePath">Path to the file or filename</param>
        /// <returns>True if the file type supports animations</returns>
        public static bool IsAnimatedTexture(string filePath)
        {
            var s = filePath.ToLower(CultureInfo.InvariantCulture);
            return s.EndsWith(".gif");
        }
    }
}