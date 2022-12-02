
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using System.Threading.Tasks;
using RimWorld;

namespace DeathRattle
{
    public class HediffGiver_BrainDamage : HediffGiver
    {
        public float minSeverity;
        public float severityAmount;
        public float baseMtbDays;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (cause.Severity < minSeverity || pawn.genes.HasGene(GeneDefOf.Deathless) || !Rand.MTBEventOccurs(baseMtbDays, 60000f, 60f))
                return;
            if (TryApply(pawn))
                SendLetter(pawn, cause);
            pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity += severityAmount;
        }

    }

}