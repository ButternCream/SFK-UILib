// File: ModItem_ApplyAtManager.cs
using HarmonyLib;
using SFKMod.Mods;
using SuperFantasyKingdom;
using UnityEngine;

namespace ModItems
{
    [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.Apply))]
    static class ItemManager_Apply_ModItems
    {
        [HarmonyPrefix]
        static bool Prefix(string itemIdentifier, Entity entity)
        {

            Plugin.Logger.LogInfo($"[ModItems] Manager.Apply {itemIdentifier} -> {(entity as UnityEngine.Object)?.name}");

            // Not ours? Let vanilla run.
            if (string.IsNullOrEmpty(itemIdentifier) || !itemIdentifier.StartsWith("mod:")) return true;

            if (entity == null) { Plugin.Logger.LogWarning("[ModItems] Apply: entity null"); return false; }

            if (!ModItemRegistry.TryGet(itemIdentifier, out var def) || def == null)
            {
                Plugin.Logger.LogWarning($"[ModItems] Apply: no def for '{itemIdentifier}'");
                return false; // skip vanilla, nothing to do
            }

            var statsRoot = entity.GetStats();
            if (statsRoot == null) { Plugin.Logger.LogWarning("[ModItems] Apply: stats root null"); return false; }

            foreach (var m in def.statMods)
            {
                var stat = FindStat(statsRoot, m.statPath);
                if (stat == null) { Plugin.Logger.LogWarning($"[ModItems] Apply: stat '{m.statPath}' not found"); continue; }

                // IMPORTANT: use origin = -1 to survive the game's RemoveAllModifiers(force:false)
                var data = new StatModifierData
                {
                    value = m.value,
                    type = ToGameType(m.valueType),
                    priority = 0,
                    origin = m.origin == 0 ? -1 : m.origin
                };
                stat.AddModifier(data);
            }

            Plugin.Logger.LogInfo($"[ModItems] Applied {def.id} to {(entity as UnityEngine.Object)?.name}");

            // We handled it; skip vanilla ItemManager.Apply (which would load/trigger Item attack path).
            return false;
        }

        static StatModifierType ToGameType(ModValueType t) => t switch
        {
            ModValueType.Flat => StatModifierType.Flat,
            ModValueType.PercentAdd => StatModifierType.PercentAdd,
            ModValueType.PercentMult => StatModifierType.PercentMult,
            _ => StatModifierType.Flat
        };

        // Reuse the same recursive stat finder you already had; included here for completeness:
        static Stat FindStat(object statsRoot, string statPath)
        {
            if (statsRoot == null || string.IsNullOrEmpty(statPath)) return null;
            var t = statsRoot.GetType();
            const System.Reflection.BindingFlags BF = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

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
    }
}
