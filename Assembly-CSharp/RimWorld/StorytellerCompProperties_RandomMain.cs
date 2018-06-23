﻿using System;
using System.Collections.Generic;

namespace RimWorld
{
	// Token: 0x02000371 RID: 881
	public class StorytellerCompProperties_RandomMain : StorytellerCompProperties
	{
		// Token: 0x04000956 RID: 2390
		public float mtbDays;

		// Token: 0x04000957 RID: 2391
		public List<IncidentCategoryEntry> categoryWeights = new List<IncidentCategoryEntry>();

		// Token: 0x04000958 RID: 2392
		public float maxThreatBigIntervalDays = 99999f;

		// Token: 0x06000F41 RID: 3905 RVA: 0x00081461 File Offset: 0x0007F861
		public StorytellerCompProperties_RandomMain()
		{
			this.compClass = typeof(StorytellerComp_RandomMain);
		}
	}
}
