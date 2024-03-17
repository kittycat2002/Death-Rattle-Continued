﻿using RimWorld;
using Verse;

namespace DeathRattle
{
    [DefOf]
    public static class RecipeDefOfComatose
    {
        public static RecipeDef ArtificialComa;

        static RecipeDefOfComatose()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RecipeDefOfComatose));
        }
    }
}