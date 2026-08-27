using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Photon.Realtime;
using UnityEngine;

// Token: 0x02000357 RID: 855
internal class VRRigCache : MonoBehaviour
{
	// Token: 0x17000202 RID: 514
	// (get) Token: 0x06001340 RID: 4928 RVA: 0x00065A58 File Offset: 0x00063C58
	// (set) Token: 0x06001341 RID: 4929 RVA: 0x00065A5F File Offset: 0x00063C5F
	public static VRRigCache Instance { get; private set; }

	// Token: 0x17000203 RID: 515
	// (get) Token: 0x06001342 RID: 4930 RVA: 0x00065A67 File Offset: 0x00063C67
	public Transform NetworkParent
	{
		get
		{
			return this.networkParent;
		}
	}

	// Token: 0x17000204 RID: 516
	// (get) Token: 0x06001343 RID: 4931 RVA: 0x00065A6F File Offset: 0x00063C6F
	// (set) Token: 0x06001344 RID: 4932 RVA: 0x00065A76 File Offset: 0x00063C76
	public static bool isInitialized { get; private set; }

	// Token: 0x1400001E RID: 30
	// (add) Token: 0x06001345 RID: 4933 RVA: 0x00065A80 File Offset: 0x00063C80
	// (remove) Token: 0x06001346 RID: 4934 RVA: 0x00065AB4 File Offset: 0x00063CB4
	public static event Action OnPostInitialize;

	// Token: 0x1400001F RID: 31
	// (add) Token: 0x06001347 RID: 4935 RVA: 0x00065AE8 File Offset: 0x00063CE8
	// (remove) Token: 0x06001348 RID: 4936 RVA: 0x00065B1C File Offset: 0x00063D1C
	public static event Action OnPostSpawnRig;

	// Token: 0x06001349 RID: 4937 RVA: 0x00065B4F File Offset: 0x00063D4F
	private void Start()
	{
		this.InitializeVRRigCache();
	}

	// Token: 0x0600134A RID: 4938 RVA: 0x00065B58 File Offset: 0x00063D58
	public void InitializeVRRigCache()
	{
		if (VRRigCache.isInitialized || ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (VRRigCache.Instance != null && VRRigCache.Instance != this)
		{
			Object.Destroy(this);
			return;
		}
		VRRigCache.Instance = this;
		if (this.rigParent == null)
		{
			this.rigParent = base.transform;
		}
		if (this.networkParent == null)
		{
			this.networkParent = base.transform;
		}
		int num = 0;
		while ((float)num < this.rigAmount)
		{
			RigContainer rigContainer = this.SpawnRig();
			VRRigCache.freeRigs.Enqueue(rigContainer);
			rigContainer.Rig.BuildInitialize();
			rigContainer.Rig.transform.parent = null;
			num++;
		}
		NetworkSystem.Instance.OnMultiplayerStarted += this.OnJoinedRoom;
		NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnLeftRoom;
		NetworkSystem.Instance.OnPlayerJoined += this.OnPlayerEnteredRoom;
		NetworkSystem.Instance.OnPlayerLeft += this.OnPlayerLeftRoom;
		VRRigCache.isInitialized = true;
		Action onPostInitialize = VRRigCache.OnPostInitialize;
		if (onPostInitialize == null)
		{
			return;
		}
		onPostInitialize();
	}

	// Token: 0x0600134B RID: 4939 RVA: 0x00065C7C File Offset: 0x00063E7C
	private void OnDestroy()
	{
		if (VRRigCache.Instance == this)
		{
			VRRigCache.Instance = null;
		}
	}

	// Token: 0x0600134C RID: 4940 RVA: 0x00065C94 File Offset: 0x00063E94
	private RigContainer SpawnRig()
	{
		if (this.rigTemplate.activeSelf)
		{
			this.rigTemplate.SetActive(false);
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.rigTemplate, this.rigParent, false);
		Action onPostSpawnRig = VRRigCache.OnPostSpawnRig;
		if (onPostSpawnRig != null)
		{
			onPostSpawnRig();
		}
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<RigContainer>();
	}

	// Token: 0x0600134D RID: 4941 RVA: 0x00065CE7 File Offset: 0x00063EE7
	internal bool TryGetVrrig(Player targetPlayer, out RigContainer playerRig)
	{
		return this.TryGetVrrig(NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber), out playerRig);
	}

