namespace BGLib.Polyglot {
    
    using UnityEngine;
    
    public class SaveLanguagePreference : MonoBehaviour, ILocalize {
        [SerializeField]
        private string preferenceKey = "Polyglot.SelectedLanguage";

        public void Start() {
            
            Localization.Instance.SelectedLanguage = (Language)PlayerPrefs.GetInt(preferenceKey);
            Localization.Instance.AddOnLocalizeEvent(this);
        }

        public void OnLocalize(LocalizationModel localization) {
            
            PlayerPrefs.SetInt(preferenceKey, (int)localization.SelectedLanguage);
        }
    }
}
