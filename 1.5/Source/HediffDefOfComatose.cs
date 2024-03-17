using RimWorld;
using Verse;

namespace DeathRattle
{
    [DefOf]
    public static class HediffDefOfComatose
    {
        public static HediffDef ArtificialComa;

        static HediffDefOfComatose()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOfComatose));
        }
    }

}