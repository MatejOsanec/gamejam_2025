using System.Linq;
using BGLib.AppFlow.Initialization;
using UnityEditor;
using Zenject;

[CanEditMultipleObjects]
[CustomEditor(typeof(AsyncSceneContext))]
public class AsyncSceneContextEditor : SceneContextEditor {

    protected override string[] PropertyNames => base.PropertyNames
        .Append("_asyncPreloaders")
        .Append("_asyncInstallers").ToArray();

    protected override string[] PropertyDisplayNames => base.PropertyDisplayNames
        .Append("Async Preloaders")
        .Append("Async Installers").ToArray();

    protected override string[] PropertyDescriptions => base.PropertyDescriptions
        .Append("Drag any IAsyncPreloader that you have added as a Mono installer to the scene context here")
        .Append("Drag any AsyncInstallers that you have added to your Scene Hierarchy here.").ToArray();
}
