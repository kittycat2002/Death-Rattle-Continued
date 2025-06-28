using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace DeathRattle;

public class HediffComp_RemoveWhenHasCapacity : HediffComp
{
    private HediffCompProperties_RemoveWhenHasCapacity Props => (HediffCompProperties_RemoveWhenHasCapacity)props;
    private PawnCapacityDef? Capacity => Props.capacity;

    public override bool CompShouldRemove => parent.pawn.health.capacities.CapableOf(Capacity);
}
[UsedImplicitly]
public class HediffCompProperties_RemoveWhenHasCapacity : HediffCompProperties
{
    [UsedImplicitly] public PawnCapacityDef? capacity;
    public HediffCompProperties_RemoveWhenHasCapacity()
    {
        compClass = typeof(HediffComp_RemoveWhenHasCapacity);
    }
}
public class HediffComp_RemoveWhenHasPart : HediffComp
{
    private HediffCompProperties_RemoveWhenHasPart Props => (HediffCompProperties_RemoveWhenHasPart)props;
    private BodyPartDef? Part => Props.part;

    public override bool CompShouldRemove
    {
        get
        {
            return parent.pawn.RaceProps.body.GetPartsWithDef(Part).Any((p) => PawnCapacityUtility.CalculatePartEfficiency(parent.pawn.health.hediffSet, p) > 0.0001f);
        }
    }
}
[UsedImplicitly]
public class HediffCompProperties_RemoveWhenHasPart : HediffCompProperties
{
    [UsedImplicitly] public BodyPartDef? part;
    public HediffCompProperties_RemoveWhenHasPart()
    {
        compClass = typeof(HediffComp_RemoveWhenHasPart);
    }
}
public class HediffComp_TendSeverity : HediffComp
{
    private HediffCompProperties_TendSeverity Props => (HediffCompProperties_TendSeverity)props;
    public float MaxSeverity => Props.maxSeverity;
}
[UsedImplicitly]
public class HediffCompProperties_TendSeverity : HediffCompProperties
{
    [UsedImplicitly] public float maxSeverity;
    public HediffCompProperties_TendSeverity()
    {
        compClass = typeof(HediffComp_TendSeverity);
    }
}

public class HediffComp_StabilizeIfOtherHediff : HediffComp
{
    private HediffCompProperties_StabilizeIfOtherHediff Props => (HediffCompProperties_StabilizeIfOtherHediff)props;
    private float SeverityPerDay => Props.severityPerDay;
    private float TargetSeverity => Props.targetSeverity;
    public override void CompPostTickInterval(ref float severityAdjustment, int delta)
    {
        if (!Pawn.IsHashIntervalTick(200, delta) || !ShouldUpdate()) return;
        float num = SeverityPerDay * 0.0033333334f;
        if (parent.Severity > TargetSeverity)
        {
            if (parent.Severity - num < TargetSeverity)
            {
                num = TargetSeverity - parent.Severity;
            }
            else
            {
                num *= -1f;
            }
        }
        else if (parent.Severity + num > TargetSeverity)
        {
            num =  TargetSeverity - parent.Severity;
        }
        severityAdjustment = num; // This is assigned on purpose, do not replace with +=.
    }
    private bool ShouldUpdate() => Props.hediffs.Any(hediffDef => Pawn.health.hediffSet.HasHediff(hediffDef));
}

public class HediffCompProperties_StabilizeIfOtherHediff : HediffCompProperties
{
    [UsedImplicitly] public float severityPerDay;
    [UsedImplicitly] public float targetSeverity;
    [UsedImplicitly] public List<HediffDef> hediffs = [];
    public HediffCompProperties_StabilizeIfOtherHediff()
    {
        compClass = typeof(HediffComp_StabilizeIfOtherHediff);
    }
}