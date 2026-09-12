using UnityEngine;

public class B_TigerController : MonoBehaviour
{
    private B_TigerUtilityAI utilityAI;
    private B_TigerFSM tigerFSM;
    private Rigidbody2D rb;

    [Header("Status")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxPosture = 100f;
    [SerializeField] private float currentPosture;
    private bool isGroggy;
    public bool IsGroggy => isGroggy;

    private void Awake()
    {
        utilityAI = GetComponent<B_TigerUtilityAI>();
        tigerFSM = GetComponent<B_TigerFSM>();
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        currentPosture = 0f;
        isGroggy = false;
    }

    private void Update()
    {
        if (currentHealth <= 0)
        {
            tigerFSM.ChangeState(new BT_DeathState());
        }
        else if (currentPosture <= 0 && !isGroggy)
        {
            isGroggy = true;
            tigerFSM.ChangeState(new BT_GroggyState());
        }
    }
}
