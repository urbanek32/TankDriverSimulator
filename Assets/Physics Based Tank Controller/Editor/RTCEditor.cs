using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(RTCTankController)), CanEditMultipleObjects]
public class RTCEditor : Editor {

	RTCTankController tankScript;

	
	void Awake () {

		tankScript = (RTCTankController)target;
	
	}
	

	public override void OnInspectorGUI () {

		if(GUILayout.Button("Create Wheel Colliders")){

			WheelCollider[] wheelColliders = tankScript.gameObject.GetComponentsInChildren<WheelCollider>();
			
			if(wheelColliders.Length >= 1)
				Debug.LogError("Your Tank has Wheel Colliders already!");
			else
				tankScript.CreateWheelColliders();

		}

		DrawDefaultInspector();

		if(GUI.changed){
			tankScript.engineTorqueCurve.MoveKey(0, new Keyframe(0, 1));
			tankScript.engineTorqueCurve.AddKey(new Keyframe(tankScript.maxSpeed, .25f));
		}

	}

}
