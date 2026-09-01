using UnityEngine;


public class SpiderWebSwamp : MonoBehaviour
{
    private E_PuppeteerController controller;
    private float timer;
    private float time = 0.3f;

    private GameObject spawnedPillar;
    private Vector2 pillarPos;
    private bool isPillarSpawned = false;

    private void Awake()
    {
        timer = 0;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.FindWithTag("Enemy_Puppeteer").GetComponent<E_PuppeteerController>();
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= time && !isPillarSpawned)
        {
            isPillarSpawned = true;

            if (controller != null && controller.spiderwebPillar != null)
            {
                Debug.Log("거미줄 기둥 생성");
                pillarPos = new Vector2(transform.position.x, controller.transform.position.y);
                spawnedPillar = Instantiate(controller.spiderwebPillar, pillarPos, Quaternion.identity); //transform.position을 수정
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "RangedDealer" || other.gameObject.tag == "DamageDealer")
        {
            Debug.Log("spiderWebSwamp" + other.tag + " 명중");
            // 캐릭터의 이동속도가 20% 느려집니다
            //other.moveSpeed = 
        }
    }
    private void OnDestroy() //거미줄 기둥 같이 삭제
    {
        if (spawnedPillar != null)
        {
            controller.isPatternEnded_F = true;
            Destroy(spawnedPillar);
        }
    }
}
