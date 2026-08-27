using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

// Token: 0x02000414 RID: 1044
[DefaultExecutionOrder(0)]
public class VRRigJobManager : MonoBehaviour
{
	// Token: 0x170002F1 RID: 753
	// (get) Token: 0x06001849 RID: 6217 RVA: 0x0007C8C5 File Offset: 0x0007AAC5
	public static VRRigJobManager Instance
	{
		get
		{
			return VRRigJobManager._instance;
		}
	}

	// Token: 0x0600184A RID: 6218 RVA: 0x0007C8CC File Offset: 0x0007AACC
	private void Awake()
	{
		VRRigJobManager._instance = this;
		this.cachedInput = new NativeArray<VRRigJobManager.VRRigTransformInput>(9, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		this.tAA = new TransformAccessArray(9, 2);
	}

	// Token: 0x0600184B RID: 6219 RVA: 0x0007C8F1 File Offset: 0x0007AAF1
	private void OnDestroy()
	{
		this.jobHandle.Complete();
		this.cachedInput.Dispose();
		this.tAA.Dispose();
	}

	// Token: 0x0600184C RID: 6220 RVA: 0x0007C914 File Offset: 0x0007AB14
	public void RegisterVRRig(VRRig rig)
	{
		this.rigList.Add(rig);
		this.tAA.Add(rig.transform);
		this.actualListSz++;
	}

	// Token: 0x0600184D RID: 6221 RVA: 0x0007C944 File Offset: 0x0007AB44
	public void DeregisterVRRig(VRRig rig)
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.rigList.Remove(rig);
		for (int i = this.actualListSz - 1; i >= 0; i--)
		{
			if (this.tAA[i] == rig.transform)
			{
				this.tAA.RemoveAtSwapBack(i);
				break;
			}
		}
		this.actualListSz--;
	}

	// Token: 0x0600184E RID: 6222 RVA: 0x0007C9B0 File Offset: 0x0007ABB0
	private void CopyInput()
	{
		for (int i = 0; i < this.actualListSz; i++)
		{
			this.cachedInput[i] = new VRRigJobManager.VRRigTransformInput
			{
				rigPosition = this.rigList[i].jobPos,
				rigRotaton = this.rigList[i].jobRotation
			};
			this.tAA[i] = this.rigList[i].transform;
		}
	}

	// Token: 0x0600184F RID: 6223 RVA: 0x0007CA30 File Offset: 0x0007AC30
	public void Update()
	{
		this.jobHandle.Complete();
		for (int i = 0; i < this.rigList.Count; i++)
		{
			this.rigList[i].RemoteRigUpdate();
		}
		this.CopyInput();
		VRRigJobManager.VRRigTransformJob jobData = new VRRigJobManager.VRRigTransformJob
		{
			input = this.cachedInput
		};
		this.jobHandle = jobData.Schedule(this.tAA, default(JobHandle));
	}

	// Token: 0x04001C06 RID: 7174
	[OnEnterPlay_SetNull]
	private static VRRigJobManager _instance;

	// Token: 0x04001C07 RID: 7175
	private const int MaxSize = 9;

	// Token: 0x04001C08 RID: 7176
	private const int questJobThreads = 2;

	// Token: 0x04001C09 RID: 7177
	private List<VRRig> rigList = new List<VRRig>(9);

	// Token: 0x04001C0A RID: 7178
	private NativeArray<VRRigJobManager.VRRigTransformInput> cachedInput;

	// Token: 0x04001C0B RID: 7179
	private TransformAccessArray tAA;

	// Token: 0x04001C0C RID: 7180
	private int actualListSz;

	// Token: 0x04001C0D RID: 7181
	private JobHandle jobHandle;

	// Token: 0x02000415 RID: 1045
	private struct VRRigTransformInput
	{
		// Token: 0x04001C0E RID: 7182
		public Vector3 rigPosition;

		// Token: 0x04001C0F RID: 7183
		public Quaternion rigRotaton;
	}

	// Token: 0x02000416 RID: 1046
	[BurstCompile]
	private struct VRRigTransformJob : IJobParallelForTransform
	{
		// Token: 0x06001851 RID: 6225 RVA: 0x0007CABC File Offset: 0x0007ACBC
		public void Execute(int i, TransformAccess tA)
		{
			if (i < this.input.Length)
			{
				tA.position = this.input[i].rigPosition;
				tA.rotation = this.input[i].rigRotaton;
			}
		}

		// Token: 0x04001C10 RID: 7184
		[ReadOnly]
		public NativeArray<VRRigJobManager.VRRigTransformInput> input;
	}
}
