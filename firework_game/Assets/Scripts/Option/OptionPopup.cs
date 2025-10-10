using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionPopup : MonoBehaviour
{[Header("Buttons")]
    [SerializeField] private Button quitButton;     // やめる（タイトルへ戻る）
    [SerializeField] private Button retryButton;    // リトライ
    [SerializeField] private Button helpButton;     // ヘルプ
    [SerializeField] private Button closeButton;    // 閉じる（ポップアップ閉じる）

    private void Start()
    {
        // 各ボタンのクリックイベント登録
        quitButton.onClick.AddListener(ClosePopup);
        retryButton.onClick.AddListener(OnRetry);
        helpButton.onClick.AddListener(OnHelp);
    }

    // リトライ（現在のシーンを再読み込み）
    private void OnRetry()
    {
        Time.timeScale = 1f; // ポーズ解除
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // ヘルプシーンへ
    private void OnHelp()
    {
        Time.timeScale = 1f; // ポーズ解除
        SceneManager.LoadScene("HelpScene"); // 実際のヘルプシーン名に変更
    }

    // ポップアップを閉じる
    private void ClosePopup()
    {
        Time.timeScale = 1f; // ゲーム再開
        Destroy(gameObject);
    }

}
