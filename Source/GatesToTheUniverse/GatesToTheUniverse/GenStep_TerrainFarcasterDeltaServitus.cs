using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse.Noise;
using Verse;
using System.Linq;
using UnityEngine;
using Verse.AI.Group;


namespace GatesToTheUniverse
{
    public class GenStep_TerrainFarcasterDeltaServitus : GenStep
    {

        private ModuleBase freqFactorNoise;
        private const float ThreshLooseRock = 0.55f;
        private const float PlaceProbabilityPerCell = 0.006f;
        private const float RubbleProbability = 0.5f;
        private BiomeDef biome1;
        private System.Random rand = new System.Random();
        private static bool debug_WarnedMissingTerrain;
        private const int MinRoofedCellsPerGroup = 20;
        private static Dictionary<ThingDef, int> numExtant = new Dictionary<ThingDef, int>();
        private static Dictionary<ThingDef, float> desiredProportions = new Dictionary<ThingDef, float>();
        private static int totalExtant = 0;
        private const float PlantMinGrowth = 0.07f;
        TerrainDef theBaseRocks;
        ThingDef theRockChunks;
        ThingDef theRocksThemselves;
        ThingDef rockRubble;




        public BiomeDef chooseABiome()
        {
            theBaseRocks = DefDatabase<TerrainDef>.GetNamed("GU_Piping", true);
            theRockChunks = DefDatabase<ThingDef>.GetNamed("ChunkSlagSteel", true);
            theRocksThemselves=DefDatabase<ThingDef>.GetNamed("GU_AncientMetals", true);
            rockRubble= DefDatabase<ThingDef>.GetNamed("FilthAsh", true);

            /*theBaseRocks = DefDatabase<TerrainDef>.GetNamed("GU_AncientMetals", true);
            theRockChunks = DefDatabase<ThingDef>.GetNamed("ChunkSlagSteel", true);
            theRocksThemselves=DefDatabase<ThingDef>.GetNamed("GU_RoseQuartz", true);*/

            return DefDatabase<BiomeDef>.GetNamed("GU_DeltaServitusIV", true);

        }

