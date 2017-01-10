using UnityEngine;
using System.Collections;

public class WandController : MonoBehaviour
{
	public bool DisabledVR;

	Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	Valve.VR.EVRButtonId padUpButton = Valve.VR.EVRButtonId.k_EButton_DPad_Up;

	SteamVR_TrackedObject trackedObj;
	//SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }

	GameObject pickup;

	public RTCTankController TankCtrl;
	public RTCTankGunController TankGunCtrl;
	public float RotationForce = 10f;
	public GameObject Barrel;


	SteamVR_Controller.Device rightDevice;
	SteamVR_Controller.Device leftDevice;
	SteamVR_TrackedController controller;

	int leftIndex;
	int rightIndex;

	// Use this for initialization
	void Start ()
	{
		if (!DisabledVR)
		{
			trackedObj = GetComponent<SteamVR_TrackedObject>();

			rightIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
			leftIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);

			rightDevice = SteamVR_Controller.Input(rightIndex);
			leftDevice = SteamVR_Controller.Input(leftIndex);

			controller = GetComponent<SteamVR_TrackedController>();
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (DisabledVR)
		{
			UpdateControllsWithDisabledVR();
			UpdateTurretRotation();
		}
		else
		{
			UpdateControlls();
		}
	}

	void UpdateControlls()
	{
		if (rightDevice.GetPressDown(gripButton))
		{
			Debug.Log("grip down");
			rightDevice.TriggerHapticPulse(2000);
			TankCtrl.motorInput = 1;
		}
		if (rightDevice.GetPressUp(gripButton))
		{
			Debug.Log("grip up");
			TankCtrl.motorInput = 0;
		}

		if (leftDevice.GetPressDown(gripButton))
		{
			Debug.Log("grip down");
			leftDevice.TriggerHapticPulse(2000);
			TankCtrl.motorInput = -1;
		}
		if (leftDevice.GetPressUp(gripButton))
		{
			Debug.Log("grip up");
			TankCtrl.motorInput = 0;
		}

		if (rightDevice.GetPressDown(triggerButton))
		{
			Debug.Log("trigger down");
			rightDevice.TriggerHapticPulse(2000);
			TankCtrl.steerInput = 1;
		}
		if (rightDevice.GetPressUp(triggerButton))
		{
			Debug.Log("trigger up");
			TankCtrl.steerInput = 0;
		}

		if (leftDevice.GetPressDown(triggerButton))
		{
			Debug.Log("trigger down");
			leftDevice.TriggerHapticPulse(2000);
			TankCtrl.steerInput = -1;
		}
		if (leftDevice.GetPressUp(triggerButton))
		{
			Debug.Log("trigger up");
			TankCtrl.steerInput = 0;
		}
			
		if (controller.controllerIndex == leftIndex)
		{
			if (controller.padTouched)
			{
				TankGunCtrl.rigid.AddRelativeTorque (0, RotationForce * leftDevice.GetAxis ().x, 0, ForceMode.Force);		

				Barrel.transform.Rotate (Vector3.right, RotationForce * leftDevice.GetAxis ().y * Time.deltaTime * -1, Space.Self);		
				
			}
		}
		if (controller.controllerIndex == rightIndex)
		{
			if (controller.padTouched)
			{
				TankGunCtrl.Shooting();
			}
		}
	}

	void UpdateControllsWithDisabledVR()
	{
		if (Input.GetMouseButtonDown(0))
		{
			TankCtrl.steerInput = -1f;
			//TankGunCtrl.rigid.MoveRotation(lastRotation);
		}
		else if (Input.GetMouseButtonUp(0))
		{
			TankCtrl.steerInput = 0f;
		}

		if (Input.GetMouseButtonDown(1))
		{
			TankCtrl.steerInput = 1f;
		}
		else if (Input.GetMouseButtonUp(1))
		{
			TankCtrl.steerInput = 0f;
		}

		if (Input.GetKeyDown(KeyCode.Mouse3))
		{
			TankCtrl.motorInput = -1f;
		}
		else if (Input.GetKeyUp(KeyCode.Mouse3))
		{
			TankCtrl.motorInput = 0f;
		}

		if (Input.GetKeyDown(KeyCode.Mouse4))
		{
			TankCtrl.motorInput = 1f;
		}
		else if (Input.GetKeyUp(KeyCode.Mouse4))
		{
			TankCtrl.motorInput = 0f;
		}
	}

	void UpdateTurretRotation()
	{
		if (Input.GetKey(KeyCode.Q))
		{
			//TankGunCtrl.transform.Rotate(Vector3.up, RotationForce * Time.deltaTime * -1, Space.Self);
			TankGunCtrl.rigid.AddRelativeTorque(0, RotationForce * -1, 0, ForceMode.Force);
		}
		else if (Input.GetKey(KeyCode.E))
		{
			TankGunCtrl.rigid.AddRelativeTorque(0, RotationForce * 1, 0, ForceMode.Force);
			//TankGunCtrl.transform.Rotate(Vector3.up, RotationForce * Time.deltaTime , Space.Self);
		}

	}

	void OnTriggerEnter(Collider coll) {
		pickup = coll.gameObject;
		Debug.Log ("enter");
	}

	void OnTriggerExit(Collider coll) {
		pickup = null;
		Debug.Log ("exit");
	}

	void OnPadClicked(object sender, ClickedEventArgs e){
		Debug.Log ("Pad Clicked! X: " + e.padX + " " + e.padY);
	}
}
