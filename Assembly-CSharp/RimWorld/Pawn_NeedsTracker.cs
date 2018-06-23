﻿using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	// Token: 0x02000509 RID: 1289
	public class Pawn_NeedsTracker : IExposable
	{
		// Token: 0x04000DBD RID: 3517
		private Pawn pawn;

		// Token: 0x04000DBE RID: 3518
		private List<Need> needs = new List<Need>();

		// Token: 0x04000DBF RID: 3519
		public Need_Mood mood;

		// Token: 0x04000DC0 RID: 3520
		public Need_Food food;

		// Token: 0x04000DC1 RID: 3521
		public Need_Rest rest;

		// Token: 0x04000DC2 RID: 3522
		public Need_Joy joy;

		// Token: 0x04000DC3 RID: 3523
		public Need_Beauty beauty;

		// Token: 0x04000DC4 RID: 3524
		public Need_RoomSize roomsize;

		// Token: 0x04000DC5 RID: 3525
		public Need_Outdoors outdoors;

		// Token: 0x04000DC6 RID: 3526
		public Need_Comfort comfort;

		// Token: 0x06001726 RID: 5926 RVA: 0x000CBDD5 File Offset: 0x000CA1D5
		public Pawn_NeedsTracker()
		{
		}

		// Token: 0x06001727 RID: 5927 RVA: 0x000CBDE9 File Offset: 0x000CA1E9
		public Pawn_NeedsTracker(Pawn newPawn)
		{
			this.pawn = newPawn;
			this.AddOrRemoveNeedsAsAppropriate();
		}

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x06001728 RID: 5928 RVA: 0x000CBE0C File Offset: 0x000CA20C
		public List<Need> AllNeeds
		{
			get
			{
				return this.needs;
			}
		}

		// Token: 0x06001729 RID: 5929 RVA: 0x000CBE28 File Offset: 0x000CA228
		public void ExposeData()
		{
			Scribe_Collections.Look<Need>(ref this.needs, "needs", LookMode.Deep, new object[]
			{
				this.pawn
			});
			if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.needs.RemoveAll((Need x) => x == null) != 0)
				{
					Log.Error("Pawn " + this.pawn.Label + " had some null needs after loading.", false);
				}
				this.BindDirectNeedFields();
			}
		}

		// Token: 0x0600172A RID: 5930 RVA: 0x000CBEC4 File Offset: 0x000CA2C4
		private void BindDirectNeedFields()
		{
			this.mood = this.TryGetNeed<Need_Mood>();
			this.food = this.TryGetNeed<Need_Food>();
			this.rest = this.TryGetNeed<Need_Rest>();
			this.joy = this.TryGetNeed<Need_Joy>();
			this.beauty = this.TryGetNeed<Need_Beauty>();
			this.comfort = this.TryGetNeed<Need_Comfort>();
			this.roomsize = this.TryGetNeed<Need_RoomSize>();
			this.outdoors = this.TryGetNeed<Need_Outdoors>();
		}

		// Token: 0x0600172B RID: 5931 RVA: 0x000CBF34 File Offset: 0x000CA334
		public void NeedsTrackerTick()
		{
			if (this.pawn.IsHashIntervalTick(150))
			{
				for (int i = 0; i < this.needs.Count; i++)
				{
					this.needs[i].NeedInterval();
				}
			}
		}

		// Token: 0x0600172C RID: 5932 RVA: 0x000CBF88 File Offset: 0x000CA388
		public T TryGetNeed<T>() where T : Need
		{
			for (int i = 0; i < this.needs.Count; i++)
			{
				if (this.needs[i].GetType() == typeof(T))
				{
					return (T)((object)this.needs[i]);
				}
			}
			return (T)((object)null);
		}

		// Token: 0x0600172D RID: 5933 RVA: 0x000CBFF8 File Offset: 0x000CA3F8
		public Need TryGetNeed(NeedDef def)
		{
			for (int i = 0; i < this.needs.Count; i++)
			{
				if (this.needs[i].def == def)
				{
					return this.needs[i];
				}
			}
			return null;
		}

		// Token: 0x0600172E RID: 5934 RVA: 0x000CC058 File Offset: 0x000CA458
		public void SetInitialLevels()
		{
			for (int i = 0; i < this.needs.Count; i++)
			{
				this.needs[i].SetInitialLevel();
			}
		}

		// Token: 0x0600172F RID: 5935 RVA: 0x000CC098 File Offset: 0x000CA498
		public void AddOrRemoveNeedsAsAppropriate()
		{
			List<NeedDef> allDefsListForReading = DefDatabase<NeedDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				NeedDef needDef = allDefsListForReading[i];
				if (this.ShouldHaveNeed(needDef))
				{
					if (this.TryGetNeed(needDef) == null)
					{
						this.AddNeed(needDef);
					}
				}
				else if (this.TryGetNeed(needDef) != null)
				{
					this.RemoveNeed(needDef);
				}
			}
		}

		// Token: 0x06001730 RID: 5936 RVA: 0x000CC108 File Offset: 0x000CA508
		private bool ShouldHaveNeed(NeedDef nd)
		{
			bool result;
			if (this.pawn.RaceProps.intelligence < nd.minIntelligence)
			{
				result = false;
			}
			else
			{
				if (nd.colonistsOnly)
				{
					if (this.pawn.Faction == null || !this.pawn.Faction.IsPlayer)
					{
						return false;
					}
				}
				if (nd.colonistAndPrisonersOnly)
				{
					if ((this.pawn.Faction == null || !this.pawn.Faction.IsPlayer) && (this.pawn.HostFaction == null || this.pawn.HostFaction != Faction.OfPlayer))
					{
						return false;
					}
				}
				if (nd.onlyIfCausedByHediff)
				{
					if (!this.pawn.health.hediffSet.hediffs.Any((Hediff x) => x.def.causesNeed == nd))
					{
						return false;
					}
				}
				if (nd.neverOnPrisoner && this.pawn.IsPrisoner)
				{
					result = false;
				}
				else if (nd == NeedDefOf.Food)
				{
					result = this.pawn.RaceProps.EatsFood;
				}
				else
				{
					result = (nd != NeedDefOf.Rest || this.pawn.RaceProps.needsRest);
				}
			}
			return result;
		}

		// Token: 0x06001731 RID: 5937 RVA: 0x000CC2B4 File Offset: 0x000CA6B4
		private void AddNeed(NeedDef nd)
		{
			Need need = (Need)Activator.CreateInstance(nd.needClass, new object[]
			{
				this.pawn
			});
			need.def = nd;
			this.needs.Add(need);
			need.SetInitialLevel();
			this.BindDirectNeedFields();
		}

		// Token: 0x06001732 RID: 5938 RVA: 0x000CC304 File Offset: 0x000CA704
		private void RemoveNeed(NeedDef nd)
		{
			Need item = this.TryGetNeed(nd);
			this.needs.Remove(item);
			this.BindDirectNeedFields();
		}
	}
}
