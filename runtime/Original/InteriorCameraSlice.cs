using System;
using UnityEngine;
namespace Crisis.Runtime.Presentation;
// Original method bodies; gameplay objects are read-only views of actual renderer/PhysX state.
public sealed class InteriorCameraSlice:MonoBehaviour {
 public Camera interiorCamera; public CameraSubjectSnapshot Subject;
 public float smoothing,fovSmoothing,centerOffset,obstacleHoldOff,doorHoldOff,fieldOfViewOverride;
 public bool overrideFieldOfView,disableScrollPastDoors,useDynamicFov;
 public float cameraDistance,staticFov,dynamicFovMin,dynamicFovMax;
 float targetFOV,subjectToObstacle=4.5f; Vector3 targetPosition;RaycastHit hit;public int obstacleMask;
 public float FrustumWidth{get;private set;}
 private float StaticFieldOfView=>(!overrideFieldOfView)?staticFov:fieldOfViewOverride;
 private float MaxFieldOfView=>(!useDynamicFov)?StaticFieldOfView:dynamicFovMax;
 public void Initialize(){targetFOV=interiorCamera.fieldOfView;cameraDistance=-1.5f-transform.position.z;SetInitialPosition();}
 public void Step()=>Update();
 public object Snapshot()=>new{position=new[]{transform.position.x,transform.position.y,transform.position.z},fov=interiorCamera.fieldOfView,FrustumWidth};
 // Audio is outside this presentation slice; the original listener algorithm is not substituted.
 void UpdateListenerPosition(){}
private void Update()
	{
		UpdateTargetPosition();
		UpdateListenerPosition();
		base.transform.position = Vector3.Lerp(base.transform.position, targetPosition, smoothing * Time.unscaledDeltaTime);
		interiorCamera.fieldOfView = Mathf.Lerp(interiorCamera.fieldOfView, targetFOV, fovSmoothing * Time.unscaledDeltaTime);
		FrustumWidth = 2f * cameraDistance * Mathf.Tan(interiorCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f)) * interiorCamera.aspect;
	}
private void UpdateTargetPosition()
	{
		if (Subject == null)
		{
			return;
		}
		float num = ((!Subject.IsRagdoll) ? (FrustumWidth * centerOffset) : 0f);
		bool flag = Physics.Raycast(Subject.transform.position, Subject.transform.TransformDirection(Vector3.right), out hit, num + obstacleHoldOff, obstacleMask);
		Rigidbody component = Subject.GetComponent<Rigidbody>();
		Vector3 vector = ((!(component != null)) ? Vector3.zero : component.velocity);
		Vector3 vector2 = Subject.KinematicWorldPosition + vector * 0.25f + Subject.transform.TransformDirection(num, 0f, 0f);
		if (flag)
		{
			CameraDoorSnapshot doorController = null;
			subjectToObstacle = num;
			float num2 = 10f;
			float num3 = 10f;
			if (!disableScrollPastDoors && hit.collider != null)
			{
				subjectToObstacle = Mathf.Abs(hit.collider.transform.position.x - Subject.transform.position.x);
				Transform parent = hit.collider.transform.parent;
				if (parent != null && parent.parent != null)
				{
					parent = parent.parent;
					doorController = parent.GetComponent<CameraDoorSnapshot>();
					if (doorController != null)
					{
						num2 = Mathf.Abs(doorController.transform.position.x - Subject.transform.position.x);
						if (Subject.ActiveWeapon != null)
						{
							num3 = Mathf.Abs(doorController.transform.position.x - Subject.ActiveWeapon.Muzzle.position.x);
						}
					}
				}
			}
			if (num2 >= 1.5f && num3 >= 0.2f)
			{
				targetPosition = new Vector3(hit.point.x + hit.normal.x * obstacleHoldOff, base.transform.position.y, base.transform.position.z);
				float num4 = subjectToObstacle / interiorCamera.aspect;
				if (useDynamicFov)
				{
					targetFOV = Mathf.Clamp(2f * Mathf.Atan(num4 / cameraDistance) * 57.29578f, dynamicFovMin, dynamicFovMax);
				}
				else
				{
					targetFOV = StaticFieldOfView;
				}
			}
			else
			{
				targetPosition = new Vector3(vector2.x - doorHoldOff, base.transform.position.y, base.transform.position.z);
				targetFOV = MaxFieldOfView;
			}
		}
		else
		{
			targetPosition = new Vector3(vector2.x, base.transform.position.y, base.transform.position.z);
			targetFOV = MaxFieldOfView;
		}
	}
private void SetInitialPosition()
	{
		UpdateTargetPosition();
		targetPosition = base.transform.position;
	}
public void SetManualTargetPosition(float worldX)
	{
		Vector3 position = new Vector3(worldX, base.transform.position.y, base.transform.position.z);
		base.transform.position = position;
		targetPosition = position;
	}
public float SubjectViewDistance()
	{
		if (Subject == null)
		{
			return FrustumWidth;
		}
		if (Subject.Direction == 1f)
		{
			float num = base.transform.position.x - FrustumWidth * 0.5f;
			float num2 = Subject.Eye.position.x - num;
			return FrustumWidth - num2;
		}
		float num3 = base.transform.position.x + FrustumWidth * 0.5f;
		float num4 = num3 - Subject.Eye.position.x;
		return FrustumWidth - num4;
	}
}