        public override void Generate(Map map)
        {
           
            List<IntVec3> list = new List<IntVec3>();
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            MapGenFloatGrid fertility = MapGenerator.Fertility;
            MapGenFloatGrid caves = MapGenerator.Caves;
            TerrainGrid terrainGrid = map.terrainGrid;
            biome1 = chooseABiome();
            foreach (IntVec3 current in map.AllCells)
            {
                Building edifice = current.GetEdifice(map);
                TerrainDef terrainDef;
                if ((edifice != null && edifice.def.Fillage == FillCategory.Full) || caves[current] > 0f)
                {
                    terrainDef = this.TerrainFrom(current, map, elevation[current], fertility[current], true);
                }
                else
                {
                    terrainDef = this.TerrainFrom(current, map, elevation[current], fertility[current], false);
                }
                if ((terrainDef == TerrainDefOf.WaterMovingShallow || terrainDef == TerrainDefOf.WaterMovingDeep) && edifice != null)
                {
                    list.Add(edifice.Position);
                    edifice.Destroy(DestroyMode.Vanish);
                }
                terrainGrid.SetTerrain(current, terrainDef);
            }
           
            RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
            //BeachMaker.Cleanup();
            foreach (TerrainPatchMaker current2 in biome1.terrainPatchMakers)
            {
                current2.Cleanup();
            }


            //Basic terrain gen ends here



            if (map.TileInfo.WaterCovered)
            {
                return;
            }
            map.regionAndRoomUpdater.Enabled = false;
            float num = 0.7f;
            List<GenStep_TerrainFarcasterDeltaServitus.RoofThreshold> list2 = new List<GenStep_TerrainFarcasterDeltaServitus.RoofThreshold>();
            list2.Add(new GenStep_TerrainFarcasterDeltaServitus.RoofThreshold
            {
                roofDef = RoofDefOf.RoofRockThick,
                minGridVal = num * 1.14f
            });
            list2.Add(new GenStep_TerrainFarcasterDeltaServitus.RoofThreshold
            {
                roofDef = RoofDefOf.RoofRockThin,
                minGridVal = num * 1.04f
            });
            //MapGenFloatGrid elevation3 = MapGenerator.Elevation;
            //MapGenFloatGrid caves2 = MapGenerator.Caves;
            foreach (IntVec3 current in map.AllCells)
            {
                float num2 = elevation[current];
                if (num2 > num)
                {
                    if (caves[current] <= 0f)
                    {
                        ThingDef def = RockDefAt(current);
                        if (map.Center.DistanceTo(current) > 5f) { 
                            GenSpawn.Spawn(def, current, map);
                        }
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (num2 > list2[i].minGridVal)
                        {
                            map.roofGrid.SetRoof(current, list2[i].roofDef);
                            break;
                        }
                    }
                }
            }
            BoolGrid visited = new BoolGrid(map);
            List<IntVec3> toRemove = new List<IntVec3>();
            foreach (IntVec3 current2 in map.AllCells)
            {
                if (!visited[current2])
                {
                    if (this.IsNaturalRoofAt(current2, map))
                    {
                        toRemove.Clear();
                        map.floodFiller.FloodFill(current2, (IntVec3 x) => this.IsNaturalRoofAt(x, map), delegate (IntVec3 x)
                        {
                            visited[x] = true;
                            toRemove.Add(x);
                        }, 2147483647, false, null);
                        if (toRemove.Count < 20)
                        {
                            for (int j = 0; j < toRemove.Count; j++)
                            {
                                map.roofGrid.SetRoof(toRemove[j], null);
                            }
                        }
                    }
                }
            }
            GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
            float num3 = 10f;
            switch (Find.WorldGrid[map.Tile].hilliness)
            {
                case Hilliness.Flat:
                    num3 = 4f;
                    break;
                case Hilliness.SmallHills:
                    num3 = 8f;
                    break;
                case Hilliness.LargeHills:
                    num3 = 11f;
                    break;
                case Hilliness.Mountainous:
                    num3 = 15f;
                    break;
                case Hilliness.Impassable:
                    num3 = 16f;
                    break;
            }
            genStep_ScatterLumpsMineable.countPer10kCellsRange = new FloatRange(num3, num3);
            genStep_ScatterLumpsMineable.Generate(map);
            map.regionAndRoomUpdater.Enabled = true;


            //Rock gen ends here

            map.regionAndRoomUpdater.Enabled = false;
            List<ThingDef> list3 = biome1.AllWildPlants.ToList<ThingDef>();
            for (int i = 0; i < list3.Count; i++)
            {
                GenStep_TerrainFarcasterDeltaServitus.numExtant.Add(list3[i], 0);
            }
            GenStep_TerrainFarcasterDeltaServitus.desiredProportions = GenPlant.CalculateDesiredPlantProportions(biome1);
            float num4 = biome1.plantDensity;
            foreach (IntVec3 c in map.AllCells.InRandomOrder(null))
            {
                if (c.GetEdifice(map) == null && c.GetCover(map) == null && caves[c] <= 0f)
                {
                    float num5 = map.fertilityGrid.FertilityAt(c);
                    float num6 = num4 * num5;
                    if (Rand.Value < num6)
                    {
                        IEnumerable<ThingDef> source = from def in list3
                                                       where def.CanEverPlantAt(c, map)
                                                       select def;
                        if (source.Any<ThingDef>())
                        {
                            ThingDef thingDef = source.RandomElementByWeight((ThingDef x) => PlantChoiceWeight(x, map));
                            int randomInRange = thingDef.plant.wildClusterSizeRange.RandomInRange;
                            for (int j = 0; j < randomInRange; j++)
                            {
                                IntVec3 c2;
                                if (j == 0)
                                {
                                    c2 = c;
                                }
                                else if (!GenPlantReproduction.TryFindReproductionDestination(c, thingDef, SeedTargFindMode.MapGenCluster, map, out c2))
                                {
                                    break;
                                }
                                Plant plant = (Plant)ThingMaker.MakeThing(thingDef, null);
                                plant.Growth = Rand.Range(0.07f, 1f);
                                if (plant.def.plant.LimitedLifespan)
                                {
                                    plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
                                }
                                GenSpawn.Spawn(plant, c2, map);
                                GenStep_TerrainFarcasterDeltaServitus.RecordAdded(thingDef);
                            }
                        }
                    }
                }
            }
            GenStep_TerrainFarcasterDeltaServitus.numExtant.Clear();
            GenStep_TerrainFarcasterDeltaServitus.desiredProportions.Clear();
            GenStep_TerrainFarcasterDeltaServitus.totalExtant = 0;
            map.regionAndRoomUpdater.Enabled = true;

            // Plant gen ends here
            
            int num7 = 0;
            while (!map.wildSpawner.AnimalEcosystemFull)
            {
                num7++;
                if (num7 >= 10)
                {
                    //Log.Error("Too many iterations.");
                    break;
                }
                IntVec3 loc = RCellFinder.RandomAnimalSpawnCell_MapGen(map);
                if (!SpawnRandomWildAnimalAt(map,loc))
                {
                    break;
                }
            }

            if (map.TileInfo.WaterCovered)
            {
                return;
            }
            this.freqFactorNoise = new Perlin(0.014999999664723873, 2.0, 0.5, 6, Rand.Range(0, 999999), QualityMode.Medium);
            this.freqFactorNoise = new ScaleBias(1.0, 1.0, this.freqFactorNoise);
            NoiseDebugUI.StoreNoiseRender(this.freqFactorNoise, "rock_chunks_freq_factor");
            MapGenFloatGrid elevation2 = MapGenerator.Elevation;
            foreach (IntVec3 current in map.AllCells)
            {
                float num2 = 0.006f * this.freqFactorNoise.GetValue(current);
                if (elevation2[current] < 0.55f && Rand.Value < num2)
                {
                    this.GrowLowRockFormationFrom(current, map);
                }
            }
            this.freqFactorNoise = null;
        }

