using UnityEngine;
using UnityEngine.UI;

namespace BGLib.Polyglot {

    [RequireComponent(typeof(Dropdown))]
    [AddComponentMenu("UI/Language Dropdown", 36)]
    public class LanguageDropdown : MonoBehaviour, ILocalize {

        [Tooltip("The dropdown to populate with all the available languages")]
        [SerializeField]
        private Dropdown dropdown = default!;

        public void Reset() {
            dropdown = GetComponent<Dropdown>();
        }

        public void Start() {

            CreateDropdown();

            Localization.Instance.AddOnLocalizeEvent(this);
        }

        private void CreateDropdown() {

            var flags = dropdown.hideFlags;
            dropdown.hideFlags = HideFlags.DontSaveInEditor;

            dropdown.options.Clear();

            var languageNames = Localization.Instance.englishLanguageNames;

            for (int index = 0; index < languageNames.Count; index++) {
                var languageName = languageNames[index];
                dropdown.options.Add(new Dropdown.OptionData(languageName));
            }

            dropdown.value = -1;
            dropdown.value = Localization.Instance.selectedLanguageIndex;

            dropdown.hideFlags = flags;
        }

        public void OnLocalize(LocalizationModel localization) {

            dropdown.onValueChanged.RemoveListener(localization.SelectLanguage);
            dropdown.value = localization.selectedLanguageIndex;
            dropdown.onValueChanged.AddListener(localization.SelectLanguage);
        }
    }
}
