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
        Destroy(gameObject);
    }
}