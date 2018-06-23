﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	// Token: 0x02000496 RID: 1174
	[HasDebugOutput]
	public static class PawnUtility
	{
		// Token: 0x04000C89 RID: 3209
		private const float HumanFilthFactor = 4f;

		// Token: 0x04000C8A RID: 3210
		private static List<Pawn> tmpPawns = new List<Pawn>();

		// Token: 0x04000C8B RID: 3211
		private static List<string> tmpPawnKinds = new List<string>();

		// Token: 0x04000C8C RID: 3212
		private static HashSet<PawnKindDef> tmpAddedPawnKinds = new HashSet<PawnKindDef>();

		// Token: 0x04000C8D RID: 3213
		private const float RecruitDifficultyMin = 0.33f;

		// Token: 0x04000C8E RID: 3214
		private const float RecruitDifficultyMax = 0.99f;

		// Token: 0x04000C8F RID: 3215
		private const float RecruitDifficultyRandomOffset = 0.2f;

		// Token: 0x04000C90 RID: 3216
		private const float RecruitDifficultyOffsetPerTechDiff = 0.15f;

		// Token: 0x04000C91 RID: 3217
		private static List<Thing> tmpThings = new List<Thing>();

		// Token: 0x060014CC RID: 5324 RVA: 0x000B77A4 File Offset: 0x000B5BA4
		public static bool IsFactionLeader(Pawn pawn)
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (allFactionsListForReading[i].leader == pawn)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060014CD RID: 5325 RVA: 0x000B77F8 File Offset: 0x000B5BF8
		public static bool IsKidnappedPawn(Pawn pawn)
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (allFactionsListForReading[i].kidnapped.KidnappedPawnsListForReading.Contains(pawn))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060014CE RID: 5326 RVA: 0x000B7858 File Offset: 0x000B5C58
		public static bool IsTravelingInTransportPodWorldObject(Pawn pawn)
		{
			return ThingOwnerUtility.AnyParentIs<TravelingTransportPods>(pawn);
		}

		// Token: 0x060014CF RID: 5327 RVA: 0x000B7874 File Offset: 0x000B5C74
		public static bool ForSaleBySettlement(Pawn pawn)
		{
			return pawn.ParentHolder is Settlement_TraderTracker;
		}

		// Token: 0x060014D0 RID: 5328 RVA: 0x000B7897 File Offset: 0x000B5C97
		public static void TryDestroyStartingColonistFamily(Pawn pawn)
		{
			if (!pawn.relations.RelatedPawns.Any((Pawn x) => Find.GameInitData.startingAndOptionalPawns.Contains(x)))
			{
				PawnUtility.DestroyStartingColonistFamily(pawn);
			}
		}

		// Token: 0x060014D1 RID: 5329 RVA: 0x000B78D4 File Offset: 0x000B5CD4
		public static void DestroyStartingColonistFamily(Pawn pawn)
		{
			foreach (Pawn pawn2 in pawn.relations.RelatedPawns.ToList<Pawn>())
			{
				if (!Find.GameInitData.startingAndOptionalPawns.Contains(pawn2))
				{
					WorldPawnSituation situation = Find.WorldPawns.GetSituation(pawn2);
					if (situation == WorldPawnSituation.Free || situation == WorldPawnSituation.Dead)
					{
						Find.WorldPawns.RemovePawn(pawn2);
						Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
					}
				}
			}
		}

		// Token: 0x060014D2 RID: 5330 RVA: 0x000B7984 File Offset: 0x000B5D84
		public static bool EnemiesAreNearby(Pawn pawn, int regionsToScan = 9, bool passDoors = false)
		{
			TraverseParms tp = (!passDoors) ? TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false) : TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
			bool foundEnemy = false;
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(tp, false), delegate(Region r)
			{
				List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].HostileTo(pawn))
					{
						foundEnemy = true;
						return true;
					}
				}
				return foundEnemy;
			}, regionsToScan, RegionType.Set_Passable);
			return foundEnemy;
		}

		// Token: 0x060014D3 RID: 5331 RVA: 0x000B7A10 File Offset: 0x000B5E10
		public static bool WillSoonHaveBasicNeed(Pawn p)
		{
			return p.needs != null && ((p.needs.rest != null && p.needs.rest.CurLevel < 0.33f) || (p.needs.food != null && p.needs.food.CurLevelPercentage < p.needs.food.PercentageThreshHungry + 0.05f));
		}

		// Token: 0x060014D4 RID: 5332 RVA: 0x000B7AAC File Offset: 0x000B5EAC
		public static float AnimalFilthChancePerCell(ThingDef def, float bodySize)
		{
			float num = bodySize * 0.00125f;
			return num * (1f - def.race.petness);
		}

		// Token: 0x060014D5 RID: 5333 RVA: 0x000B7AE0 File Offset: 0x000B5EE0
		public static float HumanFilthChancePerCell(ThingDef def, float bodySize)
		{
			float num = bodySize * 0.00125f;
			return num * 4f;
		}

		// Token: 0x060014D6 RID: 5334 RVA: 0x000B7B08 File Offset: 0x000B5F08
		public static bool CanCasuallyInteractNow(this Pawn p, bool twoWayInteraction = false)
		{
			bool result;
			if (p.Drafted)
			{
				result = false;
			}
			else if (ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(p))
			{
				result = false;
			}
			else if (p.InAggroMentalState)
			{
				result = false;
			}
			else if (!p.Awake())
			{
				result = false;
			}
			else
			{
				Job curJob = p.CurJob;
				if (curJob != null)
				{
					if (twoWayInteraction)
					{
						if (!curJob.def.casualInterruptible || !curJob.playerForced)
						{
							return false;
						}
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x060014D7 RID: 5335 RVA: 0x000B7BA0 File Offset: 0x000B5FA0
		public static IEnumerable<Pawn> SpawnedMasteredPawns(Pawn master)
		{
			if (Current.ProgramState != ProgramState.Playing || master.Faction == null || !master.RaceProps.Humanlike)
			{
				yield break;
			}
			if (!master.Spawned)
			{
				yield break;
			}
			List<Pawn> pawns = master.Map.mapPawns.SpawnedPawnsInFaction(master.Faction);
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].playerSettings != null && pawns[i].playerSettings.Master == master)
				{
					yield return pawns[i];
				}
			}
			yield break;
		}

		// Token: 0x060014D8 RID: 5336 RVA: 0x000B7BCC File Offset: 0x000B5FCC
		public static bool InValidState(Pawn p)
		{
			bool result;
			if (p.health == null)
			{
				result = false;
			}
			else
			{
				if (!p.Dead)
				{
					if (p.stances == null || p.mindState == null || p.needs == null || p.ageTracker == null)
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x060014D9 RID: 5337 RVA: 0x000B7C34 File Offset: 0x000B6034
		public static PawnPosture GetPosture(this Pawn p)
		{
			PawnPosture result;
			if (p.Dead)
			{
				result = PawnPosture.LayingOnGroundNormal;
			}
			else if (p.Downed)
			{
				if (p.jobs != null && p.jobs.posture.Laying())
				{
					result = p.jobs.posture;
				}
				else
				{
					result = PawnPosture.LayingOnGroundNormal;
				}
			}
			else if (p.jobs == null)
			{
				result = PawnPosture.Standing;
			}
			else
			{
				result = p.jobs.posture;
			}
			return result;
		}

		// Token: 0x060014DA RID: 5338 RVA: 0x000B7CBC File Offset: 0x000B60BC
		public static void ForceWait(Pawn pawn, int ticks, Thing faceTarget = null, bool maintainPosture = false)
		{
			if (ticks <= 0)
			{
				Log.ErrorOnce("Forcing a wait for zero ticks", 47045639, false);
			}
			Job job = new Job((!maintainPosture) ? JobDefOf.Wait : JobDefOf.Wait_MaintainPosture, faceTarget);
			job.expiryInterval = ticks;
			pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, true, true, null, null, false);
		}

		// Token: 0x060014DB RID: 5339 RVA: 0x000B7D24 File Offset: 0x000B6124
		public static void GiveNameBecauseOfNuzzle(Pawn namer, Pawn namee)
		{
			string text = (namee.Name != null) ? namee.Name.ToStringFull : namee.LabelIndefinite();
			namee.Name = PawnBioAndNameGenerator.GeneratePawnName(namee, NameStyle.Full, null);
			if (namer.Faction == Faction.OfPlayer)
			{
				Messages.Message("MessageNuzzledPawnGaveNameTo".Translate(new object[]
				{
					namer,
					text,
					namee.Name.ToStringFull
				}), namee, MessageTypeDefOf.NeutralEvent, true);
			}
		}

		// Token: 0x060014DC RID: 5340 RVA: 0x000B7DAC File Offset: 0x000B61AC
		public static void GainComfortFromCellIfPossible(this Pawn p)
		{
			if (Find.TickManager.TicksGame % 10 == 0)
			{
				Building edifice = p.Position.GetEdifice(p.Map);
				if (edifice != null)
				{
					float statValue = edifice.GetStatValue(StatDefOf.Comfort, true);
					if (statValue >= 0f && p.needs != null && p.needs.comfort != null)
					{
						p.needs.comfort.ComfortUsed(statValue);
					}
				}
			}
		}

		// Token: 0x060014DD RID: 5341 RVA: 0x000B7E2C File Offset: 0x000B622C
		public static float BodyResourceGrowthSpeed(Pawn pawn)
		{
			if (pawn.needs != null && pawn.needs.food != null)
			{
				switch (pawn.needs.food.CurCategory)
				{
				case HungerCategory.Fed:
					return 1f;
				case HungerCategory.Hungry:
					return 0.666f;
				case HungerCategory.UrgentlyHungry:
					return 0.333f;
				case HungerCategory.Starving:
					return 0f;
				}
			}
			return 1f;
		}

		// Token: 0x060014DE RID: 5342 RVA: 0x000B7EBC File Offset: 0x000B62BC
		public static bool FertileMateTarget(Pawn male, Pawn female)
		{
			bool result;
			if (female.gender != Gender.Female || !female.ageTracker.CurLifeStage.reproductive)
			{
				result = false;
			}
			else
			{
				CompEggLayer compEggLayer = female.TryGetComp<CompEggLayer>();
				if (compEggLayer != null)
				{
					result = !compEggLayer.FullyFertilized;
				}
				else
				{
					result = !female.health.hediffSet.HasHediff(HediffDefOf.Pregnant, false);
				}
			}
			return result;
		}

		// Token: 0x060014DF RID: 5343 RVA: 0x000B7F30 File Offset: 0x000B6330
		public static void Mated(Pawn male, Pawn female)
		{
			if (female.ageTracker.CurLifeStage.reproductive)
			{
				CompEggLayer compEggLayer = female.TryGetComp<CompEggLayer>();
				if (compEggLayer != null)
				{
					compEggLayer.Fertilize(male);
				}
				else if (Rand.Value < 0.5f && !female.health.hediffSet.HasHediff(HediffDefOf.Pregnant, false))
				{
					Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.Pregnant, female, null);
					hediff_Pregnant.father = male;
					female.health.AddHediff(hediff_Pregnant, null, null, null);
				}
			}
		}

		// Token: 0x060014E0 RID: 5344 RVA: 0x000B7FD4 File Offset: 0x000B63D4
		public static bool PlayerForcedJobNowOrSoon(Pawn pawn)
		{
			bool result;
			if (pawn.jobs == null)
			{
				result = false;
			}
			else
			{
				Job curJob = pawn.CurJob;
				if (curJob != null)
				{
					result = curJob.playerForced;
				}
				else
				{
					result = (pawn.jobs.jobQueue.Any<QueuedJob>() && pawn.jobs.jobQueue.Peek().job.playerForced);
				}
			}
			return result;
		}

		// Token: 0x060014E1 RID: 5345 RVA: 0x000B804C File Offset: 0x000B644C
		public static bool TrySpawnHatchedOrBornPawn(Pawn pawn, Thing motherOrEgg)
		{
			bool result;
			if (motherOrEgg.SpawnedOrAnyParentSpawned)
			{
				result = (GenSpawn.Spawn(pawn, motherOrEgg.PositionHeld, motherOrEgg.MapHeld, WipeMode.Vanish) != null);
			}
			else
			{
				Pawn pawn2 = motherOrEgg as Pawn;
				if (pawn2 != null)
				{
					if (pawn2.IsCaravanMember())
					{
						pawn2.GetCaravan().AddPawn(pawn, true);
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
						return true;
					}
					if (pawn2.IsWorldPawn())
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
						return true;
					}
				}
				else if (motherOrEgg.ParentHolder != null)
				{
					Pawn_InventoryTracker pawn_InventoryTracker = motherOrEgg.ParentHolder as Pawn_InventoryTracker;
					if (pawn_InventoryTracker != null)
					{
						if (pawn_InventoryTracker.pawn.IsCaravanMember())
						{
							pawn_InventoryTracker.pawn.GetCaravan().AddPawn(pawn, true);
							Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
							return true;
						}
						if (pawn_InventoryTracker.pawn.IsWorldPawn())
						{
							Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
							return true;
						}
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x060014E2 RID: 5346 RVA: 0x000B8168 File Offset: 0x000B6568
		public static ByteGrid GetAvoidGrid(this Pawn p)
		{
			ByteGrid result;
			if (p.Faction == null)
			{
				result = null;
			}
			else if (!p.Faction.def.canUseAvoidGrid)
			{
				result = null;
			}
			else if (p.Faction == Faction.OfPlayer || !p.Faction.HostileTo(Faction.OfPlayer))
			{
				result = null;
			}
			else
			{
				Lord lord = p.GetLord();
				if (lord != null)
				{
					if (lord.CurLordToil.avoidGridMode == AvoidGridMode.Ignore)
					{
						return null;
					}
					if (lord.CurLordToil.avoidGridMode == AvoidGridMode.Basic)
					{
						return p.Faction.GetAvoidGridBasic(p.Map);
					}
					if (lord.CurLordToil.avoidGridMode == AvoidGridMode.Smart)
					{
						return p.Faction.GetAvoidGridSmart(p.Map);
					}
				}
				result = p.Faction.GetAvoidGridBasic(p.Map);
			}
			return result;
		}

		// Token: 0x060014E3 RID: 5347 RVA: 0x000B8260 File Offset: 0x000B6660
		public static bool ShouldCollideWithPawns(Pawn p)
		{
			return !p.Downed && !p.Dead && p.mindState.anyCloseHostilesRecently;
		}

		// Token: 0x060014E4 RID: 5348 RVA: 0x000B82AC File Offset: 0x000B66AC
		public static bool AnyPawnBlockingPathAt(IntVec3 c, Pawn forPawn, bool actAsIfHadCollideWithPawnsJob = false, bool collideOnlyWithStandingPawns = false, bool forPathFinder = false)
		{
			return PawnUtility.PawnBlockingPathAt(c, forPawn, actAsIfHadCollideWithPawnsJob, collideOnlyWithStandingPawns, forPathFinder) != null;
		}

		// Token: 0x060014E5 RID: 5349 RVA: 0x000B82D4 File Offset: 0x000B66D4
		public static Pawn PawnBlockingPathAt(IntVec3 c, Pawn forPawn, bool actAsIfHadCollideWithPawnsJob = false, bool collideOnlyWithStandingPawns = false, bool forPathFinder = false)
		{
			List<Thing> thingList = c.GetThingList(forPawn.Map);
			Pawn result;
			if (thingList.Count == 0)
			{
				result = null;
			}
			else
			{
				bool flag = false;
				if (actAsIfHadCollideWithPawnsJob)
				{
					flag = true;
				}
				else
				{
					Job curJob = forPawn.CurJob;
					if (curJob != null && (curJob.collideWithPawns || curJob.def.collideWithPawns || forPawn.jobs.curDriver.collideWithPawns))
					{
						flag = true;
					}
					else if (forPawn.Drafted && forPawn.pather.Moving)
					{
						flag = true;
					}
				}
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn = thingList[i] as Pawn;
					if (pawn != null && pawn != forPawn && !pawn.Downed)
					{
						if (collideOnlyWithStandingPawns)
						{
							if (pawn.pather.MovingNow)
							{
								goto IL_1BC;
							}
							if (pawn.pather.Moving && pawn.pather.MovedRecently(60))
							{
								goto IL_1BC;
							}
						}
						if (!PawnUtility.PawnsCanShareCellBecauseOfBodySize(pawn, forPawn))
						{
							if (pawn.HostileTo(forPawn))
							{
								return pawn;
							}
							if (flag)
							{
								if (forPathFinder || !forPawn.Drafted || !pawn.RaceProps.Animal)
								{
									Job curJob2 = pawn.CurJob;
									if (curJob2 != null && (curJob2.collideWithPawns || curJob2.def.collideWithPawns || pawn.jobs.curDriver.collideWithPawns))
									{
										return pawn;
									}
								}
							}
						}
					}
					IL_1BC:;
				}
				result = null;
			}
			return result;
		}

		// Token: 0x060014E6 RID: 5350 RVA: 0x000B84B8 File Offset: 0x000B68B8
		private static bool PawnsCanShareCellBecauseOfBodySize(Pawn p1, Pawn p2)
		{
			bool result;
			if (p1.BodySize >= 1.5f || p2.BodySize >= 1.5f)
			{
				result = false;
			}
			else
			{
				float num = p1.BodySize / p2.BodySize;
				if (num < 1f)
				{
					num = 1f / num;
				}
				result = (num > 3.57f);
			}
			return result;
		}

		// Token: 0x060014E7 RID: 5351 RVA: 0x000B8520 File Offset: 0x000B6920
		public static bool KnownDangerAt(IntVec3 c, Map map, Pawn forPawn)
		{
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice.IsDangerousFor(forPawn);
		}

		// Token: 0x060014E8 RID: 5352 RVA: 0x000B8550 File Offset: 0x000B6950
		public static bool ShouldSendNotificationAbout(Pawn p)
		{
			bool result;
			if (Current.ProgramState != ProgramState.Playing)
			{
				result = false;
			}
			else if (PawnGenerator.IsBeingGenerated(p))
			{
				result = false;
			}
			else if (p.IsWorldPawn() && (!p.IsCaravanMember() || !p.GetCaravan().IsPlayerControlled) && !PawnUtility.IsTravelingInTransportPodWorldObject(p) && p.Corpse.DestroyedOrNull())
			{
				result = false;
			}
			else
			{
				if (p.Faction != Faction.OfPlayer)
				{
					if (p.HostFaction != Faction.OfPlayer)
					{
						return false;
					}
					if (p.RaceProps.Humanlike && p.guest.Released && !p.Downed && !p.InBed())
					{
						return false;
					}
					if (p.CurJob != null && p.CurJob.exitMapOnArrival && !PrisonBreakUtility.IsPrisonBreaking(p))
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x060014E9 RID: 5353 RVA: 0x000B8668 File Offset: 0x000B6A68
		public static bool ShouldGetThoughtAbout(Pawn pawn, Pawn subject)
		{
			return pawn.Faction == subject.Faction || (!subject.IsWorldPawn() && !pawn.IsWorldPawn());
		}

		// Token: 0x060014EA RID: 5354 RVA: 0x000B86A8 File Offset: 0x000B6AA8
		public static bool IsTeetotaler(this Pawn pawn)
		{
			return pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) < 0;
		}

		// Token: 0x060014EB RID: 5355 RVA: 0x000B86E4 File Offset: 0x000B6AE4
		public static bool IsProsthophobe(this Pawn pawn)
		{
			return pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.BodyPurist);
		}

		// Token: 0x060014EC RID: 5356 RVA: 0x000B871C File Offset: 0x000B6B1C
		public static string PawnKindsToCommaList(List<Pawn> pawns, bool useAnd = false)
		{
			PawnUtility.tmpPawns.Clear();
			PawnUtility.tmpPawns.AddRange(pawns);
			if (PawnUtility.tmpPawns.Count >= 2)
			{
				PawnUtility.tmpPawns.SortBy((Pawn x) => !x.RaceProps.Humanlike, (Pawn x) => x.GetKindLabelPlural(-1));
			}
			PawnUtility.tmpAddedPawnKinds.Clear();
			PawnUtility.tmpPawnKinds.Clear();
			for (int i = 0; i < PawnUtility.tmpPawns.Count; i++)
			{
				if (!PawnUtility.tmpAddedPawnKinds.Contains(PawnUtility.tmpPawns[i].kindDef))
				{
					PawnUtility.tmpAddedPawnKinds.Add(PawnUtility.tmpPawns[i].kindDef);
					int num = 0;
					for (int j = 0; j < PawnUtility.tmpPawns.Count; j++)
					{
						if (PawnUtility.tmpPawns[j].kindDef == PawnUtility.tmpPawns[i].kindDef)
						{
							num++;
						}
					}
					if (num == 1)
					{
						PawnUtility.tmpPawnKinds.Add("1 " + PawnUtility.tmpPawns[i].KindLabel);
					}
					else
					{
						PawnUtility.tmpPawnKinds.Add(num + " " + PawnUtility.tmpPawns[i].GetKindLabelPlural(num));
					}
				}
			}
			return PawnUtility.tmpPawnKinds.ToCommaList(useAnd);
		}

		// Token: 0x060014ED RID: 5357 RVA: 0x000B88B8 File Offset: 0x000B6CB8
		public static LocomotionUrgency ResolveLocomotion(Pawn pawn, LocomotionUrgency secondPriority)
		{
			LocomotionUrgency result;
			if (!pawn.Dead && pawn.mindState.duty != null && pawn.mindState.duty.locomotion != LocomotionUrgency.None)
			{
				result = pawn.mindState.duty.locomotion;
			}
			else
			{
				result = secondPriority;
			}
			return result;
		}

		// Token: 0x060014EE RID: 5358 RVA: 0x000B8914 File Offset: 0x000B6D14
		public static LocomotionUrgency ResolveLocomotion(Pawn pawn, LocomotionUrgency secondPriority, LocomotionUrgency thirdPriority)
		{
			LocomotionUrgency locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, secondPriority);
			LocomotionUrgency result;
			if (locomotionUrgency != LocomotionUrgency.None)
			{
				result = locomotionUrgency;
			}
			else
			{
				result = thirdPriority;
			}
			return result;
		}

		// Token: 0x060014EF RID: 5359 RVA: 0x000B8940 File Offset: 0x000B6D40
		public static Danger ResolveMaxDanger(Pawn pawn, Danger secondPriority)
		{
			Danger result;
			if (!pawn.Dead && pawn.mindState.duty != null && pawn.mindState.duty.maxDanger != Danger.Unspecified)
			{
				result = pawn.mindState.duty.maxDanger;
			}
			else
			{
				result = secondPriority;
			}
			return result;
		}

		// Token: 0x060014F0 RID: 5360 RVA: 0x000B899C File Offset: 0x000B6D9C
		public static Danger ResolveMaxDanger(Pawn pawn, Danger secondPriority, Danger thirdPriority)
		{
			Danger danger = PawnUtility.ResolveMaxDanger(pawn, secondPriority);
			Danger result;
			if (danger != Danger.Unspecified)
			{
				result = danger;
			}
			else
			{
				result = thirdPriority;
			}
			return result;
		}

		// Token: 0x060014F1 RID: 5361 RVA: 0x000B89C8 File Offset: 0x000B6DC8
		public static bool IsFighting(this Pawn pawn)
		{
			return pawn.CurJob != null && (pawn.CurJob.def == JobDefOf.AttackMelee || pawn.CurJob.def == JobDefOf.AttackStatic || pawn.CurJob.def == JobDefOf.Wait_Combat || pawn.CurJob.def == JobDefOf.PredatorHunt);
		}

		// Token: 0x060014F2 RID: 5362 RVA: 0x000B8A40 File Offset: 0x000B6E40
		public static float RecruitDifficulty(this Pawn pawn, Faction recruiterFaction, bool withPopIntent)
		{
			float num = pawn.kindDef.baseRecruitDifficulty;
			Rand.PushState();
			Rand.Seed = pawn.HashOffset();
			num += Rand.Range(-0.2f, 0.2f);
			Rand.PopState();
			if (pawn.Faction != null)
			{
				int num2 = Mathf.Abs((int)(pawn.Faction.def.techLevel - recruiterFaction.def.techLevel));
				num += (float)num2 * 0.15f;
			}
			if (withPopIntent)
			{
				float popIntent = (Current.ProgramState != ProgramState.Playing) ? 1f : Find.Storyteller.intenderPopulation.PopulationIntent;
				num = PawnUtility.PopIntentAdjustedRecruitDifficulty(num, popIntent);
			}
			return Mathf.Clamp(num, 0.33f, 0.99f);
		}

		// Token: 0x060014F3 RID: 5363 RVA: 0x000B8B0C File Offset: 0x000B6F0C
		private static float PopIntentAdjustedRecruitDifficulty(float baseDifficulty, float popIntent)
		{
			float num = Mathf.Clamp(popIntent, 0.25f, 3f);
			return 1f - (1f - baseDifficulty) * num;
		}

		// Token: 0x060014F4 RID: 5364 RVA: 0x000B8B44 File Offset: 0x000B6F44
		[DebugOutput]
		public static void PopIntentRecruitDifficulty()
		{
			List<float> list = new List<float>();
			for (float num = -1f; num < 3f; num += 0.1f)
			{
				list.Add(num);
			}
			List<float> colValues = new List<float>
			{
				0.1f,
				0.2f,
				0.3f,
				0.4f,
				0.5f,
				0.6f,
				0.7f,
				0.8f,
				0.9f,
				0.95f,
				0.99f
			};
			DebugTables.MakeTablesDialog<float, float>(colValues, (float d) => "d=" + d.ToString("F0"), list, (float rv) => rv.ToString("F1"), (float d, float pi) => PawnUtility.PopIntentAdjustedRecruitDifficulty(d, pi).ToStringPercent(), "intents");
		}

		// Token: 0x060014F5 RID: 5365 RVA: 0x000B8C64 File Offset: 0x000B7064
		public static void GiveAllStartingPlayerPawnsThought(ThoughtDef thought)
		{
			foreach (Pawn pawn in Find.GameInitData.startingAndOptionalPawns)
			{
				if (thought.IsSocial)
				{
					foreach (Pawn pawn2 in Find.GameInitData.startingAndOptionalPawns)
					{
						if (pawn2 != pawn)
						{
							pawn.needs.mood.thoughts.memories.TryGainMemory(thought, pawn2);
						}
					}
				}
				else
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(thought, null);
				}
			}
		}

		// Token: 0x060014F6 RID: 5366 RVA: 0x000B8D60 File Offset: 0x000B7160
		public static IntVec3 DutyLocation(this Pawn pawn)
		{
			IntVec3 result;
			if (pawn.mindState.duty != null && pawn.mindState.duty.focus.IsValid)
			{
				result = pawn.mindState.duty.focus.Cell;
			}
			else
			{
				result = pawn.Position;
			}
			return result;
		}

		// Token: 0x060014F7 RID: 5367 RVA: 0x000B8DC0 File Offset: 0x000B71C0
		public static bool EverBeenColonistOrTameAnimal(Pawn pawn)
		{
			return pawn.records.GetAsInt(RecordDefOf.TimeAsColonistOrColonyAnimal) > 0;
		}

		// Token: 0x060014F8 RID: 5368 RVA: 0x000B8DE8 File Offset: 0x000B71E8
		public static bool EverBeenPrisoner(Pawn pawn)
		{
			return pawn.records.GetAsInt(RecordDefOf.TimeAsPrisoner) > 0;
		}

		// Token: 0x060014F9 RID: 5369 RVA: 0x000B8E10 File Offset: 0x000B7210
		public static void RecoverFromUnwalkablePositionOrKill(IntVec3 c, Map map)
		{
			if (c.InBounds(map) && !c.Walkable(map))
			{
				PawnUtility.tmpThings.Clear();
				PawnUtility.tmpThings.AddRange(c.GetThingList(map));
				for (int i = 0; i < PawnUtility.tmpThings.Count; i++)
				{
					Pawn pawn = PawnUtility.tmpThings[i] as Pawn;
					if (pawn != null)
					{
						IntVec3 position;
						if (CellFinder.TryFindBestPawnStandCell(pawn, out position, false))
						{
							pawn.Position = position;
							pawn.Notify_Teleported(true);
						}
						else
						{
							DamageDef crush = DamageDefOf.Crush;
							float amount = 99999f;
							BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
							DamageInfo damageInfo = new DamageInfo(crush, amount, -1f, null, brain, null, DamageInfo.SourceCategory.Collapse, null);
							pawn.TakeDamage(damageInfo);
							if (!pawn.Dead)
							{
								pawn.Kill(new DamageInfo?(damageInfo), null);
							}
						}
					}
				}
			}
		}

		// Token: 0x060014FA RID: 5370 RVA: 0x000B8F08 File Offset: 0x000B7308
		public static float GetBaseManhunterOnDamageChance(Pawn pawn)
		{
			return PawnUtility.GetBaseManhunterOnDamageChance(pawn.kindDef);
		}

		// Token: 0x060014FB RID: 5371 RVA: 0x000B8F28 File Offset: 0x000B7328
		public static float GetBaseManhunterOnDamageChance(PawnKindDef kind)
		{
			return (kind != PawnKindDefOf.WildMan) ? kind.RaceProps.manhunterOnDamageChance : 0.5f;
		}

		// Token: 0x060014FC RID: 5372 RVA: 0x000B8F60 File Offset: 0x000B7360
		public static float GetManhunterOnDamageChance(Pawn pawn, float distance)
		{
			float manhunterOnDamageChance = PawnUtility.GetManhunterOnDamageChance(pawn.kindDef);
			return manhunterOnDamageChance * GenMath.LerpDoubleClamped(1f, 30f, 3f, 1f, distance);
		}

		// Token: 0x060014FD RID: 5373 RVA: 0x000B8FA0 File Offset: 0x000B73A0
		public static float GetManhunterOnDamageChance(Pawn pawn, Thing instigator = null)
		{
			float manhunterOnDamageChance;
			if (instigator != null)
			{
				manhunterOnDamageChance = PawnUtility.GetManhunterOnDamageChance(pawn, pawn.Position.DistanceTo(instigator.Position));
			}
			else
			{
				manhunterOnDamageChance = PawnUtility.GetManhunterOnDamageChance(pawn.kindDef);
			}
			return manhunterOnDamageChance;
		}

		// Token: 0x060014FE RID: 5374 RVA: 0x000B8FE4 File Offset: 0x000B73E4
		public static float GetManhunterOnDamageChance(PawnKindDef kind)
		{
			return PawnUtility.GetBaseManhunterOnDamageChance(kind) * Find.Storyteller.difficulty.manhunterChanceOnDamageFactor;
		}

		// Token: 0x060014FF RID: 5375 RVA: 0x000B9010 File Offset: 0x000B7410
		public static float GetManhunterOnDamageChance(RaceProperties race)
		{
			return race.manhunterOnDamageChance * Find.Storyteller.difficulty.manhunterChanceOnDamageFactor;
		}

		// Token: 0x06001500 RID: 5376 RVA: 0x000B903C File Offset: 0x000B743C
		public static float GetManhunterOnTameFailChance(Pawn pawn)
		{
			return PawnUtility.GetManhunterOnTameFailChance(pawn.kindDef);
		}

		// Token: 0x06001501 RID: 5377 RVA: 0x000B905C File Offset: 0x000B745C
		public static float GetManhunterOnTameFailChance(PawnKindDef kind)
		{
			return (kind != PawnKindDefOf.WildMan) ? kind.RaceProps.manhunterOnTameFailChance : 0.1f;
		}
	}
}
