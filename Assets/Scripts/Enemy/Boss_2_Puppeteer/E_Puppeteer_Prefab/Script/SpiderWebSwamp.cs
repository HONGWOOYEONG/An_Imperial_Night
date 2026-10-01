using System.Collections.Generic;
using UnityEngine;

public class SpiderWebSwamp : MonoBehaviour
{
    private E_PuppeteerController controller;
    private float timer;
    private float time = 0.3f;
    private PlayerMovement playerMovement;
    private readonly HashSet<PlayerMovement> affectedPlayers = new HashSet<PlayerMovement>();
    private GameObject spawnedPillar;
    private Vector2 pillarPos;
    private bool isPillarSpawned = false;

    private void Awake()
    {
        timer = 0;
    }

    void Start()
    {
        GameObject enemy = GameObject.FindWithTag("Enemy_Puppeteer");
        if (enemy != null) controller = enemy.GetComponent<E_PuppeteerController>();
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= time && !isPillarSpawned)
        {
            isPillarSpawned = true;
            if (controller != null && controller.spiderwebPillar != null)
            {
                pillarPos = new Vector2(transform.position.x, controller.transform.position.y);
                spawnedPillar = Instantiate(controller.spiderwebPillar, pillarPos, Quaternion.identity);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 이 장판을 감속 원인으로 등록합니다. 다른 장판의 감속은 유지됩니다.
        if (!other.CompareTag("RangedDealer") && !other.CompareTag("MeleeDealer")) return;
        playerMovement = other.GetComponentInParent<PlayerMovement>();
        if (playerMovement == null) return;
        affectedPlayers.Add(playerMovement);
        playerMovement.SetSlowMove(this, 0.8f);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement leavingPlayer = other.GetComponentInParent<PlayerMovement>();
        if (leavingPlayer == null || !affectedPlayers.Remove(leavingPlayer)) return;
        leavingPlayer.ClearSlowMove(this);
    }

    private void OnDisable()
    {
        SetSlowMoveVar();
    }

    private void OnDestroy()
    {
        SetSlowMoveVar();
        if (spawnedPillar != null)
        {
            if (controller != null) controller.isPatternEnded_F = true;
            Destroy(spawnedPillar);
        }
    }

    private void SetSlowMoveVar()
    {
        // 퇴장하거나 장판이 사라질 때 이 장판이 등록한 효과만 해제합니다.
        foreach (PlayerMovement affectedPlayer in affectedPlayers)
            if (affectedPlayer != null) affectedPlayer.ClearSlowMove(this);
        affectedPlayers.Clear();
        playerMovement = null;
    }
}
