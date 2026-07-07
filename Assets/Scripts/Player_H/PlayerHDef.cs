using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHDef : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private bool isDefending = false;
    public bool IsDefending => isDefending;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    public void OnDeffend(InputValue value)
    {
        bool isPressd = value.isPressed;

        playerMovement.SetDefending(isPressd);

        if (isPressd)
        {

        }
        else
        {

        }
    }
}
