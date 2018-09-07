﻿using System;
using Verse;

namespace RimWorld
{
	public class ThingSetMakerDef : Def
	{
		public ThingSetMaker root;

		public ThingSetMakerParams debugParams;

		public ThingSetMakerDef()
		{
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			this.root.ResolveReferences();
		}
	}
}
