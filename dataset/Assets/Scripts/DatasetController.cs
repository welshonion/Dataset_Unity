using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

public class DatasetController : MonoBehaviour
{
	private float hSliderValue = 0.0f;
	private bool menuVisible = false;
	private int operateCameraNumber;
	private bool shadowOn;


	private RecorderControllerSettings setting;

	int previousCameraNumber;

	string playModeString;

	private bool m_isRecording;

	private int frameNumber;


	private DateTimeOffset datetime;

	private string datetimeStr;
	private string datetimeStr_milli;

	private long datetime_unix;
	private string datetime_unix_str;


	private string folderPath;

	private StreamWriter sw_rgb;
	private StreamWriter sw_gt;
	private StreamWriter sw_yaml;

	[SerializeField] private GameObject cameraObject;
	[SerializeField] private Camera cameraParameter;

	Vector3 pos;
	Quaternion rot;


	[SerializeField] private float framerate = 30f;
	[SerializeField] private int width = 640;
	[SerializeField] private int height = 480;

	[SerializeField] private int finishFrameNumber = 4000;

	private RecorderController recorderController;


	// Use this for initialization
	void Start()
	{

		this.GetComponent<CameraController>().ChangeCamera(0);
		operateCameraNumber = 0;
		previousCameraNumber = 0;

		this.GetComponent<AmbientController>().changeShadow(true);
		shadowOn = true;

		changePlayMode(0);

		// `RecorderControllerSettings`はScriptableObject
		setting = ScriptableObject.CreateInstance<RecorderControllerSettings>();

		frameNumber = 0;
		m_isRecording = false;

		datetime = System.DateTimeOffset.Now;
		datetimeStr = datetime.ToString();
		datetimeStr_milli = datetime.Millisecond.ToString();
		Debug.Log(datetimeStr);


		

	}

	// Update is called once per frame
	void Update()
	{

		if (m_isRecording)
		{
			if (frameNumber > finishFrameNumber)
			{
				RecordingFinish();
				m_isRecording = false;
			}


			datetime = datetime.AddMilliseconds(Time.deltaTime * 1000f);

			datetimeStr = datetime.ToString();
			datetimeStr_milli = datetime.Millisecond.ToString();
			//Debug.Log(frameNumber);

			datetime_unix = datetime.ToUnixTimeMilliseconds();
			datetime_unix_str = datetime_unix.ToString();
			datetime_unix_str = datetime_unix_str.Substring(0, 10) + "." + datetime_unix_str.Substring(10);

            if (frameNumber < 10000)
            {
				sw_rgb.WriteLine(datetime_unix_str + " rgb/image_"+String.Format("{0:0000}",frameNumber)+".png");
			}
            else
            {
				sw_rgb.WriteLine(datetime_unix_str + " rgb/image_"+frameNumber+".png");
			}

			pos = cameraObject.transform.position;
			rot = cameraObject.transform.rotation;

			sw_gt.WriteLine(String.Format("{0} {1} {2} {3} {4} {5} {6}", datetime_unix_str, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w));

            if (frameNumber % (int)(finishFrameNumber / 10) == 0)
            {
				Debug.Log(String.Format("{0}frame", frameNumber));
            }


			frameNumber++;
        }
        else
        {
			datetime = System.DateTime.Now;

			datetimeStr = datetime.ToString();
			datetimeStr_milli = datetime.Millisecond.ToString();
		}
	}


