using System;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class ParallaxManager : MonoBehaviour
{
    [Serializable]
    private sealed class ParallaxLayerGroup
    {
        [SerializeField] private Transform[] layers = Array.Empty<Transform>();

        [Tooltip("0 keeps the layer in world space. 1 makes it follow the camera completely.")]
        [Range(0f, 1f)]
        [SerializeField] private float cameraFollow;

        private Vector3[] initialPositions;

        public ParallaxLayerGroup(float cameraFollow)
        {
            this.cameraFollow = cameraFollow;
        }

        public void CacheInitialPositions()
        {
            layers ??= Array.Empty<Transform>();
            initialPositions = new Vector3[layers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] != null)
                {
                    initialPositions[i] = layers[i].position;
                }
            }
        }

        public void Apply(Vector3 cameraOffset, Vector2 movementAxes)
        {
            if (initialPositions == null || initialPositions.Length != layers.Length)
            {
                CacheInitialPositions();
            }

            Vector3 movement = new Vector3(
                cameraOffset.x * movementAxes.x,
                cameraOffset.y * movementAxes.y,
                0f) * cameraFollow;

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] != null)
                {
                    layers[i].position = initialPositions[i] + movement;
                }
            }
        }
    }

    [Header("References")]
    [SerializeField] private Transform targetCamera;

    [Header("Depth Groups")]
    [SerializeField] private ParallaxLayerGroup backgroundLayer = new ParallaxLayerGroup(0.8f);


    [SerializeField] private ParallaxLayerGroup farLayer = new ParallaxLayerGroup(0.6f);

    [SerializeField] private ParallaxLayerGroup middleLayer = new ParallaxLayerGroup(0.3f);

    [SerializeField] private ParallaxLayerGroup nearLayer = new ParallaxLayerGroup(0.1f);

    [Header("Movement")]
    [SerializeField] private Vector2 movementAxes = Vector2.one;

    private Vector3 initialCameraPosition;

    private void Awake()
    {
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }

        CacheInitialState();
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 cameraOffset = targetCamera.position - initialCameraPosition;
        backgroundLayer.Apply(cameraOffset, movementAxes);
        farLayer.Apply(cameraOffset, movementAxes);
        middleLayer.Apply(cameraOffset, movementAxes);
        nearLayer.Apply(cameraOffset, movementAxes);
    }

    private void CacheInitialState()
    {
        if (targetCamera == null)
        {
            return;
        }

        initialCameraPosition = targetCamera.position;
        backgroundLayer.CacheInitialPositions();
        farLayer.CacheInitialPositions();
        middleLayer.CacheInitialPositions();
        nearLayer.CacheInitialPositions();
    }
}
