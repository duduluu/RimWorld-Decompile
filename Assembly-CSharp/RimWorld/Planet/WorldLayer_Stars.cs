﻿using System;
using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	// Token: 0x02000595 RID: 1429
	public class WorldLayer_Stars : WorldLayer
	{
		// Token: 0x04001018 RID: 4120
		private bool calculatedForStaticRotation = false;

		// Token: 0x04001019 RID: 4121
		private int calculatedForStartingTile = -1;

		// Token: 0x0400101A RID: 4122
		public const float DistanceToStars = 10f;

		// Token: 0x0400101B RID: 4123
		private static readonly FloatRange StarsDrawSize = new FloatRange(1f, 3.8f);

		// Token: 0x0400101C RID: 4124
		private const int StarsCount = 1500;

		// Token: 0x0400101D RID: 4125
		private const float DistToSunToReduceStarSize = 0.8f;

		// Token: 0x17000402 RID: 1026
		// (get) Token: 0x06001B47 RID: 6983 RVA: 0x000EADD0 File Offset: 0x000E91D0
		protected override int Layer
		{
			get
			{
				return WorldCameraManager.WorldSkyboxLayer;
			}
		}

		// Token: 0x17000403 RID: 1027
		// (get) Token: 0x06001B48 RID: 6984 RVA: 0x000EADEC File Offset: 0x000E91EC
		public override bool ShouldRegenerate
		{
			get
			{
				return base.ShouldRegenerate || (Find.GameInitData != null && Find.GameInitData.startingTile != this.calculatedForStartingTile) || this.UseStaticRotation != this.calculatedForStaticRotation;
			}
		}

		// Token: 0x17000404 RID: 1028
		// (get) Token: 0x06001B49 RID: 6985 RVA: 0x000EAE40 File Offset: 0x000E9240
		private bool UseStaticRotation
		{
			get
			{
				return Current.ProgramState == ProgramState.Entry;
			}
		}

		// Token: 0x17000405 RID: 1029
		// (get) Token: 0x06001B4A RID: 6986 RVA: 0x000EAE60 File Offset: 0x000E9260
		protected override Quaternion Rotation
		{
			get
			{
				Quaternion result;
				if (this.UseStaticRotation)
				{
					result = Quaternion.identity;
				}
				else
				{
					result = Quaternion.LookRotation(GenCelestial.CurSunPositionInWorldSpace());
				}
				return result;
			}
		}

		// Token: 0x06001B4B RID: 6987 RVA: 0x000EAE98 File Offset: 0x000E9298
		public override IEnumerable Regenerate()
		{
			IEnumerator enumerator = this.<Regenerate>__BaseCallProxy0().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object result = enumerator.Current;
					yield return result;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			Rand.PushState();
			Rand.Seed = Find.World.info.Seed;
			for (int i = 0; i < 1500; i++)
			{
				Vector3 unitVector = Rand.UnitVector3;
				Vector3 pos = unitVector * 10f;
				LayerSubMesh subMesh = base.GetSubMesh(WorldMaterials.Stars);
				float num = WorldLayer_Stars.StarsDrawSize.RandomInRange;
				Vector3 rhs = (!this.UseStaticRotation) ? Vector3.forward : GenCelestial.CurSunPositionInWorldSpace().normalized;
				float num2 = Vector3.Dot(unitVector, rhs);
				if (num2 > 0.8f)
				{
					num *= GenMath.LerpDouble(0.8f, 1f, 1f, 0.35f, num2);
				}
				WorldRendererUtility.PrintQuadTangentialToPlanet(pos, num, 0f, subMesh, true, true, true);
			}
			this.calculatedForStartingTile = ((Find.GameInitData == null) ? -1 : Find.GameInitData.startingTile);
			this.calculatedForStaticRotation = this.UseStaticRotation;
			Rand.PopState();
			base.FinalizeMesh(MeshParts.All);
			yield break;
		}
	}
}
