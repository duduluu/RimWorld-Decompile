﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Profiling;
using Verse;

namespace RimWorld
{
	// Token: 0x02000824 RID: 2084
	[StaticConstructorOnStartup]
	public class TransferableOneWayWidget
	{
		// Token: 0x0400190B RID: 6411
		private List<TransferableOneWayWidget.Section> sections = new List<TransferableOneWayWidget.Section>();

		// Token: 0x0400190C RID: 6412
		private string sourceLabel;

		// Token: 0x0400190D RID: 6413
		private string destinationLabel;

		// Token: 0x0400190E RID: 6414
		private string sourceCountDesc;

		// Token: 0x0400190F RID: 6415
		private bool drawMass;

		// Token: 0x04001910 RID: 6416
		private IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.DontIgnore;

		// Token: 0x04001911 RID: 6417
		private bool includePawnsMassInMassUsage;

		// Token: 0x04001912 RID: 6418
		private Func<float> availableMassGetter;

		// Token: 0x04001913 RID: 6419
		private float extraHeaderSpace;

		// Token: 0x04001914 RID: 6420
		private bool ignoreSpawnedCorpseGearAndInventoryMass;

		// Token: 0x04001915 RID: 6421
		private int tile;

		// Token: 0x04001916 RID: 6422
		private bool drawMarketValue;

		// Token: 0x04001917 RID: 6423
		private bool drawEquippedWeapon;

		// Token: 0x04001918 RID: 6424
		private bool drawNutritionEatenPerDay;

		// Token: 0x04001919 RID: 6425
		private bool drawItemNutrition;

		// Token: 0x0400191A RID: 6426
		private bool drawForagedFoodPerDay;

		// Token: 0x0400191B RID: 6427
		private bool drawDaysUntilRot;

		// Token: 0x0400191C RID: 6428
		private bool playerPawnsReadOnly;

		// Token: 0x0400191D RID: 6429
		private bool transferablesCached;

		// Token: 0x0400191E RID: 6430
		private Vector2 scrollPosition;

		// Token: 0x0400191F RID: 6431
		private TransferableSorterDef sorter1;

		// Token: 0x04001920 RID: 6432
		private TransferableSorterDef sorter2;

		// Token: 0x04001921 RID: 6433
		private static List<TransferableCountToTransferStoppingPoint> stoppingPoints = new List<TransferableCountToTransferStoppingPoint>();

		// Token: 0x04001922 RID: 6434
		private const float TopAreaHeight = 37f;

		// Token: 0x04001923 RID: 6435
		protected readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

		// Token: 0x04001924 RID: 6436
		protected readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

		// Token: 0x04001925 RID: 6437
		private const float ColumnWidth = 120f;

		// Token: 0x04001926 RID: 6438
		private const float FirstTransferableY = 6f;

		// Token: 0x04001927 RID: 6439
		private const float RowInterval = 30f;

		// Token: 0x04001928 RID: 6440
		public const float CountColumnWidth = 75f;

		// Token: 0x04001929 RID: 6441
		public const float AdjustColumnWidth = 240f;

		// Token: 0x0400192A RID: 6442
		public const float MassColumnWidth = 100f;

		// Token: 0x0400192B RID: 6443
		public static readonly Color ItemMassColor = new Color(0.7f, 0.7f, 0.7f);

		// Token: 0x0400192C RID: 6444
		private const float MarketValueColumnWidth = 100f;

		// Token: 0x0400192D RID: 6445
		private const float ExtraSpaceAfterSectionTitle = 5f;

		// Token: 0x0400192E RID: 6446
		private const float DaysUntilRotColumnWidth = 75f;

		// Token: 0x0400192F RID: 6447
		private const float NutritionEatenPerDayColumnWidth = 75f;

		// Token: 0x04001930 RID: 6448
		private const float ItemNutritionColumnWidth = 75f;

		// Token: 0x04001931 RID: 6449
		private const float ForagedFoodPerDayColumnWidth = 75f;

