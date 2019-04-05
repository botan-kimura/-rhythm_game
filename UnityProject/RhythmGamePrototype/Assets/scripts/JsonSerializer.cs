using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using System.IO;

/// <summary>
/// DictionaryデータとJsonデータ(string)の変換を行う
/// 暗号化と複合化も同時に行う
/// </summary>
public static class JsonSerializer
{

	//保存するディレクトリ名
	private const string DIRECTORY_NAME = "Data";


//ファイルのパスを取得
	private static string GetFilePath(string fileName)
	{

		string directoryPath = "./Assets";

		//ディレクトリが無ければ作成
		if (!Directory.Exists (directoryPath)) {
			Directory.CreateDirectory (directoryPath);
		}

		string filePath = directoryPath + "/" + directoryPath;

		return filePath;
	}
    
	/// <summary>
	/// Dictionaryデータをjson形式に変換して保存する
	/// </summary>
	/// <param name="dic">保存するDictionary<string, object>データ</param>
	/// <param name="fileName">保存ファイル名</param>
	public static void Save(string jsonstr, string fileName)
	{
		
		//string jsonstr = Json.Serialize (dic);
		Debug.Log ("serialized text = " + jsonstr);
		jsonstr = jsonstr + "\n" + "]";
		//string filePath = GetFilePath(fileName);
		string filePath = Application.dataPath + @"\Scripts\File\test.txt";;
		File.WriteAllText (filePath, jsonstr);


		Debug.Log ("saveFilePath = " + filePath);
	}
}