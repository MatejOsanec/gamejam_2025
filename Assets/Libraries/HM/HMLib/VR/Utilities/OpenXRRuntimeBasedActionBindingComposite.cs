using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[DisplayStringFormat("{oculusRuntime}+{otherRuntimes}")]
public class OpenXRRuntimeBasedActionBindingComposite  : InputBindingComposite<float> {

#if BS_OPENXR_VR
    private const string OCULUS_RUNTIME_NAME = "Oculus";
#endif

    [InputControl]
    public int oculusRuntime;

    [InputControl]
    public int otherRuntimes;


    public override float ReadValue(ref InputBindingCompositeContext context) {

#if BS_OPENXR_VR
        return UnityEngine.XR.OpenXR.OpenXRRuntime.name switch {
            OCULUS_RUNTIME_NAME => ReadOculusRuntimeValue(ref context) ,
            _ => ReadOtherRuntimeValue(ref context)
        };
#else
        //In this case we return -1 which means that no action happened because we have openXR support only for PC
        return -1;
#endif
    }

#if BS_OPENXR_VR
    private float ReadOculusRuntimeValue(ref InputBindingCompositeContext context) {

        return context.ReadValue<float>(oculusRuntime);
    }

    private float ReadOtherRuntimeValue(ref InputBindingCompositeContext context) {

        return context.ReadValue<float>(otherRuntimes);
    }
#endif

    public override float EvaluateMagnitude(ref InputBindingCompositeContext context) {

        return ReadValue(ref context);
    }

    static OpenXRRuntimeBasedActionBindingComposite() {

        InputSystem.RegisterBindingComposite<OpenXRRuntimeBasedActionBindingComposite>();
    }

    //kudos to unity devs who made this empty function required for the builds
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init() { }
}
