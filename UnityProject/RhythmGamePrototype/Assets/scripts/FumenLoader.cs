using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON; 

public class FumenLoader : MonoBehaviour
{
	[SerializeField]
	public IList results;

	//読み込んだ譜面を保存しておくリスト
	//※本来はマネージャなりに持たせた方が良い。
	public List<FumenData>	_curFumen	= null;


	// Use this for initialization
	void Start ()
	{
		fumenLoading("test");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void fumenReset()
	{
		_curFumen	= null;
	}

	public void fumenDebugDsip()
	{
		foreach(FumenData one in _curFumen){
			Debug.Log(
				string.Format("note Num:{0} time:{1} type:{2}",
					one.noteNum,
					one.time,
					one.type)	);
		}
	}

	public void fumenLoading(string fileName)
	{
		fumenReset();

		TextAsset txt = Instantiate(Resources.Load(fileName)) as TextAsset;
		string jsonString = txt.text;

		//譜面保存用のリストを確保する。
		_curFumen	= new List<FumenData>();

		//IListのresultsにデシリアライズしたjsonをつっこむ
		var json = (IList)Json.Deserialize(jsonString);
		results = (IList)json;
		Debug.Log(results.Count);
		FumenData	curData;	//譜面の１要素
		foreach (IDictionary resultOne in results)
		{
			curData	= new FumenData();	//譜面の１要素

			//それぞれのタイプを確認してみる。
			Debug.Log( resultOne["noteNum"].GetType() );	//Int64
			Debug.Log( resultOne["time"].GetType() );		//Double
			Debug.Log( resultOne["type"].GetType() );		//Int64

			//キャストしてクラスの変数に格納する。
			curData.noteNum	= (int)(	(long)resultOne["noteNum"]	);
			curData.time	= (float)(	(double)resultOne["time"]	);
			curData.type	= (int)(	(long)resultOne["type"]		);

			_curFumen.Add(curData);	//譜面リストに追加
		}

		fumenDebugDsip();
	}
}