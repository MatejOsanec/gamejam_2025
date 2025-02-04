namespace BGLib.UIToolkitUtilities.Controls.Editor {
#nullable enable

    using UnityEditor;
    using UnityEngine.UIElements;

    public ref struct EventCancellationToken {

        public bool isCancelled { get; private set; }

        public void Cancel() {

            isCancelled = true;
        }
    }

    public class StringListPanel : VisualElement {

        public static readonly string kUssClassName = "bg-uitoolkit__string-list";
        public static readonly string kHeaderUssClassName = kUssClassName + "__header";
        public static readonly string kItemUssClassName = kUssClassName + "__item";
        public static readonly string kIconUssClassName = kUssClassName + "__icon";

        public static readonly string kSampleHeaderText = "Sample Header";
        public static readonly string kSampleItemText = "Sample Item";

        private VisualElement? _selectedRow;
        private readonly VisualElement _content;

        public new class UxmlFactory : UxmlFactory<StringListPanel> { }

        public delegate void OnItemSelected(object? userData, ref EventCancellationToken evtCancellationToken);

        /// <summary>
        /// Fired whenever a new item is selected.
        /// </summary>
        public event OnItemSelected? onItemSelected;

        /// <summary>
        /// Whether a row is currently selected or not
        /// </summary>
        public bool rowSelected => _selectedRow != null;

        public StringListPanel() {

            var scroll = new ScrollView();
            scroll.mode = ScrollViewMode.VerticalAndHorizontal;
            Add(scroll);

            _content = new VisualElement();
            _content.AddToClassList(BaseVerticalCollectionView.ussClassName);
            _content.AddToClassList(ListView.ussClassName);
            scroll.Add(_content);

            var sampleFoldout = AppendHeader(kSampleHeaderText);
            AppendItem(kSampleItemText, null, sampleFoldout);

            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Defines.kGlobalStyleSheet));
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Defines.kStyleSheetDirectory + "StringList.uss"));
        }

        private void OnItemClicked(ClickEvent clickEvent, VisualElement item) {

            if (_selectedRow == item) {
                return;
            }

            var cancellationToken = new EventCancellationToken();
            onItemSelected?.Invoke(item.userData, ref cancellationToken);

            if (cancellationToken.isCancelled) {
                return;
            }

            _selectedRow?.RemoveFromClassList(BaseVerticalCollectionView.itemSelectedVariantUssClassName);
            _selectedRow = item;
            _selectedRow.AddToClassList(BaseVerticalCollectionView.itemSelectedVariantUssClassName);
        }

        public void ClearContent() {

            _content.Clear();
            _selectedRow = null;
        }

        private static VisualElement CreateItem(string itemText) {

            // todo: pool rows
            var item = new VisualElement();
            item.AddToClassList(kItemUssClassName);
            item.AddToClassList(BaseVerticalCollectionView.itemUssClassName);
            item.AddToClassList(ListView.itemUssClassName);

            var icon = new VisualElement();
            icon.AddToClassList(kIconUssClassName);
            item.Add(icon);

            var label = new Label(itemText.Replace(@"\", @"\\"));

            item.Add(label);

            return item;
        }

        public Foldout AppendHeader(string headerText) {

            var foldout = new Foldout();
            foldout.AddToClassList(kHeaderUssClassName);
            foldout.text = headerText;
            _content.Add(foldout);

            return foldout;
        }

        public void AppendItem(string itemText, object? itemUserData, Foldout? header = null) {

            var item = CreateItem(itemText);
            if (header == null) {
                _content.Add(item);
            }
            else {
                header.Add(item);
            }
            item.userData = itemUserData;
            item.RegisterCallback<ClickEvent, VisualElement>(OnItemClicked, item);
        }
    }

#nullable disable
}
