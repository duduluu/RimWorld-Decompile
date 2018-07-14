﻿using System;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_Adaptation : HistoryAutoRecorderWorker
	{
		public HistoryAutoRecorderWorker_Adaptation()
		{
		}

		public override float PullRecord()
		{
			return Find.StoryWatcher.watcherAdaptation.AdaptDays;
		}
	}
}
