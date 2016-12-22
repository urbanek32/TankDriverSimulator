using UnityEngine;
using System.Collections;

public class WandController : MonoBehaviour {

	Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	Valve.VR.EVRButtonId padUpButton = Valve.VR.EVRButtonId.k_EButton_DPad_Up;

	SteamVR_TrackedObject trackedObj;
	//SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }

	GameObject pickup;

	public RTCTankController TankCtrl;
	public RTCTankGunController TankGunCtrl;


	SteamVR_Controller.Device rightDevice;
	SteamVR_Controller.Device leftDevice;
	SteamVR_TrackedController controller;

	int leftIndex;
	int rightIndex;

	// Use this for initialization
	void Start () {
		trackedObj = GetComponent<SteamVR_TrackedObject> ();

		rightIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
		leftIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);

		rightDevice = SteamVR_Controller.Input(rightIndex);
		leftDevice = SteamVR_Controller.Input(leftIndex);

		controller = GetComponent<SteamVR_TrackedController>();

	}
	
	// Update is called once per frame
	void Update () {
		if (rightDevice.GetPressDown(gripButton)) {
			Debug.Log ("grip down");
			rightDevice.TriggerHapticPulse(2000);
			TankCtrl.motorInput = 1;
		}
		if (rightDevice.GetPressUp (gripButton)) {
			Debug.Log ("grip up");
			TankCtrl.motorInput = 0;
		}

		if (leftDevice.GetPressDown(gripButton)) {
			Debug.Log ("grip down");
			leftDevice.TriggerHapticPulse(2000);
			TankCtrl.motorInput = -1;
		}
		if (leftDevice.GetPressUp (gripButton)) {
			Debug.Log ("grip up");
			TankCtrl.motorInput = 0;
		}

		if (rightDevice.GetPressDown(triggerButton)) {
			Debug.Log ("trigger down");
			rightDevice.TriggerHapticPulse(2000);
			TankCtrl.steerInput = 1;
		}
		if (rightDevice.GetPressUp (triggerButton)) {
			Debug.Log ("trigger up");
			TankCtrl.steerInput = 0;
		}

		if (leftDevice.GetPressDown(triggerButton)) {
			Debug.Log ("trigger down");
			leftDevice.TriggerHapticPulse(2000);
			TankCtrl.steerInput = -1;
		}
		if (leftDevice.GetPressUp (triggerButton)) {
			Debug.Log ("trigger up");
			TankCtrl.steerInput = 0;
		}

		TankGunCtrl.IsGunRotating = controller.padPressed;
		if (controller.controllerIndex == leftIndex) {
			if (controller.padPressed) {
				Debug.Log ("L pad up");

				TankGunCtrl.rigid.AddRelativeTorque (0, (-100) * Mathf.Abs (1), 0, ForceMode.Force);
				//TankGunCtrl.LastRotation = TankGunCtrl.transform.rotation;
			} else {
				TankGunCtrl.LastRotation = TankGunCtrl.rigid.rotation;
			}
		}
		if (controller.controllerIndex == rightIndex) {
			if (controller.padPressed) {
				Debug.Log ("R pad up");
			}
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
