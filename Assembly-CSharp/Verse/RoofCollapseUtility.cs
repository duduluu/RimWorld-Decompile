﻿using System;

namespace Verse
{
	// Token: 0x02000C9D RID: 3229
	public static class RoofCollapseUtility
	{
		// Token: 0x0400305E RID: 12382
		public const float RoofMaxSupportDistance = 6.9f;

		// Token: 0x0400305F RID: 12383
		public static readonly int RoofSupportRadialCellsCount = GenRadial.NumCellsInRadius(6.9f);

		// Token: 0x06004728 RID: 18216 RVA: 0x00258CE0 File Offset: 0x002570E0
		public static bool WithinRangeOfRoofHolder(IntVec3 c, Map map, bool assumeNonNoRoofCellsAreRoofed = false)
		{
			bool connected = false;
			map.floodFiller.FloodFill(c, (IntVec3 x) => (x.Roofed(map) || x == c || (assumeNonNoRoofCellsAreRoofed && !map.areaManager.NoRoof[x])) && x.InHorDistOf(c, 6.9f), delegate(IntVec3 x)
			{
				for (int i = 0; i < 5; i++)
				{
					IntVec3 c2 = x + GenAdj.CardinalDirectionsAndInside[i];
					if (c2.InBounds(map) && c2.InHorDistOf(c, 6.9f))
					{
						Building edifice = c2.GetEdifice(map);
						if (edifice != null && edifice.def.holdsRoof)
						{
							connected = true;
							return true;
						}
					}
				}
				return false;
			}, int.MaxValue, false, null);
			return connected;
		}

		// Token: 0x06004729 RID: 18217 RVA: 0x00258D54 File Offset: 0x00257154
		public static bool ConnectedToRoofHolder(IntVec3 c, Map map, bool assumeRoofAtRoot)
		{
			bool connected = false;
			map.floodFiller.FloodFill(c, (IntVec3 x) => (x.Roofed(map) || (x == c && assumeRoofAtRoot)) && !connected, delegate(IntVec3 x)
			{
				for (int i = 0; i < 5; i++)
				{
					IntVec3 c2 = x + GenAdj.CardinalDirectionsAndInside[i];
					if (c2.InBounds(map))
					{
						Building edifice = c2.GetEdifice(map);
						if (edifice != null && edifice.def.holdsRoof)
						{
							connected = true;
							break;
						}
					}
				}
			}, int.MaxValue, false, null);
			return connected;
		}
	}
}
