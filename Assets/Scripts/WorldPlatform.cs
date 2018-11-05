using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using System.Linq;

public class WorldPlatform : MonoBehaviour
{
    private static int s_PlaneCount = 0;

    private readonly Color[] k_PlaneColors = new Color[]
	{
		new Color(1.0f, 1.0f, 1.0f),
		new Color(0.956f, 0.262f, 0.211f),
		new Color(0.913f, 0.117f, 0.388f),
		new Color(0.611f, 0.152f, 0.654f),
		new Color(0.403f, 0.227f, 0.717f),
		new Color(0.247f, 0.317f, 0.709f),
		new Color(0.129f, 0.588f, 0.952f),
		new Color(0.011f, 0.662f, 0.956f),
		new Color(0f, 0.737f, 0.831f),
		new Color(0f, 0.588f, 0.533f),
		new Color(0.298f, 0.686f, 0.313f),
		new Color(0.545f, 0.764f, 0.290f),
		new Color(0.803f, 0.862f, 0.223f),
		new Color(1.0f, 0.921f, 0.231f),
		new Color(1.0f, 0.756f, 0.027f)
	};

    private TrackedPlane m_TrackedPlane;

    // Keep previous frame's mesh polygon to avoid mesh update every frame.
    private List<Vector3> m_PreviousFrameMeshVertices = new List<Vector3>();
    private List<Vector3> m_MeshVertices = new List<Vector3>();
    private Vector3 m_PlaneCenter = new Vector3();

    private List<Color> m_MeshColors = new List<Color>();

    private List<int> m_MeshIndices = new List<int>();

    private Mesh m_Mesh;

    private MeshRenderer m_MeshRenderer;

	public Material GroundMaterial;

	public Material FloorMaterial;

	public GameObject WallPrefab;

	public GameObject BorderPrefab;

	private List<GameObject> _walls = new List<GameObject>();

	public bool IsGround {
		get {
			var floor = WorldGenerator.Instance.GetGroundPlatform();
			return !floor || floor == this;
		}
	}

	public Vector3 Position {
		get {
			return m_PlaneCenter;
		}
	}

	public int WallCount {
		get {
			return _walls.Count;
		}
	}

	public float heightFromGround;

    /// <summary>
    /// The Unity Awake() method.
    /// </summary>
    public void Awake()
    {
        m_Mesh = GetComponent<MeshFilter>().mesh;
        m_MeshRenderer = GetComponent<UnityEngine.MeshRenderer>();
    }

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	protected void Start()
	{
		WorldGenerator.Instance.AddPlatform(this);
	}
	
	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	void OnDestroy()
	{
		WorldGenerator.Instance.RemovePlatform(this);
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
        if (m_TrackedPlane == null)
        {
            return;
        }
        else if (m_TrackedPlane.SubsumedBy != null)
        {
            Destroy(gameObject);
            return;
        }
        else if (m_TrackedPlane.TrackingState != TrackingState.Tracking)
        {
            m_MeshRenderer.enabled = false;
			foreach(var wall in _walls) wall.GetComponent<Renderer>().enabled = false;
            return;
        }

        m_MeshRenderer.enabled = true;
        foreach (var wall in _walls) wall.GetComponent<Renderer>().enabled = true;

        _UpdateMeshIfNeeded();
		_UpdateHeight();
	}

    public void Initialize(TrackedPlane plane)
    {
        m_TrackedPlane = plane;
		Color c = WorldGenerator.Instance.worldColor;  //k_PlaneColors[s_PlaneCount++ % k_PlaneColors.Length];
		_SetColor(c);

        Update();
    }

	private void _SetColor(Color c)
	{
		if(IsGround) {
			m_MeshRenderer.material = GroundMaterial;
            /*m_MeshRenderer.material.SetColor("_EmissionColor", c);
            c.a = WorldGenerator.Instance.worldOpacity;
            m_MeshRenderer.material.color = c;*/
            m_MeshRenderer.material.SetColor("_MKGlowColor", c);
            c.a = WorldGenerator.Instance.worldOpacity;
            m_MeshRenderer.material.SetColor("_Color", c);
		} else {
			m_MeshRenderer.material = FloorMaterial;
            m_MeshRenderer.material.SetColor("_MKGlowColor", c);
            c.a = WorldGenerator.Instance.worldOpacity;
            m_MeshRenderer.material.SetColor("_Color", c);
		}
	}

