using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AugmentManager : MonoBehaviour
{
    [SerializeField] private GameObject augmentPanel;

    private List<Dictionary<string, object>> data;

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

    // Update is called once per frame
    public void RandomAugment()
    {
        Debug.Log("RandomAugment");

        Debug.Log(data);
        Debug.Log(augmentPanel);
        Debug.Log(augmentCards);

        for (int i = 0; i < augmentCards.Length; i++)
        {
            Debug.Log($"Card {i}: {augmentCards[i]}");

            int index = Random.Range(0, data.Count);

            augmentCards[i].SetData(data[index], ApplyAugment);
        }

        augmentPanel.SetActive(true);
    }
    public void ApplyAugment(Dictionary<string, object> augmentData)
    {
        // 테스트용으로 선택된 증강체 이름 출력
        Debug.Log($"선택한 증강체: {augmentData["Name"]}");

        // 선택 완료 후 증강체 선택창 숨김
        augmentPanel.SetActive(false);
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
