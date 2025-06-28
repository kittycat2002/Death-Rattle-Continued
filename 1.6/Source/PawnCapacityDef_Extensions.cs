using JetBrains.Annotations;
using Verse;

namespace DeathRattle;

public class PawnCapacityDef_Extensions : DefModExtension
{
    [UsedImplicitly] public HediffDef? hediffWhenZeroFlesh;
    [UsedImplicitly] public HediffDef? hediffWhenZeroMechanoid;
    [UsedImplicitly] public bool lethalFleshWhenHediffDisabled;
    [UsedImplicitly] public bool lethalMechanoidWhenHediffDisabled;
}