#if (UNITY_EDITOR && !UNITY_EDITOR_LINUX)
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Linq;
namespace ECE
{

  //Future potential features:
  // The option to covert the convex hulls generated into other colliders, like box colliders (boxelization is a fun term).
  // So that we can leverage VHACD results into alternative basic colliders.

  // New version of vhacd is out but in my testing it is much slower than the older performance branch version
  // additionally, it offers less adjustable parameters, which makes it harder to get a good fit,
  // even though the fit isn't perfect with any vhacd implementation since it's approximate convex decomposition
  // and even though the current parameters aren't particularily understandable, you can adjust them and see the output quickly until something is close enough.

  public class EasyColliderVHACD
  {
    // If you're getting an error about DLL not found exception, you'll likely be pointed to this line by the error
    // near the top of this file you'll find " const string dllName = "ECE_VHACD"; " ,
    // this is where the dll name is placed (these same comments are there as well)
    // To fix this on Mac: sometimes it can not recognize the .bundle, try either "ECE_VHACD.bundle" or "ECE_VHACD" as the dllName string
    // Occasionally this can also happen when the dll/bundles are updated, and imported into unity after they were already used
    // to correctly update when vhacd is updated, be sure to close unity, and immediately update the asset on opening the project
    // currently only supports windows, and OSX (arm / silicon and intel) versions of unity.
    const string dllName = "ECE_VHACD";

    [DllImport(dllName, EntryPoint = "GetMaxNumVerticesPerCH")]
    private static extern uint GetMaxNumVerticesPerCH();

    // extern "C" VHACD_API double* GetConvexHullCenter();
    [DllImport(dllName, EntryPoint = "GetConvexHullCenter")]
    private static extern IntPtr GetConvexHullCenter();

    // extern "C" VHACD_API unsigned int* GetConvexHullTriangles();
    [DllImport(dllName, EntryPoint = "GetConvexHullTriangles")]
    private static extern IntPtr GetConvexHullTriangles();

    // extern "C" VHACD_API double* GetConvexHullPoints();
    [DllImport(dllName, EntryPoint = "GetConvexHullPoints")]
    private static extern IntPtr GetConvexHullPoints();

    // extern "C" VHACD_API void Compute();
    [DllImport(dllName, EntryPoint = "Compute")]
    private static extern void Compute();

    // extern "C" VHACD_API bool Create();
    [DllImport(dllName, EntryPoint = "Create")]
    private static extern bool Create();

    // extern "C" VHACD_API bool Create_ASYNC();
    [DllImport(dllName, EntryPoint = "Create_ASYNC")]
    private static extern bool Create_ASYNC();

    // extern "C" VHACD_API void Destroy();
    [DllImport(dllName, EntryPoint = "Destroy")]
    private static extern void Destroy();

    // extern "C" VHACD_API int GetConvexHullNumPoints();
    [DllImport(dllName, EntryPoint = "GetConvexHullNumPoints")]
    private static extern int GetConvexHullNumPoints();

    // extern "C" VHACD_API int GetConvexHullNumTriangles();
    [DllImport(dllName, EntryPoint = "GetConvexHullNumTriangles")]
    private static extern int GetConvexHullNumTriangles();

    // extern "C" VHACD_API double GetConvexHullPoint(int index);
    [DllImport(dllName, EntryPoint = "GetConvexHullPoint")]
    private static extern double GetConvexHullPoint(int index);

    // extern "C" VHACD_API int GetConvexHullTriangle(int index);
    [DllImport(dllName, EntryPoint = "GetConvexHullTriangle")]
    private static extern int GetConvexHullTriangle(int index);

    // extern "C" VHACD_API double GetConvexHullVolume();
    [DllImport(dllName, EntryPoint = "GetConvexHullVolume")]
    private static extern double GetConvexHullVolume();

    // extern "C" VHACD_API int GetNumberOfConvexHulls();
    [DllImport(dllName, EntryPoint = "GetNumberOfConvexHulls")]
    private static extern int GetNumberOfConvexHulls();

    // extern "C" VHACD_API int GetPointSize();
    [DllImport(dllName, EntryPoint = "GetPointSize")]
    private static extern int GetPointSize();

    // extern "C" VHACD_API int GetPointSize();
    [DllImport(dllName, EntryPoint = "IsReady")]
    private static extern bool IsReady();

    // extern "C" VHACD_API void SetConvexHull(int index);
    [DllImport(dllName, EntryPoint = "SetConvexHull")]
    private static extern void SetConvexHull(int index);