	// Token: 0x0600134E RID: 4942 RVA: 0x00065D00 File Offset: 0x00063F00
	internal bool TryGetVrrig(NetPlayer targetPlayer, out RigContainer playerRig)
	{
		playerRig = null;
		if (ApplicationQuittingState.IsQuitting)
		{
			return false;
		}
		if (targetPlayer == null || targetPlayer.IsNull)
		{
			Debug.LogError("VrRigCache - target player is null");
			return false;
		}
		if (targetPlayer.IsLocal)
		{
			playerRig = this.localRig;
			return true;
		}
		if (!targetPlayer.InRoom)
		{
			this.LogWarning("player is not in room?? " + targetPlayer.UserId);
			return false;
		}
		if (VRRigCache.rigsInUse.ContainsKey(targetPlayer))
		{
			playerRig = VRRigCache.rigsInUse[targetPlayer];
		}
		else
		{
			if (VRRigCache.freeRigs.Count <= 0)
			{
				this.LogWarning("all rigs are in use");
				return false;
			}
			playerRig = VRRigCache.freeRigs.Dequeue();
			playerRig.Creator = ((PunNetPlayer)targetPlayer).playerRef;
			playerRig.CreatorWrapped = targetPlayer;
			VRRigCache.rigsInUse.Add(targetPlayer, playerRig);
			playerRig.gameObject.SetActive(true);
		}
		return true;
	}

	// Token: 0x0600134F RID: 4943 RVA: 0x00065DDC File Offset: 0x00063FDC
	private void AddRigToGorillaParent(NetPlayer player, VRRig vrrig)
	{
		GorillaParent instance = GorillaParent.instance;
		if (instance == null)
		{
			return;
		}
		if (!instance.vrrigs.Contains(vrrig))
		{
			instance.vrrigs.Add(vrrig);
		}
		if (!instance.vrrigDict.ContainsKey(player))
		{
			instance.vrrigDict.Add(player, vrrig);
			return;
		}
		instance.vrrigDict[player] = vrrig;
	}

