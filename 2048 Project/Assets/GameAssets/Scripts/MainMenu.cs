using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	Manager m;
	public Text powerText ;

	void Start () {
		m = GameObject.Find ("m").GetComponent<Manager> ();
		m.UpdatePowerPlate (powerText);
	}

	public void OnPlayClick(){
		m.Play_Click ();
		SceneManager.LoadScene ("Gameplay");
	}
	

}
