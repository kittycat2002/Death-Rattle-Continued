using System.Linq;
using Verse;

namespace DeathRattle;

[StaticConstructorOnStartup]
public static class PostDefFixer
{
    static PostDefFixer()
    {
        foreach (ThingDef item in from def in DefDatabase<ThingDef>.AllDefs
                 where def.category == ThingCategory.Pawn
                 select def)
        {
            item.recipes ??= [];
            if (item.recipes.Count > 0)
            {
                if (!item.recipes.Contains(RecipeDefOf.ArtificialComa))
                {
                    item.recipes.Insert(0, RecipeDefOf.ArtificialComa);
                }
            }
            else
            {
                item.recipes.Add(RecipeDefOf.ArtificialComa);
            }
        }
    }
}