	private Color _GetColor()
	{
		if(IsGround) return m_MeshRenderer.material.color;
		return m_MeshRenderer.material.GetColor("_Color");
	}

	private Color _GetEmissionColor()
	{
        if (IsGround) return m_MeshRenderer.material.GetColor("_EmissionColor");
        return m_MeshRenderer.material.GetColor("_MKGlowColor");
	}

	private void _UpdateHeight()
	{
        WorldPlatform ground = WorldGenerator.Instance.GetGroundPlatform();

		//if not ground
        if (ground && ground != this)
		{
			var height = (m_PlaneCenter.y - ground.m_PlaneCenter.y);
			var heightDelta = Mathf.Abs(height - heightFromGround);
			heightFromGround = height;

			//compare height with previous height and update
			if(heightDelta >= 0.05f) {
				foreach(var wall in _walls)
				{
					//update wall height
					Mesh m = wall.GetComponent<MeshFilter>().mesh;
					Vector3[] vertices = m.vertices;
					vertices[2].y = m_PlaneCenter.y - heightFromGround;
					vertices[3].y = m_PlaneCenter.y - heightFromGround;
					m.vertices = vertices;

					m.RecalculateBounds();
					m.RecalculateNormals();
				}
			}
		}
        
		//if ground
		else 
		{
			while(_walls.Count > 0)
			{
				Destroy(_walls[0]);
				_walls.RemoveAt(0);
			}
		}
	}

