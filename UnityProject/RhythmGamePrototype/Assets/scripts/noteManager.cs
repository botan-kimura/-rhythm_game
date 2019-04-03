using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class noteManager : MonoBehaviour {

	// 落下関連
	[SerializeField] private float fallingspeed;
	[SerializeField] public float fallingtime;
	[SerializeField] private float shokiichi;
	private float timer;

	public GameObject GM;
	//回転関連
	[SerializeField] private float rotspeed;

	private void OnEnable()
	{
		gameObject.transform.position = new Vector3(0,shokiichi,0);
		gameObject.transform.localScale = new Vector3(0,0,0);
		timer = 0;
		GetComponent<Renderer>().material.color = Color.white;
	}

	// Update is called once per frame
	private void Update () {
		transform.Rotate(new Vector3(Random.Range(0, 180), Random.Range(0, 180),Random.Range(0, 180)) * rotspeed * Time.deltaTime);
		timer += Time.deltaTime;
		if (gameObject.transform.position.y >= 0f)
		{
			var rakka = new Vector3(0, fallingspeed, 0);
			gameObject.transform.position -= rakka * Time.deltaTime;
		}
		if (gameObject.transform.position.y <= 0f && GetComponent<Renderer>().material.color == Color.white)
		{
			
			GM.GetComponent<GameManager>().NoteNumUp();
			gameObject.SetActive(false);
		}

		if (gameObject.transform.localScale.x <= 1f)
		{
			gameObject.transform.localScale += new Vector3(0.04f,0.04f,0.04f);
		}
		if (GetComponent<Renderer>().material.color == Color.black && timer >= fallingtime *2f)
		{
			GM.GetComponent<GameManager>().NoteNumUp();
			gameObject.SetActive(false);
			
		}
	}

	public void noteActivate(float holdT)
	{
		gameObject.transform.position = new Vector3(0,shokiichi,0);
		timer = 0;
		fallingtime = holdT;
		fallingspeed = shokiichi / fallingtime;
	}

	public void noteHold()
	{
		GetComponent<Renderer>().material.color = Color.black;
	}
	
}
