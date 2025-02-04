# Build Process
Build Process Library is purposed to create and run persisting staged build process with optional delays between stages

## Getting started

1. Implement custom build steps
```C#
    public class CustomBuildStep : BaseLeafBuildStage {
        public CustomBuildStep(float normalizedProgress) 
            : base("Do something cool", normalizedProgress) { }

        protected override IBuildStageWaiter Execute(BuildProcessRunner runner) {
            if (!isActionRequired) {
                return dontWait;
            }
            
            DoAction();

            return waitForEditor;
        }
    }
```
2. Extend the `BuildProcess` and populate it with custom build steps
```C#
    public class CustomBuildProcess : BuildProcess {
        public CustomBuildProcess() : base(
            new IBuildStage[] {
                new CustomBuildStep(normalizedProgress: 0.3F),
                new AnotherBuildStep(normalizedProgress: 0.7F)
            },
            startNormalizedProgress: 0,
            endNormalizedProgress: 1
        ) { }
    }
```
3. Implement `IBuildProcessHandler` interface with reporting callbacks
4. Instantiate a `BuildProcessRunner` with the custom process and process handler and arguments
5. Start a build process using `BuildProcessRunner.Start`

## Known issues


## Roadmap
