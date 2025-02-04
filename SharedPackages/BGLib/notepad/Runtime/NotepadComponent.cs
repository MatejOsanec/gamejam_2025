using BGLib.HierarchyIcons;
using UnityEngine;

namespace Notepad {

    [HierarchyIcon("Contains a note", Icon.Note, "#828282", "Some child contains a note", Icon.NoteParent, "#5f5f5f")]
    public class NotepadComponent : MonoBehaviour {

        public NoteSO note;
    }
}
