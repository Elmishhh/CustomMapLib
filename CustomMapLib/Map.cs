using System.Reflection;
using HarmonyLib;
using Il2CppRUMBLE.Players;
using Il2CppSystem.Xml.Serialization;
using JetBrains.Annotations;
using MelonLoader;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using CustomMapLib.Components;
using Il2CppRUMBLE;
using Il2CppRUMBLE.MoveSystem;
using Il2CppPhoton.Pun;
using Il2CppRUMBLE.Environment;
using Il2CppSteamworks;
using RumbleModUI;
using Il2CppRUMBLE.Pools;
using Il2CppPhoton.Realtime;
using Il2CppSystem.Collections;
using System.Collections;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players.Subsystems;
using RMAPI = RumbleModdingAPI.Calls;
using Il2CppExitGames.Client.Photon;
using Il2Cpp;
using Il2CppRUMBLE.Networking.MatchFlow;
using AssetsTools.NET;
using Il2CppRUMBLE.Environment.MatchFlow;
using System.Runtime.CompilerServices;
using RC = RumbleModdingAPI.Calls.ControllerMap.RightController;
using LC = RumbleModdingAPI.Calls.ControllerMap.LeftController;
using Il2CppRUMBLE.Utilities;
using UnityEngine.VFX.Utility;
using System.ComponentModel.Design;
using Il2CppTMPro;
using static CustomMapLib.CustomMapLib;
using UnityEngine.UIElements;

[assembly: AssemblyDescription(CustomMapLib.BuildInfo.Description)]
[assembly: AssemblyCopyright("Created by " + CustomMapLib.BuildInfo.Author)]
[assembly: AssemblyTrademark(CustomMapLib.BuildInfo.Company)]
[assembly: MelonInfo(typeof(CustomMapLib.CustomMapLib), CustomMapLib.BuildInfo.Name, CustomMapLib.BuildInfo.Version, CustomMapLib.BuildInfo.Author, CustomMapLib.BuildInfo.DownloadLink)]
[assembly: MelonColor(255, 255, 0, 0)]
[assembly: MelonGame(null, null)]

namespace CustomMapLib
{
    public static class BuildInfo
    {
        public const string Name = "CustomMapLib"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "allows you to make more complex custom maps"; // Description for the Mod.  (Set as null if none)
        public const string Author = "elmish"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.1.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }
    public class CustomMapLib : MelonMod
    {
        public static CustomMapLib? instance;

        public static List<Map> InitializedMaps = new List<Map>();
        public static CustomMultiplayerMaps.main? customMultiplayerMaps;

        public static Shader? urp_lit;
        public static bool mapLoaderInitialized;

        public static GameObject? physicMaterialHolder;
        public static Collider? physicMaterialHolderCollider;

        public static string? currentScene;
        public static string? previousScene;

        public static RaiseEventOptions eventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };

        public override void OnLateInitializeMelon() => instance = this;

