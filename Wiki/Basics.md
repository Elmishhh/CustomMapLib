# Basics

## Method Overrides 
a few more Overridable methods are added when inheriting from `Map` instead of `MelonMod` (keep in mind Map still has all of the features from MelonMod)

> 1: OnMapMatchLoad(bool amHost) <br />
> happens when the maps is chosen and enabled during a match

> 2: OnMapDisabled() <br />
> happens after a match when the map is disabled

> 4: OnRoundStarted() <br />
> happens right before a new round starts (pedestals go down)

> 3: OnMapCreation() <br />
> happens once when loading into the gym for the first time, please put your map creation code here

## Inherited and static variables

### Inherited variables
a few variables are accessible when inheriting from Map
you can either access them with their name like a normal variable or with base.<variableName>

> 1: (GameObject) mapParent <br />
> this is the main parent of your map, you must parent any object you create to this

> 2: (string) mapName, creatorName and mapVersion <br />
> very simple, just the map name, map creator name and the map version (do not edit)

> 3: (bool) mapInitialized <br />
> sets to true after map is initialized, nothing important and no real reason to touch it (do not edit)

> 4: HostPedestal and ClientPedestal <br />
> used to set where the client and host pedestals move to at the start of every round <br />
> call Host/ClientPedestal.SetFirstSequence(Vector3 newPosition) to set the first sequence <br />
> call Host/ClientPedestal.SetSecondSequence(Vector3 newPosition) to set the second sequence

### static variables
a few QoL variables that can be accessed with Map.<variableName>

> 1: urp_lit <br />
> the standard "Universal Render Pipeline/Lit" unity shader, use when needed

> 2: GetPhysicsMaterial <br />
> creates a new UnityEngine.PhysicMaterial because they have to be assetbundle loaded in, refer to [this](https://docs.unity3d.com/560/Documentation/Manual/class-PhysicMaterial.html) for a guilde on them 

### Misc methods and variables
general methods and variables, could be useful could be not depending on what you're doing

> method: (GameObject) CreatePrimitiveObject(PrimitiveType primitiveType, Vector3 position, Quaternion rotation, Vector3 scale, ObjectType type, SpecialState specials = null) <br />
> the method itself creates a primitive, sets the layer, sets the parent and deals with combatfloor stuff <br />
> it's not the simplest thing but it also isn't really that complicated <br />
> here's an explanation for everything <br />
> 1: primitiveType, comes from unity's Primitive objects, it's the kind of object that you're creating (cube, sphere, cylinder, capsule, plane and quad) <br />
> 2: position, it's literally just the position, nothing special to explain <br />
> 3: rotation, again it's just rotation, if you're using a vector for rotation do Quaternion.Euler(vector3) to get the quaternion <br />
> 4: scale, not much to explain here, size of your object <br />
> 5: ObjectType, either a CombatFloor (can do moves on), NonCombatFloor (cannot do moves on) or Wall (doesn't use mesh collider) <br />
> 6: SpecialState, doesn't have to be set, but if it is you can decide if you want a floor or wall bouncy or <br />
> example of a bouncy and slippery SpecialState <br />
![example](https://imgur.com/JXYkzm1.png) <br />
> **keep in mind friction only applies to ungrounded structures, also bounciness doesn't always count as a ground touch to the player**
