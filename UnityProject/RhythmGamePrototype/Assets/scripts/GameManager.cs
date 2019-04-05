using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
using UnityEngine.UI;
using MiniJSON;		// Json
using System;
// File
using System.IO;
using System.Linq;
using System.Net.Mime;
// File
using System.Text;	// File
using UnityEngine.Networking;
using UnityEngine.Playables;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
	//曲名はボタンから受け取る。そのデータから検索して、上三つはJSONデータから受け取れるようにしたい
	[SerializeField] private List<int> type;
	[SerializeField] private List<float> time;
	[SerializeField] private int[] bpm;
	[SerializeField] private string[] kyokumei;
	//リズムゲーム本体に必要なもの
	[SerializeField] private Text hantei;
	[SerializeField] private Text scoretxt;
	[SerializeField] private float timer;
	[SerializeField] private int allnotes;
	[SerializeField] private int noteNum;
	[SerializeField] private float misstime;
	[SerializeField] private float goodtime;
	[SerializeField] private float perfecttime;
	[SerializeField] private AudioSource BGM;
	[SerializeField] private ParticleSystem particlePerfect;
	[SerializeField] private ParticleSystem particleGood;
	[SerializeField] private float missdelaytime;
	[SerializeField] private float particletime;
	private int combocounter;
	private float score;
	private float holdtimer;
	public float holdtime;//(note落下に使う)
	private bool holdflag;
	private bool missflag;
	//noteを降らせる関連
	[SerializeField] private GameObject noteprefab;
	[SerializeField] private List<GameObject> notes;
	[SerializeField] private int poolnotes;
	//ポーズ関連
	[SerializeField] private GameObject pauseCanvas;
	[SerializeField] private GameObject playingCanvas;
	[SerializeField] private GameObject resultCanvas;
	[SerializeField] private GameObject MenuCanvas;
	//ロード関連
	[SerializeField]
	public IList results;
	public List<FumenData>	_curFumen	= null;
	//ストーリー関連
	public int story;
	[SerializeField] private float[] storyScore;
	[SerializeField] private GameObject[] storyButton;
	[SerializeField] private Text PlayingStory;
	//フェイリアちゃん関連
	[SerializeField] private GameObject Failure;
	private Animator anim;
	private bool nowTap;
	//メニュー画面
	[SerializeField] private GameObject menuCanvas;
	private int kyokuNum;
	[SerializeField] private int kyokusuu;
	//リザルト関連
	[SerializeField] private float Shantei;
	[SerializeField] private float Ahantei;
	[SerializeField] private float Bhantei;
	[SerializeField] private Text resultHantei;
	[SerializeField] private Text resultScore;
	//コンフィグ関連
	[SerializeField] private GameObject configCanvas;
	[SerializeField] private Text tutorial;
	//ボタンにハイスコア表示
	[SerializeField] private Text[] highscores;
	//チュートリアル関連
	private string[] tutorialStrings;
	//キャリブレーション関連
	[SerializeField] private GameObject callibration;
	[SerializeField] private float callibrate;
	//カメラワーク関連
	[SerializeField] private GameObject ImpulseSource;
	[SerializeField] private GameObject[] VCam;
	
	// Use this for initialization
	private void Start ()
	{
		shokika();
		toMenu();
		if (PlayerPrefs.HasKey("Story")==false)
		{
			Shokaikidou();
		}
		else
		{
			story = PlayerPrefs.GetInt("Story");
			callibrate = PlayerPrefs.GetFloat("Calibration");
			SetJsonFromWww();
		}
	}
	
	// Update is called once per frame
	private void Update ()
	
	{
		if (playingCanvas.activeSelf)
		{
			timer += Time.deltaTime;
			
			//曲が終わったらリザルト処理
			if (timer >= time[allnotes-1])
			{
				playingCanvas.SetActive(false);
				resultShori();
			}
			else
			{

				//ホールドの時間を計る
				if (holdflag)
				{
					holdtimer += Time.deltaTime;
				}

				//noteを降らせる
				for (int i = 0; i<=9;i++)
				{
					if (time.Count-2 >= noteNum + i && timer >= time[noteNum + i] - holdtime*2f && type[noteNum + i] <= 1)
					{
					
						GameObject fallnote = GetNote();
						fallnote.SetActive(true);
						if (type[noteNum + i] == 1)
						{
							fallnote.GetComponent<noteManager>().noteHold();
						}

						type[noteNum + i] += 2;
					}
				}

				//タップされたら判定する
				if (Input.GetKeyDown(KeyCode.Space) && type[noteNum] == 2 && missflag == false)
				{
					if (nowTap == false && holdflag == false)
					{
						anim.SetTrigger("Tsujou");
						anim.ResetTrigger("Idle");
						StartCoroutine(motionController(0));
					}

					Hantei();
				}

				//ホールド始め、フラグオン、終わり、フラグオフ&判定
				if (Input.GetKeyDown(KeyCode.Return) && type[noteNum] == 3 &&
				    Mathf.Abs(timer - time[noteNum]) <= misstime && missflag == false)
				{
					holdflag = true;
					anim.ResetTrigger("Idle");
					anim.ResetTrigger("Tsujou");
					anim.SetTrigger("Tame");
					StartCoroutine(motionController(1));
					time[noteNum] += holdtime;
				}

				if (Input.GetKeyUp(KeyCode.Return) && holdflag == true && type[noteNum] == 3 && missflag == false)
				{
					holdflag = false;
					holdtimer = 0;
					Hantei();
				}
			}
		}
	}

	//GoodとPerfectの判定、スコア加算
	private void Hantei()
	{
			//キャリブレーション中
			if (callibration.activeSelf)
			{
				callibrate += timer - time[noteNum];
				if (noteNum == 3)
				{
					resultShori();
				}
			}
			else
			{
				//チュートリアル中
				if (tutorial.text != "" && noteNum >=3)
				{
					tutorial.text =tutorialStrings[1];
				}
				//それ以外
				if (Mathf.Abs(timer - time[noteNum]+callibrate) <= perfecttime)
				{
					//perfect
					score += 900000f / (allnotes-2);
					if (combocounter != 0)
					{
						score += 100000f / (((allnotes - 3) * ((allnotes -2)/ 2)) * combocounter);
					}

					combocounter += 1;
					hantei.text = "Perfect";
					scoretxt.text = Mathf.CeilToInt(score).ToString();
					type[noteNum] += 20;
					StartCoroutine(particlesPlay(0));
				}
				else if (Mathf.Abs(timer - time[noteNum]+callibrate) <= goodtime)
				{
					//good
					score += 630000f / (allnotes-2);
					if (combocounter != 0)
					{
						score += 100000f / (((allnotes-3)* ((allnotes-2) / 2)) * combocounter);
					}

					combocounter += 1;
					hantei.text = "Good";
					type[noteNum] += 10;
					scoretxt.text = Mathf.CeilToInt(score).ToString();
					StartCoroutine(particlesPlay(1));
				}
				else
				{
					//miss
					StartCoroutine(Miss());
				}
			}
	}

	//ミスしたとき
	private IEnumerator Miss()
	{
		hantei.text = "Miss";
		combocounter = 0;
		//以下連打対策
		missflag = true;
		yield return new WaitForSeconds(missdelaytime);
		missflag = false;
	}
	
	//ポーズが押されたとき
	public void pause()
	{
		if (playingCanvas.activeSelf)
		{
			Time.timeScale = 0;
			pauseCanvas.SetActive(true);
			playingCanvas.SetActive(false);
			BGM.Pause();
		}
	}
	//resumeが押されたとき
	public void resume()
	{
		if (pauseCanvas.activeSelf)
		{
			Time.timeScale = 1;
			pauseCanvas.SetActive(false);
			playingCanvas.SetActive(true);
			BGM.Play();
		}
	}
	//retryが押されたとき
	public void shokika()
	{
		//pauseしていたらpauseをやめさせる
		if (pauseCanvas.activeSelf)
		{
			Time.timeScale = 1;
		}
		Failure.transform.position = new Vector3(0,-0.2f,-8.5f);
		//ホールドは二拍分の長さにする
		holdtime = 120f / bpm[kyokuNum];
		holdflag = false;
		noteNum = 1;
		allnotes = 0;
		timer = 0;
		score = 0;
		scoretxt.text = "";
		hantei.text = "";
		//カメラ関連
		ImpulseSource.SetActive(false);
		for (int i = 0; i <= kyokusuu; i++)
		{
			VCam[i].SetActive(false);
		}
		VCam[kyokuNum].SetActive(true);
		VCam[kyokuNum].GetComponent<PlayableDirector>().Play();
		fumenLoading(kyokumei[kyokuNum]);
		time = new List<float>();
		type = new List<int>();
		foreach(FumenData one in _curFumen)
		{
			time.Add(one.time);
			type.Add(one.type);
			allnotes += 1;
		}
		AudioClip audio = Resources.Load(kyokumei[kyokuNum],typeof(AudioClip)) as AudioClip;
		BGM.clip = audio;
		BGM.time = 0f;
		BGM.Play();
		playingCanvas.SetActive(true);
		pauseCanvas.SetActive(false);
		menuCanvas.SetActive(false);
		resultCanvas.SetActive(false);
		//animation関連TsujouSpeed
		anim = Failure.GetComponent<Animator>();
		anim.SetFloat("TameSpeed", 2f/holdtime);
		anim.SetFloat("TsujouSpeed", 1f / holdtime);
		//キャリブレーション時の特別処理
		if (callibration.activeSelf)
		{
			allnotes = 4;
		}
	}
	public void fumenReset()
	{
		_curFumen	= null;
	}

	public void fumenLoading(string fileName)
	{
		fumenReset();

		TextAsset txt = Instantiate(Resources.Load(fileName)) as TextAsset;
		string jsonString = txt.text;

		//譜面保存用のリストを確保する。
		_curFumen = new List<FumenData>();

		//IListのresultsにデシリアライズしたjsonをつっこむ
		var json = (IList) Json.Deserialize(jsonString);
		results = (IList) json;
		FumenData curData; //譜面の１要素
		foreach (IDictionary resultOne in results)
		{
			curData = new FumenData(); //譜面の１要素

			//それぞれのタイプを確認してみる。
			//Debug.Log(resultOne["noteNum"].GetType()); //Int64
			//Debug.Log(resultOne["time"].GetType()); //Double
			//Debug.Log(resultOne["type"].GetType()); //Int64

			//キャストしてクラスの変数に格納する。
			curData.noteNum = (int) ((long) resultOne["noteNum"]);
			curData.time = (float) ((double) resultOne["time"]);
			curData.type = (int) ((long) resultOne["type"]);

			_curFumen.Add(curData); //譜面リストに追加
		}
	}

	//パーティクル再生
	IEnumerator particlesPlay(int type)
	{ 
		ImpulseSource.SetActive(true);
		switch (type)
		{
			case 0:
				particlePerfect.Play();
				yield return new WaitForSeconds(particletime);
				particlePerfect.Stop();
				break;
			case 1:
				particleGood.Play();
				yield return new WaitForSeconds(particletime);
				particleGood.Stop();
				break;
		}
	}
	//フェイリアちゃんモーション関係
	IEnumerator motionController(int type)
	{
		switch (type)
		{
			case 0:
				nowTap = true;
				yield return new WaitForSeconds(holdtime * 5f);
				nowTap = false;
				Failure.transform.rotation = new Quaternion(0,0,0,0);
				Failure.transform.position = new Vector3(0,-0.2f,-8.5f);
				anim.ResetTrigger("Tsujou");
				anim.SetTrigger("Idle");
				break;
			case 1:
				yield return new WaitForSeconds(holdtime * 2.5f);
				Failure.transform.rotation = new Quaternion(0,0,0,0);
				Failure.transform.position = new Vector3(0,-0.2f,-8.5f);
				anim.ResetTrigger("Tame");
				anim.SetTrigger("Idle");
				break;
		}
	}
	//ハイスコア保存
	private void HighScore()
	{
		float highscore = PlayerPrefs.GetFloat(kyokumei[kyokuNum]);
		if (highscore <= score)
		{
			highscore = score;
		}
		PlayerPrefs.SetFloat(kyokumei[kyokuNum],highscore);
		PlayerPrefs.Save();
		SetJsonFromWww();
	}
	//note作成
	private void NoteCreate(bool i)
	{
			var newObj = Instantiate(noteprefab);
			newObj.GetComponent<noteManager>().noteActivate(holdtime);
			newObj.GetComponent<noteManager>().GM = this.gameObject;
			notes.Add(newObj);
			newObj.SetActive(i);
	}
	//noteのオブジェクトプーリング
	private GameObject GetNote()
	{
		// 使用中でないものを探して返す
		foreach (var obj in notes)
		{
			if (obj.activeSelf == false)
			{
				obj.SetActive(true);
				return obj;
			}
		}

		// 全て使用中だったら新しく作って返す
		poolnotes += 1;
		NoteCreate(true);
		var newobj = notes[poolnotes];
		return newobj;

	}

	//あとづけさーばーようそ
	private IEnumerator SetMessage( string urlTarget, string name, string message, Action<string> cbkSuccess=null, Action cbkFailed=null ) {

		WWWForm form = new WWWForm();
		form.AddField("Highscoresum",		name);
		form.AddField("Story",	message);

		// WWWを利用してリクエストを送る
		WWW www = new WWW( urlTarget, form );

		// WWWレスポンス待ち
		yield return StartCoroutine( ResponceCheckForTimeOutWWW( www, 5.0f));
		
		if(www.error != null){
			//レスポンスエラーの場合
			Debug.LogError(www.error);
			if(null!=cbkFailed){
				cbkFailed();
			}
		}else
		if(www.isDone){
			// リクエスト成功の場合
			Debug.Log( string.Format("Success:{0}",www.text));
			if(null!=cbkSuccess){
				cbkSuccess( www.text);
			}
		}
/**/
	}
	private IEnumerator ResponceCheckForTimeOutWWW(WWW www, float timeout) {
		float requestTime = Time.time;
		
		while(!www.isDone){
			if(Time.time - requestTime < timeout){
				yield return null;
			}else{
				Debug.LogWarning("TimeOut"); //タイムアウト
				break;
			}
		}
		yield return null;
	}	
	private void SetJsonFromWww(){

		for (int i =0; i<= kyokusuu;i++)
		{
			highscores[i].text = "highscore:"+"\n"+ Mathf.CeilToInt(PlayerPrefs.GetFloat(kyokumei[i])).ToString();
			score += PlayerPrefs.GetFloat(kyokumei[i]);
		}
		string highscoresum = Mathf.CeilToInt(score).ToString();
		string Story = story.ToString();

		// APIが設置してあるURLパス
		string	sTgtURL	= "http://localhost/sendsystem/Send/setMessages";
		// Wwwを利用して json データ取得をリクエストする
		//StartCoroutine(SetMessage( sTgtURL, highscoresum, Story, CallbackApiSuccess, CallbackWwwFailed));

	}
	
	private void CallbackWwwFailed() {

		// jsonデータ取得に失敗した
		Debug.Log("Failed");
	}
	private void CallbackApiSuccess( string response) {

		// json データ取得が成功したのでデシリアライズして整形し画面に表示する
		
		Debug.Log("Success");

	}
	//リザルト処理
	private void  resultShori()
	{
		//キャリブレーションorチュートリアルのとき
		if (kyokuNum == 4)
		{
			if (tutorial.text != "")
			{
				resultHantei.text = "Let's Play!";
				tutorial.text = "";
			}

			if (callibration.activeSelf)
			{
				callibrate /= -3f;
				PlayerPrefs.SetFloat("Calibration",callibrate);
				PlayerPrefs.Save();
				resultHantei.text = "Calibration Complete!";
				callibration.SetActive(false);
			}
			resultScore.text = "";
		}
		//それ以外
		else
		{
			resultScore.text = Mathf.CeilToInt(score).ToString();
			if (score >= Shantei)
			{
				resultHantei.text = kyokumei[kyokuNum] + ":S";
			}
			else if (score >= Ahantei)
			{
				resultHantei.text = kyokumei[kyokuNum] + ":A";
			}
			else if (score >= Bhantei)
			{
				resultHantei.text = kyokumei[kyokuNum] + ":B";
			}
			else
			{
				resultHantei.text = kyokumei[kyokuNum] + ":C";
			}
			HighScore();

			int j = story;
			for (int i = 0; i <= storyScore.Length - 1; i++)
			{
				if (score >= storyScore[i] && story <= i && story != storyScore.Length - 1)
				{
					story = i;
				}
			}

			if (j != story)
			{
				PlayerPrefs.SetInt("Story", story);
				PlayerPrefs.Save();
				resultScore.text += "\n" + "Unlock New Episode";
			}

		}
		pauseCanvas.SetActive(true);
		resultCanvas.SetActive(true);
	}
	//メニュー画面への遷移
	public void toMenu()
	{
		//pauseしていたらpauseをやめさせる
		if (pauseCanvas.activeSelf)
		{
			Time.timeScale = 1;
		}
		storyButton[0].SetActive(false);
		playingCanvas.SetActive(false);
		pauseCanvas.SetActive(false);
		resultCanvas.SetActive(false);
		configCanvas.SetActive(false);
		MenuCanvas.SetActive(true);
		AudioClip audio = Resources.Load(kyokumei[4],typeof(AudioClip)) as AudioClip;
		BGM.clip = audio;
		BGM.time = 0f;
		BGM.Play();
		//ストーリー関連
		for (int i =0;i<=storyScore.Length -1;i++)
		{
			storyButton[i].SetActive(false);
		}
	}
	//メニュー画面でボタン押下
	public void SongChosen(int song)
	{
		kyokuNum = song;
		shokika();
		for (int i = 0; i <= poolnotes; i++)
		{
			NoteCreate(false);
		}

	}
	//一番最初の起動
	private void Shokaikidou()
	{
		for (int i = 0; i <= kyokusuu;i++) {
			PlayerPrefs.SetFloat(kyokumei[i], 0f);
		}
		PlayerPrefs.SetInt("Story",1);	
		PlayerPrefs.SetFloat("Calibration",-0.6788552f);
		PlayerPrefs.Save();
		SetJsonFromWww();
		Config();
	}

	public void StoryChosen(int storynum)
	{
		PlayingStory.text = "";
		if (storynum == 0)
		{
			for (int i =0;i<=storyScore.Length;i++)
			{
				if (story >= i)
				{
					storyButton[i].SetActive(true);
				}
			}
		}
		else
		{
			PlayingStory.text = "Now Playing ";
			var storyName = storyButton[storynum].GetComponentInChildren<Text>();
			PlayingStory.text += storyName.text;
			AudioClip audio = Resources.Load("story"+storynum,typeof(AudioClip)) as AudioClip;
			BGM.clip = audio;
			BGM.time = 0f;
			BGM.Play();
		}
	}

	public void Config()
	{
			configCanvas.SetActive(true);
	}

	public void Calibration()
	{
		callibration.SetActive(true);
		callibrate = 0f;
		SongChosen(4);
	}

	public void Tutorial()
	{
		tutorialStrings = new string[2];
		tutorialStrings.SetValue("White Cube is in the sphere," + "\n" + "Tap Space",0);
		tutorialStrings.SetValue("While Black Cube is in the sphere," + "\n" + "Hold Enter",1);
		tutorial.text = tutorialStrings[0];
		SongChosen(4);
	}

	//基盤システムを直すためにそのいち
	public void NoteNumUp()
	{

		StartCoroutine(numup());
	}
	//基盤システムを直すためにそのに
	private IEnumerator numup()
	{
		yield return new WaitForSeconds(misstime);
		if (type[noteNum] <= 9 && callibration.activeSelf == false)
		{
			hantei.text = "Miss";
			combocounter = 0;
		}
		
		noteNum += 1;
	}
	//ポーズからメニューに行くときのバグをなくす
	public void pauseTomenu()
	{
		callibration.SetActive(false);
		tutorial.text = "";
		toMenu();
	}
}
