﻿using System;
using Verse;

namespace RimWorld
{
	public class GenStep_Ambush_Edge : GenStep_Ambush
	{
		public GenStep_Ambush_Edge()
		{
		}

		public override int SeedPart
		{
			get
			{
				return 1412216193;
			}
		}

		protected override SignalAction_Ambush MakeAmbushSignalAction(CellRect rectToDefend, IntVec3 root, GenStepParams parms)
		{
			SignalAction_Ambush signalAction_Ambush = base.MakeAmbushSignalAction(rectToDefend, root, parms);
			signalAction_Ambush.spawnPawnsOnEdge = true;
			return signalAction_Ambush;
		}
	}
}
