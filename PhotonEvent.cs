using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// Token: 0x02000469 RID: 1129
[Serializable]
public class PhotonEvent : IOnEventCallback, IEquatable<PhotonEvent>
{
	// Token: 0x17000315 RID: 789
	// (get) Token: 0x06001A40 RID: 6720 RVA: 0x000875E1 File Offset: 0x000857E1
	// (set) Token: 0x06001A41 RID: 6721 RVA: 0x000875E9 File Offset: 0x000857E9
	public bool reliable
	{
		get
		{
			return this._reliable;
		}
		set
		{
			this._reliable = value;
		}
	}

	// Token: 0x17000316 RID: 790
	// (get) Token: 0x06001A42 RID: 6722 RVA: 0x000875F2 File Offset: 0x000857F2
	// (set) Token: 0x06001A43 RID: 6723 RVA: 0x000875FA File Offset: 0x000857FA
	public bool failSilent
	{
		get
		{
			return this._failSilent;
		}
		set
		{
			this._failSilent = value;
		}
	}

	// Token: 0x06001A44 RID: 6724 RVA: 0x00087603 File Offset: 0x00085803
	private PhotonEvent()
	{
	}

	// Token: 0x06001A45 RID: 6725 RVA: 0x00087612 File Offset: 0x00085812
	public PhotonEvent(int eventId)
	{
		if (eventId == -1)
		{
			throw new Exception(string.Format("<{0}> cannot be {1}.", "eventId", -1));
		}
		this._eventId = eventId;
		this.Enable();
	}

	// Token: 0x06001A46 RID: 6726 RVA: 0x0008764D File Offset: 0x0008584D
	public PhotonEvent(string eventId) : this(StaticHash.Calculate(eventId))
	{
	}

	// Token: 0x06001A47 RID: 6727 RVA: 0x0008765B File Offset: 0x0008585B
	public PhotonEvent(int eventId, Action<int, int, object[], PhotonMessageInfoWrapped> callback) : this(eventId)
	{
		this.AddCallback(callback);
	}

	// Token: 0x06001A48 RID: 6728 RVA: 0x0008766B File Offset: 0x0008586B
	public PhotonEvent(string eventId, Action<int, int, object[], PhotonMessageInfoWrapped> callback) : this(eventId)
	{
		this.AddCallback(callback);
	}

	// Token: 0x06001A49 RID: 6729 RVA: 0x0008767C File Offset: 0x0008587C
	~PhotonEvent()
	{
		this.Dispose();
	}

	// Token: 0x06001A4A RID: 6730 RVA: 0x000876A8 File Offset: 0x000858A8
	public void AddCallback(Action<int, int, object[], PhotonMessageInfoWrapped> callback)
	{
		if (this._disposed)
		{
			return;
		}
		Delegate @delegate = this._delegate;
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		this._delegate = (Action<int, int, object[], PhotonMessageInfoWrapped>)Delegate.Combine(@delegate, callback);
	}

	// Token: 0x06001A4B RID: 6731 RVA: 0x000876D9 File Offset: 0x000858D9
	public void RemoveCallback(Action<int, int, object[], PhotonMessageInfoWrapped> callback)
	{
		if (this._disposed)
		{
			return;
		}
		if (callback != null)
		{
			this._delegate = (Action<int, int, object[], PhotonMessageInfoWrapped>)Delegate.Remove(this._delegate, callback);
		}
	}

	// Token: 0x06001A4C RID: 6732 RVA: 0x000876FE File Offset: 0x000858FE
	public void Enable()
	{
		if (this._disposed)
		{
			return;
		}
		if (this._enabled)
		{
			return;
		}
		if (Application.isPlaying)
		{
			PhotonNetwork.AddCallbackTarget(this);
		}
		this._enabled = true;
	}

	// Token: 0x06001A4D RID: 6733 RVA: 0x00087726 File Offset: 0x00085926
	public void Disable()
	{
		if (this._disposed)
		{
			return;
		}
		if (!this._enabled)
		{
			return;
		}
		if (Application.isPlaying)
		{
			PhotonNetwork.RemoveCallbackTarget(this);
		}
		this._enabled = false;
	}

	// Token: 0x06001A4E RID: 6734 RVA: 0x0008774E File Offset: 0x0008594E
	public void Dispose()
	{
		this._delegate = null;
		if (this._enabled)
		{
			this._enabled = false;
			if (Application.isPlaying)
			{
				PhotonNetwork.RemoveCallbackTarget(this);
			}
		}
		this._eventId = -1;
		this._disposed = true;
	}

