using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// data save with binaryformatter
// https://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934

// * 모바일에선 Application.dataPath, Application.persistentDataPath 디렉토리 외엔 데이터를
// 저장할수 없다
public static class ManageLevelData
{
	private static string levelDataFilePath => Path.Combine(Application.persistentDataPath, @"levels.json");
	public static Dictionary<int, Level> Levels = new Dictionary<int, Level>();

	#region Default Level Data Path

	private static readonly string pathDefaultLevelDataSave = @"Assets/Resources/Data/default_levels.json";
	private static readonly string pathDefaultLevelDataLoad = @"Data/default_levels";
	#endregion
	
	public static bool GetLevel(int _levelNumber, out Level _level)
	{
		return Levels.TryGetValue(_levelNumber, out _level);
	}
	public static void SaveToFile()
	{
		if (!File.Exists(levelDataFilePath))
		{
			Debug.Log("No Level Data so Create");
		}
		var jsonInfo = JObject.FromObject(Levels);
		File.WriteAllText(levelDataFilePath, jsonInfo.ToString());
#if UNITY_EDITOR
		File.WriteAllText(pathDefaultLevelDataSave, jsonInfo.ToString());
#endif
	}
	
	public static bool LoadFromFile()
	{
		if (File.Exists(levelDataFilePath))
		{
			var jsonString = File.ReadAllText(levelDataFilePath);
			if (!string.IsNullOrEmpty(jsonString))
			{
				ManageLevelData.Levels = JsonConvert.DeserializeObject<Dictionary<int, Level>>(jsonString);
				return true;
			}
		}
		// 데이터가 존재하지 않는다면 (설치 이후 최초인 상태 실행했을시)
		// 일단 resources 폴더에 존재하는 기본 데이터를 이용한다
		TextAsset level = Resources.Load<TextAsset>(pathDefaultLevelDataLoad);
		if (level != null)
		{
			ManageLevelData.Levels = JsonConvert.DeserializeObject<Dictionary<int, Level>>(level.text);
			SaveToFile();
			return true;
		}
		return false;
	}
}