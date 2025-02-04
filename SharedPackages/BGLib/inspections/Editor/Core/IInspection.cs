namespace BGLib.Inspections.Editor.Core {

    public interface IInspection {

        string name { get; }

        bool isCritical { get; }

        InspectionResult Inspect();

        void Fix();
    }
}
