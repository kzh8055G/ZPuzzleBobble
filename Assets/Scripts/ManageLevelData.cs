using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// https://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934

public static class ManageLevelData
{
	private static string levelDataFilePath => Path.Combine(Application.dataPath, @"levels.json");
	public static Dictionary<int, Level> Levels = new Dictionary<int, Level>();

	public static bool GetLevel(int _levelNumber, out Level _level)
	{
		return Levels.TryGetValue(_levelNumber, out _level);
	}

	public static void SaveToFile()
	{
		var jsonInfo = JObject.FromObject(Levels);
		File.WriteAllText(levelDataFilePath, jsonInfo.ToString());

        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd");
        //bf.Serialize(file, ManageLevelData.Levels);
        //file.Close();
    }

	public static bool LoadFromFile()
	{
		var pathLevelData = Path.Combine(Application.dataPath, "levels.json");
		if( File.Exists(pathLevelData))
        {
			var jsonString = File.ReadAllText(pathLevelData);
			if (!string.IsNullOrEmpty(jsonString))
			{
				ManageLevelData.Levels = JsonConvert.DeserializeObject<Dictionary<int, Level>>(jsonString);
				return true;
			}
		}
		return false;

		//catch(Exception _e)
  //      {
  //          if (File.Exists(Application.persistentDataPath + "/savedGames.gd"))
  //          {
  //              BinaryFormatter bf = new BinaryFormatter();
  //              FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
  //              file.Seek(0, SeekOrigin.Begin);
  //              ManageLevelData.Levels = (Dictionary<int, Level>)bf.Deserialize(file);
  //              file.Close();
  //              return true;
  //          }
  //      }
  //		return false;
	}
}