    /// <summary>
    /// Update mesh with a list of Vector3 and plane's center position.
    /// </summary>
    private void _UpdateMeshIfNeeded()
    {
        m_TrackedPlane.GetBoundaryPolygon(m_MeshVertices);

        if (_AreVerticesListsEqual(m_PreviousFrameMeshVertices, m_MeshVertices))
        {
            return;
        }

        m_PreviousFrameMeshVertices.Clear();
        m_PreviousFrameMeshVertices.AddRange(m_MeshVertices);

        m_PlaneCenter = m_TrackedPlane.CenterPose.position;

        int planePolygonCount = m_MeshVertices.Count;

        m_Mesh.Clear();

		//convert vertices to 2d vertices
		List<Vector2> vertices2D = new List<Vector2>(m_MeshVertices.Count);
		foreach(var vertex in m_MeshVertices) {
			vertices2D.Add(new Vector2(vertex.x, vertex.z));
		}

		m_MeshIndices = new List<int>(new Triangulator(vertices2D.ToArray()).Triangulate());

        //update vertices colors
        m_MeshColors.Clear();

        for (int i = 0; i < m_MeshVertices.Count; ++i)
        {
            m_MeshColors.Add(new Color(1f, 1f, 1f, 0.75f));
        }

        //let's build the new mesh
        m_Mesh.SetVertices(m_MeshVertices);
        m_Mesh.SetIndices(m_MeshIndices.ToArray(), MeshTopology.Triangles, 0);
        m_Mesh.SetColors(m_MeshColors);

		m_Mesh.RecalculateNormals();
		m_Mesh.RecalculateBounds();

		//uvs
		_UpdateUVs(ref m_Mesh);

        //update collider
        MeshCollider mc = gameObject.GetComponent<MeshCollider>();
		mc.sharedMesh = null;
        mc.sharedMesh = m_Mesh;

		//compute height
        heightFromGround = 0f;
        WorldPlatform ground = WorldGenerator.Instance.GetGroundPlatform();
        if(!IsGround) heightFromGround = (m_PlaneCenter.y - ground.m_PlaneCenter.y);
		else return;
		
        //now we need to create walls of the platform
		int expectedWalls = planePolygonCount;

		while(_walls.Count > expectedWalls)
		{
			Destroy(_walls[_walls.Count-1]);
			_walls.RemoveAt(_walls.Count-1);
		}

		for(int i = 0 ; i < planePolygonCount; i++)
		{
			if(_walls.Count < i+1)
			{
                _walls.Add(Instantiate(WallPrefab, transform));
			}

			GameObject wall = _walls[i];

			//get wall mesh
			Mesh wallMesh = wall.GetComponent<MeshFilter>().mesh;

			//copy renderer settings to wall
			Renderer wallMeshRenderer = wall.GetComponent<Renderer>();
            wallMeshRenderer.material.SetColor("_Color", _GetColor());
            wallMeshRenderer.material.SetColor("_MKGlowColor", _GetEmissionColor());

			//we get the two vertices that draw the current edge to the polygon
			Vector3 vertex1 = m_MeshVertices[i], vertex2 = m_MeshVertices[i == planePolygonCount - 1 ?  0 : i+1];

			//first, we create the two vertices on the ground
			Vector3 vertex3 = new Vector3(vertex1.x, m_PlaneCenter.y - heightFromGround, vertex1.z);
			Vector3 vertex4 = new Vector3(vertex2.x, m_PlaneCenter.y - heightFromGround, vertex2.z);

			//Clear the mesh
			wallMesh.Clear();

			//set mesh vertices
			Vector3[] wallVertices = new Vector3[4];
			wallVertices[0] = vertex1;
			wallVertices[1] = vertex2;
			wallVertices[2] = vertex3;
			wallVertices[3] = vertex4;
			
			wallMesh.SetVertices(new List<Vector3>(wallVertices));

			//we need to create two triangles to create the wall quad
			int[] wallTriangles = new int[6];
			wallTriangles[0] = 0;
			wallTriangles[1] = 1;
			wallTriangles[2] = 2;

			wallTriangles[3] = 2;
			wallTriangles[4] = 1;
			wallTriangles[5] = 3;

			wallMesh.SetIndices(wallTriangles, MeshTopology.Triangles, 0);

			wallMesh.FlipNormals();

			//set colors of each vertex to same color as platform mesh
			List<Color> wallColors = new List<Color>(new Color[] {
                new Color(1f, 1f, 1f, WorldGenerator.Instance.worldOpacity),
                new Color(1f, 1f, 1f, WorldGenerator.Instance.worldOpacity),
                new Color(1f, 1f, 1f, WorldGenerator.Instance.worldOpacity),
                new Color(1f, 1f, 1f, WorldGenerator.Instance.worldOpacity)
			});

			wallMesh.SetColors(wallColors);

			wallMesh.RecalculateNormals();
			wallMesh.RecalculateBounds();

			wallMesh.uv = new Vector2[] {
				new Vector2(0f,0f),
				new Vector2(1f,0f),
				new Vector2(0f,1f),
				new Vector2(1f,1f)
			};

            //update collider
            /*MeshCollider wallCollider = gameObject.GetComponent<MeshCollider>();
			wallCollider.sharedMesh = null;
            wallCollider.sharedMesh = wallMesh;*/

			//generate borders
			//_GenerateWallBorders(wall);
		}
    }

