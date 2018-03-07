﻿
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using RimWorld.Planet;

namespace GatesToTheUniverse
{
    class Building_JuryRiggedFarcaster : Building_WorkTable
    {
        
        private bool activatedBool = true;
        private bool confirmDeactivation = false;
        public MapParent mapParent;
        public Map mapHome;
        public IntVec3 locationFarcast;



        private Graphic GraphicactivatedBool;

        public override Graphic Graphic
        {
            get
            {
                if (this.activatedBool)
                {
                   
                    return base.Graphic;
                }
                if (this.GraphicactivatedBool == null)
                {
                    this.GraphicactivatedBool = this.def.building.trapUnarmedGraphicData.GraphicColoredFor(this);
                }
               
                return this.GraphicactivatedBool;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.activatedBool, "activatedBool", false, false);
            Scribe_Values.Look<bool>(ref this.confirmDeactivation, "confirmDeactivation", false, false);
            Scribe_References.Look<MapParent>(ref this.mapParent, "mapParent");
            Scribe_References.Look<Map>(ref this.mapHome, "mapHome");
            Scribe_Values.Look<IntVec3>(ref this.locationFarcast, "locationFarcast");


        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
           

             if (activatedBool) { 
                 Command_Action command_Action = new Command_Action();
                 command_Action.defaultLabel = "GU_EstablishFarcasterLink".Translate();
                 command_Action.defaultDesc = "GU_EstablishFarcasterLink".Translate();
                 command_Action.icon = ContentFinder<Texture2D>.Get("UI/GU_ActivateSigmaAlcyon", true);
                 command_Action.action = delegate
                 {
                     EstablishFarcasterLink();
                     activatedBool = false;

                 };
                yield return command_Action;

             } else
             {
                 if (!confirmDeactivation)
                 {


                         Command_Action command_Action2 = new Command_Action();
                         command_Action2.defaultLabel = "GU_DeactivateFarcasterLink".Translate();
                         command_Action2.defaultDesc = "GU_DeactivateFarcasterLink".Translate();
                         command_Action2.icon = ContentFinder<Texture2D>.Get("UI/GU_DeactivateSigmaAlcyon", true);
                         command_Action2.action = delegate
                         {
                             confirmDeactivation = true;
                             Messages.Message("GU_FarcasterConfirmDeletion".Translate(), MessageTypeDefOf.PositiveEvent);


                         };
                        yield return command_Action2;
                }
                else
                 {
                         Command_Action command_Action3 = new Command_Action();
                         command_Action3.defaultLabel = "GU_DeactivateFarcasterLinkConfirm".Translate();
                         command_Action3.defaultDesc = "GU_DeactivateFarcasterLinkConfirm".Translate();
                         command_Action3.icon = ContentFinder<Texture2D>.Get("UI/GU_DeactivateSigmaAlcyon", true);
                         command_Action3.action = delegate
                         {
                             activatedBool = true;
                             confirmDeactivation = false;
                             DeactivateFarcasterLink();
                         };
                    yield return command_Action3;
                }
            }


            if (!activatedBool)
            {
                Command_Action command_Action4 = new Command_Action();
                command_Action4.defaultLabel = "GU_SendThingsToFarcastLocation".Translate();
                command_Action4.defaultDesc = "GU_SendThingsToFarcastLocationDesc".Translate();
                command_Action4.icon = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter", true);
                command_Action4.action = delegate
                {
                    SendThingsByFarcast();

                };
                yield return command_Action4;

            }


        }

        public void EstablishFarcasterLink()
        {
            mapHome = this.Map;
            Messages.Message("GU_FarcasterActivated".Translate(), MessageTypeDefOf.PositiveEvent);
            FarcasterDestination worldObjectFarcaster = (FarcasterDestination)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("GU_FarcasterDestinationSigmaAlcyon", true));
            mapParent = (MapParent)worldObjectFarcaster;
            ///mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.FactionBase);
            mapParent.Tile = TileFinder.RandomStartingTile();
            mapParent.SetFaction(Faction.OfPlayer);
            worldObjectFarcaster.mapHome = mapHome;
            worldObjectFarcaster.mapGen = DefDatabase<MapGeneratorDef>.GetNamed("GU_FarcasterMapSigmaAlcyon", true);
            Find.WorldObjects.Add(mapParent);
            Map mymap = new Map();
            mymap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, Find.World.info.initialMapSize, null);
            mymap.TileInfo.biome = DefDatabase<BiomeDef>.GetNamed("GU_SigmaAlcyonIIb", true);
            //Log.Message(mymap.Biome.ToString());
            mapParent.Tile = base.Tile;

            Building_AncientFarcaster building_AncientFarcaster = (Building_AncientFarcaster)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("GU_AncientFarcasterPortal", true));
            building_AncientFarcaster.mapHome = mapHome;
            building_AncientFarcaster.locationHome = this.Position;
            building_AncientFarcaster.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(building_AncientFarcaster, mymap.Center, mymap);
            locationFarcast = building_AncientFarcaster.Position;

            Building_Storage building_AncientPad = (Building_Storage)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("GU_AncientFarcasterPad", true));
            building_AncientPad.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(building_AncientPad, mymap.Center- GenAdj.CardinalDirections[0]*4, mymap);

            //Farcaster portal spawning ends here

        }

        public void DeactivateFarcasterLink()
        {
            Messages.Message("GU_FarcasterDectivated".Translate(), MessageTypeDefOf.PositiveEvent);
            Find.WorldObjects.Remove(mapParent);
        }

        public void SendThingsByFarcast()
        {



           /* foreach (IntVec3 current in this.OccupiedRect().Cells)
            {
                List<Thing> thingList = current.GetThingList(base.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] is Pawn || (thingList[i] is ThingWithComps && !(thingList[i] is Building)))
                    {
                        Thing expr_90 = thingList[i];
                        expr_90.DeSpawn();
                        GenSpawn.Spawn(expr_90, current, this.connectedMap);
                    }
                }
            }*/

        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (myPawn.RaceProps.Humanlike && base.Faction == Faction.OfPlayer && !activatedBool)
            {
                if (myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, TraverseMode.ByPawn))
                {
                    Action command_action_farcast = delegate
                    {
                        Job job = new Job(DefDatabase<JobDef>.GetNamed("GU_UseJuryRiggedFarcaster", true), this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                      
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GU_UseJuryRiggedFarcaster".Translate(), command_action_farcast, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");

                }



            }
        }
    

}
}
