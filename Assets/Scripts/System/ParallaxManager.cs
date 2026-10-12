using System;
using System.Collections;
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
        private bool[] movableLayers;

        public ParallaxLayerGroup(float cameraFollow)
        {
            this.cameraFollow = cameraFollow;
        }

        public void CacheInitialPositions()
        {
            layers ??= Array.Empty<Transform>();
            initialPositions = new Vector3[layers.Length];
            movableLayers = new bool[layers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] != null)
                {
                    // 물리 오브젝트를 패럴랙스로 이동시키면 바닥 충돌체까지 움직일 수 있습니다.
                    movableLayers[i] = layers[i].GetComponentInChildren<Collider2D>(true) == null;
                    initialPositions[i] = layers[i].position;

                    if (!movableLayers[i])
                    {
                        Debug.LogWarning(
                            $"Parallax layer '{layers[i].name}' contains a Collider2D and will not be moved.",
                            layers[i]);
                    }
                }
            }
        }

        public void Apply(Vector3 cameraOffset, Vector2 movementAxes)
        {
            if (initialPositions == null || initialPositions.Length != layers.Length
                || movableLayers == null || movableLayers.Length != layers.Length)
            {
                CacheInitialPositions();
            }

            Vector3 movement = new Vector3(
                cameraOffset.x * movementAxes.x,
                cameraOffset.y * movementAxes.y,
                0f) * cameraFollow;

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] != null && movableLayers[i])
                {
                    layers[i].position = initialPositions[i] + movement;
                }
            }
        }
    }

    [Serializable]
    private sealed class ParallaxRoom
    {
        [SerializeField] private Collider2D roomBounds;
        [SerializeField] private ParallaxLayerGroup backgroundLayer = new ParallaxLayerGroup(0.8f);
        [SerializeField] private ParallaxLayerGroup farLayer = new ParallaxLayerGroup(0.6f);
        [SerializeField] private ParallaxLayerGroup middleLayer = new ParallaxLayerGroup(0.3f);
        [SerializeField] private ParallaxLayerGroup nearLayer = new ParallaxLayerGroup(0.1f);

        private Vector3 cameraOrigin;

        public bool IsValid => roomBounds != null;

        public void Initialize(Vector3 origin)
        {
            cameraOrigin = origin;
            backgroundLayer.CacheInitialPositions();
            farLayer.CacheInitialPositions();
            middleLayer.CacheInitialPositions();
            nearLayer.CacheInitialPositions();
        }

        public Vector3 GetBoundsCenter()
        {
            return roomBounds != null ? roomBounds.bounds.center : Vector3.zero;
        }

        public void SetCameraOrigin(Vector3 origin)
        {
            cameraOrigin = origin;
        }

        public void Apply(Vector3 cameraPosition, Vector2 movementAxes)
        {
            Vector3 cameraOffset = cameraPosition - cameraOrigin;
            backgroundLayer.Apply(cameraOffset, movementAxes);
            farLayer.Apply(cameraOffset, movementAxes);
            middleLayer.Apply(cameraOffset, movementAxes);
            nearLayer.Apply(cameraOffset, movementAxes);
        }
    }

    [Header("References")]
    [SerializeField] private Transform targetCamera;
    [SerializeField] private ParallaxRoom[] rooms = Array.Empty<ParallaxRoom>();

    [Header("Room Transition")]
    [SerializeField, Min(0)] private int initialRoomIndex;
    [SerializeField, Min(0f)] private float roomTransitionDelay = 2f;

    [Header("Movement")]
    [SerializeField] private Vector2 movementAxes = Vector2.one;

    private int currentRoomIndex;
    private Coroutine roomTransitionRoutine;

    private void Awake()
    {
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }

        if (rooms == null || rooms.Length == 0)
        {
            return;
        }

        currentRoomIndex = Mathf.Clamp(initialRoomIndex, 0, rooms.Length - 1);

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] == null || !rooms[i].IsValid)
            {
                continue;
            }

            Vector3 origin = i == currentRoomIndex && targetCamera != null
                ? targetCamera.position
                : rooms[i].GetBoundsCenter();

            rooms[i].Initialize(origin);
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null || !IsValidRoomIndex(currentRoomIndex))
        {
            return;
        }

        rooms[currentRoomIndex].Apply(targetCamera.position, movementAxes);
    }

    public void SetRoom(int roomIndex)
    {
        if (!IsValidRoomIndex(roomIndex) || roomIndex == currentRoomIndex)
        {
            return;
        }

        if (roomTransitionRoutine != null)
        {
            StopCoroutine(roomTransitionRoutine);
        }

        roomTransitionRoutine = StartCoroutine(ChangeRoomAfterCameraBlend(roomIndex));
    }

    private IEnumerator ChangeRoomAfterCameraBlend(int roomIndex)
    {
        if (roomTransitionDelay > 0f)
        {
            yield return new WaitForSeconds(roomTransitionDelay);
        }

        // 방이 실제로 전환된 시점의 카메라 위치를 기준점으로 사용해야
        // CameraBound 중심과 진입 위치의 차이만큼 배경이 순간 이동하지 않습니다.
        if (targetCamera != null)
        {
            rooms[roomIndex].SetCameraOrigin(targetCamera.position);
        }

        currentRoomIndex = roomIndex;
        roomTransitionRoutine = null;
    }

    private bool IsValidRoomIndex(int roomIndex)
    {
        return rooms != null
            && roomIndex >= 0
            && roomIndex < rooms.Length
            && rooms[roomIndex] != null
            && rooms[roomIndex].IsValid;
    }
}
