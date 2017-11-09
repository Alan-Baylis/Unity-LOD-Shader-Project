using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
namespace AlienInvasion
{
    public class CreateRain : MonoBehaviour
    {
        public GameObject alien;
		public GameObject ground;
		public int initialBlobCount = 100;
		public float maxSpawnDistance = 500.0f;

		private List<GameObject> blobs;// = new List<GameObject>();
		private int blobsOnScreen;
		private int activeBlobs;
		private int onScreenBlobs;
		private int targetBlobs;
		private float groundLevel = 0;
		private float maxDistance = 500.0f;
		
        // Use this for initialization
        void Start()
		{
			// Set the ground level that blobs cannot go below
			groundLevel = ground.transform.position.y;
			// Initiate the list of blobs
			blobs = new List<GameObject> ();
            // Hide the template object
            this.alien.SetActive(false);
			// maxSpawnDistance represents how far away from the camera a blob can be spawned.
			// Set the maxSpawnDistace to be the default max distance. maxSpawnDistance may be lowered or raised back to maxDistance when updated
			// depending on the height and angle of the camera
			maxSpawnDistance = maxDistance;
			// targetBlobs represents how many blobs should ideally be active.
			// Set the targetBlobs to be the default totalBlobs. targetBlobs may be lowered or raised back to the total number of blobs when updated
			//depending on the height and angle of the camera
			targetBlobs = initialBlobCount;
			// Create the initial blobs
			for (int i = 0; i < initialBlobCount; i++) {
				CreateBlob();
			}
        }
        // Update is called once per frame
        void Update()
		{
			// Increase the number of blobs by 50 if the Right Arrow key has been pressed
			if (Input.GetKeyUp (KeyCode.RightArrow)) {
				for(int i = 0; i < 50; i++){
					CreateBlob();
				}
			}
			// Descrease the number of blobs by 50 if the Left Arrow key has been pressed
			if (Input.GetKeyUp (KeyCode.LeftArrow)) {
				for(int i = 0; i < 50; i++){
					DestroyBlob();
				}
			}
			// Calculate the target number of blobs. This value is used to ensure the right number of blobs are on screen.
			setTargetBlobs ();
			// Calculate the number of active blobs
			setActiveBlobs ();
			// If there is less blobs on screen than needed, reactivate some.
			reactivateBlobs();
			// Calulate how many blobs are on the screen for the debug GUI
			setBlobsOnScreen ();
        }

		// Function to create a new blob
		public void CreateBlob(){
			// Create an instance of alien. The position associated doesnt matter as it will position itself once created.
			GameObject o = (GameObject)Instantiate (alien, new Vector3 (0.0f,0.0f,0.0f), Quaternion.identity);
			// Set the parent transform to this. This will later be changed to whatever LOD it is in.
			o.transform.parent = this.transform;
			// Name it based on which number blobs it is.
			o.name = string.Format ("blob_{0}", blobs.Count);
			// Set it to be active.
			o.SetActive (true);
			// Give it a random rotation for variety in appearance.
			o.transform.rotation = Random.rotation;
			// Set the max distance a blob can exist from the camera.
			o.GetComponent<Blob_Controller>().SetMaxDistance(maxDistance);
			// Add the blob to the blob list.
			blobs.Add (o);
		}
	
		// Function to reactive blobs
		void reactivateBlobs(){
			// Find out if theres not enough active blobs.
			if (activeBlobs < targetBlobs) {
				// If there isn't, find out how many.
				int difference = targetBlobs - activeBlobs;
				// To keep count
				int count = 0;
				// Iterate through the blobs.
				for (int i = 0; i < blobs.Count; i++) {
					// If they're inactive...
					if (blobs [i].activeSelf == false) {
						// Activate them.
						blobs [i].SetActive (true);
						// Reset them.
						blobs [i].GetComponent<Blob_Controller> ().Reset (maxSpawnDistance);
						// Increase the count.
						count++;
						// If no more are needed, break the loop.
						if (count >= difference)
							break;
					}
				}
			}
		}

		// Function to destroy a blob.
		public void DestroyBlob(){
			// Check there actually are any blobs to destroy.
			if (blobs.Count > 0) {
				// Destroy the last blob.
				Destroy (blobs [blobs.Count - 1]);
				// Then remove it from the list.
				blobs.RemoveAt (blobs.Count - 1);
			}
		}

		// Getters - Mainly used for the debug GUI.
		public int GetTotalBlobs(){return blobs.Count;}
		public int GetTargetBlobs(){return targetBlobs;}
		public int GetActiveBlobs(){return activeBlobs;}
		public int GetBlobsOnScreen(){return onScreenBlobs;}
		public float GetMaxSpawnDistance(){return maxSpawnDistance;}

		// Setters //

