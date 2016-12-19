using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (Rigidbody))]

public class RTCTankController : MonoBehaviour {

	//Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	//Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	//SteamVR_TrackedObject trackedObj;
	//SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }


	//Rigidbody.
	private Rigidbody rigid;

	//Reversing Bool.
	private bool reversing = false;

	//Wheel Transforms Of The Vehicle.	
	public Transform[] wheelTransform_L;
	public Transform[] wheelTransform_R;

	//Wheel colliders of the vehicle.
	public WheelCollider[] wheelColliders_L;
	public WheelCollider[] wheelColliders_R;

	//All Wheel Colliders.
	private List <WheelCollider> AllWheelColliders = new List<WheelCollider>();
		
	//Useless Gear Wheels.
	public Transform[] uselessGearTransform_L;
	public Transform[] uselessGearTransform_R;
		
	//Track Bones.
	public Transform[] trackBoneTransform_L;
	public Transform[] trackBoneTransform_R;
		
	//Track Customization.
	public GameObject leftTrackMesh;
	public GameObject rightTrackMesh;
	public float trackOffset = 0f;
	public float trackScrollSpeedMultiplier = 1f;
		
	//Wheels Rotation.
	private float[] rotationValueL;
	private float[] rotationValueR;

	//Center Of Mass.
	public Transform COM;

	public AnimationCurve engineTorqueCurve;
	public float engineTorque = 250.0f;
	public float brakeTorque = 250.0f;
	public float minEngineRPM = 1000.0f;
	public float maxEngineRPM = 5000.0f;
	public float maxSpeed = 80.0f;
	public float steerTorque = 3f;
	private float speed;
	private float defSteerAngle;
	private float acceleration = 0f;
	private float lastVelocity = 0f;
	private float engineRPM = 0.0f;
	public float motorInput;
	public float steerInput;

	//Sound Effects.
	private AudioSource engineStartUpAudio;
	private AudioSource engineIdleAudio;
	private AudioSource engineRunningAudio;
		
	public AudioClip engineStartUpAudioClip;
	public AudioClip engineIdleAudioClip;
	public AudioClip engineRunningAudioClip;

	//Smokes.
	public GameObject WheelSlipPrefab;
	private List <ParticleEmitter> WheelParticles = new List<ParticleEmitter>();

	public ParticleEmitter normalExhaustGas;
	public ParticleEmitter heavyExhaustGas;
		
		
	void  Start (){
			
		SetTags();
		EngineStart();
		SoundsInit();
		if(WheelSlipPrefab)
			SmokeInit();

		rigid = GetComponent<Rigidbody>();

		rigid.maxAngularVelocity = 5f;
		rigid.centerOfMass = new Vector3((COM.localPosition.x) * transform.localScale.x , (COM.localPosition.y) * transform.localScale.y , (COM.localPosition.z) * transform.localScale.z);

		rotationValueL = new float[wheelColliders_L.Length];
		rotationValueR = new float[wheelColliders_R.Length];

	}