        public bool SpawnRandomWildAnimalAt(Map map,IntVec3 loc)
        {
            PawnKindDef pawnKindDef = (from a in biome1.AllWildAnimals
                                       where map.mapTemperature.SeasonAcceptableFor(a.race)
                                       select a).RandomElementByWeight((PawnKindDef def) => biome1.CommonalityOfAnimal(def) / def.wildSpawn_GroupSizeRange.Average);
            if (pawnKindDef == null)
            {
                Log.Error("No spawnable animals right now.");
                return false;
            }
            int randomInRange = pawnKindDef.wildSpawn_GroupSizeRange.RandomInRange;
            int radius = Mathf.CeilToInt(Mathf.Sqrt((float)pawnKindDef.wildSpawn_GroupSizeRange.max));
            for (int i = 0; i < randomInRange; i++)
            {
                IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(loc, map, radius, null);
                Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionType);

                //Pawn newThing = PawnGenerator.GeneratePawn(pawnKindDef, faction);

                Pawn newPawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
                GenSpawn.Spawn(newPawn, loc2, map);
                if (faction != null && faction != Faction.OfPlayer)
                {
                    Lord lord = null;

                    LordJob_DefendPoint lordJob = new LordJob_DefendPoint(newPawn.Position);
                    lord = LordMaker.MakeNewLord(faction, lordJob, newPawn.Map, null);
                   /* if (newPawn.Map.mapPawns.SpawnedPawnsInFaction(faction).Any((Pawn p) => p != newPawn))
                    {
                        Pawn p2 = (Pawn)GenClosest.ClosestThing_Global(newPawn.Position, newPawn.Map.mapPawns.SpawnedPawnsInFaction(faction), 99999f, (Thing p) => p != newPawn && ((Pawn)p).GetLord() != null, null);
                        lord = p2.GetLord();
                    }
                    if (lord == null)
                    {
                       
                    }*/
                    lord.AddPawn(newPawn);
                }


                //GenSpawn.Spawn(newThing, loc2, map);
            }
            return true;
        }


        private float PlantChoiceWeight(ThingDef def, Map map)
        {
            float num = biome1.CommonalityOfPlant(def);
            if (GenStep_TerrainFarcasterDeltaServitus.totalExtant > 100)
            {
                float num2 = (float)GenStep_TerrainFarcasterDeltaServitus.numExtant[def] / (float)GenStep_TerrainFarcasterDeltaServitus.totalExtant;
                if (num2 < GenStep_TerrainFarcasterDeltaServitus.desiredProportions[def] * 0.8f)
                {
                    num *= 4f;
                }
            }
            return num / def.plant.wildClusterSizeRange.Average;
        }

        private static void RecordAdded(ThingDef plantDef)
        {
            GenStep_TerrainFarcasterDeltaServitus.totalExtant++;
            Dictionary<ThingDef, int> dictionary;
            (dictionary = GenStep_TerrainFarcasterDeltaServitus.numExtant)[plantDef] = dictionary[plantDef] + 1;
        }