		// Iterate through the blobs list and count how many are active.
		void setActiveBlobs(){
			activeBlobs = 0;
			for (int i = 0; i < blobs.Count; i++) {
				if (blobs [i].activeSelf == true) {
					activeBlobs++;
				}
			}
		}

		// Iterate through the list of blobs and find out how many are directly on screen.
		void setBlobsOnScreen (){
			onScreenBlobs = 0;
			for (int i = 0; i < blobs.Count; i++) {
				if(blobs[i].GetComponent<Blob_Controller>().getOnScreen() == true){
					onScreenBlobs++;
				}
			}
		}

		// Function to calculate target amount of blobs - prevents over density of blobs when close to the floor
		// It does this by calculating if the camera is at a height lower than the spawn distance and angle is at an angle where the spawning area crosses below the ground level.
		void setTargetBlobs(){
			// Get the main camera
			Camera cam = Camera.main;
			float camXAngle = cam.transform.eulerAngles.x;	// The cameras angle of rotation about the X-axis
			float minCamAngle; // The minumum amount the camera must be facing up so that all blobs can fit in allowed area.
			float minHeight; // The height where it can be guarenteed all blobs will fit in allowed area.
			float angleModifier; // Used to calculate totalModifier based on the angle of the camera, where 1 = remove all and 0 = remove none. When lower, less may be needed at certain angles.
			float heightModifier; // Used to calulate totalModifier based on the height of the camera, where 1 = remove all and 0 = remove none. When lower, less may be needed if looking down.
			float totalModifier; // Used to calculate targetBlobs based on a value between 0 & 1.
		
			// Calculate camera angle
			if (camXAngle > 180)
				camXAngle -= 360; //	This process gets the angle of rotation about the x axis of the camera.
			camXAngle = 90 - camXAngle; // This converts the angle so that 0 is looking down, and 180 is looking up.
		
			// The height above the ground where it can be guarenteed all blobs will fit in allowed area.
			minHeight = maxDistance;

			// maxSpawnDistance is the furthest away a new blobs can spawn
			maxSpawnDistance = maxDistance;

			targetBlobs = blobs.Count;

			// If the cameraY is >= the minHeight don't calculate any modifiers as all blobs can fit in allowed area, but also it stops invalid values being put into Mathf.Acos()
			if (cam.transform.position.y < groundLevel) {
				targetBlobs = 0;
			} else if (cam.transform.position.y < minHeight + groundLevel) {
				// Calculate minCamAngle by finding the angle at which the furthest spawnable point just touches the ground from the current camera height.
				// Then add the angle that the furthest spawnable point is from the viewing direction of the camera.
				minCamAngle = Mathf.Rad2Deg * (Mathf.Acos ((cam.transform.position.y - groundLevel) / minHeight)) + (cam.fieldOfView / 2);
				
				// If the camera X Angle is >= the minAngle don't calculate any modifiers as all blobs can fit in allowed area.
				if(camXAngle < minCamAngle){
					// Calculate heightModifier - a float between 0 & 1 based on how far below the camera is compared to minHeight.
					// The number is squared to roughly mimic how the height effects the size of the spawning area, due to the spawning area being a pyramid shape.
					heightModifier = 1 - Mathf.Pow(Mathf.Clamp (((cam.transform.position.y - groundLevel) / (minHeight)), 0.0f, 1.0f), 2.0f);
			
					// Calculate angleModifier - a float between 0 & 1 based on how far below the camera is compared to minCamAngle..
					// The number is squared to roughly mimic how the angle effects the size of the spawning area, due to the spawning area being a pyramid shape.
					angleModifier = 1 - Mathf.Pow(Mathf.Clamp ((camXAngle / minCamAngle), 0.0f, 1.0f), 2.0f);
			
					// Calculate totalModifer - a float between 0 & 1 to decided how much smaller targetBlobs should be than totalblobs.
					totalModifier = angleModifier * heightModifier;

					// Set targetBlobs.
					targetBlobs = blobs.Count - (int)(totalModifier * (float)blobs.Count);

					// Calculate maximum spawnable distance.
					// If the top plane of the camera viewing area is pointing down, calculate this...
					if(camXAngle < (90 - (cam.fieldOfView/2))){
						// Find the angle of the top plane of the camera viewing area.
						float angle = camXAngle + (cam.fieldOfView/2);
						// Find the distance it would take at that angle to reach the ground level.
						float distance = (cam.transform.position.y-groundLevel)/Mathf.Cos(Mathf.Deg2Rad * angle);
						// Calculate the angle between the top plane of the camera viewing area, and the angle of the camera.
						angle = cam.fieldOfView/2;
						// Calculate the maximum distance from the camera a blob can be generated
						distance = (Mathf.Cos(Mathf.Deg2Rad * angle)*distance);
						// If it is less than the default maxDistance, use that as the spawn distance, else, keep the default.
						if(distance < maxDistance)
							maxSpawnDistance = distance;
					}
				}
			}
		}
    }
}