	public void CreateWheelColliders (){
		
		List <Transform> allWheelTransformsL = new List<Transform>();
		List <Transform> allWheelTransformsR = new List<Transform>();

		foreach(Transform wheel in wheelTransform_L){
			allWheelTransformsL.Add(wheel);
		}
		foreach(Transform wheel in wheelTransform_R){
			allWheelTransformsR.Add(wheel);
		}
		
		if(allWheelTransformsR[0] == null || allWheelTransformsL[0] == null){
			Debug.LogError("You haven't choose your Wheel Transforms. Please select all of your Wheel Transforms before creating Wheel Colliders. Script needs to know their positions, aye?");
			return;
		}
		
		transform.rotation = Quaternion.identity;
		
		GameObject _WheelColliders_L = new GameObject("WheelColliders_L");
		_WheelColliders_L.transform.parent = transform;
		_WheelColliders_L.transform.rotation = transform.rotation;
		_WheelColliders_L.transform.localPosition = Vector3.zero;
		_WheelColliders_L.transform.localScale = Vector3.one;

		GameObject _WheelColliders_R = new GameObject("WheelColliders_R");
		_WheelColliders_R.transform.parent = transform;
		_WheelColliders_R.transform.rotation = transform.rotation;
		_WheelColliders_R.transform.localPosition = Vector3.zero;
		_WheelColliders_R.transform.localScale = Vector3.one;

		#region Wheel Collider Properties
		foreach(Transform wheel in allWheelTransformsL){
			
			GameObject wheelcolliderL = new GameObject(wheel.transform.name); 
			
			wheelcolliderL.transform.position = wheel.transform.position;
			wheelcolliderL.transform.rotation = transform.rotation;
			wheelcolliderL.transform.name = wheel.transform.name;
			wheelcolliderL.transform.parent = _WheelColliders_L.transform;
			wheelcolliderL.transform.localScale = Vector3.one;
			wheelcolliderL.AddComponent<WheelCollider>();
			wheelcolliderL.GetComponent<WheelCollider>().radius = (wheel.GetComponent<MeshRenderer>().bounds.size.y / 2f) / transform.localScale.y;
			
			JointSpring spring = wheelcolliderL.GetComponent<WheelCollider>().suspensionSpring;
			
			spring.spring = 50000f;
			spring.damper = 5000f;
			spring.targetPosition = .5f;

			wheelcolliderL.GetComponent<WheelCollider>().mass = 250f;
			wheelcolliderL.GetComponent<WheelCollider>().wheelDampingRate = 1f;
			wheelcolliderL.GetComponent<WheelCollider>().suspensionDistance = .3f;
			wheelcolliderL.GetComponent<WheelCollider>().forceAppPointDistance = .25f;
			wheelcolliderL.GetComponent<WheelCollider>().suspensionSpring = spring;
			
			WheelFrictionCurve sidewaysFriction = wheelcolliderL.GetComponent<WheelCollider>().sidewaysFriction;
			WheelFrictionCurve forwardFriction = wheelcolliderL.GetComponent<WheelCollider>().forwardFriction;
			
			forwardFriction.extremumSlip = .4f;
			forwardFriction.extremumValue = 1;
			forwardFriction.asymptoteSlip = .8f;
			forwardFriction.asymptoteValue = .75f;
			forwardFriction.stiffness = 1.75f;
			
			sidewaysFriction.extremumSlip = .25f;
			sidewaysFriction.extremumValue = 1;
			sidewaysFriction.asymptoteSlip = .5f;
			sidewaysFriction.asymptoteValue = .75f;
			sidewaysFriction.stiffness = 2f;
			
			wheelcolliderL.GetComponent<WheelCollider>().sidewaysFriction = sidewaysFriction;
			wheelcolliderL.GetComponent<WheelCollider>().forwardFriction = forwardFriction;
			
		}

		foreach(Transform wheel in allWheelTransformsR){
			
			GameObject wheelcolliderR = new GameObject(wheel.transform.name); 
			
			wheelcolliderR.transform.position = wheel.transform.position;
			wheelcolliderR.transform.rotation = transform.rotation;
			wheelcolliderR.transform.name = wheel.transform.name;
			wheelcolliderR.transform.parent = _WheelColliders_R.transform;
			wheelcolliderR.transform.localScale = Vector3.one;
			wheelcolliderR.AddComponent<WheelCollider>();
			wheelcolliderR.GetComponent<WheelCollider>().radius = (wheel.GetComponent<MeshRenderer>().bounds.size.y / 2f) / transform.localScale.y;
			
			JointSpring spring = wheelcolliderR.GetComponent<WheelCollider>().suspensionSpring;
			
			spring.spring = 50000f;
			spring.damper = 5000f;
			spring.targetPosition = .5f;
			
			wheelcolliderR.GetComponent<WheelCollider>().mass = 250f;
			wheelcolliderR.GetComponent<WheelCollider>().wheelDampingRate = 1f;
			wheelcolliderR.GetComponent<WheelCollider>().suspensionDistance = .3f;
			wheelcolliderR.GetComponent<WheelCollider>().forceAppPointDistance = .25f;
			wheelcolliderR.GetComponent<WheelCollider>().suspensionSpring = spring;
			
			WheelFrictionCurve sidewaysFriction = wheelcolliderR.GetComponent<WheelCollider>().sidewaysFriction;
			WheelFrictionCurve forwardFriction = wheelcolliderR.GetComponent<WheelCollider>().forwardFriction;
			
			forwardFriction.extremumSlip = .4f;
			forwardFriction.extremumValue = 1;
			forwardFriction.asymptoteSlip = .8f;
			forwardFriction.asymptoteValue = .75f;
			forwardFriction.stiffness = 1.75f;
			
			sidewaysFriction.extremumSlip = .25f;
			sidewaysFriction.extremumValue = 1;
			sidewaysFriction.asymptoteSlip = .5f;
			sidewaysFriction.asymptoteValue = .75f;
			sidewaysFriction.stiffness = 2f;
			
			wheelcolliderR.GetComponent<WheelCollider>().sidewaysFriction = sidewaysFriction;
			wheelcolliderR.GetComponent<WheelCollider>().forwardFriction = forwardFriction;
			
		}
		#endregion

		wheelColliders_L = _WheelColliders_L.GetComponentsInChildren<WheelCollider>();
		wheelColliders_R = _WheelColliders_R.GetComponentsInChildren<WheelCollider>();
		
	}

