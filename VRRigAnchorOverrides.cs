using System;
using GorillaNetworking;
using UnityEngine;

// Token: 0x020002C3 RID: 707
public class VRRigAnchorOverrides : MonoBehaviour
{
	// Token: 0x170001C5 RID: 453
	// (get) Token: 0x06001043 RID: 4163 RVA: 0x000568FD File Offset: 0x00054AFD
	// (set) Token: 0x06001044 RID: 4164 RVA: 0x00056908 File Offset: 0x00054B08
	public Transform CurrentBadgeTransform
	{
		get
		{
			return this.currentBadgeTransform;
		}
		set
		{
			if (value != this.currentBadgeTransform)
			{
				this.ResetBadge();
				this.currentBadgeTransform = value;
				this.badgeDefaultRot = this.currentBadgeTransform.localRotation;
				this.badgeDefaultPos = this.currentBadgeTransform.localPosition;
				this.UpdateBadge();
			}
		}
	}

	// Token: 0x170001C6 RID: 454
	// (get) Token: 0x06001045 RID: 4165 RVA: 0x00056958 File Offset: 0x00054B58
	public Transform HuntDefaultAnchor
	{
		get
		{
			return this.huntComputerDefaultAnchor;
		}
	}

	// Token: 0x170001C7 RID: 455
	// (get) Token: 0x06001046 RID: 4166 RVA: 0x00056960 File Offset: 0x00054B60
	public Transform HuntComputer
	{
		get
		{
			return this.huntComputer;
		}
	}

	// Token: 0x170001C8 RID: 456
	// (get) Token: 0x06001047 RID: 4167 RVA: 0x00056968 File Offset: 0x00054B68
	public Transform BuilderWatchAnchor
	{
		get
		{
			return this.builderResizeButtonDefaultAnchor;
		}
	}

	// Token: 0x170001C9 RID: 457
	// (get) Token: 0x06001048 RID: 4168 RVA: 0x00056970 File Offset: 0x00054B70
	public Transform BuilderWatch
	{
		get
		{
			return this.builderResizeButton;
		}
	}

	// Token: 0x06001049 RID: 4169 RVA: 0x00056978 File Offset: 0x00054B78
	private void Awake()
	{
		for (int i = 0; i < 8; i++)
		{
			this.overrideAnchors[i] = null;
		}
		int num = this.MapPositionToIndex(TransferrableObject.PositionState.OnChest);
		this.overrideAnchors[num] = this.chestDefaultTransform;
		this.huntDefaultTransform = this.huntComputer;
		this.builderResizeButtonDefaultTransform = this.builderResizeButton;
	}

	// Token: 0x0600104A RID: 4170 RVA: 0x000569CC File Offset: 0x00054BCC
	private void OnEnable()
	{
		this.nameTransform.parent = this.nameDefaultAnchor.parent;
		this.huntComputer = this.huntDefaultTransform;
		this.huntComputer.parent = this.huntComputerDefaultAnchor.parent;
		this.builderResizeButton = this.builderResizeButtonDefaultTransform;
		this.builderResizeButton.parent = this.builderResizeButtonDefaultAnchor.parent;
	}

	// Token: 0x0600104B RID: 4171 RVA: 0x00056A34 File Offset: 0x00054C34
	private int MapPositionToIndex(TransferrableObject.PositionState pos)
	{
		int num = (int)pos;
		int num2 = 0;
		while ((num >>= 1) != 0)
		{
			num2++;
		}
		return num2;
	}

	// Token: 0x0600104C RID: 4172 RVA: 0x00056A54 File Offset: 0x00054C54
	public void OverrideAnchor(TransferrableObject.PositionState pos, Transform anchor)
	{
		int num = this.MapPositionToIndex(pos);
		if (this.overrideAnchors[num])
		{
			foreach (object obj in this.overrideAnchors[num])
			{
				((Transform)obj).parent = null;
			}
		}
		this.overrideAnchors[num] = anchor;
	}

	// Token: 0x0600104D RID: 4173 RVA: 0x00056AD0 File Offset: 0x00054CD0
	public Transform AnchorOverride(TransferrableObject.PositionState pos, Transform fallback)
	{
		int num = this.MapPositionToIndex(pos);
		Transform transform = this.overrideAnchors[num];
		if (transform != null)
		{
			return transform;
		}
		return fallback;
	}

	// Token: 0x0600104E RID: 4174 RVA: 0x00056AF4 File Offset: 0x00054CF4
	public void UpdateNameAnchor(GameObject nameAnchor, CosmeticsController.CosmeticSlots slot)
	{
		if (slot != CosmeticsController.CosmeticSlots.Badge)
		{
			switch (slot)
			{
			case CosmeticsController.CosmeticSlots.Shirt:
				this.nameAnchors[0] = nameAnchor;
				break;
			case CosmeticsController.CosmeticSlots.Pants:
				this.nameAnchors[1] = nameAnchor;
				break;
			case CosmeticsController.CosmeticSlots.Back:
				this.nameAnchors[2] = nameAnchor;
				break;
			}
		}
		else
		{
			this.nameAnchors[3] = nameAnchor;
		}
		this.UpdateName();
	}

