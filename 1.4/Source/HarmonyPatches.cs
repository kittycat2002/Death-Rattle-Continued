using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Verse;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using System.Security.Cryptography;

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
    [HarmonyPatch(typeof(HediffSet), "DirtyCache")]
    public static class HediffSet_DirtyCache_Patch
    {
        [HarmonyPostfix]
        public static void DirtyCache_Postfix(HediffSet __instance, Pawn ___pawn)
        {
            foreach ((BodyPartDef def, HediffDef hediff) in HarmonyPatches.bodyPartDict)
            {
                if (!__instance.HasHediff(hediff)) {
                    List<BodyPartRecord> parts = ___pawn.RaceProps.body.GetPartsWithDef(def);
                    if (parts.Count > 0 && parts.All((p) => PawnCapacityUtility.CalculatePartEfficiency(__instance, p, false, null) <= 0.0001f))
                    {
                        ___pawn.health.AddHediff(hediff);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(PawnCapacitiesHandler), "Notify_CapacityLevelsDirty")]
    public static class PawnCapacitiesHandler_Notify_CapacityLevelsDirty_Patch
    {
        [HarmonyPostfix]
        public static void Notify_CapacityLevelsDirty_Postfix(PawnCapacitiesHandler __instance, Pawn ___pawn)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars)
            {
                foreach ((PawnCapacityDef def, HediffDef hediff) in ___pawn.RaceProps.IsFlesh ? HarmonyPatches.capacityDictFlesh : HarmonyPatches.capacityDictMechanoid)
                {
                    if (!___pawn.health.hediffSet.HasHediff(hediff) && !__instance.CapableOf(def))
                    {
                        ___pawn.health.AddHediff(hediff);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDead")]
    public static class ShouldBeDead
    {
        [HarmonyPostfix]
        public static bool ShouldBeDead_Postfix(bool __result, Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            if (__result == true)
            {
                return true;
            }
            List<BodyPartRecord> parts = ___pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource);
            if (parts.Count > 0 && parts.Any((p) => __instance.hediffSet.PartIsMissing(p) || __instance.hediffSet.GetPartHealth(p) <= 0.0001f))
            {
                return true;
            }
            return false;
        }
    }
}