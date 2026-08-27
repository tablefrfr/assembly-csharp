using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GorillaNetworking;
using Oculus.Platform;
using Oculus.Platform.Models;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using UnityEngine;

// Token: 0x0200012C RID: 300
public abstract class NetworkSystem : MonoBehaviour
{
	// Token: 0x170000A4 RID: 164
	// (get) Token: 0x060005DB RID: 1499 RVA: 0x000212E0 File Offset: 0x0001F4E0
	// (set) Token: 0x060005DC RID: 1500 RVA: 0x000212E8 File Offset: 0x0001F4E8
	public bool groupJoinInProgress { get; protected set; }

	// Token: 0x170000A5 RID: 165
	// (get) Token: 0x060005DD RID: 1501 RVA: 0x000212F1 File Offset: 0x0001F4F1
	// (set) Token: 0x060005DE RID: 1502 RVA: 0x000212F9 File Offset: 0x0001F4F9
	public NetSystemState netState
	{
		get
		{
			return this.testState;
		}
		protected set
		{
			Debug.Log("netstate set to:" + value.ToString());
			this.testState = value;
		}
	}

	// Token: 0x170000A6 RID: 166
	// (get) Token: 0x060005DF RID: 1503 RVA: 0x0002131E File Offset: 0x0001F51E
	public NetPlayer LocalPlayer
	{
		get
		{
			return this.netPlayerCache.Find((NetPlayer p) => p.IsLocal);
		}
	}

	// Token: 0x170000A7 RID: 167
	// (get) Token: 0x060005E0 RID: 1504 RVA: 0x0002134A File Offset: 0x0001F54A
	public bool IsMasterClient
	{
		get
		{
			return this.LocalPlayer.IsMaster;
		}
	}

	// Token: 0x170000A8 RID: 168
	// (get) Token: 0x060005E1 RID: 1505 RVA: 0x00021357 File Offset: 0x0001F557
	public Recorder LocalRecorder
	{
		get
		{
			return this.localRecorder;
		}
	}

	// Token: 0x170000A9 RID: 169
	// (get) Token: 0x060005E2 RID: 1506 RVA: 0x0002135F File Offset: 0x0001F55F
	// (set) Token: 0x060005E3 RID: 1507 RVA: 0x00021367 File Offset: 0x0001F567
	public virtual Speaker LocalSpeaker { get; set; }

	// Token: 0x14000003 RID: 3
	// (add) Token: 0x060005E4 RID: 1508 RVA: 0x00021370 File Offset: 0x0001F570
	// (remove) Token: 0x060005E5 RID: 1509 RVA: 0x000213A8 File Offset: 0x0001F5A8
	public event Action OnMultiplayerStarted;

	// Token: 0x060005E6 RID: 1510 RVA: 0x000213DD File Offset: 0x0001F5DD
	protected void MultiplayerStarted()
	{
		Action onMultiplayerStarted = this.OnMultiplayerStarted;
		if (onMultiplayerStarted == null)
		{
			return;
		}
		onMultiplayerStarted();
	}

	// Token: 0x14000004 RID: 4
	// (add) Token: 0x060005E7 RID: 1511 RVA: 0x000213F0 File Offset: 0x0001F5F0
	// (remove) Token: 0x060005E8 RID: 1512 RVA: 0x00021428 File Offset: 0x0001F628
	public event Action OnReturnedToSinglePlayer;

	// Token: 0x060005E9 RID: 1513 RVA: 0x0002145D File Offset: 0x0001F65D
	protected void SinglePlayerStarted()
	{
		Action onReturnedToSinglePlayer = this.OnReturnedToSinglePlayer;
		if (onReturnedToSinglePlayer == null)
		{
			return;
		}
		onReturnedToSinglePlayer();
	}

	// Token: 0x14000005 RID: 5
	// (add) Token: 0x060005EA RID: 1514 RVA: 0x00021470 File Offset: 0x0001F670
	// (remove) Token: 0x060005EB RID: 1515 RVA: 0x000214A8 File Offset: 0x0001F6A8
	public event Action<int> OnPlayerJoined;

	// Token: 0x060005EC RID: 1516 RVA: 0x000214DD File Offset: 0x0001F6DD
	protected void PlayerJoined(int playerID)
	{
		if (this.IsOnline)
		{
			Action<int> onPlayerJoined = this.OnPlayerJoined;
			if (onPlayerJoined == null)
			{
				return;
			}
			onPlayerJoined(playerID);
		}
	}

