using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    [Header("Step 1 - Mobile Entry")]
    public GameObject mobileEntryPanel;
    public InputField mobileInput;
    public Text authStatusText;

    [Header("Step 2 - Sign Up (shown only for new mobile numbers)")]
    public GameObject signUpPanel;
    public InputField nameInput;
    public InputField emailInput;

    [Header("Hub (game grid screen)")]
    public GameObject hubPanel;
    public Text welcomeText;
    public LauncherManager launcherManager;

    public static UserProfile CurrentUser { get; private set; }

    void Start()
    {
        signUpPanel.SetActive(false);
        hubPanel.SetActive(false);
        mobileEntryPanel.SetActive(true);
    }

    public void OnContinueClicked()
    {
        string mobile = mobileInput.text.Trim();
        if (string.IsNullOrEmpty(mobile))
        {
            authStatusText.text = "Enter your mobile number.";
            return;
        }

        authStatusText.text = "Checking...";
        FirebaseRunner.Instance.StartCoroutine(
            FirebaseRestClient.Get("users/" + mobile, OnUserLookupResult));
    }

    private void OnUserLookupResult(bool success, string response)
    {
        if (!success)
        {
            authStatusText.text = "Network issue — check WiFi and try again.";
            return;
        }

        string mobile = mobileInput.text.Trim();

        if (response == "null")
        {
            // No account yet — collect name + email once.
            authStatusText.text = "";
            mobileEntryPanel.SetActive(false);
            signUpPanel.SetActive(true);
        }
        else
        {
            UserProfile profile = JsonConvert.DeserializeObject<UserProfile>(response);
            profile.mobile = mobile;
            if (profile.scores == null) profile.scores = new System.Collections.Generic.Dictionary<string, int>();
            CurrentUser = profile;
            EnterHub();
        }
    }

    public void OnSignUpSubmit()
    {
        string mobile = mobileInput.text.Trim();
        string name = nameInput.text.Trim();
        string email = emailInput.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            authStatusText.text = "Enter your name.";
            return;
        }

        UserProfile profile = new UserProfile
        {
            name = name,
            mobile = mobile,
            email = email,
            cumulativeScore = 0
        };

        string json = JsonConvert.SerializeObject(profile);
        authStatusText.text = "Creating profile...";

        FirebaseRunner.Instance.StartCoroutine(
            FirebaseRestClient.Put("users/" + mobile, json, (success, resp) =>
            {
                if (success)
                {
                    CurrentUser = profile;
                    EnterHub();
                }
                else
                {
                    authStatusText.text = "Couldn't sign up — check WiFi and try again.";
                }
            }));
    }

    private void EnterHub()
    {
        mobileEntryPanel.SetActive(false);
        signUpPanel.SetActive(false);
        hubPanel.SetActive(true);
        welcomeText.text = "Welcome, " + CurrentUser.name + "!";
        launcherManager.PopulateLibrary();
    }
}
