using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

// Basically does what Photoshops color overlay would do, very useful for exported (ui) images that are not white
public class TextureModifications {

    [MenuItem("Assets/Modify Texture/Convert Texture to white only", false)]
    private static void ConvertTextureToWhiteOnly() {

        ManipulateTextureAsset(
            texture => {
                var newTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
                Color[] pixels = texture.GetPixels();
                for (int i = 0; i < pixels.Length; i++) {
                    pixels[i] = new Color(1.0f, 1.0f, 1.0f, pixels[i].a);
                }
                newTexture.SetPixels(pixels);
                newTexture.Apply();
                return newTexture;
            }
        );
    }

    [MenuItem("Assets/Modify Texture/Convert Texture to white only", true)]
    private static bool ConvertTextureToWhiteOnlyValidation() => IsSelectedAssetSupportedTextureImage();

    [MenuItem("Assets/Modify Texture/Trim Empty Space (keep center)", false)]
    private static void TrimWhiteSpace() {

        ManipulateTextureAsset(
            texture => {
                var width = texture.width;
                var height = texture.height;
                Color[] pixels = texture.GetPixels();

                int left = width, right = 0, top = height, bottom = 0;
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        if (pixels[x + y * width].a < Mathf.Epsilon) {
                            continue;
                        }
                        if (x < left) {
                            left = x;
                        }
                        if (x > right) {
                            right = x;
                        }
                        if (y < top) {
                            top = y;
                        }
                        if (y > bottom) {
                            bottom = y;
                        }
                    }
                }
                left = Mathf.Min(left, width - right) - 1;
                top = Mathf.Min(top, height - bottom) - 1;

                var newWidth = width - 2 * left;
                var newHeight = height - 2 * top;
                var newTexture = new Texture2D(newWidth, newHeight, texture.format, texture.mipmapCount > 1);
                var newPixels = new Color[newWidth * newHeight];
                for (int y = 0; y < newHeight; y++) {
                    for (int x = 0; x < newWidth; x++) {
                        newPixels[x + y * newWidth] = pixels[(x + left) + (y + top) * width];
                    }
                }
                newTexture.SetPixels(newPixels);
                newTexture.Apply();
                return newTexture;
            }
        );
    }

    [MenuItem("Assets/Modify Texture/Trim Empty Space (keep center)", true)]
    private static bool TrimWhiteSpaceValidation() => IsSelectedAssetSupportedTextureImage();

    private delegate Texture2D ManipulateTextureDelegate(Texture2D originalTexture);
    private static void ManipulateTextureAsset(ManipulateTextureDelegate manipulateTextureDelegate) {

        var texture = Selection.activeObject as Texture2D;
        if (texture == null) {
            Debug.LogError("Selected object is not a texture.");
            return;
        }
        var path = AssetDatabase.GetAssetPath(texture);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

        var wasReadable = importer.isReadable;
        if (!importer.isReadable) {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        var newTexture = manipulateTextureDelegate(texture);

        if (path.EndsWith(".png")) {
            File.WriteAllBytes(path, newTexture.EncodeToPNG());
        }
        else if (path.EndsWith(".jpg")) {
            File.WriteAllBytes(path, newTexture.EncodeToJPG());
        }
        Object.DestroyImmediate(newTexture);

        AssetDatabase.Refresh();
        if (!wasReadable) {
            importer.isReadable = false;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        Debug.Log("Texture Manipulation Finished");
    }

    private static bool IsSelectedAssetSupportedTextureImage() {

        var texture = Selection.activeObject as Texture2D;
        if (texture == null) {
            return false;
        }
        var path = AssetDatabase.GetAssetPath(texture);
        return path.EndsWith(".png") || path.EndsWith(".jpg");
    }
}
