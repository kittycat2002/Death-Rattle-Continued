﻿using System;
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
            bodyPartDict = DefDatabase<BodyPartDef>.AllDefsListForReading.Where((def) => def.HasModExtension<BodyPartDef_Extensions>())
                                                                         .ToDictionary(def => def ,def => def.GetModExtension<BodyPartDef_Extensions>().hediffWhenMissing);
            capacityDictFlesh = DefDatabase<PawnCapacityDef>.AllDefsListForReading.Where(def => def.GetModExtension<PawnCapacityDef_Extensions>()?.hediffWhenZeroFlesh != null)
                                                                                  .ToDictionary(def => def, def => def.GetModExtension<PawnCapacityDef_Extensions>().hediffWhenZeroFlesh);
            capacityDictMechanoid = DefDatabase<PawnCapacityDef>.AllDefsListForReading.Where(def => def.GetModExtension<PawnCapacityDef_Extensions>()?.hediffWhenZeroMechanoid != null)
                                                                                      .ToDictionary(def => def, def => def.GetModExtension<PawnCapacityDef_Extensions>().hediffWhenZeroMechanoid);
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
            if (___pawn.health.ShouldBeDead())
                return;
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
                if (___pawn.health.ShouldBeDead())
                    return;
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
    public static class ShouldBeDead_Patch
    {
        [HarmonyPostfix]
        public static bool ShouldBeDead_Postfix(bool __result, Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            if (__result)
            {
                return true;
            }
            List<BodyPartRecord> parts = ___pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource);
            if (parts.Count > 0 && parts.All((p) => __instance.hediffSet.PartIsMissing(p) || __instance.hediffSet.GetPartHealth(p) <= 0.0001f))
            {
                return true;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(Hediff), "TendableNow")]
    public static class TendableNow_Patch
    {
        [HarmonyPrefix]
        public static bool TendableNow_Prefix(ref bool __result, Hediff __instance)
        {
            HediffComp_TendSeverity comp = __instance.TryGetComp<HediffComp_TendSeverity>();
            if (comp != null && __instance.Severity >= comp.MaxSeverity) {
                __result = false;
                return false;
            }
            return true;
        }
    }
}