using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameplayManager : MonoBehaviour{

	public enum Powers
	{
		Switch,
		Destroy,
		Change,
		Scramble,
		None
	};

	public Powers currentPower = Powers.None;

	public static GameplayManager instance;
	Manager m;

	// Tile ids are in this order 
	// 2 = 0
	// 4 = 1;
	// 8 = 2;
	// 16 = 3;
	// 32 = 4;
	// 64 = 5;
	// 128 = 6;
	// 256 = 7;
	// 512 = 8;
	// 1024 = 9;
	// 2048 = 10;

	public Transform dotParent;
	public LineRenderer MainLine;

	public GameObject gameOverPanel, pausePanel, square, powerPanel;
	public Button pauseButton, powerButton;
	public GameObject DotObject;

	public bool isPaused, touchStarted;

	public Color[] TileColors = new Color[11];

	public int Score;
	public Text ScorePlate;
	public Text powerPlate;
	public Text bestScorePlate;
	public List<GameObject> currentChain = new List<GameObject>();

	public GameObject DestroPowerHelper, SwitchPowerHelper,  ChangePowerPanel, ChangePowerHelper;
	int temp_switch_int = 0;
	List<GameObject> switchObjects = new List<GameObject>();

	public Vector3 firstpos = new Vector3 (-1.9f, 2.0f, 90.0f);
	public float xGap, yGap;

	public GameObject ChangeSkinPanel, changeSkinButton;
	public List<Sprite> skinImages = new List<Sprite>();
	public List<Button> SkinSelectButtons = new List<Button> ();
	public List<Button> BuySkinButtons = new List<Button> ();
	public List<int> SkinCost = new List<int>();

	void Awake(){
		instance = this;
	}

	void Start(){
		Debug.Log (dotParent.Find ("0").transform.position.ToString () + "   " + dotParent.Find("6").transform.position.ToString());
		m = GameObject.Find ("m").GetComponent<Manager> ();
		m.UpdatePowerPlate (powerPlate);
		MainLine.sortingLayerName = "Default";
		MainLine.sortingOrder = 6;

		bestScorePlate.text = "best score" + System.Environment.NewLine + PlayerPrefs.GetInt ("bestscore");

		firstpos = dotParent.Find ("0").transform.position;
		Vector3 temp = dotParent.Find ("6").transform.position;

		xGap = temp.x - firstpos.x;
		yGap = firstpos.y - temp.y;

		UpdateSkinButtons ();

		Score = 0;
		LoadTheLevel ();
	}



	// this function is called everytime user points down on any of the boxes
	public void OnAnyDotDown(GameObject hit){
		if (currentPower == Powers.Destroy) {
			DestroyKey (hit);
			currentPower = Powers.None;
			OnPowerPanelClose ();
			DestroPowerHelper.SetActive (false);
			Invoke ("UpdateNames", 0.5f);
			return;
		}else if (currentPower == Powers.Change && temp_switch_int == 0) {
			temp_switch_int = 1;
			switchObjects.Add (hit);
			ChangePowerPanel.GetComponent<Animator> ().SetBool ("show", true);
			ChangePowerHelper.SetActive (false);
			return;
		} else if (currentPower == Powers.Switch && temp_switch_int < 2) {
			hit.GetComponent<Animator> ().SetBool ("inc", true);
			temp_switch_int++;
			switchObjects.Add (hit);

			if (temp_switch_int == 2) {
				SetKinematic (true);

				Vector3 v1 = switchObjects [0].transform.position;
				Vector3 v2 = switchObjects [1].transform.position;

				string s1 = switchObjects [0].name;
				string s2 = switchObjects [1].name;

				switchObjects [0].name = s2;
				switchObjects [1].name = s1;

//				switchObjects [0].GetComponent<Rigidbody2D> ().isKinematic = true;
//				switchObjects [1].GetComponent<Rigidbody2D> ().isKinematic = true;


				switchObjects [0].GetComponent<DotScript> ().MoveTo (v2);
				switchObjects [1].GetComponent<DotScript> ().MoveTo (v1);

				SwitchPowerHelper.SetActive (false);
			}
			return;
		}

		if (isPaused) {
			return;
		}


		if (!touchStarted) {
			m.Play_DotSelect ();

			touchStarted = true;
			currentChain.Add (hit);
			hit.GetComponent<DotScript> ().selected = true;
			hit.transform.GetChild (0).gameObject.SetActive (true);
			MainLine.transform.position = hit.transform.position;
			MainLine.positionCount = currentChain.Count;
			MainLine.SetPosition (currentChain.Count - 1, hit.transform.position);
		} else {
			if (currentChain.Count > 2 && hit == currentChain [currentChain.Count - 2]) {
				GameObject wo = currentChain [currentChain.Count - 1];
				currentChain.Remove (wo);
				wo.transform.GetChild (0).gameObject.SetActive (false);
				wo.GetComponent<DotScript> ().selected = false;
				MainLine.positionCount -= 1;
				wo.GetComponent<DotScript> ().ChildImage.gameObject.SetActive (false);
			} else if (Neighbour (hit.name) && hit.GetComponent<DotScript>().currentvalue == currentChain[0].GetComponent<DotScript>().currentvalue && !hit.GetComponent<DotScript>().selected) {
				m.Play_DotSelect ();
				currentChain.Add (hit);
				hit.transform.GetChild (0).gameObject.SetActive (true);
				hit.GetComponent<DotScript> ().selected = true;
				MainLine.positionCount = currentChain.Count;
				MainLine.SetPosition (currentChain.Count - 1, hit.transform.position);
			}
		}
	}

	public void SwitchComplete(GameObject ob){
		temp_switch_int--;
		if (temp_switch_int == 0) {
			isPaused = false;
			currentPower = Powers.None;
			SwitchPowerHelper.SetActive (false);
			OnPowerPanelClose ();
			SetKinematic (false);
			switchObjects.Clear ();
		}
	}

	public void OnAnyDotUp(GameObject hit){
		if (isPaused) {
			return;
		}

		// if user touches up when the object is seleceted and does not matches the chain and number of chain is > 1
		if (currentChain.Count > 1 ) {
			StartCoroutine (ConsumeChain ());
		}else if(!hit.GetComponent<DotScript> ().selected){
			DeactivateChain();
		}
		// if user touches up when the number of chain is == 1
		else if (currentChain.Count == 1) {
			DeactivateChain ();
		}

	}

	IEnumerator ConsumeChain(){
		isPaused = true;
		MainLine.positionCount = 0;
		int bitValue = currentChain [0].GetComponent<DotScript> ().currentvalue;
		int chainLength = currentChain.Count;

		int resultValue = bitValue;

		if (chainLength < 4) {
			resultValue *= 2;
		} else if (chainLength < 8) {
			resultValue *= 4;
		} else  {
			resultValue *= 16;
		}

		int powerAmount = 0;

		foreach (var item in currentChain) {
			if (item.GetComponent<DotScript> ().isOfferingCoin) {
				powerAmount++;
			}
		}

		if (powerAmount > 0) {
			m.UpdatePowerCharge (powerPlate, powerAmount);
		}

		UpdateScore (resultValue);
		resultValue = ClosestTo (resultValue);

		currentChain [chainLength - 1].GetComponent<DotScript> ().SetTileWithIndex (resultValue);
		currentChain [chainLength - 1].transform.GetComponent<DotScript> ().ChildImage.gameObject.SetActive (false);
		currentChain [chainLength - 1].GetComponent<DotScript> ().selected = false;
		currentChain [chainLength - 1].GetComponent<DotScript> ().isOfferingCoin = false;
		currentChain [chainLength - 1].GetComponent<DotScript> ().PowerCoin.SetActive (false);
		currentChain.RemoveAt (currentChain.Count - 1);

		foreach (var item in currentChain) {
			DestroyKey (item);
			yield return new WaitForSeconds (0.05f);
		}
		Invoke ("UpdateNames", 0.5f);

		DeactivateChain ();
		yield return new WaitForSeconds (0.5f);
		if (isNoMove ()) {
			do_Gameover ();
		}
		isPaused = false;

	}

	void DestroyKey(GameObject key){
//		Debug.Log ("Destroying " + key.name);

		int name = int.Parse (key.name);

		int topCount = name / 5;
//		Debug.Log ("Top Count " + topCount.ToString ());

		StartCoroutine (DisableKey (key));
//		for (int i = 1; i <= topCount; i++) {
//			int checker = name - (i * 5);
////			Debug.Log ("Checker " + checker.ToString ());
//			GameObject checker_object = dotParent.Find (checker.ToString ()).gameObject;
////			Debug.Log("Changing name from " + checker_object.name + " To " + (checker + 5).ToString());
//			checker_object.name = (checker + 5).ToString ();
//		}

		GameObject new_Object = Instantiate (DotObject, dotParent) as GameObject;
		new_Object.transform.position = new Vector3 (key.transform.position.x, 2.9f, 90);
		new_Object.name = (name % 5).ToString ();

	}

	IEnumerator DisableKey(GameObject key){
		key.name = "disabling";
		key.GetComponent<Rigidbody2D> ().isKinematic = true;
		key.GetComponent<BoxCollider2D> ().enabled = false;
		key.GetComponent<Animator> ().SetBool ("die", true);
		yield return new WaitForSeconds (0.3f);
		Destroy (key);
	}

	int ClosestTo(int number){
//		Debug.Log (number.ToString ());
		float x = number;
		int count = 0;

		while (x > 2f) {
			x = x / 2;
			count++;
		}


	//	count = Mathf.Min (3, count);
//		int returnvalue = (int)Mathf.Pow (2, count);
//		Debug.Log (returnvalue.ToString ());

		count = Mathf.Min (11, count);

		return count;
	}

	void DeactivateChain(){
		if (currentChain.Count > 0) {
			foreach (var item in currentChain) {
				if (item != null) {
					item.transform.GetComponent<DotScript> ().ChildImage.gameObject.SetActive (false);
					item.GetComponent<DotScript> ().selected = false;
				}
			}
		}
		currentChain.Clear ();
		MainLine.positionCount = 0;
		touchStarted = false;
	//	MainLine.gameObject.SetActive (false);
		currentChain.Clear ();
	}


	void LoadTheLevel(){
//		for (int i = 0; i < 25; i++) {
//			dotParent.Find (i.ToString ()).GetComponent<DotScript> ().SetRandomDot ();
//		}
	}

	void Reset_Dots(){
		
	}

	void UpdateScore(int add){
		Score += add;
		ScorePlate.text = "Score " + System.Environment.NewLine + Score.ToString ();
	}

	bool Neighbour(string currentName){
		try {
			if (currentChain.Count == 0) {
				return true;
			} else {
				if (Mathf.Abs (int.Parse (currentChain[currentChain.Count - 1].name) - int.Parse (currentName)) == 1  && int.Parse(currentChain[currentChain.Count - 1].name)/5 == int.Parse(currentName)/5) {
					return true;
				}
				if (Mathf.Abs (int.Parse (currentChain[currentChain.Count - 1].name) - int.Parse (currentName)) == 5){
					return true;
				}
			}
		} catch (System.Exception ex) {
			Debug.Log (ex.ToString ());
		}

		return false;
	}

	bool isNoMove(){
		for (int i = 0; i < 25; i++) {
			GameObject item = dotParent.Find (i.ToString ()).gameObject;
			int name = int.Parse (item.name);

			GameObject temp_item = null;
			// checking left
			if(name - 1 >= 0 && name-1 < 25){
				temp_item = dotParent.Find((name-1).ToString()).gameObject;
				if (item.GetComponent<DotScript> ().currentvalue == temp_item.GetComponent<DotScript> ().currentvalue && (int.Parse (item.name) / 5) == (int.Parse (temp_item.name) / 5)) {
					return false;
				}
			}

			// checking right
			if(name + 1 >= 0 && name +1 < 25){
				temp_item = dotParent.Find((name+1).ToString()).gameObject;
				if (item.GetComponent<DotScript> ().currentvalue == temp_item.GetComponent<DotScript> ().currentvalue && (int.Parse (item.name) / 5) == (int.Parse (temp_item.name) / 5)) {
					return false;
				}
			}

			// checking Up
			if(name - 5 >= 0 && name - 5 < 25){
				temp_item = dotParent.Find((name-5).ToString()).gameObject;
				if (item.GetComponent<DotScript> ().currentvalue == temp_item.GetComponent<DotScript> ().currentvalue) {
					return false;
				}
			}

			// checking Down
			if(name + 5 >= 0 && name + 5 < 25){
				temp_item = dotParent.Find((name+5).ToString()).gameObject;
				if (item.GetComponent<DotScript> ().currentvalue == temp_item.GetComponent<DotScript> ().currentvalue) {
					return false;
				}
			}

		}
		return true;
	}

	void SetKinematic(bool t){
		for (int i = 0; i < 25; i++) {
			dotParent.Find (i.ToString ()).GetComponent<Rigidbody2D> ().isKinematic = t;
		}
	}

	void do_Gameover(){
		m.Play_Win ();
		if (Score > PlayerPrefs.GetInt ("bestscore")) {
			PlayerPrefs.SetInt ("bestscore", Score);
		}
		isPaused = true;
		gameOverPanel.GetComponent<Animator> ().SetBool ("show", true);
		if (m.getLastLevel () < m.currentLevel) {
			m.setLastLevel (m.currentLevel);
		}
		pauseButton.GetComponent<Button> ().interactable = false;
	}

	public void OnPauseClick(){
		m.Play_Click ();
		isPaused = true;
		pauseButton.gameObject.SetActive (false);
		powerButton.gameObject.SetActive (false);
		changeSkinButton.gameObject.SetActive (false);
		pausePanel.GetComponent<Animator> ().SetBool ("show", true);
	}

	public void OnResumeClick(){
		m.Play_Click ();
		pauseButton.gameObject.SetActive (true);
		powerButton.gameObject.SetActive (true);
		pausePanel.GetComponent<Animator> ().SetBool ("show", false);
		changeSkinButton.gameObject.SetActive (true);
		isPaused = false;
	}

	public void OnPowerPanelClick(){
		m.Play_Click ();
		isPaused = true;
		pauseButton.gameObject.SetActive (false);
		powerButton.gameObject.SetActive (false);
		changeSkinButton.gameObject.SetActive (false);
		powerPanel.GetComponent<Animator> ().SetBool ("show", true);
	}

	public void OnPowerPanelClose(){
		pauseButton.gameObject.SetActive (true);
		powerButton.gameObject.SetActive (true);
		powerPanel.GetComponent<Animator> ().SetBool ("show", false);
		changeSkinButton.gameObject.SetActive (true);
		isPaused = false;
	}

	public void OnDestroyPowerClick(){
		m.Play_Click ();
		if (m.powerChrage >= 40) {
			m.UpdatePowerCharge (powerPlate, -40);
		} else {
			return;
		}
		currentPower = Powers.Destroy;
		powerPanel.GetComponent<Animator> ().SetBool ("show", false);
		DestroPowerHelper.SetActive (true);
	}

	public void OnSwitchPowerClick(){
		m.Play_Click ();
		if (m.powerChrage >= 40) {
			m.UpdatePowerCharge (powerPlate, -40);
		} else {
			return;
		}
		currentPower = Powers.Switch;
		powerPanel.GetComponent<Animator> ().SetBool ("show", false);
		SwitchPowerHelper.SetActive (true);
	}

	public void OnScrambleClick(){
		m.Play_Click ();
		if (m.powerChrage >= 80) {
			m.UpdatePowerCharge (powerPlate, -80);
		} else {
			return;
		}
		currentPower = Powers.Scramble;
		powerPanel.GetComponent<Animator> ().SetBool ("show", false);

		List<int> values = new List<int> ();
		List<bool> offers = new List<bool> ();

		for (int i = 0; i < 25; i++) {
			values.Add(dotParent.Find(i.ToString()).GetComponent<DotScript>().currentIndex);
			offers.Add (dotParent.Find (i.ToString ()).GetComponent<DotScript> ().isOfferingCoin);
		}

		for (int j = 0; j < 25; j++) {
			int p = Random.Range (0, values.Count);
			int v = values [p];
			bool b = offers [p];
			values.RemoveAt (p);
			offers.RemoveAt (p);

			dotParent.Find (j.ToString ()).GetComponent<DotScript> ().SetScrambledNew (v, b);
		}

		currentPower = Powers.None;
		OnPowerPanelClose ();
	}

	public void OnChangePowerClick(){
		m.Play_Click ();
		if (m.powerChrage >= 50) {
			m.UpdatePowerCharge (powerPlate, -50);
		} else {
			return;
		}
		currentPower = Powers.Change;
		powerPanel.GetComponent<Animator> ().SetBool ("show", false);
		ChangePowerHelper.SetActive (true);
	}

	public void OnChangePowerButtonsClick(int index){
		m.Play_Click ();
		ChangePowerPanel.GetComponent<Animator> ().SetBool ("show", false);
		switchObjects [0].GetComponent<DotScript> ().SetScrambledNew (index, switchObjects[0].GetComponent<DotScript>().isOfferingCoin);

		temp_switch_int = 0;
		switchObjects.Clear ();
		currentPower = Powers.None;

		OnPowerPanelClose ();
	}

	public void CancelSwitchPower(){
		m.Play_Click ();
		currentPower = Powers.None;
		foreach (var item in switchObjects) {
			item.GetComponent<Animator> ().SetBool ("inc", false);
			item.GetComponent<Animator> ().SetBool ("dec", true);
			item.GetComponent<DotScript> ().Invoke ("SetDecFalse", 1f);
		}
		SwitchPowerHelper.SetActive (false);
		temp_switch_int = 0;
		switchObjects.Clear ();

		m.UpdatePowerCharge (powerPlate, 40);

		OnPowerPanelClose ();
	}

	public void CancleDestroyPower(){
		m.Play_Click ();
		currentPower = Powers.None;

		DestroPowerHelper.SetActive (false);
		temp_switch_int = 0;
		switchObjects.Clear ();

		m.UpdatePowerCharge (powerPlate, 40);

		OnPowerPanelClose ();
	}

	public void CancleChangePower(){
		m.Play_Click ();
		currentPower = Powers.None;

		ChangePowerHelper.SetActive (false);
		if (switchObjects.Count > 0) {
			ChangePowerPanel.GetComponent<Animator> ().SetBool ("show", false);
		}

		temp_switch_int = 0;
		switchObjects.Clear ();

		m.UpdatePowerCharge (powerPlate, 50);

		OnPowerPanelClose ();
	}

	public void OnHomeClick(){
		m.Play_Click ();
		SceneManager.LoadScene ("MainMenu");
	}

	public void OnLevelSelectionClick(){
		m.Play_Click ();
		SceneManager.LoadScene ("MainMenu");
	}

	public void OnNextClick(){
		m.Play_Click ();
		SceneManager.LoadScene ("Gameplay");
	}

	public void UpdateNames(){
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				int k = (5*i + j);
				Vector3 v = Camera.main.WorldToScreenPoint (firstpos + new Vector3(xGap*j,i*(-1*yGap),0));

				RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(v), Vector2.zero);

				if (hit.collider != null && hit.collider.gameObject.name != k.ToString ()) {
					//	Debug.Log ("Target Position: " + hit.collider.gameObject.transform.position);
				//	Debug.Log (hit.collider.gameObject.name);
				//	Debug.Log ("Changed name From " + hit.collider.gameObject.name);
					hit.collider.gameObject.name = k.ToString ();
				//	Debug.Log ("To " + hit.collider.gameObject.name);
				} else if (hit.collider != null) {
				//	Debug.Log (hit.collider.gameObject.name);
				}

			}

		}
	}

	public void OnChangeSkinPanelShow(){
		m.Play_Click ();
		isPaused = true;
		pauseButton.gameObject.SetActive (false);
		powerButton.gameObject.SetActive (false);
		changeSkinButton.gameObject.SetActive (false);
		ChangeSkinPanel.GetComponent<Animator> ().SetBool ("show", true);
	}

	public void OnChangeSkinPanelClose(){
		m.Play_Click ();
		isPaused = false;
		pauseButton.gameObject.SetActive (true);
		powerButton.gameObject.SetActive (true);
		changeSkinButton.gameObject.SetActive (true);
		ChangeSkinPanel.GetComponent<Animator> ().SetBool ("show", false);
	}

	public void OnSelectSkinClick(int i){
		ApplySkin (i);
		OnChangeSkinPanelClose ();
		PlayerPrefs.SetInt ("currentskin", i);
		foreach (var item in SkinSelectButtons) {
			item.transform.GetChild (0).GetComponent<Text> ().text = "Select";
		}

		SkinSelectButtons [i].transform.GetChild (0).GetComponent<Text> ().text = "Selected";
	}

	void ApplySkin(int j){
		Sprite s = skinImages [j];
		for (int i = 0; i < dotParent.childCount; i++) {
			Image img = dotParent.transform.GetChild (i).GetComponent<Image> ();
			img.sprite = s;
			img.transform.GetChild (0).GetComponent<Image> ().sprite = s;
		}

	}

	public void OnAnySkinBuyClick(int i){
		if (SkinCost [i] <= m.powerChrage) {
			m.UpdatePowerCharge (powerPlate, -1 * SkinCost [i]);
			PlayerPrefs.SetInt ("skin_" + i.ToString (), 1);
			PlayerPrefs.SetInt ("currentskin", i);
			UpdateSkinButtons ();
		}
	}

	void UpdateSkinButtons(){
		for (int i = 0; i < SkinSelectButtons.Count; i++) {
			int j = PlayerPrefs.GetInt ("skin_" + i.ToString ());
			if (j == 1) {
				BuySkinButtons [i].gameObject.SetActive (false);
				SkinSelectButtons [i].gameObject.SetActive (true);
			} else {
				BuySkinButtons [i].gameObject.SetActive (true);
				SkinSelectButtons [i].gameObject.SetActive (false);
			}
		}

		OnSelectSkinClick (PlayerPrefs.GetInt ("currentskin"));
	}
}
