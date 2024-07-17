using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.SceneManagement;

public class LoginInManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;

    public TMP_InputField createNameInput;
    public TMP_InputField createPasswordInput;

    public GameObject CreatePlayerPanel;
    public GameObject LoginPanel;
    public GameObject MenuPanel;

    public TMP_Text accountInfo;

    public bool showPw = false;

    private string baseUrl = "http://localhost:3001";

    public void Login()
    {
        StartCoroutine(LoginCoroutine(false, "", ""));
    }

    public void LogOut()
    {
        messageText.text = string.Empty;
        createNameInput.text = string.Empty;
        createPasswordInput.text = string.Empty;
        MenuPanel.SetActive(false);
        LoginPanel.SetActive(true);
    }

    public void CreatePlayer()
    {
        if (string.IsNullOrEmpty(createNameInput.text) || string.IsNullOrEmpty(createPasswordInput.text))
        {
            Debug.LogWarning("Username or password is empty");
            messageText.text = "Please enter both username and password.";
            return;
        }
        StartCoroutine(CreatePlayerCoroutine());
    }

    public void TogglePassword()
    {
        showPw = !showPw;
        if (showPw)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            passwordInput.inputType = TMP_InputField.InputType.Standard;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            passwordInput.inputType = TMP_InputField.InputType.Password;
        }
        passwordInput.Select();
    }

    public void OpenLevel()
    {
        SceneManager.LoadScene(1);
    }

    IEnumerator LoginCoroutine(bool justCreated, string name, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerName", !justCreated ? usernameInput.text : name);
        form.AddField("password", !justCreated ? passwordInput.text : password);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/login", form))
        {
            yield return www.SendWebRequest();

            string responseJson = www.downloadHandler.text;

            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(responseJson);
                string errorMessage = errorResponse.message;
                Debug.LogError("Error: " + www.error);
                messageText.text = "Error. " + errorMessage;
            }
            else
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseJson);
                if (response.success)
                {
                    messageText.text = "Login successfull\n" +
                        "Welcome " + response.player_name;
                    LoginPanel.SetActive(false);
                    CreatePlayerPanel.SetActive(false);
                    MenuPanel.SetActive(true);
                    string logedUser = !justCreated ? usernameInput.text : name;
                    accountInfo.text = "#" + logedUser;
                }
                else
                {
                    messageText.text = "Login failed. Check your credentials!";
                }
            }
        }
    }
    IEnumerator CreatePlayerCoroutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("playerName", createNameInput.text);
        form.AddField("password", createPasswordInput.text);

        using(UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/create", form))
        {
            yield return www.SendWebRequest();

            string responseJson = www.downloadHandler.text;

            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(responseJson);
                string errorMessage = errorResponse.message;
                Debug.LogError("Error: " + www.error);
                messageText.text = "Error. " + errorMessage;
            }
            else
            {
                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(responseJson);

                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.message))
                {
                    Debug.LogError("Error: " + errorResponse.message);
                    messageText.text = "Error: " + errorResponse.message;
                }
                else
                {
                    messageText.text = "Player created successfully!";
                    StartCoroutine(LoginAfterCreate(createNameInput.text, createPasswordInput.text));
                }
            }
        }
    }

    IEnumerator LoginAfterCreate(string name, string password)
    {
        yield return new WaitForSeconds(1);
        StartCoroutine(LoginCoroutine(true, name, password));
    }

    [System.Serializable]
    private class LoginResponse
    {
        public bool success;
        public string player_name;
        public string token;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string message;
    }
}
