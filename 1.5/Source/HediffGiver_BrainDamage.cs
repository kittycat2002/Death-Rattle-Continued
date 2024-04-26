using RimWorld;
using Verse;

namespace DeathRattle
{
    [StaticConstructorOnStartup]
    static class LifeSupportChecker
    {
        public static readonly bool hasLifeSupport = false;
        private static readonly HediffDef hediffDef;
        static LifeSupportChecker()
        {
            if (ModsConfig.IsActive("Troopersmith1.LifeSupport"))
            {
                hasLifeSupport = true;
                hediffDef = HediffDef.Named("QE_LifeSupport");
            }
        }
        public static bool HasLifeSupport(Pawn pawn) { return pawn.health.hediffSet.HasHediff(hediffDef); }
    }
    public class HediffGiver_BrainDamage : HediffGiver
    {
        public float minSeverity;
        public float severityAmount;
        public float baseMtbDays;
        public bool lifeSupportPrevents;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (cause.Severity >= minSeverity
                && (pawn.genes == null || !pawn.genes.HasGene(GeneDefOf.Deathless))
                && (!LifeSupportChecker.hasLifeSupport || !lifeSupportPrevents || !LifeSupportChecker.HasLifeSupport(pawn))
                && Rand.MTBEventOccurs(baseMtbDays, 60000f, 60f))
            {
                if (TryApply(pawn))
                    SendLetter(pawn, cause);
                pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity += severityAmount;
            }
        }

    }

}