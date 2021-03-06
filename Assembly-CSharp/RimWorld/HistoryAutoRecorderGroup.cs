﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderGroup : IExposable
	{
		public HistoryAutoRecorderGroupDef def;

		public List<HistoryAutoRecorder> recorders = new List<HistoryAutoRecorder>();

		private List<SimpleCurveDrawInfo> curves = new List<SimpleCurveDrawInfo>();

		private int cachedGraphTickCount = -1;

		[CompilerGenerated]
		private static Predicate<HistoryAutoRecorder> <>f__am$cache0;

		[CompilerGenerated]
		private static Predicate<HistoryAutoRecorder> <>f__am$cache1;

		public HistoryAutoRecorderGroup()
		{
		}

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

		public void Tick()
		{
			for (int i = 0; i < this.recorders.Count; i++)
			{
				this.recorders[i].Tick();
			}
		}

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
			curveDrawerStyle.OnlyPositiveValues = this.def.onlyPositiveValues;
			SimpleCurveDrawer.DrawCurves(graphRect, this.curves, curveDrawerStyle, marks, legendRect);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look<HistoryAutoRecorderGroupDef>(ref this.def, "def");
			Scribe_Collections.Look<HistoryAutoRecorder>(ref this.recorders, "recorders", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.AddOrRemoveHistoryRecorders();
			}
		}

		public void AddOrRemoveHistoryRecorders()
		{
			if (this.recorders.RemoveAll((HistoryAutoRecorder x) => x == null) != 0)
			{
				Log.Warning("Some history auto recorders were null.", false);
			}
			using (List<HistoryAutoRecorderDef>.Enumerator enumerator = this.def.historyAutoRecorderDefs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					HistoryAutoRecorderDef recorderDef = enumerator.Current;
					if (!this.recorders.Any((HistoryAutoRecorder x) => x.def == recorderDef))
					{
						HistoryAutoRecorder historyAutoRecorder = new HistoryAutoRecorder();
						historyAutoRecorder.def = recorderDef;
						this.recorders.Add(historyAutoRecorder);
					}
				}
			}
			this.recorders.RemoveAll((HistoryAutoRecorder x) => x.def == null);
		}

		[CompilerGenerated]
		private static bool <AddOrRemoveHistoryRecorders>m__0(HistoryAutoRecorder x)
		{
			return x == null;
		}

		[CompilerGenerated]
		private static bool <AddOrRemoveHistoryRecorders>m__1(HistoryAutoRecorder x)
		{
			return x.def == null;
		}

		[CompilerGenerated]
		private sealed class <AddOrRemoveHistoryRecorders>c__AnonStorey0
		{
			internal HistoryAutoRecorderDef recorderDef;

			public <AddOrRemoveHistoryRecorders>c__AnonStorey0()
			{
			}

			internal bool <>m__0(HistoryAutoRecorder x)
			{
				return x.def == this.recorderDef;
			}
		}
	}
}
