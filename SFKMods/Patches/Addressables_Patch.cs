using HarmonyLib;
using SuperFantasyKingdom;
using System;
using UnityEngine;

namespace ModItems
{
    // Creates/returns a hidden template Item that AddressablesManager can hand back.
    static class ModItemTemplates
    {
        static readonly System.Collections.Generic.Dictionary<string, Item> _cache = new();

        public static Item GetOrCreateTemplate(string id)
        {
            if (_cache.TryGetValue(id, out var cached) && cached) return cached;

            // Find ANY Item prefab/object to use as a base template (loaded in memory)
            var bases = Resources.FindObjectsOfTypeAll<Item>();
            if (bases == null || bases.Length == 0)
            {
                Debug.LogWarning("[ModItems] No base Item found in memory; cannot create template.");
                return null;
            }
            var baseItem = bases[0];

            // Clone into a hidden, prefab-like object
            var go = UnityEngine.Object.Instantiate(baseItem.gameObject);
            go.name = $"ModItemTemplate:{id}";
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);

            var item = go.GetComponent<Item>();
            var mod = go.GetComponent<ModItemBehaviour>();
            if (mod) UnityEngine.Object.DestroyImmediate(mod);
            mod = go.AddComponent<ModItemBehaviour>();
            if (ModItemRegistry.TryGet(id, out var def)) mod.Def = def;

            _cache[id] = item;
            return item;
        }
    }

    // AddressablesPatch.cs (update this piece)
    [HarmonyPatch(typeof(AddressablesManager), nameof(AddressablesManager.GetItem))]
    static class Addressables_GetItem_Mod
    {
        static readonly System.Reflection.FieldInfo _cooldownFI =
            AccessTools.Field(typeof(AttackAbilityBase), "cooldown"); // base of Item

        [HarmonyPrefix]
        static bool Prefix(string prefabName, ref Item __result)
        {
            if (!string.IsNullOrEmpty(prefabName) &&
                prefabName.StartsWith("mod:", StringComparison.OrdinalIgnoreCase))
            {
                var tmpl = ModItemTemplates.GetOrCreateTemplate(prefabName);
                if (tmpl != null)
                {
                    // Attach / refresh ModItemBehaviour on the TEMPLATE
                    var beh = tmpl.GetComponent<ModItems.ModItemBehaviour>()
                              ?? tmpl.gameObject.AddComponent<ModItems.ModItemBehaviour>();

                    if (ModItemRegistry.TryGet(prefabName, out var def))
                    {
                        beh.Def = def;

                        // IMPORTANT: set cooldown BEFORE Item.Init() is called by ItemManager.LoadItem(...)
                        if (_cooldownFI != null) _cooldownFI.SetValue(tmpl, def.cooldown);
                    }

                    __result = tmpl; // Let ItemManager.Instantiate<Item>(tmpl) clone a ready-to-go item
                    return false;    // handled
                }
            }
            return true;
        }
    }
}
