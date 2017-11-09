using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Blob_Controller : MonoBehaviour {

	// The speed of the rain
	[Range(1.0f, 1000.0f)]
	public float ySpeed = 25.0f;
	// The amount a blob is allowed to be off screen, to give smoother turning
	// Means blobs aren't destroyed straight away allowing movement to appear smoother as there isn't just blank spaces.
	[Range(0.0f, 1.0f)]
	public float OffscreenLimit = 0.0f;
	// The maximum distance for a blob to be from the camera
	private float MaxDistance = 500.0f;
	// List of useable meshes for different LODS
	public List<Mesh> Meshes;
	// List of maximum at which to use each mesh
	public List<float> LODMaxDistances;
	// List of parent game objects
	public List<GameObject> Parents;
	// Ground gameObject - Where the ground height is. Could just use a number only, but allows for visualising it
	public GameObject ground;
	// The list of different shaders for the LODs
	private Shader[] Shaders;
	// The renderer of the gameObject
	private Renderer rend;
	// The  number to represent the ground level.
	private float groundLevel = 0;

	// Values for shaders
	private float _displacement;
	private float _speed;

	// Current mesh being used from Meshes
	private int CurrentMesh = 0;

	// Used for debug gui to display if it's on screen
	private bool onScreen = false;
	// Use this for initialization
	void Start () {
		// Get the height of the ground
		groundLevel = ground.transform.position.y;
		// Grab the renderer
		rend = GetComponent<Renderer>();
		// Initiate the list of shaders
		Shaders = new Shader[4];
		Shaders[0] = Shader.Find("Unlit/AlienShaderLOD0");
		Shaders[1] = Shader.Find("Unlit/AlienShaderLOD1");
		Shaders[2] = Shader.Find("Unlit/AlienShaderLOD2");
		Shaders[3] = Shader.Find("Unlit/AlienShaderLOD3");
		// Set the values for the shaders
		_displacement = 1.0f + Random.value;
		_speed = 0.2f + Random.value;
		// Set the shader values.
		SetShaderValues (0);
		// Reset the blob into a valid position.
		Reset (MaxDistance);
		// Check the user hasn't messed with any array sizes
		Warnings ();


	}
	// Function to set the shader value after a change in LOD
	// Only sets the values needed for each shader.
	void SetShaderValues(int i){
		switch(i){
		case 0: 
			rend.material.SetFloat ("_Displacement", _displacement);
			rend.material.SetFloat ("_Speed", _speed);
			rend.material.SetFloat ("_LOD0MaxDistance", LODMaxDistances[0]);
			break;
		case 1: 
			rend.material.SetFloat ("_Displacement", _displacement);
			rend.material.SetFloat ("_Speed", _speed);
			rend.material.SetFloat ("_LOD0MaxDistance", LODMaxDistances[0]);
			rend.material.SetFloat ("_LOD1MaxDistance", LODMaxDistances[1]);
			break;
		case 2:
			rend.material.SetFloat ("_Speed", _speed);
			rend.material.SetFloat ("_LOD1MaxDistance", LODMaxDistances[1]);
			rend.material.SetFloat ("_LOD2MaxDistance", LODMaxDistances[2]);
			break;
		case 3:
			rend.material.SetFloat ("_LOD2MaxDistance", LODMaxDistances[2]);
			rend.material.SetFloat ("_LOD3MaxDistance", LODMaxDistances[3]);
			break;
		}
	}
	
	// Update is called once per frame
	public void Update () {
		// Move Object down
		gameObject.transform.Translate (0.0f, -ySpeed * Time.deltaTime, 0.0f, Space.World);
		//gameObject.transform.localPosition = new Vector3 (Mathf.Sin (Time.time) / 10, 0.0f, Mathf.Cos (Time.time) / 10);

		// Check if on screen
		if(!CheckOnScreen ()){
			// If not, deactivate it.
			gameObject.SetActive(false);
			// And store that it is offscreen, for the debug GUI
			onScreen = false;
		};

		// Check collision with floor or if too far from camera
		if (Vector3.Distance (gameObject.transform.position, Camera.main.transform.position) > MaxDistance || gameObject.transform.position.y <= groundLevel-5.0f){
			// If so, deactivate it.
			gameObject.SetActive(false);
			// And store that it is offscreen, for the debug GUI
			onScreen = false;
		}

		// Check if LODs need to change.
		CheckMeshes ();
	}

	// Function to reset a blob.
	public void Reset(float maxSpawnDistance)
	{
		// Get the main camera.
		Camera cam = Camera.main;
		// Create a vector3 to hold the new position relative to the viewport
		Vector3 screenPoint = new Vector3(0.0f, 0.0f, 0.0f);
		// Get the angle of the camera about the X axis
		float camXAngle = cam.transform.eulerAngles.x;
		if(camXAngle > 180) camXAngle -= 360;	//	This process gets the angle of rotation about the x axis of the camera.
		camXAngle = 1-(camXAngle + 90)/180;		//	It then makes it so that looking down returns a value of 0, whilst up is 1

		// Chose whether to place it in the centre screen or in the off screen parts. This chance value changes depending on angle of camera.
		if (Random.value <= 0.05f + (0.95f * camXAngle)) { // Place it in the centre screen
			// Make it so the x value keeps it with the screen limits +- the extra off screen limits to allow less 'popping' of instances when the camera is moved
			screenPoint.x = Random.value;
			// Make it so that when looking upward, the blob can appear anywhere in the y axis of the view port, yet when looking down, only spawning right at the top so it looks like it's
			// rain falling into view.
			screenPoint.y = (1 - camXAngle) + (Random.value * camXAngle);
			// Place it a minimum of 0.2f * maxSpawnDistance away, so that when it spawns in the middle of the screen, it doesn't 'pop' as much.
			screenPoint.z = (0.2f * maxSpawnDistance) + (0.8f * Random.value * maxSpawnDistance);
		} else { // Place it in the surrounding areas of the view plane, so it falls onto screen.
			// The random chance
			float chance = Random.value;
			// Make it so the ratio of bottom to top spawns makes it so more blobs spawn at top area unless looking directly down to minimise resets
			float ratio = camXAngle * (360/cam.fieldOfView); // 1 At 0.3333<, 0 at 0
			if(ratio > 1.0f) ratio = 1.0f;
			float bottomChance = 0.25f - (ratio * 0.2f);
			float topChance = 0.5f;
			if(chance <= bottomChance){
				// Place in bottom plane
				screenPoint.x = ((Random.value * (1 + (OffscreenLimit * 2)) - OffscreenLimit));	// Anywhere along the x axis within the bounds
				screenPoint.y = 0 - (Random.value * OffscreenLimit); // Anywhere along the y axis below the viewport but above the bounds
				screenPoint.z = (0.2f * maxSpawnDistance) + (0.8f * Random.value * maxSpawnDistance) - (0.2f * Random.value * maxSpawnDistance); // Anywhere within max distance
			} else if (chance <= topChance){
				// Place in top plane
				screenPoint.x = ((Random.value * (1 + (OffscreenLimit * 2)) - OffscreenLimit));	// Anywhere along the x axis within the bounds
				screenPoint.y = 1 + (Random.value * OffscreenLimit); // Anywhere along the y axis above the viewport but below the bounds
				screenPoint.z = (0.2f * maxSpawnDistance) + (0.8f * Random.value * maxSpawnDistance) - (0.2f * Random.value * maxSpawnDistance); // Anywhere within max distance
			}
			else if (chance <= 0.75f){
				// Place in left plane
				screenPoint.x = 0 - (Random.value * OffscreenLimit);	// Anywhere along the x axis left of the viewport but right of the bounds
				screenPoint.y = ((Random.value * (1 + (OffscreenLimit * 2)) - OffscreenLimit)); // Anywhere along the y axis within the bounds
				screenPoint.z = 10 + (0.2f * maxSpawnDistance) + (0.8f * Random.value * maxSpawnDistance) - (0.2f * Random.value * maxSpawnDistance); // Anywhere within max distance
			} else {
				// Place in right plane
				screenPoint.x = 1 + (Random.value * OffscreenLimit);	// Anywhere along the x axis left of the viewport but right of the bounds
				screenPoint.y = ((Random.value * (1 + (OffscreenLimit * 2)) - OffscreenLimit)); // Anywhere along the y axis within the bounds
				screenPoint.z = 10 + (0.2f * maxSpawnDistance) + (0.8f * Random.value * maxSpawnDistance) - (0.2f * Random.value * maxSpawnDistance); // Anywhere within max distance
			}
		}

		// Calculate the worldPoint equivelent of screenPoint.
		Vector3 worldPoint = cam.ViewportToWorldPoint (screenPoint);
		// Set that as the new position
		gameObject.transform.position = worldPoint;
	}

	// Function that checks if the blob is being displayed on screen (or in allowed extra area)
	bool CheckOnScreen()
	{
		// Find the viewport position of the game object.
		Vector3 screenPoint = Camera.main.WorldToViewportPoint (gameObject.transform.position);
		// Check if it's in the allowed area and store that in inAllowedArea
		bool inAllowedArea = screenPoint.z >= -OffscreenLimit && screenPoint.x >= -OffscreenLimit && screenPoint.x <= (1+OffscreenLimit) && screenPoint.y >= -OffscreenLimit && screenPoint.y <= (1+OffscreenLimit);
		if (inAllowedArea == false) {	// If it's not, it's certainly not on screen
			return false;
		} else { // If it is, then calculate if it's actually on screen or just in the allowed area.
			onScreen = screenPoint.z >= -5 && screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1;
			return true;
		}
	}

	// Get onScreen. Used for the debug GUI
	public bool getOnScreen(){
		return onScreen;
	}

	// Check whether the LOD needs to change.
	void CheckMeshes(){
		// Finds the distance from the object to the camera
		float distance = Vector3.Distance(gameObject.transform.position, Camera.main.transform.position);
		// Iterates through the LODs until it finds which one it falls into.
		for (int i = 0; i < Meshes.Count; i++) {
			if(distance <= LODMaxDistances[i]){
				// If its the current LOD, it doesn't need to be set up again.
				if(CurrentMesh != i)
				{	
					// Set up the mesh.
					SetMesh (i);
					// Set it as the current mesh.
					CurrentMesh = i;
					// Set the shader to the appropriate shader
					rend.material.shader = Shaders[i];
					// Set up the values for the shader.
					SetShaderValues(i);
					// Changes the parent of the gameObject. As a result, all objects of each LOD are in the parent gameObject.
					// When rendering with different shaders for object in each LOD, this speeds it up.
					gameObject.transform.SetParent(Parents[i].transform);
				}
				break;
			}
		}
	}

	// A function to change the mesh
	void SetMesh(int meshNumber)
	{
		// Set the mesh to the apropriate one from the list of Meshes.
		GetComponent<MeshFilter> ().sharedMesh = Meshes [meshNumber];
	}

	// A function to set the max distance.
	public void SetMaxDistance(float mDistance){
		MaxDistance = mDistance;
	}

	// Warnings.
	void Warnings(){
		// Warn if there isn't 4 Meshes
		if (Meshes.Count != 4) {
			Debug.LogError ("Number of LOD meshes is not 4");
		}
		// Warn if there isn't 4 Max distances
		if (LODMaxDistances.Count != 4) {
			Debug.LogError ("Number of LOD max distance is not 4");
		}
		// Warn if there isn't 4 Shaders
		if (Shaders.Length != 4) {
			Debug.LogError ("Number of Shaders is not 4");
		}
		// Warn if there isn't 4 Parents
		if (Parents.Count != 4) {
			Debug.LogError ("Number of LOD Parents is not 4");
		}
	}
}
