﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace GatesToTheUniverse
{
    public class JobDriver_Farcaster : JobDriver
    {
        public override bool TryMakePreToilReservations(bool whatever)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_Farcaster building_Farcaster = null;
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate (Toil to)
            {
                building_Farcaster = (Building_Farcaster)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                return false;
            });
            Toil enterFarcaster = new Toil();
            enterFarcaster.initAction = delegate
            {
                pawn.DeSpawn();
                GenSpawn.Spawn(pawn, building_Farcaster.locationFarcast, building_Farcaster.mapParent.Map);
                FloodFillerFog.FloodUnfog(building_Farcaster.locationFarcast, building_Farcaster.mapParent.Map);

            };
            yield return enterFarcaster;
        }
    }
}