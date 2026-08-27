using System;

// Token: 0x02000125 RID: 293
public abstract class NetPlayer
{
	// Token: 0x17000090 RID: 144
	// (get) Token: 0x060005AE RID: 1454
	public abstract bool IsValid { get; }

	// Token: 0x17000091 RID: 145
	// (get) Token: 0x060005AF RID: 1455
	public abstract int ID { get; }

	// Token: 0x17000092 RID: 146
	// (get) Token: 0x060005B0 RID: 1456
	public abstract string UserId { get; }

	// Token: 0x17000093 RID: 147
	// (get) Token: 0x060005B1 RID: 1457
	public abstract bool IsMaster { get; }

	// Token: 0x17000094 RID: 148
	// (get) Token: 0x060005B2 RID: 1458
	public abstract bool IsLocal { get; }

	// Token: 0x17000095 RID: 149
	// (get) Token: 0x060005B3 RID: 1459
	public abstract bool IsNull { get; }

	// Token: 0x17000096 RID: 150
	// (get) Token: 0x060005B4 RID: 1460
	public abstract string NickName { get; }

	// Token: 0x17000097 RID: 151
	// (get) Token: 0x060005B5 RID: 1461
	public abstract string DefaultName { get; }

	// Token: 0x17000098 RID: 152
	// (get) Token: 0x060005B6 RID: 1462
	public abstract bool InRoom { get; }

	// Token: 0x060005B7 RID: 1463
	public abstract bool Equals(NetPlayer myPlayer, NetPlayer other);
}
