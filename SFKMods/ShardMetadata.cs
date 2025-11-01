using SuperFantasyKingdom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SFKMod
{
    // ---------------------------------------------
    // 0) Small metadata tag for attribution
    // ---------------------------------------------
    public class ModShardMeta : MonoBehaviour
    {
        public string SourceType;   // e.g., "CustomItem", "BuildingTavern", "QuestReward"
        public string SourceId;     // e.g., internal id/name
        public Vector3 SpawnPos;
    }

    // Thread-local context for spawns we initiate (so we can tag them in Postfix).
    static class SourceContext
    {
        [ThreadStatic] public static string Type;
        [ThreadStatic] public static string Id;
    }

    // Thread-local context for attributing AddResource to the current Collect()ing shard
    static class ShardCtx
    {
        [ThreadStatic] public static DroppedShard Current;
    }
}
