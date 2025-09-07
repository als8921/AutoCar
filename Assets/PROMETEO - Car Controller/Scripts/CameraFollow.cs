using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform carTransform;
	[Range(1, 10)]
	public float followSpeed = 2;
	[Range(1, 10)]
	public float lookSpeed = 5;
	Vector3 initialCameraPosition;
	Vector3 initialCarPosition;
	Vector3 absoluteInitCameraPosition;
	
	[Header("ROS Image Publishing")]
	public bool enableImagePublishing = true;
	private CameraImagePublisher imagePublisher;

	void Start(){
		initialCameraPosition = gameObject.transform.position;
		initialCarPosition = carTransform.position;
		absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
		
		// ROS 이미지 발행기 초기화
		if (enableImagePublishing)
		{
			SetupImagePublisher();
		}
	}
	
	void SetupImagePublisher()
	{
		// CameraImagePublisher 컴포넌트 추가
		imagePublisher = gameObject.GetComponent<CameraImagePublisher>();
		if (imagePublisher == null)
		{
			imagePublisher = gameObject.AddComponent<CameraImagePublisher>();
		}
		
		// 카메라 참조 설정
		Camera cam = GetComponent<Camera>();
		if (cam != null)
		{
			imagePublisher.targetCamera = cam;
		}
		
		Debug.Log("CameraFollow: ROS Image Publisher initialized");
	}

	void FixedUpdate()
	{
		//Look at car
		Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
		Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);

		//Move to car
		Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
		transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);

	}
	
	// 공개 메서드들
	public void ToggleImagePublishing()
	{
		enableImagePublishing = !enableImagePublishing;
		
		if (enableImagePublishing)
		{
			SetupImagePublisher();
		}
		else
		{
			if (imagePublisher != null)
			{
				imagePublisher.StopPublishing();
				DestroyImmediate(imagePublisher);
				imagePublisher = null;
			}
		}
	}
	
	public bool IsImagePublishingEnabled()
	{
		return enableImagePublishing && imagePublisher != null && imagePublisher.IsPublishing();
	}
	
	public void SetImagePublishFrequency(float frequency)
	{
		if (imagePublisher != null)
		{
			imagePublisher.SetPublishFrequency(frequency);
		}
	}
	
	public void SetImageResolution(int width, int height)
	{
		if (imagePublisher != null)
		{
			imagePublisher.SetImageResolution(width, height);
		}
	}

}
