using Unity.VisualScripting;
using UnityEngine;

public class BT_TargetDetector : MonoBehaviour
{
    public bool isRangedDetected;
    public bool isMeleeDetected;

    public bool IsTargetDetected => isMeleeDetected || isRangedDetected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("MeleeDealer"))
        {
            isMeleeDetected = true;
        }

        if (other.CompareTag("RangedDealer"))
        {
            isRangedDetected = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("MeleeDealer"))
        {
            isMeleeDetected = false;
        }

        if (other.CompareTag("RangedDealer"))
        {
            isRangedDetected = false;
        }
    }

    
}