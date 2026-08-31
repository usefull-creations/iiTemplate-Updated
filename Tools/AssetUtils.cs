using System.IO;
using System.Reflection;
using UnityEngine;

namespace StupidTemplate.Tools;

// This class is for loading assets from embedded resources
// Make sure all assets in the assets folder are marked as an embedded resource!
// You can do this by editing the .csproj or right-clicking on it, pressing properties and making the build action 'Embedded Resource'
public static class StupidAssetUtils
{
    // Make sure this matches the name of the project/csproj file!
    private const string ProjectName = "StupidTemplate";

    public static Texture2D LoadTexture2D(string imageName)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{ProjectName}.Assets.{imageName}");

        if (stream == null)
            return null;

        byte[] imageData = new byte[stream.Length];
        stream.Read(imageData, 0, imageData.Length);
        Texture2D texture = new(2, 2);
        texture.LoadImage(imageData);

        return texture;
    }

    public static AssetBundle LoadAssetBundle(string bundleName) =>
            AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    $"{ProjectName}.Assets.{bundleName}"));
}