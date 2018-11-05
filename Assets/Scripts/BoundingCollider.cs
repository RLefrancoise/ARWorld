using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingCollider : MonoBehaviour {

    public GameObject ColliderPrefab;

	protected Mesh mesh;
	protected Renderer meshRenderer;

	protected GameObject colliderObject;

	// Use this for initialization
	protected virtual void Start () {
		mesh = GetComponent<MeshFilter>().mesh;

		meshRenderer = GetComponent<Renderer>();

		colliderObject = Instantiate(ColliderPrefab, Vector3.zero, Quaternion.identity, transform);
	}
	
	// Update is called once per frame
	protected void Update ()
	{
		colliderObject.transform.position = meshRenderer.bounds.center;
		colliderObject.transform.rotation = transform.rotation;
		_ComputeBounds();
	}

	public void ToggleVisiblity() {
        colliderObject.GetComponentInChildren<Renderer>().enabled = !colliderObject.GetComponentInChildren<Renderer>().enabled;
	}

	protected virtual void _ComputeBounds()
	{
        colliderObject.transform.localScale = new Vector3(meshRenderer.bounds.size.x, 1f, meshRenderer.bounds.size.z);
	}
}
