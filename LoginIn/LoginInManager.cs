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

    public TMP_Text accountName;
    public TMP_Text accountId;
    public TMP_Text playerExperience;
    public TMP_Text playerKills;
    public TMP_Text playerCoins;

    public bool showPw = false;
    string logedUser = "";
    public int account_ID = -1;
    int playerExp = 0;
    int killed_Enemies = 0;
    int collected_Coins = 0;

    public AchievementManager achievementManager;

    private string baseUrl = "http://localhost:3001";

    private void Start()
    {
        achievementManager.OnAchievementChecked += HandleLoginAchievement;
    }

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
        logedUser = string.Empty;
        account_ID = -1;
    }

    public void GetStats()
    {
        StartCoroutine(GetExperienceCoroutine());
        StartCoroutine(GetKilled_EnemiesCoroutine());
        StartCoroutine(GetCollected_CoinsCoroutine());
    }

    public void SetExp()
    {
        playerExp += 100;
        playerExperience.text = "Player Experience: " + playerExp;
        StartCoroutine(SetExperienceCoroutine());
    }

    public void SetKills()
    {
        killed_Enemies += 1;
        playerKills.text = "Enemy killed: " + killed_Enemies;
        StartCoroutine (SetEnemyKilledCoroutine());
    }

    public void SetCoins()
    {
        collected_Coins += 1;
        playerCoins.text = "Collected coins: " + collected_Coins;
        StartCoroutine(SetCoinsCollectedCoroutine());
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
                    logedUser = !justCreated ? usernameInput.text : name;
                    accountName.text = "#" + logedUser;
                    account_ID = response.player_id;
                    accountId.text = "ID: " + account_ID;
                    GetStats();

                    achievementManager.CheckAchievement("1");
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

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/create", form))
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

    IEnumerator GetExperienceCoroutine()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/get_stat/" + account_ID + "/experience"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                playerExperience.text = "Error fetching player experience.";
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                PlayerStatsResponse response = JsonUtility.FromJson<PlayerStatsResponse>(responseJson);
                playerExp = response.experience;
                playerExperience.text = "Player exp: " + playerExp;
            }
        }
    }

    IEnumerator GetKilled_EnemiesCoroutine()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/get_stat/" + account_ID + "/killed_enemies"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                playerExperience.text = "Error fetching player experience.";
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                PlayerStatsResponse response = JsonUtility.FromJson<PlayerStatsResponse>(responseJson);
                killed_Enemies = response.killed_enemies;
                playerKills.text = "Enemy killed: " + killed_Enemies;
            }
        }
    }

    IEnumerator GetCollected_CoinsCoroutine()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/get_stat/" + account_ID + "/collected_coins"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                playerExperience.text = "Error fetching player experience.";
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                PlayerStatsResponse response = JsonUtility.FromJson<PlayerStatsResponse>(responseJson);
                collected_Coins = response.collected_coins;
                playerCoins.text = "Collected coins: " + collected_Coins;
            }
        }
    }

    IEnumerator SetExperienceCoroutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", account_ID);
        form.AddField("experience", playerExp);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/set_stats/", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                playerExperience.text = "Error updating player experience.";
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                SetStatsResponse response = JsonUtility.FromJson<SetStatsResponse>(responseJson);

                if (response.success)
                {
                    messageText.text = "Player: " + response.player_stat.playername + "\n" +
                                        "Experience: " + response.player_stat.experience;
                }
                else
                {
                    messageText.text = "Failed to update player experience.";
                }
            }
        }
    }

    IEnumerator SetEnemyKilledCoroutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", account_ID);
        form.AddField("killed_enemies", killed_Enemies);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/set_stats/", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                playerExperience.text = "Error updating player experience.";
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                SetStatsResponse response = JsonUtility.FromJson<SetStatsResponse>(responseJson);

                if (response.success)
                {
                    //messageText.text = "Player: " + response.player_stat.playername + "\n" +
                                        //"Experience: " + response.player_stat.experience;
                }
                else
                {
                    messageText.text = "Failed to update enemies killed.";
                }
            }
        }
    }

    IEnumerator SetCoinsCollectedCoroutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", account_ID);
        form.AddField("collected_coins", collected_Coins);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/set_stats/", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                playerExperience.text = "Error updating player coins.";
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                SetStatsResponse response = JsonUtility.FromJson<SetStatsResponse>(responseJson);

                if (response.success)
                {
                    //messageText.text = "Player: " + response.player_stat.playername + "\n" +
                    //"Experience: " + response.player_stat.experience;
                }
                else
                {
                    messageText.text = "Failed to update player coins.";
                }
            }
        }
    }

    private void HandleLoginAchievement(bool achieved, int id)
    {
        if (id == 1)
        {
            if (achieved)
                achievementManager.CheckAchievement("2");
            else achievementManager.AddAchievement(1);
        }
        else if (id == 2)
        {
            if (achieved) { }
            else achievementManager.AddAchievement(2);

        }
    }

    [System.Serializable]
    private class LoginResponse
    {
        public bool success;
        public string player_name;
        public string token;
        public int player_id;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string message;
    }

    [System.Serializable]
    private class PlayerStatsResponse
    {
        public int player_id;
        public int experience;
        public int killed_enemies;
        public int collected_coins;
        public string playername;
    }

    [System.Serializable]
    private class SetStatsResponse
    {
        public bool success;
        public PlayerStatsResponse player_stat;
    }

    void OnDestroy()
    {
        achievementManager.OnAchievementChecked -= HandleLoginAchievement;
    }
}
