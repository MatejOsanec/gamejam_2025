namespace BGLib.Polyglot {

    using System.Collections.Generic;
    using UnityEngine;

    public abstract class LocalizedTextComponent<T> : MonoBehaviour, ILocalize where T : Component {

        [Tooltip("The text component to localize")]
        [SerializeField]
        protected T localizedComponent = default!;

        [Tooltip(
            "Maintain original text alignment. If set to false, localization will determine whether text is left or right aligned"
        )]
        [SerializeField]
        private bool maintainTextAlignment;
        public bool MaintainTextAlignment => maintainTextAlignment;

        [Tooltip("The key to localize with")]
        [SerializeField]
        [LocalizationKey]
        private string key = string.Empty;

        public string Key {
            get => key;
            set {
                key = value;
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return;
                }
#endif
                OnLocalize(Localization.Instance);
            }
        }

        public List<object> Parameters => parameters;

        private readonly List<object> parameters = new List<object>();

        public void Start() {

            Localization.Instance.AddOnLocalizeEvent(this);
        }

        public void OnDestroy() {

            Localization.Instance.RemoveOnLocalizeEvent(this);
        }

        protected abstract void SetText(T component, string value);

        protected abstract void UpdateAlignment(T component, LanguageDirection direction);

        public void OnLocalize(LocalizationModel localization) {

#if UNITY_EDITOR
            var flags = localizedComponent.hideFlags;
            localizedComponent.hideFlags = HideFlags.DontSave;
#endif
            string value = localization.Get(key);
            if (parameters.Count > 0) {
                value = string.Format(value, parameters);
            }
            SetText(localizedComponent, value);

            var direction = localization.selectedLanguageDirection;

            if (!maintainTextAlignment) {
                UpdateAlignment(localizedComponent, direction);
            }

#if UNITY_EDITOR
            localizedComponent.hideFlags = flags;
#endif
        }

        public void ClearParameters() {

            parameters.Clear();
        }

    }
}
