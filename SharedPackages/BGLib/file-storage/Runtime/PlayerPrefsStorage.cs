using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

#if UNITY_PS4
using UnityEngine.PS4;
#elif UNITY_PS5
using UnityEngine.PS5;
#endif

public class PlayerPrefsStorage  {

    public static void Initialize() {

#if UNITY_PS4 && !UNITY_EDITOR
        PS4PlayerPrefs.userId = Utility.initialUserId;
        PS4PlayerPrefs.SetTitleStrings("Beat Saber", "SaveFile Data", string.Empty);
#elif UNITY_PS5 && !UNITY_EDITOR
        PS5PlayerPrefs.userId = Utility.initialUserId;
        PS5PlayerPrefs.SetTitleStrings("Beat Saber", "SaveFile Data", string.Empty);
#endif
    }

    public bool LoadFile(string filePath, out string value) {

        if (PlayerPrefs.HasKey(filePath)) {
            value = PlayerPrefs.GetString(filePath);
            return true;
        }

        value = string.Empty;
        return false;
    }

    public bool FileExists(string filePath) {

        return PlayerPrefs.HasKey(filePath);
    } 
}