        public TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, bool preferSolid)
        {
            TerrainDef terrainDef = null;
            TerrainDef terrainDef2 = null;

            if (terrainDef == null && preferSolid)
            {
                return GenStep_RocksFromGrid.RockDefAt(c).naturalTerrain;
            }
            //TerrainDef terrainDef2 = BeachMaker.BeachTerrainAt(c, map.Biome);
            if (terrainDef2 == TerrainDefOf.WaterOceanDeep)
            {
                return terrainDef2;
            }
            if (terrainDef == TerrainDefOf.WaterMovingShallow || terrainDef == TerrainDefOf.WaterMovingDeep)
            {
                return terrainDef;
            }
            if (terrainDef2 != null)
            {
                return terrainDef2;
            }
            if (terrainDef != null)
            {
                return terrainDef;
            }
            for (int i = 0; i < biome1.terrainPatchMakers.Count; i++)
            {
                terrainDef2 = biome1.terrainPatchMakers[i].TerrainAt(c, map);
                if (terrainDef2 != null)
                {
                    return terrainDef2;
                }
            }
           /* if (elevation > 0.55f && elevation < 0.61f)
            {
                return TerrainDefOf.Gravel;
            }*/
            if (elevation >= 0.61f)
            {
                return theBaseRocks;
               // return GenStep_RocksFromGrid.RockDefAt(c).naturalTerrain;
            }


            terrainDef2 = TerrainThreshold.TerrainAtValue(biome1.terrainsByFertility, fertility);

            if (terrainDef2 != null)
            {
                return terrainDef2;
            }
            if (!GenStep_TerrainFarcasterDeltaServitus.debug_WarnedMissingTerrain)
            {
                Log.Error(string.Concat(new object[]
                {
                    "No terrain found in biome ",
                    biome1.defName,
                    " for elevation=",
                    elevation,
                    ", fertility=",
                    fertility
                }));
                GenStep_TerrainFarcasterDeltaServitus.debug_WarnedMissingTerrain = true;
            }
            return TerrainDefOf.Sand;
        }

        private void GrowLowRockFormationFrom(IntVec3 root, Map map)
        {
            
            ThingDef mineableThing = null;
           // Log.Message(map.Biome.ToString());


            mineableThing = theRockChunks;
            
            Rot4 random = Rot4.Random;
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            IntVec3 intVec = root;
            while (true)
            {
                Rot4 random2 = Rot4.Random;
                if (!(random2 == random))
                {
                    intVec += random2.FacingCell;
                    if (!intVec.InBounds(map) || intVec.GetEdifice(map) != null || intVec.GetFirstItem(map) != null)
                    {
                        break;
                    }
                    if (elevation[intVec] > 0.55f)
                    {
                        return;
                    }
                    if (!map.terrainGrid.TerrainAt(intVec).affordances.Contains(TerrainAffordance.Heavy))
                    {
                        return;
                    }
                    GenSpawn.Spawn(mineableThing, intVec, map);
                    IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
                    for (int i = 0; i < adjacentCellsAndInside.Length; i++)
                    {
                        IntVec3 b = adjacentCellsAndInside[i];
                        if (Rand.Value < 0.5f)
                        {
                            IntVec3 c = intVec + b;
                            if (c.InBounds(map))
                            {
                                bool flag = false;
                                List<Thing> thingList = c.GetThingList(map);
                                for (int j = 0; j < thingList.Count; j++)
                                {
                                    Thing thing = thingList[j];
                                    if (thing.def.category != ThingCategory.Plant && thing.def.category != ThingCategory.Item && thing.def.category != ThingCategory.Pawn)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    FilthMaker.MakeFilth(c, map, rockRubble, 1);
                                }
                            }
                        }
                    }
                }
            }
        }



        private class RoofThreshold
        {
            public RoofDef roofDef;

            public float minGridVal;
        }


        public ThingDef RockDefAt(IntVec3 c)
        {
            ThingDef thingDef = null;
            /*float num = -999999f;
            for (int i = 0; i < RockNoises.rockNoises.Count; i++)
            {
                float value = RockNoises.rockNoises[i].noise.GetValue(c);
                if (value > num)
                {
                    thingDef = RockNoises.rockNoises[i].rockDef;
                    num = value;
                }
            }
            if (thingDef == null)
            {
                Log.ErrorOnce("Did not get rock def to generate at " + c, 50812);
                thingDef = ThingDefOf.Sandstone;
            }*/
            thingDef = theRocksThemselves;
            return thingDef;
        }

       

        private bool IsNaturalRoofAt(IntVec3 c, Map map)
        {
            return c.Roofed(map) && c.GetRoof(map).isNatural;
        }
    }
}
