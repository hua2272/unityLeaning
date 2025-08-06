using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalCleaner : MonoBehaviour
{
    void Start()
    {
        // 确保传送门只存在于它所属的场景
        if (gameObject.scene.name != SceneManager.GetActiveScene().name)
        {
            Destroy(gameObject);
        }
    }
}