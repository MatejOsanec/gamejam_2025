using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncHelper {

    public static void RunSync(Func<Task> asyncTask) {

        Task.Run(asyncTask).GetAwaiter().GetResult();
    }

    public static T RunSync<T>(Func<Task<T>> asyncTask) {

        return Task.Run(asyncTask).GetAwaiter().GetResult();
    }

    public static async Task<bool> AnyTaskTrueNonAlloc(List<Task<bool>> tasks) {

        while (tasks.Count > 0) {
            var finishedTask = await Task.WhenAny(tasks);
            if (finishedTask.Result) {
                return true;
            }
            tasks.Remove(finishedTask);
        }

        return false;
    }
}
