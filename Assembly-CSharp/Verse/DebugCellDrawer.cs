﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000C03 RID: 3075
	public sealed class DebugCellDrawer
	{
		// Token: 0x04002E00 RID: 11776
		private List<DebugCell> debugCells = new List<DebugCell>();

		// Token: 0x04002E01 RID: 11777
		private List<DebugLine> debugLines = new List<DebugLine>();

		// Token: 0x04002E02 RID: 11778
		private const int DefaultLifespanTicks = 50;

		// Token: 0x06004349 RID: 17225 RVA: 0x002393B0 File Offset: 0x002377B0
		public void FlashCell(IntVec3 c, float colorPct = 0f, string text = null, int duration = 50)
		{
			DebugCell debugCell = new DebugCell();
			debugCell.c = c;
			debugCell.displayString = text;
			debugCell.colorPct = colorPct;
			debugCell.ticksLeft = duration;
			this.debugCells.Add(debugCell);
		}

		// Token: 0x0600434A RID: 17226 RVA: 0x002393F0 File Offset: 0x002377F0
		public void FlashCell(IntVec3 c, Material mat, string text = null, int duration = 50)
		{
			DebugCell debugCell = new DebugCell();
			debugCell.c = c;
			debugCell.displayString = text;
			debugCell.customMat = mat;
			debugCell.ticksLeft = duration;
			this.debugCells.Add(debugCell);
		}

		// Token: 0x0600434B RID: 17227 RVA: 0x0023942D File Offset: 0x0023782D
		public void FlashLine(IntVec3 a, IntVec3 b, int duration = 50, SimpleColor color = SimpleColor.White)
		{
			this.debugLines.Add(new DebugLine(a.ToVector3Shifted(), b.ToVector3Shifted(), duration, color));
		}

		// Token: 0x0600434C RID: 17228 RVA: 0x00239454 File Offset: 0x00237854
		public void DebugDrawerUpdate()
		{
			for (int i = 0; i < this.debugCells.Count; i++)
			{
				this.debugCells[i].Draw();
			}
			for (int j = 0; j < this.debugLines.Count; j++)
			{
				this.debugLines[j].Draw();
			}
		}

		// Token: 0x0600434D RID: 17229 RVA: 0x002394C4 File Offset: 0x002378C4
		public void DebugDrawerTick()
		{
			for (int i = this.debugCells.Count - 1; i >= 0; i--)
			{
				DebugCell debugCell = this.debugCells[i];
				debugCell.ticksLeft--;
				if (debugCell.ticksLeft <= 0)
				{
					this.debugCells.RemoveAt(i);
				}
			}
			this.debugLines.RemoveAll((DebugLine dl) => dl.Done);
		}

		// Token: 0x0600434E RID: 17230 RVA: 0x00239550 File Offset: 0x00237950
		public void DebugDrawerOnGUI()
		{
			if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
			{
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.MiddleCenter;
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				for (int i = 0; i < this.debugCells.Count; i++)
				{
					this.debugCells[i].OnGUI();
				}
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}
	}
}
