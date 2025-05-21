using System;

namespace EPOOutline
{
    /// <summary>
    /// Color mask.
    /// </summary>
    [Flags]
    public enum ColorMask
    {
        /// <summary>
        /// No channels are in use.
        /// </summary>
        None = 0,
        /// <summary>
        /// Red channel.
        /// </summary>
        R = 1,
        /// <summary>
        /// Green channel.
        /// </summary>
        G = 2,
        /// <summary>
        /// Blue channel.
        /// </summary>
        B = 4,
        /// <summary>
        /// Alpha channel.
        /// </summary>
        A = 8
    }
}