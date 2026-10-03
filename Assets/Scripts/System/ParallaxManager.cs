using UnityEngine;

[DefaultExecutionOrder(1000)]
public class ParallaxManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetCamera;
    [SerializeField] private Transform[] layers;

    [Header("Parallax")]
    [Tooltip("Layers at or in front of this Z position stay fixed in world space.")]
    [SerializeField] private float stationaryDepth = 10f;

    [Tooltip("Layers at this Z position receive the maximum camera-follow amount.")]
    [SerializeField] private float fullParallaxDepth = 30f;

    [Tooltip("How much the farthest layer follows the camera. Keep this below 1.")]
    [Range(0f, 0.95f)]
    [SerializeField] private float maxCameraFollow = 0.8f;

    [Tooltip("Set an axis to 0 if parallax should not be applied on that axis.")]
    [SerializeField] private Vector2 movementAxes = Vector2.one;

    private Vector3 initialCameraPosition;
    private Vector3[] initialLayerPositions;
    private float[] cameraFollowFactors;

    private void Awake()
    {
        if (targetCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                targetCamera = mainCamera.transform;
            }
        }

        CacheInitialState();
    }

    private void LateUpdate()
    {
        if (targetCamera == null || initialLayerPositions == null)
        {
            return;
        }

        Vector3 cameraOffset = targetCamera.position - initialCameraPosition;

        for (int i = 0; i < layers.Length; i++)
        {
            Transform layer = layers[i];
            if (layer == null)
            {
                continue;
            }

            Vector3 position = initialLayerPositions[i];
            position.x += cameraOffset.x * movementAxes.x * cameraFollowFactors[i];
            position.y += cameraOffset.y * movementAxes.y * cameraFollowFactors[i];
            layer.position = position;
        }
    }

    private void CacheInitialState()
    {
        if (targetCamera == null)
        {
            initialLayerPositions = null;
            cameraFollowFactors = null;
            return;
        }

        initialCameraPosition = targetCamera.position;
        layers ??= System.Array.Empty<Transform>();
        initialLayerPositions = new Vector3[layers.Length];
        cameraFollowFactors = new float[layers.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            Transform layer = layers[i];
            if (layer == null)
            {
                continue;
            }

            Vector3 layerPosition = layer.position;
            initialLayerPositions[i] = layerPosition;

            float depthRatio = Mathf.InverseLerp(
                stationaryDepth,
                fullParallaxDepth,
                layerPosition.z);

            cameraFollowFactors[i] = depthRatio * maxCameraFollow;
        }
    }

    private void OnValidate()
    {
        if (fullParallaxDepth <= stationaryDepth)
        {
            fullParallaxDepth = stationaryDepth + 0.01f;
        }
    }
}
