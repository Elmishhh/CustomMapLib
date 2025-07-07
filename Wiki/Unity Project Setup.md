# Things you need:
1: unity hub (https://unity.com/download) <br />
2: Unity 2022.3.18f1 (https://unity.com/releases/editor/archive) <br />
3: GitHub Desktop (https://github.com/apps/desktop) <br />

# 1: Installing
## Part 1: Installing the unity project using GitHub Desktop <br />
1: install GitHub Desktop by running the installer <br />
2: wait for everything to install <br />
3: at the top left click on 'file' and then 'Clone Repository', go to the URL tab and put in **github.com/Elmishhh/RUMBLEMapTemplate** in the URL section, then hit Clone. remember where the Local Path points to! <br />
4: wait for everything to install again <br />
![image](https://github.com/Elmishhh/CustomMapLib/blob/main/Wiki/Unity%20Project%20Setup%20Image%20Assets/1.png)
![image](https://github.com/Elmishhh/CustomMapLib/blob/main/Wiki/Unity%20Project%20Setup%20Image%20Assets/2.png)

## Part 2: Installing Unity hub and Unity 2022.3.18f1 <br />
1: Run the unity hub installer. <br />
2: Create a unity account and get a personal license. (https://www.youtube.com/watch?v=VFYol4JoQ5s) <br />
3: Head over to https://unity.com/releases/editor/archive and find '2022.3.18f1' then click 'Install'. (make sure Unity Hub is open) <br />
4: Wait for the install to finish. <br />

## Part 3: Setting up the unity project
1: In unity hub go to the 'Projects' tab and click on 'Add' at the top left, then click on 'Add project from disk'. <br />
2: Select the RUMBLEMapTemplate folder that you downloaded in the first part. <br />
3: Launch the unity project and wait for it to install everything. <br />
4: Ignore the one time error, it will not appear again and does nothing <br />
![image](https://github.com/Elmishhh/CustomMapLib/blob/main/Wiki/Unity%20Project%20Setup%20Image%20Assets/3.png)

## You should now have Unity open and ready to create a map

# 2: Creating the map and other components
> every component inside of the unity project have short explanations when you hover over them.

## Too Many Buttons! What does what??
unity has a lot of buttons and a lot of things happening and a lot of buttons, luckily for this we're only really using 2 or 3 things at best <br />
1: Hierarchy: to the middle left of your screen there will be your Hierarchy, this is where all of your objects are, by default it will have a Main Camera and Directional Light <br />
2: Inspector: to the middle right of your screen there will be your Inspector, when selecting objects more things will appear there such as position, rotation, scale, layer and all custom components <br />
3: Assets tab: to the bottom middle if your screen there will be your Assets tab, when importing assets such as FBX files (model files), textures or more you'll need to drag them into there first <br />
![image](https://github.com/Elmishhh/CustomMapLib/blob/main/Wiki/Unity%20Project%20Setup%20Image%20Assets/4.png)

## Creating the Map's Parent object 
1: right click in the Hierarchy and click on 'Create Empty' and name 'Map Parent' (name does not actually matter) <br />
2: left click on the object you created in the Hierarchy <br />
3: Click on Add Component in the Inspector <br />
4: search and add the 'Custom Map' component <br />
5: input your Map Name, Creator Name, Map Version, Map Type (currently only Match and Any are implemented) <br />
6: So many things! What do all these options do again??? <br />
> 1: Player Killbox Distance - how far from the center of your map will the player be killed <br />
> 2: Structure Killbox Distance - how far from the center of your map will structures get destroyed <br />
> 3: Offsets - (optional) moves or rotates the map when loaded - try to avoid unless you're planning on making maps for parks, matches and the gym (currently not implemented) <br />
> 4: Match Pedestal Sequence positions - how the player pedestals move at the start of rounds for the host and client <br />
>    4.1: first sequence - moves the pedestals from their starting position to the inputted position <br />
>    4.2: second sequence - moves the pedestals from the current position (AFTER FIRST SEQUENCE) by the inputted vector <br />

## Creating the map itself
1: Create a new Object in the Hierarchy <br />
> option 1: create a primitive object by pressing right click and and going to 3D Objects <br />
> option 2: using a custom mesh - drag and drop your fbx file into your assets tab and then take it from there and drag it into the Hierarchy window <br />

2: Parent the object to your Map Parent by dragging and dropping it onto the map parent object in the Hierarchy <br />
> note: make sure the map parent and the object's positions are 0,0,0, otherwise your map wont be centered in matches <br />

3: you should now see something like this: <br />
![image](https://github.com/Elmishhh/CustomMapLib/blob/main/Wiki/Unity%20Project%20Setup%20Image%20Assets/5.png)
4: click on your object to open it in the inspector <br />
5: at the top right set the layer from 'Default' to one of the following <br />
> 8: Floor - this is for floor that you can walk on but cant do moves on <br />
> 9: CombatFloor - same as floor but you can do moves on it <br />
> 10: Enviroment - used for walls or environmental objects like the region selection console, you cannot walk on this <br />
> 11: LeanableEnviroment - same as Enviroment but has a small "leaning" feature when you go into it that is barely noticeable <br />

6: make sure your object has a collider (such as Box Collider, Sphere Collider, Capsule Collider or Mesh Collider) <br />
> if your object does not have a collider or if it's a capsule or sphere, give it a Mesh Collider and remove any other colliders on it <br />

7: after you added your collider component, click on Add Component and add the Ground Collider component as well <br />
> note: you need both the GroundCollider Component and one of unity's Collider components <br />

8: drag your object from the Hierarchy onto the 'Collider' section of the 'Ground Collider' component <br />
![image](https://github.com/Elmishhh/CustomMapLib/blob/main/Wiki/Unity%20Project%20Setup%20Image%20Assets/6.png)
9: repeat for every object you need <br />
 
## Exporting the map
1: in the top part of your screen, click on 'Custom Maps' and then 'Export as Map' <br />
![image](https://github.com/Elmishhh/CustomMapLib/blob/main/Wiki/Unity%20Project%20Setup%20Image%20Assets/7.png)
2: go to your RUMBLE folder, then UserData, then CustomMapLib and export into there <br />
> note: if this folder does not appear, simply start your game and close it <br />
3: launch your game and the map should get loaded, if you want to check that the map loaded you will be able to see it in ModUI when looking at CustomMultiplayerMaps <br /> 
> note: the map object is hidden, if you want to see the map without entering a match then you can use UnityExplorer to go into DontDestroyOnLoad and turn it on manually <br /> 

## extra custom components
there are extra custom components available:

### 1: Player Damage Collider
add this to any ground or wall object to make players take damage when hitting it/standing on it (can also be used to heal) <br />
1: Damage - how much damage the player takes upon hitting the object <br />
2: Constant Damage - makes the player take damage every X amount of seconds instead of one time instantly when touching the object <br />
3: Damage Interval - how often should players touching the collider take damage <br />
> note: this only appears if Constant Damage is ticked **on** <br />
![image](https://github.com/Elmishhh/CustomMapLib/blob/main/Wiki/Unity%20Project%20Setup%20Image%20Assets/8.png)