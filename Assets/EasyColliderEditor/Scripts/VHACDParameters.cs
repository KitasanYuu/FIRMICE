#if (UNITY_EDITOR && !UNITY_EDITOR_LINUX)
namespace ECE
{
  using UnityEngine;
  using System.Collections.Generic;
  [System.Serializable]
  public class VHACDParameters
  {
    public float NormalExtrudeMultiplier = 0.0f;

    [HideInInspector]
    /// <summary>
    /// is the current calculation for displaying a preview?
    /// </summary>
    public bool IsCalculationForPreview;

    public VHACD_CONVERSION ConvertTo;

    [HideInInspector]
    /// <summary>
    /// the suffix to add to saved convex hulls: objectName_suffix_01 etc.
    /// </summary>
    public string SaveSuffix = "_ConvexHull_";

    /// <summary>
    /// Method to use to attach resulting convex hulls to attach to object.
    /// </summary>
    public VHACD_RESULT_METHOD vhacdResultMethod = VHACD_RESULT_METHOD.AttachTo;

    [HideInInspector]
    /// <summary>
    /// Run VHACD only on the selected vertices?
    /// </summary>
    public bool UseSelectedVertices = false;

    [HideInInspector]
    /// <summary>
    /// Current mesh filter VHACD is calculating.
    /// </summary>
    public int CurrentMeshFilter = 0;

    [HideInInspector]
    /// <summary>
    /// Should child meshes be seperately done in the calculation / adding of convex hulls.
    /// </summary>
    public bool SeparateChildMeshes = false;

    [HideInInspector]
    /// <summary>
    /// should the attach to method be run per mesh that is separated?
    /// </summary>
    public bool PerMeshAttachOverride = false;

    [HideInInspector]
    /// <summary>
    /// List of mesh filters for VHACD calculation.
    /// </summary>
    public List<MeshFilter> MeshFilters = new List<MeshFilter>();

    [HideInInspector]
    /// <summary>
    /// Gameobject to attach mesh colliders to using the result of VHACD.
    /// </summary>
    public GameObject AttachTo;

    [HideInInspector]
    /// <summary>
    /// Save path of current VHACD meshes
    /// </summary>
    public string SavePath;


    [Range(0, 1f)]
    /// <summary>
    /// maximum concavity
    /// </summary>
    public double concavity;

    [Range(0, 1f)]
    /// <summary>
    /// controls bias toward clipping along symmetry planes
    /// </summary>
    public double alpha;

    [Range(0, 1f)]
    /// <summary>
    /// controls bias toward clipping along revolution axes
    /// </summary>
    public double beta;

    [Range(0, 1f)]
    /// <summary>
    /// controls adaptive sampling of the generated convex-hulls
    /// </summary>
    public double minVolumePerCH;

    [Range(10000, 64000000)]
    /// <summary>
    /// maximum number of voxels generated during voxelization stage
    /// </summary>
    public int resolution;

    [Range(4, 1024)]
    /// <summary>
    /// controls maximum number of triangles per convex hull
    /// </summary>
    public int maxNumVerticesPerConvexHull;

    [Range(1, 16)]
    /// <summary>
    /// controls the granularity of the search for the "best" clipping plane
    /// </summary>
    public int planeDownsampling;

    [Range(1, 16)]
    /// <summary>
    /// controls the precision of the convex-hull generation process during the clipping plane selection stage
    /// </summary>
    public int convexhullDownSampling;

    [Range(1, 128)]
    /// <summary>
    /// maximum number of convex hulls
    /// </summary>
    public int maxConvexHulls;

    /// <summary>
    /// When enabled, will project the output convex hull vertices onto the original source mesh
    /// </summary>
    public bool projectHullVertices;

    /// <summary>
    /// Fill mode to determine what is inside/outside the mesh
    /// Flood fill: basic flood fill algorithm
    /// Raycast fill: raycasts are used to determine inside/outside
    /// Surface: used when the surface is to represent a hollow object.
    /// </summary>
    public VHACD_FILL_MODE fillMode;

    /// <summary>
    /// Should we force recalculation when resulting hulls have a hull with triangle count >=256.
    /// </summary>
    public bool forceUnder256Triangles;

    /// <summary>
    /// Creates a VHACD parameters object with default values.
    /// </summary>
    public VHACDParameters()
    {
      concavity = 0.0025;
      alpha = 0.05;
      beta = 0.05;
      minVolumePerCH = 0.0001;
      resolution = 10000;
      maxNumVerticesPerConvexHull = 64;
      planeDownsampling = 4;
      convexhullDownSampling = 4;
      maxConvexHulls = 1;
      projectHullVertices = true;
      fillMode = VHACD_FILL_MODE.FLOOD_FILL;
      forceUnder256Triangles = true;
    }

    /// <summary>
    /// Creates a VHACDParameters object with the values of another VHACDParam
    /// </summary>
    /// <param name="other">Values to copy from</param>
    public VHACDParameters(VHACDParameters other)
    {
      ConvertTo = other.ConvertTo;
      SaveSuffix = other.SaveSuffix;
      vhacdResultMethod = other.vhacdResultMethod;
      AttachTo = other.AttachTo;
      concavity = other.concavity;
      alpha = other.alpha;
      beta = other.beta;
      minVolumePerCH = other.minVolumePerCH;
      resolution = other.resolution;
      maxNumVerticesPerConvexHull = other.maxNumVerticesPerConvexHull;
      planeDownsampling = other.planeDownsampling;
      convexhullDownSampling = other.convexhullDownSampling;
      maxConvexHulls = other.maxConvexHulls;
      projectHullVertices = other.projectHullVertices;
      fillMode = other.fillMode;
      forceUnder256Triangles = other.forceUnder256Triangles;
      SeparateChildMeshes = other.SeparateChildMeshes;
      UseSelectedVertices = other.UseSelectedVertices;
      MeshFilters = new List<MeshFilter>();

      foreach (MeshFilter f in other.MeshFilters)
      {
        MeshFilters.Add(f);
      }
      NormalExtrudeMultiplier = other.NormalExtrudeMultiplier;
      PerMeshAttachOverride = other.PerMeshAttachOverride;
    }

    /// <summary>
    /// Clones the current instance of VHACDParameters.
    /// </summary>
    /// <returns>Copy of the VHACDParameters instance.</returns>
    public VHACDParameters Clone()
    {
      return new VHACDParameters(this);
    }
  }
}
#endif