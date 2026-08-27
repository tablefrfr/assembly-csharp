using System;
using Fusion;
using GorillaExtensions;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Fusion;
using Photon.Voice.PUN;
using UnityEngine;
using UnityEngine.Scripting;

// Token: 0x0200035B RID: 859
[NetworkBehaviourWeaved(35)]
internal class VRRigSerializer : GorillaWrappedSerializer<InputStruct>, IFXContextParems<HandTapArgs>, IFXContextParems<GeoSoundArg>
{
	// Token: 0x17000205 RID: 517
	// (get) Token: 0x06001361 RID: 4961 RVA: 0x000662FC File Offset: 0x000644FC
	// (set) Token: 0x06001362 RID: 4962 RVA: 0x00066326 File Offset: 0x00064526
	[Networked]
	[NetworkedWeaved(0, 17)]
	public unsafe NetworkString<_16> nickName
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing VRRigSerializer.nickName. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(NetworkString<_16>*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing VRRigSerializer.nickName. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(NetworkString<_16>*)(this.Ptr + 0) = value;
		}
	}

	// Token: 0x17000206 RID: 518
	// (get) Token: 0x06001363 RID: 4963 RVA: 0x00066351 File Offset: 0x00064551
	// (set) Token: 0x06001364 RID: 4964 RVA: 0x0006637F File Offset: 0x0006457F
	[Networked]
	[NetworkedWeaved(17, 17)]
	public unsafe NetworkString<_16> defaultName
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing VRRigSerializer.defaultName. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(NetworkString<_16>*)(this.Ptr + 17);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing VRRigSerializer.defaultName. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(NetworkString<_16>*)(this.Ptr + 17) = value;
		}
	}

	// Token: 0x17000207 RID: 519
	// (get) Token: 0x06001365 RID: 4965 RVA: 0x000663AE File Offset: 0x000645AE
	// (set) Token: 0x06001366 RID: 4966 RVA: 0x000663DC File Offset: 0x000645DC
	[Networked]
	[NetworkedWeaved(34, 1)]
	public bool tutorialComplete
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing VRRigSerializer.tutorialComplete. Networked properties can only be accessed when Spawned() has been called.");
			}
			return ReadWriteUtilsForWeaver.ReadBoolean(this.Ptr + 34);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing VRRigSerializer.tutorialComplete. Networked properties can only be accessed when Spawned() has been called.");
			}
			ReadWriteUtilsForWeaver.WriteBoolean(this.Ptr + 34, value);
		}
	}

	// Token: 0x17000208 RID: 520
	// (get) Token: 0x06001367 RID: 4967 RVA: 0x0006640B File Offset: 0x0006460B
	public FXSystemSettings settings
	{
		get
		{
			return this.vrrig.fxSettings;
		}
	}

	// Token: 0x06001368 RID: 4968 RVA: 0x00066418 File Offset: 0x00064618
	protected override bool OnSpawnSetupCheck(PhotonMessageInfoWrapped wrappedInfo, out GameObject outTargetObject, out Type outTargetType)
	{
		outTargetObject = null;
		outTargetType = null;
		NetPlayer player = NetworkSystem.Instance.GetPlayer(wrappedInfo.senderID);
		if (this.photonView.IsRoomView)
		{
			if (player != null)
			{
				GorillaNot.instance.SendReport("creating rigs as room objects", player.UserId, player.NickName);
			}
			return false;
		}
		if (NetworkSystem.Instance.IsObjectRoomObject(base.gameObject))
		{
			NetPlayer player2 = NetworkSystem.Instance.GetPlayer(wrappedInfo.senderID);
			if (player2 != null)
			{
				Debug.LogWarning("creating rigs as room objects " + player2.UserId + " " + player2.NickName);
				GorillaNot.instance.SendReport("creating rigs as room objects", player2.UserId, player2.NickName);
			}
			return false;
		}
		if (((PunNetPlayer)player).playerRef != this.photonView.Owner)
		{
			GorillaNot.instance.SendReport("creating rigs for someone else", player.UserId, player.NickName);
			return false;
		}
		if (VRRigCache.Instance.TryGetVrrig(player, out this.rigContainer))
		{
			outTargetObject = this.rigContainer.gameObject;
			outTargetType = typeof(VRRig);
			this.vrrig = this.rigContainer.Rig;
			return true;
		}
		return false;
	}

	// Token: 0x06001369 RID: 4969 RVA: 0x00066548 File Offset: 0x00064748
	protected override void OnSuccesfullySpawned(PhotonMessageInfoWrapped info)
	{
		this.rigContainer.InitializeNetwork(this.photonView, this.voiceView, this);
		this.networkSpeaker.SetParent(this.rigContainer.SpeakerHead, false);
		base.transform.SetParent(VRRigCache.Instance.NetworkParent, true);
		this.photonView.AddCallbackTarget(this);
		NetworkSystem.Instance.IsObjectLocallyOwned(base.gameObject);
	}

	// Token: 0x0600136A RID: 4970 RVA: 0x00003051 File Offset: 0x00001251
	protected override void OnFailedSpawn()
	{
	}

	// Token: 0x0600136B RID: 4971 RVA: 0x000665B7 File Offset: 0x000647B7
	protected override void OnBeforeDespawn()
	{
		this.CleanUp(true);
	}

	// Token: 0x0600136C RID: 4972 RVA: 0x000665C0 File Offset: 0x000647C0
	private void CleanUp(bool netDestroy)
	{
		if (!this.successfullInstantiate)
		{
			return;
		}
		this.successfullInstantiate = false;
		if (this.vrrig != null)
		{
			if (!NetworkSystem.Instance.InRoom)
			{
				if (this.vrrig.isOfflineVRRig)
				{
					this.vrrig.ChangeMaterialLocal(0);
				}
			}
			else
			{
				if (this.vrrig.isOfflineVRRig)
				{
					NetworkSystem.Instance.NetDestroy(base.gameObject);
				}
				if (this.vrrig.photonView == this.photonView)
				{
					this.vrrig.photonView = null;
				}
				if (this.vrrig.rigSerializer == this)
				{
					this.vrrig.rigSerializer = null;
				}
			}
		}
		if (this.networkSpeaker != null)
		{
			if (netDestroy)
			{
				this.networkSpeaker.SetParent(base.transform, false);
			}
			else
			{
				this.networkSpeaker.SetParent(null);
			}
			this.networkSpeaker.gameObject.SetActive(false);
		}
		this.vrrig = null;
	}

	// Token: 0x0600136D RID: 4973 RVA: 0x000666B1 File Offset: 0x000648B1
	private void OnDisable()
	{
		this.CleanUp(false);
	}

	// Token: 0x0600136E RID: 4974 RVA: 0x000666BA File Offset: 0x000648BA
	private void OnDestroy()
	{
		if (this.networkSpeaker != null && this.networkSpeaker.parent != base.transform)
		{
			UnityEngine.Object.Destroy(this.networkSpeaker.gameObject);
		}
	}

	// Token: 0x0600136F RID: 4975 RVA: 0x000666F4 File Offset: 0x000648F4
	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	public unsafe void RPC_InitializeNoobMaterial(float red, float green, float blue, RpcInfo info = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (this.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = this.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 1) == 0)
				{
					NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void VRRigSerializer::RPC_InitializeNoobMaterial(System.Single,System.Single,System.Single,Fusion.RpcInfo)", this.Object, 1);
				}
				else
				{
					if (this.Runner.HasAnyActiveConnections())
					{
						int num = 8;
						num += 4;
						num += 4;
						num += 4;
						SimulationMessage* ptr = SimulationMessage.Allocate(this.Runner.Simulation, num);
						byte* data = SimulationMessage.GetData(ptr);
						int num2 = RpcHeader.Write(RpcHeader.Create(this.Object.Id, this.ObjectIndex, 1), data);
						ReadWriteUtilsForWeaver.WriteFloat((int*)(data + num2), 999.99994f, red);
						num2 += 4;
						ReadWriteUtilsForWeaver.WriteFloat((int*)(data + num2), 999.99994f, green);
						num2 += 4;
						ReadWriteUtilsForWeaver.WriteFloat((int*)(data + num2), 999.99994f, blue);
						num2 += 4;
						ptr->Offset = num2 * 8;
						this.Runner.SendRpc(ptr);
					}
					if ((localAuthorityMask & 7) != 0)
					{
						info = RpcInfo.FromLocal(this.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
						goto IL_12;
					}
				}
			}
			return;
		}
		this.InvokeRpc = false;
		IL_12:
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.InitializeNoobMaterial(red, green, blue, new PhotonMessageInfoWrapped(info));
	}

	// Token: 0x06001370 RID: 4976 RVA: 0x000668A0 File Offset: 0x00064AA0
	[PunRPC]
	public void InitializeNoobMaterial(float red, float green, float blue, PhotonMessageInfo info)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.InitializeNoobMaterial(red, green, blue, new PhotonMessageInfoWrapped(info));
	}

	// Token: 0x06001371 RID: 4977 RVA: 0x000668BC File Offset: 0x00064ABC
	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public unsafe void RPC_RequestMaterialColor(int askingPlayerID, RpcInfo info = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (this.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = this.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 7) != 0)
				{
					if ((localAuthorityMask & 1) != 1)
					{
						if (this.Runner.HasAnyActiveConnections())
						{
							int num = 8;
							num += 4;
							SimulationMessage* ptr = SimulationMessage.Allocate(this.Runner.Simulation, num);
							byte* data = SimulationMessage.GetData(ptr);
							int num2 = RpcHeader.Write(RpcHeader.Create(this.Object.Id, this.ObjectIndex, 2), data);
							*(int*)(data + num2) = askingPlayerID;
							num2 += 4;
							ptr->Offset = num2 * 8;
							this.Runner.SendRpc(ptr);
						}
						if ((localAuthorityMask & 1) == 0)
						{
							return;
						}
					}
					info = RpcInfo.FromLocal(this.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
					goto IL_12;
				}
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void VRRigSerializer::RPC_RequestMaterialColor(System.Int32,Fusion.RpcInfo)", this.Object, 7);
			}
			return;
		}
		this.InvokeRpc = false;
		IL_12:
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.RequestMaterialColor(askingPlayerID, new PhotonMessageInfoWrapped(info));
	}

	// Token: 0x06001372 RID: 4978 RVA: 0x00066A12 File Offset: 0x00064C12
	[PunRPC]
	public void RequestMaterialColor(Player askingPlayer, PhotonMessageInfo info)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.RequestMaterialColor(askingPlayer.ActorNumber, new PhotonMessageInfoWrapped(info));
	}

	// Token: 0x06001373 RID: 4979 RVA: 0x00066A30 File Offset: 0x00064C30
	[PunRPC]
	public void RequestCosmetics(PhotonMessageInfo info)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.RequestCosmetics(info);
	}

	// Token: 0x06001374 RID: 4980 RVA: 0x00066A43 File Offset: 0x00064C43
	[PunRPC]
	public void PlayDrum(int drumIndex, float drumVolume, PhotonMessageInfo info)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.PlayDrum(drumIndex, drumVolume, info);
	}

	// Token: 0x06001375 RID: 4981 RVA: 0x00066A58 File Offset: 0x00064C58
	[PunRPC]
	public void PlaySelfOnlyInstrument(int selfOnlyIndex, int noteIndex, float instrumentVol, PhotonMessageInfo info)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.PlaySelfOnlyInstrument(selfOnlyIndex, noteIndex, instrumentVol, info);
	}

	// Token: 0x06001376 RID: 4982 RVA: 0x00066A70 File Offset: 0x00064C70
	[Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = false)]
	public unsafe void RPC_PlayHandTap(int soundIndex, bool isLeftHand, float tapVolume, RpcInfo info = default(RpcInfo))
	{
		if (this.InvokeRpc)
		{
			this.InvokeRpc = false;
			GorillaNot.IncrementRPCCall(new PhotonMessageInfoWrapped(info), "RPC_PlayHandTap");
			this.handTapArgs.soundIndex = soundIndex;
			this.handTapArgs.isLeftHand = isLeftHand;
			this.handTapArgs.tapVolume = Mathf.Max(tapVolume, 0.1f);
			FXSystem.PlayFX<HandTapArgs>(FXType.PlayHandTap, this, this.handTapArgs, default(PhotonMessageInfo));
			return;
		}
		NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
		if (this.Runner.Stage != SimulationStages.Resimulate)
		{
			int localAuthorityMask = this.Object.GetLocalAuthorityMask();
			if ((localAuthorityMask & 1) == 0)
			{
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void VRRigSerializer::RPC_PlayHandTap(System.Int32,System.Boolean,System.Single,Fusion.RpcInfo)", this.Object, 1);
			}
			else if (this.Runner.HasAnyActiveConnections())
			{
				int num = 8;
				num += 4;
				num += 4;
				num += 4;
				SimulationMessage* ptr = SimulationMessage.Allocate(this.Runner.Simulation, num);
				byte* data = SimulationMessage.GetData(ptr);
				int num2 = RpcHeader.Write(RpcHeader.Create(this.Object.Id, this.ObjectIndex, 3), data);
				*(int*)(data + num2) = soundIndex;
				num2 += 4;
				ReadWriteUtilsForWeaver.WriteBoolean((int*)(data + num2), isLeftHand);
				num2 += 4;
				ReadWriteUtilsForWeaver.WriteFloat((int*)(data + num2), 999.99994f, tapVolume);
				num2 += 4;
				ptr->Offset = num2 * 8;
				this.Runner.SendRpc(ptr);
			}
		}
	}

	// Token: 0x06001377 RID: 4983 RVA: 0x00066C28 File Offset: 0x00064E28
	[PunRPC]
	public void PlayHandTap(int soundIndex, bool isLeftHand, float tapVolume, PhotonMessageInfo info = default(PhotonMessageInfo))
	{
		if (info.Sender == this.photonView.Owner && float.IsFinite(tapVolume))
		{
			this.handTapArgs.soundIndex = soundIndex;
			this.handTapArgs.isLeftHand = isLeftHand;
			this.handTapArgs.tapVolume = Mathf.Clamp(tapVolume, 0f, 0.1f);
			FXSystem.PlayFX<HandTapArgs>(FXType.PlayHandTap, this, this.handTapArgs, info);
			return;
		}
		GorillaNot.instance.SendReport("inappropriate tag data being sent hand tap", info.Sender.UserId, info.Sender.NickName);
	}

	// Token: 0x06001378 RID: 4984 RVA: 0x00066CBD File Offset: 0x00064EBD
	void IFXContextParems<HandTapArgs>.OnPlayFX(HandTapArgs parems)
	{
		this.vrrig.PlayHandTapLocal(parems.soundIndex, parems.isLeftHand, parems.tapVolume);
	}

	// Token: 0x06001379 RID: 4985 RVA: 0x00066CDC File Offset: 0x00064EDC
	[PunRPC]
	public void UpdateCosmetics(string[] currentItems, PhotonMessageInfo info)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.UpdateCosmetics(currentItems, info);
	}

	// Token: 0x0600137A RID: 4986 RVA: 0x00066CF0 File Offset: 0x00064EF0
	[PunRPC]
	public void UpdateCosmeticsWithTryon(string[] currentItems, string[] tryOnItems, PhotonMessageInfo info)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.UpdateCosmeticsWithTryon(currentItems, tryOnItems, info);
	}

	// Token: 0x0600137B RID: 4987 RVA: 0x00066D05 File Offset: 0x00064F05
	[PunRPC]
	public void PlaySplashEffect(Vector3 splashPosition, Quaternion splashRotation, float splashScale, float boundingRadius, bool bigSplash, bool enteringWater, PhotonMessageInfo info)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.PlaySplashEffect(splashPosition, splashRotation, splashScale, boundingRadius, bigSplash, enteringWater, info);
	}

	// Token: 0x0600137C RID: 4988 RVA: 0x00066D24 File Offset: 0x00064F24
	[PunRPC]
	public void PlayGeodeEffect(Vector3 hitPosition, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "PlayGeodeEffect");
		if (info.Sender == this.photonView.Owner && hitPosition.IsValid())
		{
			this.geoSoundArg.position = hitPosition;
			FXSystem.PlayFX<GeoSoundArg>(FXType.PlayHandTap, this, this.geoSoundArg, info);
			return;
		}
		GorillaNot.instance.SendReport("inappropriate tag data being sent geode effect", info.Sender.UserId, info.Sender.NickName);
	}

	// Token: 0x0600137D RID: 4989 RVA: 0x00066D9A File Offset: 0x00064F9A
	void IFXContextParems<GeoSoundArg>.OnPlayFX(GeoSoundArg parems)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.PlayGeodeEffect(parems.position);
	}

	// Token: 0x0600137E RID: 4990 RVA: 0x00066DB2 File Offset: 0x00064FB2
	[PunRPC]
	public void EnableNonCosmeticHandItemRPC(bool enable, bool isLeftHand, PhotonMessageInfo info)
	{
		VRRig vrrig = this.vrrig;
		if (vrrig == null)
		{
			return;
		}
		vrrig.EnableNonCosmeticHandItemRPC(enable, isLeftHand, info);
	}

	// Token: 0x06001380 RID: 4992 RVA: 0x00066DE5 File Offset: 0x00064FE5
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.nickName = this._nickName;
		this.defaultName = this._defaultName;
		this.tutorialComplete = this._tutorialComplete;
	}

	// Token: 0x06001381 RID: 4993 RVA: 0x00066E15 File Offset: 0x00065015
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._nickName = this.nickName;
		this._defaultName = this.defaultName;
		this._tutorialComplete = this.tutorialComplete;
	}

	// Token: 0x06001382 RID: 4994 RVA: 0x00066E44 File Offset: 0x00065044
	[NetworkRpcWeavedInvoker(1, 1, 7)]
	[Preserve]
	protected unsafe static void RPC_InitializeNoobMaterial@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* data = SimulationMessage.GetData(message);
		int num = RpcHeader.ReadSize(data) + 3 & -4;
		float num2 = (float)(*(int*)(data + num)) * 0.001f;
		num += 4;
		float red = num2;
		float num3 = (float)(*(int*)(data + num)) * 0.001f;
		num += 4;
		float green = num3;
		float num4 = (float)(*(int*)(data + num)) * 0.001f;
		num += 4;
		float blue = num4;
		RpcInfo info = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((VRRigSerializer)behaviour).RPC_InitializeNoobMaterial(red, green, blue, info);
	}

	// Token: 0x06001383 RID: 4995 RVA: 0x00066F00 File Offset: 0x00065100
	[NetworkRpcWeavedInvoker(2, 7, 1)]
	[Preserve]
	protected unsafe static void RPC_RequestMaterialColor@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* data = SimulationMessage.GetData(message);
		int num = RpcHeader.ReadSize(data) + 3 & -4;
		int num2 = *(int*)(data + num);
		num += 4;
		int askingPlayerID = num2;
		RpcInfo info = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((VRRigSerializer)behaviour).RPC_RequestMaterialColor(askingPlayerID, info);
	}

	// Token: 0x06001384 RID: 4996 RVA: 0x00066F70 File Offset: 0x00065170
	[NetworkRpcWeavedInvoker(3, 1, 7)]
	[Preserve]
	protected unsafe static void RPC_PlayHandTap@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* data = SimulationMessage.GetData(message);
		int num = RpcHeader.ReadSize(data) + 3 & -4;
		int num2 = *(int*)(data + num);
		num += 4;
		int soundIndex = num2;
		bool flag = ReadWriteUtilsForWeaver.ReadBoolean((int*)(data + num));
		num += 4;
		bool isLeftHand = flag;
		float num3 = (float)(*(int*)(data + num)) * 0.001f;
		num += 4;
		float tapVolume = num3;
		RpcInfo info = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((VRRigSerializer)behaviour).RPC_PlayHandTap(soundIndex, isLeftHand, tapVolume, info);
	}

	// Token: 0x0400164D RID: 5709
	[SerializeField]
	[DefaultForProperty("nickName", 0, 17)]
	private NetworkString<_16> _nickName;

	// Token: 0x0400164E RID: 5710
	[SerializeField]
	[DefaultForProperty("defaultName", 17, 17)]
	private NetworkString<_16> _defaultName;

	// Token: 0x0400164F RID: 5711
	[SerializeField]
	[DefaultForProperty("tutorialComplete", 34, 1)]
	private bool _tutorialComplete;

	// Token: 0x04001650 RID: 5712
	[SerializeField]
	private PhotonVoiceView voiceView;

	// Token: 0x04001651 RID: 5713
	[SerializeField]
	private VoiceNetworkObject fusionVoiceView;

	// Token: 0x04001652 RID: 5714
	public Transform networkSpeaker;

	// Token: 0x04001653 RID: 5715
	[SerializeField]
	private VRRig vrrig;

	// Token: 0x04001654 RID: 5716
	private RigContainer rigContainer;

	// Token: 0x04001655 RID: 5717
	private HandTapArgs handTapArgs = new HandTapArgs();

	// Token: 0x04001656 RID: 5718
	private GeoSoundArg geoSoundArg = new GeoSoundArg();

	// Token: 0x04001657 RID: 5719
	new static Changed<VRRigSerializer> $IL2CPP_CHANGED;

	// Token: 0x04001658 RID: 5720
	new static ChangedDelegate<VRRigSerializer> $IL2CPP_CHANGED_DELEGATE;

	// Token: 0x04001659 RID: 5721
	new static NetworkBehaviourCallbacks<VRRigSerializer> $IL2CPP_NETWORK_BEHAVIOUR_CALLBACKS;
}
