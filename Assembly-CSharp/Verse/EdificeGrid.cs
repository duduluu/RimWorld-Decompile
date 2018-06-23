﻿using System;

namespace Verse
{
	// Token: 0x02000C1E RID: 3102
	public sealed class EdificeGrid
	{
		// Token: 0x04002E5D RID: 11869
		private Map map;

		// Token: 0x04002E5E RID: 11870
		private Building[] innerArray;

		// Token: 0x060043F2 RID: 17394 RVA: 0x0023D6B7 File Offset: 0x0023BAB7
		public EdificeGrid(Map map)
		{
			this.map = map;
			this.innerArray = new Building[map.cellIndices.NumGridCells];
		}

		// Token: 0x17000AA8 RID: 2728
		// (get) Token: 0x060043F3 RID: 17395 RVA: 0x0023D6E0 File Offset: 0x0023BAE0
		public Building[] InnerArray
		{
			get
			{
				return this.innerArray;
			}
		}

		// Token: 0x17000AA9 RID: 2729
		public Building this[int index]
		{
			get
			{
				return this.innerArray[index];
			}
		}

		// Token: 0x17000AAA RID: 2730
		public Building this[IntVec3 c]
		{
			get
			{
				return this.innerArray[this.map.cellIndices.CellToIndex(c)];
			}
		}

		// Token: 0x060043F6 RID: 17398 RVA: 0x0023D74C File Offset: 0x0023BB4C
		public void Register(Building ed)
		{
			CellIndices cellIndices = this.map.cellIndices;
			CellRect cellRect = ed.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					if (UnityData.isDebugBuild && this[intVec] != null && !this[intVec].Destroyed)
					{
						Log.Error(string.Concat(new object[]
						{
							"Added edifice ",
							ed.LabelCap,
							" over edifice ",
							this[intVec].LabelCap,
							" at ",
							intVec,
							". Destroying old edifice."
						}), false);
						this[intVec].Destroy(DestroyMode.Vanish);
						return;
					}
					this.innerArray[cellIndices.CellToIndex(intVec)] = ed;
				}
			}
		}

		// Token: 0x060043F7 RID: 17399 RVA: 0x0023D858 File Offset: 0x0023BC58
		public void DeRegister(Building ed)
		{
			CellIndices cellIndices = this.map.cellIndices;
			CellRect cellRect = ed.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					this.innerArray[cellIndices.CellToIndex(j, i)] = null;
				}
			}
		}
	}
}
