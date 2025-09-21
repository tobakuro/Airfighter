using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTitleGame : MonoBehaviour
{
        void Update()
    {
        // Enterキーが押されたら
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // "PGameScene" というシーンへ遷移

            SceneManager.LoadScene("PGameScene");
        }
    }
}
