using System;
using System.Collections.Generic;
using GorillaTagScripts;
using Photon.Pun;
using UnityEngine;

// Token: 0x02000278 RID: 632
public class VRRigReliableState : MonoBehaviourPunCallbacks, IGorillaSerializeable
{
	// Token: 0x170001A7 RID: 423
	// (get) Token: 0x06000E04 RID: 3588 RVA: 0x00049C73 File Offset: 0x00047E73
	public bool HasBracelet
	{
		get
		{
			return this.braceletBeadColors.Count > 0;
		}
	}

	// Token: 0x170001A8 RID: 424
	// (get) Token: 0x06000E05 RID: 3589 RVA: 0x00049C83 File Offset: 0x00047E83
	// (set) Token: 0x06000E06 RID: 3590 RVA: 0x00049C8B File Offset: 0x00047E8B
	public bool isDirty { get; private set; } = true;

	// Token: 0x06000E07 RID: 3591 RVA: 0x00049C94 File Offset: 0x00047E94
	private void Awake()
	{
		VRRig.newPlayerJoined = (Action)Delegate.Combine(VRRig.newPlayerJoined, new Action(this.SetIsDirty));
	}

	// Token: 0x06000E08 RID: 3592 RVA: 0x00049CB6 File Offset: 0x00047EB6
	private void OnDestroy()
	{
		VRRig.newPlayerJoined = (Action)Delegate.Remove(VRRig.newPlayerJoined, new Action(this.SetIsDirty));
	}

	// Token: 0x06000E09 RID: 3593 RVA: 0x00049CD8 File Offset: 0x00047ED8
	public void SetIsDirty()
	{
		this.isDirty = true;
	}

	// Token: 0x06000E0A RID: 3594 RVA: 0x00049CE1 File Offset: 0x00047EE1
	public void SetIsNotDirty()
	{
		this.isDirty = false;
	}