	// Token: 0x14000024 RID: 36
	// (add) Token: 0x06001A4F RID: 6735 RVA: 0x00087784 File Offset: 0x00085984
	// (remove) Token: 0x06001A50 RID: 6736 RVA: 0x000877B8 File Offset: 0x000859B8
	public static event Action<PhotonEvent, Exception> OnError;

	// Token: 0x06001A51 RID: 6737 RVA: 0x000877EC File Offset: 0x000859EC
	void IOnEventCallback.OnEvent(EventData ev)
	{
		if (ev.Code != 176)
		{
			return;
		}
		if (this._disposed)
		{
			return;
		}
		if (!this._enabled)
		{
			return;
		}
		try
		{
			object[] array = (object[])ev.CustomData;
			if (array.Length == 0)
			{
				throw new Exception("Invalid/missing event data!");
			}
			int num = (int)array[0];
			int eventId = this._eventId;
			if (num == -1)
			{
				throw new Exception(string.Format("Invalid {0} ID! ({1})", "sender", -1));
			}
			if (eventId == -1)
			{
				throw new Exception(string.Format("Invalid {0} ID! ({1})", "receiver", -1));
			}
			object[] args = (array.Length == 1) ? Array.Empty<object>() : array.Skip(1).ToArray<object>();
			PhotonMessageInfoWrapped info = new PhotonMessageInfoWrapped(ev.Sender, PhotonNetwork.ServerTimestamp);
			this.InvokeDelegate(num, eventId, args, info);
		}
		catch (Exception ex)
		{
			Action<PhotonEvent, Exception> onError = PhotonEvent.OnError;
			if (onError != null)
			{
				onError(this, ex);
			}
			if (!this._failSilent)
			{
				throw ex;
			}
		}
	}

