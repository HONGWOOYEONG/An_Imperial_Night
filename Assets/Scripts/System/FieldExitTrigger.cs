using UnityEngine;

public class FieldExitTrigger : MonoBehaviour
{
    [SerializeField] private string meleeDealer = "MeleeDealer";
    [SerializeField] private string rangedTag = "RangedDealer";

    private bool closeRangeInside = false;
    private bool rangedInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(meleeDealer))
        {
            closeRangeInside = true;
        }
        else if (other.CompareTag(rangedTag))
        {
            rangedInside = true;
        }

        TryProceed();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(meleeDealer))
        {
            closeRangeInside = false;
        }
        else if (other.CompareTag(rangedTag))
        {
            rangedInside = false;
        }
    }

    private void TryProceed()
    {
        if (closeRangeInside && rangedInside)
        {
            GameSessionManager.Instance.OnFieldExitPointReached();
        }
    }
}