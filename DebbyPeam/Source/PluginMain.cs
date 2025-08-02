using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using HarmonyLib;
using DebbyPeam.Config;
using DebbyPeam.Patches;
using PEAKLib.Core;
using PEAKLib.Items;
using DebbyPeam.Utils;
namespace DebbyPeam
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(CorePlugin.Id, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(ItemsPlugin.Id, BepInDependency.DependencyFlags.HardDependency)]
    public class DebbyPeam : BaseUnityPlugin
    {
        internal const string modGUID = "deB.DebbyPeam";
        internal const string modName = "Debby Peam";
        internal const string modVersion = "0.1.0";
        readonly Harmony harmony = new Harmony(modGUID);
        internal ManualLogSource log = null!;
        internal ModDefinition modDefinition = null!;
        public DebbyPeamUtils utils;
        public static DebbyPeam instance;
        public List<Item> itemsList = new List<Item>();
        public List<GameObject> miscPrefabsList = new List<GameObject>();
        internal DebbyPeamConfig ModConfig { get; private set; } = null!;
        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            modDefinition = ModDefinition.GetOrCreate(Info.Metadata);
            log = Logger;
            utils = new DebbyPeamUtils();
            Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                MethodInfo[] typeMethods = assemblyTypes[i].GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                for (int j = 0; j < typeMethods.Length; j++)
                {
                    object[] methodAttributes;
                    try
                    {
                        methodAttributes = typeMethods[j].GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    }
                    catch
                    {
                        continue;
                    }
                    if (methodAttributes.Length > 0)
                    {
                        typeMethods[j].Invoke(null, null);
                    }
                }
            }
            PropogateLists();
            HandleContent();
            DoPatches();
            log.LogInfo("Debby Peam Successfully Loaded");
        }
        public void PropogateLists()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "debbypeam"));
            string[] allAssetPaths = bundle.GetAllAssetNames();
            for (int i = 0; i < allAssetPaths.Length; i++)
            {
                string assetPath = allAssetPaths[i][..allAssetPaths[i].LastIndexOf("/")];
                switch (assetPath)
                {
                    case "assets/debby peam/resources/items":
                        {
                            itemsList.Add(bundle.LoadAsset<GameObject>(allAssetPaths[i]).GetComponent<Item>());
                            break;
                        }
                    case "assets/debby peam/resources/misc":
                        {
                            UnityEngine.Object obj = bundle.LoadAsset(allAssetPaths[i]);
                            switch (obj)
                            {
                                case GameObject go:
                                    {
                                        miscPrefabsList.Add(go);
                                        break;
                                    }
                                default:
                                    {
                                        log.LogWarning($"\"{allAssetPaths[i]}\" was not a valid type, skipping.");
                                        break;
                                    }
                            }
                            break;
                        }
                    default:
                        {
                            log.LogWarning($"\"{assetPath}\" is not a known asset path, skipping.");
                            break;
                        }
                }
            }
        }
        public void HandleContent()
        {
            ModConfig = new DebbyPeamConfig(base.Config, itemsList);
            HandleItems();
            HandleMisc();
        }
        public void HandleItems()
        {
            for (int i = 0; i < itemsList.Count; i++)
            {
                if (!ModConfig.isItemEnabled[i].Value)
                {
                    log.LogInfo($"{itemsList[i].UIData.itemName} item was disabled!");
                    continue;
                }
                utils.FixShaders(itemsList[i].gameObject);
                new ItemContent(itemsList[i]).Register(modDefinition);
                log.LogDebug($"{itemsList[i].UIData.itemName} item was loaded!");
            }
        }
        public void HandleMisc()
        {
            for (int i = 0; i < miscPrefabsList.Count; i++)
            {
                NetworkPrefabManager.RegisterNetworkPrefab($"Misc/{miscPrefabsList[i].name}", miscPrefabsList[i]);
                log.LogDebug($"{miscPrefabsList[i].name} prefab was loaded!");
            }
        }
        public void DoPatches()
        {
            log.LogDebug("Patching Game");
            if (ModConfig.trouserRope.Value)
            {
                harmony.PatchAll(typeof(CharacterPatches));
                harmony.PatchAll(typeof(CharacterAfflictionsPatches));
                harmony.PatchAll(typeof(CharacterCustomizationPatches));
                harmony.PatchAll(typeof(RopePatches));
                harmony.PatchAll(typeof(RopeSegmentPatches));
            }
            else
            {
                log.LogInfo("Trouser Rope is disabled, skipping related patches");
            }
        }
    }
}