	// Token: 0x06000E0B RID: 3595 RVA: 0x00049CEA File Offset: 0x00047EEA
	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		this.SetIsDirty();
	}

	// Token: 0x06000E0C RID: 3596 RVA: 0x00049CF8 File Offset: 0x00047EF8
	public void SharedStart(bool isOfflineVRRig_, BodyDockPositions bDock_)
	{
		this.isOfflineVRRig = isOfflineVRRig_;
		this.bDock = bDock_;
		this.activeTransferrableObjectIndex = new int[5];
		for (int i = 0; i < this.activeTransferrableObjectIndex.Length; i++)
		{
			this.activeTransferrableObjectIndex[i] = -1;
		}
		this.transferrablePosStates = new TransferrableObject.PositionState[5];
		this.transferrableItemStates = new TransferrableObject.ItemStates[5];
		this.transferableDockPositions = new BodyDockPositions.DropPositions[5];
	}

	// Token: 0x06000E0D RID: 3597 RVA: 0x00049D60 File Offset: 0x00047F60
	void IGorillaSerializeable.OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!this.isDirty)
		{
			return;
		}
		this.isDirty = false;
		long num = 0L;
		for (int i = 0; i < this.activeTransferrableObjectIndex.Length; i++)
		{
			if (this.activeTransferrableObjectIndex[i] != -1)
			{
				num |= (long)((ulong)((byte)(1 << i)));
			}
		}
		if (this.isBraceletLeftHanded)
		{
			num |= 64L;
		}
		if (this.isMicEnabled)
		{
			num |= 32L;
		}
		if (this.isBuilderWatchEnabled)
		{
			num |= 128L;
		}
		num |= ((long)this.braceletBeadColors.Count & 15L) << 12;
		num |= (long)((long)((ulong)this.lThrowableProjectileColor.r) << 16);
		num |= (long)((long)((ulong)this.lThrowableProjectileColor.g) << 24);
		num |= (long)((long)((ulong)this.lThrowableProjectileColor.b) << 32);
		num |= (long)((long)((ulong)this.rThrowableProjectileColor.r) << 40);
		num |= (long)((long)((ulong)this.rThrowableProjectileColor.g) << 48);
		num |= (long)((long)((ulong)this.rThrowableProjectileColor.b) << 56);
		stream.SendNext(num);
		for (int j = 0; j < this.activeTransferrableObjectIndex.Length; j++)
		{
			if (this.activeTransferrableObjectIndex[j] != -1)
			{
				long num2 = (long)((ulong)this.activeTransferrableObjectIndex[j]);
				num2 |= (long)this.transferrablePosStates[j] << 32;
				num2 |= (long)this.transferrableItemStates[j] << 40;
				num2 |= (long)this.transferableDockPositions[j] << 48;
				stream.SendNext(num2);
			}
		}
		stream.SendNext(this.wearablesPackedStates);
		stream.SendNext(this.lThrowableProjectileIndex);
		stream.SendNext(this.rThrowableProjectileIndex);
		stream.SendNext(this.sizeLayerMask);
		stream.SendNext(this.randomThrowableIndex);
		if (this.braceletBeadColors.Count > 0)
		{
			long num3 = VRRigReliableState.PackBeadColors(this.braceletBeadColors, 0);
			if (this.braceletBeadColors.Count <= 3)
			{
				num3 |= (long)this.braceletSelfIndex << 30;
				stream.SendNext((int)num3);
				return;
			}
			num3 |= (long)this.braceletSelfIndex << 60;
			stream.SendNext(num3);
			if (this.braceletBeadColors.Count > 6)
			{
				stream.SendNext(VRRigReliableState.PackBeadColors(this.braceletBeadColors, 6));
			}
		}
	}

	// Token: 0x06000E0E RID: 3598 RVA: 0x00049FA4 File Offset: 0x000481A4
	void IGorillaSerializeable.OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
	{
		long num = (long)stream.ReceiveNext();
		this.isMicEnabled = ((num & 32L) != 0L);
		this.isBraceletLeftHanded = ((num & 64L) != 0L);
		this.isBuilderWatchEnabled = ((num & 128L) != 0L);
		int num2 = (int)(num >> 12) & 15;
		this.lThrowableProjectileColor.r = (byte)(num >> 16);
		this.lThrowableProjectileColor.g = (byte)(num >> 24);
		this.lThrowableProjectileColor.b = (byte)(num >> 32);
		this.rThrowableProjectileColor.r = (byte)(num >> 40);
		this.rThrowableProjectileColor.g = (byte)(num >> 48);
		this.rThrowableProjectileColor.b = (byte)(num >> 56);
		for (int i = 0; i < this.activeTransferrableObjectIndex.Length; i++)
		{
			if ((num & 1L << (i & 31)) != 0L)
			{
				long num3 = (long)stream.ReceiveNext();
				this.activeTransferrableObjectIndex[i] = (int)num3;
				this.transferrablePosStates[i] = (TransferrableObject.PositionState)(num3 >> 32 & 255L);
				this.transferrableItemStates[i] = (TransferrableObject.ItemStates)(num3 >> 40 & 255L);
				this.transferableDockPositions[i] = (BodyDockPositions.DropPositions)(num3 >> 48 & 255L);
			}
			else
			{
				this.activeTransferrableObjectIndex[i] = -1;
				this.transferrablePosStates[i] = TransferrableObject.PositionState.None;
				this.transferrableItemStates[i] = (TransferrableObject.ItemStates)0;
				this.transferableDockPositions[i] = BodyDockPositions.DropPositions.None;
			}
		}
		this.wearablesPackedStates = (int)stream.ReceiveNext();
		this.lThrowableProjectileIndex = (int)stream.ReceiveNext();
		this.rThrowableProjectileIndex = (int)stream.ReceiveNext();
		this.sizeLayerMask = (int)stream.ReceiveNext();
		this.randomThrowableIndex = (int)stream.ReceiveNext();
		this.braceletBeadColors.Clear();
		if (num2 > 0)
		{
			if (num2 <= 3)
			{
				int num4 = (int)stream.ReceiveNext();
				this.braceletSelfIndex = num4 >> 30;
				VRRigReliableState.UnpackBeadColors((long)num4, 0, num2, this.braceletBeadColors);
			}
			else
			{
				long num5 = (long)stream.ReceiveNext();
				this.braceletSelfIndex = (int)(num5 >> 60);
				if (num2 <= 6)
				{
					VRRigReliableState.UnpackBeadColors(num5, 0, num2, this.braceletBeadColors);
				}
				else
				{
					VRRigReliableState.UnpackBeadColors(num5, 0, 6, this.braceletBeadColors);
					VRRigReliableState.UnpackBeadColors((long)stream.ReceiveNext(), 6, num2, this.braceletBeadColors);
				}
			}
		}
		if (CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			this.bDock.RefreshTransferrableItems();
		}
		this.bDock.myRig.UpdateFriendshipBracelet();
		this.bDock.myRig.EnableBuilderResizeWatch(this.isBuilderWatchEnabled);
	}

	// Token: 0x06000E0F RID: 3599 RVA: 0x0004A214 File Offset: 0x00048414
	private static long PackBeadColors(List<Color> beadColors, int fromIndex)
	{
		long num = 0L;
		int num2 = Mathf.Min(fromIndex + 6, beadColors.Count);
		int num3 = 0;
		for (int i = fromIndex; i < num2; i++)
		{
			long num4 = (long)FriendshipGroupDetection.PackColor(beadColors[i]);
			num |= num4 << num3;
			num3 += 10;
		}
		return num;
	}

	// Token: 0x06000E10 RID: 3600 RVA: 0x0004A260 File Offset: 0x00048460
	private static void UnpackBeadColors(long packed, int startIndex, int endIndex, List<Color> beadColorsResult)
	{
		int num = Mathf.Min(startIndex + 6, endIndex);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			short data = (short)(packed >> num2 & 1023L);
			beadColorsResult.Add(FriendshipGroupDetection.UnpackColor(data));
			num2 += 10;
		}
	}

	// Token: 0x04000FD1 RID: 4049
	[NonSerialized]
	public int[] activeTransferrableObjectIndex;

	// Token: 0x04000FD2 RID: 4050
	[NonSerialized]
	public TransferrableObject.PositionState[] transferrablePosStates;

	// Token: 0x04000FD3 RID: 4051
	[NonSerialized]
	public TransferrableObject.ItemStates[] transferrableItemStates;

	// Token: 0x04000FD4 RID: 4052
	[NonSerialized]
	public BodyDockPositions.DropPositions[] transferableDockPositions;

	// Token: 0x04000FD5 RID: 4053
	[NonSerialized]
	public int wearablesPackedStates;

	// Token: 0x04000FD6 RID: 4054
	[NonSerialized]
	public int lThrowableProjectileIndex = -1;

	// Token: 0x04000FD7 RID: 4055
	[NonSerialized]
	public int rThrowableProjectileIndex = -1;

	// Token: 0x04000FD8 RID: 4056
	[NonSerialized]
	public Color32 lThrowableProjectileColor = Color.white;

	// Token: 0x04000FD9 RID: 4057
	[NonSerialized]
	public Color32 rThrowableProjectileColor = Color.white;

	// Token: 0x04000FDA RID: 4058
	[NonSerialized]
	public int randomThrowableIndex;

	// Token: 0x04000FDB RID: 4059
	[NonSerialized]
	public bool isMicEnabled;

	// Token: 0x04000FDC RID: 4060
	private bool isOfflineVRRig;

	// Token: 0x04000FDD RID: 4061
	private BodyDockPositions bDock;

	// Token: 0x04000FDE RID: 4062
	[NonSerialized]
	public int sizeLayerMask = 1;

	// Token: 0x04000FDF RID: 4063
	private const long IS_MIC_ENABLED_BIT = 32L;

	// Token: 0x04000FE0 RID: 4064
	private const long BRACELET_LEFTHAND_BIT = 64L;

	// Token: 0x04000FE1 RID: 4065
	private const long BUILDER_WATCH_ENABLED_BIT = 128L;

	// Token: 0x04000FE2 RID: 4066
	private const int BRACELET_NUM_BEADS_SHIFT = 12;

	// Token: 0x04000FE3 RID: 4067
	private const int LPROJECTILECOLOR_R_SHIFT = 16;

	// Token: 0x04000FE4 RID: 4068
	private const int LPROJECTILECOLOR_G_SHIFT = 24;

	// Token: 0x04000FE5 RID: 4069
	private const int LPROJECTILECOLOR_B_SHIFT = 32;

	// Token: 0x04000FE6 RID: 4070
	private const int RPROJECTILECOLOR_R_SHIFT = 40;

	// Token: 0x04000FE7 RID: 4071
	private const int RPROJECTILECOLOR_G_SHIFT = 48;

	// Token: 0x04000FE8 RID: 4072
	private const int RPROJECTILECOLOR_B_SHIFT = 56;

	// Token: 0x04000FE9 RID: 4073
	private const int POS_STATES_SHIFT = 32;

	// Token: 0x04000FEA RID: 4074
	private const int ITEM_STATES_SHIFT = 40;

	// Token: 0x04000FEB RID: 4075
	private const int DOCK_POSITIONS_SHIFT = 48;

	// Token: 0x04000FEC RID: 4076
	private const int BRACELET_SELF_INDEX_SHIFT = 60;

	// Token: 0x04000FED RID: 4077
	[NonSerialized]
	public bool isBraceletLeftHanded;

	// Token: 0x04000FEE RID: 4078
	[NonSerialized]
	public int braceletSelfIndex;

	// Token: 0x04000FEF RID: 4079
	[NonSerialized]
	public List<Color> braceletBeadColors = new List<Color>(10);

	// Token: 0x04000FF0 RID: 4080
	[NonSerialized]
	public bool isBuilderWatchEnabled;
}
