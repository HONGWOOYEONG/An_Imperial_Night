using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentCard : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    //[SerializeField] private TMP_Text targetText;

    private Dictionary<string, object> augmentData;
    private Action<Dictionary<string, object>> onSelected;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickCard);
    }

    public void SetData(Dictionary<string, object> augmentData, Action<Dictionary<string, object>> onSelected)
    {
        this.augmentData = augmentData;
        this.onSelected = onSelected;

        nameText.text = augmentData["Name"].ToString();
        descriptionText.text = augmentData["Description"].ToString();
        //targetText.text = ConvertTarget(augmentData["Target"].ToString());
    }

    //private string ConvertTarget(string target)
    //{
    //    switch (target)
    //    {
    //        case "H":
    //            return "회월 스님";

    //        case "T":
    //            return "태자 전하";

    //        case "Global":
    //        case "H|T":
    //            return "공용 증강";

    //        default:
    //            return target;
    //    }
    //}

    private void OnClickCard()
    {
        onSelected?.Invoke(augmentData);
    }
}