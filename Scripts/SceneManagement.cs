using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public int enemyCount = 0;
    private void Start()
    {
        // Tìm tất cả quái vật trong map hiện tại
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
    public void OnEnemyDefeated()
    {
        enemyCount--;

        if (enemyCount <= 0)
        {
            Debug.Log("Tất cả quái đã bị tiêu diệt. Bạn có thể qua map mới!");
        }
    }

    public void LoadLevel()
    {
        if (enemyCount > 0)
        {
            Debug.LogWarning("Vẫn còn quái vật, không thể chuyển map!");
            return;
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Chuyển sang Scene tiếp theo (nếu có)
        if (currentSceneIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
        else
        {
            Debug.LogWarning("Đây là Scene cuối cùng, không thể chuyển tiếp!");
        }
    }
}
