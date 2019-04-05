using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeRotate : MonoBehaviour {
	private float rotspeed = 1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
		transform.Rotate(new Vector3(Random.Range(0, 180), Random.Range(0, 180),Random.Range(0, 180)) * rotspeed * Time.deltaTime);	
	}
}
