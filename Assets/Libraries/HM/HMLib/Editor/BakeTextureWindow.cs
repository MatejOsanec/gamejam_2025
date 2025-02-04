using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[AddToToolDirectory(
    displayName: "Texture baker",
    description: "Bakes the result of a material into a texture.",
    packageName: "HMLib package",
    maintainer: ToolMaintainer.TechArt,
    openToolFunctionName: nameof(ShowWindow),
    labelTypes: LabelType.Tours | LabelType.TechArt)]
public class BakeTextureWindow : EditorWindow {

    private Material _imageMaterial;
    private string _filePath = "Assets/Libraries/FlipbookGenerator/Output.png";
    private Vector2Int _resolution;

    private bool _hasMaterial;
    private bool _hasResolution;
    private bool _hasFilePath;

    [MenuItem("Tools/Bake Material To Texture")]
    private static void ShowWindow() {

        var window = GetWindow<BakeTextureWindow>();
        window.titleContent = new GUIContent("Texture Baker");
        window.Show();
        window.CheckInput();
    }

    protected void OnGUI() {

        EditorGUILayout.HelpBox("Make sure the source textures (if any) have sRGB turned off and have no compression", MessageType.None);

        using (var check = new EditorGUI.ChangeCheckScope()) {
            _imageMaterial = (Material)EditorGUILayout.ObjectField("Material", _imageMaterial, typeof(Material), false);
            _resolution = EditorGUILayout.Vector2IntField("Image Resolution", _resolution);
            _filePath = FileField(_filePath);

            if (check.changed) {
                CheckInput();
            }
        }

        GUI.enabled = _hasMaterial && _hasResolution && _hasFilePath;
        if (GUILayout.Button("Bake")) {
            BakeTexture();
        }
        GUI.enabled = true;

        //tell the user what inputs are missing
        if (!_hasMaterial) {
            EditorGUILayout.HelpBox("You're still missing a material to bake.", MessageType.Warning);
        }
        if (!_hasResolution) {
            EditorGUILayout.HelpBox("Please set a size bigger than zero.", MessageType.Warning);
        }
        if (!_hasFilePath) {
            EditorGUILayout.HelpBox("No file to save the image to given.", MessageType.Warning);
        }
    }

    private void CheckInput() {

        _hasMaterial = _imageMaterial != null;
        _hasResolution = _resolution.x > 0 && _resolution.y > 0;
        _hasFilePath = false;

        try {
            string ext = Path.GetExtension(_filePath);
            _hasFilePath = ext.Equals(".png");
        } catch(ArgumentException) {}
    }

    private string FileField(string path) {

        EditorGUILayout.LabelField("Image Path");

        using(new GUILayout.HorizontalScope()) {

            path = EditorGUILayout.TextField(path);

            if (GUILayout.Button("choose")) {

                string directory = "Assets/Libraries/FlipbookGenerator/";
                string fileName = "Output.png";

                try {
                    directory = Path.GetDirectoryName(path);
                    fileName = Path.GetFileName(path);
                } catch(ArgumentException) {}

                string chosenFile = EditorUtility.SaveFilePanelInProject("Choose image file", fileName, "png", "Please enter a file name to save the image to", directory);
                if (!string.IsNullOrEmpty(chosenFile)) {
                    path = chosenFile;
                }

                Repaint();
            }
        }

        return path;
    }

    private void BakeTexture() {

        RenderTexture renderTexture = RenderTexture.GetTemporary(_resolution.x, _resolution.y);
        Graphics.Blit(null, renderTexture, _imageMaterial);

        Texture2D texture = new Texture2D(_resolution.x, _resolution.y);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(Vector2.zero, _resolution), 0, 0);

        byte[] png = texture.EncodeToPNG();
        File.WriteAllBytes(_filePath, png);
        AssetDatabase.Refresh();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);
        DestroyImmediate(texture);
    }
}
