using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;

public class WorldController : Singleton<WorldController> {

    /// <summary>
    /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
    /// </summary>
    public Camera FirstPersonCamera;
	
	void Update () {
		//if not prop selected, return
		if(!PropMenu.Instance || PropMenu.Instance.SelectedProp == null) return;

        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

		//if touch is on a button, then ignore it
		IEnumerable<Button> buttons = FindObjectsOfType<Button>();
		foreach(var b in buttons) {
			if(RectTransformUtility.RectangleContainsScreenPoint(b.GetComponent<RectTransform>(), touch.position)) return;
		}

		//raycast against colliders to search for objects we can spawn the prop on
		RaycastHit hitInfo;
		if(Physics.Raycast(FirstPersonCamera.transform.position, FirstPersonCamera.ScreenToWorldPoint(touch.position), out hitInfo))
		{
            var propObject = Instantiate(PropMenu.Instance.SelectedProp, hitInfo.point + Vector3.up * PropMenu.Instance.SelectedProp.GetComponent<Collider>().bounds.extents.y, Quaternion.identity);

            // Get the camera position and match the y-component with the hit position.
            Vector3 cameraPositionSameY = FirstPersonCamera.transform.position;
            cameraPositionSameY.y = hitInfo.point.y;

            // Have prop look toward the camera respecting his "up" perspective, which may be from ceiling.
            propObject.transform.LookAt(cameraPositionSameY, propObject.transform.up);

			return;
		}

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            var propObject = Instantiate(PropMenu.Instance.SelectedProp, hit.Pose.position, hit.Pose.rotation);

            // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
            // world evolves.
            var anchor = hit.Trackable.CreateAnchor(hit.Pose);

            // prop should look at the camera but still be flush with the plane.
            if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None)
            {
                // Get the camera position and match the y-component with the hit position.
                Vector3 cameraPositionSameY = FirstPersonCamera.transform.position;
                cameraPositionSameY.y = hit.Pose.position.y;

                // Have prop look toward the camera respecting his "up" perspective, which may be from ceiling.
                propObject.transform.LookAt(cameraPositionSameY, propObject.transform.up);
            }

            // Make prop model a child of the anchor.
            propObject.transform.parent = anchor.transform;
        }
	}
}