        public void OnEvent(EventData eventData)
        {
            if (eventData.Code == 17)
            {
                string[] splitData = eventData.CustomData.ToString().Split('|');
                string sender = splitData[0];
                if (sender == "CustomMapLib")
                {
                    string request = splitData[1];
                    switch (request)
                    {
                        case "SnapBack":
                            instancedCosmeticHandler.ChangeMode(StaffHandler.holdingMode.Back, false);
                            break;
                        case "SnapHand":
                            instancedCosmeticHandler.ChangeMode(StaffHandler.holdingMode.SingleHand, false);
                            break;
                        case "SnapDoubleHand":
                            instancedCosmeticHandler.ChangeMode(StaffHandler.holdingMode.DoubleHand, false);
                            break;
                        default:
                            MelonLogger.Msg($"invalid CustomMapLib raiseEvent: {request}");
                            break;
                    }
                }
            }
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            previousScene = currentScene;
            currentScene = sceneName;
            if (sceneName == "Gym" && previousScene == "Gym") return; // stop repeat scene loading
            try
            {
                if (mapLoaderInitialized)
                {
                    foreach (Map map in InitializedMaps)
                    {
                        if (map.handler != null)
                        {
                            map.handler.inMatch = false;
                        }
                    }
                }
            }
            catch
            {
                //MelonLogger.Error("idk why it's erroring, please ping me on discord!");
            }
            if (sceneName == "Loader")
            {
                urp_lit = Shader.Find("Universal Render Pipeline/Lit");
                LoadPhysicsMaterial();
                CreateOriginalStaff();
            }
            else if (sceneName == "Gym" && !mapLoaderInitialized)
            {
                PhotonNetwork.NetworkingClient.EventReceived += (Action<EventData>)OnEvent;
                ClassInjector.RegisterTypeInIl2Cpp<MapInternalHandler>();
                mapLoaderInitialized = true;
            }
        }
        public static PhysicMaterial GetPhysicsMaterial()
        {
            try
            {
                return GameObject.Instantiate(physicMaterialHolderCollider.material);
            }
            catch
            {
                MelonLogger.Msg("Physics material is null, please ping @elmishh on discord with your log file");
                LoadPhysicsMaterial();
            }
            return null;
        }
        public static void LoadPhysicsMaterial()
        {
            physicMaterialHolder = new GameObject();
            physicMaterialHolderCollider = physicMaterialHolder.AddComponent<BoxCollider>();
            GameObject.DontDestroyOnLoad(physicMaterialHolder);

            if (physicMaterialHolderCollider.material == null)
            {
                using (System.IO.Stream bundleStream = instance.MelonAssembly.Assembly.GetManifestResourceStream("CustomMapLib.Resources.physicsmaterial"))
                {
                    byte[] bundleBytes = new byte[bundleStream.Length];
                    bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                    Il2CppAssetBundle bundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
                    physicMaterialHolderCollider.material = GameObject.Instantiate(bundle.LoadAsset<PhysicMaterial>("baseMaterial"));
                }
            }
        }

