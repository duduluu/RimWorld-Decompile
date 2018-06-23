﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	// Token: 0x020006BB RID: 1723
	public class SignalAction_Ambush : SignalAction
	{
		// Token: 0x04001484 RID: 5252
		public float points;

		// Token: 0x04001485 RID: 5253
		public bool manhunters;

		// Token: 0x04001486 RID: 5254
		public bool mechanoids;

		// Token: 0x04001487 RID: 5255
		public IntVec3 spawnNear = IntVec3.Invalid;

		// Token: 0x04001488 RID: 5256
		public CellRect spawnAround;

		// Token: 0x04001489 RID: 5257
		public bool spawnPawnsOnEdge;

		// Token: 0x0400148A RID: 5258
		private const int PawnsDelayAfterSpawnTicks = 120;

		// Token: 0x0600251F RID: 9503 RVA: 0x0013E704 File Offset: 0x0013CB04
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.points, "points", 0f, false);
			Scribe_Values.Look<bool>(ref this.manhunters, "manhunters", false, false);
			Scribe_Values.Look<bool>(ref this.mechanoids, "mechanoids", false, false);
			Scribe_Values.Look<IntVec3>(ref this.spawnNear, "spawnNear", default(IntVec3), false);
			Scribe_Values.Look<CellRect>(ref this.spawnAround, "spawnAround", default(CellRect), false);
			Scribe_Values.Look<bool>(ref this.spawnPawnsOnEdge, "spawnPawnsOnEdge", false, false);
		}

		// Token: 0x06002520 RID: 9504 RVA: 0x0013E798 File Offset: 0x0013CB98
		protected override void DoAction(object[] args)
		{
			if (this.points > 0f)
			{
				List<Pawn> list = new List<Pawn>();
				foreach (Pawn pawn in this.GenerateAmbushPawns())
				{
					IntVec3 loc;
					if (this.spawnPawnsOnEdge)
					{
						if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(base.Map) && !x.Fogged(base.Map) && base.Map.reachability.CanReachColony(x), base.Map, CellFinder.EdgeRoadChance_Ignore, out loc))
						{
							Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
							break;
						}
					}
					else if (!SiteGenStepUtility.TryFindSpawnCellAroundOrNear(this.spawnAround, this.spawnNear, base.Map, out loc))
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
						break;
					}
					GenSpawn.Spawn(pawn, loc, base.Map, WipeMode.Vanish);
					if (!this.spawnPawnsOnEdge)
					{
						for (int i = 0; i < 10; i++)
						{
							MoteMaker.ThrowAirPuffUp(pawn.DrawPos, base.Map);
						}
					}
					list.Add(pawn);
				}
				if (list.Any<Pawn>())
				{
					if (this.manhunters)
					{
						for (int j = 0; j < list.Count; j++)
						{
							list[j].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null, false);
						}
					}
					else
					{
						Faction faction = list[0].Faction;
						LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction, true, true, false, false, true), base.Map, list);
					}
					if (!this.spawnPawnsOnEdge)
					{
						for (int k = 0; k < list.Count; k++)
						{
							list[k].jobs.StartJob(new Job(JobDefOf.Wait, 120, false), JobCondition.None, null, false, true, null, null, false);
							list[k].Rotation = Rot4.Random;
						}
					}
					Find.LetterStack.ReceiveLetter("LetterLabelAmbushInExistingMap".Translate(), "LetterAmbushInExistingMap".Translate(new object[]
					{
						Faction.OfPlayer.def.pawnsPlural
					}).CapitalizeFirst(), LetterDefOf.ThreatBig, list[0], null, null);
				}
			}
		}

		// Token: 0x06002521 RID: 9505 RVA: 0x0013EA14 File Offset: 0x0013CE14
		private IEnumerable<Pawn> GenerateAmbushPawns()
		{
			IEnumerable<Pawn> result;
			if (this.manhunters)
			{
				PawnKindDef animalKind;
				if (!ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(this.points, base.Map.Tile, out animalKind) && !ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(this.points, -1, out animalKind))
				{
					result = Enumerable.Empty<Pawn>();
				}
				else
				{
					result = ManhunterPackIncidentUtility.GenerateAnimals(animalKind, base.Map.Tile, this.points);
				}
			}
			else
			{
				Faction faction;
				if (this.mechanoids)
				{
					faction = Faction.OfMechanoids;
				}
				else
				{
					faction = (base.Map.ParentFaction ?? Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Undefined));
				}
				if (faction == null)
				{
					result = Enumerable.Empty<Pawn>();
				}
				else
				{
					result = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
					{
						groupKind = PawnGroupKindDefOf.Combat,
						tile = base.Map.Tile,
						faction = faction,
						points = this.points
					}, true);
				}
			}
			return result;
		}
	}
}
