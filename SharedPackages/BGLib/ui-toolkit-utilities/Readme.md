---
uid: BGLib.UIToolkitUtilities
summary: A small collection of utilities that help extend Unity's UI Toolkit.
---

# UIToolkitUtilities
A small collection of utilities that help extend Unity's UI Toolkit.

## VisualElementExtensions
Adds functionality to the base class of many controls in the UI Builder

## Controls
A collection of controls that will work in Game and Editor.
All controls should be able to be instantiated directly in code, or be able to be placed on a UI Document and found through the Query system. EG for Table:
```
Table table = visualElement.Query<Table>("my-table");
```

All public functions and variables should be inline documented, so for more details on what happens specifically, please open the class.

Styling classes should follow the format `bg-uitoolkit__<controlname-lowercase>__<your-class-name-lowercase>`  with optionally `--<state>` at the end of the selector, following the [BEM Model](https://en.bem.info/methodology/css/)

For example: `.unity-slider`, `.unity-slider__input`, `.unity-slider__input--disabled`


### Table
```
Table table = new() {
    numRows = myData.Length,
    showHeader = true,
    showFooter = false,
};

myContainer.Add(table);
```

#### Usage
To properly use the table, create an enum type with an entry for each of your columns, and use it to create a `List<TableColumn>` describing your column information.
Then, bind to the `fillCell` event and call `Initialize(yourColumnData);`
```
table.fillCellEvent += (Enum myEnum, int rowIndex, VisualElement cell) => {
    MyRowData rowData = rowDatas[rowIndex];
    switch(myEnum) {
        ColumnTypeOne:
            cell.Add(new Label(rowData.coolText));
            return;
        default:
            return;
    }
}
```

#### TableColumn
The TableColumn class describes information about an individual column, and can be identified by its `cellType`. It's important that you define (or reuse) an enum that will be used with your table as it will allow you to identify the column in the `fillCell` event.

#### FillCellEvent
Will fire for each individual cell. Using your enum and rowIndex you can map your data and add VisualElements like [Labels](https://docs.unity3d.com/ScriptReference/UIElements.Label.html) and [Buttons](https://docs.unity3d.com/ScriptReference/UIElements.Button.html) to each cell.

#### Header & Footers
The header is quite simple and will display the name of the column as the first row.
The footer is similar to a normal row, but it fires a dedicated event (`fillFooterCellEvent`) so you can insert different content, like an add row button.


### TextInputFoldout
Element that adds an additional text field so you can rename the content that you are folding.

#### Styling
Style the table by using the following css classes:
- `bg-uitoolkit__table__row`
- `bg-uitoolkit__table__row-alt`: Applied to every other row
- `bg-uitoolkit__table__cell`
- `bg-uitoolkit__table__cell-header`
- `bg-uitoolkit__table__label-header`


## Editor Controls
A collection of controls that will only work in Editor runtime.
### Table
Extended version of the [base table control](#controls) but with the added option to build a right-click menu to show and hide columns.
Enable the functionality by setting `enableTogglingColumnVisibility` on the class, or set it on the UI Document in the editor.

### TextInputFoldout
Extended version of the [base TextInputFoldout control](#controls) that allows you to bind a SerializedProperty to the text field to automatically sync changes to the Unity Object you are pulling the data from.

### SaveGameObjectElement
Holds a game object that could potentially be saved. It's useful in case you are working in view controller a few layers deeper in your hierarchy, you can easily find this class with `GetFirstAncestorOfType` and invoke a save event on the object you are monitoring.
Maybe to extend, it could monitor the component for if unsaved prefab changes arise and then save it without having to send a signal.

| Function                 | Notes                                                                                                                                                                  |
| ------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| MontorGameObject         | Allows you to monitor a GameObject. The save button text will be adjusted accordingly to whether it can save this GameObject as prefab instance root, or to the scene. |
| StopMonitoringGameObject | Clears the currently monitored gameobject and adjusts text accordingly.                                                                                                |
| PropertyWasModified      | Notify the element that the monitored game object should be saved.                                                                                                     |
