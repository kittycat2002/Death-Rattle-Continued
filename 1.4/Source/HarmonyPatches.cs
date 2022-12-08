using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Verse;
using HarmonyLib;
using RimWorld;
using System.Reflection;

namespace DeathRattle
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        public static Dictionary<BodyPartDef, HediffDef> bodyPartDict = new Dictionary<BodyPartDef, HediffDef>();
        public static Dictionary<PawnCapacityDef, HediffDef> capacityDictFlesh = new Dictionary<PawnCapacityDef, HediffDef>();
        public static Dictionary<PawnCapacityDef, HediffDef> capacityDictMechanoid = new Dictionary<PawnCapacityDef, HediffDef>();
        static HarmonyPatches()
        {
            foreach (BodyPartDef def in DefDatabase<BodyPartDef>.AllDefsListForReading.Where((d) => d.HasModExtension<BodyPartDef_Extensions>()))
            {
                bodyPartDict.Add(def, def.GetModExtension<BodyPartDef_Extensions>().hediffWhenMissing);
            }
            foreach (PawnCapacityDef def in DefDatabase<PawnCapacityDef>.AllDefsListForReading.Where((d) => d.HasModExtension<PawnCapacityDef_Extensions>()))
            {
                PawnCapacityDef_Extensions ext = def.GetModExtension<PawnCapacityDef_Extensions>();
                if (ext.hediffWhenZeroFlesh != null)
                    capacityDictFlesh.Add(def, ext.hediffWhenZeroFlesh);
                if (ext.hediffWhenZeroMechanoid != null)
                    capacityDictFlesh.Add(def, ext.hediffWhenZeroMechanoid);
            }
            var harmony = new Harmony("cat2002.deathrattle");
            harmony.PatchAll();
        }
    }
    [HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
    public static class HealthTick_Patch
    {
        [HarmonyPostfix]
        public static void HealthTick_Postfix(Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            List<BodyPartDef> bodyPartDefs = DefDatabase<BodyPartDef>.AllDefsListForReading;
            foreach ((BodyPartDef def, HediffDef hediff) in HarmonyPatches.bodyPartDict)
            {
                if (!__instance.hediffSet.HasHediff(hediff)) {
                    List<BodyPartRecord> parts = ___pawn.RaceProps.body.GetPartsWithDef(def);
                    if (parts.Count > 0 && parts.All((p) => PawnCapacityUtility.CalculatePartEfficiency(__instance.hediffSet, p, false, null) <= 0.0001f))
                    {
                        __instance.AddHediff(hediff);
                    }
                }
            }
            List<PawnCapacityDef> pawnCapacityDefs = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
            foreach ((PawnCapacityDef def, HediffDef hediff) in ___pawn.RaceProps.IsFlesh ? HarmonyPatches.capacityDictFlesh : HarmonyPatches.capacityDictMechanoid)
            {
                if (!__instance.hediffSet.HasHediff(hediff) && !__instance.capacities.CapableOf(def))
                {
                    __instance.AddHediff(hediff);
                }
            }
        }
    }
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDead")]
    public static class ShouldBeDead
    {
        [HarmonyPostfix]
        public static bool ShouldBeDead_Postfix(bool __result, Pawn_HealthTracker __instance)
        {
            if (__result == true)
            {
                return true;
            }
            BodyPartRecord brain = __instance.hediffSet.GetBrain();
            if (brain == null || __instance.hediffSet.PartIsMissing(brain) || __instance.hediffSet.GetPartHealth(brain) <= 0.0001f)
            {
                return true;
            }
            return false;
        }
    }
}