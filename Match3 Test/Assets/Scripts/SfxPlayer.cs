using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
	public AudioSource efxSource;
	public static SfxPlayer instance = null;

	[SerializeField] AudioClip buttonClickSfx;
	[SerializeField] AudioClip gemFlipSfx;
	[SerializeField] AudioClip gameClearSfx;

	// Start is called before the first frame update
	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
	}

	public void PlayButtonSfx()
	{
		PlaySingle(buttonClickSfx);
	}

	public void PlayGemFlipSfx()
	{
		PlaySingle(gemFlipSfx);
	}

	public void PlayGameClearSfx()
	{
		PlaySingle(gameClearSfx);
	}
	
	public void PlaySingle (AudioClip clip)
	{
		efxSource.clip = clip;
		efxSource.Play();
	}
}
