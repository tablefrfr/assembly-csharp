using System;
using Photon.Pun;
using UnityEngine;

// Token: 0x02000358 RID: 856
internal class VrrigReliableSerializer : GorillaSerializer
{
	// Token: 0x0600135D RID: 4957 RVA: 0x00066294 File Offset: 0x00064494
	protected override bool OnInstantiateSetup(PhotonMessageInfo info, out GameObject outTargetObject, out Type outTargetType)
	{
		outTargetObject = null;
		outTargetType = null;
		if (info.Sender != info.photonView.Owner || this.photonView.IsRoomView)
		{
			return false;
		}
		RigContainer rigContainer;
		if (VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
		{
			outTargetObject = rigContainer.gameObject;
			outTargetType = typeof(VRRigReliableState);
			return true;
		}
		return false;
	}
}
