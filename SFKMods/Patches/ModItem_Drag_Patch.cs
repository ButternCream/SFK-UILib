// File: ModItem_GiveDragItemPost.cs
using HarmonyLib;
using SuperFantasyKingdom;

namespace ModItems
{
    [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.GiveDragItem))]
    static class ItemManager_GiveDragItem_ModPost
    {
        [HarmonyPostfix]
        static void Postfix(ItemManager __instance, UnitBase unit)
        {
            var g = __instance.GetDragItem();
            if (g == null) return;
            var id = g.GetItemIdentifier();
            if (!string.IsNullOrEmpty(id) && id.StartsWith("mod:"))
            {
                // Apply now (our Apply prefix ensures proper handling)
                ItemManager.Instance.Apply(id, unit);
            }
        }
    }
}
