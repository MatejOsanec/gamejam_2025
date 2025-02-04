#if UNITY_PS4 || UNITY_PS5

using System.Threading.Tasks;
using UnityEngine;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Dialog;
#endif

public static class CorruptedSaveDialog {

    public static void Show(SaveDataAsyncEventScheduler saveDataAsyncEventScheduler, int userId, DirName[] corruptedDirNames, bool canRestoreDirectories, bool useAnimations = true) {

        var openDialogRequest = GetOpenDialogRequest(userId, corruptedDirNames, useAnimations, canRestoreDirectories, isAsync: false);

        try {
            Dialogs.OpenDialog(openDialogRequest, new Dialogs.OpenDialogResponse());
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when showing corrupted save dialog with message: {exception.ExtendedMessage}");
        }
    }

    public static Task ShowAsync(SaveDataAsyncEventScheduler saveDataAsyncEventScheduler, int userId, DirName[] corruptedDirNames, bool canRestoreDirectories, bool useAnimations = true) {

        var openDialogRequest = GetOpenDialogRequest(userId, corruptedDirNames, useAnimations, canRestoreDirectories, isAsync: true);
        return saveDataAsyncEventScheduler.InvokeAsyncEvent(Dialogs.OpenDialog, openDialogRequest, new Dialogs.OpenDialogResponse());
    }

    private static Dialogs.OpenDialogRequest GetOpenDialogRequest(int userId, DirName[] corruptedDirNames, bool useAnimations, bool canRestoreDirectories, bool isAsync) {

        Dialogs.Animation animationMode = useAnimations ? Dialogs.Animation.On : Dialogs.Animation.Off;
        Dialogs.AnimationParam animationParam = new(animationMode, animationMode);

        Dialogs.SystemMessageParam systemMessage = new();

#if UNITY_PS4
        systemMessage.SysMsgType = canRestoreDirectories ? Dialogs.SystemMessageType.CurruptedAndRestore : Dialogs.SystemMessageType.CorruptedAndDelete;
#elif UNITY_PS5
        systemMessage.SysMsgType = Dialogs.SystemMessageType.Corrupted;
#endif

        Dialogs.Items items = new() {
            DirNames = corruptedDirNames
        };

        Dialogs.OptionParam optionParam = new() {
            Back = Dialogs.OptionBack.Enable
        };

        return new Dialogs.OpenDialogRequest() {
            UserId = userId,
            Async = isAsync,
            Mode = Dialogs.DialogMode.SystemMsg,
            DispType = Dialogs.DialogType.Load,
            Animations = animationParam,
            SystemMessage = systemMessage,
            Items = items,
            Option = optionParam
        };
    }
}
#endif // UNITY_PS4 ||UNITY_PS5
