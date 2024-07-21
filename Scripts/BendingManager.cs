using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BendingManager : MonoBehaviour
{
    public float BendDistance = 20f;
    public float CurveDistance = 5f;
    [Range(0.0f, 45f)]
    public float BendAngle = 30f;
    public bool EnableBend = true;
    public float ClipHeight = 10f;
    public Color CrossSectionColor = Color.black;
    public float extraCullingSpace = 10f;

    [SerializeField]
    private List<Material> _materials = new List<Material>();
    public Camera bendCamera;

    [Tooltip("Path to the folder containing materials, relative to Assets/")]
    public string materialsFolderPath = "Materials/BendMaterials";

    private static readonly int DistanceID = Shader.PropertyToID("_Distance");
    private static readonly int CurveDistanceID = Shader.PropertyToID("_CurveDistance");
    private static readonly int BendAngleID = Shader.PropertyToID("_BendAngle");
    private static readonly int EnableBendID = Shader.PropertyToID("_EnableBend");
    private static readonly int BendCameraPosID = Shader.PropertyToID("_BendCameraPos");
    private static readonly int BendCameraForwardID = Shader.PropertyToID("_BendCameraForward");
    private static readonly int ClipHeightID = Shader.PropertyToID("_ClipHeight");
    private static readonly int CrossSectionColorID = Shader.PropertyToID("_CrossSectionColor");

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        LoadMaterialsFromFolder();
        UpdateBendProperties();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

        foreach (Material mat in _materials)
        {
            if (mat != null)
            {
                mat.SetFloat(EnableBendID, 0);
            }
        }
    }

    private void Update()
    {
        UpdateBendProperties();
    }

    private void LoadMaterialsFromFolder()
    {
        _materials.Clear();

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/" + materialsFolderPath });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material != null)
            {
                _materials.Add(material);
            }
        }
        Debug.Log($"Loaded {_materials.Count} materials from folder: {materialsFolderPath}");
#else
        Debug.LogWarning("Material folder loading is only supported in the Unity Editor.");
#endif
    }

    private void UpdateBendProperties()
    {
        if (bendCamera == null)
        {
            Debug.LogWarning("Bend Camera is not assigned in BendingManager.");
            return;
        }

        foreach (Material mat in _materials)
        {
            if (mat != null)
            {
                mat.SetFloat(DistanceID, BendDistance);
                mat.SetFloat(CurveDistanceID, CurveDistance);
                mat.SetFloat(BendAngleID, BendAngle);
                mat.SetFloat(EnableBendID, EnableBend ? 1 : 0);
                mat.SetVector(BendCameraPosID, bendCamera.transform.position);
                mat.SetVector(BendCameraForwardID, bendCamera.transform.forward);
                mat.SetFloat(ClipHeightID, ClipHeight);
                mat.SetColor(CrossSectionColorID, CrossSectionColor);
            }
        }
    }

    private void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (!EnableBend || bendCamera == null) return;

        float maxBendTan = Mathf.Tan(Mathf.Deg2Rad * BendAngle);
        float maxBendHeight = maxBendTan * (bendCamera.farClipPlane - BendDistance);

        float frustumSize = Mathf.Max(bendCamera.orthographicSize, bendCamera.orthographicSize + maxBendHeight + extraCullingSpace);
        float aspect = bendCamera.aspect;

        Matrix4x4 customProjection = Matrix4x4.Ortho(
            -frustumSize * aspect, frustumSize * aspect,
            -frustumSize, frustumSize,
            bendCamera.nearClipPlane, bendCamera.farClipPlane + maxBendHeight + extraCullingSpace
        );

        bendCamera.cullingMatrix = customProjection * bendCamera.worldToCameraMatrix;
    }

    private void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (bendCamera != null)
        {
            bendCamera.ResetCullingMatrix();
        }
    }

    // Optional: Method to manually reload materials from the folder
    public void ReloadMaterialsFromFolder()
    {
        LoadMaterialsFromFolder();
        UpdateBendProperties();
    }

    // Optional: Methods to manually add or remove materials
    public void AddMaterial(Material material)
    {
        if (material != null && !_materials.Contains(material))
        {
            _materials.Add(material);
            UpdateBendProperties();
        }
    }

    public void RemoveMaterial(Material material)
    {
        if (_materials.Remove(material))
        {
            UpdateBendProperties();
        }
    }

    public void ClearMaterials()
    {
        _materials.Clear();
    }
}