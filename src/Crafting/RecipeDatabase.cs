using System.Collections.Generic;
using Embervale.Core.Diagnostics;
using Godot;

namespace Embervale.Crafting;

/// <summary>
/// Process-wide registry of <see cref="CraftingRecipeResource"/>s, scanned once at
/// startup from <c>res://data/recipes</c> (mirrors the established database pattern).
/// The crafting UI lists <see cref="All"/> filtered by station + known recipes, and a
/// <see cref="CraftingComponent"/> resolves a known recipe back by id. New recipe = drop
/// a <c>.tres</c>, no code change.
/// </summary>
public static class RecipeDatabase
{
    private const string DefaultDirectory = "res://data/recipes";

    private static readonly Dictionary<string, CraftingRecipeResource> ById = new();
    private static readonly List<CraftingRecipeResource> AllList = new();

    public static IReadOnlyList<CraftingRecipeResource> All => AllList;

    public static void Initialize(string directory = DefaultDirectory)
    {
        ById.Clear();
        AllList.Clear();

        if (!DirAccess.DirExistsAbsolute(directory))
        {
            Log.Warn($"RecipeDatabase: directory '{directory}' not found; no recipes loaded.");
            return;
        }

        foreach (string file in DirAccess.GetFilesAt(directory))
        {
            string name = file.EndsWith(".remap") ? file[..^6] : file;
            if (!name.EndsWith(".tres"))
            {
                continue;
            }

            var recipe = GD.Load<CraftingRecipeResource>($"{directory}/{name}");
            if (recipe == null)
            {
                continue;
            }

            if (ById.ContainsKey(recipe.Id))
            {
                Log.Warn($"Duplicate recipe id '{recipe.Id}' in {name}; overwriting.");
            }
            else
            {
                AllList.Add(recipe);
            }

            ById[recipe.Id] = recipe;
        }

        Log.Info($"RecipeDatabase loaded {ById.Count} recipe(s) from {directory}.");
    }

    public static CraftingRecipeResource? Get(string id)
    {
        return ById.TryGetValue(id, out CraftingRecipeResource? recipe) ? recipe : null;
    }
}
