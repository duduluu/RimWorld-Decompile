﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x020008FA RID: 2298
	public static class InfestationCellFinder
	{
		// Token: 0x04001CCF RID: 7375
		private static List<InfestationCellFinder.LocationCandidate> locationCandidates = new List<InfestationCellFinder.LocationCandidate>();

		// Token: 0x04001CD0 RID: 7376
		private static Dictionary<Region, float> regionsDistanceToUnroofed = new Dictionary<Region, float>();

		// Token: 0x04001CD1 RID: 7377
		private static ByteGrid closedAreaSize;

		// Token: 0x04001CD2 RID: 7378
		private static ByteGrid distToColonyBuilding;

		// Token: 0x04001CD3 RID: 7379
		private const float MinRequiredScore = 7.5f;

		// Token: 0x04001CD4 RID: 7380
		private const float MinMountainousnessScore = 0.17f;

		// Token: 0x04001CD5 RID: 7381
		private const int MountainousnessScoreRadialPatternIdx = 700;

		// Token: 0x04001CD6 RID: 7382
		private const int MountainousnessScoreRadialPatternSkip = 10;

		// Token: 0x04001CD7 RID: 7383
		private const float MountainousnessScorePerRock = 1f;

		// Token: 0x04001CD8 RID: 7384
		private const float MountainousnessScorePerThickRoof = 0.5f;

		// Token: 0x04001CD9 RID: 7385
		private const float MinCellTempToSpawnHive = -17f;

		// Token: 0x04001CDA RID: 7386
		private const float MaxDistanceToColonyBuilding = 30f;

		// Token: 0x04001CDB RID: 7387
		private static List<Pair<IntVec3, float>> tmpCachedInfestationChanceCellColors;

		// Token: 0x04001CDC RID: 7388
		private static HashSet<Region> tempUnroofedRegions = new HashSet<Region>();

		// Token: 0x04001CDD RID: 7389
		private static List<IntVec3> tmpColonyBuildingsLocs = new List<IntVec3>();

		// Token: 0x04001CDE RID: 7390
		private static List<KeyValuePair<IntVec3, float>> tmpDistanceResult = new List<KeyValuePair<IntVec3, float>>();

		// Token: 0x0600353D RID: 13629 RVA: 0x001C8284 File Offset: 0x001C6684
		public static bool TryFindCell(out IntVec3 cell, Map map)
		{
			InfestationCellFinder.CalculateLocationCandidates(map);
			InfestationCellFinder.LocationCandidate locationCandidate;
			bool result;
			if (!InfestationCellFinder.locationCandidates.TryRandomElementByWeight((InfestationCellFinder.LocationCandidate x) => x.score, out locationCandidate))
			{
				cell = IntVec3.Invalid;
				result = false;
			}
			else
			{
				cell = CellFinder.FindNoWipeSpawnLocNear(locationCandidate.cell, map, ThingDefOf.Hive, Rot4.North, 2, (IntVec3 x) => InfestationCellFinder.GetScoreAt(x, map) > 0f && x.GetFirstThing(map, ThingDefOf.Hive) == null && x.GetFirstThing(map, ThingDefOf.TunnelHiveSpawner) == null);
				result = true;
			}
			return result;
		}

		// Token: 0x0600353E RID: 13630 RVA: 0x001C8324 File Offset: 0x001C6724
		private static float GetScoreAt(IntVec3 cell, Map map)
		{
			float result;
			if ((float)InfestationCellFinder.distToColonyBuilding[cell] > 30f)
			{
				result = 0f;
			}
			else if (!cell.Walkable(map))
			{
				result = 0f;
			}
			else if (cell.Fogged(map))
			{
				result = 0f;
			}
			else if (InfestationCellFinder.CellHasBlockingThings(cell, map))
			{
				result = 0f;
			}
			else if (!cell.Roofed(map) || !cell.GetRoof(map).isThickRoof)
			{
				result = 0f;
			}
			else
			{
				Region region = cell.GetRegion(map, RegionType.Set_Passable);
				if (region == null)
				{
					result = 0f;
				}
				else if (InfestationCellFinder.closedAreaSize[cell] < 16)
				{
					result = 0f;
				}
				else
				{
					float temperature = cell.GetTemperature(map);
					if (temperature < -17f)
					{
						result = 0f;
					}
					else
					{
						float mountainousnessScoreAt = InfestationCellFinder.GetMountainousnessScoreAt(cell, map);
						if (mountainousnessScoreAt < 0.17f)
						{
							result = 0f;
						}
						else
						{
							int num = InfestationCellFinder.StraightLineDistToUnroofed(cell, map);
							float num2;
							if (!InfestationCellFinder.regionsDistanceToUnroofed.TryGetValue(region, out num2))
							{
								num2 = (float)num * 1.15f;
							}
							else
							{
								num2 = Mathf.Min(num2, (float)num * 4f);
							}
							num2 = Mathf.Pow(num2, 1.55f);
							float num3 = Mathf.InverseLerp(0f, 12f, (float)num);
							float num4 = Mathf.Lerp(1f, 0.18f, map.glowGrid.GameGlowAt(cell, false));
							float num5 = 1f - Mathf.Clamp(InfestationCellFinder.DistToBlocker(cell, map) / 11f, 0f, 0.6f);
							float num6 = Mathf.InverseLerp(-17f, -7f, temperature);
							float num7 = num2 * num3 * num5 * mountainousnessScoreAt * num4 * num6;
							num7 = Mathf.Pow(num7, 1.2f);
							if (num7 < 7.5f)
							{
								result = 0f;
							}
							else
							{
								result = num7;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0600353F RID: 13631 RVA: 0x001C8528 File Offset: 0x001C6928
		public static void DebugDraw()
		{
			if (DebugViewSettings.drawInfestationChance)
			{
				if (InfestationCellFinder.tmpCachedInfestationChanceCellColors == null)
				{
					InfestationCellFinder.tmpCachedInfestationChanceCellColors = new List<Pair<IntVec3, float>>();
				}
				if (Time.frameCount % 8 == 0)
				{
					InfestationCellFinder.tmpCachedInfestationChanceCellColors.Clear();
					Map currentMap = Find.CurrentMap;
					CellRect cellRect = Find.CameraDriver.CurrentViewRect;
					cellRect.ClipInsideMap(currentMap);
					cellRect = cellRect.ExpandedBy(1);
					InfestationCellFinder.CalculateTraversalDistancesToUnroofed(currentMap);
					InfestationCellFinder.CalculateClosedAreaSizeGrid(currentMap);
					InfestationCellFinder.CalculateDistanceToColonyBuildingGrid(currentMap);
					float num = 0.001f;
					for (int i = 0; i < currentMap.Size.z; i++)
					{
						for (int j = 0; j < currentMap.Size.x; j++)
						{
							IntVec3 cell = new IntVec3(j, 0, i);
							float scoreAt = InfestationCellFinder.GetScoreAt(cell, currentMap);
							if (scoreAt > num)
							{
								num = scoreAt;
							}
						}
					}
					for (int k = 0; k < currentMap.Size.z; k++)
					{
						for (int l = 0; l < currentMap.Size.x; l++)
						{
							IntVec3 intVec = new IntVec3(l, 0, k);
							if (cellRect.Contains(intVec))
							{
								float scoreAt2 = InfestationCellFinder.GetScoreAt(intVec, currentMap);
								if (scoreAt2 > 7.5f)
								{
									float second = GenMath.LerpDouble(7.5f, num, 0f, 1f, scoreAt2);
									InfestationCellFinder.tmpCachedInfestationChanceCellColors.Add(new Pair<IntVec3, float>(intVec, second));
								}
							}
						}
					}
				}
				for (int m = 0; m < InfestationCellFinder.tmpCachedInfestationChanceCellColors.Count; m++)
				{
					IntVec3 first = InfestationCellFinder.tmpCachedInfestationChanceCellColors[m].First;
					float second2 = InfestationCellFinder.tmpCachedInfestationChanceCellColors[m].Second;
					CellRenderer.RenderCell(first, SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 1f, second2), false));
				}
			}
			else
			{
				InfestationCellFinder.tmpCachedInfestationChanceCellColors = null;
			}
		}

		// Token: 0x06003540 RID: 13632 RVA: 0x001C8740 File Offset: 0x001C6B40
		private static void CalculateLocationCandidates(Map map)
		{
			InfestationCellFinder.locationCandidates.Clear();
			InfestationCellFinder.CalculateTraversalDistancesToUnroofed(map);
			InfestationCellFinder.CalculateClosedAreaSizeGrid(map);
			InfestationCellFinder.CalculateDistanceToColonyBuildingGrid(map);
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 cell = new IntVec3(j, 0, i);
					float scoreAt = InfestationCellFinder.GetScoreAt(cell, map);
					if (scoreAt > 0f)
					{
						InfestationCellFinder.locationCandidates.Add(new InfestationCellFinder.LocationCandidate(cell, scoreAt));
					}
				}
			}
		}

		// Token: 0x06003541 RID: 13633 RVA: 0x001C87E4 File Offset: 0x001C6BE4
		private static bool CellHasBlockingThings(IntVec3 cell, Map map)
		{
			List<Thing> thingList = cell.GetThingList(map);
			int i = 0;
			while (i < thingList.Count)
			{
				bool result;
				if (thingList[i] is Pawn || thingList[i] is Hive || thingList[i] is TunnelHiveSpawner)
				{
					result = true;
				}
				else
				{
					bool flag = thingList[i].def.category == ThingCategory.Building && thingList[i].def.passability == Traversability.Impassable;
					if (!flag || !GenSpawn.SpawningWipes(ThingDefOf.Hive, thingList[i].def))
					{
						i++;
						continue;
					}
					result = true;
				}
				return result;
			}
			return false;
		}

		// Token: 0x06003542 RID: 13634 RVA: 0x001C88AC File Offset: 0x001C6CAC
		private static int StraightLineDistToUnroofed(IntVec3 cell, Map map)
		{
			int num = int.MaxValue;
			int i = 0;
			while (i < 4)
			{
				Rot4 rot = new Rot4(i);
				IntVec3 facingCell = rot.FacingCell;
				int num2 = 0;
				int num3;
				for (;;)
				{
					IntVec3 intVec = cell + facingCell * num2;
					if (!intVec.InBounds(map))
					{
						goto Block_1;
					}
					num3 = num2;
					if (InfestationCellFinder.NoRoofAroundAndWalkable(intVec, map))
					{
						break;
					}
					num2++;
				}
				IL_74:
				if (num3 < num)
				{
					num = num3;
				}
				i++;
				continue;
				Block_1:
				num3 = int.MaxValue;
				goto IL_74;
			}
			int result;
			if (num == 2147483647)
			{
				result = map.Size.x;
			}
			else
			{
				result = num;
			}
			return result;
		}

		// Token: 0x06003543 RID: 13635 RVA: 0x001C8970 File Offset: 0x001C6D70
		private static float DistToBlocker(IntVec3 cell, Map map)
		{
			int num = int.MinValue;
			int num2 = int.MinValue;
			for (int i = 0; i < 4; i++)
			{
				Rot4 rot = new Rot4(i);
				IntVec3 facingCell = rot.FacingCell;
				int num3 = 0;
				int num4;
				for (;;)
				{
					IntVec3 c = cell + facingCell * num3;
					num4 = num3;
					if (!c.InBounds(map) || !c.Walkable(map))
					{
						break;
					}
					num3++;
				}
				if (num4 > num)
				{
					num2 = num;
					num = num4;
				}
				else if (num4 > num2)
				{
					num2 = num4;
				}
			}
			return (float)Mathf.Min(num, num2);
		}

		// Token: 0x06003544 RID: 13636 RVA: 0x001C8A28 File Offset: 0x001C6E28
		private static bool NoRoofAroundAndWalkable(IntVec3 cell, Map map)
		{
			bool result;
			if (!cell.Walkable(map))
			{
				result = false;
			}
			else if (cell.Roofed(map))
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					Rot4 rot = new Rot4(i);
					IntVec3 c = rot.FacingCell + cell;
					if (c.InBounds(map) && c.Roofed(map))
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06003545 RID: 13637 RVA: 0x001C8AB0 File Offset: 0x001C6EB0
		private static float GetMountainousnessScoreAt(IntVec3 cell, Map map)
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < 700; i += 10)
			{
				IntVec3 c = cell + GenRadial.RadialPattern[i];
				if (c.InBounds(map))
				{
					Building edifice = c.GetEdifice(map);
					if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isNaturalRock)
					{
						num += 1f;
					}
					else if (c.Roofed(map) && c.GetRoof(map).isThickRoof)
					{
						num += 0.5f;
					}
					num2++;
				}
			}
			return num / (float)num2;
		}

		// Token: 0x06003546 RID: 13638 RVA: 0x001C8B88 File Offset: 0x001C6F88
		private static void CalculateTraversalDistancesToUnroofed(Map map)
		{
			InfestationCellFinder.tempUnroofedRegions.Clear();
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					Region region = intVec.GetRegion(map, RegionType.Set_Passable);
					if (region != null && InfestationCellFinder.NoRoofAroundAndWalkable(intVec, map))
					{
						InfestationCellFinder.tempUnroofedRegions.Add(region);
					}
				}
			}
			Dijkstra<Region>.Run(InfestationCellFinder.tempUnroofedRegions, (Region x) => x.Neighbors, (Region a, Region b) => Mathf.Sqrt((float)a.extentsClose.CenterCell.DistanceToSquared(b.extentsClose.CenterCell)), InfestationCellFinder.regionsDistanceToUnroofed, null);
			InfestationCellFinder.tempUnroofedRegions.Clear();
		}

		// Token: 0x06003547 RID: 13639 RVA: 0x001C8C70 File Offset: 0x001C7070
		private static void CalculateClosedAreaSizeGrid(Map map)
		{
			if (InfestationCellFinder.closedAreaSize == null)
			{
				InfestationCellFinder.closedAreaSize = new ByteGrid(map);
			}
			else
			{
				InfestationCellFinder.closedAreaSize.ClearAndResizeTo(map);
			}
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					if (InfestationCellFinder.closedAreaSize[j, i] == 0 && !intVec.Impassable(map))
					{
						int area = 0;
						map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate(IntVec3 c)
						{
							area++;
						}, int.MaxValue, false, null);
						area = Mathf.Min(area, 255);
						map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate(IntVec3 c)
						{
							InfestationCellFinder.closedAreaSize[c] = (byte)area;
						}, int.MaxValue, false, null);
					}
				}
			}
		}

		// Token: 0x06003548 RID: 13640 RVA: 0x001C8DCC File Offset: 0x001C71CC
		private static void CalculateDistanceToColonyBuildingGrid(Map map)
		{
			if (InfestationCellFinder.distToColonyBuilding == null)
			{
				InfestationCellFinder.distToColonyBuilding = new ByteGrid(map);
			}
			else if (!InfestationCellFinder.distToColonyBuilding.MapSizeMatches(map))
			{
				InfestationCellFinder.distToColonyBuilding.ClearAndResizeTo(map);
			}
			InfestationCellFinder.distToColonyBuilding.Clear(byte.MaxValue);
			InfestationCellFinder.tmpColonyBuildingsLocs.Clear();
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				InfestationCellFinder.tmpColonyBuildingsLocs.Add(allBuildingsColonist[i].Position);
			}
			Dijkstra<IntVec3>.Run(InfestationCellFinder.tmpColonyBuildingsLocs, (IntVec3 x) => DijkstraUtility.AdjacentCellsNeighborsGetter(x, map), delegate(IntVec3 a, IntVec3 b)
			{
				float result;
				if (a.x == b.x || a.z == b.z)
				{
					result = 1f;
				}
				else
				{
					result = 1.41421354f;
				}
				return result;
			}, InfestationCellFinder.tmpDistanceResult, null);
			for (int j = 0; j < InfestationCellFinder.tmpDistanceResult.Count; j++)
			{
				InfestationCellFinder.distToColonyBuilding[InfestationCellFinder.tmpDistanceResult[j].Key] = (byte)Mathf.Min(InfestationCellFinder.tmpDistanceResult[j].Value, 254.999f);
			}
		}

		// Token: 0x020008FB RID: 2299
		private struct LocationCandidate
		{
			// Token: 0x04001CE3 RID: 7395
			public IntVec3 cell;

			// Token: 0x04001CE4 RID: 7396
			public float score;

			// Token: 0x0600354E RID: 13646 RVA: 0x001C9007 File Offset: 0x001C7407
			public LocationCandidate(IntVec3 cell, float score)
			{
				this.cell = cell;
				this.score = score;
			}
		}
	}
}
