using JetBrains.Annotations;
using RimWorld;
using Verse;
// ReSharper disable InconsistentNaming

namespace DeathRattle;

[DefOf]
public static class HediffDefOf
{
	[UsedImplicitly] public static HediffDef? IntestinalFailure;
	[UsedImplicitly] public static HediffDef? LiverFailure;
	[UsedImplicitly] public static HediffDef? KidneyFailure;
	[UsedImplicitly] public static HediffDef? ClinicalDeathNoHeartbeat;
	[UsedImplicitly] public static HediffDef? ClinicalDeathAsphyxiation;
	[UsedImplicitly] public static HediffDef? Coma;
	[UsedImplicitly] public static HediffDef? ArtificialComa;
		
	[UsedImplicitly] public static HediffDef? WakeUpHigh;

	[MayRequire("Troopersmith1.LifeSupport")] [UsedImplicitly]
	public static HediffDef? QE_LifeSupport;

	static HediffDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
	}
}