namespace BGLib.Inspections.Editor.Core {

    using System.Collections.Generic;

    public interface IInspectionGroup {

        string name { get; }
        IEnumerable<IInspection> inspections { get; }
    }
}