	void SetTags(){

		for(int i = 0; i <= 30; i++){

			if(LayerMask.LayerToName(i) == "TankCollider"){
				break;
			}else{
				if(i == 30){
					Debug.LogError ("Couldn't found ''TankCollider'' layer! Create ''TankCollider'' layer. You can create layers from Project Settings --> Tags and Layers");
					Debug.Break();
				}
			}

		}

		for(int i = 0; i <= 30; i++){

			if(LayerMask.LayerToName(i) == "Wheel"){
				break;
			}else{
				if(i == 30){
					Debug.LogError ("Couldn't found ''Wheel'' layer! Create ''Wheel'' layer. You can create layers from Project Settings --> Tags and Layers");
					Debug.Break();
				}
			}

		}

		for(int i = 0; i <= 30; i++){

			if(LayerMask.LayerToName(i) == "Bullet"){
				break;
			}else{
				if(i == 30){
					Debug.LogError ("Couldn't found ''Bullet'' layer! Create ''Bullet'' layer. You can create layers from Project Settings --> Tags and Layers");
					Debug.Break();
				}
			}

		}


		Transform[] gObjects = transform.GetComponentsInChildren<Transform>();
		gameObject.layer = LayerMask.NameToLayer("TankCollider");

		foreach(Transform t in gObjects){
			if(t.GetComponent<WheelCollider>() == null)
				t.gameObject.layer = LayerMask.NameToLayer("TankCollider");
			else
				t.gameObject.layer = LayerMask.NameToLayer("Wheel");
		}
		
	}
		
	void EngineStart(){
			
		engineStartUpAudio = CreateAudioSource("engineStartUpAudio", 5f, 0f, engineStartUpAudioClip, false, true, true);
			
	}
		
	void SoundsInit(){

		engineIdleAudio = CreateAudioSource("engineIdleAudio", 5f, .5f, engineIdleAudioClip, true, true, false);
			
		engineRunningAudio = CreateAudioSource("engineRunningAudio", 5f, 0f, engineRunningAudioClip, true, true, false);
	
	}

	public AudioSource CreateAudioSource(string audioName, float minDistance, float volume, AudioClip audioClip, bool loop, bool playNow, bool destroyAfterFinished){
		
		GameObject audioSource = new GameObject(audioName);
		audioSource.transform.position = transform.position;
		audioSource.transform.rotation = transform.rotation;
		audioSource.transform.parent = transform;
		audioSource.AddComponent<AudioSource>();
		audioSource.GetComponent<AudioSource>().minDistance = minDistance;
		audioSource.GetComponent<AudioSource>().volume = volume;
		audioSource.GetComponent<AudioSource>().clip = audioClip;
		audioSource.GetComponent<AudioSource>().loop = loop;
		audioSource.GetComponent<AudioSource>().spatialBlend = 1f;
		
		if(playNow)
			audioSource.GetComponent<AudioSource>().Play();
		
		if(destroyAfterFinished)
			Destroy(audioSource, audioClip.length);
		
		return audioSource.GetComponent<AudioSource>();
		
	}

