﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Verse;

namespace RimWorld
{
	public static class GenStuff
	{
		[CompilerGenerated]
		private static Func<ThingDef, float> <>f__am$cache0;

		[CompilerGenerated]
		private static Func<ThingDef, float> <>f__am$cache1;

		public static ThingDef DefaultStuffFor(BuildableDef bd)
		{
			if (!bd.MadeFromStuff)
			{
				return null;
			}
			ThingDef thingDef = bd as ThingDef;
			if (thingDef != null)
			{
				if (thingDef.IsMeleeWeapon)
				{
					if (ThingDefOf.Steel.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Steel;
					}
					if (ThingDefOf.Plasteel.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Plasteel;
					}
				}
				if (thingDef.IsApparel)
				{
					if (ThingDefOf.Cloth.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Cloth;
					}
					if (ThingDefOf.Leather_Plain.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Leather_Plain;
					}
					if (ThingDefOf.Steel.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Steel;
					}
				}
			}
			if (ThingDefOf.WoodLog.stuffProps.CanMake(bd))
			{
				return ThingDefOf.WoodLog;
			}
			if (ThingDefOf.Steel.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Steel;
			}
			if (ThingDefOf.Plasteel.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Plasteel;
			}
			if (ThingDefOf.BlocksGranite.stuffProps.CanMake(bd))
			{
				return ThingDefOf.BlocksGranite;
			}
			if (ThingDefOf.Cloth.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Cloth;
			}
			if (ThingDefOf.Leather_Plain.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Leather_Plain;
			}
			return GenStuff.AllowedStuffsFor(bd, TechLevel.Undefined).First<ThingDef>();
		}

		public static ThingDef RandomStuffFor(ThingDef td)
		{
			if (!td.MadeFromStuff)
			{
				return null;
			}
			return GenStuff.AllowedStuffsFor(td, TechLevel.Undefined).RandomElement<ThingDef>();
		}

		public static ThingDef RandomStuffByCommonalityFor(ThingDef td, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				return null;
			}
			ThingDef result;
			if (!GenStuff.TryRandomStuffByCommonalityFor(td, out result, maxTechLevel))
			{
				result = GenStuff.DefaultStuffFor(td);
			}
			return result;
		}

		public static IEnumerable<ThingDef> AllowedStuffsFor(BuildableDef td, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				yield break;
			}
			List<ThingDef> allDefs = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefs.Count; i++)
			{
				ThingDef d = allDefs[i];
				if (d.IsStuff && (maxTechLevel == TechLevel.Undefined || d.techLevel <= maxTechLevel) && d.stuffProps.CanMake(td))
				{
					yield return d;
				}
			}
			yield break;
		}

		public static bool TryRandomStuffByCommonalityFor(ThingDef td, out ThingDef stuff, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				stuff = null;
				return true;
			}
			IEnumerable<ThingDef> source = GenStuff.AllowedStuffsFor(td, maxTechLevel);
			return source.TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out stuff);
		}

		public static bool TryRandomStuffFor(ThingDef td, out ThingDef stuff, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				stuff = null;
				return true;
			}
			IEnumerable<ThingDef> source = GenStuff.AllowedStuffsFor(td, maxTechLevel);
			return source.TryRandomElement(out stuff);
		}

		public static ThingDef RandomStuffInexpensiveFor(ThingDef thingDef, Faction faction)
		{
			return GenStuff.RandomStuffInexpensiveFor(thingDef, (faction == null) ? TechLevel.Undefined : faction.def.techLevel);
		}

		public static ThingDef RandomStuffInexpensiveFor(ThingDef thingDef, TechLevel maxTechLevel)
		{
			if (!thingDef.MadeFromStuff)
			{
				return null;
			}
			IEnumerable<ThingDef> enumerable = GenStuff.AllowedStuffsFor(thingDef, maxTechLevel);
			float cheapestPrice = -1f;
			foreach (ThingDef thingDef2 in enumerable)
			{
				float num = thingDef2.BaseMarketValue / thingDef2.VolumePerUnit;
				if (cheapestPrice == -1f || num < cheapestPrice)
				{
					cheapestPrice = num;
				}
			}
			enumerable = from x in enumerable
			where x.BaseMarketValue / x.VolumePerUnit <= cheapestPrice * 4f
			select x;
			ThingDef result;
			if (enumerable.TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out result))
			{
				return result;
			}
			return null;
		}

		[CompilerGenerated]
		private static float <TryRandomStuffByCommonalityFor>m__0(ThingDef x)
		{
			return x.stuffProps.commonality;
		}

		[CompilerGenerated]
		private static float <RandomStuffInexpensiveFor>m__1(ThingDef x)
		{
			return x.stuffProps.commonality;
		}

		[CompilerGenerated]
		private sealed class <AllowedStuffsFor>c__Iterator0 : IEnumerable, IEnumerable<ThingDef>, IEnumerator, IDisposable, IEnumerator<ThingDef>
		{
			internal BuildableDef td;

			internal List<ThingDef> <allDefs>__0;

			internal int <i>__1;

			internal ThingDef <d>__2;

			internal TechLevel maxTechLevel;

			internal ThingDef $current;

			internal bool $disposing;

			internal int $PC;

			[DebuggerHidden]
			public <AllowedStuffsFor>c__Iterator0()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)this.$PC;
				this.$PC = -1;
				switch (num)
				{
				case 0u:
					if (!td.MadeFromStuff)
					{
						return false;
					}
					allDefs = DefDatabase<ThingDef>.AllDefsListForReading;
					i = 0;
					break;
				case 1u:
					IL_D0:
					i++;
					break;
				default:
					return false;
				}
				if (i >= allDefs.Count)
				{
					this.$PC = -1;
				}
				else
				{
					d = allDefs[i];
					if (d.IsStuff && (maxTechLevel == TechLevel.Undefined || d.techLevel <= maxTechLevel) && d.stuffProps.CanMake(td))
					{
						this.$current = d;
						if (!this.$disposing)
						{
							this.$PC = 1;
						}
						return true;
					}
					goto IL_D0;
				}
				return false;
			}

			ThingDef IEnumerator<ThingDef>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.$current;
				}
			}

			[DebuggerHidden]
			public void Dispose()
			{
				this.$disposing = true;
				this.$PC = -1;
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.System.Collections.Generic.IEnumerable<Verse.ThingDef>.GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<ThingDef> IEnumerable<ThingDef>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
				{
					return this;
				}
				GenStuff.<AllowedStuffsFor>c__Iterator0 <AllowedStuffsFor>c__Iterator = new GenStuff.<AllowedStuffsFor>c__Iterator0();
				<AllowedStuffsFor>c__Iterator.td = td;
				<AllowedStuffsFor>c__Iterator.maxTechLevel = maxTechLevel;
				return <AllowedStuffsFor>c__Iterator;
			}
		}

		[CompilerGenerated]
		private sealed class <RandomStuffInexpensiveFor>c__AnonStorey1
		{
			internal float cheapestPrice;

			public <RandomStuffInexpensiveFor>c__AnonStorey1()
			{
			}

			internal bool <>m__0(ThingDef x)
			{
				return x.BaseMarketValue / x.VolumePerUnit <= this.cheapestPrice * 4f;
			}
		}
	}
}
