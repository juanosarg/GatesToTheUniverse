using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace GatesToTheUniverse
{
    public class JobDriver_AncientFarcaster : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_AncientFarcaster building_Farcaster = null;
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate (Toil to)
            {
                building_Farcaster = (Building_AncientFarcaster)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                return false;
            });
            Toil enterFarcaster = new Toil();
            enterFarcaster.initAction = delegate
            {
                pawn.DeSpawn();
                GenSpawn.Spawn(pawn, building_Farcaster.locationHome, building_Farcaster.mapHome);
            };
            yield return enterFarcaster;
        }
    }
}
