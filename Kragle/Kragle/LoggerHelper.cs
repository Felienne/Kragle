namespace Kragle
{
    /// <summary>
    ///     Static helper methods for nicely formatting log messages.
    /// </summary>
    public class LoggerHelper
    {
        /// <summary>
        ///     Makes a <code>string</code> of the given length. If the string is too short, the given character is padded on the
        ///     right. If the string is too long, the string is cut off and the last characters are replaced with "...".
        /// </summary>
        /// <param name="s">a <code>string</code></param>
        /// <param name="len">the desired string length</param>
        /// <param name="pad">the padding character</param>
        /// <returns>the string of the desired length</returns>
        public static string ForceLength(string s, int len, char pad = ' ')
        {
            if (s.Length > len)
            {
                return s.Substring(0, len - 3) + "...";
            }

            return s.PadRight(len, pad);
        }

        /// <summary>
        ///     Formats a string to indicate a degree of progress.
        /// </summary>
        /// <param name="msg">the status message</param>
        /// <param name="current">the dividend</param>
        /// <param name="max">the divisor</param>
        /// <returns></returns>
        public static string FormatProgress(string msg, double current, double max)
        {
            return string.Format("{0} ({1} / {2}) ({3:P2})", msg, current, max, current / max);
        }
    }
}
