using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZPuzzleBubble;

public class TestDownloadAddressable : MonoBehaviour
{
    private void Awake()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                LevelDataManager.Instance.DownloadLevel();
                //StartCoroutine();
            });
        }
    }
}
