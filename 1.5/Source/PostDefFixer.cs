using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DeathRattle
{
    [StaticConstructorOnStartup]
    public static class PostDefFixer
    {
        static PostDefFixer()
        {
            foreach (ThingDef item in from def in DefDatabase<ThingDef>.AllDefs
                                      where def.category == ThingCategory.Pawn
                                      select def)
            {

                if (item.recipes == null)
                {
                    item.recipes = new List<RecipeDef>();
                }
                if (item.recipes.Count > 0)
                {
                    if (!item.recipes.Contains(RecipeDefOfComatose.ArtificialComa))
                    {
                        item.recipes.Insert(0, RecipeDefOfComatose.ArtificialComa);
                    }
                }
                else
                {
                    item.recipes.Add(RecipeDefOfComatose.ArtificialComa);
                }

            }
        }
    }
}