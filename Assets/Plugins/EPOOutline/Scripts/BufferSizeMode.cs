namespace EPOOutline
{
    /// <summary>
    /// Describes how main buffer size is going to be calculated.
    /// </summary>
    public enum BufferSizeMode
    {
        /// <summary>
        /// Uses width and the <see cref="EPOOutline.Outliner.PrimarySizeReference"/> to calculate the buffer size.
        /// The width of the buffer will be equal to the <see cref="EPOOutline.Outliner.PrimarySizeReference"/>.
        /// The height will be calculated to match the aspect ratio.
        /// </summary>
        WidthControlsHeight,
        /// <summary>
        /// Uses height and the <see cref="EPOOutline.Outliner.PrimarySizeReference"/> to calculate the buffer size.
        /// The height of the buffer will be equal to the <see cref="EPOOutline.Outliner.PrimarySizeReference"/>.
        /// The width will be calculated to match the aspect ratio.
        /// </summary>
        HeightControlsWidth,
        /// <summary>
        /// The buffer size will be calculated as the target size (usually the screen size) scaled by <see cref="EPOOutline.Outliner.PrimaryRendererScale"/>.
        /// </summary>
        Scaled,
        /// <summary>
        /// The buffer size will be equal to the target size (usually the screen size).
        /// </summary>
        Native
    }
}