using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GatesToTheUniverse
{
    class FarcasterStorylineController
    {
        public static int sentFirstLetterOnce = 0;

        public static void sendFirstLetter()
        {
            if(sentFirstLetterOnce == 0)
            {
                Find.LetterStack.ReceiveLetter("GU_FirstFarcastingLabel".Translate(), "GU_FirstFarcastingLetter".Translate(), LetterDefOf.PositiveEvent);
                sentFirstLetterOnce = 1;
            }
        }
    }
}
