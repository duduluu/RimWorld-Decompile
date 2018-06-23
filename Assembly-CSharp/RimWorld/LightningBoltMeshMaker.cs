﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	// Token: 0x02000445 RID: 1093
	public static class LightningBoltMeshMaker
	{
		// Token: 0x04000B85 RID: 2949
		private static List<Vector2> verts2D;

		// Token: 0x04000B86 RID: 2950
		private static Vector2 lightningTop;

		// Token: 0x04000B87 RID: 2951
		private const float LightningHeight = 200f;

		// Token: 0x04000B88 RID: 2952
		private const float LightningRootXVar = 50f;

		// Token: 0x04000B89 RID: 2953
		private const float VertexInterval = 0.25f;

		// Token: 0x04000B8A RID: 2954
		private const float MeshWidth = 2f;

		// Token: 0x04000B8B RID: 2955
		private const float UVIntervalY = 0.04f;

		// Token: 0x04000B8C RID: 2956
		private const float PerturbAmp = 12f;

		// Token: 0x04000B8D RID: 2957
		private const float PerturbFreq = 0.007f;

		// Token: 0x060012F9 RID: 4857 RVA: 0x000A3C84 File Offset: 0x000A2084
		public static Mesh NewBoltMesh()
		{
			LightningBoltMeshMaker.lightningTop = new Vector2(Rand.Range(-50f, 50f), 200f);
			LightningBoltMeshMaker.MakeVerticesBase();
			LightningBoltMeshMaker.PeturbVerticesRandomly();
			LightningBoltMeshMaker.DoubleVertices();
			return LightningBoltMeshMaker.MeshFromVerts();
		}

		// Token: 0x060012FA RID: 4858 RVA: 0x000A3CCC File Offset: 0x000A20CC
		private static void MakeVerticesBase()
		{
			int num = (int)Math.Ceiling((double)((Vector2.zero - LightningBoltMeshMaker.lightningTop).magnitude / 0.25f));
			Vector2 b = LightningBoltMeshMaker.lightningTop / (float)num;
			LightningBoltMeshMaker.verts2D = new List<Vector2>();
			Vector2 vector = Vector2.zero;
			for (int i = 0; i < num; i++)
			{
				LightningBoltMeshMaker.verts2D.Add(vector);
				vector += b;
			}
		}

		// Token: 0x060012FB RID: 4859 RVA: 0x000A3D48 File Offset: 0x000A2148
		private static void PeturbVerticesRandomly()
		{
			Perlin perlin = new Perlin(0.0070000002160668373, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
			List<Vector2> list = LightningBoltMeshMaker.verts2D.ListFullCopy<Vector2>();
			LightningBoltMeshMaker.verts2D.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				float d = 12f * (float)perlin.GetValue((double)i, 0.0, 0.0);
				Vector2 item = list[i] + d * Vector2.right;
				LightningBoltMeshMaker.verts2D.Add(item);
			}
		}

		// Token: 0x060012FC RID: 4860 RVA: 0x000A3DFC File Offset: 0x000A21FC
		private static void DoubleVertices()
		{
			List<Vector2> list = LightningBoltMeshMaker.verts2D.ListFullCopy<Vector2>();
			Vector3 vector = default(Vector3);
			Vector2 a = default(Vector2);
			LightningBoltMeshMaker.verts2D.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				if (i <= list.Count - 2)
				{
					vector = Quaternion.AngleAxis(90f, Vector3.up) * (list[i] - list[i + 1]);
					a = new Vector2(vector.y, vector.z);
					a.Normalize();
				}
				Vector2 item = list[i] - 1f * a;
				Vector2 item2 = list[i] + 1f * a;
				LightningBoltMeshMaker.verts2D.Add(item);
				LightningBoltMeshMaker.verts2D.Add(item2);
			}
		}

		// Token: 0x060012FD RID: 4861 RVA: 0x000A3EF0 File Offset: 0x000A22F0
		private static Mesh MeshFromVerts()
		{
			Vector3[] array = new Vector3[LightningBoltMeshMaker.verts2D.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Vector3(LightningBoltMeshMaker.verts2D[i].x, 0f, LightningBoltMeshMaker.verts2D[i].y);
			}
			float num = 0f;
			Vector2[] array2 = new Vector2[LightningBoltMeshMaker.verts2D.Count];
			for (int j = 0; j < LightningBoltMeshMaker.verts2D.Count; j += 2)
			{
				array2[j] = new Vector2(0f, num);
				array2[j + 1] = new Vector2(1f, num);
				num += 0.04f;
			}
			int[] array3 = new int[LightningBoltMeshMaker.verts2D.Count * 3];
			for (int k = 0; k < LightningBoltMeshMaker.verts2D.Count - 2; k += 2)
			{
				int num2 = k * 3;
				array3[num2] = k;
				array3[num2 + 1] = k + 1;
				array3[num2 + 2] = k + 2;
				array3[num2 + 3] = k + 2;
				array3[num2 + 4] = k + 1;
				array3[num2 + 5] = k + 3;
			}
			return new Mesh
			{
				vertices = array,
				uv = array2,
				triangles = array3,
				name = "MeshFromVerts()"
			};
		}
	}
}
