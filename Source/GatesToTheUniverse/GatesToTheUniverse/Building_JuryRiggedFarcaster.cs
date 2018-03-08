
using RimWorld;
using Verse;
using Verse.Sound;
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
        public Map myMap;
        public Map mapHome;
        public Map mapFarcast;
        public IntVec3 locationFarcast;
        public SoundDef soundDef = DefDatabase<SoundDef>.GetNamed("GU_FarcasterSound", true);
        public BiomeDef originalBiome;
        public int originalTile;






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
            Scribe_References.Look<Map>(ref this.mapFarcast, "mapFarcast");
            Scribe_Values.Look<IntVec3>(ref this.locationFarcast, "locationFarcast");
            Scribe_Defs.Look<BiomeDef>(ref this.originalBiome, "originalBiome");
            Scribe_Values.Look<int>(ref this.originalTile, "originalTile");




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
                 command_Action.defaultDesc = "GU_EstablishFarcasterLinkSigmaAlcyonDesc".Translate();
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
                         command_Action2.defaultDesc = "GU_DeactivateFarcasterLinkSigmaAlcyonDesc".Translate();
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
                         command_Action3.defaultDesc = "GU_DeactivateFarcasterLinkSigmaAlcyonDesc".Translate();
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
                command_Action4.icon = ContentFinder<Texture2D>.Get("UI/GU_SendThingsToFarcastLocation", true);
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
            mapParent.Tile = TileFinder.RandomStartingTile();
            mapParent.SetFaction(Faction.OfPlayer);
            worldObjectFarcaster.mapHome = mapHome;
            worldObjectFarcaster.mapGen = DefDatabase<MapGeneratorDef>.GetNamed("GU_FarcasterMapSigmaAlcyon", true);
            Find.WorldObjects.Add(mapParent);
            myMap = new Map();
            myMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, Find.World.info.initialMapSize, null);
            originalBiome = myMap.TileInfo.biome;
            originalTile = mapParent.Tile;
            myMap.TileInfo.biome = DefDatabase<BiomeDef>.GetNamed("GU_SigmaAlcyonIIb", true);
            mapParent.Tile = base.Tile;

            mapFarcast = myMap;

            //Remote Farcaster portal spawning 

            Building_AncientFarcaster building_AncientFarcaster = (Building_AncientFarcaster)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("GU_AncientFarcasterPortal", true));
            building_AncientFarcaster.mapHome = mapHome;
            building_AncientFarcaster.locationHome = this.Position;
            building_AncientFarcaster.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(building_AncientFarcaster, myMap.Center, myMap);
            locationFarcast = building_AncientFarcaster.Position;

            //An auxiliary pad too

            Building_Storage building_AncientPad = (Building_Storage)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("GU_AncientFarcasterPad", true));
            building_AncientPad.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(building_AncientPad, myMap.Center- GenAdj.CardinalDirections[0]*4, myMap);

            //Farcaster portal spawning ends here

        }

        public void DeactivateFarcasterLink()
        {
            Messages.Message("GU_FarcasterDectivated".Translate(), MessageTypeDefOf.PositiveEvent);
            mapParent.Tile = originalTile;
            mapFarcast.TileInfo.biome = originalBiome;
            Find.WorldObjects.Remove(mapParent);
        }

        public void SendThingsByFarcast()
        {

            CellRect rect = GenAdj.OccupiedRect(this.Position, this.Rotation, this.def.size);
            rect = rect.ExpandedBy(4);
            soundDef.PlayOneShotOnCamera(null);

            foreach (IntVec3 current in rect.Cells)
            {
                Building edifice = current.GetEdifice(this.Map);
                if (edifice!=null&&((edifice.def.defName == "GU_FarcasterPad")||(edifice.def.defName == "GU_AncientFarcasterPad")))
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
                                GenSpawn.Spawn(thingToSend, mapFarcast.Center - GenAdj.CardinalDirections[0] * 4, mapFarcast);
                            }
                        }
                    }
                }
            }

            

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
