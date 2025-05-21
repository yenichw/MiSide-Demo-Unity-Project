using System;

namespace EPOOutline
{
    /// <summary>
    /// Describes <see cref="EPOOutline.Outlinable"/> rendering modes.
    /// </summary>
    [Flags]
    public enum OutlinableDrawingMode
    {
        /// <summary>
        /// Simple outline.
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Renders to Z buffer explicitly. Useful for things that don't usually render to ZBuffer, like sprites or transparent objects.
        /// </summary>
        ZOnly = 2,
        /// <summary>
        /// The object is rendered as a generic mask.
        /// </summary>
        GenericMask = 4,
        /// <summary>
        /// The object is rendered as an obstacle.
        /// </summary>
        Obstacle = 8,
        /// <summary>
        /// The object is rendered as a mask.
        /// </summary>
        Mask = 16
    }
}