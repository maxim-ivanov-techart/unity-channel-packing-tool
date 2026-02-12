using UnityEditor;
using UnityEngine;
using System.IO;

public class ChannelPackerWindow : EditorWindow
{
    private enum TextureChannel
    {
        R,G,B,A
    }
    
    private Texture2D _textureR;
    private Texture2D _textureG;
    private Texture2D _textureB;
    private Texture2D _textureA;
    
    private TextureChannel _channelR = TextureChannel.R;
    private TextureChannel _channelG = TextureChannel.R;
    private TextureChannel _channelB = TextureChannel.R;
    private TextureChannel _channelA = TextureChannel.R;

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
        
        EditorGUILayout.BeginHorizontal();
        DrawChannelSlot("R", ref _textureR, ref _channelR, new Color(1f, 0.3f, 0.3f));
        DrawChannelSlot("G", ref _textureG, ref _channelG, new Color(0.3f, 1f, 0.3f));
        DrawChannelSlot("B", ref _textureB, ref _channelB, new Color(0.3f, 0.6f, 1f));
        DrawChannelSlot("A", ref _textureA, ref _channelA, Color.white);
        EditorGUILayout.EndHorizontal();
        
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

    /// <summary>
    /// Packs selected texture channels into a single RGBA texture and saves it as PNG.
    /// Enable read/write settings automatically and restores them after packing.
    /// </summary>
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
            float r = pixelsR != null ? GetChannel(pixelsR[i], _channelR) : 0f;
            float g = pixelsG != null ? GetChannel(pixelsG[i], _channelG) : 0f;
            float b = pixelsB != null ? GetChannel(pixelsB[i], _channelB) : 0f;
            float a = pixelsA != null ? GetChannel(pixelsA[i], _channelA) : 0f;;
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

    /// <summary>
    /// Enables or disables read/write setting for a texture and reimports it.
    /// </summary>
    /// <param name="texture">Target texture to modify</param>
    /// <param name="readable">Read/write state</param>
    /// <returns>Original read/write state before modification</returns>
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
    
    /// <summary>
    /// Validates that all assigned textures have matching dimensions.
    /// </summary>
    /// <returns>Error message if sizes mismatch, null if valid</returns>
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

    /// <summary>
    /// Extracts a specific color channel value from a Color struct.
    /// </summary>
    /// <param name="color">Source color</param>
    /// <param name="channel">Channel to extract (R, G, B, or A)</param>
    /// <returns>Channel value (0-1 range)</returns>
    private float GetChannel(Color color, TextureChannel channel)
    {
        switch (channel)
        {
            case TextureChannel.R: return color.r;
            case TextureChannel.G: return color.g;
            case TextureChannel.B: return color.b;
            case TextureChannel.A: return color.a;
            default: return 0f;
        }
    }

    /// <summary>
    /// Draws a single channel slot UI with texture preview, label, and channel selector.
    /// </summary>
    /// <param name="label">Channel name to display</param>
    /// <param name="texture">Reference to the texture</param>
    /// <param name="channel">Reference to the channel selection</param>
    /// <param name="labelColor">Color for the channel label</param>
    private void DrawChannelSlot(string label, ref Texture2D texture, ref TextureChannel channel, Color labelColor)
    {
        float blockWidth = (position.width - 20) / 4;
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(blockWidth));
        float previewSize = blockWidth - 10;
        texture = (Texture2D)EditorGUI.ObjectField(
            GUILayoutUtility.GetRect(previewSize, previewSize),
            texture,
            typeof(Texture2D),
            false
        );
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
        labelStyle.normal.textColor = labelColor;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label(label, labelStyle);
        channel = (TextureChannel)EditorGUILayout.EnumPopup(channel);
        EditorGUILayout.EndVertical();
    }
}
