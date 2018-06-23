﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using RimWorld;
using Steamworks;

namespace Verse.Steam
{
	// Token: 0x02000FBF RID: 4031
	[HasDebugOutput]
	public static class Workshop
	{
		// Token: 0x04003FB9 RID: 16313
		private static WorkshopItemHook uploadingHook;

		// Token: 0x04003FBA RID: 16314
		private static UGCUpdateHandle_t curUpdateHandle;

		// Token: 0x04003FBB RID: 16315
		private static WorkshopInteractStage curStage = WorkshopInteractStage.None;

		// Token: 0x04003FBC RID: 16316
		private static Callback<RemoteStoragePublishedFileSubscribed_t> subscribedCallback;

		// Token: 0x04003FBD RID: 16317
		private static Callback<RemoteStoragePublishedFileUnsubscribed_t> unsubscribedCallback;

		// Token: 0x04003FBE RID: 16318
		private static Callback<ItemInstalled_t> installedCallback;

		// Token: 0x04003FBF RID: 16319
		private static CallResult<SubmitItemUpdateResult_t> submitResult;

		// Token: 0x04003FC0 RID: 16320
		private static CallResult<CreateItemResult_t> createResult;

		// Token: 0x04003FC1 RID: 16321
		private static CallResult<SteamUGCRequestUGCDetailsResult_t> requestDetailsResult;

		// Token: 0x04003FC2 RID: 16322
		private static UGCQueryHandle_t detailsQueryHandle;

		// Token: 0x04003FC3 RID: 16323
		private static int detailsQueryCount = -1;

		// Token: 0x04003FC4 RID: 16324
		public const uint InstallInfoFolderNameMaxLength = 257u;

		// Token: 0x04003FC5 RID: 16325
		[CompilerGenerated]
		private static Callback<RemoteStoragePublishedFileSubscribed_t>.DispatchDelegate <>f__mg$cache0;

		// Token: 0x04003FC6 RID: 16326
		[CompilerGenerated]
		private static Callback<ItemInstalled_t>.DispatchDelegate <>f__mg$cache1;

		// Token: 0x04003FC7 RID: 16327
		[CompilerGenerated]
		private static Callback<RemoteStoragePublishedFileUnsubscribed_t>.DispatchDelegate <>f__mg$cache2;

		// Token: 0x04003FC8 RID: 16328
		[CompilerGenerated]
		private static CallResult<SubmitItemUpdateResult_t>.APIDispatchDelegate <>f__mg$cache3;

		// Token: 0x04003FC9 RID: 16329
		[CompilerGenerated]
		private static CallResult<CreateItemResult_t>.APIDispatchDelegate <>f__mg$cache4;

		// Token: 0x04003FCA RID: 16330
		[CompilerGenerated]
		private static CallResult<SteamUGCRequestUGCDetailsResult_t>.APIDispatchDelegate <>f__mg$cache5;

		// Token: 0x04003FCB RID: 16331
		[CompilerGenerated]
		private static CallResult<SubmitItemUpdateResult_t>.APIDispatchDelegate <>f__mg$cache6;

		// Token: 0x17000FC5 RID: 4037
		// (get) Token: 0x06006173 RID: 24947 RVA: 0x003132C8 File Offset: 0x003116C8
		public static WorkshopInteractStage CurStage
		{
			get
			{
				return Workshop.curStage;
			}
		}

		// Token: 0x06006174 RID: 24948 RVA: 0x003132E4 File Offset: 0x003116E4
		internal static void Init()
		{
			if (Workshop.<>f__mg$cache0 == null)
			{
				Workshop.<>f__mg$cache0 = new Callback<RemoteStoragePublishedFileSubscribed_t>.DispatchDelegate(Workshop.OnItemSubscribed);
			}
			Workshop.subscribedCallback = Callback<RemoteStoragePublishedFileSubscribed_t>.Create(Workshop.<>f__mg$cache0);
			if (Workshop.<>f__mg$cache1 == null)
			{
				Workshop.<>f__mg$cache1 = new Callback<ItemInstalled_t>.DispatchDelegate(Workshop.OnItemInstalled);
			}
			Workshop.installedCallback = Callback<ItemInstalled_t>.Create(Workshop.<>f__mg$cache1);
			if (Workshop.<>f__mg$cache2 == null)
			{
				Workshop.<>f__mg$cache2 = new Callback<RemoteStoragePublishedFileUnsubscribed_t>.DispatchDelegate(Workshop.OnItemUnsubscribed);
			}
			Workshop.unsubscribedCallback = Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create(Workshop.<>f__mg$cache2);
		}