	private void _GenerateWallBorders(GameObject wall)
	{
		//first clear all previous borders
		IEnumerable<Transform> children = wall.transform.GetComponentsInChildren<Transform>().Where(x => x.gameObject != wall);
		foreach(var c in children) Destroy(c.gameObject);

		Mesh wallMesh = wall.GetComponent<MeshFilter>().mesh;

		for(var v = 0 ; v < wallMesh.vertices.Length ; v++)
		{
			Vector3 v1 = wallMesh.vertices[v];
			Vector3 v2 = wallMesh.vertices[v == wallMesh.vertices.Length-1 ? 0 : v+1];

			Vector3 edgeDirection = v2 - v1;
			Vector3 sideFaceDirection = Quaternion.AngleAxis(90f, Vector3.up) * edgeDirection;

			//first side face
			Vector3 p1_1 = v1 + Vector3.up * WorldGenerator.Instance.platformBorderThickness;
			Vector3 p2_1 = v1 - sideFaceDirection * WorldGenerator.Instance.platformBorderThickness;
			Vector3 p3_1 = v1 + Vector3.down * WorldGenerator.Instance.platformBorderThickness;
			Vector3 p4_1 = v1 + sideFaceDirection * WorldGenerator.Instance.platformBorderThickness;

            //second side face
            Vector3 p1_2 = v2 + Vector3.up * WorldGenerator.Instance.platformBorderThickness;
            Vector3 p2_2 = v2 - sideFaceDirection * WorldGenerator.Instance.platformBorderThickness;
            Vector3 p3_2 = v2 + Vector3.down * WorldGenerator.Instance.platformBorderThickness;
            Vector3 p4_2 = v2 + sideFaceDirection * WorldGenerator.Instance.platformBorderThickness;

			//now we build the border
			GameObject border = Instantiate(BorderPrefab, wall.transform);

			border.GetComponent<Renderer>().material.color = wall.GetComponent<Renderer>().material.GetColor("_Color");
			border.GetComponent<Renderer>().material.SetColor("_EmissionColor", wall.GetComponent<Renderer>().material.GetColor("_MKGlowColor"));

			Mesh borderMesh = border.GetComponent<MeshFilter>().mesh;
			borderMesh.Clear();

			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			List<Color> colors = new List<Color>();

			//create side edges
			vertices.Add(p1_1);
			vertices.Add(p2_1);
			vertices.Add(p3_1);
			vertices.Add(p4_1);

			triangles.Add(0); //p1 1
			triangles.Add(1); //p2 1
			triangles.Add(2); //p3 1
			triangles.Add(3); //p4 1

            vertices.Add(p1_2);
            vertices.Add(p2_2);
            vertices.Add(p3_2);
            vertices.Add(p4_2);

            triangles.Add(4); //p1 2
            triangles.Add(5); //p2 2
            triangles.Add(6); //p3 2
            triangles.Add(7); //p4 2

			//join side edges

			//p1_1, p2_1, p1_2, p2_2
			triangles.Add(0);
			triangles.Add(1);
			triangles.Add(4);
			triangles.Add(5);

            //p2_1, p3_1, p2_2, p3_2
            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(5);
            triangles.Add(6);

            //p3_1, p4_1, p3_2, p4_2
            triangles.Add(2);
            triangles.Add(3);
            triangles.Add(6);
            triangles.Add(7);

            //p4_1, p1_1, p4_2, p1_2
            triangles.Add(3);
            triangles.Add(0);
            triangles.Add(7);
            triangles.Add(4);

			foreach(var vt in vertices) colors.Add(new Color(1f, 1f, 1f, WorldGenerator.Instance.worldOpacity));

			borderMesh.SetVertices(vertices);
			borderMesh.SetIndices(triangles.ToArray(), MeshTopology.Quads, 0);
			borderMesh.SetColors(colors);
			borderMesh.RecalculateNormals();
			borderMesh.RecalculateBounds();
		}
	}

	private void _UpdateUVs(ref Mesh m)
	{
        Vector2[] uvs = new Vector2[m.vertices.Length];
        int v = 0;
        while (v < uvs.Length)
        {
            if (Mathf.Abs(m.normals[v].y) > 0.5f)
            {
                // if normal is like vector3.up
                uvs[v] = new Vector2(m.vertices[v].x, m.vertices[v].z);
            }
            else if (Mathf.Abs(m.normals[v].x) > 0.5f)
            {
                // if normal is like vector3.right
                uvs[v] = new Vector2(m.vertices[v].z, m.vertices[v].y);
            }
            else
            {
                // last case if it's like vector3.forward
                uvs[v] = new Vector2(m.vertices[v].x, m.vertices[v].y);
            }

            v++;
        }

        m.uv = uvs;
	}

    private bool _AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
    {
        if (firstList.Count != secondList.Count)
        {
            return false;
        }

        for (int i = 0; i < firstList.Count; i++)
        {
            if (firstList[i] != secondList[i])
            {
                return false;
            }
        }

        return true;
    }
}
