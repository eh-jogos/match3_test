using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[System.Serializable] public class StringSignals:UnityEvent<string> {}
public class ScoreSystem : MonoBehaviour
{
	public StringSignals updateScoreProgress;
	public StringSignals updateGoalLabels;
	public StringSignals updateLevelLabels;
	public StringSignals updateScoreLabels;
	public StringSignals updateEndScreenMessage;
	public StringSignals updateEndScreenButton;
	public UnityEvent gameRetry;
	public UnityEvent gameNext;

	[Header("Score System Parameters")]
	[SerializeField] int pointPerGem = 25;
	[SerializeField] int firstGoal = 2000;
	[SerializeField] float goalIncrementPerLevel = 1.25f;
	[Header("Game Strings")]
	[SerializeField] string baseLevelText = "Level {0}";
	[SerializeField] string baseScoreText = "{0}/{1}";
	[TextArea(1,2)]
	[SerializeField] string baseGoalText = "Goal\n{0}";
	[TextArea(1,2)]
	[SerializeField] string successMessage = "Well Done!\n=D";
	[TextArea(1,2)]
	[SerializeField] string failMessage = "Too Bad!\nD=";

	int currentGoal;
	int currentScore = 0;
	int currentLevel = 1;
	// Start is called before the first frame update
	void Start()
	{
		currentGoal = firstGoal;
		UpdateScoreProgress();
		UpdateLevelAndGoalLabels();
	}
	
	public void InitializeLevelGoal()
	{
		currentLevel++;
		currentScore = 0;
		currentGoal = Mathf.RoundToInt(currentGoal * goalIncrementPerLevel);
		UpdateScoreProgress();
		UpdateLevelAndGoalLabels();
	}

	void UpdateLevelAndGoalLabels()
	{
		updateGoalLabels.Invoke(string.Format(baseGoalText, currentGoal));
		updateLevelLabels.Invoke(string.Format(baseLevelText, currentLevel));
	}

	void UpdateScoreProgress()
	{
		updateScoreProgress.Invoke(string.Format(baseScoreText, currentScore, currentGoal));
	}

	void UpdateScoreLabel()
	{
		updateScoreLabels.Invoke(currentScore.ToString());
	}

	public void onGameTimerTimedOut()
	{
		UpdateScoreLabel();
		if (currentScore >= currentGoal)
		{
			updateEndScreenMessage.Invoke(successMessage);
			updateEndScreenButton.Invoke("Next");
		}
		else
		{
			updateEndScreenMessage.Invoke(failMessage);
			updateEndScreenButton.Invoke("Retry");
		}
	}

	public void onEndScreenActionPressed(){
		if (currentScore >= currentGoal)
		{
			InitializeLevelGoal();
			gameNext.Invoke();
		}
		else
		{
			currentScore = 0;
			UpdateScoreProgress();
			UpdateLevelAndGoalLabels();
			gameNext.Invoke();
		}
	}

	public void IncrementScore(int num_of_gems)
	{
		int score = num_of_gems * pointPerGem;
		if (num_of_gems >= 9)
			score *= 5;
		else if (num_of_gems >= 6)
			score *= 3;
		else if (num_of_gems == 5)
			score = Mathf.RoundToInt(score * 2.5f);
		else if (num_of_gems == 4)
			score *= 2;
		
		currentScore += score;
		UpdateScoreProgress();
	}
}
