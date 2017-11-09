using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPS_Counter : MonoBehaviour {
	
	public Text Output_FPS;
	public Text Output_FrameTime;
	public  float updateInterval = 0.5F;
	
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeLeft; // Left time for current interval

	void Start()
	{
		timeLeft = updateInterval;
		Output_FPS.text = "Calculating...";
	}
	// Update is called once per frame
	void OnGUI () {
		//deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		timeLeft -= Time.deltaTime;
		accum += Time.timeScale / Time.deltaTime;
		++frames;

		if (timeLeft <= 0.0) {
			float fps = accum / frames;
			float frameTime = (1/fps)*1000;
			Output_FPS.text = string.Format ("{0:0}", fps);
			Output_FrameTime.text = string.Format ("{0:F2}ms", frameTime);
			timeLeft = updateInterval;
			accum = 0.0f;
			frames = 0;
		}
	}
}
