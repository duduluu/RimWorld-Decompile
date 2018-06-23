﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	// Token: 0x0200077B RID: 1915
	public class TradeShip : PassingShip, ITrader, IThingHolder
	{
		// Token: 0x040016CF RID: 5839
		public TraderKindDef def;

		// Token: 0x040016D0 RID: 5840
		private ThingOwner things;

		// Token: 0x040016D1 RID: 5841
		private List<Pawn> soldPrisoners = new List<Pawn>();

		// Token: 0x040016D2 RID: 5842
		private int randomPriceFactorSeed = -1;

		// Token: 0x040016D3 RID: 5843
		private static List<string> tmpExtantNames = new List<string>();

		// Token: 0x06002A48 RID: 10824 RVA: 0x00166A0A File Offset: 0x00164E0A
		public TradeShip()
		{
		}

		// Token: 0x06002A49 RID: 10825 RVA: 0x00166A28 File Offset: 0x00164E28
		public TradeShip(TraderKindDef def)
		{
			this.def = def;
			this.things = new ThingOwner<Thing>(this);
			TradeShip.tmpExtantNames.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				TradeShip.tmpExtantNames.AddRange(from x in maps[i].passingShipManager.passingShips
				select x.name);
			}
			this.name = NameGenerator.GenerateName(RulePackDefOf.NamerTraderGeneral, TradeShip.tmpExtantNames, false, null);
			this.randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
			this.loadID = Find.UniqueIDsManager.GetNextPassingShipID();
		}

		// Token: 0x17000684 RID: 1668
		// (get) Token: 0x06002A4A RID: 10826 RVA: 0x00166B00 File Offset: 0x00164F00
		public override string FullTitle
		{
			get
			{
				return this.name + " (" + this.def.label + ")";
			}
		}

		// Token: 0x17000685 RID: 1669
		// (get) Token: 0x06002A4B RID: 10827 RVA: 0x00166B38 File Offset: 0x00164F38
		public int Silver
		{
			get
			{
				return this.CountHeldOf(ThingDefOf.Silver, null);
			}
		}

		// Token: 0x17000686 RID: 1670
		// (get) Token: 0x06002A4C RID: 10828 RVA: 0x00166B5C File Offset: 0x00164F5C
		public IThingHolder ParentHolder
		{
			get
			{
				return base.Map;
			}
		}

		// Token: 0x17000687 RID: 1671
		// (get) Token: 0x06002A4D RID: 10829 RVA: 0x00166B78 File Offset: 0x00164F78
		public TraderKindDef TraderKind
		{
			get
			{
				return this.def;
			}
		}

		// Token: 0x17000688 RID: 1672
		// (get) Token: 0x06002A4E RID: 10830 RVA: 0x00166B94 File Offset: 0x00164F94
		public int RandomPriceFactorSeed
		{
			get
			{
				return this.randomPriceFactorSeed;
			}
		}

		// Token: 0x17000689 RID: 1673
		// (get) Token: 0x06002A4F RID: 10831 RVA: 0x00166BB0 File Offset: 0x00164FB0
		public string TraderName
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x1700068A RID: 1674
		// (get) Token: 0x06002A50 RID: 10832 RVA: 0x00166BCC File Offset: 0x00164FCC
		public bool CanTradeNow
		{
			get
			{
				return !base.Departed;
			}
		}

		// Token: 0x1700068B RID: 1675
		// (get) Token: 0x06002A51 RID: 10833 RVA: 0x00166BEC File Offset: 0x00164FEC
		public float TradePriceImprovementOffsetForPlayer
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x1700068C RID: 1676
		// (get) Token: 0x06002A52 RID: 10834 RVA: 0x00166C08 File Offset: 0x00165008
		public Faction Faction
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700068D RID: 1677
		// (get) Token: 0x06002A53 RID: 10835 RVA: 0x00166C20 File Offset: 0x00165020
		public IEnumerable<Thing> Goods
		{
			get
			{
				for (int i = 0; i < this.things.Count; i++)
				{
					Pawn p = this.things[i] as Pawn;
					if (p == null || !this.soldPrisoners.Contains(p))
					{
						yield return this.things[i];
					}
				}
				yield break;
			}
		}

		// Token: 0x06002A54 RID: 10836 RVA: 0x00166C4C File Offset: 0x0016504C
		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			foreach (Thing t in TradeUtility.AllLaunchableThingsForTrade(base.Map))
			{
				yield return t;
			}
			foreach (Pawn p in TradeUtility.AllSellableColonyPawns(base.Map))
			{
				yield return p;
			}
			yield break;
		}

		// Token: 0x06002A55 RID: 10837 RVA: 0x00166C78 File Offset: 0x00165078
		public void GenerateThings()
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.traderDef = this.def;
			parms.tile = new int?(base.Map.Tile);
			this.things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms), true, false);
		}

		// Token: 0x06002A56 RID: 10838 RVA: 0x00166CD0 File Offset: 0x001650D0
		public override void PassingShipTick()
		{
			base.PassingShipTick();
			for (int i = this.things.Count - 1; i >= 0; i--)
			{
				Pawn pawn = this.things[i] as Pawn;
				if (pawn != null)
				{
					pawn.Tick();
					if (pawn.Dead)
					{
						this.things.Remove(pawn);
					}
				}
			}
		}

		// Token: 0x06002A57 RID: 10839 RVA: 0x00166D3C File Offset: 0x0016513C
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<TraderKindDef>(ref this.def, "def");
			Scribe_Deep.Look<ThingOwner>(ref this.things, "things", new object[]
			{
				this
			});
			Scribe_Collections.Look<Pawn>(ref this.soldPrisoners, "soldPrisoners", LookMode.Reference, new object[0]);
			Scribe_Values.Look<int>(ref this.randomPriceFactorSeed, "randomPriceFactorSeed", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.soldPrisoners.RemoveAll((Pawn x) => x == null);
			}
		}

		// Token: 0x06002A58 RID: 10840 RVA: 0x00166DD8 File Offset: 0x001651D8
		public override void TryOpenComms(Pawn negotiator)
		{
			if (this.CanTradeNow)
			{
				Find.WindowStack.Add(new Dialog_Trade(negotiator, this, false));
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(this.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradeShip".Translate(new object[]
				{
					Faction.OfPlayer.def.pawnsPlural
				}), LetterDefOf.NeutralEvent, false, true);
				TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon);
			}
		}

		// Token: 0x06002A59 RID: 10841 RVA: 0x00166E56 File Offset: 0x00165256
		public override void Depart()
		{
			base.Depart();
			this.things.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
			this.soldPrisoners.Clear();
		}

		// Token: 0x06002A5A RID: 10842 RVA: 0x00166E78 File Offset: 0x00165278
		public override string GetCallLabel()
		{
			return this.name + " (" + this.def.label + ")";
		}

		// Token: 0x06002A5B RID: 10843 RVA: 0x00166EB0 File Offset: 0x001652B0
		public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			int result;
			if (thing != null)
			{
				result = thing.stackCount;
			}
			else
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x06002A5C RID: 10844 RVA: 0x00166EE4 File Offset: 0x001652E4
		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
			Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
			if (thing2 != null)
			{
				if (!thing2.TryAbsorbStack(thing, false))
				{
					thing.Destroy(DestroyMode.Vanish);
				}
			}
			else
			{
				Pawn pawn = thing as Pawn;
				if (pawn != null && pawn.RaceProps.Humanlike)
				{
					this.soldPrisoners.Add(pawn);
				}
				this.things.TryAdd(thing, false);
			}
		}

		// Token: 0x06002A5D RID: 10845 RVA: 0x00166F68 File Offset: 0x00165368
		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				this.soldPrisoners.Remove(pawn);
			}
			TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(base.Map), base.Map, thing);
		}

		// Token: 0x06002A5E RID: 10846 RVA: 0x00166FB8 File Offset: 0x001653B8
		private Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
		{
			for (int i = 0; i < this.things.Count; i++)
			{
				if (this.things[i].def == thingDef && this.things[i].Stuff == stuffDef)
				{
					return this.things[i];
				}
			}
			return null;
		}

		// Token: 0x06002A5F RID: 10847 RVA: 0x0016702C File Offset: 0x0016542C
		public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			if (thing == null)
			{
				Log.Error("Changing count of thing trader doesn't have: " + thingDef, false);
			}
			thing.stackCount += count;
		}

		// Token: 0x06002A60 RID: 10848 RVA: 0x0016706C File Offset: 0x0016546C
		public override string ToString()
		{
			return this.FullTitle;
		}

		// Token: 0x06002A61 RID: 10849 RVA: 0x00167088 File Offset: 0x00165488
		public ThingOwner GetDirectlyHeldThings()
		{
			return this.things;
		}

		// Token: 0x06002A62 RID: 10850 RVA: 0x001670A3 File Offset: 0x001654A3
		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
	}
}
