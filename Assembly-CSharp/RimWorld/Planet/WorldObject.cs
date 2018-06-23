﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	// Token: 0x02000617 RID: 1559
	[StaticConstructorOnStartup]
	public class WorldObject : IExposable, ILoadReferenceable, ISelectable
	{
		// Token: 0x04001255 RID: 4693
		public WorldObjectDef def;

		// Token: 0x04001256 RID: 4694
		public int ID = -1;

		// Token: 0x04001257 RID: 4695
		private int tileInt = -1;

		// Token: 0x04001258 RID: 4696
		private Faction factionInt;

		// Token: 0x04001259 RID: 4697
		public int creationGameTicks = -1;

		// Token: 0x0400125A RID: 4698
		private List<WorldObjectComp> comps = new List<WorldObjectComp>();

		// Token: 0x0400125B RID: 4699
		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

		// Token: 0x0400125C RID: 4700
		private const float BaseDrawSize = 0.7f;

		// Token: 0x170004A6 RID: 1190
		// (get) Token: 0x06001F81 RID: 8065 RVA: 0x000F8728 File Offset: 0x000F6B28
		public List<WorldObjectComp> AllComps
		{
			get
			{
				return this.comps;
			}
		}

		// Token: 0x170004A7 RID: 1191
		// (get) Token: 0x06001F82 RID: 8066 RVA: 0x000F8744 File Offset: 0x000F6B44
		// (set) Token: 0x06001F83 RID: 8067 RVA: 0x000F8760 File Offset: 0x000F6B60
		public int Tile
		{
			get
			{
				return this.tileInt;
			}
			set
			{
				if (this.tileInt != value)
				{
					this.tileInt = value;
					if (this.Spawned)
					{
						if (!this.def.useDynamicDrawer)
						{
							Find.World.renderer.Notify_StaticWorldObjectPosChanged();
						}
					}
					this.PositionChanged();
				}
			}
		}

		// Token: 0x170004A8 RID: 1192
		// (get) Token: 0x06001F84 RID: 8068 RVA: 0x000F87B8 File Offset: 0x000F6BB8
		public bool Spawned
		{
			get
			{
				return Find.WorldObjects.Contains(this);
			}
		}

		// Token: 0x170004A9 RID: 1193
		// (get) Token: 0x06001F85 RID: 8069 RVA: 0x000F87D8 File Offset: 0x000F6BD8
		public virtual Vector3 DrawPos
		{
			get
			{
				return Find.WorldGrid.GetTileCenter(this.Tile);
			}
		}

		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x06001F86 RID: 8070 RVA: 0x000F8800 File Offset: 0x000F6C00
		public Faction Faction
		{
			get
			{
				return this.factionInt;
			}
		}

		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x06001F87 RID: 8071 RVA: 0x000F881C File Offset: 0x000F6C1C
		public virtual string Label
		{
			get
			{
				return this.def.label;
			}
		}

		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x06001F88 RID: 8072 RVA: 0x000F883C File Offset: 0x000F6C3C
		public string LabelCap
		{
			get
			{
				return this.Label.CapitalizeFirst();
			}
		}

		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x06001F89 RID: 8073 RVA: 0x000F885C File Offset: 0x000F6C5C
		public virtual string LabelShort
		{
			get
			{
				return this.Label;
			}
		}

		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x06001F8A RID: 8074 RVA: 0x000F8878 File Offset: 0x000F6C78
		public virtual string LabelShortCap
		{
			get
			{
				return this.LabelShort.CapitalizeFirst();
			}
		}

		// Token: 0x170004AF RID: 1199
		// (get) Token: 0x06001F8B RID: 8075 RVA: 0x000F8898 File Offset: 0x000F6C98
		public virtual Material Material
		{
			get
			{
				return this.def.Material;
			}
		}

		// Token: 0x170004B0 RID: 1200
		// (get) Token: 0x06001F8C RID: 8076 RVA: 0x000F88B8 File Offset: 0x000F6CB8
		public virtual bool SelectableNow
		{
			get
			{
				return this.def.selectable;
			}
		}

		// Token: 0x170004B1 RID: 1201
		// (get) Token: 0x06001F8D RID: 8077 RVA: 0x000F88D8 File Offset: 0x000F6CD8
		public virtual bool NeverMultiSelect
		{
			get
			{
				return this.def.neverMultiSelect;
			}
		}

		// Token: 0x170004B2 RID: 1202
		// (get) Token: 0x06001F8E RID: 8078 RVA: 0x000F88F8 File Offset: 0x000F6CF8
		public virtual Texture2D ExpandingIcon
		{
			get
			{
				return this.def.ExpandingIconTexture ?? ((Texture2D)this.Material.mainTexture);
			}
		}

		// Token: 0x170004B3 RID: 1203
		// (get) Token: 0x06001F8F RID: 8079 RVA: 0x000F8930 File Offset: 0x000F6D30
		public virtual Color ExpandingIconColor
		{
			get
			{
				return this.Material.color;
			}
		}

		// Token: 0x170004B4 RID: 1204
		// (get) Token: 0x06001F90 RID: 8080 RVA: 0x000F8950 File Offset: 0x000F6D50
		public virtual float ExpandingIconPriority
		{
			get
			{
				return this.def.expandingIconPriority;
			}
		}

		// Token: 0x170004B5 RID: 1205
		// (get) Token: 0x06001F91 RID: 8081 RVA: 0x000F8970 File Offset: 0x000F6D70
		public virtual bool ExpandMore
		{
			get
			{
				return this.def.expandMore;
			}
		}

		// Token: 0x170004B6 RID: 1206
		// (get) Token: 0x06001F92 RID: 8082 RVA: 0x000F8990 File Offset: 0x000F6D90
		public virtual bool AppendFactionToInspectString
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004B7 RID: 1207
		// (get) Token: 0x06001F93 RID: 8083 RVA: 0x000F89A8 File Offset: 0x000F6DA8
		public IThingHolder ParentHolder
		{
			get
			{
				return (!this.Spawned) ? null : Find.World;
			}
		}

		// Token: 0x170004B8 RID: 1208
		// (get) Token: 0x06001F94 RID: 8084 RVA: 0x000F89D4 File Offset: 0x000F6DD4
		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats
		{
			get
			{
				yield break;
			}
		}

		// Token: 0x170004B9 RID: 1209
		// (get) Token: 0x06001F95 RID: 8085 RVA: 0x000F89F8 File Offset: 0x000F6DF8
		public BiomeDef Biome
		{
			get
			{
				return (!this.Spawned) ? null : Find.WorldGrid[this.Tile].biome;
			}
		}

		// Token: 0x06001F96 RID: 8086 RVA: 0x000F8A34 File Offset: 0x000F6E34
		public virtual IEnumerable<IncidentTargetTypeDef> AcceptedTypes()
		{
			if (this.def.incidentTargetTypes != null)
			{
				foreach (IncidentTargetTypeDef type in this.def.incidentTargetTypes)
				{
					yield return type;
				}
			}
			for (int i = 0; i < this.comps.Count; i++)
			{
				foreach (IncidentTargetTypeDef type2 in this.comps[i].AcceptedTypes())
				{
					yield return type2;
				}
			}
			yield break;
		}

		// Token: 0x06001F97 RID: 8087 RVA: 0x000F8A60 File Offset: 0x000F6E60
		public virtual void ExposeData()
		{
			Scribe_Defs.Look<WorldObjectDef>(ref this.def, "def");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.InitializeComps();
			}
			Scribe_Values.Look<int>(ref this.ID, "ID", -1, false);
			Scribe_Values.Look<int>(ref this.tileInt, "tile", -1, false);
			Scribe_References.Look<Faction>(ref this.factionInt, "faction", false);
			Scribe_Values.Look<int>(ref this.creationGameTicks, "creationGameTicks", 0, false);
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostExposeData();
			}
		}

		// Token: 0x06001F98 RID: 8088 RVA: 0x000F8B08 File Offset: 0x000F6F08
		private void InitializeComps()
		{
			for (int i = 0; i < this.def.comps.Count; i++)
			{
				WorldObjectComp worldObjectComp = (WorldObjectComp)Activator.CreateInstance(this.def.comps[i].compClass);
				worldObjectComp.parent = this;
				this.comps.Add(worldObjectComp);
				worldObjectComp.Initialize(this.def.comps[i]);
			}
		}

		// Token: 0x06001F99 RID: 8089 RVA: 0x000F8B84 File Offset: 0x000F6F84
		public virtual void SetFaction(Faction newFaction)
		{
			if (!this.def.canHaveFaction && newFaction != null)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to set faction to ",
					newFaction,
					" but this world object (",
					this,
					") cannot have faction."
				}), false);
			}
			else
			{
				this.factionInt = newFaction;
			}
		}

		// Token: 0x06001F9A RID: 8090 RVA: 0x000F8BE8 File Offset: 0x000F6FE8
		public virtual string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.Faction != null && this.AppendFactionToInspectString)
			{
				stringBuilder.Append("Faction".Translate() + ": " + this.Faction.Name);
			}
			for (int i = 0; i < this.comps.Count; i++)
			{
				string text = this.comps[i].CompInspectStringExtra();
				if (!text.NullOrEmpty())
				{
					if (Prefs.DevMode && char.IsWhiteSpace(text[text.Length - 1]))
					{
						Log.ErrorOnce(this.comps[i].GetType() + " CompInspectStringExtra ended with whitespace: " + text, 25612, false);
						text = text.TrimEndNewlines();
					}
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(text);
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06001F9B RID: 8091 RVA: 0x000F8CF0 File Offset: 0x000F70F0
		public virtual void Tick()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].CompTick();
			}
		}

		// Token: 0x06001F9C RID: 8092 RVA: 0x000F8D2D File Offset: 0x000F712D
		public virtual void ExtraSelectionOverlaysOnGUI()
		{
		}

		// Token: 0x06001F9D RID: 8093 RVA: 0x000F8D30 File Offset: 0x000F7130
		public virtual void DrawExtraSelectionOverlays()
		{
		}

		// Token: 0x06001F9E RID: 8094 RVA: 0x000F8D33 File Offset: 0x000F7133
		public virtual void PostMake()
		{
			this.InitializeComps();
		}

		// Token: 0x06001F9F RID: 8095 RVA: 0x000F8D3C File Offset: 0x000F713C
		public virtual void PostAdd()
		{
		}

		// Token: 0x06001FA0 RID: 8096 RVA: 0x000F8D3F File Offset: 0x000F713F
		protected virtual void PositionChanged()
		{
		}

		// Token: 0x06001FA1 RID: 8097 RVA: 0x000F8D42 File Offset: 0x000F7142
		public virtual void SpawnSetup()
		{
			if (!this.def.useDynamicDrawer)
			{
				Find.World.renderer.Notify_StaticWorldObjectPosChanged();
			}
			if (this.def.useDynamicDrawer)
			{
				Find.WorldDynamicDrawManager.RegisterDrawable(this);
			}
		}

		// Token: 0x06001FA2 RID: 8098 RVA: 0x000F8D80 File Offset: 0x000F7180
		public virtual void PostRemove()
		{
			if (!this.def.useDynamicDrawer)
			{
				Find.World.renderer.Notify_StaticWorldObjectPosChanged();
			}
			if (this.def.useDynamicDrawer)
			{
				Find.WorldDynamicDrawManager.DeRegisterDrawable(this);
			}
			Find.WorldSelector.Deselect(this);
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostPostRemove();
			}
		}

		// Token: 0x06001FA3 RID: 8099 RVA: 0x000F8E04 File Offset: 0x000F7204
		public virtual void Print(LayerSubMesh subMesh)
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			WorldRendererUtility.PrintQuadTangentialToPlanet(this.DrawPos, 0.7f * averageTileSize, 0.015f, subMesh, false, true, true);
		}

		// Token: 0x06001FA4 RID: 8100 RVA: 0x000F8E38 File Offset: 0x000F7238
		public virtual void Draw()
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			float transitionPct = ExpandableWorldObjectsUtility.TransitionPct;
			if (this.def.expandingIcon && transitionPct > 0f)
			{
				Color color = this.Material.color;
				float num = 1f - transitionPct;
				WorldObject.propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num));
				Vector3 drawPos = this.DrawPos;
				float size = 0.7f * averageTileSize;
				float altOffset = 0.015f;
				Material material = this.Material;
				MaterialPropertyBlock materialPropertyBlock = WorldObject.propertyBlock;
				WorldRendererUtility.DrawQuadTangentialToPlanet(drawPos, size, altOffset, material, false, false, materialPropertyBlock);
			}
			else
			{
				WorldRendererUtility.DrawQuadTangentialToPlanet(this.DrawPos, 0.7f * averageTileSize, 0.015f, this.Material, false, false, null);
			}
		}

		// Token: 0x06001FA5 RID: 8101 RVA: 0x000F8F18 File Offset: 0x000F7318
		public T GetComponent<T>() where T : WorldObjectComp
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				T t = this.comps[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}

		// Token: 0x06001FA6 RID: 8102 RVA: 0x000F8F7C File Offset: 0x000F737C
		public WorldObjectComp GetComponent(Type type)
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				if (type.IsAssignableFrom(this.comps[i].GetType()))
				{
					return this.comps[i];
				}
			}
			return null;
		}

		// Token: 0x06001FA7 RID: 8103 RVA: 0x000F8FE0 File Offset: 0x000F73E0
		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				foreach (Gizmo gizmo in this.comps[i].GetGizmos())
				{
					yield return gizmo;
				}
			}
			yield break;
		}

		// Token: 0x06001FA8 RID: 8104 RVA: 0x000F900C File Offset: 0x000F740C
		public virtual IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				foreach (Gizmo g in this.comps[i].GetCaravanGizmos(caravan))
				{
					yield return g;
				}
			}
			yield break;
		}

		// Token: 0x06001FA9 RID: 8105 RVA: 0x000F9040 File Offset: 0x000F7440
		public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				foreach (FloatMenuOption f in this.comps[i].GetFloatMenuOptions(caravan))
				{
					yield return f;
				}
			}
			yield break;
		}

		// Token: 0x06001FAA RID: 8106 RVA: 0x000F9074 File Offset: 0x000F7474
		public virtual IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				foreach (FloatMenuOption f in this.comps[i].GetTransportPodsFloatMenuOptions(pods, representative))
				{
					yield return f;
				}
			}
			yield break;
		}

		// Token: 0x06001FAB RID: 8107 RVA: 0x000F90AC File Offset: 0x000F74AC
		public virtual IEnumerable<InspectTabBase> GetInspectTabs()
		{
			IEnumerable<InspectTabBase> result;
			if (this.def.inspectorTabsResolved != null)
			{
				result = this.def.inspectorTabsResolved;
			}
			else
			{
				result = Enumerable.Empty<InspectTabBase>();
			}
			return result;
		}

		// Token: 0x06001FAC RID: 8108 RVA: 0x000F90E8 File Offset: 0x000F74E8
		public virtual bool AllMatchingObjectsOnScreenMatchesWith(WorldObject other)
		{
			return this.Faction == other.Faction;
		}

		// Token: 0x06001FAD RID: 8109 RVA: 0x000F910C File Offset: 0x000F750C
		public override string ToString()
		{
			return string.Concat(new object[]
			{
				base.GetType().Name,
				" ",
				this.LabelCap,
				" (tile=",
				this.Tile,
				")"
			});
		}

		// Token: 0x06001FAE RID: 8110 RVA: 0x000F916C File Offset: 0x000F756C
		public override int GetHashCode()
		{
			return this.ID;
		}

		// Token: 0x06001FAF RID: 8111 RVA: 0x000F9188 File Offset: 0x000F7588
		public string GetUniqueLoadID()
		{
			return "WorldObject_" + this.ID;
		}

		// Token: 0x06001FB0 RID: 8112 RVA: 0x000F91B4 File Offset: 0x000F75B4
		public virtual string GetDescription()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.def.description);
			for (int i = 0; i < this.comps.Count; i++)
			{
				string descriptionPart = this.comps[i].GetDescriptionPart();
				if (!descriptionPart.NullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(descriptionPart);
				}
			}
			return stringBuilder.ToString();
		}
	}
}
