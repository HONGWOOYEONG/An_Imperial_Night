using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance { get; private set; }
    [SerializeField] private GameObject augmentPanel;
    [SerializeField] private GameObject playerHud;
    [SerializeField] private GameObject TimerUI;
    [SerializeField] private GameObject flameUI;
    [SerializeField] private GameObject bossUI;

    private List<Dictionary<string, object>> data;
    private int remainingN;
    public event System.Action OnSequenceCompleted;

    [Header("CardList")]
    [SerializeField] private AugmentCard[] augmentCards;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("Start 호출");

        data = CSVReader.Read("Augmentation");

        Debug.Log(data == null ? "data == null" : $"data.Count = {data.Count}");

        ResolveUIReferences(); // * 빌드에서 씬 참조가 빠져도 이름으로 UI를 다시 연결합니다.
        if (augmentPanel != null) augmentPanel.SetActive(false);
    }

    public void StartAugmentSequence(int n)
    {
        if (n <= 0)
        {
            OnSequenceCompleted?.Invoke();
            return;
        }
        remainingN = n;
        SetAugmentSelectionMode(true); // * 증강 선택 중에는 전투 HUD만 숨기고 플레이어 입력은 유지합니다.
        ShowNextAugment();

    }

    private void ShowNextAugment()
    {
        if (remainingN <= 0)
        {
            if (augmentPanel != null) augmentPanel.SetActive(false);
            if (playerHud != null) playerHud.SetActive(true);
            if (TimerUI != null) TimerUI.SetActive(true);
            SetAugmentSelectionMode(false); // * 모든 선택이 끝난 뒤에 HUD를 복구합니다.
            OnSequenceCompleted?.Invoke(); // * 마지막 증강 선택이 끝난 시점을 세션매니저에 전달
            return;
        }

        RandomAugment();
    }

    private void RandomAugment()
    {
        ResolveUIReferences(); // * 증강 표시 직전에 참조를 다시 확인해 SetActive null 예외를 막습니다.
        if (augmentPanel == null)
        {
            Debug.LogError("AugmentManager: AugmentPanel을 찾지 못해 증강을 표시할 수 없습니다.");
            return;
        }

        // * 빌드에서는 비활성 TMP에 값을 먼저 넣으면 첫 프레임에 글자가 갱신되지 않을 수 있어 패널부터 켭니다.
        augmentPanel.SetActive(true);
        augmentPanel.transform.SetAsLastSibling();

        // * 빌드에서 직렬화 배열이 비거나 끊겨도 현재 표시 중인 카드 컴포넌트를 직접 다시 찾습니다.
        augmentCards = augmentPanel.GetComponentsInChildren<AugmentCard>(true);

        // * 카드 데이터 처리 중 문제가 생겨도 전투 HUD가 남지 않도록 먼저 숨깁니다.
        if (TimerUI != null) TimerUI.SetActive(false);
        if (playerHud != null) playerHud.SetActive(false);
        if (flameUI != null) flameUI.SetActive(false);
        if (bossUI != null) bossUI.SetActive(false);

        List<int> pool = new List<int>();
        for(int i=0; i< data.Count; i++) pool.Add(i); 

        for (int i = 0; i < augmentCards.Length; i++)
        {
            if (pool.Count == 0) break;
            if (augmentCards[i] == null) continue;

            int poolIndex = Random.Range(0, pool.Count);
            int index = pool[poolIndex];

            augmentCards[i].SetData(data[index], ApplyAugment);
            pool.RemoveAt(poolIndex); // * 한 화면에 같은 증강이 중복 표시되지 않게 선택 항목을 제거합니다.
        }

        Canvas.ForceUpdateCanvases(); // * 카드 제목과 설명을 같은 프레임에 강제로 갱신합니다.

    }
    public void ApplyAugment(Dictionary<string, object> augmentData)
    {
        remainingN--;

        // 선택 완료 후 증강체 선택창 숨김
        augmentPanel.SetActive(false);

        //남아있으면 반복
        ShowNextAugment();
    }

    private void SetAugmentSelectionMode(bool selecting)
    {
        if (selecting)
        {
            if (flameUI != null) flameUI.SetActive(false);
            if (bossUI != null) bossUI.SetActive(false);
            return;
        }

        if (flameUI != null) flameUI.SetActive(true);
        if (bossUI != null) bossUI.SetActive(true);
    }

    private void ResolveUIReferences()
    {
        if (augmentPanel == null) augmentPanel = FindSceneObject("AugmentPanel");
        if (playerHud == null) playerHud = FindSceneObject("Hud");
        if (TimerUI == null) TimerUI = FindSceneObject("GameTimerPanel");
        if (flameUI == null) flameUI = FindSceneObject("GameFlamePanel");
        if (bossUI == null) bossUI = FindSceneObject("Boss");
    }

    private static GameObject FindSceneObject(string objectName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform item in transforms)
        {
            if (item is RectTransform && item.name == objectName)
                return item.gameObject;
        }
        return null;
    }
    
}
