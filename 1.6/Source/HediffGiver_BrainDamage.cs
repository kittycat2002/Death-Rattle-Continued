using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace DeathRattle;

public class HediffGiver_BrainDamage : HediffGiver
{
    [UsedImplicitly] public float minSeverity;
    [UsedImplicitly] public float severityAmount;
    [UsedImplicitly] public float baseMtbDays;
    [UsedImplicitly] public bool lifeSupportPrevents;

    public override void OnIntervalPassed(Pawn pawn, Hediff cause)
    {
        if (!(cause.Severity >= minSeverity)
            || (pawn.genes != null && pawn.genes.HasActiveGene(GeneDefOf.Deathless))
            || (lifeSupportPrevents && HediffDefOf.QE_LifeSupport != null && pawn.health.hediffSet.HasHediff(HediffDefOf.QE_LifeSupport))
            || !Rand.MTBEventOccurs(baseMtbDays, 60000f, 60f)) return;
        if (TryApply(pawn))
            SendLetter(pawn, cause);
        pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity += severityAmount;
    }

}