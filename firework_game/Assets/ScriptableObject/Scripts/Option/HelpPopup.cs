using UnityEngine;
using UnityEngine.UI;

public class HelpPopup : MonoBehaviour
{
    [SerializeField] private Button closeButton; // 閉じるボタン

    private void Start()
    {
        closeButton.onClick.AddListener(CloseHelp);
    }

    private void CloseHelp()
    {
        // 時間を再開
        Time.timeScale = 1f;

        // プレイヤー操作を再開
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null)
            player.SetPaused(false);

        Destroy(gameObject);
    }
}