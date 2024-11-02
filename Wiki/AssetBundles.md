# Assetbundles
assetbundles are used to load custom models in unity games during runtime <br />
same goes with modding, we use assetbundles to load custom models that you otherwise cant

## Creating Assetbundles

1: download unity hub from https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe (link from the official unity website) <br />
2: after you download unity hub, download unity version 2022.1.13f1 from unityhub://2022.1.13f1/22856944e6d2 (link from the official unity archive website) <br />
3: hope the universe doesn't end before unity finishes downloading <br />
4: create a new project in unity `2022.1.13f1` with the `Universtal 3D` template <br />
![URP option](https://imgur.com/rzWlMkS.png) <br />
5: create a new c# script in unity, call it "CreateAssetBundles" and open it with visual studio 2022 <br />
6: delete everything inside the c# file you just created and paste this code instead <br />
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
7: go back to your unity project and in the top left hover over `edit` and select `Project Settings...` at the bottom <br />
8: in the new window that opened, go to the bottom left and install XR Plugin Management <br />
9: select `OpenXR` in the plugin providers and allow unity to restart the editor after it's done (unity will tell you when it's done) <br />
10: in Interaction Profiles add `Oculus Touch Controller Profile` and `Valve Index Controller Profile` <br />
![interaction profiles plus button location](https://imgur.com/dBjauSy.png) <br />
![what profiles to add](https://imgur.com/YYGPIgH.png) <br />
11: your project is now fully set up, when you want to build an assetbundle just add your selected prefabs to the same bundle and click on `Build AssetBundles` in the `assets` tab at the top left <br />


## loading Assetbundles
