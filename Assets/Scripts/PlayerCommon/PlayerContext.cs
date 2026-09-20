using System.Collections.Generic;
using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    public Transform trans;
    private Vector3 currentPosition;
    private bool doTransPosition;

    private void Awake()
    {
        trans = GetComponent<Transform>();
        
    }

    private void Update()
    {
        currentPosition = base.transform.position;
    }

    public Vector3 getPosition()
    {
        return currentPosition;
    }

    public void setTransPosition()
    {
        doTransPosition = true;
    }
}
