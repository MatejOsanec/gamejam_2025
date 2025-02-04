namespace BGLib.SaveDataCore {

    public enum SaveDataResult : int {
        OK_NoFileLoaded = -2,
        OK_NotDirty = -1,
        OK = 0,

        GenericError = 100,
        UnknownLoaderState = 101,

        // Loading
        FailedToLoadOrDeserialize = 200,
        SynchronousLoadAlreadyInLoadingState = 201,
        AsyncLoadStateButNoTask = 202,
        UpdateMethodDoesNotExist = 203,
        UpdateMethodFailed = 204,
        CriticalPostLoadStepFailed = 205,

        // Saving
        LoadingNotCompleted = 300,
        NoInstanceToSave = 301,

        // Access
        AttemptedAccessWhileUnloaded = 400,
        AttemptedReloadWhileLoading = 401,
    }

    public static class SaveDataResultExtensions {

        public static bool IsError(this SaveDataResult result) => result > 0;
    }
}
