using System.Collections.Generic;

namespace BGLib.HierarchyIcons {

    public enum Icon {
        None,
        EditOffTintable,
        EditTintable,
        Note,
        NoteParent
    }

    internal class Defines {

        public static Dictionary<Icon, string> iconDatabase = new() {
            { Icon.None, string.Empty },
            { Icon.EditOffTintable, "Packages/com.beatgames.bglib.hierarchy-icons/Icons/EditOffTintable.png" },
            { Icon.EditTintable, "Packages/com.beatgames.bglib.hierarchy-icons/Icons/EditTintable.png" },
            { Icon.Note, "Packages/com.beatgames.bglib.hierarchy-icons/Icons/Note.png" },
            { Icon.NoteParent, "Packages/com.beatgames.bglib.hierarchy-icons/Icons/NoteParent.png" },
        };
    }
}
