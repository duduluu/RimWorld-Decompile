﻿using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000E4B RID: 3659
	[StaticConstructorOnStartup]
	public class EditWindow_Log : EditWindow
	{
		// Token: 0x040038FD RID: 14589
		private static LogMessage selectedMessage = null;

		// Token: 0x040038FE RID: 14590
		private static Vector2 messagesScrollPosition;

		// Token: 0x040038FF RID: 14591
		private static Vector2 detailsScrollPosition;

		// Token: 0x04003900 RID: 14592
		private static float detailsPaneHeight = 100f;

		// Token: 0x04003901 RID: 14593
		private static bool canAutoOpen = true;

		// Token: 0x04003902 RID: 14594
		public static bool wantsToOpen = false;

		// Token: 0x04003903 RID: 14595
		private float listingViewHeight;

		// Token: 0x04003904 RID: 14596
		private bool borderDragging = false;

		// Token: 0x04003905 RID: 14597
		private const float CountWidth = 28f;

		// Token: 0x04003906 RID: 14598
		private const float Yinc = 25f;

		// Token: 0x04003907 RID: 14599
		private const float DetailsPaneBorderHeight = 7f;

		// Token: 0x04003908 RID: 14600
		private const float DetailsPaneMinHeight = 10f;

		// Token: 0x04003909 RID: 14601
		private const float ListingMinHeight = 80f;

		// Token: 0x0400390A RID: 14602
		private const float TopAreaHeight = 26f;

		// Token: 0x0400390B RID: 14603
		private const float MessageMaxHeight = 30f;

		// Token: 0x0400390C RID: 14604
		private static readonly Texture2D AltMessageTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.17f, 0.17f, 0.17f, 0.85f));

		// Token: 0x0400390D RID: 14605
		private static readonly Texture2D SelectedMessageTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.25f, 0.25f, 0.17f, 0.85f));

		// Token: 0x0400390E RID: 14606
		private static readonly Texture2D StackTraceAreaTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.5f));

		// Token: 0x0400390F RID: 14607
		private static readonly Texture2D StackTraceBorderTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0.3f, 0.3f, 1f));

		// Token: 0x04003910 RID: 14608
		private static readonly string MessageDetailsControlName = "MessageDetailsTextArea";

		// Token: 0x0600564D RID: 22093 RVA: 0x002C7C47 File Offset: 0x002C6047
		public EditWindow_Log()
		{
			this.optionalTitle = "Debug log";
		}

		// Token: 0x17000D81 RID: 3457
		// (get) Token: 0x0600564E RID: 22094 RVA: 0x002C7C64 File Offset: 0x002C6064
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2((float)UI.screenWidth / 2f, (float)UI.screenHeight / 2f);
			}
		}

		// Token: 0x17000D82 RID: 3458
		// (get) Token: 0x0600564F RID: 22095 RVA: 0x002C7C98 File Offset: 0x002C6098
		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000D83 RID: 3459
		// (get) Token: 0x06005650 RID: 22096 RVA: 0x002C7CB0 File Offset: 0x002C60B0
		// (set) Token: 0x06005651 RID: 22097 RVA: 0x002C7CCA File Offset: 0x002C60CA
		private static LogMessage SelectedMessage
		{
			get
			{
				return EditWindow_Log.selectedMessage;
			}
			set
			{
				if (EditWindow_Log.selectedMessage != value)
				{
					EditWindow_Log.selectedMessage = value;
					if (UnityData.IsInMainThread && GUI.GetNameOfFocusedControl() == EditWindow_Log.MessageDetailsControlName)
					{
						UI.UnfocusCurrentControl();
					}
				}
			}
		}

		// Token: 0x06005652 RID: 22098 RVA: 0x002C7D06 File Offset: 0x002C6106
		public static void TryAutoOpen()
		{
			if (EditWindow_Log.canAutoOpen)
			{
				EditWindow_Log.wantsToOpen = true;
			}
		}

		// Token: 0x06005653 RID: 22099 RVA: 0x002C7D19 File Offset: 0x002C6119
		public static void ClearSelectedMessage()
		{
			EditWindow_Log.SelectedMessage = null;
			EditWindow_Log.detailsScrollPosition = Vector2.zero;
		}

		// Token: 0x06005654 RID: 22100 RVA: 0x002C7D2C File Offset: 0x002C612C
		public static void SelectLastMessage(bool expandDetailsPane = false)
		{
			EditWindow_Log.ClearSelectedMessage();
			EditWindow_Log.SelectedMessage = Log.Messages.LastOrDefault<LogMessage>();
			EditWindow_Log.messagesScrollPosition.y = (float)Log.Messages.Count<LogMessage>() * 30f;
			if (expandDetailsPane)
			{
				EditWindow_Log.detailsPaneHeight = 9999f;
			}
		}

		// Token: 0x06005655 RID: 22101 RVA: 0x002C7D79 File Offset: 0x002C6179
		public static void ClearAll()
		{
			EditWindow_Log.ClearSelectedMessage();
			EditWindow_Log.messagesScrollPosition = Vector2.zero;
		}

		// Token: 0x06005656 RID: 22102 RVA: 0x002C7D8B File Offset: 0x002C618B
		public override void PostClose()
		{
			base.PostClose();
			EditWindow_Log.wantsToOpen = false;
		}

		// Token: 0x06005657 RID: 22103 RVA: 0x002C7D9C File Offset: 0x002C619C
		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Tiny;
			WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
			if (widgetRow.ButtonText("Clear", "Clear all log messages.", true, false))
			{
				Log.Clear();
				EditWindow_Log.ClearAll();
			}
			if (widgetRow.ButtonText("Trace big", "Set the stack trace to be large on screen.", true, false))
			{
				EditWindow_Log.detailsPaneHeight = 700f;
			}
			if (widgetRow.ButtonText("Trace medium", "Set the stack trace to be medium-sized on screen.", true, false))
			{
				EditWindow_Log.detailsPaneHeight = 300f;
			}
			if (widgetRow.ButtonText("Trace small", "Set the stack trace to be small on screen.", true, false))
			{
				EditWindow_Log.detailsPaneHeight = 100f;
			}
			if (EditWindow_Log.canAutoOpen)
			{
				if (widgetRow.ButtonText("Auto-open is ON", "", true, false))
				{
					EditWindow_Log.canAutoOpen = false;
				}
			}
			else if (widgetRow.ButtonText("Auto-open is OFF", "", true, false))
			{
				EditWindow_Log.canAutoOpen = true;
			}
			if (widgetRow.ButtonText("Copy to clipboard", "Copy all messages to the clipboard.", true, false))
			{
				this.CopyAllMessagesToClipboard();
			}
			Text.Font = GameFont.Small;
			Rect rect = new Rect(inRect);
			rect.yMin += 26f;
			rect.yMax = inRect.height;
			if (EditWindow_Log.selectedMessage != null)
			{
				rect.yMax -= EditWindow_Log.detailsPaneHeight;
			}
			Rect detailsRect = new Rect(inRect);
			detailsRect.yMin = rect.yMax;
			this.DoMessagesListing(rect);
			this.DoMessageDetails(detailsRect, inRect);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect))
			{
				EditWindow_Log.ClearSelectedMessage();
			}
			EditWindow_Log.detailsPaneHeight = Mathf.Max(EditWindow_Log.detailsPaneHeight, 10f);
			EditWindow_Log.detailsPaneHeight = Mathf.Min(EditWindow_Log.detailsPaneHeight, inRect.height - 80f);
		}

		// Token: 0x06005658 RID: 22104 RVA: 0x002C7F8C File Offset: 0x002C638C
		public static void Notify_MessageDequeued(LogMessage oldMessage)
		{
			if (EditWindow_Log.SelectedMessage == oldMessage)
			{
				EditWindow_Log.SelectedMessage = null;
			}
		}

		// Token: 0x06005659 RID: 22105 RVA: 0x002C7FA0 File Offset: 0x002C63A0
		private void DoMessagesListing(Rect listingRect)
		{
			Rect viewRect = new Rect(0f, 0f, listingRect.width - 16f, this.listingViewHeight + 100f);
			Widgets.BeginScrollView(listingRect, ref EditWindow_Log.messagesScrollPosition, viewRect, true);
			float width = viewRect.width - 28f;
			Text.Font = GameFont.Tiny;
			float num = 0f;
			bool flag = false;
			foreach (LogMessage logMessage in Log.Messages)
			{
				float num2 = Text.CalcHeight(logMessage.text, width);
				if (num2 > 30f)
				{
					num2 = 30f;
				}
				GUI.color = new Color(1f, 1f, 1f, 0.7f);
				Rect rect = new Rect(4f, num, 28f, num2);
				Widgets.Label(rect, logMessage.repeats.ToStringCached());
				Rect rect2 = new Rect(28f, num, width, num2);
				if (EditWindow_Log.selectedMessage == logMessage)
				{
					GUI.DrawTexture(rect2, EditWindow_Log.SelectedMessageTex);
				}
				else if (flag)
				{
					GUI.DrawTexture(rect2, EditWindow_Log.AltMessageTex);
				}
				if (Widgets.ButtonInvisible(rect2, false))
				{
					EditWindow_Log.ClearSelectedMessage();
					EditWindow_Log.SelectedMessage = logMessage;
				}
				GUI.color = logMessage.Color;
				Widgets.Label(rect2, logMessage.text);
				num += num2;
				flag = !flag;
			}
			if (Event.current.type == EventType.Layout)
			{
				this.listingViewHeight = num;
			}
			Widgets.EndScrollView();
			GUI.color = Color.white;
		}

		// Token: 0x0600565A RID: 22106 RVA: 0x002C8168 File Offset: 0x002C6568
		private void DoMessageDetails(Rect detailsRect, Rect outRect)
		{
			if (EditWindow_Log.selectedMessage != null)
			{
				Rect rect = detailsRect;
				rect.height = 7f;
				Rect rect2 = detailsRect;
				rect2.yMin = rect.yMax;
				GUI.DrawTexture(rect, EditWindow_Log.StackTraceBorderTex);
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
				{
					this.borderDragging = true;
					Event.current.Use();
				}
				if (this.borderDragging)
				{
					EditWindow_Log.detailsPaneHeight = outRect.height + Mathf.Round(3.5f) - Event.current.mousePosition.y;
				}
				if (Event.current.rawType == EventType.MouseUp)
				{
					this.borderDragging = false;
				}
				GUI.DrawTexture(rect2, EditWindow_Log.StackTraceAreaTex);
				string text = EditWindow_Log.selectedMessage.text + "\n" + EditWindow_Log.selectedMessage.StackTrace;
				GUI.SetNextControlName(EditWindow_Log.MessageDetailsControlName);
				Widgets.TextAreaScrollable(rect2, text, ref EditWindow_Log.detailsScrollPosition, true);
			}
		}

		// Token: 0x0600565B RID: 22107 RVA: 0x002C827C File Offset: 0x002C667C
		private void CopyAllMessagesToClipboard()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (LogMessage logMessage in Log.Messages)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendLine(logMessage.text);
				stringBuilder.Append(logMessage.StackTrace);
				if (stringBuilder[stringBuilder.Length - 1] != '\n')
				{
					stringBuilder.AppendLine();
				}
			}
			GUIUtility.systemCopyBuffer = stringBuilder.ToString();
		}
	}
}
