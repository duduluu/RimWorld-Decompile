﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x0200013A RID: 314
	internal class WorkGiver_CleanFilth : WorkGiver_Scanner
	{
		// Token: 0x04000313 RID: 787
		private int MinTicksSinceThickened = 600;

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000665 RID: 1637 RVA: 0x00042AA0 File Offset: 0x00040EA0
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x06000666 RID: 1638 RVA: 0x00042AB8 File Offset: 0x00040EB8
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Filth);
			}
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x06000667 RID: 1639 RVA: 0x00042AD4 File Offset: 0x00040ED4
		public override int LocalRegionsToScanFirst
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x00042AEC File Offset: 0x00040EEC
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerFilthInHomeArea.FilthInHomeArea;
		}

		// Token: 0x06000669 RID: 1641 RVA: 0x00042B14 File Offset: 0x00040F14
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			bool result;
			if (pawn.Faction != Faction.OfPlayer)
			{
				result = false;
			}
			else
			{
				Filth filth = t as Filth;
				if (filth == null)
				{
					result = false;
				}
				else if (!filth.Map.areaManager.Home[filth.Position])
				{
					result = false;
				}
				else
				{
					LocalTargetInfo target = t;
					result = (pawn.CanReserve(target, 1, -1, null, forced) && filth.TicksSinceThickened >= this.MinTicksSinceThickened);
				}
			}
			return result;
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x00042BB8 File Offset: 0x00040FB8
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Job job = new Job(JobDefOf.Clean);
			job.AddQueuedTarget(TargetIndex.A, t);
			int num = 15;
			Map map = t.Map;
			Room room = t.GetRoom(RegionType.Set_Passable);
			for (int i = 0; i < 100; i++)
			{
				IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
				{
					List<Thing> thingList = intVec.GetThingList(map);
					for (int j = 0; j < thingList.Count; j++)
					{
						Thing thing = thingList[j];
						if (this.HasJobOnThing(pawn, thing, forced) && thing != t)
						{
							job.AddQueuedTarget(TargetIndex.A, thing);
						}
					}
					if (job.GetTargetQueue(TargetIndex.A).Count >= num)
					{
						break;
					}
				}
			}
			if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
			{
				job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
			}
			return job;
		}
	}
}
