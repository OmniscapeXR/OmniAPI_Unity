using UnityEngine;
#if TMP_PRESENT
using TMPro;
#else
using UnityEngine.UI;
#endif


public class DevLoginButton : MonoBehaviour
{
#if TMP_PRESENT
public TMP_InputField bearerTokenField;
public TMP_InputField userIdField;
#else
    public InputField bearerTokenField;
    public InputField userIdField;
#endif

    //Making changes!
    public OmniscapeInitializer initializer;

    public void Submit()
    {
        if (initializer == null) { Debug.LogError("[DevLoginButton] Initializer ref missing."); return; }
        var token = bearerTokenField ? bearerTokenField.text : "";
        var uid   = userIdField ? userIdField.text : "";
        initializer.ContinueWithDevCredentials(token, uid);
    }
}