using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
namespace DebbyPeam.Config
{
    public class DebbyPeamConfig
    {
        readonly BepInEx.Logging.ManualLogSource log = DebbyPeam.instance.log;
        internal readonly ConfigEntry<bool> trouserRope;
        internal readonly ConfigEntry<bool> throwableSkull;
        internal readonly List<ConfigEntry<bool>> isItemEnabled = new List<ConfigEntry<bool>>();
        internal DebbyPeamConfig(ConfigFile cfg, List<Item> itemsList)
        {
            cfg.SaveOnConfigSet = false;
            trouserRope = cfg.Bind("Miscellaneous", "Enable Trouser Ropes?", false, "When true players will spawn in with a rope attached to them");
            throwableSkull = cfg.Bind("Miscellaneous", "Enable Throwable Cursed Skull?", true, "When true, throwing a Cursed Skull at someone will use it on them");
            log.LogDebug("Added config for Trouser Rope");
            for (int i = 0; i < itemsList.Count; i++)
            {
                isItemEnabled.Add(cfg.Bind("Items", $"Enable {itemsList[i].UIData.itemName}?", true));
                log.LogDebug($"Added config for \"{itemsList[i].UIData.itemName}\"");
            }
            ClearOrphanedEntries(cfg);
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }
        private static void ClearOrphanedEntries(ConfigFile cfg)
        {
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            Dictionary<ConfigDefinition, string> orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg);
            orphanedEntries.Clear();
        }
    }
}