		// Token: 0x06006175 RID: 24949 RVA: 0x00313368 File Offset: 0x00311768
		internal static void Upload(WorkshopUploadable item)
		{
			if (Workshop.curStage != WorkshopInteractStage.None)
			{
				Messages.Message("UploadAlreadyInProgress".Translate(), MessageTypeDefOf.RejectInput, false);
			}
			else
			{
				Workshop.uploadingHook = item.GetWorkshopItemHook();
				if (Workshop.uploadingHook.PublishedFileId != PublishedFileId_t.Invalid)
				{
					if (Prefs.LogVerbose)
					{
						Log.Message(string.Concat(new object[]
						{
							"Workshop: Starting item update for mod '",
							Workshop.uploadingHook.Name,
							"' with PublishedFileId ",
							Workshop.uploadingHook.PublishedFileId
						}), false);
					}
					Workshop.curStage = WorkshopInteractStage.SubmittingItem;
					Workshop.curUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), Workshop.uploadingHook.PublishedFileId);
					Workshop.SetWorkshopItemDataFrom(Workshop.curUpdateHandle, Workshop.uploadingHook, false);
					SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(Workshop.curUpdateHandle, "[Auto-generated text]: Update on " + DateTime.Now.ToString() + ".");
					if (Workshop.<>f__mg$cache3 == null)
					{
						Workshop.<>f__mg$cache3 = new CallResult<SubmitItemUpdateResult_t>.APIDispatchDelegate(Workshop.OnItemSubmitted);
					}
					Workshop.submitResult = CallResult<SubmitItemUpdateResult_t>.Create(Workshop.<>f__mg$cache3);
					Workshop.submitResult.Set(hAPICall, null);
				}
				else
				{
					if (Prefs.LogVerbose)
					{
						Log.Message("Workshop: Starting item creation for mod '" + Workshop.uploadingHook.Name + "'.", false);
					}
					Workshop.curStage = WorkshopInteractStage.CreatingItem;
					SteamAPICall_t hAPICall2 = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeFirst);
					if (Workshop.<>f__mg$cache4 == null)
					{
						Workshop.<>f__mg$cache4 = new CallResult<CreateItemResult_t>.APIDispatchDelegate(Workshop.OnItemCreated);
					}
					Workshop.createResult = CallResult<CreateItemResult_t>.Create(Workshop.<>f__mg$cache4);
					Workshop.createResult.Set(hAPICall2, null);
				}
				Find.WindowStack.Add(new Dialog_WorkshopOperationInProgress());
			}
		}

		// Token: 0x06006176 RID: 24950 RVA: 0x0031351E File Offset: 0x0031191E
		internal static void Unsubscribe(WorkshopUploadable item)
		{
			SteamUGC.UnsubscribeItem(item.GetPublishedFileId());
		}

		// Token: 0x06006177 RID: 24951 RVA: 0x00313530 File Offset: 0x00311930
		internal static void RequestItemsDetails(PublishedFileId_t[] publishedFileIds)
		{
			if (Workshop.detailsQueryCount >= 0)
			{
				Log.Error("Requested Workshop item details while a details request was already pending.", false);
			}
			else
			{
				Workshop.detailsQueryCount = publishedFileIds.Length;
				Workshop.detailsQueryHandle = SteamUGC.CreateQueryUGCDetailsRequest(publishedFileIds, (uint)Workshop.detailsQueryCount);
				SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(Workshop.detailsQueryHandle);
				if (Workshop.<>f__mg$cache5 == null)
				{
					Workshop.<>f__mg$cache5 = new CallResult<SteamUGCRequestUGCDetailsResult_t>.APIDispatchDelegate(Workshop.OnGotItemDetails);
				}
				Workshop.requestDetailsResult = CallResult<SteamUGCRequestUGCDetailsResult_t>.Create(Workshop.<>f__mg$cache5);
				Workshop.requestDetailsResult.Set(hAPICall, null);
			}
		}

		// Token: 0x06006178 RID: 24952 RVA: 0x003135B0 File Offset: 0x003119B0
		internal static void OnItemSubscribed(RemoteStoragePublishedFileSubscribed_t result)
		{
			if (Workshop.IsOurAppId(result.m_nAppID))
			{
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item subscribed: " + result.m_nPublishedFileId, false);
				}
				WorkshopItems.Notify_Subscribed(result.m_nPublishedFileId);
			}
		}

		// Token: 0x06006179 RID: 24953 RVA: 0x00313608 File Offset: 0x00311A08
		internal static void OnItemInstalled(ItemInstalled_t result)
		{
			if (Workshop.IsOurAppId(result.m_unAppID))
			{
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item installed: " + result.m_nPublishedFileId, false);
				}
				WorkshopItems.Notify_Installed(result.m_nPublishedFileId);
			}
		}

		// Token: 0x0600617A RID: 24954 RVA: 0x00313660 File Offset: 0x00311A60
		internal static void OnItemUnsubscribed(RemoteStoragePublishedFileUnsubscribed_t result)
		{
			if (Workshop.IsOurAppId(result.m_nAppID))
			{
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item unsubscribed: " + result.m_nPublishedFileId, false);
				}
				Page_ModsConfig page_ModsConfig = Find.WindowStack.WindowOfType<Page_ModsConfig>();
				if (page_ModsConfig != null)
				{
					page_ModsConfig.Notify_SteamItemUnsubscribed(result.m_nPublishedFileId);
				}
				Page_SelectScenario page_SelectScenario = Find.WindowStack.WindowOfType<Page_SelectScenario>();
				if (page_SelectScenario != null)
				{
					page_SelectScenario.Notify_SteamItemUnsubscribed(result.m_nPublishedFileId);
				}
				WorkshopItems.Notify_Unsubscribed(result.m_nPublishedFileId);
			}
		}

		// Token: 0x0600617B RID: 24955 RVA: 0x003136F4 File Offset: 0x00311AF4
		private static void OnItemCreated(CreateItemResult_t result, bool IOFailure)
		{
			if (IOFailure || result.m_eResult != EResult.k_EResultOK)
			{
				Workshop.uploadingHook = null;
				Dialog_WorkshopOperationInProgress.CloseAll();
				Log.Error("Workshop: OnItemCreated failure. Result: " + result.m_eResult.GetLabel(), false);
				Find.WindowStack.Add(new Dialog_MessageBox("WorkshopSubmissionFailed".Translate(new object[]
				{
					GenText.SplitCamelCase(result.m_eResult.GetLabel())
				}), null, null, null, null, null, false, null, null));
			}
			else
			{
				Workshop.uploadingHook.PublishedFileId = result.m_nPublishedFileId;
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item created. PublishedFileId: " + Workshop.uploadingHook.PublishedFileId, false);
				}
				Workshop.curUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), Workshop.uploadingHook.PublishedFileId);
				Workshop.SetWorkshopItemDataFrom(Workshop.curUpdateHandle, Workshop.uploadingHook, true);
				Workshop.curStage = WorkshopInteractStage.SubmittingItem;
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Submitting item.", false);
				}
				SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(Workshop.curUpdateHandle, "[Auto-generated text]: Initial upload.");
				if (Workshop.<>f__mg$cache6 == null)
				{
					Workshop.<>f__mg$cache6 = new CallResult<SubmitItemUpdateResult_t>.APIDispatchDelegate(Workshop.OnItemSubmitted);
				}
				Workshop.submitResult = CallResult<SubmitItemUpdateResult_t>.Create(Workshop.<>f__mg$cache6);
				Workshop.submitResult.Set(hAPICall, null);
				Workshop.createResult = null;
			}
		}

		// Token: 0x0600617C RID: 24956 RVA: 0x00313844 File Offset: 0x00311C44
		private static void OnItemSubmitted(SubmitItemUpdateResult_t result, bool IOFailure)
		{
			if (IOFailure || result.m_eResult != EResult.k_EResultOK)
			{
				Workshop.uploadingHook = null;
				Dialog_WorkshopOperationInProgress.CloseAll();
				Log.Error("Workshop: OnItemSubmitted failure. Result: " + result.m_eResult.GetLabel(), false);
				Find.WindowStack.Add(new Dialog_MessageBox("WorkshopSubmissionFailed".Translate(new object[]
				{
					GenText.SplitCamelCase(result.m_eResult.GetLabel())
				}), null, null, null, null, null, false, null, null));
			}
			else
			{
				SteamUtility.OpenWorkshopPage(Workshop.uploadingHook.PublishedFileId);
				Messages.Message("WorkshopUploadSucceeded".Translate(new object[]
				{
					Workshop.uploadingHook.Name
				}), MessageTypeDefOf.TaskCompletion, false);
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item submit result: " + result.m_eResult, false);
				}
			}
			Workshop.curStage = WorkshopInteractStage.None;
			Workshop.submitResult = null;
		}

		// Token: 0x0600617D RID: 24957 RVA: 0x0031393C File Offset: 0x00311D3C
		private static void OnGotItemDetails(SteamUGCRequestUGCDetailsResult_t result, bool IOFailure)
		{
			if (IOFailure)
			{
				Log.Error("Workshop: OnGotItemDetails IOFailure.", false);
				Workshop.detailsQueryCount = -1;
			}
			else
			{
				if (Workshop.detailsQueryCount < 0)
				{
					Log.Warning("Got unexpected Steam Workshop item details response.", false);
				}
				string text = "Steam Workshop Item details received:";
				for (int i = 0; i < Workshop.detailsQueryCount; i++)
				{
					SteamUGCDetails_t steamUGCDetails_t;
					SteamUGC.GetQueryUGCResult(Workshop.detailsQueryHandle, (uint)i, out steamUGCDetails_t);
					if (steamUGCDetails_t.m_eResult != EResult.k_EResultOK)
					{
						text = text + "\n  Query result: " + steamUGCDetails_t.m_eResult;
					}
					else
					{
						text = text + "\n  Title: " + steamUGCDetails_t.m_rgchTitle;
						text = text + "\n  PublishedFileId: " + steamUGCDetails_t.m_nPublishedFileId;
						text = text + "\n  Created: " + DateTime.FromFileTimeUtc((long)((ulong)steamUGCDetails_t.m_rtimeCreated)).ToString();
						text = text + "\n  Updated: " + DateTime.FromFileTimeUtc((long)((ulong)steamUGCDetails_t.m_rtimeUpdated)).ToString();
						text = text + "\n  Added to list: " + DateTime.FromFileTimeUtc((long)((ulong)steamUGCDetails_t.m_rtimeAddedToUserList)).ToString();
						text = text + "\n  File size: " + steamUGCDetails_t.m_nFileSize.ToStringKilobytes("F2");
						text = text + "\n  Preview size: " + steamUGCDetails_t.m_nPreviewFileSize.ToStringKilobytes("F2");
						text = text + "\n  File name: " + steamUGCDetails_t.m_pchFileName;
						text = text + "\n  CreatorAppID: " + steamUGCDetails_t.m_nCreatorAppID;
						text = text + "\n  ConsumerAppID: " + steamUGCDetails_t.m_nConsumerAppID;
						text = text + "\n  Visibiliy: " + steamUGCDetails_t.m_eVisibility;
						text = text + "\n  FileType: " + steamUGCDetails_t.m_eFileType;
						text = text + "\n  Owner: " + steamUGCDetails_t.m_ulSteamIDOwner;
					}
					text += "\n";
				}
				Log.Message(text.TrimEndNewlines(), false);
				Workshop.detailsQueryCount = -1;
			}
		}

		// Token: 0x0600617E RID: 24958 RVA: 0x00313B5C File Offset: 0x00311F5C
		public static void GetUpdateStatus(out EItemUpdateStatus updateStatus, out float progPercent)
		{
			ulong num;
			ulong num2;
			updateStatus = SteamUGC.GetItemUpdateProgress(Workshop.curUpdateHandle, out num, out num2);
			progPercent = num / num2;
		}

		// Token: 0x0600617F RID: 24959 RVA: 0x00313B84 File Offset: 0x00311F84
		public static string UploadButtonLabel(PublishedFileId_t pfid)
		{
			return (!(pfid != PublishedFileId_t.Invalid)) ? "UploadToSteamWorkshop".Translate() : "UpdateOnSteamWorkshop".Translate();
		}

		// Token: 0x06006180 RID: 24960 RVA: 0x00313BC4 File Offset: 0x00311FC4
		private static void SetWorkshopItemDataFrom(UGCUpdateHandle_t updateHandle, WorkshopItemHook hook, bool creating)
		{
			hook.PrepareForWorkshopUpload();
			SteamUGC.SetItemTitle(updateHandle, hook.Name);
			if (creating)
			{
				SteamUGC.SetItemDescription(updateHandle, hook.Description);
			}
			if (!File.Exists(hook.PreviewImagePath))
			{
				Log.Warning("Missing preview file at " + hook.PreviewImagePath, false);
			}
			else
			{
				SteamUGC.SetItemPreview(updateHandle, hook.PreviewImagePath);
			}
			IList<string> tags = hook.Tags;
			tags.Add(VersionControl.CurrentMajor + "." + VersionControl.CurrentMinor);
			SteamUGC.SetItemTags(updateHandle, tags);
			SteamUGC.SetItemContent(updateHandle, hook.Directory.FullName);
		}

		// Token: 0x06006181 RID: 24961 RVA: 0x00313C78 File Offset: 0x00312078
		internal static IEnumerable<PublishedFileId_t> AllSubscribedItems()
		{
			uint numSub = SteamUGC.GetNumSubscribedItems();
			PublishedFileId_t[] subbedItems = new PublishedFileId_t[numSub];
			uint count = SteamUGC.GetSubscribedItems(subbedItems, numSub);
			int i = 0;
			while ((long)i < (long)((ulong)count))
			{
				PublishedFileId_t pfid = subbedItems[i];
				yield return pfid;
				i++;
			}
			yield break;
		}

		// Token: 0x06006182 RID: 24962 RVA: 0x00313C9C File Offset: 0x0031209C
		[DebugOutput]
		[Category("System")]
		internal static void SteamWorkshopStatus()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("All subscribed items (" + SteamUGC.GetNumSubscribedItems() + " total):");
			List<PublishedFileId_t> list = Workshop.AllSubscribedItems().ToList<PublishedFileId_t>();
			for (int i = 0; i < list.Count; i++)
			{
				stringBuilder.AppendLine("   " + Workshop.ItemStatusString(list[i]));
			}
			stringBuilder.AppendLine("All installed mods:");
			foreach (ModMetaData modMetaData in ModLister.AllInstalledMods)
			{
				stringBuilder.AppendLine("   " + modMetaData.Identifier + ": " + Workshop.ItemStatusString(modMetaData.GetPublishedFileId()));
			}
			Log.Message(stringBuilder.ToString(), false);
			List<PublishedFileId_t> list2 = Workshop.AllSubscribedItems().ToList<PublishedFileId_t>();
			PublishedFileId_t[] array = new PublishedFileId_t[list2.Count];
			for (int j = 0; j < list2.Count; j++)
			{
				array[j] = (PublishedFileId_t)list2[j].m_PublishedFileId;
			}
			Workshop.RequestItemsDetails(array);
		}

		// Token: 0x06006183 RID: 24963 RVA: 0x00313E08 File Offset: 0x00312208
		private static string ItemStatusString(PublishedFileId_t pfid)
		{
			string result;
			if (pfid == PublishedFileId_t.Invalid)
			{
				result = "[unpublished]";
			}
			else
			{
				string text = "[" + pfid + "] ";
				ulong num;
				string str;
				uint num2;
				if (SteamUGC.GetItemInstallInfo(pfid, out num, out str, 257u, out num2))
				{
					text += "\n      installed";
					text = text + "\n      folder=" + str;
					text = text + "\n      sizeOnDisk=" + (num / 1024f).ToString("F2") + "Kb";
				}
				else
				{
					text += "\n      not installed";
				}
				result = text;
			}
			return result;
		}

		// Token: 0x06006184 RID: 24964 RVA: 0x00313EBC File Offset: 0x003122BC
		private static bool IsOurAppId(AppId_t appId)
		{
			return !(appId != SteamUtils.GetAppID());
		}
	}
}
