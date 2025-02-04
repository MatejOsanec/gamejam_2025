namespace BGLib.BuildProcess.Editor {

    using System.Collections;

    public interface IBuildStageWaiter {

        IEnumerator Wait();
    }
}
