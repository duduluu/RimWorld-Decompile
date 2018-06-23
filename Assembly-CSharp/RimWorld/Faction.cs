﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	// Token: 0x02000554 RID: 1364
	public class Faction : IExposable, ILoadReferenceable, ICommunicable
	{
		// Token: 0x04000EFB RID: 3835
		public FactionDef def;

		// Token: 0x04000EFC RID: 3836
		private string name = null;

		// Token: 0x04000EFD RID: 3837
		public int loadID = -1;

		// Token: 0x04000EFE RID: 3838
		public int randomKey;

		// Token: 0x04000EFF RID: 3839
		public float colorFromSpectrum = -999f;

		// Token: 0x04000F00 RID: 3840
		public float centralMelanin = 0.5f;

		// Token: 0x04000F01 RID: 3841
		private List<FactionRelation> relations = new List<FactionRelation>();

		// Token: 0x04000F02 RID: 3842
		public Pawn leader = null;

		// Token: 0x04000F03 RID: 3843
		private FactionTacticalMemory tacticalMemoryInt = new FactionTacticalMemory();

		// Token: 0x04000F04 RID: 3844
		public KidnappedPawnsTracker kidnapped;

		// Token: 0x04000F05 RID: 3845
		private List<PredatorThreat> predatorThreats = new List<PredatorThreat>();

		// Token: 0x04000F06 RID: 3846
		public Dictionary<Map, ByteGrid> avoidGridsBasic = new Dictionary<Map, ByteGrid>();

		// Token: 0x04000F07 RID: 3847
		public Dictionary<Map, ByteGrid> avoidGridsSmart = new Dictionary<Map, ByteGrid>();

		// Token: 0x04000F08 RID: 3848
		public bool defeated;

		// Token: 0x04000F09 RID: 3849
		public int lastTraderRequestTick = -9999999;

		// Token: 0x04000F0A RID: 3850
		private int naturalGoodwillTimer;

		// Token: 0x04000F0B RID: 3851
		private List<Map> avoidGridsBasicKeysWorkingList;

		// Token: 0x04000F0C RID: 3852
		private List<ByteGrid> avoidGridsBasicValuesWorkingList;

		// Token: 0x04000F0D RID: 3853
		private List<Map> avoidGridsSmartKeysWorkingList;

		// Token: 0x04000F0E RID: 3854
		private List<ByteGrid> avoidGridsSmartValuesWorkingList;

		// Token: 0x04000F0F RID: 3855
		private static List<PawnKindDef> allPawnKinds = new List<PawnKindDef>();

		// Token: 0x0600195F RID: 6495 RVA: 0x000DC254 File Offset: 0x000DA654
		public Faction()
		{
			this.randomKey = Rand.Range(0, int.MaxValue);
			this.kidnapped = new KidnappedPawnsTracker(this);
		}

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x06001960 RID: 6496 RVA: 0x000DC2F4 File Offset: 0x000DA6F4
		// (set) Token: 0x06001961 RID: 6497 RVA: 0x000DC32B File Offset: 0x000DA72B
		public string Name
		{
			get
			{
				string labelCap;
				if (this.HasName)
				{
					labelCap = this.name;
				}
				else
				{
					labelCap = this.def.LabelCap;
				}
				return labelCap;
			}
			set
			{
				this.name = value;
			}
		}

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x06001962 RID: 6498 RVA: 0x000DC338 File Offset: 0x000DA738
		public bool HasName
		{
			get
			{
				return this.name != null;
			}
		}

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x06001963 RID: 6499 RVA: 0x000DC35C File Offset: 0x000DA75C
		public bool IsPlayer
		{
			get
			{
				return this.def.isPlayer;
			}
		}

		// Token: 0x17000394 RID: 916
		// (get) Token: 0x06001964 RID: 6500 RVA: 0x000DC37C File Offset: 0x000DA77C
		public int PlayerGoodwill
		{
			get
			{
				return this.GoodwillWith(Faction.OfPlayer);
			}
		}

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x06001965 RID: 6501 RVA: 0x000DC39C File Offset: 0x000DA79C
		public FactionRelationKind PlayerRelationKind
		{
			get
			{
				return this.RelationKindWith(Faction.OfPlayer);
			}
		}

		// Token: 0x17000396 RID: 918
		// (get) Token: 0x06001966 RID: 6502 RVA: 0x000DC3BC File Offset: 0x000DA7BC
		public FactionTacticalMemory TacticalMemory
		{
			get
			{
				if (this.tacticalMemoryInt == null)
				{
					this.tacticalMemoryInt = new FactionTacticalMemory();
				}
				return this.tacticalMemoryInt;
			}
		}

		// Token: 0x17000397 RID: 919
		// (get) Token: 0x06001967 RID: 6503 RVA: 0x000DC3F0 File Offset: 0x000DA7F0
		public Color Color
		{
			get
			{
				Color result;
				if (this.def.colorSpectrum.NullOrEmpty<Color>())
				{
					result = Color.white;
				}
				else
				{
					result = ColorsFromSpectrum.Get(this.def.colorSpectrum, this.colorFromSpectrum);
				}
				return result;
			}
		}

		// Token: 0x06001968 RID: 6504 RVA: 0x000DC43C File Offset: 0x000DA83C
		public void ExposeData()
		{
			Scribe_References.Look<Pawn>(ref this.leader, "leader", false);
			Scribe_Collections.Look<Map, ByteGrid>(ref this.avoidGridsBasic, "avoidGridsBasic", LookMode.Reference, LookMode.Deep, ref this.avoidGridsBasicKeysWorkingList, ref this.avoidGridsBasicValuesWorkingList);
			Scribe_Collections.Look<Map, ByteGrid>(ref this.avoidGridsSmart, "avoidGridsSmart", LookMode.Reference, LookMode.Deep, ref this.avoidGridsSmartKeysWorkingList, ref this.avoidGridsSmartValuesWorkingList);
			Scribe_Defs.Look<FactionDef>(ref this.def, "def");
			Scribe_Values.Look<string>(ref this.name, "name", null, false);
			Scribe_Values.Look<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Values.Look<int>(ref this.randomKey, "randomKey", 0, false);
			Scribe_Values.Look<float>(ref this.colorFromSpectrum, "colorFromSpectrum", 0f, false);
			Scribe_Values.Look<float>(ref this.centralMelanin, "centralMelanin", 0f, false);
			Scribe_Collections.Look<FactionRelation>(ref this.relations, "relations", LookMode.Deep, new object[0]);
			Scribe_Deep.Look<FactionTacticalMemory>(ref this.tacticalMemoryInt, "tacticalMemory", new object[0]);
			Scribe_Deep.Look<KidnappedPawnsTracker>(ref this.kidnapped, "kidnapped", new object[]
			{
				this
			});
			Scribe_Collections.Look<PredatorThreat>(ref this.predatorThreats, "predatorThreats", LookMode.Deep, new object[0]);
			Scribe_Values.Look<bool>(ref this.defeated, "defeated", false, false);
			Scribe_Values.Look<int>(ref this.lastTraderRequestTick, "lastTraderRequestTick", -9999999, false);
			Scribe_Values.Look<int>(ref this.naturalGoodwillTimer, "naturalGoodwillTimer", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.predatorThreats.RemoveAll((PredatorThreat x) => x.predator == null);
			}
		}

		// Token: 0x06001969 RID: 6505 RVA: 0x000DC5D8 File Offset: 0x000DA9D8
		public void FactionTick()
		{
			this.CheckNaturalTendencyToReachGoodwillThreshold();
			this.kidnapped.KidnappedPawnsTrackerTick();
			for (int i = this.predatorThreats.Count - 1; i >= 0; i--)
			{
				PredatorThreat predatorThreat = this.predatorThreats[i];
				if (predatorThreat.Expired)
				{
					this.predatorThreats.RemoveAt(i);
					if (predatorThreat.predator.Spawned)
					{
						predatorThreat.predator.Map.attackTargetsCache.UpdateTarget(predatorThreat.predator);
					}
				}
			}
			if (Find.TickManager.TicksGame % 1000 == 200 && this.IsPlayer)
			{
				if (NamePlayerFactionAndBaseUtility.CanNameFactionNow())
				{
					FactionBase factionBase = Find.WorldObjects.FactionBases.Find((FactionBase x) => NamePlayerFactionAndBaseUtility.CanNameFactionBaseSoon(x));
					if (factionBase != null)
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(factionBase));
					}
					else
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFaction());
					}
				}
				else
				{
					FactionBase factionBase2 = Find.WorldObjects.FactionBases.Find((FactionBase x) => NamePlayerFactionAndBaseUtility.CanNameFactionBaseNow(x));
					if (factionBase2 != null)
					{
						if (NamePlayerFactionAndBaseUtility.CanNameFactionSoon())
						{
							Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(factionBase2));
						}
						else
						{
							Find.WindowStack.Add(new Dialog_NamePlayerFactionBase(factionBase2));
						}
					}
				}
			}
		}

		// Token: 0x0600196A RID: 6506 RVA: 0x000DC760 File Offset: 0x000DAB60
		private void CheckNaturalTendencyToReachGoodwillThreshold()
		{
			if (!this.IsPlayer)
			{
				int playerGoodwill = this.PlayerGoodwill;
				if (this.def.naturalColonyGoodwill.Includes(playerGoodwill))
				{
					this.naturalGoodwillTimer = 0;
				}
				else
				{
					this.naturalGoodwillTimer++;
					if (playerGoodwill < this.def.naturalColonyGoodwill.min)
					{
						if (this.def.goodwillDailyGain != 0f)
						{
							int num = (int)(10f / this.def.goodwillDailyGain * 60000f);
							if (this.naturalGoodwillTimer >= num)
							{
								Faction ofPlayer = Faction.OfPlayer;
								int goodwillChange = Mathf.Min(10, this.def.naturalColonyGoodwill.min - playerGoodwill);
								string reason = "GoodwillChangedReason_NaturallyOverTime".Translate(new object[]
								{
									this.def.naturalColonyGoodwill.min.ToString()
								});
								this.TryAffectGoodwillWith(ofPlayer, goodwillChange, true, true, reason, null);
								this.naturalGoodwillTimer = 0;
							}
						}
					}
					else if (playerGoodwill > this.def.naturalColonyGoodwill.max)
					{
						if (this.def.goodwillDailyFall != 0f)
						{
							int num2 = (int)(10f / this.def.goodwillDailyFall * 60000f);
							if (this.naturalGoodwillTimer >= num2)
							{
								Faction ofPlayer = Faction.OfPlayer;
								int goodwillChange = -Mathf.Min(10, playerGoodwill - this.def.naturalColonyGoodwill.max);
								string reason = "GoodwillChangedReason_NaturallyOverTime".Translate(new object[]
								{
									this.def.naturalColonyGoodwill.max.ToString()
								});
								this.TryAffectGoodwillWith(ofPlayer, goodwillChange, true, true, reason, null);
								this.naturalGoodwillTimer = 0;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600196B RID: 6507 RVA: 0x000DC94C File Offset: 0x000DAD4C
		public ByteGrid GetAvoidGridBasic(Map map)
		{
			ByteGrid byteGrid;
			ByteGrid result;
			if (this.avoidGridsBasic.TryGetValue(map, out byteGrid))
			{
				result = byteGrid;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x0600196C RID: 6508 RVA: 0x000DC97C File Offset: 0x000DAD7C
		public ByteGrid GetAvoidGridSmart(Map map)
		{
			ByteGrid byteGrid;
			ByteGrid result;
			if (this.avoidGridsSmart.TryGetValue(map, out byteGrid))
			{
				result = byteGrid;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x0600196D RID: 6509 RVA: 0x000DC9AC File Offset: 0x000DADAC
		public void Notify_MapRemoved(Map map)
		{
			this.avoidGridsBasic.Remove(map);
			this.avoidGridsSmart.Remove(map);
			this.tacticalMemoryInt.Notify_MapRemoved(map);
		}

		// Token: 0x0600196E RID: 6510 RVA: 0x000DC9D8 File Offset: 0x000DADD8
		public void TryMakeInitialRelationsWith(Faction other)
		{
			if (this.RelationWith(other, true) == null)
			{
				int a = (!this.def.permanentEnemy) ? this.def.startingGoodwill.RandomInRange : -100;
				if (this.IsPlayer)
				{
					a = 100;
				}
				int b = (!other.def.permanentEnemy) ? other.def.startingGoodwill.RandomInRange : -100;
				if (other.IsPlayer)
				{
					b = 100;
				}
				int num = Mathf.Min(a, b);
				FactionRelationKind kind;
				if (num <= -10)
				{
					kind = FactionRelationKind.Hostile;
				}
				else if (num >= 75)
				{
					kind = FactionRelationKind.Ally;
				}
				else
				{
					kind = FactionRelationKind.Neutral;
				}
				FactionRelation factionRelation = new FactionRelation();
				factionRelation.other = other;
				factionRelation.goodwill = num;
				factionRelation.kind = kind;
				this.relations.Add(factionRelation);
				FactionRelation factionRelation2 = new FactionRelation();
				factionRelation2.other = this;
				factionRelation2.goodwill = num;
				factionRelation2.kind = kind;
				other.relations.Add(factionRelation2);
			}
		}

		// Token: 0x0600196F RID: 6511 RVA: 0x000DCAE8 File Offset: 0x000DAEE8
		public PawnKindDef RandomPawnKind()
		{
			Faction.allPawnKinds.Clear();
			if (this.def.pawnGroupMakers != null)
			{
				for (int i = 0; i < this.def.pawnGroupMakers.Count; i++)
				{
					List<PawnGenOption> options = this.def.pawnGroupMakers[i].options;
					for (int j = 0; j < options.Count; j++)
					{
						Faction.allPawnKinds.Add(options[j].kind);
					}
				}
			}
			PawnKindDef result;
			if (!Faction.allPawnKinds.Any<PawnKindDef>())
			{
				result = this.def.basicMemberKind;
			}
			else
			{
				PawnKindDef pawnKindDef = Faction.allPawnKinds.RandomElement<PawnKindDef>();
				Faction.allPawnKinds.Clear();
				result = pawnKindDef;
			}
			return result;
		}

		// Token: 0x06001970 RID: 6512 RVA: 0x000DCBBC File Offset: 0x000DAFBC
		public FactionRelation RelationWith(Faction other, bool allowNull = false)
		{
			FactionRelation result;
			if (other == this)
			{
				Log.Error("Tried to get relation between faction " + this + " and itself.", false);
				result = new FactionRelation();
			}
			else
			{
				for (int i = 0; i < this.relations.Count; i++)
				{
					if (this.relations[i].other == other)
					{
						return this.relations[i];
					}
				}
				if (!allowNull)
				{
					Log.Error(string.Concat(new object[]
					{
						"Faction ",
						this.name,
						" has null relation with ",
						other,
						". Returning dummy relation."
					}), false);
					result = new FactionRelation();
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		// Token: 0x06001971 RID: 6513 RVA: 0x000DCC8C File Offset: 0x000DB08C
		public int GoodwillWith(Faction other)
		{
			return this.RelationWith(other, false).goodwill;
		}

		// Token: 0x06001972 RID: 6514 RVA: 0x000DCCB0 File Offset: 0x000DB0B0
		public FactionRelationKind RelationKindWith(Faction other)
		{
			return this.RelationWith(other, false).kind;
		}

		// Token: 0x06001973 RID: 6515 RVA: 0x000DCCD4 File Offset: 0x000DB0D4
		public bool TryAffectGoodwillWith(Faction other, int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
		{
			bool result;
			if (this.def.hidden || other.def.hidden || this.def.permanentEnemy || other.def.permanentEnemy || this.defeated || other.defeated || other == this)
			{
				result = false;
			}
			else if (goodwillChange > 0 && ((this.IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(other)) || (other.IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(this))))
			{
				result = false;
			}
			else if (goodwillChange == 0)
			{
				result = true;
			}
			else
			{
				int num = this.GoodwillWith(other);
				int num2 = Mathf.Clamp(num + goodwillChange, -100, 100);
				if (num == num2)
				{
					result = true;
				}
				else
				{
					FactionRelation factionRelation = this.RelationWith(other, false);
					factionRelation.goodwill = num2;
					bool flag;
					factionRelation.CheckKindThresholds(this, canSendHostilityLetter, reason, (lookTarget == null) ? GlobalTargetInfo.Invalid : lookTarget.Value, out flag);
					FactionRelation factionRelation2 = other.RelationWith(this, false);
					FactionRelationKind kind = factionRelation2.kind;
					factionRelation2.goodwill = factionRelation.goodwill;
					factionRelation2.kind = factionRelation.kind;
					bool flag2;
					if (kind != factionRelation2.kind)
					{
						other.Notify_RelationKindChanged(this, kind, canSendHostilityLetter, reason, (lookTarget == null) ? GlobalTargetInfo.Invalid : lookTarget.Value, out flag2);
					}
					else
					{
						flag2 = false;
					}
					if (canSendMessage && !flag && !flag2 && Current.ProgramState == ProgramState.Playing && (this.IsPlayer || other.IsPlayer))
					{
						Faction faction = (!this.IsPlayer) ? this : other;
						string text;
						if (!reason.NullOrEmpty())
						{
							text = "MessageGoodwillChangedWithReason".Translate(new object[]
							{
								faction.name,
								num.ToString("F0"),
								factionRelation.goodwill.ToString("F0"),
								reason
							});
						}
						else
						{
							text = "MessageGoodwillChanged".Translate(new object[]
							{
								faction.name,
								num.ToString("F0"),
								factionRelation.goodwill.ToString("F0")
							});
						}
						Messages.Message(text, (lookTarget == null) ? GlobalTargetInfo.Invalid : lookTarget.Value, ((float)goodwillChange <= 0f) ? MessageTypeDefOf.NegativeEvent : MessageTypeDefOf.PositiveEvent, true);
					}
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06001974 RID: 6516 RVA: 0x000DCF90 File Offset: 0x000DB390
		public bool TrySetNotHostileTo(Faction other, bool canSendLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
		{
			if (this.RelationKindWith(other) == FactionRelationKind.Hostile)
			{
				this.TrySetRelationKind(other, FactionRelationKind.Neutral, canSendLetter, reason, lookTarget);
			}
			return this.RelationKindWith(other) != FactionRelationKind.Hostile;
		}

		// Token: 0x06001975 RID: 6517 RVA: 0x000DCFCC File Offset: 0x000DB3CC
		public bool TrySetNotAlly(Faction other, bool canSendLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
		{
			if (this.RelationKindWith(other) == FactionRelationKind.Ally)
			{
				this.TrySetRelationKind(other, FactionRelationKind.Neutral, canSendLetter, reason, lookTarget);
			}
			return this.RelationKindWith(other) != FactionRelationKind.Ally;
		}

		// Token: 0x06001976 RID: 6518 RVA: 0x000DD008 File Offset: 0x000DB408
		public bool TrySetRelationKind(Faction other, FactionRelationKind kind, bool canSendLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
		{
			FactionRelation factionRelation = this.RelationWith(other, false);
			bool result;
			if (factionRelation.kind == kind)
			{
				result = true;
			}
			else
			{
				switch (kind)
				{
				case FactionRelationKind.Hostile:
					this.TryAffectGoodwillWith(other, -75 - factionRelation.goodwill, false, canSendLetter, reason, lookTarget);
					result = (factionRelation.kind == FactionRelationKind.Hostile);
					break;
				case FactionRelationKind.Neutral:
					this.TryAffectGoodwillWith(other, -factionRelation.goodwill, false, canSendLetter, reason, lookTarget);
					result = (factionRelation.kind == FactionRelationKind.Neutral);
					break;
				case FactionRelationKind.Ally:
					this.TryAffectGoodwillWith(other, 75 - factionRelation.goodwill, false, canSendLetter, reason, lookTarget);
					result = (factionRelation.kind == FactionRelationKind.Ally);
					break;
				default:
					throw new NotSupportedException(kind.ToString());
				}
			}
			return result;
		}

		// Token: 0x06001977 RID: 6519 RVA: 0x000DD0D0 File Offset: 0x000DB4D0
		public void RemoveAllRelations()
		{
			foreach (Faction faction in Find.FactionManager.AllFactionsListForReading)
			{
				if (faction != this)
				{
					faction.relations.RemoveAll((FactionRelation x) => x.other == this);
				}
			}
			this.relations.Clear();
		}

		// Token: 0x06001978 RID: 6520 RVA: 0x000DD15C File Offset: 0x000DB55C
		public void TryAppendRelationKindChangedInfo(StringBuilder text, FactionRelationKind previousKind, FactionRelationKind newKind, string reason = null)
		{
			string text2 = null;
			this.TryAppendRelationKindChangedInfo(ref text2, previousKind, newKind, reason);
			if (!text2.NullOrEmpty())
			{
				text.AppendLine();
				text.AppendLine();
				text.Append(text2);
			}
		}

		// Token: 0x06001979 RID: 6521 RVA: 0x000DD19C File Offset: 0x000DB59C
		public void TryAppendRelationKindChangedInfo(ref string text, FactionRelationKind previousKind, FactionRelationKind newKind, string reason = null)
		{
			if (previousKind != newKind)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				if (newKind == FactionRelationKind.Hostile)
				{
					text += "LetterRelationsChange_Hostile".Translate(new object[]
					{
						this.name,
						this.PlayerGoodwill.ToStringWithSign(),
						-75.ToStringWithSign(),
						0.ToStringWithSign()
					});
					if (!reason.NullOrEmpty())
					{
						text = text + "\n\n" + "FinalStraw".Translate(new object[]
						{
							reason.CapitalizeFirst()
						});
					}
				}
				else if (newKind == FactionRelationKind.Ally)
				{
					text += "LetterRelationsChange_Ally".Translate(new object[]
					{
						this.name,
						this.PlayerGoodwill.ToStringWithSign(),
						75.ToStringWithSign(),
						0.ToStringWithSign()
					});
					if (!reason.NullOrEmpty())
					{
						string text2 = text;
						text = string.Concat(new string[]
						{
							text2,
							"\n\n",
							"LastFactionRelationsEvent".Translate(),
							": ",
							reason.CapitalizeFirst()
						});
					}
				}
				else if (newKind == FactionRelationKind.Neutral)
				{
					if (previousKind == FactionRelationKind.Hostile)
					{
						text += "LetterRelationsChange_NeutralFromHostile".Translate(new object[]
						{
							this.name,
							this.PlayerGoodwill.ToStringWithSign(),
							0.ToStringWithSign(),
							-75.ToStringWithSign(),
							75.ToStringWithSign()
						});
						if (!reason.NullOrEmpty())
						{
							string text2 = text;
							text = string.Concat(new string[]
							{
								text2,
								"\n\n",
								"LastFactionRelationsEvent".Translate(),
								": ",
								reason.CapitalizeFirst()
							});
						}
					}
					else
					{
						text += "LetterRelationsChange_NeutralFromAlly".Translate(new object[]
						{
							this.name,
							this.PlayerGoodwill.ToStringWithSign(),
							0.ToStringWithSign(),
							-75.ToStringWithSign(),
							75.ToStringWithSign()
						});
						if (!reason.NullOrEmpty())
						{
							string text2 = text;
							text = string.Concat(new string[]
							{
								text2,
								"\n\n",
								"Reason".Translate(),
								": ",
								reason.CapitalizeFirst()
							});
						}
					}
				}
			}
		}

		// Token: 0x0600197A RID: 6522 RVA: 0x000DD42C File Offset: 0x000DB82C
		public void Notify_MemberTookDamage(Pawn member, DamageInfo dinfo)
		{
			if (dinfo.Instigator != null)
			{
				if (!this.IsPlayer)
				{
					Pawn pawn = dinfo.Instigator as Pawn;
					if (pawn != null && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.PredatorHunt)
					{
						this.TookDamageFromPredator(pawn);
					}
					if (dinfo.Instigator.Faction != null && dinfo.Def.externalViolence && !this.HostileTo(dinfo.Instigator.Faction))
					{
						if (!member.InAggroMentalState)
						{
							if (pawn == null || !pawn.InMentalState || pawn.MentalStateDef != MentalStateDefOf.Berserk)
							{
								if (!member.InMentalState || !member.MentalStateDef.IsExtreme || member.MentalStateDef.category != MentalStateCategory.Malicious || this.PlayerRelationKind != FactionRelationKind.Ally)
								{
									if (dinfo.Instigator.Faction != Faction.OfPlayer || !PrisonBreakUtility.IsPrisonBreaking(member))
									{
										if (dinfo.Instigator.Faction == Faction.OfPlayer && !this.IsMutuallyHostileCrossfire(dinfo))
										{
											float num = Mathf.Min(100f, dinfo.Amount);
											int num2 = (int)(-1.3f * num);
											Faction faction = dinfo.Instigator.Faction;
											int goodwillChange = num2;
											string reason = "GoodwillChangedReason_AttackedPawn".Translate(new object[]
											{
												member.LabelShort
											});
											GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(member);
											this.TryAffectGoodwillWith(faction, goodwillChange, true, true, reason, lookTarget);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600197B RID: 6523 RVA: 0x000DD5F4 File Offset: 0x000DB9F4
		public void Notify_MemberCaptured(Pawn member, Faction violator)
		{
			if (violator != this)
			{
				if (this.RelationKindWith(violator) != FactionRelationKind.Hostile)
				{
					FactionRelationKind kind = FactionRelationKind.Hostile;
					string reason = "GoodwillChangedReason_CapturedPawn".Translate(new object[]
					{
						member.LabelShort
					});
					this.TrySetRelationKind(violator, kind, true, reason, new GlobalTargetInfo?(member));
				}
			}
		}

		// Token: 0x0600197C RID: 6524 RVA: 0x000DD650 File Offset: 0x000DBA50
		public void Notify_MemberDied(Pawn member, DamageInfo? dinfo, bool wasWorldPawn, Map map)
		{
			if (!this.IsPlayer)
			{
				if (!wasWorldPawn && !PawnGenerator.IsBeingGenerated(member) && Current.ProgramState == ProgramState.Playing && map != null && map.IsPlayerHome && !this.HostileTo(Faction.OfPlayer))
				{
					if (dinfo != null && dinfo.Value.Category == DamageInfo.SourceCategory.Collapse)
					{
						bool flag = MessagesRepeatAvoider.MessageShowAllowed("FactionRelationAdjustmentCrushed-" + this.Name, 5f);
						Faction ofPlayer = Faction.OfPlayer;
						int goodwillChange = (!member.RaceProps.Humanlike) ? -15 : -25;
						bool canSendMessage = flag;
						string reason = "GoodwillChangedReason_PawnCrushed".Translate(new object[]
						{
							member.LabelShort
						});
						this.TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage, true, reason, new GlobalTargetInfo?(new TargetInfo(member.Position, map, false)));
					}
					else if (dinfo != null && (dinfo.Value.Instigator == null || dinfo.Value.Instigator.Faction == null))
					{
						Faction ofPlayer = Faction.OfPlayer;
						int goodwillChange = (!member.RaceProps.Humanlike) ? -3 : -5;
						string reason = "GoodwillChangedReason_PawnDied".Translate(new object[]
						{
							member.LabelShort
						});
						GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(member);
						this.TryAffectGoodwillWith(ofPlayer, goodwillChange, true, true, reason, lookTarget);
					}
				}
				if (member == this.leader)
				{
					this.Notify_LeaderDied();
				}
			}
		}

		// Token: 0x0600197D RID: 6525 RVA: 0x000DD7FC File Offset: 0x000DBBFC
		public void Notify_LeaderDied()
		{
			Pawn pawn = this.leader;
			this.GenerateNewLeader();
			Find.LetterStack.ReceiveLetter("LetterLeadersDeathLabel".Translate(new object[]
			{
				this.name,
				this.def.leaderTitle
			}).CapitalizeFirst(), "LetterLeadersDeath".Translate(new object[]
			{
				pawn.Name.ToStringFull,
				this.name,
				this.leader.Name.ToStringFull,
				this.def.leaderTitle
			}).CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, this, null);
		}

		// Token: 0x0600197E RID: 6526 RVA: 0x000DD8AC File Offset: 0x000DBCAC
		public void Notify_LeaderLost()
		{
			Pawn pawn = this.leader;
			this.GenerateNewLeader();
			Find.LetterStack.ReceiveLetter("LetterLeaderChangedLabel".Translate(new object[]
			{
				this.name,
				this.def.leaderTitle
			}).CapitalizeFirst(), "LetterLeaderChanged".Translate(new object[]
			{
				pawn.Name.ToStringFull,
				this.name,
				this.leader.Name.ToStringFull,
				this.def.leaderTitle
			}).CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, this, null);
		}

		// Token: 0x0600197F RID: 6527 RVA: 0x000DD95C File Offset: 0x000DBD5C
		public void Notify_RelationKindChanged(Faction other, FactionRelationKind previousKind, bool canSendLetter, string reason, GlobalTargetInfo lookTarget, out bool sentLetter)
		{
			if (Current.ProgramState != ProgramState.Playing || other != Faction.OfPlayer)
			{
				canSendLetter = false;
			}
			sentLetter = false;
			FactionRelationKind factionRelationKind = this.RelationKindWith(other);
			if (factionRelationKind == FactionRelationKind.Hostile)
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive.ToList<Pawn>())
					{
						if ((pawn.Faction == this && pawn.HostFaction == other) || (pawn.Faction == other && pawn.HostFaction == this))
						{
							pawn.guest.SetGuestStatus(pawn.HostFaction, true);
						}
					}
				}
			}
			if (other == Faction.OfPlayer)
			{
				List<Site> list = new List<Site>();
				List<Site> sites = Find.WorldObjects.Sites;
				for (int i = 0; i < sites.Count; i++)
				{
					if (sites[i].factionMustRemainHostile && sites[i].Faction == this && !this.HostileTo(Faction.OfPlayer) && !sites[i].HasMap)
					{
						list.Add(sites[i]);
					}
				}
				if (list.Any<Site>())
				{
					string label;
					string text;
					if (list.Count == 1)
					{
						label = "LetterLabelSiteNoLongerHostile".Translate();
						text = "LetterSiteNoLongerHostile".Translate(new object[]
						{
							this.Name,
							list[0].Label
						});
					}
					else
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int j = 0; j < list.Count; j++)
						{
							if (stringBuilder.Length != 0)
							{
								stringBuilder.AppendLine();
							}
							stringBuilder.Append("  - " + list[j].LabelCap);
							ImportantPawnComp component = list[j].GetComponent<ImportantPawnComp>();
							if (component != null && component.pawn.Any)
							{
								stringBuilder.Append(" (" + component.pawn[0].LabelCap + ")");
							}
						}
						label = "LetterLabelSiteNoLongerHostileMulti".Translate();
						text = "LetterSiteNoLongerHostileMulti".Translate(new object[]
						{
							this.Name
						}) + ":\n\n" + stringBuilder;
					}
					Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new LookTargets(from x in list
					select new GlobalTargetInfo(x.Tile)), null, null);
					for (int k = 0; k < list.Count; k++)
					{
						Find.WorldObjects.Remove(list[k]);
					}
				}
			}
			if (canSendLetter)
			{
				string text2 = "";
				this.TryAppendRelationKindChangedInfo(ref text2, previousKind, factionRelationKind, reason);
				if (factionRelationKind == FactionRelationKind.Hostile)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Hostile".Translate(new object[]
					{
						this.name
					}), text2, LetterDefOf.NegativeEvent, lookTarget, this, null);
					sentLetter = true;
				}
				else if (factionRelationKind == FactionRelationKind.Ally)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Ally".Translate(new object[]
					{
						this.name
					}), text2, LetterDefOf.PositiveEvent, lookTarget, this, null);
					sentLetter = true;
				}
				else if (factionRelationKind == FactionRelationKind.Neutral)
				{
					if (previousKind == FactionRelationKind.Hostile)
					{
						Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromHostile".Translate(new object[]
						{
							this.name
						}), text2, LetterDefOf.PositiveEvent, lookTarget, this, null);
						sentLetter = true;
					}
					else
					{
						Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromAlly".Translate(new object[]
						{
							this.name
						}), text2, LetterDefOf.NeutralEvent, lookTarget, this, null);
						sentLetter = true;
					}
				}
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				List<Map> maps = Find.Maps;
				for (int l = 0; l < maps.Count; l++)
				{
					maps[l].attackTargetsCache.Notify_FactionHostilityChanged(this, other);
					LordManager lordManager = maps[l].lordManager;
					for (int m = 0; m < lordManager.lords.Count; m++)
					{
						Lord lord = lordManager.lords[m];
						if (lord.faction == other)
						{
							lord.Notify_FactionRelationsChanged(this, previousKind);
						}
						else if (lord.faction == this)
						{
							lord.Notify_FactionRelationsChanged(other, previousKind);
						}
					}
				}
			}
		}

		// Token: 0x06001980 RID: 6528 RVA: 0x000DDE50 File Offset: 0x000DC250
		public void Notify_PlayerTraded(float marketValueSentByPlayer, Pawn playerNegotiator)
		{
			Faction ofPlayer = Faction.OfPlayer;
			int goodwillChange = (int)(marketValueSentByPlayer / 400f);
			string reason = "GoodwillChangedReason_Traded".Translate();
			GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(playerNegotiator);
			this.TryAffectGoodwillWith(ofPlayer, goodwillChange, true, true, reason, lookTarget);
		}

		// Token: 0x06001981 RID: 6529 RVA: 0x000DDE94 File Offset: 0x000DC294
		public void Notify_MemberExitedMap(Pawn member, bool free)
		{
			if (free && member.HostFaction != null && member.guest != null && (member.guest.Released || !member.IsPrisoner))
			{
				bool flag = false;
				float num = 0f;
				if (!member.InMentalState && member.health.hediffSet.BleedRateTotal < 0.001f)
				{
					flag = true;
					num += 15f;
					if (PawnUtility.IsFactionLeader(member))
					{
						num += 40f;
					}
				}
				num += (float)Mathf.Min(member.mindState.timesGuestTendedToByPlayer, 10) * 1f;
				Faction hostFaction = member.HostFaction;
				int goodwillChange = (int)num;
				string reason = (!flag) ? "GoodwillChangedReason_Tended".Translate(new object[]
				{
					member.LabelShort
				}) : "GoodwillChangedReason_ExitedMapHealthy".Translate(new object[]
				{
					member.LabelShort
				});
				this.TryAffectGoodwillWith(hostFaction, goodwillChange, true, true, reason, null);
			}
			member.mindState.timesGuestTendedToByPlayer = 0;
		}

		// Token: 0x06001982 RID: 6530 RVA: 0x000DDFB0 File Offset: 0x000DC3B0
		public void GenerateNewLeader()
		{
			this.leader = null;
			if (this.def.pawnGroupMakers != null)
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (PawnGroupMaker pawnGroupMaker in from x in this.def.pawnGroupMakers
				where x.kindDef == PawnGroupKindDefOf.Combat
				select x)
				{
					foreach (PawnGenOption pawnGenOption in pawnGroupMaker.options)
					{
						if (pawnGenOption.kind.factionLeader)
						{
							list.Add(pawnGenOption.kind);
						}
					}
				}
				PawnKindDef kindDef;
				if (list.TryRandomElement(out kindDef))
				{
					this.leader = PawnGenerator.GeneratePawn(kindDef, this);
					if (this.leader.RaceProps.IsFlesh)
					{
						this.leader.relations.everSeenByPlayer = true;
					}
					if (!Find.WorldPawns.Contains(this.leader))
					{
						Find.WorldPawns.PassToWorld(this.leader, PawnDiscardDecideMode.KeepForever);
					}
				}
			}
		}

		// Token: 0x06001983 RID: 6531 RVA: 0x000DE11C File Offset: 0x000DC51C
		public string GetCallLabel()
		{
			return this.name;
		}

		// Token: 0x06001984 RID: 6532 RVA: 0x000DE138 File Offset: 0x000DC538
		public string GetInfoText()
		{
			string labelCap = this.def.LabelCap;
			string text = labelCap;
			return string.Concat(new string[]
			{
				text,
				"\n",
				"goodwill".Translate().CapitalizeFirst(),
				": ",
				this.PlayerGoodwill.ToStringWithSign()
			});
		}

		// Token: 0x06001985 RID: 6533 RVA: 0x000DE19C File Offset: 0x000DC59C
		Faction ICommunicable.GetFaction()
		{
			return this;
		}

		// Token: 0x06001986 RID: 6534 RVA: 0x000DE1B4 File Offset: 0x000DC5B4
		public void TryOpenComms(Pawn negotiator)
		{
			Dialog_Negotiation dialog_Negotiation = new Dialog_Negotiation(negotiator, this, FactionDialogMaker.FactionDialogFor(negotiator, this), true);
			dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
			Find.WindowStack.Add(dialog_Negotiation);
		}

		// Token: 0x06001987 RID: 6535 RVA: 0x000DE1E8 File Offset: 0x000DC5E8
		private bool LeaderIsAvailableToTalk()
		{
			bool result;
			if (this.leader == null)
			{
				result = false;
			}
			else
			{
				if (this.leader.Spawned)
				{
					if (this.leader.Downed || this.leader.IsPrisoner || !this.leader.Awake() || this.leader.InMentalState)
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06001988 RID: 6536 RVA: 0x000DE26C File Offset: 0x000DC66C
		public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
		{
			FloatMenuOption result;
			if (this.IsPlayer)
			{
				result = null;
			}
			else
			{
				string text = "CallOnRadio".Translate(new object[]
				{
					this.GetCallLabel()
				});
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					" (",
					this.PlayerRelationKind.GetLabel(),
					", ",
					this.PlayerGoodwill.ToStringWithSign(),
					")"
				});
				if (!this.LeaderIsAvailableToTalk())
				{
					string str;
					if (this.leader != null)
					{
						str = "LeaderUnavailable".Translate(new object[]
						{
							this.leader.LabelShort
						});
					}
					else
					{
						str = "LeaderUnavailableNoLeader".Translate();
					}
					result = new FloatMenuOption(text + " (" + str + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
				}
				else
				{
					result = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate()
					{
						console.GiveUseCommsJob(negotiator, this);
					}, MenuOptionPriority.InitiateSocial, null, null, 0f, null, null), negotiator, console, "ReservedBy");
				}
			}
			return result;
		}

		// Token: 0x06001989 RID: 6537 RVA: 0x000DE3B8 File Offset: 0x000DC7B8
		private void TookDamageFromPredator(Pawn predator)
		{
			for (int i = 0; i < this.predatorThreats.Count; i++)
			{
				if (this.predatorThreats[i].predator == predator)
				{
					this.predatorThreats[i].lastAttackTicks = Find.TickManager.TicksGame;
					return;
				}
			}
			PredatorThreat predatorThreat = new PredatorThreat();
			predatorThreat.predator = predator;
			predatorThreat.lastAttackTicks = Find.TickManager.TicksGame;
			this.predatorThreats.Add(predatorThreat);
		}

		// Token: 0x0600198A RID: 6538 RVA: 0x000DE448 File Offset: 0x000DC848
		public bool HasPredatorRecentlyAttackedAnyone(Pawn predator)
		{
			for (int i = 0; i < this.predatorThreats.Count; i++)
			{
				if (this.predatorThreats[i].predator == predator)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600198B RID: 6539 RVA: 0x000DE49C File Offset: 0x000DC89C
		private bool IsMutuallyHostileCrossfire(DamageInfo dinfo)
		{
			return dinfo.Instigator != null && dinfo.IntendedTarget != null && dinfo.IntendedTarget.HostileTo(dinfo.Instigator) && dinfo.IntendedTarget.HostileTo(this);
		}

		// Token: 0x17000398 RID: 920
		// (get) Token: 0x0600198C RID: 6540 RVA: 0x000DE4F4 File Offset: 0x000DC8F4
		public static Faction OfPlayer
		{
			get
			{
				Faction ofPlayerSilentFail = Faction.OfPlayerSilentFail;
				if (ofPlayerSilentFail == null)
				{
					Log.Error("Could not find player faction.", false);
				}
				return ofPlayerSilentFail;
			}
		}

		// Token: 0x17000399 RID: 921
		// (get) Token: 0x0600198D RID: 6541 RVA: 0x000DE524 File Offset: 0x000DC924
		public static Faction OfMechanoids
		{
			get
			{
				return Find.FactionManager.OfMechanoids;
			}
		}

		// Token: 0x1700039A RID: 922
		// (get) Token: 0x0600198E RID: 6542 RVA: 0x000DE544 File Offset: 0x000DC944
		public static Faction OfInsects
		{
			get
			{
				return Find.FactionManager.OfInsects;
			}
		}

		// Token: 0x1700039B RID: 923
		// (get) Token: 0x0600198F RID: 6543 RVA: 0x000DE564 File Offset: 0x000DC964
		public static Faction OfAncients
		{
			get
			{
				return Find.FactionManager.OfAncients;
			}
		}

		// Token: 0x1700039C RID: 924
		// (get) Token: 0x06001990 RID: 6544 RVA: 0x000DE584 File Offset: 0x000DC984
		public static Faction OfAncientsHostile
		{
			get
			{
				return Find.FactionManager.OfAncientsHostile;
			}
		}

		// Token: 0x1700039D RID: 925
		// (get) Token: 0x06001991 RID: 6545 RVA: 0x000DE5A4 File Offset: 0x000DC9A4
		public static Faction OfPlayerSilentFail
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					GameInitData gameInitData = Find.GameInitData;
					if (gameInitData != null && gameInitData.playerFaction != null)
					{
						return gameInitData.playerFaction;
					}
				}
				return Find.FactionManager.OfPlayer;
			}
		}

		// Token: 0x06001992 RID: 6546 RVA: 0x000DE5F4 File Offset: 0x000DC9F4
		public string GetUniqueLoadID()
		{
			return "Faction_" + this.loadID;
		}

		// Token: 0x06001993 RID: 6547 RVA: 0x000DE620 File Offset: 0x000DCA20
		public override string ToString()
		{
			string result;
			if (this.name != null)
			{
				result = this.name;
			}
			else if (this.def != null)
			{
				result = this.def.defName;
			}
			else
			{
				result = "[faction of no def]";
			}
			return result;
		}

		// Token: 0x06001994 RID: 6548 RVA: 0x000DE670 File Offset: 0x000DCA70
		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ByteGrid avoidGridSmart = this.GetAvoidGridSmart(Find.CurrentMap);
			if (avoidGridSmart != null)
			{
				stringBuilder.Append("Avoidgrid val at mouse: ");
				if (UI.MouseCell().InBounds(Find.CurrentMap))
				{
					stringBuilder.AppendLine(avoidGridSmart[UI.MouseCell()].ToString());
				}
				stringBuilder.Append("Avoidgrid pathcost at mouse: ");
				if (UI.MouseCell().InBounds(Find.CurrentMap))
				{
					stringBuilder.AppendLine(((int)(avoidGridSmart[UI.MouseCell()] * 8)).ToString());
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06001995 RID: 6549 RVA: 0x000DE72C File Offset: 0x000DCB2C
		public void DebugDrawOnMap()
		{
			ByteGrid avoidGridSmart = this.GetAvoidGridSmart(Find.CurrentMap);
			if (avoidGridSmart != null)
			{
				avoidGridSmart.DebugDraw();
			}
		}
	}
}
