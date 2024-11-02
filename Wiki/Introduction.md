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
2: find the solution explorer on your screen, it should look like ![](https://imgur.com/a/shGqqLT) <br />
3: right click Dependencies and click on `Add Project Reference...`, a new window should open <br />
4: in the bottom right, click on `Browse...` and add: <br />
> 1: from "RUMBLE\Mods" adds "CustomMapLib", "ModUI", "CustomMultiplayerMaps" and "RumbleModdingAPI"