	// Token: 0x06001350 RID: 4944 RVA: 0x00065E40 File Offset: 0x00064040
	public void OnPlayerEnteredRoom(int joiningPlayerID)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(joiningPlayerID);
		if (player.ID == -1)
		{
			Debug.LogError("LocalPlayer returned, vrrig no correctly initialised");
		}
		RigContainer rigContainer;
		if (this.TryGetVrrig(player, out rigContainer))
		{
			this.AddRigToGorillaParent(player, rigContainer.Rig);
		}
	}

	// Token: 0x06001351 RID: 4945 RVA: 0x00065E84 File Offset: 0x00064084
	public void OnJoinedRoom()
	{
		foreach (NetPlayer netPlayer in NetworkSystem.Instance.AllNetPlayers)
		{
			RigContainer rigContainer;
			if (this.TryGetVrrig(netPlayer, out rigContainer))
			{
				this.AddRigToGorillaParent(netPlayer, rigContainer.Rig);
			}
		}
	}

	// Token: 0x06001352 RID: 4946 RVA: 0x00065EC8 File Offset: 0x000640C8
	private void RemoveRigFromGorillaParent(NetPlayer player, VRRig vrrig)
	{
		GorillaParent instance = GorillaParent.instance;
		if (instance == null)
		{
			return;
		}
		if (instance.vrrigs.Contains(vrrig))
		{
			instance.vrrigs.Remove(vrrig);
		}
		if (instance.vrrigDict.ContainsKey(player))
		{
			instance.vrrigDict.Remove(player);
		}
	}

	// Token: 0x06001353 RID: 4947 RVA: 0x00065F20 File Offset: 0x00064120
	public void OnPlayerLeftRoom(int playerID)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(playerID);
		if (player == null)
		{
			Debug.LogError("Leaving players NetPlayer is Null");
			this.CheckForMissingPlayer();
		}
		RigContainer rigContainer;
		if (VRRigCache.rigsInUse.TryGetValue(player, out rigContainer))
		{
			rigContainer.gameObject.Disable();
			VRRigCache.freeRigs.Enqueue(rigContainer);
			VRRigCache.rigsInUse.Remove(player);
			this.RemoveRigFromGorillaParent(player, rigContainer.Rig);
			return;
		}
		this.LogError("failed to find player's vrrig who left " + player.UserId);
	}

	// Token: 0x06001354 RID: 4948 RVA: 0x00065FA4 File Offset: 0x000641A4
	private void CheckForMissingPlayer()
	{
		foreach (KeyValuePair<NetPlayer, RigContainer> keyValuePair in VRRigCache.rigsInUse)
		{
			if (keyValuePair.Key == null || keyValuePair.Value == null)
			{
				Debug.LogError("Somehow null reference in rigsInUse");
			}
			else if (!keyValuePair.Key.InRoom)
			{
				keyValuePair.Value.gameObject.Disable();
				VRRigCache.freeRigs.Enqueue(keyValuePair.Value);
				VRRigCache.rigsInUse.Remove(keyValuePair.Key);
				this.RemoveRigFromGorillaParent(keyValuePair.Key, keyValuePair.Value.Rig);
			}
		}
	}

	// Token: 0x06001355 RID: 4949 RVA: 0x00066074 File Offset: 0x00064274
	public void OnLeftRoom()
	{
		foreach (NetPlayer netPlayer in VRRigCache.rigsInUse.Keys.ToArray<NetPlayer>())
		{
			RigContainer rigContainer = VRRigCache.rigsInUse[netPlayer];
			if (!(rigContainer == null))
			{
				VRRig rig = VRRigCache.rigsInUse[netPlayer].Rig;
				rigContainer.gameObject.Disable();
				VRRigCache.rigsInUse.Remove(netPlayer);
				this.RemoveRigFromGorillaParent(netPlayer, rig);
				VRRigCache.freeRigs.Enqueue(rigContainer);
			}
		}
	}

	// Token: 0x06001356 RID: 4950 RVA: 0x000660F8 File Offset: 0x000642F8
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal VRRig[] GetAllRigs()
	{
		VRRig[] array = new VRRig[VRRigCache.rigsInUse.Count + VRRigCache.freeRigs.Count];
		int num = 0;
		foreach (RigContainer rigContainer in VRRigCache.rigsInUse.Values)
		{
			array[num] = rigContainer.Rig;
			num++;
		}
		foreach (RigContainer rigContainer2 in VRRigCache.freeRigs)
		{
			array[num] = rigContainer2.Rig;
			num++;
		}
		return array;
	}

	// Token: 0x06001357 RID: 4951 RVA: 0x000661C0 File Offset: 0x000643C0
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetAllRigsHash()
	{
		int num = 0;
		foreach (RigContainer rigContainer in VRRigCache.rigsInUse.Values)
		{
			num += rigContainer.GetInstanceID();
		}
		foreach (RigContainer rigContainer2 in VRRigCache.freeRigs)
		{
			num += rigContainer2.GetInstanceID();
		}
		return num;
	}

	// Token: 0x06001358 RID: 4952 RVA: 0x00003051 File Offset: 0x00001251
	private void LogInfo(string log)
	{
	}

	// Token: 0x06001359 RID: 4953 RVA: 0x00003051 File Offset: 0x00001251
	private void LogWarning(string log)
	{
	}

	// Token: 0x0600135A RID: 4954 RVA: 0x00003051 File Offset: 0x00001251
	private void LogError(string log)
	{
	}

	// Token: 0x0400163F RID: 5695
	public RigContainer localRig;

	// Token: 0x04001640 RID: 5696
	[SerializeField]
	private Transform rigParent;

	// Token: 0x04001641 RID: 5697
	[SerializeField]
	private Transform networkParent;

	// Token: 0x04001642 RID: 5698
	[SerializeField]
	private GameObject rigTemplate;

	// Token: 0x04001643 RID: 5699
	[SerializeField]
	private float rigAmount = 10f;

	// Token: 0x04001644 RID: 5700
	[OnEnterPlay_Clear]
	private static Queue<RigContainer> freeRigs = new Queue<RigContainer>(10);

	// Token: 0x04001645 RID: 5701
	[OnEnterPlay_Clear]
	private static Dictionary<NetPlayer, RigContainer> rigsInUse = new Dictionary<NetPlayer, RigContainer>(10);
}
