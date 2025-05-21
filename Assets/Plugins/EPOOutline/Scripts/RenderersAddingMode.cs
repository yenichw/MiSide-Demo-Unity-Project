using System;

namespace EPOOutline
{
    /// <summary>
    /// Describes how to filter renderers when adding to the list.
    /// </summary>
    [Flags]
    public enum RenderersAddingMode
    {
        /// <summary>
        /// Adds all renderers.
        /// </summary>
        All = -1,
        /// <summary>
        /// Adds none of the renderers.
        /// </summary>
        None = 0,
        /// <summary>
        /// Adds <see cref="UnityEngine.MeshRenderer"/>s.
        /// </summary>
        MeshRenderer = 1,
        /// <summary>
        /// Adds <see cref="UnityEngine.SkinnedMeshRenderer"/>s.
        /// </summary>
        SkinnedMeshRenderer = 2,
        /// <summary>
        /// Adds <see cref="UnityEngine.SpriteRenderer"/>s.
        /// </summary>
        SpriteRenderer = 4,
        /// <summary>
        /// Adds other kinds of renderers.
        /// </summary>
        Others = 4096
    }
}