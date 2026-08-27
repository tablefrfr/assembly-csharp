using System;
using Fusion;
using Photon.Pun;

// Token: 0x02000124 RID: 292
public struct PhotonMessageInfoWrapped
{
	// Token: 0x1700008F RID: 143
	// (get) Token: 0x060005AA RID: 1450 RVA: 0x00020DC9 File Offset: 0x0001EFC9
	public double SentServerTime
	{
		get
		{
			return this.sentTick / 1000.0;
		}
	}

	// Token: 0x060005AB RID: 1451 RVA: 0x00020DDD File Offset: 0x0001EFDD
	public PhotonMessageInfoWrapped(PhotonMessageInfo info)
	{
		if (info.Sender != null)
		{
			this.senderID = info.Sender.ActorNumber;
			this.sentTick = info.SentServerTimestamp;
			return;
		}
		this.senderID = -1;
		this.sentTick = int.MinValue;
	}

	// Token: 0x060005AC RID: 1452 RVA: 0x00020E18 File Offset: 0x0001F018
	public PhotonMessageInfoWrapped(RpcInfo info)
	{
		this.senderID = info.Source.PlayerId;
		this.sentTick = info.Tick.Raw;
	}

	// Token: 0x060005AD RID: 1453 RVA: 0x00020E3D File Offset: 0x0001F03D
	public PhotonMessageInfoWrapped(int playerID, int tick)
	{
		this.senderID = playerID;
		this.sentTick = tick;
	}

	// Token: 0x0400071E RID: 1822
	public int senderID;

	// Token: 0x0400071F RID: 1823
	public int sentTick;
}
