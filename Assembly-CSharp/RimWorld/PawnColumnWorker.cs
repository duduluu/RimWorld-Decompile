﻿using System;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	// Token: 0x020002B3 RID: 691
	[StaticConstructorOnStartup]
	public abstract class PawnColumnWorker
	{
		// Token: 0x040006AA RID: 1706
		public PawnColumnDef def;

		// Token: 0x040006AB RID: 1707
		protected const int DefaultCellHeight = 30;

		// Token: 0x040006AC RID: 1708
		private static readonly Texture2D SortingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting", true);

		// Token: 0x040006AD RID: 1709
		private static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending", true);

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x06000B8C RID: 2956 RVA: 0x000684A0 File Offset: 0x000668A0
		protected virtual Color DefaultHeaderColor
		{
			get
			{
				return Color.white;
			}
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x06000B8D RID: 2957 RVA: 0x000684BC File Offset: 0x000668BC
		protected virtual GameFont DefaultHeaderFont
		{
			get
			{
				return GameFont.Small;
			}
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x000684D4 File Offset: 0x000668D4
		public virtual void DoHeader(Rect rect, PawnTable table)
		{
			if (!this.def.label.NullOrEmpty())
			{
				Text.Font = this.DefaultHeaderFont;
				GUI.color = this.DefaultHeaderColor;
				Text.Anchor = TextAnchor.LowerCenter;
				Rect rect2 = rect;
				rect2.y += 3f;
				Widgets.Label(rect2, this.def.LabelCap.Truncate(rect.width, null));
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				Text.Font = GameFont.Small;
			}
			else if (this.def.HeaderIcon != null)
			{
				Vector2 headerIconSize = this.def.HeaderIconSize;
				int num = (int)((rect.width - headerIconSize.x) / 2f);
				Rect position = new Rect(rect.x + (float)num, rect.yMax - headerIconSize.y, headerIconSize.x, headerIconSize.y);
				GUI.DrawTexture(position, this.def.HeaderIcon);
			}
			if (table.SortingBy == this.def)
			{
				Texture2D texture2D = (!table.SortingDescending) ? PawnColumnWorker.SortingIcon : PawnColumnWorker.SortingDescendingIcon;
				Rect position2 = new Rect(rect.xMax - (float)texture2D.width - 1f, rect.yMax - (float)texture2D.height - 1f, (float)texture2D.width, (float)texture2D.height);
				GUI.DrawTexture(position2, texture2D);
			}
			if (this.def.HeaderInteractable)
			{
				Rect interactableHeaderRect = this.GetInteractableHeaderRect(rect, table);
				Widgets.DrawHighlightIfMouseover(interactableHeaderRect);
				if (interactableHeaderRect.Contains(Event.current.mousePosition))
				{
					string headerTip = this.GetHeaderTip(table);
					if (!headerTip.NullOrEmpty())
					{
						TooltipHandler.TipRegion(interactableHeaderRect, headerTip);
					}
				}
				if (Widgets.ButtonInvisible(interactableHeaderRect, false))
				{
					this.HeaderClicked(rect, table);
				}
			}
		}

		// Token: 0x06000B8F RID: 2959
		public abstract void DoCell(Rect rect, Pawn pawn, PawnTable table);

		// Token: 0x06000B90 RID: 2960 RVA: 0x000686CC File Offset: 0x00066ACC
		public virtual int GetMinWidth(PawnTable table)
		{
			int result;
			if (!this.def.label.NullOrEmpty())
			{
				Text.Font = this.DefaultHeaderFont;
				int num = Mathf.CeilToInt(Text.CalcSize(this.def.LabelCap).x);
				Text.Font = GameFont.Small;
				result = num;
			}
			else if (this.def.HeaderIcon != null)
			{
				result = Mathf.CeilToInt(this.def.HeaderIconSize.x);
			}
			else
			{
				result = 1;
			}
			return result;
		}

		// Token: 0x06000B91 RID: 2961 RVA: 0x00068764 File Offset: 0x00066B64
		public virtual int GetMaxWidth(PawnTable table)
		{
			return 1000000;
		}

		// Token: 0x06000B92 RID: 2962 RVA: 0x00068780 File Offset: 0x00066B80
		public virtual int GetOptimalWidth(PawnTable table)
		{
			return this.GetMinWidth(table);
		}

		// Token: 0x06000B93 RID: 2963 RVA: 0x0006879C File Offset: 0x00066B9C
		public virtual int GetMinCellHeight(Pawn pawn)
		{
			return 30;
		}

		// Token: 0x06000B94 RID: 2964 RVA: 0x000687B4 File Offset: 0x00066BB4
		public virtual int GetMinHeaderHeight(PawnTable table)
		{
			int result;
			if (!this.def.label.NullOrEmpty())
			{
				Text.Font = this.DefaultHeaderFont;
				int num = Mathf.CeilToInt(Text.CalcSize(this.def.LabelCap).y);
				Text.Font = GameFont.Small;
				result = num;
			}
			else if (this.def.HeaderIcon != null)
			{
				result = Mathf.CeilToInt(this.def.HeaderIconSize.y);
			}
			else
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x06000B95 RID: 2965 RVA: 0x0006884C File Offset: 0x00066C4C
		public virtual int Compare(Pawn a, Pawn b)
		{
			return 0;
		}

		// Token: 0x06000B96 RID: 2966 RVA: 0x00068864 File Offset: 0x00066C64
		protected virtual Rect GetInteractableHeaderRect(Rect headerRect, PawnTable table)
		{
			float num = Mathf.Min(25f, headerRect.height);
			return new Rect(headerRect.x, headerRect.yMax - num, headerRect.width, num);
		}

		// Token: 0x06000B97 RID: 2967 RVA: 0x000688A8 File Offset: 0x00066CA8
		protected virtual void HeaderClicked(Rect headerRect, PawnTable table)
		{
			if (this.def.sortable && !Event.current.shift)
			{
				if (Event.current.button == 0)
				{
					if (table.SortingBy != this.def)
					{
						table.SortBy(this.def, true);
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					}
					else if (table.SortingDescending)
					{
						table.SortBy(this.def, false);
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					}
					else
					{
						table.SortBy(null, false);
						SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
					}
				}
				else if (Event.current.button == 1)
				{
					if (table.SortingBy != this.def)
					{
						table.SortBy(this.def, false);
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					}
					else if (table.SortingDescending)
					{
						table.SortBy(null, false);
						SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
					}
					else
					{
						table.SortBy(this.def, true);
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					}
				}
			}
		}

		// Token: 0x06000B98 RID: 2968 RVA: 0x000689E0 File Offset: 0x00066DE0
		protected virtual string GetHeaderTip(PawnTable table)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!this.def.headerTip.NullOrEmpty())
			{
				stringBuilder.Append(this.def.headerTip);
			}
			if (this.def.sortable)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("ClickToSortByThisColumn".Translate());
			}
			return stringBuilder.ToString();
		}
	}
}
