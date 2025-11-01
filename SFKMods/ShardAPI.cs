using SFKMod.Patches;
using SFKMod.UILib;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Spawner;
using UnityEngine;

namespace SFKMod
{
    // ---------------------------------------------
    // Easy API: spawn Faith shards through the real pipeline
    // ---------------------------------------------
    public static class ShardAPI
    {
        static DroppedShardSpawner _cached;

        static DroppedShardSpawner Spawner
        {
            get
            {
                if (_cached && _cached.gameObject != null) return _cached;
                _cached = UnityEngine.Object.FindObjectOfType<DroppedShardSpawner>();
                return _cached;
            }
        }

        /// <summary>
        /// Spawns a Faith shard at position using the game's real spawner (animations, sounds, stats, saves).
        /// </summary>
        public static void SpawnFaith(Vector3 position, int amount, string sourceId = "MyCustomItem")
        {
            if (!Spawner)
            {
                Plugin.Logger.LogWarning("[ShardAPI] DroppedShardSpawner not found in scene.");
                return;
            }

            // Tag the next spawn with our origin. Cleared in finally.
            SourceContext.Type = "CustomItem";
            SourceContext.Id = sourceId;
            try
            {
                Spawner.Spawn(position, amount, ResourceType.Faith);
            }
            finally
            {
                SourceContext.Type = null;
                SourceContext.Id = null;
            }
        }

        /// <summary>
        /// Generic spawn helper if you want other resource types too.
        /// </summary>
        public static void SpawnResource(Vector3 position, int amount, ResourceType type, string sourceType = "CustomItem", string sourceId = "Unknown")
        {
            if (!Spawner)
            {
                Plugin.Logger.LogWarning("[ShardAPI] DroppedShardSpawner not found in scene.");
                return;
            }

            SourceContext.Type = sourceType;
            SourceContext.Id = sourceId;
            try
            {
                Spawner.Spawn(position, amount, type);
            }
            finally
            {
                SourceContext.Type = null;
                SourceContext.Id = null;
            }
        }
    }
}
