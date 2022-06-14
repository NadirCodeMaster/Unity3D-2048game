using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DotScript : MonoBehaviour , IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {

	public int currentvalue = 0;
	public int currentIndex = 0;
	public bool isOfferingCoin;
	public bool selected;
	public int[] dotvalues = new int[11]{2,4,8,16,32,64,128,256,512,1024,2048};

	public Image ChildImage;
	public Text ChildText;
	public GameObject PowerCoin;

	bool move;
	Vector3 target;

	Manager m;

	void Start(){
		m = GameObject.Find ("m").GetComponent<Manager> ();

		if (currentvalue == 0) {
			SetRandomDot ();
		}
	}

	public void SetRandomDot(){
		int i = Random.Range (0, 3);
		int j = Random.Range (0, 10);

		if (j == 0) {
			isOfferingCoin = true;
			PowerCoin.SetActive (true);
		} else {
			isOfferingCoin = false;
			PowerCoin.SetActive (false);
		}

		SetTileWithIndex (i);
	}

	public void OnPointerDown(PointerEventData data)
	{
	//	if (GameplayManager.instance.isPaused)return;
		if (data.pointerCurrentRaycast.gameObject == null)	return;
		
		GameplayManager.instance.OnAnyDotDown (data.pointerCurrentRaycast.gameObject);
	}

	public void OnPointerUp(PointerEventData data)
	{
	//	if (data.pointerCurrentRaycast.gameObject == null)	return;
	//	Debug.Log ("Pointing up at " + data.pointerCurrentRaycast.gameObject.name);

	//	if (GameplayManager.instance.isPaused)return;

		if (data.pointerCurrentRaycast.gameObject != null && data.pointerCurrentRaycast.gameObject.transform.parent!= null && data.pointerCurrentRaycast.gameObject.transform.parent.name == "DotParent") {
			GameplayManager.instance.OnAnyDotUp (data.pointerCurrentRaycast.gameObject);
		} else {
			if (GameplayManager.instance.currentChain.Count > 0) {
				GameplayManager.instance.OnAnyDotUp (GameplayManager.instance.currentChain [GameplayManager.instance.currentChain.Count - 1]);
			}
		}
	}

	public  void OnPointerEnter(PointerEventData data)
	{
		if (GameplayManager.instance.isPaused)return;
		if (data.pointerCurrentRaycast.gameObject == null)	return;
		if(GameplayManager.instance.touchStarted)
			GameplayManager.instance.OnAnyDotDown (data.pointerCurrentRaycast.gameObject);
	}

	public  void OnPointerExit(PointerEventData data)
	{
//		if (data.pointerCurrentRaycast.gameObject == null)	return;
//		Debug.Log ("r "+  data.pointerCurrentRaycast.gameObject.name);
//
//		if (data.pointerCurrentRaycast.gameObject.transform.parent!= null && data.pointerCurrentRaycast.gameObject.transform.parent.name == "DotParent" && GameplayManager.instance.touchStarted) {
//			Debug.Log ("Exiting Something ");
//			GameplayManager.instance.OnAnyDotExit (data.pointerCurrentRaycast.gameObject);
//		}
	}

	public void SetTileWithIndex(int index){
		currentvalue = dotvalues [index];
		currentIndex = index;
		GetComponent<Image> ().color = GameplayManager.instance.TileColors [index];
		ChildImage.color = GameplayManager.instance.TileColors [index];
		ChildText.text = dotvalues [index].ToString ();
	}

//	public void SetTileWithValue(int value){
//		GetComponent<Image> ().color = GameplayManager.instance.TileColors [value];
//		ChildImage.color = GameplayManager.instance.TileColors [value];
//		ChildText.text = dotvalues [value].ToString ();
//	}

	public void SetScrambledNew(int index, bool coinOffer){
		
		if (coinOffer) {
			isOfferingCoin = true;
			PowerCoin.SetActive (true);
		} else {
			isOfferingCoin = false;
			PowerCoin.SetActive (false);
		}

		GetComponent<Animator> ().SetBool ("spawn", true);

		currentvalue = dotvalues [index];
		currentIndex = index;
		GetComponent<Image> ().color = GameplayManager.instance.TileColors [index];
		ChildImage.color = GameplayManager.instance.TileColors [index];
		ChildText.text = dotvalues [index].ToString ();

		Invoke ("SetSpawnFalse", 0.5f);
	}

	void SetSpawnFalse(){
		GetComponent<Animator> ().SetBool ("spawn", false);
	}

	public void MoveTo(Vector3 postion){
		target = postion;
		move = true;
	}

	void Update(){
		if (move && Vector3.Distance (transform.position, target) > 0.1f) {
			transform.position = Vector3.MoveTowards (transform.position, target, 0.09f);
		} else if(move && Vector3.Distance (transform.position, target) < 0.1f){
			move = false;
			transform.position = target;
			GetComponent<Rigidbody2D> ().isKinematic = false;
			GetComponent<Animator> ().SetBool ("inc", false);
			GetComponent<Animator> ().SetBool ("dec", true);
			Invoke ("SetDecFalse", 1f);
			GameplayManager.instance.SwitchComplete (gameObject);
		}
	}

	void SetDecFalse(){
		GetComponent<Animator> ().SetBool ("dec", false);
	}
}
