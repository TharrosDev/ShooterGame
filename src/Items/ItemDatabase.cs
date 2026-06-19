using System.Collections.Generic;
using Embervale.Core.Diagnostics;
using Godot;

namespace Embervale.Items;

/// <summary>
/// Process-wide registry mapping item ids to their <see cref="ItemResource"/>
/// templates. Populated once at startup by scanning <c>res://data/items</c>, it
/// lets persistence and loot resolve items by their stable string id rather than
/// hard references — so new items are added by dropping a <c>.tres</c> in the
/// folder, no code change required.
/// </summary>
public static class ItemDatabase
{
    private const string DefaultDirectory = "res://data/items";

    private static readonly Dictionary<string, ItemResource> ById = new();

    public static IReadOnlyDictionary<string, ItemResource> All => ById;

    /// <summary>Scans the items directory and (re)builds the id → template map.</summary>
    public static void Initialize(string directory = DefaultDirectory)
    {
        ById.Clear();

        foreach (string file in DirAccess.GetFilesAt(directory))
        {
            // Exported builds expose resources as "<name>.tres.remap".
            string name = file.EndsWith(".remap") ? file[..^6] : file;
            if (!name.EndsWith(".tres"))
            {
                continue;
            }

            var item = GD.Load<ItemResource>($"{directory}/{name}");
            if (item == null)
            {
                continue;
            }

            if (ById.ContainsKey(item.Id))
            {
                Log.Warn($"Duplicate item id '{item.Id}' in {name}; overwriting.");
            }

            ById[item.Id] = item;
        }

        Log.Info($"ItemDatabase loaded {ById.Count} item(s) from {directory}.");
    }

    public static ItemResource? Get(string id)
    {
        return ById.TryGetValue(id, out ItemResource? item) ? item : null;
    }

    public static bool TryGet(string id, out ItemResource item)
    {
        return ById.TryGetValue(id, out item!);
    }
}