	// Token: 0x0600104F RID: 4175 RVA: 0x00056B4C File Offset: 0x00054D4C
	private void UpdateName()
	{
		foreach (GameObject gameObject in this.nameAnchors)
		{
			if (gameObject)
			{
				this.nameTransform.parent = gameObject.transform;
				this.nameTransform.localRotation = Quaternion.identity;
				this.nameTransform.localPosition = Vector3.zero;
				return;
			}
		}
		this.nameTransform.parent = this.nameDefaultAnchor;
		this.nameTransform.localRotation = Quaternion.identity;
		this.nameTransform.localPosition = Vector3.zero;
	}

	// Token: 0x06001050 RID: 4176 RVA: 0x00056BDD File Offset: 0x00054DDD
	public void UpdateBadgeAnchor(GameObject badgeAnchor, CosmeticsController.CosmeticSlots slot)
	{
		switch (slot)
		{
		case CosmeticsController.CosmeticSlots.Shirt:
			this.badgeAnchors[0] = badgeAnchor;
			break;
		case CosmeticsController.CosmeticSlots.Pants:
			this.badgeAnchors[1] = badgeAnchor;
			break;
		case CosmeticsController.CosmeticSlots.Back:
			this.badgeAnchors[2] = badgeAnchor;
			break;
		}
		this.UpdateBadge();
	}

	// Token: 0x06001051 RID: 4177 RVA: 0x00056C1C File Offset: 0x00054E1C
	private void UpdateBadge()
	{
		if (!this.currentBadgeTransform)
		{
			return;
		}
		foreach (GameObject gameObject in this.badgeAnchors)
		{
			if (gameObject)
			{
				this.currentBadgeTransform.localRotation = gameObject.transform.localRotation;
				this.currentBadgeTransform.localPosition = gameObject.transform.localPosition;
				return;
			}
		}
		this.ResetBadge();
	}

	// Token: 0x06001052 RID: 4178 RVA: 0x00056C8B File Offset: 0x00054E8B
	private void ResetBadge()
	{
		if (!this.currentBadgeTransform)
		{
			return;
		}
		this.currentBadgeTransform.localRotation = this.badgeDefaultRot;
		this.currentBadgeTransform.localPosition = this.badgeDefaultPos;
	}

	// Token: 0x04001283 RID: 4739
	[SerializeField]
	internal Transform nameDefaultAnchor;

	// Token: 0x04001284 RID: 4740
	[SerializeField]
	internal Transform nameTransform;

	// Token: 0x04001285 RID: 4741
	[SerializeField]
	internal Transform chestDefaultTransform;

	// Token: 0x04001286 RID: 4742
	[SerializeField]
	internal Transform huntComputer;

	// Token: 0x04001287 RID: 4743
	[SerializeField]
	internal Transform huntComputerDefaultAnchor;

	// Token: 0x04001288 RID: 4744
	private Transform huntDefaultTransform;

	// Token: 0x04001289 RID: 4745
	[SerializeField]
	protected Transform builderResizeButton;

	// Token: 0x0400128A RID: 4746
	[SerializeField]
	protected Transform builderResizeButtonDefaultAnchor;

	// Token: 0x0400128B RID: 4747
	private Transform builderResizeButtonDefaultTransform;

	// Token: 0x0400128C RID: 4748
	private readonly Transform[] overrideAnchors = new Transform[8];

	// Token: 0x0400128D RID: 4749
	private GameObject nameLastObjectToAttach;

	// Token: 0x0400128E RID: 4750
	private Transform currentBadgeTransform;

	// Token: 0x0400128F RID: 4751
	private Vector3 badgeDefaultPos;

	// Token: 0x04001290 RID: 4752
	private Quaternion badgeDefaultRot;

	// Token: 0x04001291 RID: 4753
	private GameObject[] badgeAnchors = new GameObject[3];

	// Token: 0x04001292 RID: 4754
	private GameObject[] nameAnchors = new GameObject[4];

	// Token: 0x04001293 RID: 4755
	[SerializeField]
	public Transform friendshipBraceletLeftDefaultAnchor;

	// Token: 0x04001294 RID: 4756
	public Transform friendshipBraceletLeftAnchor;

	// Token: 0x04001295 RID: 4757
	[SerializeField]
	public Transform friendshipBraceletRightDefaultAnchor;

	// Token: 0x04001296 RID: 4758
	public Transform friendshipBraceletRightAnchor;
}
