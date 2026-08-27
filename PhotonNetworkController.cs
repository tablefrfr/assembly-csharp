using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fusion;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaTagScripts;
using Photon.Pun;
using UnityEngine;
using WebSocketSharp;

namespace GorillaNetworking
{
	// Token: 0x020006D8 RID: 1752
	public class PhotonNetworkController : MonoBehaviour
	{
		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x0600295B RID: 10587 RVA: 0x000CB87F File Offset: 0x000C9A7F
		// (set) Token: 0x0600295C RID: 10588 RVA: 0x000CB887 File Offset: 0x000C9A87
		public string StartLevel
		{
			get
			{
				return this.startLevel;
			}
			set
			{
				this.startLevel = value;
			}
		}

		// Token: 0x170004CD RID: 1229
		// (get) Token: 0x0600295D RID: 10589 RVA: 0x000CB890 File Offset: 0x000C9A90
		// (set) Token: 0x0600295E RID: 10590 RVA: 0x000CB898 File Offset: 0x000C9A98
		public GTZone StartZone
		{
			get
			{
				return this.startZone;
			}
			set
			{
				this.startZone = value;
			}
		}

		// Token: 0x170004CE RID: 1230
		// (get) Token: 0x0600295F RID: 10591 RVA: 0x000CB8A1 File Offset: 0x000C9AA1
		public GTZone CurrentRoomZone
		{
			get
			{
				if (!(this.currentJoinTrigger != null))
				{
					return GTZone.none;
				}
				return this.currentJoinTrigger.zone;
			}
		}

		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x06002960 RID: 10592 RVA: 0x000CB8BF File Offset: 0x000C9ABF
		// (set) Token: 0x06002961 RID: 10593 RVA: 0x000CB8C7 File Offset: 0x000C9AC7
		public GorillaGeoHideShowTrigger StartGeoTrigger
		{
			get
			{
				return this.startGeoTrigger;
			}
			set
			{
				this.startGeoTrigger = value;
			}
		}

