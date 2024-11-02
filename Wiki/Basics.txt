# Basics

## Method Overrides 
a few more Overridable methods are added when inheriting from Map instead of MelonMod (keep in mind Map still has all of the features from MelonMod)

> 1: OnMapMatchLoad(bool amHost)
> happens when the maps is chosen and enabled during a match

> 2: OnMapDisabled() 
> happens after a match when the map is disabled

> 4: OnRoundStarted()
> happens right before a new round starts (pedestals go down)

> 3: OnMapCreation()
> happens once when loading into the gym for the first time, please put your map creation code here

## Inherited and static variables

### Inherited variables
a few variables are accessible when inheriting from Map
you can either access them with their name like a normal variable or with base.<variableName>

> 1: (GameObject) mapParent
> this is the main parent of your map, you must parent any object you create to this

> 2: (string) mapName, creatorName and mapVersion
> very simple, just the map name, map creator name and the map version, dont edit these please

> 3: (bool) mapInitialized
> sets to true after map is initialized, nothing important and no real reason to touch it

> 4: HostPedestal and ClientPedestal
> used to set where the client and host pedestals move to at the start of every round
> call Host/ClientPedestal.SetFirstSequence(Vector3 newPosition) to set the first sequence
> call Host/ClientPedestal.SetSecondSequence(Vector3 newPosition) to set the second sequence

### static variables
a few QoL variables that can be accessed with Map.<variableName>

> 1: urp_lit
> the standard "Universal Render Pipeline/Lit" unity shader, use when needed

> 2: physicMaterialCollider
> holds a physic material (those have to be assetbundle loaded in) for whoever wants to use it, please refer to https://docs.unity3d.com/560/Documentation/Manual/class-PhysicMaterial.html
