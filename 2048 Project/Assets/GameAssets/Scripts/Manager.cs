using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

	public AudioClip click, dotSelect, dotReset, win;
	AudioSource aud;
	public string currentRegion;
	public int currentLevel;
	public int levelNumber;
	public bool reset;
	public bool dontgo;

	public int powerChrage;

	void Awake(){
		DontDestroyOnLoad (gameObject);
	}
		
	void Start(){
		if (!PlayerPrefs.HasKey ("powercharge")) {
			PlayerPrefs.SetInt ("powercharge", 4000);
		}

		powerChrage = PlayerPrefs.GetInt ("powercharge");
		aud = GetComponent<AudioSource> ();

		if (!dontgo) {
			StartCoroutine (Go ());
		}

		if (!PlayerPrefs.HasKey ("skin_0")) {
			PlayerPrefs.SetInt ("skin_0", 1);
		}

		if (reset) {
			PlayerPrefs.DeleteAll ();
		}
	}

	public void UpdatePowerPlate(Text t){
		t.text = powerChrage.ToString ();
	}

	public void UpdatePowerCharge(Text t, int amount){
		powerChrage += amount;
		PlayerPrefs.SetInt ("powercharge", powerChrage);
		t.text = powerChrage.ToString ();
	}

	IEnumerator Go(){
		yield return new WaitForSeconds (0.1f);
		SceneManager.LoadScene ("MainMenu");
	}

	public void Play_Click(){
		if (PlayerPrefs.GetInt ("sound") == 0) {
			aud.PlayOneShot (click);
		}
	}

	public void Play_DotSelect(){
		if (PlayerPrefs.GetInt ("sound") == 0) {
			aud.PlayOneShot (dotSelect);
		}
	}

	public void Play_DotReset(){
		if (PlayerPrefs.GetInt ("sound") == 0) {
			aud.PlayOneShot (dotReset);
		}
	}

	public void Play_Win(){
		if (PlayerPrefs.GetInt ("sound") == 0) {
			aud.PlayOneShot (win);
		}
	}

	public int getLastLevel(){
		Debug.Log ("Sending Last Level as " + PlayerPrefs.GetInt (currentRegion + "_Last").ToString ());
		return PlayerPrefs.GetInt (currentRegion + "_Last");
	}

	public void setLastLevel(int l){
		Debug.Log ("Setting Last level as " + l.ToString ());
		PlayerPrefs.SetInt (currentRegion + "_Last", l);
	}

}
