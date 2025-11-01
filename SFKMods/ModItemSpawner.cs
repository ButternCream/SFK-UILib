using SFKMod.Mods;
using SuperFantasyKingdom;
using UnityEngine;

namespace ModItems
{
    public static class ModItemSpawner
    {
        // Spawns a draggable item using the game's pipeline (UI/world container handled by ItemManager)
        public static void SpawnDrag(string id, Vector2 worldPos)
        {
            if (ItemManager.Instance == null)
            {
                Plugin.Logger.LogWarning("[ModItems] ItemManager.Instance is null; cannot spawn drag item.");
                return;
            }
            ItemManager.Instance.SpawnDragItem(id, worldPos, false);
        }

        // Applies an item directly to an entity (no dragging)
        public static void ApplyTo(Entity entity, string id)
        {
            if (ItemManager.Instance == null)
            {
                Plugin.Logger.LogWarning("[ModItems] ItemManager.Instance is null; cannot Apply.");
                return;
            }
            ItemManager.Instance.Apply(id, entity);
        }
    }
}