	// Token: 0x14000006 RID: 6
	// (add) Token: 0x060005ED RID: 1517 RVA: 0x000214F8 File Offset: 0x0001F6F8
	// (remove) Token: 0x060005EE RID: 1518 RVA: 0x00021530 File Offset: 0x0001F730
	public event Action<int> OnPlayerLeft;

	// Token: 0x060005EF RID: 1519 RVA: 0x00021565 File Offset: 0x0001F765
	protected void PlayerLeft(int playerID)
	{
		Action<int> onPlayerLeft = this.OnPlayerLeft;
		if (onPlayerLeft == null)
		{
			return;
		}
		onPlayerLeft(playerID);
	}

	// Token: 0x060005F0 RID: 1520 RVA: 0x00021578 File Offset: 0x0001F778
	public virtual void Initialise()
	{
		Debug.Log("INITIALISING NETWORKSYSTEMS");
		if (NetworkSystem.Instance)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		NetworkSystem.Instance = this;
		NetCrossoverUtils.Prewarm();
	}

	// Token: 0x060005F1 RID: 1521 RVA: 0x00003051 File Offset: 0x00001251
	protected virtual void Update()
	{
	}

	// Token: 0x060005F2 RID: 1522
	public abstract void SetAuthenticationValues(Dictionary<string, string> authValues);

	// Token: 0x060005F3 RID: 1523
	public abstract Task<NetJoinResult> ConnectToRoom(string roomName, RoomConfig opts, int regionIndex = -1);

	// Token: 0x060005F4 RID: 1524
	public abstract Task JoinFriendsRoom(string userID, int actorID, string keyToFollow, string shufflerToFollow);

	// Token: 0x060005F5 RID: 1525
	public abstract Task ReturnToSinglePlayer();

	// Token: 0x060005F6 RID: 1526
	public abstract void JoinPubWithFriends();

	// Token: 0x170000AA RID: 170
	// (get) Token: 0x060005F7 RID: 1527 RVA: 0x000215A7 File Offset: 0x0001F7A7
	public bool WrongVersion
	{
		get
		{
			return this.isWrongVersion;
		}
	}

	// Token: 0x060005F8 RID: 1528 RVA: 0x000215AF File Offset: 0x0001F7AF
	public void SetWrongVersion()
	{
		this.isWrongVersion = true;
	}

	// Token: 0x060005F9 RID: 1529 RVA: 0x000215B8 File Offset: 0x0001F7B8
	public GameObject NetInstantiate(GameObject prefab, bool isRoomObject = false)
	{
		return this.NetInstantiate(prefab, Vector3.zero, Quaternion.identity, false);
	}

	// Token: 0x060005FA RID: 1530 RVA: 0x000215CC File Offset: 0x0001F7CC
	public GameObject NetInstantiate(GameObject prefab, Vector3 position, bool isRoomObject = false)
	{
		return this.NetInstantiate(prefab, position, Quaternion.identity, false);
	}

