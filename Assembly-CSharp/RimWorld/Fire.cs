﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	// Token: 0x020006C1 RID: 1729
	public class Fire : AttachableThing, ISizeReporter
	{
		// Token: 0x040014C0 RID: 5312
		private int ticksSinceSpawn;

		// Token: 0x040014C1 RID: 5313
		public float fireSize = 0.1f;

		// Token: 0x040014C2 RID: 5314
		private int ticksSinceSpread;

		// Token: 0x040014C3 RID: 5315
		private float flammabilityMax = 0.5f;

		// Token: 0x040014C4 RID: 5316
		private int ticksUntilSmoke = 0;

		// Token: 0x040014C5 RID: 5317
		private Sustainer sustainer = null;

		// Token: 0x040014C6 RID: 5318
		private static List<Thing> flammableList = new List<Thing>();

		// Token: 0x040014C7 RID: 5319
		private static int fireCount;

		// Token: 0x040014C8 RID: 5320
		private static int lastFireCountUpdateTick;

		// Token: 0x040014C9 RID: 5321
		public const float MinFireSize = 0.1f;

		// Token: 0x040014CA RID: 5322
		private const float MinSizeForSpark = 1f;

		// Token: 0x040014CB RID: 5323
		private const float TicksBetweenSparksBase = 150f;

		// Token: 0x040014CC RID: 5324
		private const float TicksBetweenSparksReductionPerFireSize = 40f;

		// Token: 0x040014CD RID: 5325
		private const float MinTicksBetweenSparks = 75f;

		// Token: 0x040014CE RID: 5326
		private const float MinFireSizeToEmitSpark = 1f;

		// Token: 0x040014CF RID: 5327
		public const float MaxFireSize = 1.75f;

		// Token: 0x040014D0 RID: 5328
		private const int TicksToBurnFloor = 7500;

		// Token: 0x040014D1 RID: 5329
		private const int ComplexCalcsInterval = 150;

		// Token: 0x040014D2 RID: 5330
		private const float CellIgniteChancePerTickPerSize = 0.01f;

		// Token: 0x040014D3 RID: 5331
		private const float MinSizeForIgniteMovables = 0.4f;

		// Token: 0x040014D4 RID: 5332
		private const float FireBaseGrowthPerTick = 0.00055f;

		// Token: 0x040014D5 RID: 5333
		private static readonly IntRange SmokeIntervalRange = new IntRange(130, 200);

		// Token: 0x040014D6 RID: 5334
		private const int SmokeIntervalRandomAddon = 10;

		// Token: 0x040014D7 RID: 5335
		private const float BaseSkyExtinguishChance = 0.04f;

		// Token: 0x040014D8 RID: 5336
		private const int BaseSkyExtinguishDamage = 10;

		// Token: 0x040014D9 RID: 5337
		private const float HeatPerFireSizePerInterval = 160f;

		// Token: 0x040014DA RID: 5338
		private const float HeatFactorWhenDoorPresent = 0.15f;

		// Token: 0x040014DB RID: 5339
		private const float SnowClearRadiusPerFireSize = 3f;

		// Token: 0x040014DC RID: 5340
		private const float SnowClearDepthFactor = 0.1f;

		// Token: 0x040014DD RID: 5341
		private const int FireCountParticlesOff = 15;

		// Token: 0x170005A1 RID: 1441
		// (get) Token: 0x06002557 RID: 9559 RVA: 0x001405F0 File Offset: 0x0013E9F0
		public override string Label
		{
			get
			{
				string result;
				if (this.parent != null)
				{
					result = "FireOn".Translate(new object[]
					{
						this.parent.LabelCap
					});
				}
				else
				{
					result = "Fire".Translate();
				}
				return result;
			}
		}

		// Token: 0x170005A2 RID: 1442
		// (get) Token: 0x06002558 RID: 9560 RVA: 0x00140640 File Offset: 0x0013EA40
		public override string InspectStringAddon
		{
			get
			{
				return "Burning".Translate() + " (" + "FireSizeLower".Translate(new object[]
				{
					(this.fireSize * 100f).ToString("F0")
				}) + ")";
			}
		}

		// Token: 0x170005A3 RID: 1443
		// (get) Token: 0x06002559 RID: 9561 RVA: 0x0014069C File Offset: 0x0013EA9C
		private float SpreadInterval
		{
			get
			{
				float num = 150f - (this.fireSize - 1f) * 40f;
				if (num < 75f)
				{
					num = 75f;
				}
				return num;
			}
		}

		// Token: 0x0600255A RID: 9562 RVA: 0x001406DC File Offset: 0x0013EADC
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksSinceSpawn, "ticksSinceSpawn", 0, false);
			Scribe_Values.Look<float>(ref this.fireSize, "fireSize", 0f, false);
		}

		// Token: 0x0600255B RID: 9563 RVA: 0x0014070D File Offset: 0x0013EB0D
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.RecalcPathsOnAndAroundMe(map);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.HomeArea, this, OpportunityType.Important);
			this.ticksSinceSpread = (int)(this.SpreadInterval * Rand.Value);
		}

		// Token: 0x0600255C RID: 9564 RVA: 0x00140740 File Offset: 0x0013EB40
		public float CurrentSize()
		{
			return this.fireSize;
		}

		// Token: 0x0600255D RID: 9565 RVA: 0x0014075C File Offset: 0x0013EB5C
		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (this.sustainer != null)
			{
				if (this.sustainer.externalParams.sizeAggregator == null)
				{
					this.sustainer.externalParams.sizeAggregator = new SoundSizeAggregator();
				}
				this.sustainer.externalParams.sizeAggregator.RemoveReporter(this);
			}
			Map map = base.Map;
			base.DeSpawn(mode);
			this.RecalcPathsOnAndAroundMe(map);
		}

		// Token: 0x0600255E RID: 9566 RVA: 0x001407CC File Offset: 0x0013EBCC
		private void RecalcPathsOnAndAroundMe(Map map)
		{
			IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
			for (int i = 0; i < adjacentCellsAndInside.Length; i++)
			{
				IntVec3 c = base.Position + adjacentCellsAndInside[i];
				if (c.InBounds(map))
				{
					map.pathGrid.RecalculatePerceivedPathCostAt(c);
				}
			}
		}

		// Token: 0x0600255F RID: 9567 RVA: 0x0014082C File Offset: 0x0013EC2C
		public override void AttachTo(Thing parent)
		{
			base.AttachTo(parent);
			Pawn pawn = parent as Pawn;
			if (pawn != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.WasOnFire, new object[]
				{
					pawn
				});
			}
		}

		// Token: 0x06002560 RID: 9568 RVA: 0x00140864 File Offset: 0x0013EC64
		public override void Tick()
		{
			this.ticksSinceSpawn++;
			if (Fire.lastFireCountUpdateTick != Find.TickManager.TicksGame)
			{
				Fire.fireCount = base.Map.listerThings.ThingsOfDef(this.def).Count;
				Fire.lastFireCountUpdateTick = Find.TickManager.TicksGame;
			}
			if (this.sustainer != null)
			{
				this.sustainer.Maintain();
			}
			else if (!base.Position.Fogged(base.Map))
			{
				SoundInfo info = SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.PerTick);
				this.sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, SoundDefOf.FireBurning, info);
			}
			Profiler.BeginSample("Spawn particles");
			this.ticksUntilSmoke--;
			if (this.ticksUntilSmoke <= 0)
			{
				this.SpawnSmokeParticles();
			}
			if (Fire.fireCount < 15 && this.fireSize > 0.7f && Rand.Value < this.fireSize * 0.01f)
			{
				MoteMaker.ThrowMicroSparks(this.DrawPos, base.Map);
			}
			Profiler.EndSample();
			Profiler.BeginSample("Spread");
			if (this.fireSize > 1f)
			{
				this.ticksSinceSpread++;
				if ((float)this.ticksSinceSpread >= this.SpreadInterval)
				{
					this.TrySpread();
					this.ticksSinceSpread = 0;
				}
			}
			Profiler.EndSample();
			if (this.IsHashIntervalTick(150))
			{
				this.DoComplexCalcs();
			}
			if (this.ticksSinceSpawn >= 7500)
			{
				this.TryBurnFloor();
			}
		}

		// Token: 0x06002561 RID: 9569 RVA: 0x00140A18 File Offset: 0x0013EE18
		private void SpawnSmokeParticles()
		{
			if (Fire.fireCount < 15)
			{
				MoteMaker.ThrowSmoke(this.DrawPos, base.Map, this.fireSize);
			}
			if (this.fireSize > 0.5f && this.parent == null)
			{
				MoteMaker.ThrowFireGlow(base.Position, base.Map, this.fireSize);
			}
			float num = this.fireSize / 2f;
			if (num > 1f)
			{
				num = 1f;
			}
			num = 1f - num;
			this.ticksUntilSmoke = Fire.SmokeIntervalRange.Lerped(num) + (int)(10f * Rand.Value);
		}

		// Token: 0x06002562 RID: 9570 RVA: 0x00140AC4 File Offset: 0x0013EEC4
		private void DoComplexCalcs()
		{
			bool flag = false;
			Profiler.BeginSample("Determine flammability");
			Fire.flammableList.Clear();
			this.flammabilityMax = 0f;
			if (!base.Position.GetTerrain(base.Map).extinguishesFire)
			{
				if (this.parent == null)
				{
					if (base.Position.TerrainFlammableNow(base.Map))
					{
						this.flammabilityMax = base.Position.GetTerrain(base.Map).GetStatValueAbstract(StatDefOf.Flammability, null);
					}
					List<Thing> list = base.Map.thingGrid.ThingsListAt(base.Position);
					for (int i = 0; i < list.Count; i++)
					{
						Thing thing = list[i];
						if (thing is Building_Door)
						{
							flag = true;
						}
						float statValue = thing.GetStatValue(StatDefOf.Flammability, true);
						if (statValue >= 0.01f)
						{
							Fire.flammableList.Add(list[i]);
							if (statValue > this.flammabilityMax)
							{
								this.flammabilityMax = statValue;
							}
							if (this.parent == null && this.fireSize > 0.4f && list[i].def.category == ThingCategory.Pawn)
							{
								list[i].TryAttachFire(this.fireSize * 0.2f);
							}
						}
					}
				}
				else
				{
					Fire.flammableList.Add(this.parent);
					this.flammabilityMax = this.parent.GetStatValue(StatDefOf.Flammability, true);
				}
			}
			Profiler.EndSample();
			if (this.flammabilityMax < 0.01f)
			{
				this.Destroy(DestroyMode.Vanish);
			}
			else
			{
				Profiler.BeginSample("Do damage");
				Thing thing2;
				if (this.parent != null)
				{
					thing2 = this.parent;
				}
				else if (Fire.flammableList.Count > 0)
				{
					thing2 = Fire.flammableList.RandomElement<Thing>();
				}
				else
				{
					thing2 = null;
				}
				if (thing2 != null)
				{
					if (this.fireSize >= 0.4f || thing2 == this.parent || thing2.def.category != ThingCategory.Pawn)
					{
						this.DoFireDamage(thing2);
					}
				}
				Profiler.EndSample();
				if (base.Spawned)
				{
					Profiler.BeginSample("Room heat");
					float num = this.fireSize * 160f;
					if (flag)
					{
						num *= 0.15f;
					}
					GenTemperature.PushHeat(base.Position, base.Map, num);
					Profiler.EndSample();
					Profiler.BeginSample("Snow clear");
					if (Rand.Value < 0.4f)
					{
						float radius = this.fireSize * 3f;
						SnowUtility.AddSnowRadial(base.Position, base.Map, radius, -(this.fireSize * 0.1f));
					}
					Profiler.EndSample();
					Profiler.BeginSample("Grow/extinguish");
					this.fireSize += 0.00055f * this.flammabilityMax * 150f;
					if (this.fireSize > 1.75f)
					{
						this.fireSize = 1.75f;
					}
					if (base.Map.weatherManager.RainRate > 0.01f)
					{
						if (this.VulnerableToRain())
						{
							if (Rand.Value < 6f)
							{
								base.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, 10f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
							}
						}
					}
					Profiler.EndSample();
				}
			}
		}

		// Token: 0x06002563 RID: 9571 RVA: 0x00140E48 File Offset: 0x0013F248
		private void TryBurnFloor()
		{
			if (this.parent == null && base.Spawned)
			{
				if (base.Position.TerrainFlammableNow(base.Map))
				{
					base.Map.terrainGrid.Notify_TerrainBurned(base.Position);
				}
			}
		}

		// Token: 0x06002564 RID: 9572 RVA: 0x00140EA0 File Offset: 0x0013F2A0
		private bool VulnerableToRain()
		{
			bool result;
			if (!base.Spawned)
			{
				result = false;
			}
			else
			{
				RoofDef roofDef = base.Map.roofGrid.RoofAt(base.Position);
				if (roofDef == null)
				{
					result = true;
				}
				else if (roofDef.isThickRoof)
				{
					result = false;
				}
				else
				{
					Thing edifice = base.Position.GetEdifice(base.Map);
					result = (edifice != null && edifice.def.holdsRoof);
				}
			}
			return result;
		}

		// Token: 0x06002565 RID: 9573 RVA: 0x00140F24 File Offset: 0x0013F324
		private void DoFireDamage(Thing targ)
		{
			float num = 0.0125f + 0.0036f * this.fireSize;
			num = Mathf.Clamp(num, 0.0125f, 0.05f);
			int num2 = GenMath.RoundRandom(num * 150f);
			if (num2 < 1)
			{
				num2 = 1;
			}
			Pawn pawn = targ as Pawn;
			if (pawn != null)
			{
				BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire, null);
				Find.BattleLog.Add(battleLogEntry_DamageTaken);
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, (float)num2, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
				dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
				targ.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
				if (pawn.apparel != null)
				{
					Apparel apparel;
					if (pawn.apparel.WornApparel.TryRandomElement(out apparel))
					{
						apparel.TakeDamage(new DamageInfo(DamageDefOf.Flame, (float)num2, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
					}
				}
			}
			else
			{
				targ.TakeDamage(new DamageInfo(DamageDefOf.Flame, (float)num2, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
			}
		}

		// Token: 0x06002566 RID: 9574 RVA: 0x00141028 File Offset: 0x0013F428
		protected void TrySpread()
		{
			IntVec3 intVec = base.Position;
			bool flag;
			if (Rand.Chance(0.8f))
			{
				intVec = base.Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(1, 8)];
				flag = true;
			}
			else
			{
				intVec = base.Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(10, 20)];
				flag = false;
			}
			if (intVec.InBounds(base.Map))
			{
				if (Rand.Chance(FireUtility.ChanceToStartFireIn(intVec, base.Map)))
				{
					if (!flag)
					{
						CellRect startRect = CellRect.SingleCell(base.Position);
						CellRect endRect = CellRect.SingleCell(intVec);
						if (GenSight.LineOfSight(base.Position, intVec, base.Map, startRect, endRect, null))
						{
							Spark spark = (Spark)GenSpawn.Spawn(ThingDefOf.Spark, base.Position, base.Map, WipeMode.Vanish);
							spark.Launch(this, intVec, intVec, ProjectileHitFlags.All, null);
						}
					}
					else
					{
						FireUtility.TryStartFireIn(intVec, base.Map, 0.1f);
					}
				}
			}
		}
	}
}
