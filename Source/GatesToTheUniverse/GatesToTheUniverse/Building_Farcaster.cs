
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using RimWorld.Planet;

namespace GatesToTheUniverse
{
    class Building_Farcaster : Building_WorkTable
    {
        
        private bool activatedBool = true;
        private bool confirmDeactivation = false;
        private MapParent mapParent;

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

        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
           

             if (activatedBool) { 
                 Command_Action command_Action = new Command_Action();
                 command_Action.defaultLabel = "Establish farcaster link";
                 command_Action.defaultDesc = "Establish farcaster link";
                 command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/AbandonHome", true);
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
                         command_Action2.defaultLabel = "Deactivate farcaster link";
                         command_Action2.defaultDesc = "Deactivate farcaster link";
                         command_Action2.icon = ContentFinder<Texture2D>.Get("UI/Commands/Detonate", true);
                         command_Action2.action = delegate
                         {
                             confirmDeactivation = true;
                             Messages.Message("Are you sure? Destination and everything in it will be lost forever. Click again to confirm", MessageTypeDefOf.PositiveEvent);


                         };
                        yield return command_Action2;
                }
                else
                 {
                         Command_Action command_Action3 = new Command_Action();
                         command_Action3.defaultLabel = "Deactivate farcaster link (confirm)";
                         command_Action3.defaultDesc = "Deactivate farcaster link (confirm)";
                         command_Action3.icon = ContentFinder<Texture2D>.Get("UI/Commands/Detonate", true);
                         command_Action3.action = delegate
                         {
                             activatedBool = true;
                             confirmDeactivation = false;
                             DeactivateFarcasterLink();
                         };
                    yield return command_Action3;
                }
            }



            

        }

        public void EstablishFarcasterLink()
        {
            Messages.Message("Farcaster activated", MessageTypeDefOf.PositiveEvent);
            mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("GU_FarcasterDestination", true));
            ///mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.FactionBase);
            mapParent.Tile = TileFinder.RandomStartingTile();
            mapParent.SetFaction(Faction.OfPlayer);
            Find.WorldObjects.Add(mapParent);
            Map mymap = new Map();
            mymap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, Find.World.info.initialMapSize, null);
            mymap.TileInfo.biome = DefDatabase<BiomeDef>.GetNamed("GU_Alien1", true);
            Log.Message(mymap.Biome.ToString());


            //Map mymap = MapGenerator.GenerateMap(Find.World.info.initialMapSize, mapParent, mapParent.MapGeneratorDef, mapParent.ExtraGenStepDefs, null);
            mapParent.Tile = base.Tile;
           

      





        }

        public void DeactivateFarcasterLink()
        {
            Messages.Message("Farcaster deactivated", MessageTypeDefOf.PositiveEvent);
            Find.WorldObjects.Remove(mapParent);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (myPawn.RaceProps.Humanlike && base.Faction == Faction.OfPlayer)
            {

                Action command_action_farcast = delegate
                {

                    myPawn.DeSpawn();
                    GenSpawn.Spawn(myPawn, myPawn.Position, mapParent.Map);
                };
               
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Use farcaster", command_action_farcast, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");

            }
        }
    

}
}