    // extern "C" VHACD_API void SetMaxHulls(int value);
    [DllImport(dllName, EntryPoint = "SetMaxHulls")]
    private static extern void SetMaxHulls(int value);

    // extern "C" VHACD_API void SetMaxVerticesPerHull(int value);
    [DllImport(dllName, EntryPoint = "SetMaxVerticesPerHull")]
    private static extern void SetMaxVerticesPerHull(int value);

    // For branch PERFORMANCE ENHANCEMENTS
    // extern "C" VHACD_API void SetParameters(
    // 	double concavity,
    // 	double alpha,
    // 	double beta,
    // 	double minVolumePerConvexHull,
    // 	int resolution,
    // 	int maxNumVerticesPerCH,
    // 	int planeDownsampling,
    // 	int convexhullDownsampling,
    // 	int maxConvexHulls,
    // 	bool projectHullVertices,
    // 	unsigned int fillMode);
    [DllImport(dllName, EntryPoint = "SetParameters")]
    private static extern void SetParameters(
      double concavity,
      double alpha,
      double beta,
      double minVolumePerConvexHull,
      int resolution,
      int maxNumVerticesPerCH,
      int planeDownsampling,
      int convexhullDownsampling,
      int maxConvexHulls,
      bool projectHullVertices,
      uint fillMode
    );

    // extern "C" VHACD_API void SetPoint(int index, float value);
    [DllImport(dllName, EntryPoint = "SetPoint")]
    private static extern void SetPoint(int index, float value);

    // extern "C" VHACD_API void SetPoints(float pointsArr[], int size);
    [DllImport(dllName, EntryPoint = "SetPoints")]
    private static extern void SetPoints(float[] pointsArr, int size);

    // extern "C" VHACD_API void SetPointSize(int size);
    [DllImport(dllName, EntryPoint = "SetPointSize")]
    private static extern void SetPointSize(int size);

    // extern "C" VHACD_API void SetResolution(int value);
    [DllImport(dllName, EntryPoint = "SetResolution")]
    private static extern void SetResolution(int value);

    // extern "C" VHACD_API void SetTriangle(int index, int value);
    [DllImport(dllName, EntryPoint = "SetTriangle")]
    private static extern void SetTriangle(int index, int value);

    // extern "C" VHACD_API void SetTriangles(int trianglesArr[], int size);
    [DllImport(dllName, EntryPoint = "SetTriangles")]
    private static extern void SetTriangles(int[] trianglesArr, int size);

    // extern "C" VHACD_API void SetTriangleSize(int size);
    [DllImport(dllName, EntryPoint = "SetTriangleSize")]
    private static extern void SetTriangleSize(int size);


    /// <summary>
    /// Is an instance of vhacd initialized?
    /// </summary>
    /// this is a fix for the new version of vhacd which is not used yet for various reasons.
    private bool isInitialized = false;

    /// <summary>
    /// Initializes a VHACD instance
    /// </summary>
    /// <param name="async">Use async process?</param>
    /// <returns></returns>
    public bool Init(bool async = true)
    {
      Clean();
      if (async)
      {
        // fixes an issue in new vhacd without changing the api.
        isInitialized = true;
        // If you're getting an error about DLL not found exception, you'll likely be pointed to this line by the error
        // near the top of this file you'll find " const string dllName = "ECE_VHACD"; " ,
        // this is where the dll name is placed (these same comments are there as well)
        // To fix this on Mac: sometimes it can not recognize the .bundle, try either "ECE_VHACD.bundle" or "ECE_VHACD" as the dllName string
        // Occasionally this can also happen when the dll/bundles are updated, and imported into unity after they were already used
        // to correctly update when vhacd is updated, be sure to close unity, and immediately update the asset on opening the project
        // currently only supports windows, and OSX (arm / silicon and intel) versions of unity.
        return Create_ASYNC();
      }
      else
      {
        isInitialized = true;
        return Create();
      }
    }

    /// <summary>
    /// Destroys the VHACD instance, if it is already initialized.
    /// </summary>
    /// <returns>true if destroyed, false if not destroyed(because it wasn't initialized yet)</returns>
    public bool Clean()
    {
      // fixes issue where vhacd isn't initialized and tries to destroy itself (in new vhacd)
      if (isInitialized)
      {
        // Debug.Log("Clean" + dllName);
        isInitialized = false;
        Destroy();
        return true;
      }
      else
      {
        return false;
      }
    }

