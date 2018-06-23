﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x020002FA RID: 762
	public class HistoryAutoRecorderGroup : IExposable
	{
		// Token: 0x0400084E RID: 2126
		public HistoryAutoRecorderGroupDef def = null;

		// Token: 0x0400084F RID: 2127
		public List<HistoryAutoRecorder> recorders;

		// Token: 0x04000850 RID: 2128
		private List<SimpleCurveDrawInfo> curves;

		// Token: 0x04000851 RID: 2129
		private int cachedGraphTickCount = -1;

		// Token: 0x06000CB5 RID: 3253 RVA: 0x0006FF3B File Offset: 0x0006E33B
		public HistoryAutoRecorderGroup()
		{
			this.recorders = new List<HistoryAutoRecorder>();
			this.curves = new List<SimpleCurveDrawInfo>();
		}

		// Token: 0x06000CB6 RID: 3254 RVA: 0x0006FF68 File Offset: 0x0006E368
		public void CreateRecorders()
		{
			foreach (HistoryAutoRecorderDef historyAutoRecorderDef in this.def.historyAutoRecorderDefs)
			{
				HistoryAutoRecorder historyAutoRecorder = new HistoryAutoRecorder();
				historyAutoRecorder.def = historyAutoRecorderDef;
				this.recorders.Add(historyAutoRecorder);
			}
		}

		// Token: 0x06000CB7 RID: 3255 RVA: 0x0006FFE0 File Offset: 0x0006E3E0
		public float GetMaxDay()
		{
			float num = 0f;
			foreach (HistoryAutoRecorder historyAutoRecorder in this.recorders)
			{
				int count = historyAutoRecorder.records.Count;
				if (count != 0)
				{
					float num2 = (float)((count - 1) * historyAutoRecorder.def.recordTicksFrequency) / 60000f;
					if (num2 > num)
					{
						num = num2;
					}
				}
			}
			return num;
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x00070084 File Offset: 0x0006E484
		public void Tick()
		{
			for (int i = 0; i < this.recorders.Count; i++)
			{
				this.recorders[i].Tick();
			}
		}

		// Token: 0x06000CB9 RID: 3257 RVA: 0x000700C4 File Offset: 0x0006E4C4
		public void DrawGraph(Rect graphRect, Rect legendRect, FloatRange section, List<CurveMark> marks)
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (ticksGame != this.cachedGraphTickCount)
			{
				this.cachedGraphTickCount = ticksGame;
				this.curves.Clear();
				for (int i = 0; i < this.recorders.Count; i++)
				{
					HistoryAutoRecorder historyAutoRecorder = this.recorders[i];
					SimpleCurveDrawInfo simpleCurveDrawInfo = new SimpleCurveDrawInfo();
					simpleCurveDrawInfo.color = historyAutoRecorder.def.graphColor;
					simpleCurveDrawInfo.label = historyAutoRecorder.def.LabelCap;
					simpleCurveDrawInfo.labelY = historyAutoRecorder.def.GraphLabelY;
					simpleCurveDrawInfo.curve = new SimpleCurve();
					for (int j = 0; j < historyAutoRecorder.records.Count; j++)
					{
						simpleCurveDrawInfo.curve.Add(new CurvePoint((float)j * (float)historyAutoRecorder.def.recordTicksFrequency / 60000f, historyAutoRecorder.records[j]), false);
					}
					simpleCurveDrawInfo.curve.SortPoints();
					if (historyAutoRecorder.records.Count == 1)
					{
						simpleCurveDrawInfo.curve.Add(new CurvePoint(1.66666669E-05f, historyAutoRecorder.records[0]), true);
					}
					this.curves.Add(simpleCurveDrawInfo);
				}
			}
			if (Mathf.Approximately(section.min, section.max))
			{
				section.max += 1.66666669E-05f;
			}
			SimpleCurveDrawerStyle curveDrawerStyle = Find.History.curveDrawerStyle;
			curveDrawerStyle.FixedSection = section;
			curveDrawerStyle.UseFixedScale = this.def.useFixedScale;
			curveDrawerStyle.FixedScale = this.def.fixedScale;
			curveDrawerStyle.YIntegersOnly = this.def.integersOnly;
			SimpleCurveDrawer.DrawCurves(graphRect, this.curves, curveDrawerStyle, marks, legendRect);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		// Token: 0x06000CBA RID: 3258 RVA: 0x0007029A File Offset: 0x0006E69A
		public void ExposeData()
		{
			Scribe_Defs.Look<HistoryAutoRecorderGroupDef>(ref this.def, "def");
			Scribe_Collections.Look<HistoryAutoRecorder>(ref this.recorders, "recorders", LookMode.Deep, new object[0]);
		}
	}
}
