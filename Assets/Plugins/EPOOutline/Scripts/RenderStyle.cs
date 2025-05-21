namespace EPOOutline
{
    /// <summary>
    /// Describes how outlinable should be rendered.
    /// </summary>
    public enum RenderStyle
    {
        /// <summary>
        /// Always renders the same. No matter obscured or not.
        /// </summary>
        Single = 1,
        /// <summary>
        /// Renders with different settings when obscured and clear.
        /// </summary>
        FrontBack = 2
    }
}