using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModItems
{
    public enum ModValueType { Flat, PercentAdd, PercentMult }

    [Serializable]
    public class ModStatMod
    {
        // Example: "maxShield" (short) or "UnitStats.maxShield" (full path)
        public string statPath;
        public ModValueType valueType;
        public float value;      // e.g., 0.3f = +30%
        public int origin = 9000;
    }

    [Serializable]
    public class ModItemDef
    {
        public string id;         // MUST be unique (e.g., "mod:BigShield100")
        public string title;
        public string description;
        public Sprite icon;
        public int cost = 0;
        public float cooldown = 9999999f;
        public List<ModStatMod> statMods = new();
    }

    // Attached to instantiated Items to carry the definition
    public class ModItemBehaviour : MonoBehaviour
    {
        public ModItemDef Def;
    }

    public static class ModItemRegistry
    {
        static readonly Dictionary<string, ModItemDef> _defs = new();
        public static void Register(ModItemDef def) => _defs[def.id] = def;
        public static bool TryGet(string id, out ModItemDef def) => _defs.TryGetValue(id, out def);
    }
}