        #region harmony patch hell
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "PreLoadMaps")]
        public static class MapLoadPatch
        {
            public static void Postfix()
            {
                MelonLogger.Msg("[CustomMapLib - PreLoadMaps]: adding custom maps from CustomMapLib");
                foreach (Map map in InitializedMaps)
                {
                    try
                    {
                        string mapCombinedName = $"{map.mapName} {map.mapVersion}";
                        map.mapParent = new GameObject(mapCombinedName);
                        map.mapParent.transform.SetParent(customMultiplayerMaps.mapsParent.transform);
                        map.mapParent.SetActive(false);
                        map.handler = map.mapParent.AddComponent<MapInternalHandler>();
                        map.handler._map = map;
                        map.OnMapCreation();
                        MelonLogger.Msg($"[CustomMapLib - PreLoadMaps]: Loading {map.mapName} by {map.creatorName}");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"[CustomMapLib - PreLoadMaps]: Fatal error occured when loading {map.mapName}, Version {map.mapVersion}, game will now quit, please ping @elmishh with your log file immediately");
                        MelonLogger.Error(ex);
                        Application.Quit();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MatchHandler), "ExecuteNextRound")]
        public static class RoundPatch
        {
            public static void Prefix()
            {
                foreach (Map map in InitializedMaps)
                {
                    if (map.handler.inMatch)
                    {
                        map.OnRoundStarted();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "GetEnabledMapsString")]
        public static class MapListPatch
        {
            public static bool Prefix(ref string __result)
            {
                MelonLogger.Msg("GetEnabledMapsString was called");
                int loadedmaps = 13;
                string mapList = "";
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    MelonLogger.Msg("[CustomMapLib - GetEnabledMapsString]: Opponent has mod");
                    foreach (var map in InitializedMaps) loadedmaps++;

                    for (int i = 4; i < loadedmaps; i++)
                    {
                        string mapName = customMultiplayerMaps.CustomMultiplayerMaps.Settings[i].Name;
                        bool mapEnabled = (bool)customMultiplayerMaps.CustomMultiplayerMaps.Settings[i].Value;
                        if (mapEnabled == true)
                        {
                            mapList += $"{mapName}|";
                        }
                    }
                    __result = mapList;
                    return false;
                }
                MelonLogger.Msg("opponent does not have mod");
                return true;
            }
        }
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "ModsReceived")]
        public static class Patch2
        {
            public static bool Prefix()
            {
                MelonLogger.Msg($"ModsReceived was called");
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    MelonLogger.Msg($"[CustomMapLib - ModsReceived]: both players have mod");
                    MelonLogger.Msg($"{customMultiplayerMaps.enabled}, {customMultiplayerMaps.EventSent}, {PhotonNetwork.IsMasterClient}");
                    if (customMultiplayerMaps.enabled && !customMultiplayerMaps.EventSent && !PhotonNetwork.IsMasterClient)
                    {
                        MelonLogger.Msg(customMultiplayerMaps.currentScene);
                        if ((customMultiplayerMaps.currentScene == "Map0") || (customMultiplayerMaps.currentScene == "Map1"))
                        {
                            MelonLogger.Msg($"[CustomMapLib - ModsReceived]: raising event with code 69");
                            PhotonNetwork.RaiseEvent(customMultiplayerMaps.myEventCode, customMultiplayerMaps.GetEnabledMapsString(), CustomMultiplayerMaps.main.eventOptions, SendOptions.SendReliable);
                        }
                    }
                    return false;
                }
                MelonLogger.Msg($"ModsReceived - opponent does not have mod");
                return true;
            }
        }
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "OnEvent")]
        public static class NetworkEventRecievedPatch
        {
            public static bool Prefix(ref EventData eventData)
            {
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    if (eventData.Code == customMultiplayerMaps.myEventCode)
                    {
                        MelonLogger.Msg($"[CustomMapLib - OnEvent 69]: opponent has mod");
                        string[] enabledMaps = eventData.CustomData.ToString().Split('|');
                        MelonLogger.Msg("CustomMapLib || OnEvent intercepted successfully");
                        customMultiplayerMaps.ProcessEventCode69(enabledMaps); // for future referrence, this chooses a map
                        return false;
                    }
                    else if (eventData.Code == customMultiplayerMaps.myEventCode2)
                    {
                        MelonLogger.Msg($"[CustomMapLib - OnEvent 70]: opponent has mod");
                        string[] availableMaps = new string[] { eventData.CustomData.ToString() };
                        customMultiplayerMaps.ProcessEventCode70(availableMaps);
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "ProcessEventCode69")]
        public static class ProcessPatch
        {
            public static bool Prefix(ref string[] processedString)
            {
                MelonLogger.Msg($"ProcessEventCode69 called");
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    MelonLogger.Msg($"[CustomMapLib - ProcessEventCode69 override]: opponent has mod");
                    string selectedMap = SelectRandomMap(processedString); // processedString = the opponent's enabled maps
                    PhotonNetwork.RaiseEvent(customMultiplayerMaps.myEventCode2, selectedMap, CustomMultiplayerMaps.main.eventOptions2, SendOptions.SendReliable);
                }
                return true;
            }
            private static string SelectRandomMap(string[] opponentMaps)
            {
                MelonLogger.Msg(1);
                string[] localMaps = customMultiplayerMaps.GetEnabledMapsString().Split('|');
                MelonLogger.Msg(2);

                string debug_localMaps = "[";
                string debug_opponentMaps = "[";

                MelonLogger.Msg(3);
                for (int i = 0; i < localMaps.Length - 1; i++)
                {
                    MelonLogger.Msg(3.1);
                    debug_localMaps += localMaps[i];
                    MelonLogger.Msg(3.2);
                    if (i != localMaps.Length - 1)
                    {
                        MelonLogger.Msg(3.3);
                        debug_localMaps += ", ";
                    }
                    MelonLogger.Msg(3.4);
                }
                MelonLogger.Msg(4);

                for (int i = 0; i < opponentMaps.Length - 1; i++)
                {
                    MelonLogger.Msg(4.1);
                    debug_opponentMaps += opponentMaps[i];
                    MelonLogger.Msg(4.2);
                    if (i != opponentMaps.Length - 1)
                    {
                        MelonLogger.Msg(4.3);
                        debug_opponentMaps += ", ";
                    }
                    MelonLogger.Msg(4.4);
                }
                MelonLogger.Msg(5);

                debug_localMaps += "]";
                debug_opponentMaps += "]";

                MelonLogger.Msg($"[CustomMapLib - SelectRandomMap]: opponent maps: {debug_opponentMaps}");
                MelonLogger.Msg($"[CustomMapLib - SelectRandomMap]: local maps: {debug_opponentMaps}");

                MelonLogger.Msg(6);
                List<string> mutualMaps = new List<string>();
                foreach (string opponentMap in opponentMaps)
                {
                    MelonLogger.Msg(7);
                    foreach (string myMap in localMaps)
                    {
                        MelonLogger.Msg(8);
                        if (opponentMap == myMap)
                        {
                            MelonLogger.Msg(9);
                            mutualMaps.Add(myMap);
                            MelonLogger.Msg(10);
                        }
                    }
                    MelonLogger.Msg(11);
                }

                MelonLogger.Msg(12);
                if (mutualMaps.Count == 1)
                {
                    MelonLogger.Warning($"[CustomMapLib - SelectRandomMap]: no mutual maps found, disabling. {mutualMaps[0]}");
                    return "NO_MUTUAL_MAPS";
                }

                string debug_mutualMaps = "[";

                MelonLogger.Msg(13);
                for (int i = 0; i < mutualMaps.Count - 1; i++)
                {
                    MelonLogger.Msg(14);
                    debug_mutualMaps += localMaps[i];
                    MelonLogger.Msg(15);
                    if (i != mutualMaps.Count - 1)
                    {
                        MelonLogger.Msg(16);
                        debug_mutualMaps += ", ";
                        MelonLogger.Msg(17);
                    }
                    MelonLogger.Msg(18);
                }
                debug_mutualMaps += "]";
                MelonLogger.Msg(19);

                System.Random rng = new System.Random();
                MelonLogger.Error($"[CustomMapLib - SelectRandomMap]: choosing map - list length: {mutualMaps.Count}, maps: {debug_mutualMaps}, selected map index: {rng}");
                MelonLogger.Msg(20);
                string map = mutualMaps[rng.Next(0, mutualMaps.Count - 2)]; // - 2 because there's always an empty one at the end
                MelonLogger.Msg(21);
                MelonLogger.Msg($"map selected: {map}");
                return map;
            }
        }
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "ProcessEventCode70")]
        public static class SecondProcessPatch
        {
            public static bool Prefix(ref string[] processedString)
            {
                MelonLogger.Msg($"ProcessEventCode70 called");
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    if (processedString[0] == "NO_MUTUAL_MAPS") return false;
                    bool mapFound = false;
                    MelonLogger.Msg($"[CustomMapLib - ProcessEventCode70 override]: both players have mod");
                    MelonLogger.Warning($"[CustomMapLib - ProcessEventCode70 override]: looking for map with name: {processedString[0]}");
                    for (int i = 0; i < customMultiplayerMaps.mapsParent.transform.childCount; i++)
                    {
                        if (customMultiplayerMaps.mapsParent.transform.GetChild(i).name == processedString[0])
                        {
                            customMultiplayerMaps.mapsParent.transform.GetChild(i).gameObject.SetActive(true);
                            if (customMultiplayerMaps.currentScene == "Map0") customMultiplayerMaps.UnLoadMap0();
                            else if (customMultiplayerMaps.currentScene == "Map1") customMultiplayerMaps.UnLoadMap1();
                            PoolManager.instance.ResetPools(AssetType.Structure);
                            mapFound = true;
                            MelonLogger.Msg($"[CustomMapLib - ProcessEventCode70 override]: loading {processedString[0]}"); // note for tomorrow: this is a bandaid fix for maps not being found and the normal maps getting de-loaded, also add a check if the mutual maps are empty!
                        }
                    }
                    if (!mapFound)
                    {
                        MelonLogger.Error("[CustomMapLib - ProcessEventCode70 override]: Map not found, please dm or ping @elmishh if this happens with a log file");
                    }
                    return false;
                }
                return true;
            }
        }
        #endregion
        #region cosmetic stuff
        public static GameObject? cosmetic;
        public static GameObject? instancedCosmetic;
        public static StaffHandler instancedCosmeticHandler;
        public static GameObject? instancedChest;
        public static GameObject? instancedRightHand;
        public static GameObject? instancedLeftHand;
        private void CreateOriginalStaff()
        {
            Il2CppAssetBundle bundle;
            using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("CustomMapLib.Resources.asset"))
            {
                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                bundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
                cosmetic = GameObject.Instantiate(bundle.LoadAsset<GameObject>("untitled2"));
                GameObject.DontDestroyOnLoad(cosmetic);
                cosmetic.transform.position = Vector3.one * 9999;
            }
        }

        [HarmonyPatch(typeof(PlayerController), "Initialize", new Type[] { typeof(Il2CppRUMBLE.Players.Player) })]
        public static class PlayerSpawnPatch
        {
            private static void Postfix(ref PlayerController __instance, ref Il2CppRUMBLE.Players.Player player)
            {
                if (__instance.assignedPlayer.Data.GeneralData.PlayFabMasterId == "3F73EBEC8EDD260F")
                {
                    MelonCoroutines.Start(delayedMethod(__instance));
                }
            }
            public static System.Collections.IEnumerator delayedMethod(PlayerController __instance)
            {
                yield return new WaitForSeconds(2);

                instancedChest = __instance.gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(4).GetChild(0).gameObject;
                instancedCosmetic = GameObject.Instantiate(cosmetic);
                instancedCosmetic.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
                instancedCosmetic.transform.SetParent(instancedChest.transform);
                instancedCosmetic.transform.localPosition = new Vector3(0, 0.1f, -0.15f);
                instancedCosmetic.transform.localRotation = Quaternion.Euler(35, 90, 0);
                instancedCosmeticHandler = instancedCosmetic.AddComponent<StaffHandler>();
                instancedCosmeticHandler.isLocal = __instance.controllerType == Il2CppRUMBLE.Players.ControllerType.Local;

                instancedLeftHand = __instance.transform.GetChild(1).GetChild(1).gameObject;
                instancedRightHand = __instance.transform.GetChild(1).GetChild(2).gameObject;
            }
        }
        #endregion
    }
    public class Map : MelonMod
    {
        public string mapName;
        public string creatorName;
        public string mapVersion;
        public GameObject mapParent;
        public bool mapInitialized;
        public MapInternalHandler handler;

        public _PedestalSequences._HostPedestal HostPedestal = new _PedestalSequences._HostPedestal();
        public _PedestalSequences._InternalClientPedestal ClientPedestal = new _PedestalSequences._InternalClientPedestal();

        public bool inMatch
        {
            get { return handler.inMatch; }
        }
        public void Initialize(string _mapName, string _mapVersion, string _creatorName)
        {
            if (!mapInitialized)
            {
                mapName = _mapName;
                mapVersion = _mapVersion;
                creatorName = _creatorName;
                customMultiplayerMaps = (CustomMultiplayerMaps.main)FindMelon("CustomMultiplayerMaps", "UlvakSkillz");
                string mapCombinedName = $"{mapName} {mapVersion}";
                customMultiplayerMaps.CustomMultiplayerMaps.AddToList(mapCombinedName, true, 0, $"Enable or Disable {mapName} - {creatorName}", new RumbleModUI.Tags());

                HostPedestal._instance = this;
                ClientPedestal._instance = this;

                CustomMapLib.InitializedMaps.Add(this);
                mapInitialized = true;
                customMultiplayerMaps.CustomMultiplayerMaps.GetFromFile();
            }
        }
        [Obsolete("new version does not need 'this' in the end, please update to it.")] // allow older maps to work without needing an update
        public void Initialize(string _mapName, string _mapVersion, string _creatorName, Map _instance) => Initialize(_mapName, _mapVersion, _creatorName);
        public GameObject CreatePrimitiveObject(PrimitiveType primitiveType, Vector3 position, Quaternion rotation, Vector3 scale, ObjectType type, PrimitivePhysicsMaterial primitivePhysicsMaterial = null)
        {
            GameObject temp = GameObject.CreatePrimitive(primitiveType);
            temp.transform.position = position;
            temp.transform.rotation = rotation;
            temp.transform.localScale = scale;
            temp.GetComponent<Renderer>().material.shader = urp_lit;
            Collider col = temp.GetComponent<Collider>();
            if (type != ObjectType.Wall)
            {
                GameObject.Destroy(col);
                col = temp.AddComponent<MeshCollider>();
                temp.layer = (int)type;
            }
            else
            {
                temp.layer = 11;
            }
            GroundCollider groundcol = temp.AddComponent<GroundCollider>();
            groundcol.isMainGroundCollider = true;
            groundcol.collider = col;
            temp.transform.SetParent(mapParent.transform);
            if (primitivePhysicsMaterial != null)
            {
                PhysicMaterial mat = CustomMapLib.GetPhysicsMaterial();
                if (mat != null)
                {
                    col.material = mat;
                    switch (primitivePhysicsMaterial.options)
                    {
                        case Options.Bouncy:
                            col.material.bounciness = primitivePhysicsMaterial.Bounciness;
                            col.material.bouncyness = primitivePhysicsMaterial.Bounciness;
                            col.material.bounceCombine = PhysicMaterialCombine.Maximum;
                            break;

                        case Options.Friction:
                            col.material.dynamicFriction = primitivePhysicsMaterial.Friction;
                            col.material.dynamicFriction2 = primitivePhysicsMaterial.Friction;
                            col.material.staticFriction = primitivePhysicsMaterial.Friction;
                            col.material.staticFriction2 = primitivePhysicsMaterial.Friction;
                            col.material.frictionCombine = PhysicMaterialCombine.Minimum;
                            break;

                        case Options.Both:
                            col.material.bounciness = primitivePhysicsMaterial.Bounciness;
                            col.material.bouncyness = primitivePhysicsMaterial.Bounciness;

                            col.material.dynamicFriction = primitivePhysicsMaterial.Friction;
                            col.material.dynamicFriction2 = primitivePhysicsMaterial.Friction;
                            col.material.staticFriction = primitivePhysicsMaterial.Friction;
                            col.material.staticFriction2 = primitivePhysicsMaterial.Friction;
                            col.material.frictionCombine = PhysicMaterialCombine.Minimum;
                            break;

                        default:
                            MelonLogger.Error("what the fuck did you do? did you forget to set options?");
                            break;
                    }
                }
            }
            return temp;

        }

        #region enums and dummy classes
        public enum ObjectType
        {
            CombatFloor = 9,
            NonCombatFloor = 11,
            Wall = 1
        }
        public class PrimitivePhysicsMaterial
        {
            public float Bounciness = 0;
            public float Friction = 0;

            public Options options;
        }
        public enum Options
        {
            Bouncy = 0,
            Friction = 1,
            Both = 2
        };

        public class _PedestalSequences
        {
            public class _InternalClientPedestal
            {
                public Map _instance;
                public void SetFirstSequence(Vector3 newPosition)
                {
                    _instance.handler.ClientPedestalSequence1 = newPosition;
                }
                public void SetSecondSequence(Vector3 newPosition)
                {
                    _instance.handler.ClientPedestalSequence2 = newPosition;
                }
            }
            public class _HostPedestal
            {
                public Map _instance;
                public void SetFirstSequence(Vector3 newPosition)
                {
                    _instance.handler.HostPedestalSequence1 = newPosition;
                }
                public void SetSecondSequence(Vector3 newPosition)
                {
                    _instance.handler.HostPedestalSequence2 = newPosition;
                }
            }
        }

        #endregion

        public virtual void OnMapMatchLoad(bool amHost) { }
        public virtual void OnMapDisabled() { }
        public virtual void OnMapCreation() { }
        public virtual void OnRoundStarted() { }
    }

    [RegisterTypeInIl2Cpp]
    public class StaffHandler : MonoBehaviour
    {
        public bool isLocal;

        public GameObject rightHandHold;
        public GameObject leftHandHold;

        public bool RCGrip;
        public bool LCGrip;

        public enum holdingMode
        {
            Back = 0,
            SingleHand = 1,
            DoubleHand = 2,
        };

        public holdingMode currentMode = holdingMode.Back;

        public Vector3 handOffset = new Vector3(0.015f, -0.1f, 0.012f);
        public Quaternion rotationOffset = Quaternion.Euler(15, 0, 0);
        public Quaternion doubleHandRotationOffset = Quaternion.Euler(-90, 0, 0);

        public void ChangeMode(holdingMode newMode, bool doRaiseEvent)
        {
            currentMode = newMode;
            if (newMode == holdingMode.Back)
            {
                transform.SetParent(CustomMapLib.instancedChest.transform);
                transform.localPosition = new Vector3(0, 0.1f, -0.15f);
                transform.localRotation = Quaternion.Euler(35, 90, 0);

                if (doRaiseEvent)
                {
                    PhotonNetwork.RaiseEvent(17, "CustomMapLib|SnapBack", CustomMapLib.eventOptions, SendOptions.SendReliable);
                }
            }
            else if (newMode == holdingMode.SingleHand)
            {
                transform.SetParent(instancedRightHand.transform);
                transform.localPosition = handOffset;
                transform.localRotation = rotationOffset;

                if (doRaiseEvent)
                {
                    PhotonNetwork.RaiseEvent(17, "CustomMapLib|SnapHand", CustomMapLib.eventOptions, SendOptions.SendReliable);
                }
            }
            else if (newMode == holdingMode.DoubleHand)
            {
                if (doRaiseEvent)
                {
                    PhotonNetwork.RaiseEvent(17, "CustomMapLib|SnapDoubleHand", CustomMapLib.eventOptions, SendOptions.SendReliable);
                }
            }
        }

        public void Start()
        {
            rightHandHold = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightHandHold.transform.localScale = Vector3.one * 0.1f;
            rightHandHold.transform.SetParent(transform);
            rightHandHold.transform.localPosition = Vector3.zero + transform.forward * -12.5f + transform.up * 12.5f;
            //rightHandHold.GetComponent<Renderer>().material.shader = CustomMapLib.urp_lit;
            rightHandHold.GetComponent<Renderer>().enabled = false;

            leftHandHold = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftHandHold.transform.localScale = Vector3.one * 0.1f;
            leftHandHold.transform.SetParent(transform);
            leftHandHold.transform.localPosition = Vector3.zero + transform.forward * 15f + transform.up * -15f;
            //leftHandHold.GetComponent<Renderer>().material.shader = CustomMapLib.urp_lit;
            leftHandHold.GetComponent<Renderer>().enabled = false;
        }

        public void FixedUpdate()
        {
            RCGrip = RC.GetGrip() > 0.5f;
            LCGrip = LC.GetGrip() > 0.5f;
            if (currentMode == holdingMode.Back)
            {
                if (RCGrip && Vector3.Distance(rightHandHold.transform.position, instancedRightHand.transform.position) < 0.2 && isLocal)
                {
                    ChangeMode(holdingMode.SingleHand, true);
                }
            }
            else if (currentMode == holdingMode.SingleHand)
            {
                if (LCGrip && Vector3.Distance(leftHandHold.transform.position, instancedLeftHand.transform.position) < 0.2 && isLocal)
                {
                    ChangeMode(holdingMode.DoubleHand, true);
                }
                else if (!RCGrip && isLocal)
                {
                    ChangeMode(holdingMode.Back, true);
                }
            }
            else if (currentMode == holdingMode.DoubleHand)
            {
                Quaternion rotation = LookAt(instancedLeftHand.transform.position, instancedRightHand.transform.position) * doubleHandRotationOffset;
                Vector3 position = (instancedLeftHand.transform.position + instancedRightHand.transform.position * 3) / 4;

                transform.rotation = rotation;
                transform.position = position;

                if (!LCGrip && isLocal)
                {
                    ChangeMode(holdingMode.SingleHand, true);
                }
                else if (!RCGrip && isLocal)
                {
                    ChangeMode(holdingMode.Back, true);
                }
            }
        }
        public Quaternion LookAt(Vector3 objectPosition, Vector3 lookAtPosition)
        {
            Vector3 targetDir = objectPosition - lookAtPosition;
            Quaternion lookDir = Quaternion.LookRotation(targetDir);
            return lookDir;
        }
    }
}
