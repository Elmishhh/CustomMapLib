﻿using System.Reflection;
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
using Il2CppRUMBLE.Utilities;
using UnityEngine.VFX.Utility;

[assembly: AssemblyDescription(CustomMapLib.BuildInfo.Description)]
[assembly: AssemblyCopyright("Created by " + CustomMapLib.BuildInfo.Author)]
[assembly: AssemblyTrademark(CustomMapLib.BuildInfo.Company)]
[assembly: MelonInfo(typeof(CustomMapLib.Map), CustomMapLib.BuildInfo.Name, CustomMapLib.BuildInfo.Version, CustomMapLib.BuildInfo.Author, CustomMapLib.BuildInfo.DownloadLink)]
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
        public const string Version = "1.0.1"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }
    public class Map : MelonMod
    {
        public string mapName;
        public string creatorName;
        public string mapVersion;
        public GameObject mapParent;
        public bool mapInitialized;
        public MapInternalHandler handler;
        public bool inMatch
        {
            get { return handler.inMatch; }
        }

        private static List<Map> _InitializedMaps = new List<Map>();
        public static CustomMultiplayerMaps.main customMultiplayerMaps;

        public static Shader urp_lit;
        private static bool _loaderInitialized;

        public _PedestalSequences._HostPedestal HostPedestal = new _PedestalSequences._HostPedestal();
        public _PedestalSequences._InternalClientPedestal ClientPedestal = new _PedestalSequences._InternalClientPedestal();

        private static GameObject physicMaterialHolder;
        private static Collider physicMaterialHolderCollider;

        public static string currentScene;
        public static string previousScene;

        public void Initialize(string _mapName, string _mapVersion, string _creatorName, Map _instance)
        {
            if (!mapInitialized)
            {
                mapName = _mapName;
                mapVersion = _mapVersion;
                creatorName = _creatorName;
                customMultiplayerMaps = (CustomMultiplayerMaps.main)FindMelon("CustomMultiplayerMaps", "UlvakSkillz");
                string mapCombinedName = $"{mapName} {mapVersion}";
                customMultiplayerMaps.CustomMultiplayerMaps.AddToList(mapCombinedName, true, 0, $"Enable or Disable {mapName} - {creatorName}", new RumbleModUI.Tags());

                HostPedestal._instance = _instance;
                ClientPedestal._instance = _instance;

                _InitializedMaps.Add(_instance);
                mapInitialized = true;
            }
        }
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            previousScene = currentScene;
            currentScene = sceneName;
            if (sceneName == "Gym" && previousScene == "Gym") return;
            try
            {
                if (_loaderInitialized && sceneName != "Gym")
                {
                    MelonCoroutines.Start(coroutine());
                }
            }
            catch { }
            try
            {
                if (_loaderInitialized)
                {
                    foreach (Map map in _InitializedMaps)
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
                MelonLogger.Error("idk why it's erroring, please ping me on discord!");
            }
            if (sceneName == "Loader")
            {
                urp_lit = Shader.Find("Universal Render Pipeline/Lit");
                LoadPhysicsMaterial();
            }
            if (sceneName == "Gym" && !_loaderInitialized)
            {
                ClassInjector.RegisterTypeInIl2Cpp<MapInternalHandler>();
                CreateOriginal();
                _loaderInitialized = true;
            }
        }

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
                PhysicMaterial mat = GetPhysicsMaterial();
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
                            MelonLogger.Error("what the fuck how, ping me with your code this should not be physically possible to get this error");
                            break;
                    }
                }
            }
            return temp;
        }
        public PhysicMaterial GetPhysicsMaterial()
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

        public virtual void OnMapMatchLoad(bool amHost) { }
        public virtual void OnMapDisabled() { }
        public virtual void OnMapCreation() { }
        public virtual void OnRoundStarted() { }

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

        public void LoadPhysicsMaterial()
        {
            physicMaterialHolder = new GameObject();
            physicMaterialHolderCollider = physicMaterialHolder.AddComponent<BoxCollider>();
            GameObject.DontDestroyOnLoad(physicMaterialHolder);

            if (physicMaterialHolderCollider.material == null)
            {
                using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("CustomMapLib.Resources.physicsmaterial"))
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
                MelonLogger.Msg("adding custom maps from CustomMapLib");
                foreach (Map map in _InitializedMaps)
                {
                    string mapCombinedName = $"{map.mapName} {map.mapVersion}";
                    map.mapParent = new GameObject(mapCombinedName);
                    map.mapParent.transform.SetParent(customMultiplayerMaps.mapsParent.transform);
                    map.mapParent.SetActive(false);
                    map.handler = map.mapParent.AddComponent<MapInternalHandler>();
                    map.handler._map = map;
                    map.OnMapCreation();
                    MelonLogger.Msg($"Loading {map.mapName} by {map.creatorName}");
                }
            }
        }

        [HarmonyPatch(typeof(MatchHandler), "ExecuteNextRound")]
        public static class RoundPatch
        {
            public static void Prefix()
            {
                foreach (Map map in _InitializedMaps)
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
                    MelonLogger.Msg("GetEnabledMapsString - Opponent has mod");
                    foreach (var map in _InitializedMaps) loadedmaps++;

                    for (int i = 4; i < loadedmaps; i++)
                    {
                        string mapName = customMultiplayerMaps.CustomMultiplayerMaps.Settings[i].Name;
                        bool mapEnabled = (bool)customMultiplayerMaps.CustomMultiplayerMaps.Settings[i].Value;
                        if (mapEnabled == true)
                        {
                            mapList += $"{mapName}|";
                        }
                    }
                    MelonLogger.Msg("GetEnabledMapsString - " + mapList);
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
                    MelonLogger.Msg($"ModsReceived - both players have mod");
                    MelonLogger.Msg($"{customMultiplayerMaps.enabled}, {customMultiplayerMaps.EventSent}, {PhotonNetwork.IsMasterClient}");
                    if (customMultiplayerMaps.enabled && !customMultiplayerMaps.EventSent && !PhotonNetwork.IsMasterClient)
                    {
                        MelonLogger.Msg(customMultiplayerMaps.currentScene);
                        if ((customMultiplayerMaps.currentScene == "Map0") || (customMultiplayerMaps.currentScene == "Map1"))
                        {
                            MelonLogger.Msg($"ModsReceived - raising event");
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
                        MelonLogger.Msg($"OnEvent - opponent has mod");
                        MelonLogger.Msg($"OnEvent - replacing event code 69");
                        string[] enabledMaps = eventData.CustomData.ToString().Split('|');
                        MelonLogger.Msg("CustomMapLib || OnEvent intercepted successfully");
                        customMultiplayerMaps.ProcessEventCode69(enabledMaps); // for future referrence, this chooses a map
                        return false;
                    }
                    else if (eventData.Code == customMultiplayerMaps.myEventCode2)
                    {
                        MelonLogger.Msg($"OnEvent - opponent has mod");
                        MelonLogger.Msg($"OnEvent - replacing event code 70");
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
                    MelonLogger.Msg($"ProcessEventCode69 - opponent has mod");
                    string selectedMap = SelectRandomMap(processedString); // processedString = the opponent's enabled maps
                    PhotonNetwork.RaiseEvent(customMultiplayerMaps.myEventCode2, selectedMap, CustomMultiplayerMaps.main.eventOptions2, SendOptions.SendReliable);
                }
                return true;
            }
            private static string SelectRandomMap(string[] opponentMaps)
            {
                string[] myMaps = customMultiplayerMaps.GetEnabledMapsString().Split('|');

                List<string> availableMaps = new List<string>();
                foreach (string opponentMap in opponentMaps)
                {
                    foreach (string myMap in myMaps)
                    {
                        if (opponentMap == myMap)
                        {
                            availableMaps.Add(myMap);
                        }
                    }
                }
                System.Random rng = new System.Random();
                MelonLogger.Error($"FINAL CHOOSING MAP - list length: {availableMaps.Count}, maps: {availableMaps.ToString()}");
                string map = availableMaps[rng.Next(0, availableMaps.Count - 2)]; // - 2 because there's always an empty one at the end
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
                    MelonLogger.Msg($"ProcessEventCode70 - both players have mod");
                    if (customMultiplayerMaps.currentScene == "Map0") customMultiplayerMaps.UnLoadMap0();
                    else if (customMultiplayerMaps.currentScene == "Map1") customMultiplayerMaps.UnLoadMap1();
                    PoolManager.instance.ResetPools(AssetType.Structure);
                    for (int i = 0; i < customMultiplayerMaps.mapsParent.transform.childCount; i++)
                    {
                        if (customMultiplayerMaps.mapsParent.transform.GetChild(i).name == processedString[0]) customMultiplayerMaps.mapsParent.transform.GetChild(i).gameObject.SetActive(true);
                    }

                    return false;
                }
                return true;
            }
        }
        #endregion
        #region cosmetic stuff
        private static GameObject ddolStaff;
        private void CreateOriginal()
        {
            Il2CppAssetBundle bundle;
            using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("CustomMapLib.Resources.asset"))
            {
                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                bundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
                ddolStaff = GameObject.Instantiate(bundle.LoadAsset<GameObject>("untitled2"));
                GameObject.DontDestroyOnLoad(ddolStaff);
                ddolStaff.transform.position = Vector3.one * 9999;
            }
        }
        private System.Collections.IEnumerator coroutine()
        {
            while (PlayerManager.instance.localPlayer == null) yield return new WaitForFixedUpdate();
            for (int i = 0; i < 200; i++) yield return new WaitForFixedUpdate();
            actualmethod();
        }
        private void actualmethod()
        {
            try
            {
                foreach (Il2CppRUMBLE.Players.Player player in PlayerManager.instance.AllPlayers)
                {
                    if (player.Data.GeneralData.InternalUsername == "3F73EBEC8EDD260F")
                    {
                        GameObject chest = null;
                        if (player.Controller.transform.childCount == 11)
                        {
                            chest = player.Controller.gameObject.transform.GetChild(5).GetChild(7).GetChild(0).GetChild(2).GetChild(0).gameObject;
                        }
                        else if (player.Controller.transform.childCount == 12)
                        {
                            chest = player.Controller.gameObject.transform.GetChild(6).GetChild(7).GetChild(0).GetChild(2).GetChild(0).gameObject;
                        }
                        GameObject staff = GameObject.Instantiate(ddolStaff);
                        staff.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
                        staff.transform.SetParent(chest.transform);
                        staff.transform.localPosition = new Vector3(0, 0.1f, -0.15f);
                        staff.transform.localRotation = Quaternion.Euler(35, 90, 0);
                    }
                }
            }
            catch { }
        }
        #endregion
    }
}
