using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AlienInvasion;

public class Debug_GUI : MonoBehaviour {

	// The text objects to send results to
	public Text Output_FPS;
	public Text Output_FrameTime;
	public Text Output_Total_Blobs;
	public Text Output_Target_Blobs;
	public Text Output_Active_Blobs;
	public Text Output_On_Screen_Blobs;
	public Text Output_Cam_X;
	public Text Output_Cam_Y;
	public Text Output_Cam_Z;
	public Text Output_Cam_Rot_X;
	public Text Output_Max_Spawn_Distance;
	public GameObject AlienRain;

	// How often to update.
	public  float updateInterval = 1.0f;
	
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeLeft; // Left time for current interval
	private Camera cam;
	
	void Start()
	{
		cam = Camera.main;
		timeLeft = updateInterval;
		Output_FPS.text = "Calculating...";
		// Set first values
		Output_Total_Blobs.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetTotalBlobs());
		Output_Target_Blobs.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetTargetBlobs());
		Output_Active_Blobs.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetActiveBlobs());
		Output_On_Screen_Blobs.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetBlobsOnScreen());
		Output_Max_Spawn_Distance.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetMaxSpawnDistance());
		// Find camera angle
		float camXAngle = cam.transform.eulerAngles.x;
		if (camXAngle > 180)
			camXAngle -= 360; //	This process gets the angle of rotation about the x axis of the camera.
		camXAngle = 90 - camXAngle;
		Output_Cam_Rot_X.text = string.Format ("{0:0}", camXAngle);
	}
	// Update is called once per frame
	void OnGUI () {
		// Track how much time has passed and how many frames
		timeLeft -= Time.deltaTime;
		accum += Time.timeScale / Time.deltaTime;
		++frames;

		// If there's no time left
		if (timeLeft <= 0.0) {
			// Calculate the fps over that time. Keeps it a smoother number
			float fps = accum / frames;
			float frameTime = (1/fps)*1000;
			Output_FPS.text = string.Format ("{0:0}", fps);
			Output_FrameTime.text = string.Format ("{0:F2}ms", frameTime);

			timeLeft = updateInterval;
			accum = 0.0f;
			frames = 0;
			Output_Active_Blobs.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetActiveBlobs());
			Output_On_Screen_Blobs.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetBlobsOnScreen());
		}

		// Output data
		Output_Target_Blobs.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetTargetBlobs());
		Output_Total_Blobs.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetTotalBlobs());

		Output_Cam_X.text = string.Format ("X:{0:0}", cam.transform.position.x);
		Output_Cam_Y.text = string.Format ("Y:{0:0}", cam.transform.position.y);
		Output_Cam_Z.text = string.Format ("Z:{0:0}", cam.transform.position.z);

		Output_Max_Spawn_Distance.text = string.Format ("{0:0}", AlienRain.GetComponent<AlienInvasion.CreateRain>().GetMaxSpawnDistance());

		// Calculate angle of camera about the x-axis
		float camXAngle = cam.transform.eulerAngles.x;
		if (camXAngle > 180)
			camXAngle -= 360; //	This process gets the angle of rotation about the x axis of the camera.
		camXAngle = 90 - camXAngle;
		Output_Cam_Rot_X.text = string.Format ("{0:0}", camXAngle);
	}
}
