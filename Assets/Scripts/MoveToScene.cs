using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MoveToScene : MonoBehaviour
{
    [SerializeField]
    private string sceneName;

    //private Button button;
	// Start is called before the first frame update

	private void Awake()
	{
        var button = GetComponent<Button>();
        if( button != null)
		{
            button.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(sceneName))
                {
                    SceneManager.LoadScene(sceneName);
                }
            });
        }
	}
}
