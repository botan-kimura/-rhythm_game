using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;	//Text
using MiniJSON;		// Json
using System;		// File
using System.IO;
using System.Runtime.Serialization;

// File
using System.Text;	// File
using UnityEngine.Networking;

[System.Serializable]
public class fumenMaker : MonoBehaviour
{

	private float time;
	private int notenum;
	public List<FumenData> noteList;	//連番なのでDictionaryの必要は無いと思われる。
	public FumenData fumen;

	[SerializeField] private ParticleSystem particlePerfect;
	[SerializeField] private ParticleSystem particleGood;
	
	// Use this for initialization
	private void Start () {
		noteList = new List<FumenData>();
		fumen = null;
		notenum = 0;
	}
	
	// Update is called once per frame
	private void Update ()
	{
		time += Time.deltaTime;
		if (Input.GetKeyDown(KeyCode.Space))
		{
			fumen = new FumenData();

			fumen.time = time;
			fumen.type = 0;
			fumen.noteNum = notenum;

			noteList.Add(fumen);
			
			notenum += 1;
			StartCoroutine(particlesPlay(0));
		}else if (Input.GetKeyDown(KeyCode.Return))
		{
			fumen = new FumenData();

			fumen.time = time;
			fumen.type = 1;
			fumen.noteNum = notenum;

			noteList.Add(fumen);
			
			notenum += 1;
			
			StartCoroutine(particlesPlay(1));
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
//			JsonSerializer.Save(jsonstr,"Arisia");

			//list で保存したデータを key:value のhashtableに変換する。
			//※json化するには key名(文字列) が必要になるので list では駄目。
			List<Hashtable> hashList	= new List<Hashtable>();
			Hashtable hashOne	= null;
			foreach(FumenData noteOne in noteList){
				hashOne	= new Hashtable();
				hashOne.Add(	"time",		noteOne.time	);
				hashOne.Add(	"type",		noteOne.type	);
				hashOne.Add(	"noteNum",	noteOne.noteNum	);

				hashList.Add(hashOne);
			}

			//エンコードしてjsonデータにする。
			string json = Json.Serialize(hashList);

			//ファイルを書き込む
			string path = string.Format("{0}/{1}",Application.persistentDataPath,"noteJson.txt");
			File.WriteAllText( path, json, System.Text.Encoding.UTF8);
			Debug.Log(path);

		}
		
	}
	IEnumerator particlesPlay(int type)
	{
		switch (type)
		{
			case 0:
				particlePerfect.Play();
				yield return new WaitForSeconds(0.3f);
				particlePerfect.Stop();
				break;
			case 1:
				particleGood.Play();
				yield return new WaitForSeconds(0.3f);
				particleGood.Stop();
				break;
		}
	}

}