namespace EPOOutline
{
    /// <summary>
    /// Describes dilate quality to use to render the outline.
    /// </summary>
    public enum DilateQuality
    {
        /// <summary>
        /// Base quality. Normal quality with the best performance.
        /// </summary>
        Base,
        /// <summary>
        /// Higher quality dilate, but requires more processing power.
        /// </summary>
        High,
        /// <summary>
        /// Best quality dilate, but requires has a performance overhead.
        /// </summary>
        Ultra
    }
}