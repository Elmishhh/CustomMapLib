using Il2CppSystem.Xml.Serialization;
using MelonLoader;
using System.Reflection;
using BuildInfo = ProBendingArena.BuildInfo;
using CustomMapLib;
using UnityEngine;
using System.Reflection.Metadata;
using Il2CppPhoton.Compression;
using System.Runtime.Serialization;
using Il2CppRUMBLE.MoveSystem;
using Il2Cpp;

[assembly: AssemblyDescription(BuildInfo.Description)]
[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: AssemblyTrademark(BuildInfo.Company)]
[assembly: MelonInfo(typeof(ProBendingArena.ProBending), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author, BuildInfo.DownloadLink)]
[assembly: MelonGame(null, null)]

namespace ProBendingArena
{
    public static class BuildInfo // all the info of the map and stuff
    {
        public const string Name = "Pro Bending Arena"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "please work"; // Description for the Mod.  (Set as null if none)
        public const string Author = "elmish"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }
    public class ProBending : Map // the actual map
    {
        public override void OnLateInitializeMelon() => Initialize(BuildInfo.Name, BuildInfo.Version, BuildInfo.Author, this); // initializes the map, required

        GameObject arena;
        public override void OnMapCreation()
        {
            Il2CppAssetBundle bundle = LoadBundle("ProBendingArena.Resources.probending"); // loads the bundle from the local path
            arena = GameObject.Instantiate(bundle.LoadAsset<GameObject>("untitled124356")); // loads the object by name, yes i was lazy to give it a normal name
            arena.transform.SetParent(mapParent.transform);
            arena.transform.localScale = Vector3.one * 0.5f;
            GetChildRecursive(arena); // probably useless, read further note at the method

            /*
            I AM SETTING UP COLLIDERS MANUALLY HERE, NOT RECOMMENDED, USE A MESHCOLLIDER INSTEAD
            this is done because i wanted to customize the collisions a bit
            a meshcollider would be much simpler to use (and more efficient as having a structure on multiple ground colliders causes some issues)
            */

            GameObject temp1 = CreatePrimitiveObject(PrimitiveType.Cube, new Vector3(0, -0.5f, 0), Quaternion.identity, new Vector3(6.5f, 1, 16.61f), ObjectType.CombatFloor);

            GameObject temp2 = CreatePrimitiveObject(PrimitiveType.Cube, new Vector3(-2.5207f, -0.5f, 3.2537f), Quaternion.identity, new Vector3(5, 1, 8.9f), ObjectType.CombatFloor);
            temp2.transform.localRotation = Quaternion.Euler(0f, 21.1f, 0);
            GameObject temp3 = CreatePrimitiveObject(PrimitiveType.Cube, new Vector3(2.5229f, -0.5f, -3.2511f), Quaternion.identity, new Vector3(5, 1, 8.9f), ObjectType.CombatFloor);
            temp3.transform.localRotation = Quaternion.Euler(0f, 21.1f, 0);

            GameObject temp4 = CreatePrimitiveObject(PrimitiveType.Cube, new Vector3(-2.5215f, -0.5f, -3.252f), Quaternion.identity, new Vector3(5, 1, 8.9f), ObjectType.CombatFloor);
            temp4.transform.localRotation = Quaternion.Euler(0, 338.9f, 0);
            GameObject temp5 = CreatePrimitiveObject(PrimitiveType.Cube, new Vector3(2.52f, -0.5f, 3.2551f), Quaternion.identity, new Vector3(5, 1, 8.9f), ObjectType.CombatFloor);
            temp5.transform.localRotation = Quaternion.Euler(0, 338.9f, 0);

            temp1.GetComponent<MeshRenderer>().enabled = false; // makes colliders invisible
            temp2.GetComponent<MeshRenderer>().enabled = false;
            temp3.GetComponent<MeshRenderer>().enabled = false;
            temp4.GetComponent<MeshRenderer>().enabled = false;
            temp5.GetComponent<MeshRenderer>().enabled = false;

            PrimitivePhysicsMaterial Bouncy = new PrimitivePhysicsMaterial() { Bounciness = 1000000, options = Options.Bouncy };
            GameObject temp6 = CreatePrimitiveObject(PrimitiveType.Cube, new Vector3(4.75f, 0.25f, -4.15f), Quaternion.identity, new Vector3(0.2f, 0.5f, 9), ObjectType.Wall, Bouncy);
            temp6.transform.localRotation = Quaternion.Euler(0, 21, 0);
            GameObject temp7 = CreatePrimitiveObject(PrimitiveType.Cube, new Vector3(4.75f, 0.25f, 4.15f), Quaternion.identity, new Vector3(0.2f, 0.5f, 9), ObjectType.Wall, Bouncy);
            temp7.transform.localRotation = Quaternion.Euler(0, -21, 0);
            GameObject temp8 = CreatePrimitiveObject(PrimitiveType.Cube, new Vector3(-4.75f, 0.25f, -4.15f), Quaternion.identity, new Vector3(0.2f, 0.5f, 9), ObjectType.Wall, Bouncy);
            temp8.transform.localRotation = Quaternion.Euler(0, -21, 0);
            GameObject temp9 = CreatePrimitiveObject(PrimitiveType.Cube, new Vector3(-4.75f, 0.25f, 4.15f), Quaternion.identity, new Vector3(0.2f, 0.5f, 9), ObjectType.Wall, Bouncy);
            temp9.transform.localRotation = Quaternion.Euler(0, 21, 0);

            temp6.GetComponent<MeshRenderer>().enabled = false; // makes colliders invisible again
            temp7.GetComponent<MeshRenderer>().enabled = false;
            temp8.GetComponent<MeshRenderer>().enabled = false;
            temp9.GetComponent<MeshRenderer>().enabled = false;

            mapParent.transform.localScale = Vector3.one * 2; // doubles the scale of everything because it was too small
        }
        public Il2CppAssetBundle LoadBundle(string path) // loads bundle from memory path
        {
            using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream(path))
            {
                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                return Il2CppAssetBundleManager.LoadFromMemory(bundleBytes); 
            }
        }
        public void GetChildRecursive(GameObject obj) // this is not needed for shit, in fact, this shouldnt even be here and will probably get removed when i have time to make a new release and test it
        {
            if (obj.transform.childCount > 0)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    GetChildRecursive(obj.transform.GetChild(i).gameObject);
                }
            }
            MeshCollider collider = obj.AddComponent<MeshCollider>();
            obj.AddComponent<GroundCollider>().collider = collider;
        }
    }
}
