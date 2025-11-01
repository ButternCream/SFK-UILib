using HarmonyLib;
using SFKMod.Mods;
using SuperFantasyKingdom;
using UnityEngine;

namespace ModItems
{
    [HarmonyPatch]
    public static class ModItemPatches
    {
        static StatModifierType ToGameType(ModValueType t) => t switch
        {
            ModValueType.Flat => StatModifierType.Flat,
            ModValueType.PercentAdd => StatModifierType.PercentAdd,
            ModValueType.PercentMult => StatModifierType.PercentMult,
            _ => StatModifierType.Flat
        };

        // Resolve a Stat by (short) name recursively on the stats aggregate
        static Stat FindStat(object statsRoot, string statPath)
        {
            if (statsRoot == null || string.IsNullOrEmpty(statPath)) return null;
            var t = statsRoot.GetType();
            const System.Reflection.BindingFlags BF = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

            // shallow first
            foreach (var f in t.GetFields(BF))
                if (f.FieldType == typeof(Stat) && f.Name == statPath)
                    return f.GetValue(statsRoot) as Stat;
            foreach (var p in t.GetProperties(BF))
                if (p.CanRead && p.GetIndexParameters().Length == 0 && p.PropertyType == typeof(Stat) && p.Name == statPath)
                    return p.GetValue(statsRoot, null) as Stat;

            // recursive by short name
            return Scan(statsRoot, statPath);

            static Stat Scan(object obj, string name)
            {
                if (obj == null) return null;
                var tt = obj.GetType();
                const System.Reflection.BindingFlags BF2 = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

                foreach (var f in tt.GetFields(BF2))
                {
                    var v = f.GetValue(obj);
                    if (v is Stat s && f.Name == name) return s;
                    if (IsPlain(v)) { var r = Scan(v, name); if (r != null) return r; }
                }
                foreach (var p in tt.GetProperties(BF2))
                {
                    if (!p.CanRead || p.GetIndexParameters().Length != 0) continue;
                    object v; try { v = p.GetValue(obj, null); } catch { continue; }
                    if (v is Stat s && p.Name == name) return s;
                    if (IsPlain(v)) { var r = Scan(v, name); if (r != null) return r; }
                }
                return null;
            }

            static bool IsPlain(object v)
            {
                if (v == null) return false;
                var vt = v.GetType();
                return !vt.IsPrimitive && vt != typeof(string) && !typeof(UnityEngine.Object).IsAssignableFrom(vt);
            }
        }

        [HarmonyPatch(typeof(Item), nameof(Item.Apply))]
        static class Item_Apply_Custom
        {
            [HarmonyPrefix]
            static bool Prefix(Item __instance, object target, bool force)
            {
                var beh = (__instance as Component)?.GetComponent<ModItems.ModItemBehaviour>();
                if (beh?.Def == null) return true; // not ours → use vanilla

                Plugin.Logger.LogInfo($"[ModItems] Apply {beh.Def.id} to {(target as UnityEngine.Object)?.name}");

                var entity = target as Entity;
                var statsRoot = entity?.GetStats();
                if (statsRoot == null) { Plugin.Logger.LogWarning("[ModItems] Apply: no stats root"); return false; }

                // Apply each defined modifier
                foreach (var m in beh.Def.statMods)
                {
                    var stat = FindStat(statsRoot, m.statPath);  // your helper from before
                    if (stat == null) { Plugin.Logger.LogWarning($"[ModItems] Stat '{m.statPath}' not found"); continue; }

                    var data = new StatModifierData
                    {
                        value = m.value,
                        type = ToGameType(m.valueType),
                        priority = 0,
                        origin = m.origin  // use -1 for permanence
                    };
                    stat.AddModifier(data);
                }

                // end/consume like vanilla
                __instance.End();
                return false; // skip vanilla attack.Trigger path
            }
        }


        // UI overrides for our items
        [HarmonyPatch(typeof(Item), nameof(Item.GetTitle))]
        [HarmonyPostfix]
        static void Item_GetTitle(Item __instance, ref string __result)
        {
            var beh = (__instance as Component)?.GetComponent<ModItemBehaviour>();
            if (beh?.Def != null && !string.IsNullOrEmpty(beh.Def.title))
                __result = beh.Def.title;
        }

        [HarmonyPatch(typeof(Item), nameof(Item.GetDescription))]
        [HarmonyPostfix]
        static void Item_GetDescription(Item __instance, ref string __result)
        {
            var beh = (__instance as Component)?.GetComponent<ModItemBehaviour>();
            if (beh?.Def != null && !string.IsNullOrEmpty(beh.Def.description))
                __result = beh.Def.description;
        }

        [HarmonyPatch(typeof(Item), nameof(Item.GetIcon))]
        [HarmonyPostfix]
        static void Item_GetIcon(Item __instance, ref Sprite __result)
        {
            var beh = (__instance as Component)?.GetComponent<ModItemBehaviour>();
            if (beh?.Def != null && beh.Def.icon != null)
                __result = beh.Def.icon;
        }

        [HarmonyPatch(typeof(Item), nameof(Item.GetCost))]
        [HarmonyPostfix]
        static void Item_GetCost(Item __instance, ref int __result)
        {
            var beh = (__instance as Component)?.GetComponent<ModItemBehaviour>();
            if (beh?.Def != null)
                __result = beh.Def.cost;
        }
    }
}
