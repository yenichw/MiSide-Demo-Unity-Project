namespace EPOOutline
{
    /// <summary>
    /// Outline rendering strategy.
    /// </summary>
    public enum OutlineRenderingStrategy
    {
        /// <summary>
        /// All outlines are rendered to the same buffer.
        /// </summary>
        Default,
        /// <summary>
        /// Each outlinable rendered and postprocessed separately. Allows to have overlapping outlinables but greatly increases the performance penalty.
        /// </summary>
        PerObject
    }
}