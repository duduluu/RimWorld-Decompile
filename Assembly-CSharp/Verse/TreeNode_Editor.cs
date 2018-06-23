﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000E7A RID: 3706
	public class TreeNode_Editor : TreeNode
	{
		// Token: 0x040039D9 RID: 14809
		public object obj;

		// Token: 0x040039DA RID: 14810
		public FieldInfo owningField;

		// Token: 0x040039DB RID: 14811
		public int owningIndex = -1;

		// Token: 0x040039DC RID: 14812
		private MethodInfo editWidgetsMethod = null;

		// Token: 0x040039DD RID: 14813
		public EditTreeNodeType nodeType;

		// Token: 0x040039DE RID: 14814
		private int indexToDelete = -1;

		// Token: 0x06005757 RID: 22359 RVA: 0x002CE77A File Offset: 0x002CCB7A
		private TreeNode_Editor()
		{
		}

		// Token: 0x17000DC5 RID: 3525
		// (get) Token: 0x06005758 RID: 22360 RVA: 0x002CE798 File Offset: 0x002CCB98
		public object ParentObj
		{
			get
			{
				return ((TreeNode_Editor)this.parentNode).obj;
			}
		}

		// Token: 0x17000DC6 RID: 3526
		// (get) Token: 0x06005759 RID: 22361 RVA: 0x002CE7C0 File Offset: 0x002CCBC0
		public Type ObjectType
		{
			get
			{
				Type result;
				if (this.owningField != null)
				{
					result = this.owningField.FieldType;
				}
				else if (this.IsListItem)
				{
					result = this.ListRootObject.GetType().GetGenericArguments()[0];
				}
				else
				{
					if (this.obj == null)
					{
						throw new InvalidOperationException();
					}
					result = this.obj.GetType();
				}
				return result;
			}
		}

		// Token: 0x17000DC7 RID: 3527
		// (get) Token: 0x0600575A RID: 22362 RVA: 0x002CE830 File Offset: 0x002CCC30
		// (set) Token: 0x0600575B RID: 22363 RVA: 0x002CE8AC File Offset: 0x002CCCAC
		public object Value
		{
			get
			{
				object value;
				if (this.owningField != null)
				{
					value = this.owningField.GetValue(this.ParentObj);
				}
				else
				{
					if (!this.IsListItem)
					{
						throw new InvalidOperationException();
					}
					value = this.ListRootObject.GetType().GetProperty("Item").GetValue(this.ListRootObject, new object[]
					{
						this.owningIndex
					});
				}
				return value;
			}
			set
			{
				if (this.owningField != null)
				{
					this.owningField.SetValue(this.ParentObj, value);
				}
				if (this.IsListItem)
				{
					this.ListRootObject.GetType().GetProperty("Item").SetValue(this.ListRootObject, value, new object[]
					{
						this.owningIndex
					});
				}
			}
		}

		// Token: 0x17000DC8 RID: 3528
		// (get) Token: 0x0600575C RID: 22364 RVA: 0x002CE918 File Offset: 0x002CCD18
		public bool IsListItem
		{
			get
			{
				return this.owningIndex >= 0;
			}
		}

		// Token: 0x17000DC9 RID: 3529
		// (get) Token: 0x0600575D RID: 22365 RVA: 0x002CE93C File Offset: 0x002CCD3C
		private object ListRootObject
		{
			get
			{
				return this.ParentObj;
			}
		}

		// Token: 0x17000DCA RID: 3530
		// (get) Token: 0x0600575E RID: 22366 RVA: 0x002CE958 File Offset: 0x002CCD58
		public override bool Openable
		{
			get
			{
				bool result;
				if (this.obj == null)
				{
					result = false;
				}
				else if (this.nodeType == EditTreeNodeType.TerminalValue)
				{
					result = false;
				}
				else
				{
					if (this.nodeType == EditTreeNodeType.ListRoot)
					{
						if ((int)this.obj.GetType().GetProperty("Count").GetValue(this.obj, null) == 0)
						{
							return false;
						}
					}
					result = true;
				}
				return result;
			}
		}

		// Token: 0x17000DCB RID: 3531
		// (get) Token: 0x0600575F RID: 22367 RVA: 0x002CE9D8 File Offset: 0x002CCDD8
		public bool HasContentLines
		{
			get
			{
				return this.nodeType != EditTreeNodeType.TerminalValue;
			}
		}

		// Token: 0x17000DCC RID: 3532
		// (get) Token: 0x06005760 RID: 22368 RVA: 0x002CE9FC File Offset: 0x002CCDFC
		public bool HasNewButton
		{
			get
			{
				if (this.nodeType == EditTreeNodeType.ComplexObject)
				{
					if (this.obj == null)
					{
						return true;
					}
				}
				return this.owningField != null && this.owningField.FieldType.HasAttribute<EditorReplaceableAttribute>();
			}
		}

		// Token: 0x17000DCD RID: 3533
		// (get) Token: 0x06005761 RID: 22369 RVA: 0x002CEA58 File Offset: 0x002CCE58
		public bool HasDeleteButton
		{
			get
			{
				return this.IsListItem || (this.owningField != null && this.owningField.FieldType.HasAttribute<EditorNullableAttribute>());
			}
		}

		// Token: 0x17000DCE RID: 3534
		// (get) Token: 0x06005762 RID: 22370 RVA: 0x002CEAA8 File Offset: 0x002CCEA8
		public string ExtraInfoText
		{
			get
			{
				string result;
				if (this.obj == null)
				{
					result = "null";
				}
				else if (this.obj.GetType().HasAttribute<EditorShowClassNameAttribute>())
				{
					result = this.obj.GetType().Name;
				}
				else if (this.obj.GetType().IsGenericType && this.obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
				{
					int num = (int)this.obj.GetType().GetProperty("Count").GetValue(this.obj, null);
					result = string.Concat(new string[]
					{
						"(",
						num.ToString(),
						" ",
						(num != 1) ? "elements" : "element",
						")"
					});
				}
				else
				{
					result = "";
				}
				return result;
			}
		}

		// Token: 0x17000DCF RID: 3535
		// (get) Token: 0x06005763 RID: 22371 RVA: 0x002CEBB8 File Offset: 0x002CCFB8
		public string LabelText
		{
			get
			{
				string result;
				if (this.owningField != null)
				{
					result = this.owningField.Name;
				}
				else if (this.IsListItem)
				{
					result = this.owningIndex.ToString();
				}
				else
				{
					result = this.ObjectType.Name;
				}
				return result;
			}
		}

		// Token: 0x06005764 RID: 22372 RVA: 0x002CEC18 File Offset: 0x002CD018
		public static TreeNode_Editor NewRootNode(object rootObj)
		{
			if (rootObj.GetType().IsValueEditable())
			{
				throw new ArgumentException();
			}
			TreeNode_Editor treeNode_Editor = new TreeNode_Editor();
			treeNode_Editor.owningField = null;
			treeNode_Editor.obj = rootObj;
			treeNode_Editor.nestDepth = 0;
			treeNode_Editor.RebuildChildNodes();
			treeNode_Editor.InitiallyCacheData();
			return treeNode_Editor;
		}

		// Token: 0x06005765 RID: 22373 RVA: 0x002CEC6C File Offset: 0x002CD06C
		public static TreeNode_Editor NewChildNodeFromField(TreeNode_Editor parent, FieldInfo fieldInfo)
		{
			TreeNode_Editor treeNode_Editor = new TreeNode_Editor();
			treeNode_Editor.parentNode = parent;
			treeNode_Editor.nestDepth = parent.nestDepth + 1;
			treeNode_Editor.owningField = fieldInfo;
			if (!fieldInfo.FieldType.IsValueEditable())
			{
				treeNode_Editor.obj = fieldInfo.GetValue(parent.obj);
				treeNode_Editor.RebuildChildNodes();
			}
			treeNode_Editor.InitiallyCacheData();
			return treeNode_Editor;
		}

		// Token: 0x06005766 RID: 22374 RVA: 0x002CECDC File Offset: 0x002CD0DC
		private static TreeNode_Editor NewChildNodeFromListItem(TreeNode_Editor parent, int listIndex)
		{
			TreeNode_Editor treeNode_Editor = new TreeNode_Editor();
			treeNode_Editor.parentNode = parent;
			treeNode_Editor.nestDepth = parent.nestDepth + 1;
			treeNode_Editor.owningIndex = listIndex;
			object obj = parent.obj;
			Type type = obj.GetType();
			Type type2 = type.GetGenericArguments()[0];
			if (!type2.IsValueEditable())
			{
				object value = type.GetProperty("Item").GetValue(obj, new object[]
				{
					listIndex
				});
				treeNode_Editor.obj = value;
				treeNode_Editor.RebuildChildNodes();
			}
			treeNode_Editor.InitiallyCacheData();
			return treeNode_Editor;
		}

		// Token: 0x06005767 RID: 22375 RVA: 0x002CED78 File Offset: 0x002CD178
		private void InitiallyCacheData()
		{
			if (this.obj != null && this.obj.GetType().IsGenericType && this.obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
			{
				this.nodeType = EditTreeNodeType.ListRoot;
			}
			else if (this.ObjectType.IsValueEditable())
			{
				this.nodeType = EditTreeNodeType.TerminalValue;
			}
			else
			{
				this.nodeType = EditTreeNodeType.ComplexObject;
			}
			if (this.obj != null)
			{
				this.editWidgetsMethod = this.obj.GetType().GetMethod("DoEditWidgets");
			}
		}

		// Token: 0x06005768 RID: 22376 RVA: 0x002CEE24 File Offset: 0x002CD224
		public void RebuildChildNodes()
		{
			if (this.obj != null)
			{
				this.children = new List<TreeNode>();
				Type objType = this.obj.GetType();
				if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(List<>))
				{
					int num = (int)objType.GetProperty("Count").GetValue(this.obj, null);
					for (int i = 0; i < num; i++)
					{
						TreeNode_Editor item = TreeNode_Editor.NewChildNodeFromListItem(this, i);
						this.children.Add(item);
					}
				}
				else
				{
					foreach (FieldInfo fieldInfo in from f in this.obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					orderby this.InheritanceDistanceBetween(objType, f.DeclaringType) descending
					select f)
					{
						if (fieldInfo.GetCustomAttributes(typeof(UnsavedAttribute), true).Length <= 0 && fieldInfo.GetCustomAttributes(typeof(EditorHiddenAttribute), true).Length <= 0)
						{
							TreeNode_Editor item2 = TreeNode_Editor.NewChildNodeFromField(this, fieldInfo);
							this.children.Add(item2);
						}
					}
				}
			}
		}

		// Token: 0x06005769 RID: 22377 RVA: 0x002CEFA4 File Offset: 0x002CD3A4
		private int InheritanceDistanceBetween(Type childType, Type parentType)
		{
			Type type = childType;
			int num = 0;
			while (type != parentType)
			{
				type = type.BaseType;
				num++;
				if (type == null)
				{
					Log.Error(childType + " is not a subclass of " + parentType, false);
					return -1;
				}
			}
			return num;
		}

		// Token: 0x0600576A RID: 22378 RVA: 0x002CEFF8 File Offset: 0x002CD3F8
		public void CheckLatentDelete()
		{
			if (this.indexToDelete >= 0)
			{
				this.obj.GetType().GetMethod("RemoveAt").Invoke(this.obj, new object[]
				{
					this.indexToDelete
				});
				this.RebuildChildNodes();
				this.indexToDelete = -1;
			}
		}

		// Token: 0x0600576B RID: 22379 RVA: 0x002CF058 File Offset: 0x002CD458
		public void Delete()
		{
			if (this.owningField != null)
			{
				this.owningField.SetValue(this.obj, null);
			}
			else
			{
				if (!this.IsListItem)
				{
					throw new InvalidOperationException();
				}
				((TreeNode_Editor)this.parentNode).indexToDelete = this.owningIndex;
			}
		}

		// Token: 0x0600576C RID: 22380 RVA: 0x002CF0B8 File Offset: 0x002CD4B8
		public void DoSpecialPreElements(Listing_TreeDefs listing)
		{
			if (this.obj != null)
			{
				if (this.editWidgetsMethod != null)
				{
					WidgetRow widgetRow = listing.StartWidgetsRow(this.nestDepth);
					this.editWidgetsMethod.Invoke(this.obj, new object[]
					{
						widgetRow
					});
				}
				Editable editable = this.obj as Editable;
				if (editable != null)
				{
					GUI.color = new Color(1f, 0.5f, 0.5f, 1f);
					foreach (string text in editable.ConfigErrors())
					{
						listing.InfoText(text, this.nestDepth);
					}
					GUI.color = Color.white;
				}
			}
		}

		// Token: 0x0600576D RID: 22381 RVA: 0x002CF1A0 File Offset: 0x002CD5A0
		public override string ToString()
		{
			string text = "EditTreeNode(";
			if (this.ParentObj != null)
			{
				text = text + " owningObj=" + this.ParentObj;
			}
			if (this.owningField != null)
			{
				text = text + " owningField=" + this.owningField;
			}
			if (this.owningIndex >= 0)
			{
				text = text + " owningIndex=" + this.owningIndex;
			}
			return text + ")";
		}
	}
}