		// Token: 0x04001932 RID: 6450
		private const float GrazeabilityInnerColumnWidth = 40f;

		// Token: 0x04001933 RID: 6451
		private const float EquippedWeaponIconSize = 30f;

		// Token: 0x04001934 RID: 6452
		public const float TopAreaWidth = 515f;

		// Token: 0x04001935 RID: 6453
		private static readonly Texture2D CanGrazeIcon = ContentFinder<Texture2D>.Get("UI/Icons/CanGraze", true);

		// Token: 0x04001936 RID: 6454
		private static readonly Texture2D PregnantIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Pregnant", true);

		// Token: 0x04001937 RID: 6455
		private static readonly Texture2D BondIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Bond", true);

		// Token: 0x04001938 RID: 6456
		[TweakValue("Interface", 0f, 50f)]
		private static float PregnancyIconWidth = 30f;

		// Token: 0x04001939 RID: 6457
		[TweakValue("Interface", 0f, 50f)]
		private static float BondIconWidth = 30f;

		// Token: 0x06002EE0 RID: 12000 RVA: 0x0018F784 File Offset: 0x0018DB84
		public TransferableOneWayWidget(IEnumerable<TransferableOneWay> transferables, string sourceLabel, string destinationLabel, string sourceCountDesc, bool drawMass = false, IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.DontIgnore, bool includePawnsMassInMassUsage = false, Func<float> availableMassGetter = null, float extraHeaderSpace = 0f, bool ignoreSpawnedCorpseGearAndInventoryMass = false, int tile = -1, bool drawMarketValue = false, bool drawEquippedWeapon = false, bool drawNutritionEatenPerDay = false, bool drawItemNutrition = false, bool drawForagedFoodPerDay = false, bool drawDaysUntilRot = false, bool playerPawnsReadOnly = false)
		{
			if (transferables != null)
			{
				this.AddSection(null, transferables);
			}
			this.sourceLabel = sourceLabel;
			this.destinationLabel = destinationLabel;
			this.sourceCountDesc = sourceCountDesc;
			this.drawMass = drawMass;
			this.ignorePawnInventoryMass = ignorePawnInventoryMass;
			this.includePawnsMassInMassUsage = includePawnsMassInMassUsage;
			this.availableMassGetter = availableMassGetter;
			this.extraHeaderSpace = extraHeaderSpace;
			this.ignoreSpawnedCorpseGearAndInventoryMass = ignoreSpawnedCorpseGearAndInventoryMass;
			this.tile = tile;
			this.drawMarketValue = drawMarketValue;
			this.drawEquippedWeapon = drawEquippedWeapon;
			this.drawNutritionEatenPerDay = drawNutritionEatenPerDay;
			this.drawItemNutrition = drawItemNutrition;
			this.drawForagedFoodPerDay = drawForagedFoodPerDay;
			this.drawDaysUntilRot = drawDaysUntilRot;
			this.playerPawnsReadOnly = playerPawnsReadOnly;
			this.sorter1 = TransferableSorterDefOf.Category;
			this.sorter2 = TransferableSorterDefOf.MarketValue;
		}

		// Token: 0x1700077E RID: 1918
		// (get) Token: 0x06002EE1 RID: 12001 RVA: 0x0018F880 File Offset: 0x0018DC80
		public float TotalNumbersColumnsWidths
		{
			get
			{
				float num = 315f;
				if (this.drawMass)
				{
					num += 100f;
				}
				if (this.drawMarketValue)
				{
					num += 100f;
				}
				if (this.drawDaysUntilRot)
				{
					num += 75f;
				}
				if (this.drawItemNutrition)
				{
					num += 75f;
				}
				if (this.drawNutritionEatenPerDay)
				{
					num += 75f;
				}
				if (this.drawForagedFoodPerDay)
				{
					num += 75f;
				}
				return num;
			}
		}