	void SmokeInit(){

		WheelCollider[] wheelcolliders = GetComponentsInChildren<WheelCollider>();
		
		foreach(WheelCollider wc in wheelcolliders){
			AllWheelColliders.Add (wc);
		}
		
		for(int i = 0; i < AllWheelColliders.Count; i++){
			GameObject wp = (GameObject)Instantiate(WheelSlipPrefab, AllWheelColliders[i].transform.position, transform.rotation) as GameObject;
			WheelParticles.Add (wp.GetComponent<ParticleEmitter>());
		}
		
		for(int i = 0; i < AllWheelColliders.Count; i++){
			WheelParticles[i].transform.position = AllWheelColliders[i].transform.position;
			WheelParticles[i].transform.parent = AllWheelColliders[i].transform;
		}
		
	}

	void Update(){

		WheelAlign();

	}
		
	void  FixedUpdate (){
			
		AnimateGears();
		Engine();
		Braking();
		Inputs();
		Audio();
		SmokeInstantiateRate();

	}
		
	void Engine(){

		//Speed Limiter.
		if(speed > maxSpeed){
			
			for(int i = 0; i < AllWheelColliders.Count; i++){
				AllWheelColliders[i].motorTorque = 0;
			}

		//Applying Motor Torque.
		}else{
			
			for(int i = 0; i < wheelColliders_L.Length; i++){
				
				if(!reversing){
					if(wheelColliders_L[i].isGrounded && Mathf.Abs(wheelColliders_L[i].rpm) < 1000)
						wheelColliders_L[i].motorTorque = engineTorque * Mathf.Clamp((Mathf.Clamp(motorInput, 0f, 1f)) + Mathf.Clamp(steerInput, -1f, 1f), -1f, 1f) * engineTorqueCurve.Evaluate(speed);
					else
						wheelColliders_L[i].motorTorque = 0;
				}else{
					if(speed < 30){
						wheelColliders_L[i].motorTorque = (engineTorque * motorInput);
					}else{
						wheelColliders_L[i].motorTorque = 0;
					}
				}
				
			}

			for(int i = 0; i < wheelColliders_R.Length; i++){
				
				if(!reversing){
					if(wheelColliders_R[i].isGrounded && Mathf.Abs(wheelColliders_R[i].rpm) < 1000)
						wheelColliders_R[i].motorTorque = engineTorque * Mathf.Clamp((Mathf.Clamp(motorInput, 0f, 1f)) + Mathf.Clamp(-steerInput, -1f, 1f), -1f, 1f) * engineTorqueCurve.Evaluate(speed);
					else
						wheelColliders_R[i].motorTorque = 0;
				}else{
					if(speed < 30){
						wheelColliders_R[i].motorTorque = (engineTorque * motorInput);
					}else{
						wheelColliders_R[i].motorTorque = 0;
					}
				}
				
			}
			
		}

		//Steering.
		if(!reversing){
			if(wheelColliders_L[2].isGrounded || wheelColliders_R[2].isGrounded){
				if(Mathf.Abs(rigid.angularVelocity.y) < 1f){
					rigid.AddRelativeTorque((Vector3.up * steerInput) * steerTorque, ForceMode.Acceleration);
				}
			}
		}else{
			if(wheelColliders_L[2].isGrounded || wheelColliders_R[2].isGrounded){
				if(Mathf.Abs(rigid.angularVelocity.y) < 1f){
					rigid.AddRelativeTorque((-Vector3.up * steerInput) * steerTorque, ForceMode.Acceleration);
				}
			}
		}
			
	}

