﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000BFF RID: 3071
	public class CellBoolDrawer
	{
		// Token: 0x04002DEC RID: 11756
		public ICellBoolGiver giver;

		// Token: 0x04002DED RID: 11757
		private bool wantDraw = false;

		// Token: 0x04002DEE RID: 11758
		private Material material;

		// Token: 0x04002DEF RID: 11759
		private bool materialCaresAboutVertexColors;

		// Token: 0x04002DF0 RID: 11760
		private bool dirty = true;

		// Token: 0x04002DF1 RID: 11761
		private List<Mesh> meshes = new List<Mesh>();

		// Token: 0x04002DF2 RID: 11762
		private int mapSizeX;

		// Token: 0x04002DF3 RID: 11763
		private int mapSizeZ;

		// Token: 0x04002DF4 RID: 11764
		private float opacity = 0.33f;

		// Token: 0x04002DF5 RID: 11765
		private static List<Vector3> verts = new List<Vector3>();

		// Token: 0x04002DF6 RID: 11766
		private static List<int> tris = new List<int>();

		// Token: 0x04002DF7 RID: 11767
		private static List<Color> colors = new List<Color>();

		// Token: 0x04002DF8 RID: 11768
		private const float DefaultOpacity = 0.33f;

		// Token: 0x04002DF9 RID: 11769
		private const int MaxCellsPerMesh = 16383;

		// Token: 0x06004335 RID: 17205 RVA: 0x002386FC File Offset: 0x00236AFC
		public CellBoolDrawer(ICellBoolGiver giver, int mapSizeX, int mapSizeZ, float opacity = 0.33f)
		{
			this.giver = giver;
			this.mapSizeX = mapSizeX;
			this.mapSizeZ = mapSizeZ;
			this.opacity = opacity;
		}

		// Token: 0x06004336 RID: 17206 RVA: 0x00238751 File Offset: 0x00236B51
		public void MarkForDraw()
		{
			this.wantDraw = true;
		}

		// Token: 0x06004337 RID: 17207 RVA: 0x0023875B File Offset: 0x00236B5B
		public void CellBoolDrawerUpdate()
		{
			if (this.wantDraw)
			{
				this.ActuallyDraw();
				this.wantDraw = false;
			}
		}

		// Token: 0x06004338 RID: 17208 RVA: 0x00238778 File Offset: 0x00236B78
		private void ActuallyDraw()
		{
			if (this.dirty)
			{
				this.RegenerateMesh();
			}
			for (int i = 0; i < this.meshes.Count; i++)
			{
				Graphics.DrawMesh(this.meshes[i], Vector3.zero, Quaternion.identity, this.material, 0);
			}
		}

		// Token: 0x06004339 RID: 17209 RVA: 0x002387D7 File Offset: 0x00236BD7
		public void SetDirty()
		{
			this.dirty = true;
		}

		// Token: 0x0600433A RID: 17210 RVA: 0x002387E4 File Offset: 0x00236BE4
		public void RegenerateMesh()
		{
			for (int i = 0; i < this.meshes.Count; i++)
			{
				this.meshes[i].Clear();
			}
			int num = 0;
			int num2 = 0;
			if (this.meshes.Count < num + 1)
			{
				Mesh mesh = new Mesh();
				mesh.name = "CellBoolDrawer";
				this.meshes.Add(mesh);
			}
			Mesh mesh2 = this.meshes[num];
			CellRect cellRect = new CellRect(0, 0, this.mapSizeX, this.mapSizeZ);
			float y = AltitudeLayer.MapDataOverlay.AltitudeFor();
			bool careAboutVertexColors = false;
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
				{
					int index = CellIndicesUtility.CellToIndex(j, k, this.mapSizeX);
					if (this.giver.GetCellBool(index))
					{
						CellBoolDrawer.verts.Add(new Vector3((float)j, y, (float)k));
						CellBoolDrawer.verts.Add(new Vector3((float)j, y, (float)(k + 1)));
						CellBoolDrawer.verts.Add(new Vector3((float)(j + 1), y, (float)(k + 1)));
						CellBoolDrawer.verts.Add(new Vector3((float)(j + 1), y, (float)k));
						Color cellExtraColor = this.giver.GetCellExtraColor(index);
						CellBoolDrawer.colors.Add(cellExtraColor);
						CellBoolDrawer.colors.Add(cellExtraColor);
						CellBoolDrawer.colors.Add(cellExtraColor);
						CellBoolDrawer.colors.Add(cellExtraColor);
						if (cellExtraColor != Color.white)
						{
							careAboutVertexColors = true;
						}
						int count = CellBoolDrawer.verts.Count;
						CellBoolDrawer.tris.Add(count - 4);
						CellBoolDrawer.tris.Add(count - 3);
						CellBoolDrawer.tris.Add(count - 2);
						CellBoolDrawer.tris.Add(count - 4);
						CellBoolDrawer.tris.Add(count - 2);
						CellBoolDrawer.tris.Add(count - 1);
						num2++;
						if (num2 >= 16383)
						{
							this.FinalizeWorkingDataIntoMesh(mesh2);
							num++;
							if (this.meshes.Count < num + 1)
							{
								Mesh mesh3 = new Mesh();
								mesh3.name = "CellBoolDrawer";
								this.meshes.Add(mesh3);
							}
							mesh2 = this.meshes[num];
							num2 = 0;
						}
					}
				}
			}
			this.FinalizeWorkingDataIntoMesh(mesh2);
			this.CreateMaterialIfNeeded(careAboutVertexColors);
			this.dirty = false;
		}

		// Token: 0x0600433B RID: 17211 RVA: 0x00238A84 File Offset: 0x00236E84
		private void FinalizeWorkingDataIntoMesh(Mesh mesh)
		{
			if (CellBoolDrawer.verts.Count > 0)
			{
				mesh.SetVertices(CellBoolDrawer.verts);
				CellBoolDrawer.verts.Clear();
				mesh.SetTriangles(CellBoolDrawer.tris, 0);
				CellBoolDrawer.tris.Clear();
				mesh.SetColors(CellBoolDrawer.colors);
				CellBoolDrawer.colors.Clear();
			}
		}

		// Token: 0x0600433C RID: 17212 RVA: 0x00238AE4 File Offset: 0x00236EE4
		private void CreateMaterialIfNeeded(bool careAboutVertexColors)
		{
			if (this.material == null || this.materialCaresAboutVertexColors != careAboutVertexColors)
			{
				this.material = SolidColorMaterials.SimpleSolidColorMaterial(new Color(this.giver.Color.r, this.giver.Color.g, this.giver.Color.b, this.opacity * this.giver.Color.a), careAboutVertexColors);
				this.materialCaresAboutVertexColors = careAboutVertexColors;
				this.material.renderQueue = 3600;
			}
		}
	}
}
