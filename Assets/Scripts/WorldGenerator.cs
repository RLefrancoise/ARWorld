using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.HelloAR;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

public class WorldGenerator : Singleton<WorldGenerator> {

    public float platformBorderThickness = 0.03f;

    public int platformBorderEdges = 4;

    /// <summary>
    /// The color of the generated world
    /// </summary>
    public Color worldColor = Color.blue;

    /// <summary>
    /// The opacity of the generated world
    /// </summary>
    public float worldOpacity = 0.75f;

    public Text debugText;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject WorldPlatform;

    /// <summary>
    /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
    /// </summary>
    public GameObject SearchingForPlaneUI;

    /// <summary>
    /// A list to hold new planes ARCore began tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();

    /// <summary>
    /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

    private List<WorldPlatform> _platforms = new List<WorldPlatform>();

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    public void AddPlatform(WorldPlatform platform)
    {
        if(!_platforms.Contains(platform)) _platforms.Add(platform);
    }

    public void RemovePlatform(WorldPlatform platform)
    {
        if(_platforms.Contains(platform)) _platforms.Remove(platform);
    }

    /// <summary>
    /// Get the ground platform
    /// </summary>
    public WorldPlatform GetGroundPlatform()
    {
        float minHeight = 0f;
        WorldPlatform groundPlatform = null;

        int currentPlatform = 0;

        while (currentPlatform < _platforms.Count) {
            float height = _platforms[currentPlatform].GetComponent<WorldPlatform>().Position.y;
            if(currentPlatform == 0 || height < minHeight)
            {
                minHeight = height;
                groundPlatform = _platforms[currentPlatform];
            }
            currentPlatform++;
        }

        return groundPlatform;
    }

    /// <summary>
    /// The Unity Update() method.
    /// </summary>
    public void Update()
    {
        debugText.text = "";
        foreach(var platform in _platforms)
        {
            debugText.text += string.Format("Platform height: {0} Walls: {1}\n", platform.heightFromGround, platform.WallCount);
        }

        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        _QuitOnConnectionErrors();

        // Check that motion tracking is tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
            if (!m_IsQuitting && Session.Status.IsValid())
            {
                SearchingForPlaneUI.SetActive(true);
            }

            return;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
        Session.GetTrackables<TrackedPlane>(m_NewPlanes, TrackableQueryFilter.New);
        for (int i = 0; i < m_NewPlanes.Count; i++)
        {
            // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
            // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
            // coordinates.
            GameObject planeObject = Instantiate(WorldPlatform, Vector3.zero, Quaternion.identity,
                transform);
            planeObject.GetComponent<WorldPlatform>().Initialize(m_NewPlanes[i]);
        }

        // Hide snackbar when currently tracking at least one plane.
        Session.GetTrackables<TrackedPlane>(m_AllPlanes);
        bool showSearchingUI = true;
        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
            {
                showSearchingUI = false;
                break;
            }
        }

        SearchingForPlaneUI.SetActive(showSearchingUI);
    }

    /// <summary>
    /// Quit the application if there was a connection error for the ARCore session.
    /// </summary>
    private void _QuitOnConnectionErrors()
    {
        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
