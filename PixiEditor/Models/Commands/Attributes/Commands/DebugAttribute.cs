﻿namespace PixiEditor.Models.Commands.Attributes
{
    public partial class Command
    {
        public class DebugAttribute : BasicAttribute
        {
            public DebugAttribute(string internalName, string displayName, string description) : base($"#DEBUG#{internalName}", displayName, description)
            {
            }

            public DebugAttribute(string internalName, object parameter, string displayName, string description)
                : base($"#DEBUG#{internalName}", parameter, displayName, description)
            {
            }
        }
    }
}