	// Token: 0x06001A52 RID: 6738 RVA: 0x000878F0 File Offset: 0x00085AF0
	private void InvokeDelegate(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
	{
		Action<int, int, object[], PhotonMessageInfoWrapped> @delegate = this._delegate;
		if (@delegate == null)
		{
			return;
		}
		@delegate(sender, target, args, info);
	}

	// Token: 0x06001A53 RID: 6739 RVA: 0x00087907 File Offset: 0x00085B07
	public void RaiseLocal(params object[] args)
	{
		this.Raise(PhotonEvent.RaiseMode.Local, args);
	}

	// Token: 0x06001A54 RID: 6740 RVA: 0x00087911 File Offset: 0x00085B11
	public void RaiseOthers(params object[] args)
	{
		this.Raise(PhotonEvent.RaiseMode.RemoteOthers, args);
	}

	// Token: 0x06001A55 RID: 6741 RVA: 0x0008791B File Offset: 0x00085B1B
	public void RaiseAll(params object[] args)
	{
		this.Raise(PhotonEvent.RaiseMode.RemoteAll, args);
	}

	// Token: 0x06001A56 RID: 6742 RVA: 0x00087928 File Offset: 0x00085B28
	private void Raise(PhotonEvent.RaiseMode mode, params object[] args)
	{
		if (this._disposed)
		{
			return;
		}
		if (!Application.isPlaying)
		{
			return;
		}
		if (!this._enabled)
		{
			return;
		}
		SendOptions sendOptions = this._reliable ? PhotonEvent.gSendReliable : PhotonEvent.gSendUnreliable;
		switch (mode)
		{
		case PhotonEvent.RaiseMode.Local:
			this.InvokeDelegate(this._eventId, this._eventId, args, new PhotonMessageInfoWrapped(PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.ServerTimestamp));
			return;
		case PhotonEvent.RaiseMode.RemoteOthers:
		{
			object[] eventContent = args.Prepend(this._eventId).ToArray<object>();
			PhotonNetwork.RaiseEvent(176, eventContent, PhotonEvent.gReceiversOthers, sendOptions);
			return;
		}
		case PhotonEvent.RaiseMode.RemoteAll:
		{
			object[] eventContent2 = args.Prepend(this._eventId).ToArray<object>();
			PhotonNetwork.RaiseEvent(176, eventContent2, PhotonEvent.gReceiversAll, sendOptions);
			return;
		}
		default:
			return;
		}
	}

	// Token: 0x06001A57 RID: 6743 RVA: 0x000879F4 File Offset: 0x00085BF4
	public bool Equals(PhotonEvent other)
	{
		return !(other == null) && (this._eventId == other._eventId && this._enabled == other._enabled && this._reliable == other._reliable && this._failSilent == other._failSilent) && this._disposed == other._disposed;
	}

	// Token: 0x06001A58 RID: 6744 RVA: 0x00087A54 File Offset: 0x00085C54
	public override bool Equals(object obj)
	{
		PhotonEvent photonEvent = obj as PhotonEvent;
		return photonEvent != null && this.Equals(photonEvent);
	}

	// Token: 0x06001A59 RID: 6745 RVA: 0x00087A74 File Offset: 0x00085C74
	public override int GetHashCode()
	{
		int staticHash = this._eventId.GetStaticHash();
		int i = StaticHash.Combine(this._enabled, this._reliable, this._failSilent, this._disposed);
		return StaticHash.Combine(staticHash, i);
	}

	// Token: 0x06001A5A RID: 6746 RVA: 0x00087AB0 File Offset: 0x00085CB0
	public static PhotonEvent operator +(PhotonEvent photonEvent, Action<int, int, object[], PhotonMessageInfoWrapped> callback)
	{
		if (photonEvent == null)
		{
			throw new ArgumentNullException("photonEvent");
		}
		photonEvent.AddCallback(callback);
		return photonEvent;
	}

	// Token: 0x06001A5B RID: 6747 RVA: 0x00087ACE File Offset: 0x00085CCE
	public static PhotonEvent operator -(PhotonEvent photonEvent, Action<int, int, object[], PhotonMessageInfoWrapped> callback)
	{
		if (photonEvent == null)
		{
			throw new ArgumentNullException("photonEvent");
		}
		photonEvent.RemoveCallback(callback);
		return photonEvent;
	}

	// Token: 0x06001A5C RID: 6748 RVA: 0x00087AEC File Offset: 0x00085CEC
	static PhotonEvent()
	{
		PhotonEvent.gSendUnreliable.Encrypt = true;
		PhotonEvent.gSendReliable = SendOptions.SendReliable;
		PhotonEvent.gSendReliable.Encrypt = true;
	}

	// Token: 0x06001A5D RID: 6749 RVA: 0x00087B45 File Offset: 0x00085D45
	public static bool operator ==(PhotonEvent x, PhotonEvent y)
	{
		return EqualityComparer<PhotonEvent>.Default.Equals(x, y);
	}

	// Token: 0x06001A5E RID: 6750 RVA: 0x00087B53 File Offset: 0x00085D53
	public static bool operator !=(PhotonEvent x, PhotonEvent y)
	{
		return !EqualityComparer<PhotonEvent>.Default.Equals(x, y);
	}

	// Token: 0x04001E58 RID: 7768
	private const int INVALID_ID = -1;

	// Token: 0x04001E59 RID: 7769
	[SerializeField]
	private int _eventId = -1;

	// Token: 0x04001E5A RID: 7770
	[SerializeField]
	private bool _enabled;

	// Token: 0x04001E5B RID: 7771
	[SerializeField]
	private bool _reliable;

	// Token: 0x04001E5C RID: 7772
	[SerializeField]
	private bool _failSilent;

	// Token: 0x04001E5D RID: 7773
	[NonSerialized]
	private bool _disposed;

	// Token: 0x04001E5E RID: 7774
	private Action<int, int, object[], PhotonMessageInfoWrapped> _delegate;

	// Token: 0x04001E60 RID: 7776
	public const byte PHOTON_EVENT_CODE = 176;

	// Token: 0x04001E61 RID: 7777
	private static readonly RaiseEventOptions gReceiversAll = new RaiseEventOptions
	{
		Receivers = ReceiverGroup.All
	};

	// Token: 0x04001E62 RID: 7778
	private static readonly RaiseEventOptions gReceiversOthers = new RaiseEventOptions
	{
		Receivers = ReceiverGroup.Others
	};

	// Token: 0x04001E63 RID: 7779
	private static readonly SendOptions gSendReliable;

	// Token: 0x04001E64 RID: 7780
	private static readonly SendOptions gSendUnreliable = SendOptions.SendUnreliable;

	// Token: 0x0200046A RID: 1130
	public enum RaiseMode
	{
		// Token: 0x04001E66 RID: 7782
		Local,
		// Token: 0x04001E67 RID: 7783
		RemoteOthers,
		// Token: 0x04001E68 RID: 7784
		RemoteAll
	}
}
