namespace EPOOutline
{
    /// <summary>
    /// The type of blur that is going to be used to blur the outline
    /// </summary>
    public enum BlurType
    {
        /// <summary>
        /// Simple 3x3 box blur
        /// </summary>
        Box = 1,
        /// <summary>
        /// Gaussian blur with 3x3 kernel
        /// </summary>
        Gaussian5x5 = 2,
        /// <summary>
        /// Gaussian blur with 9x9 kernel
        /// </summary>
        Gaussian9x9 = 3,
        /// <summary>
        /// Gaussian blur with 13x13 kernel
        /// </summary>
        Gaussian13x13 = 4
    }
}