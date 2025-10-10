using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    // optionポップアップ
    [SerializeField] private GameObject optionPopup;

    // Resultポップアップ
    [SerializeField] private GameObject resultPopup;

    // 現在のポップアップ
    private GameObject currentPopup;

    void Update()
    {
        // Escapeキーが押されたとき
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ポップアップが未表示なら開く
            if (currentPopup == null)
            {
                currentPopup = PopupManager.Instance.Open(optionPopup);
                Time.timeScale = 0f; // ポーズ
            }
            else
            {
                ClosePopup();
            }
        }
    }

    // ポップアップを閉じる
    private void ClosePopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
            Time.timeScale = 1f; // 再開
        }
    }
}