	void OnGUI()
	{
		GUI.Label(new Rect(90, 150, 100, 50), datetimeStr);

		GUI.Label(new Rect(90, 200, 100, 50), datetimeStr_milli);

		if (menuVisible == true)
		{
			GUI.BeginGroup(new Rect(50, 50, Screen.width - 100, 320));

			GUI.Box(new Rect(0, 0, Screen.width - 100, 320), "Control Menu");

			if (GUI.Button(new Rect(Screen.width - 100 - 50, 10, 40, 40), "X"))
			{
				menuVisible = false;
			}

			// ---------- Sky Control ----------
			GUI.Label(new Rect(20, 40, 100, 30), "Sky Control");
			if (GUI.Button(new Rect(20, 60, 80, 40), "Sunny"))
			{
				this.GetComponent<AmbientController>().changeSkybox(AmbientController.AmbientType.AMBIENT_SKYBOX_SUNNY);
			}
			if (GUI.Button(new Rect(110, 60, 80, 40), "Cloud"))
			{
				this.GetComponent<AmbientController>().changeSkybox(AmbientController.AmbientType.AMBIENT_SKYBOX_CLOUD);
			}
			if (GUI.Button(new Rect(200, 60, 80, 40), "Night"))
			{
				this.GetComponent<AmbientController>().changeSkybox(AmbientController.AmbientType.AMBIENT_SKYBOX_NIGHT);
			}

			// ---------- Shadow Control ----------
			GUI.Label(new Rect(20, 110, 100, 30), "Shadow Control");
			if (GUI.Button(new Rect(20, 130, 80, 40), "On / Off"))
			{
				if (shadowOn == false)
				{
					this.GetComponent<AmbientController>().changeShadow(true);
					shadowOn = true;
				}
				else
				{
					this.GetComponent<AmbientController>().changeShadow(false);
					shadowOn = false;
				}
			}
			GUI.Label(new Rect(120, 130, 100, 30), "Time");
			hSliderValue = GUI.HorizontalSlider(new Rect(120, 155, 150, 30), hSliderValue, 0.0f, 100.0f);
			this.GetComponent<AmbientController>().rotateAmbientLight(hSliderValue);

			// ---------- Effect Control ----------
			GUI.Label(new Rect(20, 180, 100, 30), "Effect Control");
			if (GUI.Button(new Rect(20, 200, 80, 40), "None"))
			{
				this.GetComponent<AmbientController>().changeParticle(AmbientController.ParticleType.PARTICLE_NONE);
			}
			if (GUI.Button(new Rect(110, 200, 80, 40), "Wind"))
			{
				this.GetComponent<AmbientController>().changeParticle(AmbientController.ParticleType.PARTICLE_WIND);
			}
			if (GUI.Button(new Rect(200, 200, 80, 40), "Rain"))
			{
				this.GetComponent<AmbientController>().changeParticle(AmbientController.ParticleType.PARTICLE_RAIN);
			}

			// ---------- Camera Control ----------
			/*if (operateCameraNumber < 100)
			{
				GUI.Label (new Rect (400, 40, 100, 30), "Camera Control");
				if (GUI.Button (new Rect (400, 60, 50, 40), "<---"))
				{
					operateCameraNumber--;
					if (operateCameraNumber < 0)
					{
						operateCameraNumber = this.GetComponent<CameraController>().targetCameraNames.Count -1;
						previousCameraNumber = operateCameraNumber;
					}
				}
				if (GUI.Button (new Rect (600, 60, 50, 40), "--->"))
				{
					operateCameraNumber++;
					if (operateCameraNumber > this.GetComponent<CameraController>().targetCameraNames.Count -1)
					{
						operateCameraNumber = 0;
						previousCameraNumber = operateCameraNumber;
					}
				}
				GUI.Label (new Rect (460, 60, 140, 20), this.GetComponent<CameraController>().targetCameraNames[operateCameraNumber]);
				if (GUI.Button (new Rect (450, 80, 150, 20), "Change"))
				{
					this.GetComponent<CameraController>().ChangeCamera(operateCameraNumber);
					previousCameraNumber = operateCameraNumber;
				}
			}*/


			// ---------- Info Control ----------
			if (GUI.Button(new Rect(400, 200, 120, 40), "Start Recording"))
			{

				menuVisible = false;

				InitAICars();
				changePlayMode(2);

				Recording();

			}

			if (GUI.Button(new Rect(400, 250, 120, 40), "Stop Recording"))
			{
				menuVisible = false;

				RecordingFinish();
			}


			GUI.Label(new Rect(400, 180, 100, 30), "Recording");
			/*if (GUI.Button(new Rect(400, 200, 120, 40), "ZENRIN"))
			{
				Application.OpenURL(ZENRIN_URL);
			}
			if (GUI.Button(new Rect(530, 200, 120, 40), "Pocket Queries"))
			{
				Application.OpenURL(PQ_URL);
			}*/



			GUI.EndGroup();
		}
		else
		{
			// ---------- Menu Button ----------
			if (GUI.Button(new Rect(Screen.width - 120, 20, 100, 40), "Menu"))
			{
				menuVisible = true;
			}
		}

		// Display PlayMode 
		GUI.Box(new Rect(30, Screen.height - 60, 250, 50), "Mode = " + playModeString);

	}





