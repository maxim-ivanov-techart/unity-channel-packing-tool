using UnityEditor;
using UnityEngine;
using System.IO;

public class ChannelPackerWindow : EditorWindow
{
    private Texture2D _textureR;
    private Texture2D _textureG;
    private Texture2D _textureB;
    private Texture2D _textureA;

    [MenuItem("Tools/Channel Packer")]
    public static void OpenWindow()
    {
        ChannelPackerWindow window = GetWindow<ChannelPackerWindow>();
        window.titleContent = new GUIContent("Channel Packer");
    }

    private void OnGUI()
    {
        GUILayout.Label("RGBA Channel Packer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        _textureR = (Texture2D)EditorGUILayout.ObjectField("R Channel", _textureR, typeof(Texture2D), false);
        _textureG = (Texture2D)EditorGUILayout.ObjectField("G Channel", _textureG, typeof(Texture2D), false);
        _textureB = (Texture2D)EditorGUILayout.ObjectField("B Channel", _textureB, typeof(Texture2D), false);
        _textureA = (Texture2D)EditorGUILayout.ObjectField("A Channel", _textureA, typeof(Texture2D), false);
        EditorGUILayout.Space();
        string sizeError = GetSizeError();
        if (sizeError  != null)
        {
            EditorGUILayout.HelpBox(sizeError, MessageType.Error);
        }
        
        GUI.enabled = sizeError == null;
        
        if (GUILayout.Button("Pack"))
        {
            Pack();
        }
        
        GUI.enabled = true;
    }

    private void Pack()
    {
        Texture2D referenceTexture = _textureR ?? _textureG ?? _textureB ?? _textureA;
        if (referenceTexture == null)
        {
            Debug.LogWarning("Channel Packer: add at least one texture");
        }
        
        string path = EditorUtility.SaveFilePanelInProject(
            "Save", "pack", "png", "Choose where to save");

        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        
        Texture2D[] textures = { _textureR, _textureG, _textureB, _textureA };
        bool[] wasReadable = new bool[4];

        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] != null)
            {
                wasReadable[i] = SetTextureReadable(textures[i], true);
            }
        }
        
        int width = referenceTexture.width;
        int height = referenceTexture.height;
        
        Color[] pixelsR = _textureR != null ? _textureR.GetPixels() : null;
        Color[] pixelsG = _textureG != null ? _textureG.GetPixels() : null;
        Color[] pixelsB = _textureB != null ? _textureB.GetPixels() : null;
        Color[] pixelsA = _textureA != null ? _textureA.GetPixels() : null;
        
        Texture2D resultTexture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] resultColors = new Color[width * height];

        for (int i = 0; i < resultColors.Length; i++)
        {
            float r = pixelsR != null ? pixelsR[i].r : 0.0f;
            float g = pixelsG != null ? pixelsG[i].g : 0.0f;
            float b = pixelsB != null ? pixelsB[i].b : 0.0f;
            float a = pixelsA != null ? pixelsA[i].a : 0.0f;
            resultColors[i] = new Color(r, g, b, a);
        }
        resultTexture2D.SetPixels(resultColors);
        resultTexture2D.Apply();
        
        byte[] bytes = resultTexture2D.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();

        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] != null && !wasReadable[i])
            {
                SetTextureReadable(textures[i], false);
            }
        }
        
        Debug.Log("Channel Packer: Saved in" + path);
    }

    private bool SetTextureReadable(Texture2D texture, bool readable)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
        {
            return false;
        }

        bool wasReadable = importer.isReadable;
        if (wasReadable == readable)
        {
            return wasReadable;
        }

        importer.isReadable = readable;
        importer.SaveAndReimport();
        return wasReadable;
    }
    
    private string GetSizeError()
    {
        Texture2D[] textures = { _textureR, _textureG, _textureB, _textureA };
        Texture2D referenceTexture = null;
        foreach (Texture2D texture in textures)
        {
            if (texture != null)
            {
                referenceTexture = texture;
                break;
            }
        }

        if (referenceTexture == null)
        {
            return null;
        }

        foreach (Texture2D texture in textures)
        {
            if (texture == null)
            {
                continue;
            }

            if (texture.width != referenceTexture.width || texture.height != referenceTexture.height)
            {
                return
                    $"The sizes don't match: {referenceTexture.width}x{referenceTexture.height} and {texture.width}x{texture.height}";
            }
        }
        return null;
    }
}
