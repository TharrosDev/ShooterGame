using System.Collections.Generic;
using Embervale.Core.Diagnostics;
using Godot;

namespace Embervale.Magic;

/// <summary>
/// Process-wide registry of <see cref="SpellResource"/>s, scanned once at startup
/// from <c>res://data/spells</c> (mirrors <see cref="Embervale.Progression.PerkDatabase"/>).
/// A <see cref="SpellcastingComponent"/> resolves its known spell ids through here,
/// and save/load restores a spell list by id. New spell = drop a <c>.tres</c>.
/// </summary>
public static class SpellDatabase
{
    private const string DefaultDirectory = "res://data/spells";

    private static readonly Dictionary<string, SpellResource> ById = new();
    private static readonly List<SpellResource> AllList = new();

    public static IReadOnlyList<SpellResource> All => AllList;

    public static void Initialize(string directory = DefaultDirectory)
    {
        ById.Clear();
        AllList.Clear();

        if (!DirAccess.DirExistsAbsolute(directory))
        {
            Log.Warn($"SpellDatabase: directory '{directory}' not found; no spells loaded.");
            return;
        }

        foreach (string file in DirAccess.GetFilesAt(directory))
        {
            string name = file.EndsWith(".remap") ? file[..^6] : file;
            if (!name.EndsWith(".tres"))
            {
                continue;
            }

            var spell = GD.Load<SpellResource>($"{directory}/{name}");
            if (spell == null)
            {
                continue;
            }

            if (ById.ContainsKey(spell.Id))
            {
                Log.Warn($"Duplicate spell id '{spell.Id}' in {name}; overwriting.");
            }
            else
            {
                AllList.Add(spell);
            }

            ById[spell.Id] = spell;
        }

        Log.Info($"SpellDatabase loaded {ById.Count} spell(s) from {directory}.");
    }

    public static SpellResource? Get(string id)
    {
        return ById.TryGetValue(id, out SpellResource? spell) ? spell : null;
    }
}
