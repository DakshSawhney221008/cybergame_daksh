using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PurrNet;
using PurrNet.Packing;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : NetworkBehaviour
{
    [Header("Quiz Data")]
    [SerializeField] QuestionSet questionData;

    [Header("Quiz References")]
    [SerializeField] TMP_Text questionTxt;
    [SerializeField] List<TMP_Text> optionsTxt;
    [SerializeField] List<Button> optionsBtn;
    [SerializeField] List<Image> optionsImage;
    [SerializeField] Sprite buttonRed, buttonGreen, buttonBlue, buttonYellow;

    [Header("Leaderboard References")]
    [SerializeField] Transform leaderboardContent;
    [SerializeField] GameObject leaderboardPanel, leaderboardEntryPrefab, nextQuestion;

    [Header("Other")]
    [SerializeField] Timer quizTimer;
    [SerializeField] SyncVar<int> currentQuestionIdx = new(0);


    Question currQuestion;
    bool questionEndTasksCompleted = false, leaderboardUpdated = false, leaderboardTimeout = false;
    Dictionary<PlayerID, int> playerSelectedOptions = new Dictionary<PlayerID, int>();

    protected override void OnSpawned()
    {
        base.OnSpawned();

        foreach (var x in GameData.Instance.nicknames)
        {
            Debug.Log($"ID: {x.Key}, Nickname: {x.Value}");
        }

        if (isServer)
        {
            quizTimer.onTimerComplete.AddListener(TimerOver);
        }

        currentQuestionIdx.onChanged += OnQuestionIdxChanged;
        GameData.Instance.scores.onChanged += OnPlayerDataChanged;

        Debug.Log($"Player ID: {networkManager.localPlayer.id}, IsServer: {isServer}");
        SetQuestion();
    }

    private void OnDestroy()
    {
        currentQuestionIdx.onChanged -= OnQuestionIdxChanged;
        quizTimer.onTimerComplete.RemoveAllListeners();
    }

    private void OnQuestionIdxChanged(int val)
    {
        StartCoroutine(OnQuestionIdxChangedCoroutine());
    }

    private IEnumerator OnQuestionIdxChangedCoroutine()
    {
        yield return new WaitWhile(() => questionEndTasksCompleted != true);

        SetQuestion();
        questionEndTasksCompleted = false;
    }


    private void SetQuestion()
    {
        Debug.Log($"Setting Question Number {currentQuestionIdx.value + 1}");
        currQuestion = questionData.GetQuestion(currentQuestionIdx.value);

        questionTxt.text = currQuestion.text;
        for (int i = 0; i < optionsTxt.Count; i++)
        {
            optionsTxt[i].text = currQuestion.options[i];
        }

        if(isServer)
            quizTimer.StartTimer();

        ToggleOptionsBtn(true);
    }

    public void AnswerSelected(int optionIdx)
    {
        ToggleOptionsBtn(false);
        FadeSprite(optionsImage[optionIdx], buttonYellow);
        PlayerAnsweredQuestion(networkManager.localPlayer, optionIdx);
    }

    private void TimerOver()
    {
        Debug.Log(playerSelectedOptions.Count + " players answered.");
        foreach(var x in networkManager.players)
        {
            if(playerSelectedOptions.ContainsKey(x))
            {
                ShowAnswer(x, playerSelectedOptions[x]);
            }
            else
            {
                ShowAnswer(x, -1);
            }
        }

        currentQuestionIdx.value++;
        playerSelectedOptions.Clear();
    }

    [ServerRpc]
    private void PlayerAnsweredQuestion(PlayerID id, int optionIdx)
    {
        playerSelectedOptions.Add(id, optionIdx);

        if (playerSelectedOptions.Count >= networkManager.playerCount)
        {
            foreach(var x in playerSelectedOptions)
            {
                ShowAnswer(x.Key, x.Value);
            }

            currentQuestionIdx.value++;

            playerSelectedOptions.Clear();
        }
    }

    [TargetRpc]
    private void ShowAnswer(PlayerID id, int optionIdx)
    {
        if(networkManager.localPlayer.id == id.id)
        {
            if(optionIdx == -1)
            {
                ToggleOptionsBtn(false);
                FadeSprite(optionsImage[currQuestion.correctIndex], buttonGreen);
            }
            else if(optionIdx == currQuestion.correctIndex)
            {
                FadeSprite(optionsImage[optionIdx], buttonGreen);
                IncScore(networkManager.localPlayer.id.value, isServer);
                //if(isServer)
                //{
                //    ulong serverID = networkManager.localPlayer.id.value;
                //    if (GameData.Instance.scores.Keys.Contains(serverID))
                //        GameData.Instance.scores[serverID]++;
                //    else
                //    {
                //        serverID = (serverID == 0) ? (ulong)1 : 0;
                //        GameData.Instance.scores[serverID]++;
                //    }
                //}
                //else
                //{
                //    GameData.Instance.scores[networkManager.localPlayer.id.value]++;
                //}
            }
            else
            {
                FadeSprite(optionsImage[optionIdx], buttonRed);
                FadeSprite(optionsImage[currQuestion.correctIndex], buttonGreen);
            }

            StartCoroutine(QuestionEndTasks());
        }
    }

    [ServerRpc]
    private void IncScore(ulong id, bool server)
    {
        if (server)
        {
            //if (GameData.Instance.scores.Keys.Contains(id))
            //    GameData.Instance.scores[id]++;
            //else
            //{
            //    id = (id == 0) ? (ulong)1 : 0;
            Debug.Log($"Server ID: {id}");
                GameData.Instance.scores[0]++;
            //}
        }
        else
        {
            GameData.Instance.scores[id]++;
        }
    }


    private IEnumerator QuestionEndTasks()
    {
        yield return new WaitForSeconds(1.5f);

        // Change Options BG Image to blue again
        foreach (var optionImage in optionsImage)
        {
            optionImage.sprite = buttonBlue;
        }

        StartCoroutine(LeaderboardTimeout());
        yield return new WaitUntil(() =>
        {
            return leaderboardUpdated || leaderboardTimeout;
        });
        StopCoroutine(LeaderboardTimeout());
        leaderboardTimeout = false;
        leaderboardUpdated = false;

        // Show Leaderboard for 5 seconds
        leaderboardPanel.gameObject.SetActive(true);
        for (int i = 0; i < GameData.Instance.scores.Count; i++)
        {
            LeaderboardEntry leaderboardEntry;
            var score = GameData.Instance.scores.Values.ElementAt(i);
            var nickname = GameData.Instance.nicknames.Values.ElementAt(i);

            if (i >= leaderboardContent.childCount)
            {
                leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboardContent).GetComponent<LeaderboardEntry>();

            }
            else
            {
                leaderboardEntry = leaderboardContent.GetChild(i).GetComponent<LeaderboardEntry>();
            }

            leaderboardEntry.nicknameTxt.text = nickname;
            leaderboardEntry.scoreTxt.text = score.ToString();
        }

        if (GameData.Instance.scores.Count < leaderboardContent.childCount)
        {
            for (int i = GameData.Instance.scores.Count; i < leaderboardContent.childCount; i++)
            {
                leaderboardContent.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (isServer)
        {
            nextQuestion.SetActive(true);
        }
    }

    private IEnumerator LeaderboardTimeout()
    {
        yield return new WaitForSeconds(5f);
        leaderboardTimeout = true;
    }

    private void OnPlayerDataChanged(SyncDictionaryChange<ulong, int> playerData)
    {
        leaderboardUpdated = true;
    }

    public void NextQuestionBtn()
    {
        NextQuestionTasks();
    }

    [ObserversRpc(runLocally:true)]
    private void NextQuestionTasks()
    {
        leaderboardPanel.SetActive(false);

        questionEndTasksCompleted = true;
    }

    private void FadeSprite(Image spriteImage, Sprite newSprite)
    {
        spriteImage.DOFade(0f, 0.5f).OnComplete(() =>
        {
            spriteImage.sprite = newSprite;

            spriteImage.DOFade(1f, 0.5f);
        });
    }

    private void ToggleOptionsBtn(bool action)
    {
        foreach(var optionBtn in optionsBtn)
        {
            optionBtn.interactable = action;
        }
    }
}
