using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ImpulseManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnEnable()
	{
		GetComponent<CinemachineImpulseSource>().GenerateImpulse();
		StartCoroutine(waiting());
	}

	private IEnumerator waiting()
	{
		yield return new WaitForSeconds(0.4f);
		gameObject.SetActive(false);
	}
}
