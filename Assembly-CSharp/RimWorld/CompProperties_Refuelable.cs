﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x02000728 RID: 1832
	public class CompProperties_Refuelable : CompProperties
	{
		// Token: 0x04001615 RID: 5653
		public float fuelConsumptionRate = 1f;

		// Token: 0x04001616 RID: 5654
		public float fuelCapacity = 2f;

		// Token: 0x04001617 RID: 5655
		public float initialFuelPercent = 0f;

		// Token: 0x04001618 RID: 5656
		public float autoRefuelPercent = 0.3f;

		// Token: 0x04001619 RID: 5657
		public float fuelConsumptionPerTickInRain;

		// Token: 0x0400161A RID: 5658
		public ThingFilter fuelFilter;

		// Token: 0x0400161B RID: 5659
		public bool destroyOnNoFuel;

		// Token: 0x0400161C RID: 5660
		public bool consumeFuelOnlyWhenUsed;

		// Token: 0x0400161D RID: 5661
		public bool showFuelGizmo;

		// Token: 0x0400161E RID: 5662
		public bool targetFuelLevelConfigurable;

		// Token: 0x0400161F RID: 5663
		public float initialConfigurableTargetFuelLevel;

		// Token: 0x04001620 RID: 5664
		public bool drawOutOfFuelOverlay = true;

		// Token: 0x04001621 RID: 5665
		public float minimumFueledThreshold = 0f;

		// Token: 0x04001622 RID: 5666
		public bool drawFuelGaugeInMap = false;

		// Token: 0x04001623 RID: 5667
		public bool atomicFueling = false;

		// Token: 0x04001624 RID: 5668
		public float fuelMultiplier = 1f;

		// Token: 0x04001625 RID: 5669
		public string fuelLabel;

		// Token: 0x04001626 RID: 5670
		public string fuelGizmoLabel;

		// Token: 0x04001627 RID: 5671
		public string outOfFuelMessage;

		// Token: 0x04001628 RID: 5672
		public string fuelIconPath;

		// Token: 0x04001629 RID: 5673
		private Texture2D fuelIcon;

		// Token: 0x06002862 RID: 10338 RVA: 0x00158CEC File Offset: 0x001570EC
		public CompProperties_Refuelable()
		{
			this.compClass = typeof(CompRefuelable);
		}

		// Token: 0x17000634 RID: 1588
		// (get) Token: 0x06002863 RID: 10339 RVA: 0x00158D68 File Offset: 0x00157168
		public string FuelLabel
		{
			get
			{
				return this.fuelLabel.NullOrEmpty() ? "Fuel".Translate() : this.fuelLabel;
			}
		}

		// Token: 0x17000635 RID: 1589
		// (get) Token: 0x06002864 RID: 10340 RVA: 0x00158DA4 File Offset: 0x001571A4
		public string FuelGizmoLabel
		{
			get
			{
				return this.fuelGizmoLabel.NullOrEmpty() ? "Fuel".Translate() : this.fuelGizmoLabel;
			}
		}

		// Token: 0x17000636 RID: 1590
		// (get) Token: 0x06002865 RID: 10341 RVA: 0x00158DE0 File Offset: 0x001571E0
		public Texture2D FuelIcon
		{
			get
			{
				if (this.fuelIcon == null)
				{
					if (!this.fuelIconPath.NullOrEmpty())
					{
						this.fuelIcon = ContentFinder<Texture2D>.Get(this.fuelIconPath, true);
					}
					else
					{
						ThingDef thingDef;
						if (this.fuelFilter.AnyAllowedDef != null)
						{
							thingDef = this.fuelFilter.AnyAllowedDef;
						}
						else
						{
							thingDef = ThingDefOf.Chemfuel;
						}
						this.fuelIcon = thingDef.uiIcon;
					}
				}
				return this.fuelIcon;
			}
		}

		// Token: 0x06002866 RID: 10342 RVA: 0x00158E6A File Offset: 0x0015726A
		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			this.fuelFilter.ResolveReferences();
		}

		// Token: 0x06002867 RID: 10343 RVA: 0x00158E80 File Offset: 0x00157280
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string err in this.<ConfigErrors>__BaseCallProxy0(parentDef))
			{
				yield return err;
			}
			if (this.destroyOnNoFuel && this.initialFuelPercent <= 0f)
			{
				yield return "Refuelable component has destroyOnNoFuel, but initialFuelPercent <= 0";
			}
			if ((!this.consumeFuelOnlyWhenUsed || this.fuelConsumptionPerTickInRain > 0f) && parentDef.tickerType != TickerType.Normal)
			{
				yield return string.Format("Refuelable component set to consume fuel per tick, but parent tickertype is {0} instead of {1}", parentDef.tickerType, TickerType.Normal);
			}
			yield break;
		}
	}
}