		// Token: 0x1700077F RID: 1919
		// (get) Token: 0x06002EE2 RID: 12002 RVA: 0x0018F910 File Offset: 0x0018DD10
		private bool AnyTransferable
		{
			get
			{
				if (!this.transferablesCached)
				{
					this.CacheTransferables();
				}
				for (int i = 0; i < this.sections.Count; i++)
				{
					if (this.sections[i].cachedTransferables.Any<TransferableOneWay>())
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x06002EE3 RID: 12003 RVA: 0x0018F97C File Offset: 0x0018DD7C
		public void AddSection(string title, IEnumerable<TransferableOneWay> transferables)
		{
			TransferableOneWayWidget.Section item = default(TransferableOneWayWidget.Section);
			item.title = title;
			item.transferables = transferables;
			item.cachedTransferables = new List<TransferableOneWay>();
			this.sections.Add(item);
			this.transferablesCached = false;
		}

		// Token: 0x06002EE4 RID: 12004 RVA: 0x0018F9C4 File Offset: 0x0018DDC4
		private void CacheTransferables()
		{
			this.transferablesCached = true;
			for (int i = 0; i < this.sections.Count; i++)
			{
				List<TransferableOneWay> cachedTransferables = this.sections[i].cachedTransferables;
				cachedTransferables.Clear();
				cachedTransferables.AddRange(this.sections[i].transferables.OrderBy((TransferableOneWay tr) => tr, this.sorter1.Comparer).ThenBy((TransferableOneWay tr) => tr, this.sorter2.Comparer).ThenBy((TransferableOneWay tr) => TransferableUIUtility.DefaultListOrderPriority(tr)).ToList<TransferableOneWay>());
			}
		}

		// Token: 0x06002EE5 RID: 12005 RVA: 0x0018FAB0 File Offset: 0x0018DEB0
		public void OnGUI(Rect inRect)
		{
			bool flag;
			this.OnGUI(inRect, out flag);
		}

		// Token: 0x06002EE6 RID: 12006 RVA: 0x0018FAC8 File Offset: 0x0018DEC8
		public void OnGUI(Rect inRect, out bool anythingChanged)
		{
			if (!this.transferablesCached)
			{
				this.CacheTransferables();
			}
			Profiler.BeginSample("TransferableOneWayWidget.OnGUI()");
			TransferableUIUtility.DoTransferableSorters(this.sorter1, this.sorter2, delegate(TransferableSorterDef x)
			{
				this.sorter1 = x;
				this.CacheTransferables();
			}, delegate(TransferableSorterDef x)
			{
				this.sorter2 = x;
				this.CacheTransferables();
			});
			if (!this.sourceLabel.NullOrEmpty() || !this.destinationLabel.NullOrEmpty())
			{
				float num = inRect.width - 515f;
				Rect position = new Rect(inRect.x + num, inRect.y, inRect.width - num, 37f);
				GUI.BeginGroup(position);
				Text.Font = GameFont.Medium;
				if (!this.sourceLabel.NullOrEmpty())
				{
					Rect rect = new Rect(0f, 0f, position.width / 2f, position.height);
					Text.Anchor = TextAnchor.UpperLeft;
					Widgets.Label(rect, this.sourceLabel);
				}
				if (!this.destinationLabel.NullOrEmpty())
				{
					Rect rect2 = new Rect(position.width / 2f, 0f, position.width / 2f, position.height);
					Text.Anchor = TextAnchor.UpperRight;
					Widgets.Label(rect2, this.destinationLabel);
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.EndGroup();
			}
			Rect mainRect = new Rect(inRect.x, inRect.y + 37f + this.extraHeaderSpace, inRect.width, inRect.height - 37f - this.extraHeaderSpace);
			this.FillMainRect(mainRect, out anythingChanged);
			Profiler.EndSample();
		}

		// Token: 0x06002EE7 RID: 12007 RVA: 0x0018FC78 File Offset: 0x0018E078
		private void FillMainRect(Rect mainRect, out bool anythingChanged)
		{
			anythingChanged = false;
			Text.Font = GameFont.Small;
			if (this.AnyTransferable)
			{
				float num = 6f;
				for (int i = 0; i < this.sections.Count; i++)
				{
					num += (float)this.sections[i].cachedTransferables.Count * 30f;
					if (this.sections[i].title != null)
					{
						num += 30f;
					}
				}
				float num2 = 6f;
				float availableMass = (this.availableMassGetter == null) ? float.MaxValue : this.availableMassGetter();
				Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, num);
				Widgets.BeginScrollView(mainRect, ref this.scrollPosition, viewRect, true);
				float num3 = this.scrollPosition.y - 30f;
				float num4 = this.scrollPosition.y + mainRect.height;
				for (int j = 0; j < this.sections.Count; j++)
				{
					List<TransferableOneWay> cachedTransferables = this.sections[j].cachedTransferables;
					if (cachedTransferables.Any<TransferableOneWay>())
					{
						if (this.sections[j].title != null)
						{
							Widgets.ListSeparator(ref num2, viewRect.width, this.sections[j].title);
							num2 += 5f;
						}
						for (int k = 0; k < cachedTransferables.Count; k++)
						{
							if (num2 > num3 && num2 < num4)
							{
								Rect rect = new Rect(0f, num2, viewRect.width, 30f);
								int countToTransfer = cachedTransferables[k].CountToTransfer;
								Profiler.BeginSample("DoRow()");
								this.DoRow(rect, cachedTransferables[k], k, availableMass);
								Profiler.EndSample();
								if (countToTransfer != cachedTransferables[k].CountToTransfer)
								{
									anythingChanged = true;
								}
							}
							num2 += 30f;
						}
					}
				}
				Widgets.EndScrollView();
			}
			else
			{
				GUI.color = Color.gray;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(mainRect, "NoneBrackets".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
		}

		// Token: 0x06002EE8 RID: 12008 RVA: 0x0018FEFC File Offset: 0x0018E2FC
		private void DoRow(Rect rect, TransferableOneWay trad, int index, float availableMass)
		{
			if (index % 2 == 1)
			{
				Widgets.DrawLightHighlight(rect);
			}
			Text.Font = GameFont.Small;
			GUI.BeginGroup(rect);
			float num = rect.width;
			int maxCount = trad.MaxCount;
			Rect rect2 = new Rect(num - 240f, 0f, 240f, rect.height);
			TransferableOneWayWidget.stoppingPoints.Clear();
			if (this.availableMassGetter != null && (!(trad.AnyThing is Pawn) || this.includePawnsMassInMassUsage))
			{
				float num2 = availableMass + this.GetMass(trad.AnyThing) * (float)trad.CountToTransfer;
				int threshold = (num2 > 0f) ? Mathf.FloorToInt(num2 / this.GetMass(trad.AnyThing)) : 0;
				TransferableOneWayWidget.stoppingPoints.Add(new TransferableCountToTransferStoppingPoint(threshold, "M<", ">M"));
			}
			Pawn pawn = trad.AnyThing as Pawn;
			bool flag = pawn != null && (pawn.IsColonist || pawn.IsPrisonerOfColony);
			Profiler.BeginSample("DoCountAdjustInterface()");
			Rect rect3 = rect2;
			int min = 0;
			int max = maxCount;
			List<TransferableCountToTransferStoppingPoint> extraStoppingPoints = TransferableOneWayWidget.stoppingPoints;
			TransferableUIUtility.DoCountAdjustInterface(rect3, trad, index, min, max, false, extraStoppingPoints, this.playerPawnsReadOnly && flag);
			Profiler.EndSample();
			num -= 240f;
			if (this.drawMarketValue)
			{
				Rect rect4 = new Rect(num - 100f, 0f, 100f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				Profiler.BeginSample("DrawMarketValue()");
				this.DrawMarketValue(rect4, trad);
				Profiler.EndSample();
				num -= 100f;
			}
			if (this.drawMass)
			{
				Rect rect5 = new Rect(num - 100f, 0f, 100f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				Profiler.BeginSample("DrawMass()");
				this.DrawMass(rect5, trad, availableMass);
				Profiler.EndSample();
				num -= 100f;
			}
			if (this.drawDaysUntilRot)
			{
				Rect rect6 = new Rect(num - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				Profiler.BeginSample("DrawDaysUntilRot()");
				this.DrawDaysUntilRot(rect6, trad);
				Profiler.EndSample();
				num -= 75f;
			}
			if (this.drawItemNutrition)
			{
				Rect rect7 = new Rect(num - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				Profiler.BeginSample("DrawItemNutrition()");
				this.DrawItemNutrition(rect7, trad);
				Profiler.EndSample();
				num -= 75f;
			}
			if (this.drawForagedFoodPerDay)
			{
				Rect rect8 = new Rect(num - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				Profiler.BeginSample("DrawGrazeability()");
				bool flag2 = this.DrawGrazeability(rect8, trad);
				Profiler.EndSample();
				if (!flag2)
				{
					Profiler.BeginSample("DrawForagedFoodPerDay()");
					this.DrawForagedFoodPerDay(rect8, trad);
					Profiler.EndSample();
				}
				num -= 75f;
			}
			if (this.drawNutritionEatenPerDay)
			{
				Rect rect9 = new Rect(num - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				Profiler.BeginSample("DrawNutritionEatenPerDay()");
				this.DrawNutritionEatenPerDay(rect9, trad);
				Profiler.EndSample();
				num -= 75f;
			}
			if (this.ShouldShowCount(trad))
			{
				Rect rect10 = new Rect(num - 75f, 0f, 75f, rect.height);
				Widgets.DrawHighlightIfMouseover(rect10);
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect11 = rect10;
				rect11.xMin += 5f;
				rect11.xMax -= 5f;
				Widgets.Label(rect11, maxCount.ToStringCached());
				TooltipHandler.TipRegion(rect10, this.sourceCountDesc);
			}
			num -= 75f;
			if (this.drawEquippedWeapon)
			{
				Rect rect12 = new Rect(num - 30f, 0f, 30f, rect.height);
				Rect iconRect = new Rect(num - 30f, (rect.height - 30f) / 2f, 30f, 30f);
				Profiler.BeginSample("DrawEquippedWeapon()");
				this.DrawEquippedWeapon(rect12, iconRect, trad);
				Profiler.EndSample();
				num -= 30f;
			}
			Pawn pawn2 = trad.AnyThing as Pawn;
			if (pawn2 != null && pawn2.def.race.Animal)
			{
				Rect rect13 = new Rect(num - TransferableOneWayWidget.BondIconWidth, (rect.height - TransferableOneWayWidget.BondIconWidth) / 2f, TransferableOneWayWidget.BondIconWidth, TransferableOneWayWidget.BondIconWidth);
				num -= TransferableOneWayWidget.BondIconWidth;
				Rect rect14 = new Rect(num - TransferableOneWayWidget.PregnancyIconWidth, (rect.height - TransferableOneWayWidget.PregnancyIconWidth) / 2f, TransferableOneWayWidget.PregnancyIconWidth, TransferableOneWayWidget.PregnancyIconWidth);
				num -= TransferableOneWayWidget.PregnancyIconWidth;
				string iconTooltipText = TrainableUtility.GetIconTooltipText(pawn2);
				if (!iconTooltipText.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect13, iconTooltipText);
				}
				if (pawn2.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, null) != null)
				{
					GUI.DrawTexture(rect13, TransferableOneWayWidget.BondIcon);
				}
				if (pawn2.health.hediffSet.HasHediff(HediffDefOf.Pregnant, true))
				{
					TooltipHandler.TipRegion(rect14, PawnColumnWorker_Pregnant.GetTooltipText(pawn2));
					GUI.DrawTexture(rect14, TransferableOneWayWidget.PregnantIcon);
				}
			}
			Rect idRect = new Rect(0f, 0f, num, rect.height);
			Profiler.BeginSample("DrawTransferableInfo()");
			TransferableUIUtility.DrawTransferableInfo(trad, idRect, Color.white);
			Profiler.EndSample();
			GenUI.ResetLabelAlign();
			GUI.EndGroup();
		}

		// Token: 0x06002EE9 RID: 12009 RVA: 0x001904CC File Offset: 0x0018E8CC
		private bool ShouldShowCount(TransferableOneWay trad)
		{
			bool result;
			if (!trad.HasAnyThing)
			{
				result = true;
			}
			else
			{
				Pawn pawn = trad.AnyThing as Pawn;
				result = (pawn == null || !pawn.RaceProps.Humanlike || trad.MaxCount != 1);
			}
			return result;
		}

		// Token: 0x06002EEA RID: 12010 RVA: 0x00190524 File Offset: 0x0018E924
		private void DrawDaysUntilRot(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				if (trad.ThingDef.IsNutritionGivingIngestible)
				{
					int num = int.MaxValue;
					for (int i = 0; i < trad.things.Count; i++)
					{
						CompRottable compRottable = trad.things[i].TryGetComp<CompRottable>();
						if (compRottable != null)
						{
							num = Mathf.Min(num, DaysUntilRotCalculator.ApproxTicksUntilRot_AssumeTimePassesBy(compRottable, this.tile, null));
						}
					}
					if (num < 36000000)
					{
						Widgets.DrawHighlightIfMouseover(rect);
						float num2 = (float)num / 60000f;
						GUI.color = Color.yellow;
						Widgets.Label(rect, num2.ToString("0.#"));
						GUI.color = Color.white;
						TooltipHandler.TipRegion(rect, "DaysUntilRotTip".Translate());
					}
				}
			}
		}

		// Token: 0x06002EEB RID: 12011 RVA: 0x00190604 File Offset: 0x0018EA04
		private void DrawItemNutrition(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				if (trad.ThingDef.IsNutritionGivingIngestible)
				{
					Widgets.DrawHighlightIfMouseover(rect);
					GUI.color = Color.green;
					Widgets.Label(rect, trad.ThingDef.GetStatValueAbstract(StatDefOf.Nutrition, null).ToString("0.##"));
					GUI.color = Color.white;
					TooltipHandler.TipRegion(rect, "ItemNutritionTip".Translate(new object[]
					{
						(1.6f * ThingDefOf.Human.race.baseHungerRate).ToString("0.##")
					}));
				}
			}
		}

		// Token: 0x06002EEC RID: 12012 RVA: 0x001906B8 File Offset: 0x0018EAB8
		private bool DrawGrazeability(Rect rect, TransferableOneWay trad)
		{
			bool result;
			if (!trad.HasAnyThing)
			{
				result = false;
			}
			else
			{
				Pawn pawn = trad.AnyThing as Pawn;
				if (pawn == null || !VirtualPlantsUtility.CanEverEatVirtualPlants(pawn))
				{
					result = false;
				}
				else
				{
					rect.width = 40f;
					Rect position = new Rect(rect.x + (float)((int)((rect.width - 28f) / 2f)), rect.y + (float)((int)((rect.height - 28f) / 2f)), 28f, 28f);
					Widgets.DrawHighlightIfMouseover(rect);
					GUI.DrawTexture(position, TransferableOneWayWidget.CanGrazeIcon);
					TooltipHandler.TipRegion(rect, delegate()
					{
						string text = "AnimalCanGrazeTip".Translate();
						if (this.tile != -1)
						{
							text = text + "\n\n" + VirtualPlantsUtility.GetVirtualPlantsStatusExplanationAt(this.tile, Find.TickManager.TicksAbs);
						}
						return text;
					}, trad.GetHashCode() ^ 1948571634);
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06002EED RID: 12013 RVA: 0x0019078C File Offset: 0x0018EB8C
		private void DrawForagedFoodPerDay(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				Pawn p = trad.AnyThing as Pawn;
				if (p != null)
				{
					bool flag;
					float foragedNutritionPerDay = ForagedFoodPerDayCalculator.GetBaseForagedNutritionPerDay(p, out flag);
					if (!flag)
					{
						Widgets.DrawHighlightIfMouseover(rect);
						GUI.color = ((foragedNutritionPerDay != 0f) ? Color.green : Color.gray);
						Widgets.Label(rect, "+" + foragedNutritionPerDay.ToString("0.##"));
						GUI.color = Color.white;
						TooltipHandler.TipRegion(rect, () => "NutritionForagedPerDayTip".Translate(new object[]
						{
							StatDefOf.ForagedNutritionPerDay.Worker.GetExplanationFull(StatRequest.For(p), StatDefOf.ForagedNutritionPerDay.toStringNumberSense, foragedNutritionPerDay)
						}), trad.GetHashCode() ^ 1958671422);
					}
				}
			}
		}

		// Token: 0x06002EEE RID: 12014 RVA: 0x00190864 File Offset: 0x0018EC64
		private void DrawNutritionEatenPerDay(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				Pawn p = trad.AnyThing as Pawn;
				if (p != null && p.RaceProps.EatsFood && !p.Dead && p.needs.food != null)
				{
					Widgets.DrawHighlightIfMouseover(rect);
					string text = (p.needs.food.FoodFallPerTick * 60000f).ToString("0.##");
					DietCategory resolvedDietCategory = p.RaceProps.ResolvedDietCategory;
					if (resolvedDietCategory != DietCategory.Omnivorous)
					{
						text = text + " (" + resolvedDietCategory.ToStringHumanShort() + ")";
					}
					GUI.color = new Color(1f, 0.5f, 0f);
					Widgets.Label(rect, text);
					GUI.color = Color.white;
					TooltipHandler.TipRegion(rect, delegate()
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.Append("NoDietCategoryLetter".Translate() + " - " + DietCategory.Omnivorous.ToStringHuman());
						DietCategory[] array = (DietCategory[])Enum.GetValues(typeof(DietCategory));
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i] != DietCategory.NeverEats && array[i] != DietCategory.Omnivorous)
							{
								stringBuilder.AppendLine();
								stringBuilder.Append(array[i].ToStringHumanShort() + " - " + array[i].ToStringHuman());
							}
						}
						return "NutritionEatenPerDayTip".Translate(new object[]
						{
							ThingDefOf.MealSimple.GetStatValueAbstract(StatDefOf.Nutrition, null).ToString("0.##"),
							stringBuilder.ToString(),
							p.RaceProps.foodType.ToHumanString()
						});
					}, trad.GetHashCode() ^ 385968958);
				}
			}
		}

		// Token: 0x06002EEF RID: 12015 RVA: 0x0019098C File Offset: 0x0018ED8C
		private void DrawMarketValue(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				Widgets.DrawHighlightIfMouseover(rect);
				Widgets.Label(rect, trad.AnyThing.MarketValue.ToStringMoney());
				TooltipHandler.TipRegion(rect, "MarketValueTip".Translate());
			}
		}

		// Token: 0x06002EF0 RID: 12016 RVA: 0x001909DC File Offset: 0x0018EDDC
		private void DrawMass(Rect rect, TransferableOneWay trad, float availableMass)
		{
			if (trad.HasAnyThing)
			{
				Thing anyThing = trad.AnyThing;
				Pawn pawn = anyThing as Pawn;
				if (pawn == null || this.includePawnsMassInMassUsage || MassUtility.CanEverCarryAnything(pawn))
				{
					Widgets.DrawHighlightIfMouseover(rect);
					if (pawn == null || this.includePawnsMassInMassUsage)
					{
						float mass = this.GetMass(anyThing);
						if (pawn != null)
						{
							float gearMass = 0f;
							float invMass = 0f;
							gearMass = MassUtility.GearMass(pawn);
							if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, this.ignorePawnInventoryMass))
							{
								invMass = MassUtility.InventoryMass(pawn);
							}
							TooltipHandler.TipRegion(rect, () => this.GetPawnMassTip(trad, 0f, mass - gearMass - invMass, gearMass, invMass), trad.GetHashCode() * 59);
						}
						else
						{
							TooltipHandler.TipRegion(rect, "ItemWeightTip".Translate());
						}
						if (mass > availableMass)
						{
							GUI.color = Color.red;
						}
						else
						{
							GUI.color = TransferableOneWayWidget.ItemMassColor;
						}
						Widgets.Label(rect, mass.ToStringMass());
					}
					else
					{
						float cap = MassUtility.Capacity(pawn, null);
						float gearMass = MassUtility.GearMass(pawn);
						float invMass = (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, this.ignorePawnInventoryMass)) ? MassUtility.InventoryMass(pawn) : 0f;
						float num = cap - gearMass - invMass;
						if (num > 0f)
						{
							GUI.color = Color.green;
						}
						else if (num < 0f)
						{
							GUI.color = Color.red;
						}
						else
						{
							GUI.color = Color.gray;
						}
						Widgets.Label(rect, num.ToStringMassOffset());
						TooltipHandler.TipRegion(rect, () => this.GetPawnMassTip(trad, cap, 0f, gearMass, invMass), trad.GetHashCode() * 59);
					}
					GUI.color = Color.white;
				}
			}
		}

