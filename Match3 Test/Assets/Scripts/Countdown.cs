using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Countdown : MonoBehaviour {
	public UnityEvent gameTimedOut;
	[SerializeField] float timeStart = 120;
	private TextMeshProUGUI tmpText;
	private float currenTime;

	// Use this for initialization
	void Start () {
		tmpText = GetComponent<TextMeshProUGUI>();
        enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (currenTime > 0)
		{
			currenTime -= Time.deltaTime;
			currenTime = Mathf.Clamp(currenTime, 0, timeStart);
			tmpText.text = Mathf.Round(currenTime).ToString();
		}
		else
		{
			gameTimedOut.Invoke();
			Debug.Log("TIMED OUT");
			enabled = false;
		}
	}
	
	public void Restart()
	{
		currenTime = timeStart;
		enabled = true;
	}
}