		// Token: 0x06002962 RID: 10594 RVA: 0x000CB8D0 File Offset: 0x000C9AD0
		public void Awake()
		{
			if (PhotonNetworkController.Instance == null)
			{
				PhotonNetworkController.Instance = this;
			}
			else if (PhotonNetworkController.Instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			this.updatedName = false;
			this.playersInRegion = new int[this.serverRegions.Length];
			this.pingInRegion = new int[this.serverRegions.Length];
		}

		// Token: 0x06002963 RID: 10595 RVA: 0x000CB940 File Offset: 0x000C9B40
		public void Start()
		{
			base.StartCoroutine(this.DisableOnStart());
			NetworkSystem.Instance.OnMultiplayerStarted += this.OnJoinedRoom;
			NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnDisconnected;
			PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance = true;
		}

		// Token: 0x06002964 RID: 10596 RVA: 0x000CB996 File Offset: 0x000C9B96
		private IEnumerator DisableOnStart()
		{
			ZoneManagement.SetActiveZone(this.StartZone);
			yield break;
		}

		// Token: 0x06002965 RID: 10597 RVA: 0x000CB9A8 File Offset: 0x000C9BA8
		public void FixedUpdate()
		{
			this.headRightHandDistance = (Player.Instance.headCollider.transform.position - Player.Instance.rightControllerTransform.position).magnitude;
			this.headLeftHandDistance = (Player.Instance.headCollider.transform.position - Player.Instance.leftControllerTransform.position).magnitude;
			this.headQuat = Player.Instance.headCollider.transform.rotation;
			if (!this.disableAFKKick && Quaternion.Angle(this.headQuat, this.lastHeadQuat) <= 0.01f && Mathf.Abs(this.headRightHandDistance - this.lastHeadRightHandDistance) < 0.001f && Mathf.Abs(this.headLeftHandDistance - this.lastHeadLeftHandDistance) < 0.001f && this.pauseTime + this.disconnectTime < Time.realtimeSinceStartup)
			{
				this.pauseTime = Time.realtimeSinceStartup;
				NetworkSystem.Instance.ReturnToSinglePlayer();
			}
			else if (Quaternion.Angle(this.headQuat, this.lastHeadQuat) > 0.01f || Mathf.Abs(this.headRightHandDistance - this.lastHeadRightHandDistance) >= 0.001f || Mathf.Abs(this.headLeftHandDistance - this.lastHeadLeftHandDistance) >= 0.001f)
			{
				this.pauseTime = Time.realtimeSinceStartup;
			}
			this.lastHeadRightHandDistance = this.headRightHandDistance;
			this.lastHeadLeftHandDistance = this.headLeftHandDistance;
			this.lastHeadQuat = this.headQuat;
			if (this.deferredJoin && Time.time >= this.partyJoinDeferredUntilTimestamp)
			{
				if ((this.partyJoinDeferredUntilTimestamp != 0f || NetworkSystem.Instance.netState == NetSystemState.Idle) && this.currentJoinTrigger != null)
				{
					this.deferredJoin = false;
					this.partyJoinDeferredUntilTimestamp = 0f;
					if (this.currentJoinTrigger == this.privateTrigger)
					{
						this.AttemptToJoinSpecificRoom(this.customRoomID, FriendshipGroupDetection.Instance.IsInParty ? JoinType.JoinWithParty : JoinType.Solo);
						return;
					}
					this.AttemptToJoinPublicRoom(this.currentJoinTrigger, this.currentJoinType);
					return;
				}
				else if (NetworkSystem.Instance.netState != NetSystemState.PingRecon && NetworkSystem.Instance.netState != NetSystemState.Initialization)
				{
					this.deferredJoin = false;
					this.partyJoinDeferredUntilTimestamp = 0f;
				}
			}
		}

		// Token: 0x06002966 RID: 10598 RVA: 0x000CBBF5 File Offset: 0x000C9DF5
		public void DeferJoining(float duration)
		{
			this.partyJoinDeferredUntilTimestamp = Mathf.Max(this.partyJoinDeferredUntilTimestamp, Time.time + duration);
		}

		// Token: 0x06002967 RID: 10599 RVA: 0x000CBC0F File Offset: 0x000C9E0F
		public void ClearDeferredJoin()
		{
			this.partyJoinDeferredUntilTimestamp = 0f;
			this.deferredJoin = false;
		}

		// Token: 0x06002968 RID: 10600 RVA: 0x000CBC23 File Offset: 0x000C9E23
		public void AttemptToJoinPublicRoom(GorillaNetworkJoinTrigger triggeredTrigger, JoinType roomJoinType = JoinType.Solo)
		{
			this.AttemptToJoinPublicRoomAsync(triggeredTrigger, roomJoinType);
		}

		// Token: 0x06002969 RID: 10601 RVA: 0x000CBC30 File Offset: 0x000C9E30
		private void AttemptToJoinPublicRoomAsync(GorillaNetworkJoinTrigger triggeredTrigger, JoinType roomJoinType)
		{
			PhotonNetworkController.<AttemptToJoinPublicRoomAsync>d__65 <AttemptToJoinPublicRoomAsync>d__;
			<AttemptToJoinPublicRoomAsync>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<AttemptToJoinPublicRoomAsync>d__.<>4__this = this;
			<AttemptToJoinPublicRoomAsync>d__.triggeredTrigger = triggeredTrigger;
			<AttemptToJoinPublicRoomAsync>d__.roomJoinType = roomJoinType;
			<AttemptToJoinPublicRoomAsync>d__.<>1__state = -1;
			<AttemptToJoinPublicRoomAsync>d__.<>t__builder.Start<PhotonNetworkController.<AttemptToJoinPublicRoomAsync>d__65>(ref <AttemptToJoinPublicRoomAsync>d__);
		}

		// Token: 0x0600296A RID: 10602 RVA: 0x000CBC78 File Offset: 0x000C9E78
		private Task SendPartyFollowCommands()
		{
			PhotonNetworkController.<SendPartyFollowCommands>d__66 <SendPartyFollowCommands>d__;
			<SendPartyFollowCommands>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<SendPartyFollowCommands>d__.<>1__state = -1;
			<SendPartyFollowCommands>d__.<>t__builder.Start<PhotonNetworkController.<SendPartyFollowCommands>d__66>(ref <SendPartyFollowCommands>d__);
			return <SendPartyFollowCommands>d__.<>t__builder.Task;
		}

		// Token: 0x0600296B RID: 10603 RVA: 0x000CBCB3 File Offset: 0x000C9EB3
		public void AttemptToJoinSpecificRoom(string roomID, JoinType roomJoinType)
		{
			this.AttemptToJoinSpecificRoomAsync(roomID, roomJoinType);
		}

		// Token: 0x0600296C RID: 10604 RVA: 0x000CBCC0 File Offset: 0x000C9EC0
		public Task AttemptToJoinSpecificRoomAsync(string roomID, JoinType roomJoinType)
		{
			PhotonNetworkController.<AttemptToJoinSpecificRoomAsync>d__68 <AttemptToJoinSpecificRoomAsync>d__;
			<AttemptToJoinSpecificRoomAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<AttemptToJoinSpecificRoomAsync>d__.<>4__this = this;
			<AttemptToJoinSpecificRoomAsync>d__.roomID = roomID;
			<AttemptToJoinSpecificRoomAsync>d__.roomJoinType = roomJoinType;
			<AttemptToJoinSpecificRoomAsync>d__.<>1__state = -1;
			<AttemptToJoinSpecificRoomAsync>d__.<>t__builder.Start<PhotonNetworkController.<AttemptToJoinSpecificRoomAsync>d__68>(ref <AttemptToJoinSpecificRoomAsync>d__);
			return <AttemptToJoinSpecificRoomAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600296D RID: 10605 RVA: 0x000CBD14 File Offset: 0x000C9F14
		private void DisconnectCleanup()
		{
			if (ApplicationQuittingState.IsQuitting)
			{
				return;
			}
			if (GorillaParent.instance != null)
			{
				GorillaScoreboardSpawner[] componentsInChildren = GorillaParent.instance.GetComponentsInChildren<GorillaScoreboardSpawner>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].OnLeftRoom();
				}
			}
			this.attemptingToConnect = true;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.offlineVRRig)
			{
				if (skinnedMeshRenderer != null)
				{
					skinnedMeshRenderer.enabled = true;
				}
			}
			if (GorillaComputer.instance != null && !ApplicationQuittingState.IsQuitting)
			{
				this.UpdateTriggerScreens();
			}
			Player.Instance.maxJumpSpeed = 6.5f;
			Player.Instance.jumpMultiplier = 1.1f;
			GorillaNot.instance.currentMasterClient = null;
			GorillaTagger.Instance.offlineVRRig.huntComputer.SetActive(false);
			this.initialGameMode = "";
		}