		// Token: 0x06002EF1 RID: 12017 RVA: 0x00190C30 File Offset: 0x0018F030
		private void DrawEquippedWeapon(Rect rect, Rect iconRect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				Pawn pawn = trad.AnyThing as Pawn;
				if (pawn != null && pawn.equipment != null && pawn.equipment.Primary != null)
				{
					ThingWithComps primary = pawn.equipment.Primary;
					Widgets.DrawHighlightIfMouseover(rect);
					Widgets.ThingIcon(iconRect, primary, 1f);
					TooltipHandler.TipRegion(rect, primary.LabelCap);
				}
			}
		}

		// Token: 0x06002EF2 RID: 12018 RVA: 0x00190CB0 File Offset: 0x0018F0B0
		private string GetPawnMassTip(TransferableOneWay trad, float capacity, float pawnMass, float gearMass, float invMass)
		{
			string result;
			if (!trad.HasAnyThing)
			{
				result = "";
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (capacity != 0f)
				{
					stringBuilder.Append("MassCapacity".Translate() + ": " + capacity.ToStringMass());
				}
				else
				{
					stringBuilder.Append("Mass".Translate() + ": " + pawnMass.ToStringMass());
				}
				if (gearMass != 0f)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("EquipmentAndApparelMass".Translate() + ": " + gearMass.ToStringMass());
				}
				if (invMass != 0f)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("InventoryMass".Translate() + ": " + invMass.ToStringMass());
				}
				result = stringBuilder.ToString();
			}
			return result;
		}

		// Token: 0x06002EF3 RID: 12019 RVA: 0x00190DA8 File Offset: 0x0018F1A8
		private float GetMass(Thing thing)
		{
			float result;
			if (thing == null)
			{
				result = 0f;
			}
			else
			{
				float num = thing.GetStatValue(StatDefOf.Mass, true);
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					if (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, this.ignorePawnInventoryMass))
					{
						num -= MassUtility.InventoryMass(pawn);
					}
				}
				else if (this.ignoreSpawnedCorpseGearAndInventoryMass)
				{
					Corpse corpse = thing as Corpse;
					if (corpse != null && corpse.Spawned)
					{
						num -= MassUtility.GearAndInventoryMass(corpse.InnerPawn);
					}
				}
				result = num;
			}
			return result;
		}

		// Token: 0x02000825 RID: 2085
		private struct Section
		{
			// Token: 0x0400193D RID: 6461
			public string title;

			// Token: 0x0400193E RID: 6462
			public IEnumerable<TransferableOneWay> transferables;

			// Token: 0x0400193F RID: 6463
			public List<TransferableOneWay> cachedTransferables;
		}
	}
}
