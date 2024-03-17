using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DeathRattle
{
    public class HediffComp_RemoveWhenHasCapacity : HediffComp
    {
        public HediffCompProperties_RemoveWhenHasCapacity Props => (HediffCompProperties_RemoveWhenHasCapacity)props;
        public PawnCapacityDef Capacity => Props.capacity;

        public override bool CompShouldRemove
        {
            get
            {
                return parent.pawn.health.capacities.CapableOf(Capacity);
            }
        }
    }
    public class HediffCompProperties_RemoveWhenHasCapacity : HediffCompProperties
    {
        public PawnCapacityDef capacity;
        public HediffCompProperties_RemoveWhenHasCapacity()
        {
            compClass = typeof(HediffComp_RemoveWhenHasCapacity);
        }
    }
    public class HediffComp_RemoveWhenHasPart : HediffComp
    {
        public HediffCompProperties_RemoveWhenHasPart Props => (HediffCompProperties_RemoveWhenHasPart)props;
        public BodyPartDef Part => Props.part;

        public override bool CompShouldRemove
        {
            get
            {
                return parent.pawn.RaceProps.body.GetPartsWithDef(Part).Any((p) => PawnCapacityUtility.CalculatePartEfficiency(parent.pawn.health.hediffSet, p, false, null) > 0.0001f);
            }
        }
    }
    public class HediffCompProperties_RemoveWhenHasPart : HediffCompProperties
    {
        public BodyPartDef part;
        public HediffCompProperties_RemoveWhenHasPart()
        {
            compClass = typeof(HediffComp_RemoveWhenHasPart);
        }
    }
    public class HediffComp_TendSeverity : HediffComp
    {
        public HediffCompProperties_TendSeverity Props => (HediffCompProperties_TendSeverity)props;
        public float MaxSeverity => Props.maxSeverity;
    }
    public class HediffCompProperties_TendSeverity : HediffCompProperties
    {
        public float maxSeverity;
        public HediffCompProperties_TendSeverity()
        {
            compClass = typeof(HediffComp_TendSeverity);
        }
    }
}