	void changePlayMode(int modeNumber)
	{

		switch (modeNumber)
		{
			case 0:
				playModeString = "Normal";
				break;
			case 1:
				playModeString = "FlyThrough\nkey: z = decelerate,  x = accelerate\n arrow key:  up , down, left, right";
				break;
			case 2:
				playModeString = "Driving";
				break;
		}
	}


	void InitAICars()
	{

		GameObject[] targetAICars = GameObject.FindGameObjectsWithTag("AICars");
		foreach (GameObject targetAICar in targetAICars)
		{
			targetAICar.GetComponent<AICarMove>().InitAICar();
			operateCameraNumber = 200;
			this.GetComponent<CameraController>().ChangeCamera(operateCameraNumber);
		}

	}


	void Recording()
    {
		folderPath = "Recordings/data_" + datetime.ToString("yyyyMMddHHmm") + "_fps" + (int)framerate + "/";
		Directory.CreateDirectory(folderPath);

		sw_rgb = new StreamWriter(folderPath + "rgb.txt", false);
		sw_rgb.WriteLine("# color images");
		sw_rgb.WriteLine("# file: '" + folderPath + "'");
		sw_rgb.WriteLine("# timestamp filename");

		sw_gt = new StreamWriter(folderPath + "groundtruth.txt", false);
		sw_gt.WriteLine("# ground truth trajectory");
		sw_gt.WriteLine("# file: '" + folderPath + "'");
		sw_gt.WriteLine("# timestamp tx ty tz qx qy qz qw");


		// Recording Mode
		setting.SetRecordModeToManual();


		//*****Frame Rate*****//
		//Playback
		setting.FrameRatePlayback = FrameRatePlayback.Constant;
		//Target FPS Value
		setting.FrameRate = framerate;
		//Cap FPS
		setting.CapFrameRate = true;


		//*************************//
		//*Image Recorder Settings*//
		//*************************//
		var imageRecorderSettings = ScriptableObject.CreateInstance<ImageRecorderSettings>();

		//*****Capture*****//
		//imageRecorderSettings.imageInputSettings = new CameraInputSettings()
		//{
		//	Source
		//};
		//Source
		//imageRecorderSettings.imageInputSettings.source = ImageSource.MainCamera;

		// この設定では、ゲームビューを解像度640x480で撮影します
		imageRecorderSettings.imageInputSettings = new CameraInputSettings()
		{
			Source = ImageSource.MainCamera,
			OutputWidth = width,
			OutputHeight = height,
			// change to another tag if using ImageSource.TaggedCamera
			//CameraTag = "Depth", 
			RecordTransparency = false,
			CaptureUI = false

		};

		// 動画のファイル名を指定します。撮影された動画は、プロジェクトルート直下に、このファイル名で保存されます
		imageRecorderSettings.OutputFile = folderPath + "rgb/image_<Frame>";
		// 動画のフォーマットを指定します。MP4とWEBMのどちらかを指定します。
		imageRecorderSettings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
		// レコーダーを有効にします
		imageRecorderSettings.Enabled = true;
		// レコーダーを追加します
		setting.AddRecorderSettings(imageRecorderSettings);

		recorderController = new RecorderController(setting);

		recorderController.PrepareRecording();
		recorderController.StartRecording();

		ExportYaml();

		m_isRecording = true;


		/*
		imageRecorderSettings.imageInputSettings = new GameViewInputSettings()
		{

			OutputWidth = 640,
			OutputHeight = 480,
		};
		// 動画のファイル名を指定します。撮影された動画は、プロジェクトルート直下に、このファイル名で保存されます
		imageRecorderSettings.OutputFile = "Recordings/test/image_<Take>_<Frame><Extension>";
		// 動画のフォーマットを指定します。MP4とWEBMのどちらかを指定します。
		imageRecorderSettings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
		// レコーダーを有効にします
		imageRecorderSettings.Enabled = true;
		// レコーダーを追加します
		setting.AddRecorderSettings(imageRecorderSettings);

		var recorderController = new RecorderController(setting);

		recorderController.PrepareRecording();
		recorderController.StartRecording();
		*/


		/*
		// MovieRecorderSettingsもScriptableObject
		var movieRecorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
		// 撮影する対象を指定します
		// この設定では、ゲームビューを解像度640x480で撮影します
		movieRecorderSettings.ImageInputSettings = new GameViewInputSettings() {
			outputWidth = 640,
			outputHeight = 480, 
		};
		// 音声も録画対象に含めます
		movieRecorderSettings.audioInputSettings.preserveAudio = true;
		// 動画のファイル名を指定します。撮影された動画は、プロジェクトルート直下に、このファイル名で保存されます
		movieRecorderSettings.outputFile = "dark-movie-recording";
		// 動画のフォーマットを指定します。MP4とWEBMのどちらかを指定します。
		movieRecorderSettings.outputFormat = VideoRecorderOutputFormat.MP4;
		// レコーダーを有効にします
		movieRecorderSettings.enabled = true;
		// レコーダーを追加します
		setting.AddRecorederSettings(movieRecorderSettings);
		最後にRecorderControllerを初期化します。

		var recorderController = new RecorderController(setting);
		レコーダーコントローラの起動と停止
		RecorderControllerを手動で初期化した場合は、録画の開始と停止を行うことで動画を撮影することができます。 具体的には起動にはRecorderController.StartRecording()を、停止にRecorderController.StopRecording()を呼び出します。

		recorderController.StartRecording();
		// ここから撮影が開始されるのでコンテンツを動かすなどする

		// ここで撮影終了。動画が保存される。
		recorderController.StopRecording();

		*/


		//recorderController.StartRecording();
		// ここから撮影が開始されるのでコンテンツを動かすなどする

		// ここで撮影終了。動画が保存される。
		//recorderController.StopRecording();


	}

