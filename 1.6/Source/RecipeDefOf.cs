using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace DeathRattle;

[DefOf]
public static class RecipeDefOf
{
    [UsedImplicitly] public static RecipeDef? ArtificialComa;

    static RecipeDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RecipeDefOf));
    }
}