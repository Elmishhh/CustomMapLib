# Project setup

## part 1: visual studio

first of all, you must have visual studio for this, <br />
I use visual studio 2022 community, so I'll cover it here.

### Installation

1: install the visual studio installer from https://visualstudio.microsoft.com/vs/ and select 2022 community <br />
2: in visual studio installer, select `.NET desktop development` and `Game development with Unity` <br />
3: finish install of visual studio


## part 2: project setup

1: create a new `Class Library` project and select `.NET 6.0 (Long Term Support)` as your Framework, if that's not an option please install .NET 6 from https://dotnet.microsoft.com/en-us/download/dotnet/6.0 <br />
2: find the solution explorer on your screen, it should look like this <br />
![this](https://imgur.com/y1MTxhG.png) <br />
3: right click Dependencies and click on `Add Project Reference...`, a new window should open <br />
4: in the bottom right, click on `Browse...` and add: <br />
> 1: from "RUMBLE\Mods" adds "CustomMapLib", "ModUI", "CustomMultiplayerMaps" and "RumbleModdingAPI" <br />
> 2: from "RUMBLE\MelonLoader\net6" add everything (excluding the runtime folder) <br />
> 3: from "RUMBLE\MelonLoader\Il2CppAssemblies" add everything with "Il2Cpp" and "Unity" in the name

5: add this at the start of your code <br />

```cs
using CustomMapLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ExampleCustomMap.Class1), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author, BuildInfo.DownloadLink)]
[assembly: MelonGame(null, null)]
```

**make sure you replace `ExampleCustomMap.Class1` with your namespace and class name!** <br />
**if you're getting an error with `BuildInfo`, add `using BuildInfo = <myNamespace>.BuildInfo;` at the start of your code (and ofc replace <myNamespace> with your namespace**

6: add this in your namespace 

```cs
public static class BuildInfo
{
    public const string Name = "add map name here";
    public const string Description = "add map description here";
    public const string Author = "add map author here";
    public const string Company = null; // not important
    public const string Version = "1.0.0"; // change when updating your map to avoid 2 players matching with different versions!
    public const string DownloadLink = null; // not important
}
```

7: add an inheritence for your main class to `Map` (i might be using the wrong term, dont kill me please) <br />
8: Override the `OnLateInitializeMelon()` method and add `Initialize(BuildInfo.Name, BuildInfo.Version, BuildInfo.Author, this);` <br />
9: you should be done now and your project should look like this, if you have any errors please ping me on the rumble modding discord or rumble mapping discord (@elmishh) <br />
![example image](https://imgur.com/q4GWOSh.png)

