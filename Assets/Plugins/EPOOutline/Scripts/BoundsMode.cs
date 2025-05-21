namespace EPOOutline
{
    /// <summary>
    /// The bounds mode that is going to be used for bounds calculation for renderers.
    /// </summary>
    public enum BoundsMode
    {
        /// <summary>
        /// The bounds are going to be taken from the renderer itself.
        /// </summary>
        Default,
        /// <summary>
        /// The bounds are going to be taken from the renderer, but it will be recalculated each time.
        /// </summary>
        ForceRecalculate,
        /// <summary>
        /// The bounds are going to be set manually.
        /// </summary>
        Manual
    }
}