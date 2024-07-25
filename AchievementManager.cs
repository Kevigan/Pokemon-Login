
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.SocialPlatforms.Impl;
using System;

public class AchievementManager : MonoBehaviour
{
    public TMP_Text messageText;
    private string baseUrl = "http://localhost:3001";

    public TMP_Text achievementsText;
    public TMP_Text achievementsPrefab;
    public GameObject achievementContainer;
    List<Achievement> achievements = new List<Achievement>();

    public LoginInManager loginInManager;

    List<TMP_Text> instatiatedAchievements;

    public delegate void AchievementChecked(bool hasAchievement, int id);
    public event AchievementChecked OnAchievementChecked;

    public void AddAchievement(int achievementId)
    {
        print(achievementId);
        StartCoroutine(AddAchievementCoroutine(loginInManager.account_ID, achievementId));
    }

    public void GetAllAchievements()
    {
        if (loginInManager.account_ID > 0)
        {
            StartCoroutine(GetAchievements());
        }
       
    }

    public void CheckAchievement(string achievementID)
    {
        string playerID = loginInManager.account_ID.ToString();
        StartCoroutine(CheckAchievementCoroutine(playerID, achievementID, result =>
        {
        }));
    }

    IEnumerator GetAchievements()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/get_allAchievements"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching achievements: " + www.error);
                achievementsText.text = "Error fetching achievements. Please try again later.";
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                JSONNode jsonResponse = JSON.Parse(responseJson);

                foreach (JSONNode node in jsonResponse.AsArray)
                {
                    Achievement achievement = new Achievement
                    {
                        id = node["achievement_id"],
                        name = node["achievement_name"],
                        description = node["description"],
                        type = node["type"]
                    };
                    achievements.Add(achievement);
                }

                achievementsText.text = "Achievements:\n";

                // Get achievements for the specific player
                int playerID = loginInManager.account_ID;
                UnityWebRequest playerAchievementsRequest = UnityWebRequest.Get(baseUrl + "/player_achievements/" + playerID);
                yield return playerAchievementsRequest.SendWebRequest();

                if (playerAchievementsRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error fetching player achievements: " + playerAchievementsRequest.error);
                    yield break;
                }

                // Parse player achievements
                JSONNode playerAchievementsJson = JSON.Parse(playerAchievementsRequest.downloadHandler.text);
                HashSet<int> achievedAchievementIds = new HashSet<int>();
                foreach (JSONNode node in playerAchievementsJson.AsArray)
                {
                    achievedAchievementIds.Add(node["achievement_id"].AsInt);
                }

                instatiatedAchievements = new List<TMP_Text>();

                foreach (var achievement in achievements)
                {
                    bool isAchieved = achievedAchievementIds.Contains(achievement.id);
                    TMP_Text achievementTextObj = Instantiate(achievementsPrefab, achievementContainer.transform);
                    instatiatedAchievements.Add(achievementTextObj);
                    achievementTextObj.color = isAchieved ? Color.green : Color.grey;
                    achievementTextObj.text = "#" + achievement.id + "/ " + achievement.name + "/ " + achievement.description + (isAchieved ? " (Achieved)" : " (Not Achieved)");
                }
            }
        }
    }

    IEnumerator AddAchievementCoroutine(int playerId, int achievementId)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerId", playerId);
        form.AddField("achievementId", achievementId);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/add_achievement", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string responseJson = www.downloadHandler.text;
                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(responseJson);
                string errorMessage = errorResponse.message;
                Debug.LogError("Network error: " + www.error);
                messageText.text = "Error: " + errorMessage;
            }
            else
            {
                AchievementResponse response = JsonUtility.FromJson<AchievementResponse>(www.downloadHandler.text);
                messageText.text = "Achievement added: " + response.achievement_id;
                Debug.Log("Achievement added: " + response.achievement_id);
            }
        }
    }

    IEnumerator CheckAchievementCoroutine(string playerId, string achievementId, System.Action<bool> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + $"/has_achievement/{playerId}/{achievementId}"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error checking achievement: " + www.error);
            }
            else
            {
                AchievementCheckResponse response = JsonUtility.FromJson<AchievementCheckResponse>(www.downloadHandler.text);
                bool hasAchievement = response.hasAchievement;
                Debug.Log("Has Achievement: " + hasAchievement);

                int achievementIdInt = -1;
                try
                {
                    achievementIdInt = int.Parse(achievementId);
                    Debug.Log("Parsed integer using int.Parse: " + achievementIdInt);
                }
                catch (FormatException)
                {
                    Debug.LogError("FormatException: The string is not a valid integer.");
                }

                OnAchievementChecked?.Invoke(hasAchievement, achievementIdInt);
            }
        }
    }

    [System.Serializable]
    public class AchievementCheckResponse
    {
        public bool hasAchievement;
    }

    [System.Serializable]
    private class AchievementResponse
    {
        public int achievement_id;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string message;
    }

    [System.Serializable]
    public class Achievement
    {
        public int id;
        public string name;
        public string description;
        public string type;
    }

    public void OnCloseAchievements()
    {
        foreach(TMP_Text text in instatiatedAchievements)
        {
            UnityEngine.Object.Destroy(text.gameObject);
        }
        achievements.Clear();
    }
}