using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZPuzzleBubble;

public class TestPlayManager : Singleton<TestPlayManager>
{
    //public 
    public void PlayTestMode(int _level)
    {
        // SceneManager.sceneLoaded += (_scene, _mode) =>
        // {
        //     //_scene.
        //     if (_scene.name == "InGame")
        //     {
        //         
        //     }
        // };
        SceneManager.LoadSceneAsync("InGame").completed += (_op) =>
        {
            _OnLoadedPlayScene(_level);
            //var scene = SceneManager.GetActiveScene();
            //scene.GetRootGameObjects();
            //_op.

        };
    }

    private void _OnLoadedPlayScene(int _level)
    {
        var stage = Object.FindObjectOfType<Stage>();
        if (stage != null)
        {
            stage.PlayTestMode(_level);
            Debug.Log("dsdsd");
        }
    }
    // // Start is called before the first frame update
    // void Start()
    // {
    //     
    // }
    //
    // // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
