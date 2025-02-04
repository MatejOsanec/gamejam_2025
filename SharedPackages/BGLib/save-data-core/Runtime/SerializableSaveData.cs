namespace BGLib.SaveDataCore {

    using Newtonsoft.Json;

    /// <summary>
    /// Interface that allows classes that are, or are members of the save file to be marked as dirty.
    /// State management is handled manually.
    /// </summary>
    public interface ISerializableSaveData {

        [JsonIgnore]
        public bool isDirty { get; set; }
    }

    /// <summary>
    /// Base class for all save files.
    /// </summary>
    public abstract class VersionableSaveData : ISerializableSaveData {

        [JsonProperty(Order = 0)]
        public string version = "0.0.0";

        [JsonIgnore]
        public virtual bool isDirty {
            get => isDirty;
            set => _isDirty = value;
        }

        [JsonIgnore]
        protected bool _isDirty = false;
    }
}
