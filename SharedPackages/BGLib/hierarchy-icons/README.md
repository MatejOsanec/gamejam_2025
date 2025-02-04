# Hierarchy Icons
This package adds functionality to show icons on objects in the Unity Hierarchy view.
By default, it will highlight dirty prefabs in the scene

## Dirty prefabs in scene
If your prefab has a red <img src="Icons/EditOffTintable.png" alt="drawing" style="width:20px;"/> icon in your hierarchy view, it means that that prefab has changes that are not saved down.

By adding a `HierarchyIgnorePrefabOverrides` component to your scene you can change it to a green <img src="Icons/EditTintable.png" alt="drawing" style="width:20px;"/> icon, meaning it is a tolerated local change.

## Custom icons for MonoBehaviours
These are added via Annotation. An example from BGLib.Notepad:
```c#
namespace Notepad {

    [HierarchyIcon("Contains a note", "Packages/com.beatgames.bglib.hierarchy-icons/Icons/Note.png", "#828282",
        "Some child contains a note", "Packages/com.beatgames.bglib.hierarchy-icons/Icons/NoteParent.png", "#5f5f5f")]
    public class NotepadComponent : MonoBehaviour {

        public NoteSO note;
    }
}
```

The second line of parent icons are optional and can be omitted. Colors can be omitted as well, then your icon will render as white.

The benefit of this setup is that you can include _this_ package where you want to setup icons, instead of this package importing each location that wants to add icons.

## Example
<img src="Documentation~/Example.png">
