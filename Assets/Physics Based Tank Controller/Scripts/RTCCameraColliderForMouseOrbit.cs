using UnityEngine;
using System.Collections;

public class RTCCameraColliderForMouseOrbit : MonoBehaviour {

	public LayerMask layerMask;

	private RTCCamera orbitScript;

	private float occDist;
	private GameObject pivotTarget;
	private GameObject target;
	private float defaultDistance;

	void Start () {

		orbitScript = GetComponent<RTCCamera>();
		target = orbitScript.tank.gameObject;
		defaultDistance = orbitScript.distance;

		pivotTarget = new GameObject("Pivot Position");

	}

	void Update () {
	
		pivotTarget.transform.position = new Vector3(target.transform.position.x, target.transform.position.y + orbitScript.heightOffset, target.transform.position.z);
		pivotTarget.transform.LookAt(transform);
		
		RaycastHit hit;

		if(Physics.Raycast(pivotTarget.transform.position, pivotTarget.transform.TransformDirection(Vector3.forward), out hit, defaultDistance, layerMask))
			occDist = (Mathf.Lerp (defaultDistance, 0, hit.distance / (defaultDistance))); 
		else
			occDist = 0;

		orbitScript.distance = (defaultDistance) - occDist;

	}
}
