# Assetbundles
assetbundles are used to load custom models in unity games during runtime <br />
same goes with modding, we use assetbundles to load custom models that you otherwise cant

## Creating Assetbundles

1: download unity hub from https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe (link from the official unity website) <br />
2: after you download unity hub, download unity version 2022.1.13f1 from https://unity.com/releases/editor/archive <br />
3: hope the universe doesn't end before unity finishes downloading <br />
4: create a new project in unity `2022.1.13f1` with the `Universtal 3D` template <br />
![URP option](https://imgur.com/rzWlMkS.png) <br />
5: in your unity project, create a new folder inside the `Assets` folder and name it `Editor` <br />
6: in the Editor folder create a new c# script and call it "CreateAssetBundles" <br />
7: open the c# script and delete everything inside, after that copy the following code and add it in there <br />
```cs
using UnityEngine;
using UnityEditor;
using System.IO;
 
public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }
}
```
8: go back to your unity project and in the top left hover over `edit` and select `Project Settings...` at the bottom <br />
9: in the new window that opened, go to the bottom left and install XR Plugin Management <br />
10: select `OpenXR` in the plugin providers and allow unity to restart the editor after it's done (unity will tell you when it's done) <br />
11: in Interaction Profiles add `Oculus Touch Controller Profile` and `Valve Index Controller Profile` <br />
![interaction profiles plus button location](https://imgur.com/dBjauSy.png) <br />
![what profiles to add](https://imgur.com/YYGPIgH.png) <br />
12: your project is now fully set up, when you want to build an assetbundle just add your selected prefabs to the same bundle and click on `Build AssetBundles` in the `assets` tab at the top left <br />


# loading Assetbundles

## loading from memory (user friendly way)
much nicer on mod users to download this way

1: create a new folder in your project <br />
2: add your assetbundle to your folder <br />
3: right click the assetbundle in visual studio and click properties <br />
4: set `Build Action` to `Embedded resource` <br />
5: create a new method in your mod, we'll call it "LoadAsset" for now (name does not matter) <br />
6: add the following code inside your method <br />
```cs
using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("ProjectName.FolderName.AssetbundleName"))
{
    byte[] bundleBytes = new byte[bundleStream.Length];
    bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
    Il2CppAssetBundle bundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
    var asset = GameObject.Instantiate(bundle.LoadAsset<GameObject>("AssetName");
}
```
(note, if you're getting errors from `Il2CppAssetBundle` make sure you're on melonloader 0.6.5 or higher and reference `UnityEngine.Il2cppAssetbundleManager` from the net6 folder) <br />
7: change `ProjectName.FolderName.AssetbundleName` to your path, example: "CustomMapLib.Resources.asset" <br />
![example image](https://imgur.com/Q0D0GYE.png) <br />
8: replace "Assetname" when to what your asset is called in unity when you bundled it, also replace <GameObject> to whatever asset you're loading if needed <br />
9: store the asset in a variable and place it in DontDestroyOnLoad <br />



### loading from folder (if you want the user to be able to edit the asset)
less preferrable than loading from memory if the asset is always consistent but better if it can be changed

1: create a new method in your code, we'll call it "LoadAsset" again (or use an existing method) <br />
2: add `Il2CppAssetBundle bundle = Il2CppAssetBundleManager.LoadFromFile("path goes here");` <br />
3: edit the path, example: "UserData/MyAssets/asset" <br />
4: load the asset with `var asset = GameObject.Instantiate(bundle.LoadAsset<GameObject>("AssetName");` (same as before) <br />