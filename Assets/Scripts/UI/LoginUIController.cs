using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

[RequireComponent(typeof(UIDocument))]
public class LoginUIController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private TextField inputField;
    private Button confirmButton;
    [SerializeField] private GameObject gachaUI;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        inputField = root.Q<TextField>("inputField");
        confirmButton = root.Q<Button>("confirmButton");

        confirmButton.clicked += OnConfirmClicked;
    }

    private void OnConfirmClicked()
    {
        var loginManager = LoginManager.Instance;
        if (loginManager == null)
            return;

        loginManager.Username = inputField.value;
        StartCoroutine(LoginAndSwitchUI());
    }

    private IEnumerator LoginAndSwitchUI()
    {
        var username = string.IsNullOrEmpty(inputField.value) ? $"Player{Random.Range(10000000, 99999999)}" : inputField.value;
        yield return LoginManager.Instance.Login(username);

        if (NetworkManager.Instance.CurrentUser != null)
        {
            gameObject.SetActive(false);
            if (gachaUI != null)
            {
                gachaUI.SetActive(true);
                gachaUI.GetComponent<GachaUIController>().Initialize();

                NetworkManager.Instance.Initialize();
            }
        }
    }
}