	void RecordingFinish()
    {

		sw_rgb.Flush();
		sw_rgb.Close();
		sw_gt.Flush();
		sw_gt.Close();

		m_isRecording = false;

		recorderController.StopRecording();

		UnityEngine.Application.Quit();
	}

	void ExportYaml()
    {

		Debug.Log(cameraParameter.sensorSize.x);
		float fx, fy, cx, cy, k1, k2, p1, p2, k3;

		fx = cameraParameter.focalLength * 640.0f / cameraParameter.sensorSize.x;
		fy = cameraParameter.focalLength * height / cameraParameter.sensorSize.y;
		cx = width / 2;
		cy = height / 2;
		k1 = 0.0f;
		k2 = 0.0f;
		p1 = 0.0f;
		p2 = 0.0f;
		k3 = 0.0f;


		//Debug.Log(String.Format("fx:{0},fy:{1},cx:,cy",fx,fy));


		sw_yaml = new StreamWriter(folderPath + "exp.yaml", false);
		sw_yaml.WriteLine("% YAML:1.0");
		sw_yaml.WriteLine("");
		sw_yaml.WriteLine("#--------------------------------------------------------------------------------------------");
		sw_yaml.WriteLine("# Camera Parameters. Adjust them!");
		sw_yaml.WriteLine("#--------------------------------------------------------------------------------------------");
		sw_yaml.WriteLine("");
		sw_yaml.WriteLine("# Camera calibration and distortion parameters (OpenCV)");
		sw_yaml.WriteLine(String.Format("Camera.fx: {0:F}",fx));
		sw_yaml.WriteLine(String.Format("Camera.fy: {0:F}", fy));
		sw_yaml.WriteLine(String.Format("Camera.cx: {0:F}", cx));
		sw_yaml.WriteLine(String.Format("Camera.cy: {0:F}", cy));
		sw_yaml.WriteLine("");
		sw_yaml.WriteLine(String.Format("Camera.k1: {0:F}", k1));
		sw_yaml.WriteLine(String.Format("Camera.k2: {0:F}", k2));
		sw_yaml.WriteLine(String.Format("Camera.p1: {0:F}", p1));
		sw_yaml.WriteLine(String.Format("Camera.p2: {0:F}", p2));
		sw_yaml.WriteLine(String.Format("Camera.k3: {0:F}", k3));
		sw_yaml.WriteLine("");
		sw_yaml.WriteLine("# Camera frames per second ");
		sw_yaml.WriteLine(String.Format("Camera.fps: {0:F}", framerate));
		sw_yaml.WriteLine("");
		sw_yaml.WriteLine("# Color order of the images (0: BGR, 1: RGB. It is ignored if images are grayscale)");
		sw_yaml.WriteLine("Camera.RGB: 1");
		sw_yaml.WriteLine("");



		sw_yaml.WriteLine("#--------------------------------------------------------------------------------------------");
		sw_yaml.WriteLine("# ORB Parameters");
		sw_yaml.WriteLine("#--------------------------------------------------------------------------------------------");
		sw_yaml.WriteLine("");
		sw_yaml.WriteLine("# ORB Extractor: Number of features per image");
		sw_yaml.WriteLine("ORBextractor.nFeatures: 1000");
		sw_yaml.WriteLine("");
		sw_yaml.WriteLine("# ORB Extractor: Scale factor between levels in the scale pyramid 	");
		sw_yaml.WriteLine("ORBextractor.scaleFactor: 1.2");
		sw_yaml.WriteLine("");
		sw_yaml.WriteLine("# ORB Extractor: Number of levels in the scale pyramid	");
		sw_yaml.WriteLine("ORBextractor.nLevels: 8");
		sw_yaml.WriteLine("");
		sw_yaml.WriteLine("# ORB Extractor: Fast threshold");
		sw_yaml.WriteLine("# Image is divided in a grid. At each cell FAST are extracted imposing a minimum response.");
		sw_yaml.WriteLine("# Firstly we impose iniThFAST. If no corners are detected we impose a lower value minThFAST");
		sw_yaml.WriteLine("# You can lower these values if your images have low contrast			");
		sw_yaml.WriteLine("		ORBextractor.iniThFAST: 20");
		sw_yaml.WriteLine("ORBextractor.minThFAST: 7");
		sw_yaml.WriteLine("");



		sw_yaml.WriteLine("#--------------------------------------------------------------------------------------------");
		sw_yaml.WriteLine("# Viewer Parameters");
		sw_yaml.WriteLine("#--------------------------------------------------------------------------------------------");
		sw_yaml.WriteLine("Viewer.KeyFrameSize: 0.05");
		sw_yaml.WriteLine("Viewer.KeyFrameLineWidth: 1");
		sw_yaml.WriteLine("Viewer.GraphLineWidth: 0.9");
		sw_yaml.WriteLine("Viewer.PointSize:2");
		sw_yaml.WriteLine("Viewer.CameraSize: 0.08");
		sw_yaml.WriteLine("Viewer.CameraLineWidth: 3"); 
		sw_yaml.WriteLine("Viewer.ViewpointX: 0");
		sw_yaml.WriteLine("Viewer.ViewpointY: -0.7");
		sw_yaml.WriteLine("Viewer.ViewpointZ: -1.8");
		sw_yaml.WriteLine("Viewer.ViewpointF: 500");
		sw_yaml.WriteLine("");

		sw_yaml.Flush();
		sw_yaml.Close();
	}
}
