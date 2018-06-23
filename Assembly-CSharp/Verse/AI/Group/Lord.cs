﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group
{
	// Token: 0x020009E3 RID: 2531
	[StaticConstructorOnStartup]
	public class Lord : IExposable, ILoadReferenceable
	{
		// Token: 0x04002443 RID: 9283
		public LordManager lordManager;

		// Token: 0x04002444 RID: 9284
		private LordToil curLordToil;

		// Token: 0x04002445 RID: 9285
		private StateGraph graph;

		// Token: 0x04002446 RID: 9286
		public int loadID = -1;

		// Token: 0x04002447 RID: 9287
		private LordJob curJob;

		// Token: 0x04002448 RID: 9288
		public Faction faction;

		// Token: 0x04002449 RID: 9289
		public List<Pawn> ownedPawns = new List<Pawn>();

		// Token: 0x0400244A RID: 9290
		public List<Thing> extraForbiddenThings = new List<Thing>();

		// Token: 0x0400244B RID: 9291
		private bool initialized;

		// Token: 0x0400244C RID: 9292
		public int ticksInToil = 0;

		// Token: 0x0400244D RID: 9293
		public int numPawnsLostViolently = 0;

		// Token: 0x0400244E RID: 9294
		public int numPawnsEverGained = 0;

		// Token: 0x0400244F RID: 9295
		public int initialColonyHealthTotal = 0;

		// Token: 0x04002450 RID: 9296
		public int lastPawnHarmTick = -99999;

		// Token: 0x04002451 RID: 9297
		private const int AttackTargetCacheInterval = 60;

		// Token: 0x04002452 RID: 9298
		private static readonly Material FlagTex = MaterialPool.MatFrom("UI/Overlays/SquadFlag");

		// Token: 0x04002453 RID: 9299
		private int tmpCurLordToilIdx = -1;

		// Token: 0x04002454 RID: 9300
		private Dictionary<int, LordToilData> tmpLordToilData = new Dictionary<int, LordToilData>();

		// Token: 0x04002455 RID: 9301
		private Dictionary<int, TriggerData> tmpTriggerData = new Dictionary<int, TriggerData>();

		// Token: 0x170008BF RID: 2239
		// (get) Token: 0x060038C6 RID: 14534 RVA: 0x001E4DD0 File Offset: 0x001E31D0
		public Map Map
		{
			get
			{
				return this.lordManager.map;
			}
		}

		// Token: 0x170008C0 RID: 2240
		// (get) Token: 0x060038C7 RID: 14535 RVA: 0x001E4DF0 File Offset: 0x001E31F0
		public StateGraph Graph
		{
			get
			{
				return this.graph;
			}
		}

		// Token: 0x170008C1 RID: 2241
		// (get) Token: 0x060038C8 RID: 14536 RVA: 0x001E4E0C File Offset: 0x001E320C
		public LordToil CurLordToil
		{
			get
			{
				return this.curLordToil;
			}
		}

		// Token: 0x170008C2 RID: 2242
		// (get) Token: 0x060038C9 RID: 14537 RVA: 0x001E4E28 File Offset: 0x001E3228
		public LordJob LordJob
		{
			get
			{
				return this.curJob;
			}
		}

		// Token: 0x170008C3 RID: 2243
		// (get) Token: 0x060038CA RID: 14538 RVA: 0x001E4E44 File Offset: 0x001E3244
		private bool CanExistWithoutPawns
		{
			get
			{
				return this.curJob is LordJob_VoluntarilyJoinable;
			}
		}

		// Token: 0x170008C4 RID: 2244
		// (get) Token: 0x060038CB RID: 14539 RVA: 0x001E4E68 File Offset: 0x001E3268
		public bool AnyActivePawn
		{
			get
			{
				for (int i = 0; i < this.ownedPawns.Count; i++)
				{
					if (!this.ownedPawns[i].Dead && this.ownedPawns[i].mindState.Active)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x060038CC RID: 14540 RVA: 0x001E4ED4 File Offset: 0x001E32D4
		private void Init()
		{
			this.initialized = true;
			this.initialColonyHealthTotal = this.Map.wealthWatcher.HealthTotal;
		}

		// Token: 0x060038CD RID: 14541 RVA: 0x001E4EF4 File Offset: 0x001E32F4
		public string GetUniqueLoadID()
		{
			return "Lord_" + this.loadID;
		}

		// Token: 0x060038CE RID: 14542 RVA: 0x001E4F20 File Offset: 0x001E3320
		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.loadID, "loadID", 0, false);
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			Scribe_Collections.Look<Thing>(ref this.extraForbiddenThings, "extraForbiddenThings", LookMode.Reference, new object[0]);
			Scribe_Collections.Look<Pawn>(ref this.ownedPawns, "ownedPawns", LookMode.Reference, new object[0]);
			Scribe_Deep.Look<LordJob>(ref this.curJob, "lordJob", new object[0]);
			Scribe_Values.Look<bool>(ref this.initialized, "initialized", true, false);
			Scribe_Values.Look<int>(ref this.ticksInToil, "ticksInToil", 0, false);
			Scribe_Values.Look<int>(ref this.numPawnsEverGained, "numPawnsEverGained", 0, false);
			Scribe_Values.Look<int>(ref this.numPawnsLostViolently, "numPawnsLostViolently", 0, false);
			Scribe_Values.Look<int>(ref this.initialColonyHealthTotal, "initialColonyHealthTotal", 0, false);
			Scribe_Values.Look<int>(ref this.lastPawnHarmTick, "lastPawnHarmTick", -99999, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.extraForbiddenThings.RemoveAll((Thing x) => x == null);
			}
			this.ExposeData_StateGraph();
		}

		// Token: 0x060038CF RID: 14543 RVA: 0x001E5040 File Offset: 0x001E3440
		private void ExposeData_StateGraph()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.tmpLordToilData.Clear();
				for (int i = 0; i < this.graph.lordToils.Count; i++)
				{
					if (this.graph.lordToils[i].data != null)
					{
						this.tmpLordToilData.Add(i, this.graph.lordToils[i].data);
					}
				}
				this.tmpTriggerData.Clear();
				int num = 0;
				for (int j = 0; j < this.graph.transitions.Count; j++)
				{
					for (int k = 0; k < this.graph.transitions[j].triggers.Count; k++)
					{
						if (this.graph.transitions[j].triggers[k].data != null)
						{
							this.tmpTriggerData.Add(num, this.graph.transitions[j].triggers[k].data);
						}
						num++;
					}
				}
				this.tmpCurLordToilIdx = this.graph.lordToils.IndexOf(this.curLordToil);
			}
			Scribe_Collections.Look<int, LordToilData>(ref this.tmpLordToilData, "lordToilData", LookMode.Value, LookMode.Deep);
			Scribe_Collections.Look<int, TriggerData>(ref this.tmpTriggerData, "triggerData", LookMode.Value, LookMode.Deep);
			Scribe_Values.Look<int>(ref this.tmpCurLordToilIdx, "curLordToilIdx", -1, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.curJob.LostImportantReferenceDuringLoading)
				{
					this.lordManager.RemoveLord(this);
				}
				else
				{
					LordJob job = this.curJob;
					this.curJob = null;
					this.SetJob(job);
					foreach (KeyValuePair<int, LordToilData> keyValuePair in this.tmpLordToilData)
					{
						if (keyValuePair.Key < 0 || keyValuePair.Key >= this.graph.lordToils.Count)
						{
							Log.Error(string.Concat(new object[]
							{
								"Could not find lord toil for lord toil data of type \"",
								keyValuePair.Value.GetType(),
								"\" (lord job: \"",
								this.curJob.GetType(),
								"\"), because lord toil index is out of bounds: ",
								keyValuePair.Key
							}), false);
						}
						else
						{
							this.graph.lordToils[keyValuePair.Key].data = keyValuePair.Value;
						}
					}
					this.tmpLordToilData.Clear();
					foreach (KeyValuePair<int, TriggerData> keyValuePair2 in this.tmpTriggerData)
					{
						Trigger triggerByIndex = this.GetTriggerByIndex(keyValuePair2.Key);
						if (triggerByIndex == null)
						{
							Log.Error(string.Concat(new object[]
							{
								"Could not find trigger for trigger data of type \"",
								keyValuePair2.Value.GetType(),
								"\" (lord job: \"",
								this.curJob.GetType(),
								"\"), because trigger index is out of bounds: ",
								keyValuePair2.Key
							}), false);
						}
						else
						{
							triggerByIndex.data = keyValuePair2.Value;
						}
					}
					this.tmpTriggerData.Clear();
					if (this.tmpCurLordToilIdx < 0 || this.tmpCurLordToilIdx >= this.graph.lordToils.Count)
					{
						Log.Error(string.Concat(new object[]
						{
							"Current lord toil index out of bounds (lord job: \"",
							this.curJob.GetType(),
							"\"): ",
							this.tmpCurLordToilIdx
						}), false);
					}
					else
					{
						this.curLordToil = this.graph.lordToils[this.tmpCurLordToilIdx];
					}
				}
			}
		}

		// Token: 0x060038D0 RID: 14544 RVA: 0x001E546C File Offset: 0x001E386C
		public void SetJob(LordJob lordJob)
		{
			if (this.curJob != null)
			{
				this.curJob.Cleanup();
			}
			this.curJob = lordJob;
			this.curLordToil = null;
			lordJob.lord = this;
			Rand.PushState();
			Rand.Seed = this.loadID * 193;
			this.graph = lordJob.CreateGraph();
			Rand.PopState();
			this.graph.ErrorCheck();
			if (this.faction != null && !this.faction.IsPlayer && this.faction.def.autoFlee)
			{
				LordToil_PanicFlee lordToil_PanicFlee = new LordToil_PanicFlee();
				lordToil_PanicFlee.avoidGridMode = AvoidGridMode.Smart;
				for (int i = 0; i < this.graph.lordToils.Count; i++)
				{
					Transition transition = new Transition(this.graph.lordToils[i], lordToil_PanicFlee, false, true);
					transition.AddPreAction(new TransitionAction_Message("MessageFightersFleeing".Translate(new object[]
					{
						this.faction.def.pawnsPlural.CapitalizeFirst(),
						this.faction.Name
					}), null, 1f));
					transition.AddTrigger(new Trigger_FractionPawnsLost(0.5f));
					this.graph.AddTransition(transition);
				}
				this.graph.AddToil(lordToil_PanicFlee);
			}
			for (int j = 0; j < this.graph.lordToils.Count; j++)
			{
				this.graph.lordToils[j].lord = this;
			}
			for (int k = 0; k < this.ownedPawns.Count; k++)
			{
				this.Map.attackTargetsCache.UpdateTarget(this.ownedPawns[k]);
			}
		}

		// Token: 0x060038D1 RID: 14545 RVA: 0x001E563C File Offset: 0x001E3A3C
		public void Cleanup()
		{
			this.curJob.Cleanup();
			if (this.curLordToil != null)
			{
				this.curLordToil.Cleanup();
			}
			for (int i = 0; i < this.ownedPawns.Count; i++)
			{
				if (this.ownedPawns[i].mindState != null)
				{
					this.ownedPawns[i].mindState.duty = null;
				}
				this.Map.attackTargetsCache.UpdateTarget(this.ownedPawns[i]);
				if (this.ownedPawns[i].Spawned && this.ownedPawns[i].CurJob != null)
				{
					this.ownedPawns[i].jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				}
			}
		}

		// Token: 0x060038D2 RID: 14546 RVA: 0x001E5718 File Offset: 0x001E3B18
		public void AddPawn(Pawn p)
		{
			if (this.ownedPawns.Contains(p))
			{
				Log.Error(string.Concat(new object[]
				{
					"Lord for ",
					this.faction.ToStringSafe<Faction>(),
					" tried to add ",
					p,
					" whom it already controls."
				}), false);
			}
			else if (p.GetLord() != null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to add pawn ",
					p,
					" to lord ",
					this,
					" but this pawn is already a member of lord ",
					p.GetLord(),
					". Pawns can't be members of more than one lord at the same time."
				}), false);
			}
			else
			{
				this.ownedPawns.Add(p);
				this.numPawnsEverGained++;
				this.Map.attackTargetsCache.UpdateTarget(p);
				this.curLordToil.UpdateAllDuties();
				this.curJob.Notify_PawnAdded(p);
			}
		}

		// Token: 0x060038D3 RID: 14547 RVA: 0x001E580D File Offset: 0x001E3C0D
		private void RemovePawn(Pawn p)
		{
			this.ownedPawns.Remove(p);
			if (p.mindState != null)
			{
				p.mindState.duty = null;
			}
			this.Map.attackTargetsCache.UpdateTarget(p);
		}

		// Token: 0x060038D4 RID: 14548 RVA: 0x001E5848 File Offset: 0x001E3C48
		public void GotoToil(LordToil newLordToil)
		{
			LordToil previousToil = this.curLordToil;
			if (this.curLordToil != null)
			{
				this.curLordToil.Cleanup();
			}
			this.curLordToil = newLordToil;
			this.ticksInToil = 0;
			if (this.curLordToil.lord != this)
			{
				Log.Error("curLordToil lord is " + ((this.curLordToil.lord != null) ? this.curLordToil.lord.ToString() : "null (forgot to add toil to graph?)"), false);
				this.curLordToil.lord = this;
			}
			this.curLordToil.Init();
			for (int i = 0; i < this.graph.transitions.Count; i++)
			{
				if (this.graph.transitions[i].sources.Contains(this.curLordToil))
				{
					this.graph.transitions[i].SourceToilBecameActive(this.graph.transitions[i], previousToil);
				}
			}
			this.curLordToil.UpdateAllDuties();
		}

		// Token: 0x060038D5 RID: 14549 RVA: 0x001E5963 File Offset: 0x001E3D63
		public void LordTick()
		{
			if (!this.initialized)
			{
				this.Init();
			}
			this.curLordToil.LordToilTick();
			this.CheckTransitionOnSignal(TriggerSignal.ForTick);
			this.ticksInToil++;
		}

		// Token: 0x060038D6 RID: 14550 RVA: 0x001E599C File Offset: 0x001E3D9C
		private Trigger GetTriggerByIndex(int index)
		{
			int num = 0;
			for (int i = 0; i < this.graph.transitions.Count; i++)
			{
				for (int j = 0; j < this.graph.transitions[i].triggers.Count; j++)
				{
					if (num == index)
					{
						return this.graph.transitions[i].triggers[j];
					}
					num++;
				}
			}
			return null;
		}

		// Token: 0x060038D7 RID: 14551 RVA: 0x001E5A32 File Offset: 0x001E3E32
		public void ReceiveMemo(string memo)
		{
			this.CheckTransitionOnSignal(TriggerSignal.ForMemo(memo));
		}

		// Token: 0x060038D8 RID: 14552 RVA: 0x001E5A44 File Offset: 0x001E3E44
		public void Notify_FactionRelationsChanged(Faction otherFaction, FactionRelationKind previousRelationKind)
		{
			this.CheckTransitionOnSignal(new TriggerSignal
			{
				type = TriggerSignalType.FactionRelationsChanged,
				faction = otherFaction,
				previousRelationKind = new FactionRelationKind?(previousRelationKind)
			});
		}

		// Token: 0x060038D9 RID: 14553 RVA: 0x001E5A80 File Offset: 0x001E3E80
		public void Notify_PawnLost(Pawn pawn, PawnLostCondition cond)
		{
			if (this.ownedPawns.Contains(pawn))
			{
				this.RemovePawn(pawn);
				if (cond == PawnLostCondition.IncappedOrKilled || cond == PawnLostCondition.MadePrisoner)
				{
					this.numPawnsLostViolently++;
				}
				this.curJob.Notify_PawnLost(pawn, cond);
				if (this.lordManager.lords.Contains(this))
				{
					if (this.ownedPawns.Count == 0 && !this.CanExistWithoutPawns)
					{
						this.lordManager.RemoveLord(this);
					}
					else
					{
						this.curLordToil.Notify_PawnLost(pawn, cond);
						this.CheckTransitionOnSignal(new TriggerSignal
						{
							type = TriggerSignalType.PawnLost,
							thing = pawn,
							condition = cond
						});
					}
				}
			}
			else
			{
				Log.Error(string.Concat(new object[]
				{
					"Lord lost pawn ",
					pawn,
					" it didn't have. Condition=",
					cond
				}), false);
			}
		}

		// Token: 0x060038DA RID: 14554 RVA: 0x001E5B84 File Offset: 0x001E3F84
		public void Notify_BuildingDamaged(Building building, DamageInfo dinfo)
		{
			this.CheckTransitionOnSignal(new TriggerSignal
			{
				type = TriggerSignalType.BuildingDamaged,
				thing = building,
				dinfo = dinfo
			});
		}

		// Token: 0x060038DB RID: 14555 RVA: 0x001E5BBC File Offset: 0x001E3FBC
		public void Notify_PawnDamaged(Pawn victim, DamageInfo dinfo)
		{
			this.CheckTransitionOnSignal(new TriggerSignal
			{
				type = TriggerSignalType.PawnDamaged,
				thing = victim,
				dinfo = dinfo
			});
		}

		// Token: 0x060038DC RID: 14556 RVA: 0x001E5BF4 File Offset: 0x001E3FF4
		public void Notify_PawnAttemptArrested(Pawn victim)
		{
			this.CheckTransitionOnSignal(new TriggerSignal
			{
				type = TriggerSignalType.PawnArrestAttempted,
				thing = victim
			});
		}

		// Token: 0x060038DD RID: 14557 RVA: 0x001E5C22 File Offset: 0x001E4022
		public void Notify_PawnAcquiredTarget(Pawn detector, Thing newTarg)
		{
		}

		// Token: 0x060038DE RID: 14558 RVA: 0x001E5C25 File Offset: 0x001E4025
		public void Notify_ReachedDutyLocation(Pawn pawn)
		{
			this.curLordToil.Notify_ReachedDutyLocation(pawn);
		}

		// Token: 0x060038DF RID: 14559 RVA: 0x001E5C34 File Offset: 0x001E4034
		public void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
		{
			this.curLordToil.Notify_ConstructionFailed(pawn, frame, newBlueprint);
		}

		// Token: 0x060038E0 RID: 14560 RVA: 0x001E5C48 File Offset: 0x001E4048
		private bool CheckTransitionOnSignal(TriggerSignal signal)
		{
			if (Trigger_PawnHarmed.SignalIsHarm(signal))
			{
				this.lastPawnHarmTick = Find.TickManager.TicksGame;
			}
			for (int i = 0; i < this.graph.transitions.Count; i++)
			{
				if (this.graph.transitions[i].sources.Contains(this.curLordToil))
				{
					if (this.graph.transitions[i].CheckSignal(this, signal))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060038E1 RID: 14561 RVA: 0x001E5CEC File Offset: 0x001E40EC
		private Vector3 DebugCenter()
		{
			Vector3 result = this.Map.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
			if ((from p in this.ownedPawns
			where p.Spawned
			select p).Any<Pawn>())
			{
				result.x = (from p in this.ownedPawns
				where p.Spawned
				select p).Average((Pawn p) => p.DrawPos.x);
				result.z = (from p in this.ownedPawns
				where p.Spawned
				select p).Average((Pawn p) => p.DrawPos.z);
			}
			return result;
		}

		// Token: 0x060038E2 RID: 14562 RVA: 0x001E5DF0 File Offset: 0x001E41F0
		public void DebugDraw()
		{
			Vector3 a = this.DebugCenter();
			IntVec3 flagLoc = this.curLordToil.FlagLoc;
			if (flagLoc.IsValid)
			{
				Graphics.DrawMesh(MeshPool.plane14, flagLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Building), Quaternion.identity, Lord.FlagTex, 0);
			}
			GenDraw.DrawLineBetween(a, flagLoc.ToVector3Shifted(), SimpleColor.Red);
			foreach (Pawn pawn in this.ownedPawns)
			{
				SimpleColor color = pawn.InMentalState ? SimpleColor.Yellow : SimpleColor.White;
				GenDraw.DrawLineBetween(a, pawn.DrawPos, color);
			}
		}

		// Token: 0x060038E3 RID: 14563 RVA: 0x001E5EB8 File Offset: 0x001E42B8
		public void DebugOnGUI()
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			string label;
			if (this.CurLordToil != null)
			{
				label = string.Concat(new object[]
				{
					"toil ",
					this.graph.lordToils.IndexOf(this.CurLordToil),
					"\n",
					this.CurLordToil.ToString()
				});
			}
			else
			{
				label = "toil=NULL";
			}
			Vector2 vector = this.DebugCenter().MapToUIPosition();
			Widgets.Label(new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f), label);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		// Token: 0x060038E4 RID: 14564 RVA: 0x001E5F74 File Offset: 0x001E4374
		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Start steal threshold: " + StealAIUtility.StartStealingMarketValueThreshold(this).ToString("F0"));
			stringBuilder.AppendLine("Duties:");
			foreach (Pawn pawn in this.ownedPawns)
			{
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"   ",
					pawn.LabelCap,
					" - ",
					pawn.mindState.duty
				}));
			}
			if (this.faction != null)
			{
				stringBuilder.AppendLine("Faction data:");
				stringBuilder.AppendLine(this.faction.DebugString());
			}
			stringBuilder.AppendLine("Raw save data:");
			stringBuilder.AppendLine(Scribe.saver.DebugOutputFor(this));
			return stringBuilder.ToString();
		}

		// Token: 0x060038E5 RID: 14565 RVA: 0x001E6094 File Offset: 0x001E4494
		private bool ShouldDoDebugOutput()
		{
			IntVec3 a = UI.MouseCell();
			IntVec3 flagLoc = this.curLordToil.FlagLoc;
			bool result;
			if (flagLoc.IsValid && a == flagLoc)
			{
				result = true;
			}
			else
			{
				for (int i = 0; i < this.ownedPawns.Count; i++)
				{
					if (a == this.ownedPawns[i].Position)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}
	}
}
