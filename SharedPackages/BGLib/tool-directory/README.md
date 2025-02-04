# Tool Directory
A tool to find and get information about tools in the project.

## Getting started
### Users
Open the tool via the 'Beat Games/Tools Directory' context menu and explore tools.

### Developers
Use the `AddToToolDirectory` attribute to annotate your core class. Your tool will now be displayed in the tool window.

To add new labels: add a new enum type in `LabelType` and create a new entry corresponding to your new type in `LabelDefinitions::_labelDefinitions`.

To add new authors: add a new enum type in `ToolMaintainer` and create a new entry corresponding to your new type in `ToolMaintainers::_toolMaintainers`.

## Known issues
None.

## Roadmap
- Unit tests. There are a few asserts to catch problems that should also be unit tested
- Quick search functionality with a shortcut to open the window as a dialog