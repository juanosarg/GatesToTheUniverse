
using System.Collections.Generic;
using RimWorld.Planet;
using System;
using Verse;


namespace GatesToTheUniverse
{
    class FarcasterDestination : FactionBase
    {
        public bool shouldBeDeleted;
        public Map mapHome;
        public MapGeneratorDef mapGen;


        public IntVec3 holeLocation;

        public int depth = -1;

        public override void ExposeData()
        {
            base.ExposeData();
           
            Scribe_Values.Look<int>(ref this.depth, "depth", -1, false);
        }


        public override MapGeneratorDef MapGeneratorDef
        {
            get
            {

                return mapGen;
            }
        }


        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            bool result = false;
            if (this.shouldBeDeleted)
            {
                result = true;
                alsoRemoveWorldObject = true;
            }
            return result;
        }
    }
}
