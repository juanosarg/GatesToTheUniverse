using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using RimWorld.Planet;

namespace GatesToTheUniverse
{
    class Building_AncientFarcaster : Building_WorkTable
    {

       public Map mapHome;
       public IntVec3 locationHome;


        /* public override void SpawnSetup(Map map, bool respawningAfterLoad)
         {

             this.mapHome = (FarcasterDestination)this.Map.mapHome;
         }*/

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Map>(ref this.mapHome, "mapHome");
            Scribe_Values.Look<IntVec3>(ref this.locationHome, "locationHome");


        }




        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (myPawn.RaceProps.Humanlike && base.Faction == Faction.OfPlayer)
            {
                if (myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, TraverseMode.ByPawn))
                {
                    Action command_action_farcast = delegate
                    {
                        Job job = new Job(DefDatabase<JobDef>.GetNamed("GU_UseAncientFarcaster", true), this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GU_UseFarcaster".Translate(), command_action_farcast, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");

                }



            }
        }

    }
}
