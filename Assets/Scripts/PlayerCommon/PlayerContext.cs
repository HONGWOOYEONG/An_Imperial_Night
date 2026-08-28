using System.Collections.Generic;
using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    private Transform thisTransform;
    private Vector3 currentPosition;

    private void Awake()
    {
        thisTransform = GetComponent<Transform>();
        
    }

    private void Update()
    {
        currentPosition = transform.position;
    }

    public Vector3 getPosition()
    {
        return currentPosition;
    }
}