		// Token: 0x0600296E RID: 10606 RVA: 0x000CBDF4 File Offset: 0x000C9FF4
		public void OnJoinedRoom()
		{
			if (NetworkSystem.Instance.GameModeString.IsNullOrEmpty())
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
			}
			this.initialGameMode = NetworkSystem.Instance.GameModeString;
			if (NetworkSystem.Instance.SessionIsPrivate)
			{
				this.currentJoinTrigger = this.privateTrigger;
				PhotonNetworkController.Instance.UpdateTriggerScreens();
			}
			else if (this.currentJoinType != JoinType.FollowingParty)
			{
				bool flag = false;
				for (int i = 0; i < GorillaComputer.instance.allowedMapsToJoin.Length; i++)
				{
					if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.allowedMapsToJoin[i]))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					GorillaComputer.instance.roomNotAllowed = true;
					NetworkSystem.Instance.ReturnToSinglePlayer();
					return;
				}
			}
			if (NetworkSystem.Instance.IsMasterClient)
			{
				GorillaGameModes.GameMode.LoadGameModeFromProperty(this.initialGameMode);
			}
			NetworkSystem.Instance.SetMyTutorialComplete();
			GameObject gameObject;
			VRRigCache.Instance.GetComponent<PhotonPrefabPool>().networkPrefabs.TryGetValue("Player Network Controller", out gameObject);
			if (gameObject == null)
			{
				Debug.LogError("OnJoinedRoom: Unable to find player prefab to spawn");
				return;
			}
			NetworkSystem.Instance.NetInstantiate(gameObject, this.playerOffset.transform.position, this.playerOffset.transform.rotation, false);
			GorillaComputer.instance.roomFull = false;
			GorillaComputer.instance.roomNotAllowed = false;
			if (this.currentJoinType == JoinType.JoinWithParty || this.currentJoinType == JoinType.JoinWithNearby || this.currentJoinType == JoinType.ForceJoinWithParty)
			{
				this.keyToFollow = NetworkSystem.Instance.LocalPlayer.UserId + this.keyStr;
				NetworkSystem.Instance.BroadcastMyRoom(true, this.keyToFollow, this.shuffler);
			}
			GorillaNot.instance.currentMasterClient = null;
			this.UpdateCurrentJoinTrigger();
			this.UpdateTriggerScreens();
			GorillaScoreboardTotalUpdater.instance.JoinedRoom();
		}

		// Token: 0x0600296F RID: 10607 RVA: 0x000CBFC4 File Offset: 0x000CA1C4
		public void RegisterJoinTrigger(GorillaNetworkJoinTrigger trigger)
		{
			this.allJoinTriggers.Add(trigger);
		}

		// Token: 0x06002970 RID: 10608 RVA: 0x000CBFD4 File Offset: 0x000CA1D4
		private void UpdateCurrentJoinTrigger()
		{
			if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.forestMapTrigger.gameModeName))
			{
				this.currentJoinTrigger = GorillaComputer.instance.forestMapTrigger;
				return;
			}
			if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.caveMapTrigger.gameModeName))
			{
				this.currentJoinTrigger = GorillaComputer.instance.caveMapTrigger;
				return;
			}
			if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.canyonMapTrigger.gameModeName))
			{
				this.currentJoinTrigger = GorillaComputer.instance.canyonMapTrigger;
				return;
			}
			if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.cityMapTrigger.gameModeName))
			{
				this.currentJoinTrigger = GorillaComputer.instance.cityMapTrigger;
				return;
			}
			if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.mountainMapTrigger.gameModeName))
			{
				this.currentJoinTrigger = GorillaComputer.instance.mountainMapTrigger;
				return;
			}
			if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.skyjungleMapTrigger.gameModeName))
			{
				this.currentJoinTrigger = GorillaComputer.instance.skyjungleMapTrigger;
				return;
			}
			if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.basementMapTrigger.gameModeName))
			{
				this.currentJoinTrigger = GorillaComputer.instance.basementMapTrigger;
				return;
			}
			if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.beachMapTrigger.gameModeName))
			{
				this.currentJoinTrigger = GorillaComputer.instance.beachMapTrigger;
				return;
			}
			if (NetworkSystem.Instance.GameModeString.Contains(GorillaComputer.instance.rotatingMapTrigger.gameModeName))
			{
				this.currentJoinTrigger = GorillaComputer.instance.rotatingMapTrigger;
				return;
			}
			if (NetworkSystem.Instance.SessionIsPrivate)
			{
				if (this.currentJoinTrigger != this.privateTrigger)
				{
					Debug.LogError("IN a private game but private trigger isnt current");
					return;
				}
			}
			else
			{
				Debug.LogError("Not in private room and unabel tp update jointrigger.");
			}
		}

		// Token: 0x06002971 RID: 10609 RVA: 0x000CC1F4 File Offset: 0x000CA3F4
		public void UpdateTriggerScreens()
		{
			foreach (GorillaNetworkJoinTrigger gorillaNetworkJoinTrigger in this.allJoinTriggers)
			{
				gorillaNetworkJoinTrigger.UpdateUI();
			}
		}

		// Token: 0x06002972 RID: 10610 RVA: 0x000CC244 File Offset: 0x000CA444
		public void AttemptToFollowIntoPub(string userIDToFollow, int actorNumberToFollow, string newKeyStr, string shufflerStr, JoinType joinType)
		{
			this.friendToFollow = userIDToFollow;
			this.keyToFollow = userIDToFollow + newKeyStr;
			this.shuffler = shufflerStr;
			this.currentJoinType = joinType;
			this.ClearDeferredJoin();
			if (NetworkSystem.Instance.InRoom)
			{
				NetworkSystem.Instance.JoinFriendsRoom(this.friendToFollow, actorNumberToFollow, this.keyToFollow, this.shuffler);
			}
		}

		// Token: 0x06002973 RID: 10611 RVA: 0x000CC2A5 File Offset: 0x000CA4A5
		public void OnDisconnected()
		{
			this.DisconnectCleanup();
		}

		// Token: 0x06002974 RID: 10612 RVA: 0x000CC2AD File Offset: 0x000CA4AD
		public void OnApplicationQuit()
		{
			if (PhotonNetwork.IsConnected)
			{
				PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion != "dev";
			}
		}

		// Token: 0x06002975 RID: 10613 RVA: 0x000CC2D0 File Offset: 0x000CA4D0
		private string ReturnRoomName()
		{
			if (this.isPrivate)
			{
				return this.customRoomID;
			}
			return this.RandomRoomName();
		}

		// Token: 0x06002976 RID: 10614 RVA: 0x000CC2E8 File Offset: 0x000CA4E8
		private string RandomRoomName()
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
			return this.RandomRoomName();
		}

		// Token: 0x06002977 RID: 10615 RVA: 0x000CC340 File Offset: 0x000CA540
		public byte GetRoomSize(string gameModeName)
		{
			if (gameModeName.Contains("ball"))
			{
				return 5;
			}
			return 10;
		}

		// Token: 0x06002978 RID: 10616 RVA: 0x000CC354 File Offset: 0x000CA554
		private string GetRegionWithLowestPing()
		{
			int num = 10000;
			int num2 = 0;
			for (int i = 0; i < this.serverRegions.Length; i++)
			{
				Debug.Log("ping in region " + this.serverRegions[i] + " is " + this.pingInRegion[i].ToString());
				if (this.pingInRegion[i] < num && this.pingInRegion[i] > 0)
				{
					num = this.pingInRegion[i];
					num2 = i;
				}
			}
			return this.serverRegions[num2];
		}

		// Token: 0x06002979 RID: 10617 RVA: 0x000CC3D4 File Offset: 0x000CA5D4
		public int TotalUsers()
		{
			int num = 0;
			foreach (int num2 in this.playersInRegion)
			{
				num += num2;
			}
			return num;
		}

		// Token: 0x0600297A RID: 10618 RVA: 0x000CC404 File Offset: 0x000CA604
		public string CurrentState()
		{
			if (NetworkSystem.Instance == null)
			{
				Debug.Log("Null netsys!!!");
			}
			return NetworkSystem.Instance.netState.ToString();
		}

		// Token: 0x0600297B RID: 10619 RVA: 0x000CC440 File Offset: 0x000CA640
		private void OnApplicationPause(bool pause)
		{
			if (pause)
			{
				this.timeWhenApplicationPaused = new DateTime?(DateTime.Now);
				return;
			}
			if ((DateTime.Now - (this.timeWhenApplicationPaused ?? DateTime.Now)).TotalSeconds > (double)this.disconnectTime)
			{
				this.timeWhenApplicationPaused = null;
				NetworkSystem instance = NetworkSystem.Instance;
				if (instance != null)
				{
					instance.ReturnToSinglePlayer();
				}
			}
			if (NetworkSystem.Instance != null && !NetworkSystem.Instance.InRoom && NetworkSystem.Instance.netState == NetSystemState.InGame)
			{
				NetworkSystem instance2 = NetworkSystem.Instance;
				if (instance2 == null)
				{
					return;
				}
				instance2.ReturnToSinglePlayer();
			}
		}

		// Token: 0x0600297C RID: 10620 RVA: 0x000CC4ED File Offset: 0x000CA6ED
		private void OnApplicationFocus(bool focus)
		{
			if (!focus && NetworkSystem.Instance != null && !NetworkSystem.Instance.InRoom && NetworkSystem.Instance.netState == NetSystemState.InGame)
			{
				NetworkSystem instance = NetworkSystem.Instance;
				if (instance == null)
				{
					return;
				}
				instance.ReturnToSinglePlayer();
			}
		}

		// Token: 0x04002C6D RID: 11373
		public static volatile PhotonNetworkController Instance;

		// Token: 0x04002C6E RID: 11374
		public int incrementCounter;

		// Token: 0x04002C6F RID: 11375
		public PlayFabAuthenticator playFabAuthenticator;

		// Token: 0x04002C70 RID: 11376
		public string[] serverRegions;

		// Token: 0x04002C71 RID: 11377
		public bool isPrivate;

		// Token: 0x04002C72 RID: 11378
		public string customRoomID;

		// Token: 0x04002C73 RID: 11379
		public GameObject playerOffset;

		// Token: 0x04002C74 RID: 11380
		public SkinnedMeshRenderer[] offlineVRRig;

		// Token: 0x04002C75 RID: 11381
		public bool attemptingToConnect;

		// Token: 0x04002C76 RID: 11382
		private int currentRegionIndex;

		// Token: 0x04002C77 RID: 11383
		public string currentGameType;

		// Token: 0x04002C78 RID: 11384
		public bool roomCosmeticsInitialized;

		// Token: 0x04002C79 RID: 11385
		public GameObject photonVoiceObjectPrefab;

		// Token: 0x04002C7A RID: 11386
		public Dictionary<string, bool> playerCosmeticsLookup = new Dictionary<string, bool>();

		// Token: 0x04002C7B RID: 11387
		private bool pastFirstConnection;

		// Token: 0x04002C7C RID: 11388
		private float lastHeadRightHandDistance;

		// Token: 0x04002C7D RID: 11389
		private float lastHeadLeftHandDistance;

		// Token: 0x04002C7E RID: 11390
		private float pauseTime;

		// Token: 0x04002C7F RID: 11391
		private float disconnectTime = 120f;

		// Token: 0x04002C80 RID: 11392
		public bool disableAFKKick;

		// Token: 0x04002C81 RID: 11393
		private float headRightHandDistance;

		// Token: 0x04002C82 RID: 11394
		private float headLeftHandDistance;

		// Token: 0x04002C83 RID: 11395
		private Quaternion headQuat;

		// Token: 0x04002C84 RID: 11396
		private Quaternion lastHeadQuat;

		// Token: 0x04002C85 RID: 11397
		public GameObject[] disableOnStartup;

		// Token: 0x04002C86 RID: 11398
		public GameObject[] enableOnStartup;

		// Token: 0x04002C87 RID: 11399
		public bool updatedName;

		// Token: 0x04002C88 RID: 11400
		private int[] playersInRegion;

		// Token: 0x04002C89 RID: 11401
		private int[] pingInRegion;

		// Token: 0x04002C8A RID: 11402
		public List<string> friendIDList = new List<string>();

		// Token: 0x04002C8B RID: 11403
		private JoinType currentJoinType;

		// Token: 0x04002C8C RID: 11404
		private string friendToFollow;

		// Token: 0x04002C8D RID: 11405
		private string keyToFollow;

		// Token: 0x04002C8E RID: 11406
		public string shuffler;

		// Token: 0x04002C8F RID: 11407
		public string keyStr;

		// Token: 0x04002C90 RID: 11408
		private string platformTag = "OTHER";

		// Token: 0x04002C91 RID: 11409
		private string startLevel;

		// Token: 0x04002C92 RID: 11410
		[SerializeField]
		private GTZone startZone;

		// Token: 0x04002C93 RID: 11411
		private GorillaGeoHideShowTrigger startGeoTrigger;

		// Token: 0x04002C94 RID: 11412
		public GorillaNetworkJoinTrigger privateTrigger;

		// Token: 0x04002C95 RID: 11413
		internal string initialGameMode = "";

		// Token: 0x04002C96 RID: 11414
		public GorillaNetworkJoinTrigger currentJoinTrigger;

		// Token: 0x04002C97 RID: 11415
		public string autoJoinRoom;

		// Token: 0x04002C98 RID: 11416
		private bool deferredJoin;

		// Token: 0x04002C99 RID: 11417
		private float partyJoinDeferredUntilTimestamp;

		// Token: 0x04002C9A RID: 11418
		private DateTime? timeWhenApplicationPaused;

		// Token: 0x04002C9B RID: 11419
		[NetworkPrefab]
		[SerializeField]
		private NetworkObject testPlayerPrefab;

		// Token: 0x04002C9C RID: 11420
		private List<GorillaNetworkJoinTrigger> allJoinTriggers = new List<GorillaNetworkJoinTrigger>();
	}
}