	void Braking(){
		
		for(int i = 0; i < AllWheelColliders.Count; i++){
			
			if(motorInput == 0){
				if(speed < 25 && Mathf.Abs(steerInput) < .1f)
					AllWheelColliders[i].brakeTorque = brakeTorque / 5f;
				else
					AllWheelColliders[i].brakeTorque = 0;
			}else if(motorInput < -.1f && AllWheelColliders[0].rpm > 50){
				AllWheelColliders[i].brakeTorque = brakeTorque * (Mathf.Abs(motorInput));
			}else{
				AllWheelColliders[i].brakeTorque = 0;
			}
			
		}
		
	}

	void Inputs(){
		
		//Motor Input.
		//motorInput = Input.GetAxis("Vertical");
		//Debug.Log ("motorInput " + motorInput);
		
		//Steering Input.
		//steerInput = Input.GetAxis("Horizontal");
		//Debug.Log ("steerInput " + steerInput);
		
		//Reversing Bool.
		if(motorInput < 0  && AllWheelColliders[0].rpm < 50)
			reversing = true;
		else reversing = false;
		
		speed = rigid.velocity.magnitude * 3.0f;
		
		//Acceleration Calculation.
		acceleration = 0f;
		acceleration = (transform.InverseTransformDirection(rigid.velocity).z - lastVelocity) / Time.fixedDeltaTime;
		lastVelocity = transform.InverseTransformDirection(rigid.velocity).z;
		
		//Drag Limit.
		rigid.drag = Mathf.Clamp((acceleration / 10f), 0f, 1f);
		
		//EngineRPM
		engineRPM = Mathf.Clamp((((Mathf.Abs((wheelColliders_L[0].rpm + wheelColliders_R[0].rpm)) * 5f) + minEngineRPM)), minEngineRPM, maxEngineRPM);
		
	}

	void Audio(){
		
		//Audio
		engineIdleAudio.GetComponent<AudioSource>().pitch = Mathf.Clamp ((Mathf.Abs(engineRPM) / Mathf.Abs (maxEngineRPM) + 1), 1f, 2f);
		engineRunningAudio.GetComponent<AudioSource>().pitch = Mathf.Lerp (engineRunningAudio.GetComponent<AudioSource>().pitch, Mathf.Lerp (.4f, 1f, ((engineRPM - minEngineRPM / 1.5f) / (maxEngineRPM + minEngineRPM)) + Mathf.Clamp((Mathf.Clamp(motorInput, 0f, 1f)) + Mathf.Clamp(Mathf.Abs(steerInput), 0f, .5f), .35f, .85f)), Time.deltaTime * 2f);
		engineRunningAudio.GetComponent<AudioSource>().volume = Mathf.Lerp (engineRunningAudio.GetComponent<AudioSource>().volume, Mathf.Clamp((Mathf.Clamp(Mathf.Abs(motorInput), 0f, 1f)) + Mathf.Clamp(Mathf.Abs(steerInput), 0f, 1f), .35f, .85f), Time.deltaTime * 2f);
		
		if(engineStartUpAudio)
			engineStartUpAudio.GetComponent<AudioSource>().volume = Mathf.Lerp(engineStartUpAudio.GetComponent<AudioSource>().volume, 1, Time.deltaTime * 5);
		
	}
		
