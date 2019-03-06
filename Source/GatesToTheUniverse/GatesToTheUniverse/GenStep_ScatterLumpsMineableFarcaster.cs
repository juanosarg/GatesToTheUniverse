﻿using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace GatesToTheUniverse
{
    public class GenStep_ScatterLumpsMineableFarcaster : GenStep_Scatterer
    {
        public ThingDef forcedDefToScatter;

        public int forcedLumpSize;

        [Unsaved]
        protected List<IntVec3> recentLumpCells = new List<IntVec3>();
        public override int SeedPart
        {
            get
            {
                return 826504673;
            }
        }
        public override void Generate(Map map, GenStepParams parms)
        {
            this.minSpacing = 5f;
            this.warnOnFail = false;
            int num = base.CalculateFinalCount(map);
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec;
                if (!this.TryFindScatterCell(map, out intVec))
                {
                    return;
                }
                this.ScatterAt(intVec, map, 1);
                this.usedSpots.Add(intVec);
            }
            this.usedSpots.Clear();
        }

        protected ThingDef ChooseThingDef()
        {
            if (this.forcedDefToScatter != null)
            {
                return this.forcedDefToScatter;
            }
            return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight(delegate (ThingDef d)
            {
                if (d.building == null)
                {
                    return 0f;
                }
                return 1f;
            });
        }

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (base.NearUsedSpot(c, this.minSpacing))
            {
                return false;
            }
            Building edifice = c.GetEdifice(map);
            return edifice != null && edifice.def.defName== "GU_RoseQuartz";
        }

        protected override void ScatterAt(IntVec3 c, Map map, int stackCount = 1)
        {
            ThingDef thingDef = this.ChooseThingDef();
            int numCells = (this.forcedLumpSize <= 0) ? thingDef.building.mineableScatterLumpSizeRange.RandomInRange : this.forcedLumpSize;
            this.recentLumpCells.Clear();
            foreach (IntVec3 current in GridShapeMaker.IrregularLump(c, map, numCells))
            {
                GenSpawn.Spawn(thingDef, current, map);
                this.recentLumpCells.Add(current);
            }
        }
    }
}

