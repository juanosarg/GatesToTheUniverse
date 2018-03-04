
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace GatesToTheUniverse
{
    class Building_Farcaster : Building_WorkTable
    {

        public override IEnumerable<Gizmo> GetGizmos()
        {
            
            IList<Gizmo> list = new List<Gizmo>();
            foreach (Gizmo current in base.GetGizmos())
            {
                list.Add(current);
            }

            Command_Action com = new Command_Action();
            com.icon = ContentFinder<Texture2D>.Get("UI/Commands/Detonate", true);
            com.defaultDesc = "CommandDetonateDesc".Translate();
            
            com.defaultLabel = "CommandDetonateLabel".Translate();

            list.Add(com);
            
            return list;
            
        }

    }
}
