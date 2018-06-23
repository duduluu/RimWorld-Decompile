﻿using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	// Token: 0x02000840 RID: 2112
	public static class TimeControls
	{
		// Token: 0x040019D6 RID: 6614
		public static readonly Vector2 TimeButSize = new Vector2(32f, 24f);

		// Token: 0x040019D7 RID: 6615
		private static readonly TimeSpeed[] CachedTimeSpeedValues = (TimeSpeed[])Enum.GetValues(typeof(TimeSpeed));

		// Token: 0x06002FCF RID: 12239 RVA: 0x0019EC8C File Offset: 0x0019D08C
		private static void PlaySoundOf(TimeSpeed speed)
		{
			SoundDef soundDef = null;
			switch (speed)
			{
			case TimeSpeed.Paused:
				soundDef = SoundDefOf.Clock_Stop;
				break;
			case TimeSpeed.Normal:
				soundDef = SoundDefOf.Clock_Normal;
				break;
			case TimeSpeed.Fast:
				soundDef = SoundDefOf.Clock_Fast;
				break;
			case TimeSpeed.Superfast:
				soundDef = SoundDefOf.Clock_Superfast;
				break;
			case TimeSpeed.Ultrafast:
				soundDef = SoundDefOf.Clock_Superfast;
				break;
			}
			if (soundDef != null)
			{
				soundDef.PlayOneShotOnCamera(null);
			}
		}

		// Token: 0x06002FD0 RID: 12240 RVA: 0x0019ED00 File Offset: 0x0019D100
		public static void DoTimeControlsGUI(Rect timerRect)
		{
			TickManager tickManager = Find.TickManager;
			GUI.BeginGroup(timerRect);
			Rect rect = new Rect(0f, 0f, TimeControls.TimeButSize.x, TimeControls.TimeButSize.y);
			for (int i = 0; i < TimeControls.CachedTimeSpeedValues.Length; i++)
			{
				TimeSpeed timeSpeed = TimeControls.CachedTimeSpeedValues[i];
				if (timeSpeed != TimeSpeed.Ultrafast)
				{
					if (Widgets.ButtonImage(rect, TexButton.SpeedButtonTextures[(int)timeSpeed]))
					{
						if (timeSpeed == TimeSpeed.Paused)
						{
							tickManager.TogglePaused();
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
						}
						else
						{
							tickManager.CurTimeSpeed = timeSpeed;
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
						}
						TimeControls.PlaySoundOf(tickManager.CurTimeSpeed);
					}
					if (tickManager.CurTimeSpeed == timeSpeed)
					{
						GUI.DrawTexture(rect, TexUI.HighlightTex);
					}
					rect.x += rect.width;
				}
			}
			if (Find.TickManager.slower.ForcedNormalSpeed)
			{
				Widgets.DrawLineHorizontal(rect.width * 2f, rect.height / 2f, rect.width * 2f);
			}
			GUI.EndGroup();
			GenUI.AbsorbClicksInRect(timerRect);
			UIHighlighter.HighlightOpportunity(timerRect, "TimeControls");
			if (Event.current.type == EventType.KeyDown)
			{
				if (KeyBindingDefOf.TogglePause.KeyDownEvent)
				{
					Find.TickManager.TogglePaused();
					TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (KeyBindingDefOf.TimeSpeed_Normal.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
					TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (KeyBindingDefOf.TimeSpeed_Fast.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Fast;
					TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (KeyBindingDefOf.TimeSpeed_Superfast.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
					TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (Prefs.DevMode)
				{
					if (KeyBindingDefOf.TimeSpeed_Ultrafast.KeyDownEvent)
					{
						Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
						TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
						Event.current.Use();
					}
					if (KeyBindingDefOf.Dev_TickOnce.KeyDownEvent && tickManager.CurTimeSpeed == TimeSpeed.Paused)
					{
						tickManager.DoSingleTick();
						SoundDefOf.Clock_Stop.PlayOneShotOnCamera(null);
					}
				}
			}
		}
	}
}
