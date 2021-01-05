using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

public class DatasetController : MonoBehaviour
{
	private float hSliderValue = 0.0f;
	private bool menuVisible = false;
	private int operateCameraNumber;
	private bool shadowOn;

	private const string ZENRIN_URL = "http://www.zenrin.co.jp/";
	private const string PQ_URL = "http://www.pocket-queries.co.jp/";


	private RecorderControllerSettings setting;



	int previousCameraNumber;

	string playModeString;



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


	}

	// Update is called once per frame
	void Update()
	{

	}


	void OnGUI()
	{

		if (menuVisible == true)
		{
			GUI.BeginGroup(new Rect(50, 50, Screen.width - 100, 270));

			GUI.Box(new Rect(0, 0, Screen.width - 100, 270), "Control Menu");

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
			GUI.Label(new Rect(120, 130, 100, 30), "TIme");
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


				// Recording Mode
				setting.SetRecordModeToManual();


				//*****Frame Rate*****//
				//Playback
				setting.FrameRatePlayback = FrameRatePlayback.Constant;
				//Target FPS Value
				setting.FrameRate = 30f;
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
}
