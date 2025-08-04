// based on the original game.Yen Chezky(yenichw)
using System;

namespace EPOOutline
{
    public class SerializedPassInfoAttribute : Attribute
    {
        public readonly string Title;
        public readonly string ShadersFolder;

        public SerializedPassInfoAttribute(string title, string shadersFolder)
        {
            Title = title;
            ShadersFolder = shadersFolder;
        }
    }
}