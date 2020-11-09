using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// https://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934

public static class ManageLevelData
{
	public static Dictionary<int, Level> Levels = new Dictionary<int, Level>();

	public static bool GetLevel(int _levelNumber, out Level _level)
	{
		return Levels.TryGetValue(_levelNumber, out _level);
	}

	public static void SaveToFile()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd");
		bf.Serialize(file, ManageLevelData.Levels);
		file.Close();
	}

	public static bool LoadFromFile()
	{
		if (File.Exists(Application.persistentDataPath + "/savedGames.gd"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
			file.Seek(0, SeekOrigin.Begin);
			ManageLevelData.Levels = (Dictionary<int, Level>)bf.Deserialize(file);
			file.Close();
			return true;
		}
		return false;
	}
}
