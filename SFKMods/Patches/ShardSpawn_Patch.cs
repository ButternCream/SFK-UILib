using BepInEx.Logging;
using HarmonyLib;
using SFKMod.UILib;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Buildings;
using SuperFantasyKingdom.Spawner;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace SFKMod.Patches
{
    [HarmonyPatch(typeof(DroppedShardSpawner), nameof(DroppedShardSpawner.Spawn), new[] { typeof(Vector3), typeof(int), typeof(ResourceType) })]
    static class DroppedShardSpawner_Spawn_Patch
    {
        [HarmonyPrefix]
        static void Prefix(Vector3 pos, int amt, ResourceType resourceType)
        {
            Plugin.Logger.LogInfo($"[Spawner.Spawn] type={resourceType} amt={amt} pos={pos}");
        }

        [HarmonyPostfix]
        static void Postfix(Vector3 pos, int amt, ResourceType resourceType)
        {
            var shard = FindClosestShardAt(pos, 0.6f);
            if (shard == null) return;
            var meta = shard.gameObject.GetComponent<ModShardMeta>() ?? shard.gameObject.AddComponent<ModShardMeta>();
            meta.SpawnPos = pos;
            meta.SourceType = SourceContext.Type ?? "Vanilla";
            meta.SourceId = SourceContext.Id ?? "Unknown";
        }

        static DroppedShard FindClosestShardAt(Vector3 pos, float maxDist)
        {
            var all = UnityEngine.Object.FindObjectsOfType<DroppedShard>();
            DroppedShard best = null;
            float bestSq = maxDist * maxDist;
            foreach (var s in all)
            {
                var dsq = (s.transform.position - pos).sqrMagnitude;
                if (dsq <= bestSq)
                {
                    best = s;
                    bestSq = dsq;
                }
            }
            return best;
        }
    }

    // ---------------------------------------------
    // 2) Attribute AddResource to the shard that caused it
    // ---------------------------------------------
    [HarmonyPatch(typeof(DroppedShard), "Collect")]
    static class DroppedShard_Collect_TagAndLog
    {
        [HarmonyPrefix]
        static void Prefix(DroppedShard __instance)
        {
            ShardCtx.Current = __instance;

            var t = __instance.GetType();
            var fType = AccessTools.Field(t, "m_Type");
            var fAmount = AccessTools.Field(t, "m_Amount");

            var type = (ResourceType)(fType?.GetValue(__instance) ?? default(ResourceType));
            var amt = (int)(fAmount?.GetValue(__instance) ?? 0);

            var meta = __instance.GetComponent<ModShardMeta>();
            string who = meta ? $"{meta.SourceType}:{meta.SourceId}" : "<unattributed>";
            Plugin.Logger.LogInfo($"[Collect] {type}+={amt} from {who}");
        }

        [HarmonyPostfix]
        static void Postfix()
        {
            ShardCtx.Current = null;
        }
    }

    [HarmonyPatch(typeof(ResourceManager),
                  nameof(ResourceManager.AddResource),
                  new[] { typeof(ResourceType), typeof(int) })]
    static class ResourceManager_AddResource_Trace
    {
        [HarmonyPrefix]
        static void Prefix(ResourceType resourceType, int amt)
        {
            if (ShardCtx.Current != null)
            {
                var meta = ShardCtx.Current.GetComponent<ModShardMeta>();
                string who = meta ? $"{meta.SourceType}:{meta.SourceId}" : "<vanilla>";
                Plugin.Logger.LogInfo($"[AddResource] {resourceType}+={amt} via shard {who}");
            }
        }
    }
}
