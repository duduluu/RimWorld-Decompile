﻿using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000057 RID: 87
	public class JobDriver_PlayHorseshoes : JobDriver_WatchBuilding
	{
		// Token: 0x040001F5 RID: 501
		private const int HorseshoeThrowInterval = 400;

		// Token: 0x06000299 RID: 665 RVA: 0x0001C4FC File Offset: 0x0001A8FC
		protected override void WatchTickAction()
		{
			if (this.pawn.IsHashIntervalTick(400))
			{
				MoteMaker.ThrowHorseshoe(this.pawn, base.TargetA.Cell);
			}
			base.WatchTickAction();
		}
	}
}
