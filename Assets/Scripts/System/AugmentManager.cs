using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance { get; private set; }
    [SerializeField] private GameObject augmentPanel;

    private List<Dictionary<string, object>> data;
    float remainingN;

    [Header("CardList")]
    [SerializeField] private AugmentCard[] augmentCards;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Debug.Log("Start 호출");

        data = CSVReader.Read("Augmentation");

        Debug.Log(data == null ? "data == null" : $"data.Count = {data.Count}");

        augmentPanel.SetActive(false);
    }

    public void StartAugmentSequence(int n)
    {
        if (n <= 0) return;
        remainingN = n;
        ShowNextAugment();

    }

    private void ShowNextAugment()
    {
        if (remainingN <= 0)
        {
            augmentPanel.SetActive(false);
            return;
        }

        RandomAugment();
    }

    private void RandomAugment()
    {
        List<int> pool = new List<int>();
        for(int i=0; i< data.Count; i++) pool.Add(i); 

        for (int i = 0; i < augmentCards.Length; i++)
        {
            if (pool.Count == 0) break;

            int poolIndex = Random.Range(0, pool.Count);
            int index = pool[poolIndex];

            augmentCards[i].SetData(data[index], ApplyAugment);
        }

        augmentPanel.SetActive(true);
    }
    public void ApplyAugment(Dictionary<string, object> augmentData)
    {
        remainingN--;

        // 선택 완료 후 증강체 선택창 숨김
        augmentPanel.SetActive(false);

        //남아있으면 반복
        ShowNextAugment();
    }

    public void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("앉기 입력");
            RandomAugment();
        }
    }
}
