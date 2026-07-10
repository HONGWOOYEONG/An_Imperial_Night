using System.Collections;
using UnityEngine;

public class OBJ_AbillityAttack : MonoBehaviour
{
   
    float destroyTime = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        destroyTime = 22 / 60;
        Destroy(gameObject, destroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

  
}
