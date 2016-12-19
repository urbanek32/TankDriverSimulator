using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RTCCamera : MonoBehaviour
{
	public Transform tank;
	private Transform target;
	public float distance = 5.0f;
	public float xSpeed = 250.0f;
	public float ySpeed = 120.0f;
	public float yMinLimit = -20.0f;
	public float yMaxLimit = 80.0f;
	
	private float x;
	private float y;

	public float heightOffset = 0;
	
	void Awake()
	{

		Vector3 angles = transform.eulerAngles;
		x = angles.x;
		y = angles.y;
		
		if(GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}

		GameObject newTarget = new GameObject("target");
		target = newTarget.transform;
		FindObjectOfType<RTCTankGunController>().target = target;

	}
	
	void LateUpdate()
	{
		if(tank != null)
		{

			x += (float)(Input.GetAxis("Mouse X") * xSpeed * 0.02f);
			y -= (float)(Input.GetAxis("Mouse Y") * ySpeed * 0.02f);
			
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			
			Quaternion rotation = Quaternion.Euler(y, x, 0);
			Vector3 position = rotation * (new Vector3(0.0f, heightOffset, -distance)) + tank.position;
			
			transform.rotation = rotation;
			transform.position = position;
			target.position = transform.position + (transform.forward * 100);

		}
	}
	
	private float ClampAngle(float angle, float min, float max)
	{
		if(angle < -360)
		{
			angle += 360;
		}
		if(angle > 360)
		{
			angle -= 360;
		}
		return Mathf.Clamp (angle, min, max);
	}
}