    /// <summary>
    /// Gets all calculated convex hulls and turn them into meshes.
    /// </summary>
    /// <returns>convex hull meshes</returns>
    public Mesh[] CreateConvexHullMeshes()
    {
      int numHulls = GetNumberOfConvexHulls();
      Mesh[] meshes = new Mesh[numHulls];
      for (int i = 0; i < numHulls; i++)
      {
        // get the current convex hull
        SetConvexHull(i);
        // get the ch data
        int pointCount = GetConvexHullNumPoints();
        int triangleCount = GetConvexHullNumTriangles();
        // create new vertex and triangles array.
        Vector3[] vertices = new Vector3[pointCount];
        int[] triangles = new int[triangleCount * 3];
        for (int j = 0; j < triangleCount * 3; j++)
        {
          triangles[j] = GetConvexHullTriangle(j);
        }
        // assing each point to a vertex in the array
        Vector3 point = Vector3.zero;
        for (int j = 0; j < pointCount * 3; j += 3)
        {
          // note that in VHACD, each vertex is not a vector.
          // instead each vertex is simply the 3 values in order.
          point.x = (float)GetConvexHullPoint(j);
          point.y = (float)GetConvexHullPoint(j + 1);
          point.z = (float)GetConvexHullPoint(j + 2);
          vertices[j / 3] = point;
        }
        // create and save the mesh.
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        // be sure to add it to our meshes array
        meshes[i] = mesh;
      }
      //return the array of meshes so they can be added as convex mesh colliders.
      return meshes;
    }

    /// <summary>
    /// Gets the number of convex hulls calcalated
    /// </summary>
    /// <returns># convex hulls computed</returns>
    public int GetConvexHullCount()
    {
      return GetNumberOfConvexHulls();
    }

    /// <summary>
    /// Checks if the async computation of convex hulls is complete
    /// </summary>
    /// <returns>true if finished</returns>
    public bool IsComputeFinished()
    {
      return IsReady();
    }

