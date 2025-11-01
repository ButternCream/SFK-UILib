// Replace ModItemTemplateCache with this smarter selector
using SFKMod.Mods;
using SuperFantasyKingdom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

static class ModItemTemplateCache
{
    static Item _template;
    static bool _isWorldTemplate;

    public static Item Get()
    {
        if (_template && _template.gameObject) return _template;

        // Gather all candidates (scene + loaded)
        var scene = UnityEngine.Object.FindObjectsOfType<Item>(includeInactive: true);
        var loaded = Resources.FindObjectsOfTypeAll<Item>();
        var candidates = scene.Concat(loaded).Distinct().ToList();

        if (candidates.Count == 0) return null;

        // Rank: prefer world items (no RectTransform, no Canvas in parents, has SpriteRenderer or Collider2D)
        Item ScoreAndPick(IEnumerable<Item> items, out bool world)
        {
            foreach (var it in items)
            {
                var rt = it.GetComponent<RectTransform>();
                var inCanvas = it.GetComponentInParent<Canvas>(true) != null;
                var hasSR = it.GetComponentInChildren<SpriteRenderer>(true) != null;
                var hasCol = it.GetComponentInChildren<Collider2D>(true) != null;

                if (rt == null && !inCanvas && (hasSR || hasCol))
                {
                    world = true;
                    return it;
                }
            }
            world = false;
            return items.FirstOrDefault();
        }

        _template = ScoreAndPick(candidates, out _isWorldTemplate);
        Plugin.Logger.LogInfo($"[ModItems] Template captured: '{_template.name}' kind={(_isWorldTemplate ? "World" : "UI")}");
        return _template;
    }

    public static bool IsWorldTemplate() => _isWorldTemplate;
}