	void AnimateGears(){
			
			for(int i = 0; i < uselessGearTransform_R.Length; i++){
				uselessGearTransform_R[i].transform.rotation = wheelColliders_R[i].transform.rotation * Quaternion.Euler( rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)], wheelColliders_R[i].steerAngle, 0);
			}
			
			for(int i = 0; i < uselessGearTransform_L.Length; i++){
				uselessGearTransform_L[i].transform.rotation = wheelColliders_L[i].transform.rotation * Quaternion.Euler( rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)], wheelColliders_L[i].steerAngle, 0);
			}
			
	}

	void  WheelAlign (){

		RaycastHit hit;
			
		//Right Wheels Transform.
		for(int k = 0; k < wheelColliders_R.Length; k++){
			
			Vector3 ColliderCenterPoint = wheelColliders_R[k].transform.TransformPoint( wheelColliders_R[k].center );
			
			if ( Physics.Raycast( ColliderCenterPoint, -wheelColliders_R[k].transform.up, out hit, (wheelColliders_R[k].suspensionDistance + wheelColliders_R[k].radius) * transform.localScale.y) ) {
				wheelTransform_R[k].transform.position = hit.point + (wheelColliders_R[k].transform.up * wheelColliders_R[k].radius) * transform.localScale.y;
				trackBoneTransform_R[k].transform.position = hit.point + (wheelColliders_R[k].transform.up * trackOffset) * transform.localScale.y;
			}else{
				wheelTransform_R[k].transform.position = ColliderCenterPoint - (wheelColliders_R[k].transform.up * wheelColliders_R[k].suspensionDistance) * transform.localScale.y;
				trackBoneTransform_R[k].transform.position = ColliderCenterPoint - (wheelColliders_R[k].transform.up * (wheelColliders_R[k].suspensionDistance + wheelColliders_R[k].radius - trackOffset)) * transform.localScale.y;
			}
			
			wheelTransform_R[k].transform.rotation = wheelColliders_R[k].transform.rotation * Quaternion.Euler( rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)], 0, 0);
			rotationValueR[k] += wheelColliders_R[k].rpm * ( 6 ) * Time.deltaTime;
			
		}
		
		//Left Wheels Transform.
		for(int i = 0; i < wheelColliders_L.Length; i++){
			
			Vector3 ColliderCenterPoint = wheelColliders_L[i].transform.TransformPoint( wheelColliders_L[i].center );
			
			if ( Physics.Raycast( ColliderCenterPoint, -wheelColliders_L[i].transform.up, out hit, (wheelColliders_L[i].suspensionDistance + wheelColliders_L[i].radius) * transform.localScale.y) ) {
				wheelTransform_L[i].transform.position = hit.point + (wheelColliders_L[i].transform.up * wheelColliders_L[i].radius) * transform.localScale.y;
				trackBoneTransform_L[i].transform.position = hit.point + (wheelColliders_L[i].transform.up * trackOffset) * transform.localScale.y;
			}else{
				wheelTransform_L[i].transform.position = ColliderCenterPoint - (wheelColliders_L[i].transform.up * wheelColliders_L[i].suspensionDistance) * transform.localScale.y;
				trackBoneTransform_L[i].transform.position = ColliderCenterPoint - (wheelColliders_L[i].transform.up * (wheelColliders_L[i].suspensionDistance + wheelColliders_L[i].radius - trackOffset)) * transform.localScale.y;
			}
			
			wheelTransform_L[i].transform.rotation = wheelColliders_L[i].transform.rotation * Quaternion.Euler( rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)], 0, 0);
			rotationValueL[i] += wheelColliders_L[i].rpm * ( 6 ) * Time.deltaTime;
			
		}
		
		//Scrolling Track Texture Offset.
		leftTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2((rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)]/1000) * trackScrollSpeedMultiplier, 0));
		rightTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2((rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)]/1000) * trackScrollSpeedMultiplier, 0));
		leftTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2((rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)]/1000) * trackScrollSpeedMultiplier, 0));
		rightTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2((rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)]/1000) * trackScrollSpeedMultiplier, 0));
			
	}
		
	void SmokeInstantiateRate (){


		if (WheelParticles.Count > 0){

			for(int i = 0; i < AllWheelColliders.Count; i++){

				WheelHit CorrespondingGroundHit;
				AllWheelColliders[i].GetGroundHit( out CorrespondingGroundHit );

				if(speed > 25 && AllWheelColliders[i].isGrounded) 
					WheelParticles[i].emit = true;
				else WheelParticles[i].emit = false;

			}
			
		}
			
		if(normalExhaustGas){
			if(speed < 15)
				normalExhaustGas.emit = true;
			else normalExhaustGas.emit = false;
		}
		
		if(heavyExhaustGas){
			if(Mathf.Abs(motorInput) > .1f || Mathf.Abs(steerInput) > .1f)
				heavyExhaustGas.emit = true;
			else heavyExhaustGas.emit = false;
		}
		
	}
		
	}