    /// <summary>
    /// Checks to see if each convex hull is under 256 vertices and 256 triangles.
    /// </summary>
    /// <returns>True if under limits</returns>
    public bool IsValid()
    {
      // calculate max triangle count and max vertex count
      int maxTriangleCount = 0;
      int maxVertexCount = 0;
      int numHulls = GetConvexHullCount();
      for (int i = 0; i < numHulls; i++)
      {
        SetConvexHull(i);
        int currentTriCount = GetConvexHullNumTriangles();
        int currentVertCount = GetConvexHullNumPoints();
        if (currentTriCount > maxTriangleCount)
        {
          maxTriangleCount = currentTriCount;
        }
        if (currentVertCount > maxVertexCount)
        {
          maxVertexCount = currentVertCount;
        }
      }
      // if both are under 256, unity will generate no errors.
      // (these errors are hidden in some older versions of unity.)
      if (maxVertexCount < 256 && maxTriangleCount < 256)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Sets the vertices and triangles array for VHACD from mesh
    /// </summary>
    /// <param name="meshFilter">mesh filter of the source mesh</param>
    /// <param name="attachTo">transform the mesh collider will be attached to</param>
    /// <returns>true if preparation succeeds, false otherwise</returns>
    public bool PrepareMeshData(MeshFilter meshFilter, Transform attachTo, Mesh mesh)
    {
      if (mesh == null) return false;
      // convert from meshes' object to world space, to attach to's local space, then calculate, then add.
      Vector3[] vertices = mesh.vertices;
      int[] triangles = mesh.triangles;
      List<Vector3> localAttachVertices = new List<Vector3>();
      if (meshFilter != null && meshFilter.transform != attachTo && meshFilter.sharedMesh != null && meshFilter.sharedMesh == mesh)
      {
        foreach (Vector3 v in vertices)
        {
          // transform from mesh filters local space, to world space, to the attach to's local space.
          localAttachVertices.Add(attachTo.transform.InverseTransformPoint(meshFilter.transform.TransformPoint(v)));
        }
      }
      else
      {
        localAttachVertices = vertices.ToList();
      }
      // could have used an intptr but this way is fast enough
      // and we can let VHACD take care of itself.
      SetPointSize(localAttachVertices.Count * 3);
      SetTriangleSize(triangles.Length);
      for (int i = 0; i < localAttachVertices.Count; i++)
      {
        SetPoint(i * 3, localAttachVertices[i].x);
        SetPoint(i * 3 + 1, localAttachVertices[i].y);
        SetPoint(i * 3 + 2, localAttachVertices[i].z);
      }
      for (int i = 0; i < triangles.Length; i++)
      {
        SetTriangle(i, triangles[i]);
      }
      return true;
    }

    /// <summary>
    /// prepares an array of meshfilters for a single convex hull decomposition.
    /// </summary>
    /// <param name="meshFilters">array of meshfilters</param>
    public bool PrepareMeshData(List<MeshFilter> meshFilters, Transform attachTo)
    {
      // convert vertices from the mesh filters local space, to world space, to the attach to's local space.
      int vertexCount = 0;
      int triangleCount = 0;
      List<Vector3> localAttachVertices = new List<Vector3>();
      List<int> triangles = new List<int>();
      foreach (MeshFilter mf in meshFilters)
      {
        // skip any mesh filters that were deleted, or had their shared mesh changed to null
        if (mf == null || mf.sharedMesh == null) continue;
        // convert to local vertices of the attach to object.
        Vector3[] verts = mf.sharedMesh.vertices;
        foreach (Vector3 v in verts)
        {
          localAttachVertices.Add(attachTo.transform.InverseTransformPoint(mf.transform.TransformPoint(v)));
        }
        // get current shared mesh triangles
        int[] tris = mf.sharedMesh.triangles;
        for (int i = 0; i < tris.Length; i++)
        {
          // add the vertex index, plus the offset of the total running vertex count.
          triangles.Add(tris[i] + vertexCount);
        }
        vertexCount += mf.sharedMesh.vertices.Length;
        triangleCount += mf.sharedMesh.triangles.Length;
      }
      // could have used intptr etc. to pass all the data at once
      // but this way is still fast enough, and we can just let VHACD handle it's own stuff.
      SetPointSize(vertexCount * 3);
      SetTriangleSize(triangleCount);
      for (int i = 0; i < localAttachVertices.Count; i++)
      {
        SetPoint(i * 3, localAttachVertices[i].x);
        SetPoint(i * 3 + 1, localAttachVertices[i].y);
        SetPoint(i * 3 + 2, localAttachVertices[i].z);
      }
      for (int i = 0; i < triangles.Count; i++)
      {
        SetTriangle(i, triangles[i]);
      }
      return true;
    }

    /// <summary>
    /// Recalculates the current convex hulls based on max triangles and vertices with an aim to get max triangles below 256
    /// </summary>
    /// <returns></returns>
    public bool RecomputeVHACD()
    {
      int maxTriangleCount = 0;
      int maxVertexCount = 0;
      int numHulls = GetConvexHullCount();
      // calculate max triangles and vertices from convex hulls.
      for (int i = 0; i < numHulls; i++)
      {
        SetConvexHull(i);
        int currentTriCount = GetConvexHullNumTriangles();
        int currentVertCount = GetConvexHullNumPoints();
        if (currentTriCount > maxTriangleCount)
        {
          maxTriangleCount = currentTriCount;
        }
        if (currentVertCount > maxVertexCount)
        {
          maxVertexCount = currentVertCount;
        }
      }
      // reduce the max number of vertices.
      float trisPerVertMax = (float)maxTriangleCount / maxVertexCount;
      int maxVerticesPerConvexHull = (int)(255 / trisPerVertMax);
      // set the new max number of vertices
      SetMaxVerticesPerHull(maxVerticesPerConvexHull);
      // compute again.
      Compute();
      return true;
    }

    /// <summary>
    /// Calls compute method on the current VHACD instance
    /// </summary>
    /// <returns></returns>
    public bool RunVHACD()
    {
      Compute();
      return true;
    }

    /// <summary>
    /// Sets parameters on the current VHACD instance
    /// </summary>
    /// <param name="parameters">parameters to set</param>
    public bool SetParameters(VHACDParameters parameters)
    {
      SetParameters(
        parameters.concavity,
        parameters.alpha,
        parameters.beta,
        parameters.minVolumePerCH,
        parameters.resolution,
        parameters.maxNumVerticesPerConvexHull,
        parameters.planeDownsampling,
        parameters.convexhullDownSampling,
        parameters.maxConvexHulls,
        parameters.projectHullVertices,
        (uint)parameters.fillMode
      );
      return true;
    }
  }

  /// <summary>
  /// Fill mode for VHACD
  /// </summary>
  public enum VHACD_FILL_MODE
  {
    FLOOD_FILL,
    SURFACE_ONLY,
    RAYCAST_FILL,
  }
}
#endif