	// Token: 0x060005FB RID: 1531
	public abstract GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool isRoomObject = false);

	// Token: 0x060005FC RID: 1532
	public abstract GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, int playerAuthID, bool isRoomObject = false);

	// Token: 0x060005FD RID: 1533
	public abstract void SetPlayerObject(GameObject playerInstance, int? owningPlayerID = null);

	// Token: 0x060005FE RID: 1534
	public abstract void NetDestroy(GameObject instance);

	// Token: 0x060005FF RID: 1535
	public abstract void CallRPC(MonoBehaviour component, NetworkSystem.RPC rpcMethod, bool sendToSelf = true);

	// Token: 0x06000600 RID: 1536
	public abstract void CallRPC<T>(MonoBehaviour component, NetworkSystem.RPC rpcMethod, RPCArgBuffer<T> args, bool sendToSelf = true) where T : struct;

	// Token: 0x06000601 RID: 1537
	public abstract void CallRPC(MonoBehaviour component, NetworkSystem.StringRPC rpcMethod, string message, bool sendToSelf = true);

	// Token: 0x06000602 RID: 1538
	public abstract void CallRPC(int targetPlayerID, MonoBehaviour component, NetworkSystem.RPC rpcMethod);

	// Token: 0x06000603 RID: 1539
	public abstract void CallRPC<T>(int targetPlayerID, MonoBehaviour component, NetworkSystem.RPC rpcMethod, RPCArgBuffer<T> args) where T : struct;

	// Token: 0x06000604 RID: 1540
	public abstract void CallRPC(int targetPlayerID, MonoBehaviour component, NetworkSystem.StringRPC rpcMethod, string message);

	// Token: 0x06000605 RID: 1541 RVA: 0x000215DC File Offset: 0x0001F7DC
	public static string GetRandomRoomName()
	{
		string text = "";
		for (int i = 0; i < 4; i++)
		{
			text += "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".Substring(Random.Range(0, "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".Length), 1);
		}
		if (GorillaComputer.instance.CheckAutoBanListForName(text))
		{
			return text;
		}
		return NetworkSystem.GetRandomRoomName();
	}

	// Token: 0x06000606 RID: 1542
	public abstract string GetRandomWeightedRegion();

	// Token: 0x06000607 RID: 1543 RVA: 0x00021634 File Offset: 0x0001F834
	protected Task RefreshOculusNonce()
	{
		NetworkSystem.<RefreshOculusNonce>d__78 <RefreshOculusNonce>d__;
		<RefreshOculusNonce>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
		<RefreshOculusNonce>d__.<>1__state = -1;
		<RefreshOculusNonce>d__.<>t__builder.Start<NetworkSystem.<RefreshOculusNonce>d__78>(ref <RefreshOculusNonce>d__);
		return <RefreshOculusNonce>d__.<>t__builder.Task;
	}

	// Token: 0x06000608 RID: 1544 RVA: 0x00021670 File Offset: 0x0001F870
	protected virtual void GetOculusNonceCallback(Message<UserProof> message)
	{
		AuthenticationValues authValues = PhotonNetwork.AuthValues;
		if (authValues != null)
		{
			Dictionary<string, object> dictionary = PhotonNetwork.AuthValues.AuthPostData as Dictionary<string, object>;
			if (dictionary != null)
			{
				if (message.IsError)
				{
					base.StartCoroutine(this.ReGetNonce());
					return;
				}
				dictionary["Nonce"] = message.Data.Value;
				authValues.SetAuthPostData(dictionary);
				PhotonNetwork.AuthValues = authValues;
				this.nonceRefreshed = true;
			}
		}
	}

	// Token: 0x06000609 RID: 1545 RVA: 0x000216D9 File Offset: 0x0001F8D9
	private IEnumerator ReGetNonce()
	{
		yield return new WaitForSeconds(3f);
		Users.GetUserProof().OnComplete(new Message<UserProof>.Callback(this.GetOculusNonceCallback));
		yield return null;
		yield break;
	}

	// Token: 0x0600060A RID: 1546 RVA: 0x000216E8 File Offset: 0x0001F8E8
	public void BroadcastMyRoom(bool create, string key, string shuffler)
	{
		string text = NetworkSystem.ShuffleRoomName(NetworkSystem.Instance.RoomName, shuffler.Substring(2, 8), true) + "|" + NetworkSystem.ShuffleRoomName("ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".Substring(NetworkSystem.Instance.currentRegionIndex, 1), shuffler.Substring(0, 2), true);
		Debug.Log(string.Format("Broadcasting room {0} region {1}({2}). Create: {3} key: {4} (shuffler {5}) shuffled: {6}", new object[]
		{
			NetworkSystem.Instance.RoomName,
			NetworkSystem.Instance.currentRegionIndex,
			NetworkSystem.Instance.regionNames[NetworkSystem.Instance.currentRegionIndex],
			create,
			key,
			shuffler,
			text
		}));
		GorillaServer instance = GorillaServer.Instance;
		BroadcastMyRoomRequest broadcastMyRoomRequest = new BroadcastMyRoomRequest();
		broadcastMyRoomRequest.KeyToFollow = key;
		broadcastMyRoomRequest.RoomToJoin = text;
		broadcastMyRoomRequest.Set = create;
		instance.BroadcastMyRoom(broadcastMyRoomRequest, delegate(ExecuteFunctionResult result)
		{
		}, delegate(PlayFabError error)
		{
		});
	}

	// Token: 0x0600060B RID: 1547 RVA: 0x00021800 File Offset: 0x0001FA00
	public bool InstantCheckGroupData(string userID, string keyToFollow)
	{
		bool success = false;
		GetSharedGroupDataRequest getSharedGroupDataRequest = new GetSharedGroupDataRequest();
		getSharedGroupDataRequest.Keys = new List<string>
		{
			keyToFollow
		};
		getSharedGroupDataRequest.SharedGroupId = userID;
		PlayFabClientAPI.GetSharedGroupData(getSharedGroupDataRequest, delegate(GetSharedGroupDataResult result)
		{
			Debug.Log("Get Shared Group Data returned a success");
			Debug.Log(result.Data.ToStringFull());
			if (result.Data.Count > 0)
			{
				success = true;
				return;
			}
			Debug.Log("RESULT returned but no DATA");
		}, delegate(PlayFabError error)
		{
			Debug.Log("ERROR - no group data found");
		}, null, null);
		return success;
	}

	// Token: 0x0600060C RID: 1548 RVA: 0x00021870 File Offset: 0x0001FA70
	public NetPlayer GetNetPlayerByID(int playerActorNumber)
	{
		return this.netPlayerCache.Find((NetPlayer a) => a.ID == playerActorNumber);
	}

	// Token: 0x0600060D RID: 1549 RVA: 0x000218A4 File Offset: 0x0001FAA4
	public static string ShuffleRoomName(string room, string shuffle, bool encode)
	{
		NetworkSystem.shuffleStringBuilder.Clear();
		int num;
		if (!int.TryParse(shuffle, out num))
		{
			Debug.Log("Shuffle room failed");
			return "";
		}
		for (int i = 0; i < room.Length; i++)
		{
			int num2 = int.Parse(shuffle.Substring(i * 2 % (shuffle.Length - 1), 2));
			int index = NetworkSystem.mod("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".IndexOf(room[i]) + (encode ? num2 : (-num2)), "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Length);
			NetworkSystem.shuffleStringBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"[index]);
		}
		return NetworkSystem.shuffleStringBuilder.ToString();
	}

	// Token: 0x0600060E RID: 1550 RVA: 0x0000DE29 File Offset: 0x0000C029
	public static int mod(int x, int m)
	{
		return (x % m + m) % m;
	}

	// Token: 0x0600060F RID: 1551
	public abstract Task AwaitSceneReady();

	// Token: 0x170000AB RID: 171
	// (get) Token: 0x06000610 RID: 1552
	public abstract string CurrentPhotonBackend { get; }

	// Token: 0x06000611 RID: 1553
	public abstract NetPlayer GetLocalPlayer();

	// Token: 0x06000612 RID: 1554
	public abstract NetPlayer GetPlayer(int PlayerID);

	// Token: 0x06000613 RID: 1555
	public abstract void SetMyNickName(string name);

	// Token: 0x06000614 RID: 1556
	public abstract string GetMyNickName();

	// Token: 0x06000615 RID: 1557
	public abstract string GetMyDefaultName();

	// Token: 0x06000616 RID: 1558
	public abstract string GetNickName(int playerID);

	// Token: 0x06000617 RID: 1559
	public abstract string GetMyUserID();

	// Token: 0x06000618 RID: 1560
	public abstract string GetUserID(int playerID);

	// Token: 0x06000619 RID: 1561
	public abstract void SetMyTutorialComplete();

	// Token: 0x0600061A RID: 1562
	public abstract bool GetMyTutorialCompletion();

	// Token: 0x0600061B RID: 1563
	public abstract bool GetPlayerTutorialCompletion(int playerID);

	// Token: 0x0600061C RID: 1564 RVA: 0x0002194A File Offset: 0x0001FB4A
	public void AddVoiceSettings(SO_NetworkVoiceSettings settings)
	{
		this.VoiceSettings = settings;
	}

	// Token: 0x0600061D RID: 1565
	public abstract void AddRemoteVoiceAddedCallback(Action<RemoteVoiceLink> callback);

	// Token: 0x170000AC RID: 172
	// (get) Token: 0x0600061E RID: 1566
	public abstract VoiceConnection VoiceConnection { get; }

	// Token: 0x170000AD RID: 173
	// (get) Token: 0x0600061F RID: 1567
	public abstract bool IsOnline { get; }

	// Token: 0x170000AE RID: 174
	// (get) Token: 0x06000620 RID: 1568
	public abstract bool InRoom { get; }

	// Token: 0x170000AF RID: 175
	// (get) Token: 0x06000621 RID: 1569
	public abstract string RoomName { get; }

	// Token: 0x170000B0 RID: 176
	// (get) Token: 0x06000622 RID: 1570
	public abstract string GameModeString { get; }

	// Token: 0x170000B1 RID: 177
	// (get) Token: 0x06000623 RID: 1571
	public abstract string CurrentRegion { get; }

	// Token: 0x170000B2 RID: 178
	// (get) Token: 0x06000624 RID: 1572
	public abstract bool SessionIsPrivate { get; }

	// Token: 0x170000B3 RID: 179
	// (get) Token: 0x06000625 RID: 1573
	public abstract int LocalPlayerID { get; }

	// Token: 0x170000B4 RID: 180
	// (get) Token: 0x06000626 RID: 1574
	public abstract int MasterAuthID { get; }

	// Token: 0x170000B5 RID: 181
	// (get) Token: 0x06000627 RID: 1575
	public abstract int[] AllPlayerIDs { get; }

	// Token: 0x170000B6 RID: 182
	// (get) Token: 0x06000628 RID: 1576 RVA: 0x00021953 File Offset: 0x0001FB53
	public NetPlayer[] AllNetPlayers
	{
		get
		{
			return this.netPlayerCache.ToArray();
		}
	}

	// Token: 0x06000629 RID: 1577
	protected abstract void UpdatePlayerIDCache();

	// Token: 0x0600062A RID: 1578
	protected abstract void UpdateNetPlayerList();

	// Token: 0x170000B7 RID: 183
	// (get) Token: 0x0600062B RID: 1579
	public abstract float SimTime { get; }

	// Token: 0x170000B8 RID: 184
	// (get) Token: 0x0600062C RID: 1580
	public abstract float SimDeltaTime { get; }

	// Token: 0x170000B9 RID: 185
	// (get) Token: 0x0600062D RID: 1581
	public abstract int SimTick { get; }

	// Token: 0x170000BA RID: 186
	// (get) Token: 0x0600062E RID: 1582
	public abstract int RoomPlayerCount { get; }

	// Token: 0x0600062F RID: 1583
	public abstract int GlobalPlayerCount();

	// Token: 0x170000BB RID: 187
	// (get) Token: 0x06000630 RID: 1584 RVA: 0x00021960 File Offset: 0x0001FB60
	// (set) Token: 0x06000631 RID: 1585 RVA: 0x00021968 File Offset: 0x0001FB68
	public RoomConfig CurrentRoom { get; protected set; }

	// Token: 0x06000632 RID: 1586
	public abstract bool IsObjectLocallyOwned(GameObject obj);

	// Token: 0x06000633 RID: 1587
	public abstract bool IsObjectRoomObject(GameObject obj);

	// Token: 0x06000634 RID: 1588
	public abstract bool ShouldUpdateObject(GameObject obj);

	// Token: 0x06000635 RID: 1589
	public abstract bool ShouldWriteObjectData(GameObject obj);

	// Token: 0x06000636 RID: 1590
	public abstract int GetOwningPlayerID(GameObject obj);

	// Token: 0x06000637 RID: 1591
	public abstract bool ShouldSpawnLocally(int playerID);

	// Token: 0x06000638 RID: 1592
	public abstract bool IsTotalAuthority();

	// Token: 0x04000739 RID: 1849
	public static NetworkSystem Instance;

	// Token: 0x0400073A RID: 1850
	public NetworkSystemConfig config;

	// Token: 0x0400073B RID: 1851
	public bool changingSceneManually;

	// Token: 0x0400073C RID: 1852
	public string[] regionNames;

	// Token: 0x0400073D RID: 1853
	public int currentRegionIndex;

	// Token: 0x0400073F RID: 1855
	private bool nonceRefreshed;

	// Token: 0x04000740 RID: 1856
	protected bool isWrongVersion;

	// Token: 0x04000741 RID: 1857
	private NetSystemState testState;

	// Token: 0x04000742 RID: 1858
	protected int[] playerIDCache;

	// Token: 0x04000743 RID: 1859
	protected List<NetPlayer> netPlayerCache = new List<NetPlayer>();

	// Token: 0x04000744 RID: 1860
	protected Recorder localRecorder;

	// Token: 0x04000745 RID: 1861
	protected Speaker localSpeaker;

	// Token: 0x04000747 RID: 1863
	protected SO_NetworkVoiceSettings VoiceSettings;

	// Token: 0x04000748 RID: 1864
	protected List<Action<RemoteVoiceLink>> remoteVoiceAddedCallbacks = new List<Action<RemoteVoiceLink>>();

	// Token: 0x0400074D RID: 1869
	protected static readonly byte[] EmptyArgs = new byte[0];

	// Token: 0x0400074E RID: 1870
	public const string roomCharacters = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789";

	// Token: 0x0400074F RID: 1871
	public const string shuffleCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

	// Token: 0x04000750 RID: 1872
	private static StringBuilder shuffleStringBuilder = new StringBuilder(4);

	// Token: 0x0200012D RID: 301
	// (Invoke) Token: 0x0600063C RID: 1596
	public delegate void RPC(byte[] data);

	// Token: 0x0200012E RID: 302
	// (Invoke) Token: 0x06000640 RID: 1600
	public delegate void StringRPC(string message);

	// Token: 0x0200012F RID: 303
	// (Invoke) Token: 0x06000644 RID: 1604
	public delegate void StaticRPC(byte[] data);

	// Token: 0x02000130 RID: 304
	// (Invoke) Token: 0x06000648 RID: 1608
	public delegate void StaticRPCPlaceholder(byte[] args);
}
