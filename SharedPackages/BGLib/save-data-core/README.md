# Save Data Core
Core functionality for save data and settings; versioning, serialization and save location.

## Getting started
First, Ensure the package is loaded and your asmdef contains a reference to the runtime assembly.
We can then start creating the required classes for our save files.

#### VersionableSaveData
This gives us free versioning and dirty tracking in our file, you are required to inherit from it.
```c#
public class MyCoolSaveFile : VersionableSaveData {

    /// <summary>
    /// This is multiplied by an internal multiplier.
    /// </summary>
    [JsonIgnore]
    public int pauseButtonPressDurationLevel {
        get { return _pauseButtonPressDurationLevel; }
        set {
            isDirty = true;
            _pauseButtonPressDurationLevel = value;
        }
    }

    [JsonProperty("pauseButtonPressDurationLevel")]
    private int _pauseButtonPressDurationLevel = 0;


    /// <summary>
    /// Rotation of the room compared to the data provided by the VR APIs.
    /// </summary>
    [JsonIgnore]
    public float roomRotation {
        get { return _roomRotation; }
        set {
            isDirty = true;
            _roomRotation = value;
        }
    }

    [JsonProperty("roomRotation")]
    private float _roomRotation = 0;
}
```

Making the public way of interacting with your variable a method gives us the ability to mark the file as dirty when you modify it.

##### Sub-Classes and other containers
These are a bit trickier to mark dirty. There are a couple of approaches.
- Implement `ISerializableSaveData` interface. This forces you to add an isDirty bool to your class, and then you can rely on accessing that in `VersionableSaveData.ClearDirtyRecursive`
- Manually marking the file as dirty when you modify something inside your sub-class.


#### SaveDataHandler
Is the manager for your save file and the main way to interact with the save file. 
The main purpose is set-up to link your save file type, handling updates and config
```c#
public class MySaveDataHandler : SaveDataHandler<MyCoolSaveFile> {

    protected override Version version => new Version("1.0.0");
    protected override Version firstVersion => return new Version("1.0.0");
    protected override string fileNameWithExtension => return "MyCoolSaveFile.json";

    public MySaveDataHandler(IFileStorage fileStorage) : base(fileStorage) { }
}
```

Overriding InternalLoadAsync will give you the possibility to verify the save file, or maintain loadable references that you want to keep consistent with the save file.
```c#
    public class MySaveDataHandler : SaveDataHandler<MyCoolSaveFile> {

        ...

        protected override async Task<SaveDataResult> InternalLoadAsync() {

            SaveDataResult baseResult = await base.InternalLoadAsync();
            if (baseResult > SaveDataResult.OK) {
                return baseResult;
            }

            try {
                // Update a local reference
                // clamp value between x & y that could have been modified on disk
            }
            catch (Exception e) {
                Debug.LogException(e);
                return SaveDataResult.CriticalPostLoadStepFailed;
            }

            return baseResult;
        }
    }
```

#### Interacting with your save file
```c#
// Var definition
// All of these are recommended to have their lifetime maintained in Zenject.
IFileStorage fileStorage;
MySaveDataHandler mySaveDataHandler = new(fileStorage);
SaveDataFlushingService saveDataFlushingService = new(fileStorage);

// In your functions:
saveDataFlushingService.Register(mySaveDataHandler);

bool result = await mySaveDataHandler.LoadAsync();

// automatically marks the save file as dirty
mySaveDataHandler.instance.myCoolInt = 1337;

// Implementations may differ here, check whether it inherits from SerializableSaveDataSubClass
mySaveDataHandler.instance.coolClass.coolNestedInt = 4834;

// Then in another spot, on a regular basis call...
saveDataFlushingService.FlushSaveFiles();

// or maybe if you goofed up
await saveDataFlushingService.ResetChangesAsync();

// Or directly inline
bool result = await mySaveDataHandler.SaveAsync();

```

from editor you have access to a little more options, like deleting the save file, or non-async functions.
```c#
// fire & forget
mySaveDataHandler.DeleteAsync();
```

## Update System
A built-in system to support updates to your save files with full history compatibility. 

> ⚠ It does not (yet) handle maintaining data across structural changes ⚠ <br/>

If you wish to update your file, first bump the version:
```diff
public class MySaveDataHandler : SaveDataHandler<MyCoolSaveFile> {

-    protected override Version version { get { return new Version("1.0.0"); } }
+    protected override Version version { get { return new Version("1.0.1"); } }
```
<br/>

Then, in your save data handler, create a migration function:
```c#
public class MySaveDataHandler : SaveDataHandler<MyCoolSaveFile> {

    // ...

    public Version UpdateFromVersion_1_0_0(MyCoolSaveFile deserializedJson) {

        deserializedJson.performancePresetKey = "newDefaultKey";
        return new ("1.0.1");
    }
}
```

The migration function is supposed to upgrade the data from the version it finds on disk, to whatever version you are currently supporting. 
Therefore, it's important that you have a full 'chain' of update functions present in your save data handler. 

Whether you are bumping [major, minor or patch](https://semver.org/), it does not matter. A function just has to exist to get anyone up from whatever old version they are at, back up to modern times.

Since your handler class can theoretically grow very large because of this, It's recommended to insert new update functions up top, so the most recent ones can immediately be found.

### Testing the Update System
With reflection, you can find all classes inheriting from SaveDataHandler
```c#
public static IEnumerable<UpdaterTestsDescriptor> GetAllSaveDataHandlerDerivatives() {

    // In assembly, this turns into "SaveDataHandler`1" because of the <T>. Strings just avoid that 🤡
    var allTypes = typeof(MainSettingsHandler).Assembly.GetTypes();
    var types = allTypes.Where(t => t.BaseType.ToString().Contains("SaveDataHandler"));

    List<UpdaterTestsDescriptor> result = new();
    foreach (var type in types) {
        result.Add(new() { type = type });
    }

    return result;
}
```
<br/>

Then, in your test:
```c#
var testHandler = (ISaveDataHandler)Activator.CreateInstance(_toTest, new [] { fileStorage });
testHandler.TestFullUpdateLoop();
```

`TestFullUpdateLoop` will emulate going from `firstVersion` to `version` and has internal exceptions for when you are missing part of your update chain.

> ⚠ It will not test the actual contents of your updates!


## Known issues
N/A as of this commit.

## Roadmap
N/A
