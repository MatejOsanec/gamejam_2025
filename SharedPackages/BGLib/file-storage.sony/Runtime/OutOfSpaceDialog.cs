#if UNITY_PS4 || UNITY_PS5

using System.Threading.Tasks;
using UnityEngine;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Dialog;
#endif

public class OutOfSpaceDialog {

    public static void Show(SaveDataAsyncEventScheduler saveDataAsyncEventScheduler, int userId, DirName[] outOfSpaceDirNames, ulong requiredBlocks, bool useAnimations = true) {

        Dialogs.OpenDialogRequest openDialogRequest = GetOpenDialogRequest(userId, outOfSpaceDirNames, requiredBlocks, useAnimations, isAsync: false);

        try {
            Dialogs.OpenDialog(openDialogRequest, new Dialogs.OpenDialogResponse());
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when showing out of space dialog with message: {exception.ExtendedMessage}");
        }
    }

    public static Task ShowAsync(SaveDataAsyncEventScheduler saveDataAsyncEventScheduler, int userId, DirName[] outOfSpaceDirNames, ulong requiredBlocks, bool useAnimations = true) {

        var openDialogRequest = GetOpenDialogRequest(userId, outOfSpaceDirNames, requiredBlocks, useAnimations, isAsync: true);
        return saveDataAsyncEventScheduler.InvokeAsyncEvent(Dialogs.OpenDialog, openDialogRequest, new Dialogs.OpenDialogResponse());
    }

    private static Dialogs.OpenDialogRequest GetOpenDialogRequest(
        int userId,
        DirName[] outOfSpaceDirNames,
        ulong requiredBlocks,
        bool useAnimations,
        bool isAsync
    ) {

        Dialogs.Animation animationMode = useAnimations ? Dialogs.Animation.On : Dialogs.Animation.Off;
        Dialogs.AnimationParam animationParam = new(animationMode, animationMode);

        Dialogs.SystemMessageParam systemMessageParam = new() {
            SysMsgType = Dialogs.SystemMessageType.NoSpaceContinuable,
            Value = requiredBlocks,
        };

        Dialogs.Items items = new() {
            DirNames = outOfSpaceDirNames
        };

        Dialogs.OptionParam optionParam = new() {
            Back = Dialogs.OptionBack.Disable
        };

        return new Dialogs.OpenDialogRequest() {
            UserId = userId,
            Async = isAsync,
            SystemMessage = systemMessageParam,
            Mode = Dialogs.DialogMode.SystemMsg,
            Animations = animationParam,
            DispType = Dialogs.DialogType.Save,
            Option = optionParam,
            Items = items,
        };
    }
}
#endif // UNITY_PS4 || UNITY_PS5