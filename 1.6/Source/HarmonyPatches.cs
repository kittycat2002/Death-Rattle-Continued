using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DeathRattle;

[StaticConstructorOnStartup]
class HarmonyPatches
{
	internal static readonly Dictionary<BodyPartDef, HediffDef> bodyPartDict;
	internal static readonly Dictionary<PawnCapacityDef, HediffDef> capacityDictFlesh;
	internal static readonly Dictionary<PawnCapacityDef, HediffDef> capacityDictMechanoid;
	internal static readonly List<PawnCapacityDef> capacityDictFleshNoHediff;
	internal static readonly List<PawnCapacityDef> capacityDictMechanoidNoHediff;

	static HarmonyPatches()
	{
		bodyPartDict = DefDatabase<BodyPartDef>.AllDefsListForReading
			.Where(def => def.GetModExtension<BodyPartDef_Extensions>()?.hediffWhenMissing != null)
			.ToDictionary(def => def, def => def.GetModExtension<BodyPartDef_Extensions>().hediffWhenMissing!);
		capacityDictFlesh = DefDatabase<PawnCapacityDef>.AllDefsListForReading
			.Where(def => def.GetModExtension<PawnCapacityDef_Extensions>()?.hediffWhenZeroFlesh != null)
			.ToDictionary(def => def,
				def => def.GetModExtension<PawnCapacityDef_Extensions>().hediffWhenZeroFlesh!);
		capacityDictMechanoid = DefDatabase<PawnCapacityDef>.AllDefsListForReading
			.Where(def => def.GetModExtension<PawnCapacityDef_Extensions>()?.hediffWhenZeroMechanoid != null)
			.ToDictionary(def => def,
				def => def.GetModExtension<PawnCapacityDef_Extensions>().hediffWhenZeroMechanoid!);
		capacityDictFleshNoHediff = DefDatabase<PawnCapacityDef>.AllDefsListForReading.Where(def =>
			def.GetModExtension<PawnCapacityDef_Extensions>()?.lethalFleshWhenHediffDisabled != null).ToList();
		capacityDictMechanoidNoHediff = DefDatabase<PawnCapacityDef>.AllDefsListForReading.Where(def =>
			def.GetModExtension<PawnCapacityDef_Extensions>()?.lethalMechanoidWhenHediffDisabled == true).ToList();
		Harmony harmony = new Harmony("cat2002.deathrattle");
		harmony.PatchAll();
	}
}

[HarmonyPatch(typeof(HediffSet), nameof(HediffSet.DirtyCache))]
public static class HediffSet_DirtyCache_Patch
{
	public static void Postfix(HediffSet __instance, Pawn ___pawn)
	{
		if (!HediffHelpers.ShouldApplyHediffs(__instance, ___pawn)) return;
		foreach ((BodyPartDef def, HediffDef hediff) in HarmonyPatches.bodyPartDict)
		{
			if (__instance.HasHediff(hediff)) continue;
			List<BodyPartRecord> parts = ___pawn.RaceProps.body.GetPartsWithDef(def);
			if (parts.Count > 0 && parts.All((p) =>
				    PawnCapacityUtility.CalculatePartEfficiency(__instance, p) <= 0.0001f))
			{
				___pawn.health.AddHediff(hediff);
			}
		}
	}
}

[HarmonyPatch(typeof(PawnCapacitiesHandler), nameof(PawnCapacitiesHandler.Notify_CapacityLevelsDirty))]
public static class PawnCapacitiesHandler_Notify_CapacityLevelsDirty_Patch
{
	public static void Postfix(PawnCapacitiesHandler __instance, Pawn ___pawn)
	{
		if (Scribe.mode == LoadSaveMode.LoadingVars) return;
		if (!HediffHelpers.ShouldApplyHediffs(___pawn.health.hediffSet, ___pawn)) return;
		foreach ((PawnCapacityDef def, HediffDef hediff) in ___pawn.RaceProps.IsFlesh
			         ? HarmonyPatches.capacityDictFlesh
			         : HarmonyPatches.capacityDictMechanoid)
		{
			if (!___pawn.health.hediffSet.HasHediff(hediff) && !__instance.CapableOf(def))
			{
				___pawn.health.AddHediff(hediff);
			}
		}
	}
}

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ShouldBeDead))]
public static class ShouldBeDead_Patch
{
	public static bool Postfix(bool __result, Pawn_HealthTracker __instance, Pawn ___pawn)
	{
		if (__result)
		{
			return true;
		}

		List<BodyPartRecord> parts = ___pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource);
		return parts.Count > 0 && parts.All((p) =>
			__instance.hediffSet.PartIsMissing(p) || __instance.hediffSet.GetPartHealth(p) <= 0.0001f);
	}
}

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ShouldBeDeadFromRequiredCapacity))]
public static class ShouldBeDeadFromRequiredCapacity_Patch
{
	public static PawnCapacityDef? Postfix(PawnCapacityDef? __result, Pawn_HealthTracker __instance,
		Pawn ___pawn)
	{
		if (__result != null || HediffHelpers.ShouldAffectPawn(___pawn))
		{
			return __result;
		}

		foreach (PawnCapacityDef pawnCapacityDef in (___pawn.RaceProps.IsFlesh
			         ? HarmonyPatches.capacityDictFleshNoHediff
			         : HarmonyPatches.capacityDictMechanoidNoHediff).Where(pawnCapacityDef => !__instance.capacities.CapableOf(pawnCapacityDef)))
		{
			if (DebugViewSettings.logCauseOfDeath)
			{
				Log.Message($"CauseOfDeath: no longer capable of {pawnCapacityDef}");
			}

			return pawnCapacityDef;
		}

		return null;
	}
}

[HarmonyPatch(typeof(Hediff), nameof(Hediff.TendableNow))]
internal static class Hediff_TendableNow_Patch
{
	internal static bool Prefix(ref bool __result, Hediff __instance)
	{
		if (!__instance.TryGetComp(out HediffComp_TendSeverity comp) ||
		    !(__instance.Severity >= comp.MaxSeverity)) return true;
		__result = false;
		return false;

	}
}

internal static class HediffHelpers
{
	public static bool ShouldApplyHediffs(HediffSet hediffs, Pawn pawn)
	{
		return !pawn.health.ShouldBeDead() && !hediffs.HasPreventsDeath && ShouldAffectPawn(pawn);
	}

	public static bool ShouldAffectPawn(Pawn pawn)
	{
		return !(!DeathRattleSettings.AffectsShamblers && pawn.IsShambler) &&
		       !(!DeathRattleSettings.AffectsAnimals && !pawn.IsShambler && !pawn.RaceProps.Humanlike) &&
		       !(DeathRattleSettings.ColonyOnly && pawn.Faction is not { IsPlayer: true });
	}
}