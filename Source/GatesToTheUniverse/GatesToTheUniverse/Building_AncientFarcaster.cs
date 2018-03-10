using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using RimWorld.Planet;
using Verse.Sound;

namespace GatesToTheUniverse
{
    class Building_AncientFarcaster : Building_WorkTable
    {

       public Map mapHome;
       public IntVec3 locationHome;
        public SoundDef soundDef = DefDatabase<SoundDef>.GetNamed("GU_FarcasterSound", true);




        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Map>(ref this.mapHome, "mapHome");
            Scribe_Values.Look<IntVec3>(ref this.locationHome, "locationHome");

        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }



           
                Command_Action command_Action4 = new Command_Action();
                command_Action4.defaultLabel = "GU_SendThingsToFarcastLocation".Translate();
                command_Action4.defaultDesc = "GU_SendThingsToFarcastLocationDesc".Translate();
                command_Action4.icon = ContentFinder<Texture2D>.Get("UI/GU_SendThingsToFarcastLocation", true);
                command_Action4.action = delegate
                {
                    SendThingsByFarcast();

                };
                yield return command_Action4;

            


        }

        public void SendThingsByFarcast()
        {

            CellRect rect = GenAdj.OccupiedRect(this.Position, this.Rotation, this.def.size);
            rect = rect.ExpandedBy(4);
            soundDef.PlayOneShotOnCamera(null);

            foreach (IntVec3 current in rect.Cells)
            {
                Building edifice = current.GetEdifice(this.Map);
                if (edifice != null && ((edifice.def.defName == "GU_FarcasterPad") || (edifice.def.defName == "GU_AncientFarcasterPad")))
                {
                    foreach (IntVec3 current2 in edifice.OccupiedRect().Cells)
                    {
                        List<Thing> thingList = current2.GetThingList(this.Map);
                        for (int i = 0; i < thingList.Count; i++)
                        {
                            if (thingList[i] is Pawn || (thingList[i] is ThingWithComps && !(thingList[i] is Building)))
                            {
                                Thing thingToSend = thingList[i];
                                thingToSend.DeSpawn();
                                GenSpawn.Spawn(thingToSend, locationHome - GenAdj.CardinalDirections[0] * 4, mapHome);
                            }
                        }
                    }
                }
            }
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
