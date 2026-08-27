using System;
using System.Diagnostics;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// Token: 0x0200046B RID: 1131
public class PhotonTag : MonoBehaviour, IOnEventCallback, IEquatable<PhotonTag>
{
	// Token: 0x17000317 RID: 791
	// (get) Token: 0x06001A5F RID: 6751 RVA: 0x00087B64 File Offset: 0x00085D64
	public Id128 TagId
	{
		get
		{
			return this._tagId;
		}
	}

	// Token: 0x17000318 RID: 792
	// (get) Token: 0x06001A60 RID: 6752 RVA: 0x00087B6C File Offset: 0x00085D6C
	public Id128 SubId
	{
		get
		{
			return this._subId;
		}
	}

	// Token: 0x06001A61 RID: 6753 RVA: 0x00087B74 File Offset: 0x00085D74
	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			PhotonNetwork.AddCallbackTarget(this);
		}
	}

	// Token: 0x06001A62 RID: 6754 RVA: 0x00087B83 File Offset: 0x00085D83
	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			PhotonNetwork.RemoveCallbackTarget(this);
		}
	}

	// Token: 0x06001A63 RID: 6755 RVA: 0x00087B92 File Offset: 0x00085D92
	void IOnEventCallback.OnEvent(EventData ev)
	{
		byte code = ev.Code;
	}

	// Token: 0x06001A64 RID: 6756 RVA: 0x00003051 File Offset: 0x00001251
	[Conditional("UNITY_EDITOR")]
	private void Reset()
	{
	}

	// Token: 0x06001A65 RID: 6757 RVA: 0x00087BA1 File Offset: 0x00085DA1
	[Conditional("UNITY_EDITOR")]
	private void ComputeID()
	{
		if (Application.isPlaying)
		{
			return;
		}
		this._tagId = ComponentUtils.ComputeStaticHash128(this, 0);
	}

	// Token: 0x06001A66 RID: 6758 RVA: 0x00087BC0 File Offset: 0x00085DC0
	public bool Equals(PhotonTag other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		bool flag = this._tagId.Equals(other._tagId) && this._subId.Equals(other._subId);
		return base.Equals(other) && flag;
	}

	// Token: 0x06001A67 RID: 6759 RVA: 0x00087C0C File Offset: 0x00085E0C
	public override bool Equals(object obj)
	{
		if (this != obj)
		{
			PhotonTag photonTag = obj as PhotonTag;
			return photonTag != null && this.Equals(photonTag);
		}
		return true;
	}

	// Token: 0x06001A68 RID: 6760 RVA: 0x00087C32 File Offset: 0x00085E32
	public override int GetHashCode()
	{
		return StaticHash.Combine(this._tagId.GetHashCode(), this._subId.GetHashCode());
	}

	// Token: 0x06001A69 RID: 6761 RVA: 0x00087C5B File Offset: 0x00085E5B
	public static bool operator ==(PhotonTag x, PhotonTag y)
	{
		return object.Equals(x, y);
	}

	// Token: 0x06001A6A RID: 6762 RVA: 0x00087C64 File Offset: 0x00085E64
	public static bool operator !=(PhotonTag x, PhotonTag y)
	{
		return !object.Equals(x, y);
	}

	// Token: 0x04001E69 RID: 7785
	public const byte PHOTON_TAG_CODE = 177;

	// Token: 0x04001E6A RID: 7786
	[SerializeField]
	private Id128 _tagId;

	// Token: 0x04001E6B RID: 7787
	[SerializeField]
	private Id128 _subId;
}
