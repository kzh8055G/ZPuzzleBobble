using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ZPuzzleBubble
{
    public class Singleton<T> where T : class, new()
    {
        private static T instace = null;
        public static T Instance => instace ?? (instace = new T());
    }

    public class LevelDataManager : Singleton<LevelDataManager>
    {
        private string pathlevelDataCache => Path.Combine(Application.persistentDataPath, @"levels.json");
        private readonly string pathDefaultLevelData = @"Assets/Levels/default_levels.json";
        private readonly string addressDefaultLevelData = @"Assets/Levels/default_levels.json";
        
        private Dictionary<int, Level> levels = new Dictionary<int, Level>();
        
        // Test
        public void DownloadLevel()
        {
            // ClearDependencyCacheAsync 호출시 다음의 Exception 이 발생한다
            // -> Exception: Attempting to use an invalid operation handle
            //Addressables.ClearDependencyCacheAsync(addressDefaultLevelData); 


            Addressables.GetDownloadSizeAsync(addressDefaultLevelData).Completed += (_op) =>
            {
                Debug.Log(_op.Result);
                if (_op.Result > 0)
                {
                    Addressables.DownloadDependenciesAsync(addressDefaultLevelData).Completed += ( _op) => 
                    {
                        if (_op.Status == AsyncOperationStatus.Succeeded)
                        {
                            Debug.Log("Download Completed");
                        }
                    };
                }
            };
        }
        
        
        public void LoadLevelData(Action<bool> _onCompleted = null)
        {
            List<string> cachePaths = new List<string>();
            Caching.GetAllCachePaths(cachePaths);
            foreach (var path in cachePaths)
            {
                Debug.Log(path);
            }
            // if (File.Exists(pathlevelDataCache))
            // {
            //     var jsonString = File.ReadAllText(pathlevelDataCache);
            //     if (!string.IsNullOrEmpty(jsonString))
            //     {
            //         levels = JsonConvert.DeserializeObject<Dictionary<int, Level>>(jsonString);
            //         _onCompleted?.Invoke(true);
            //         return;
            //     }
            // }
            // 무조건 다운로드는 받아둬야한다
            Addressables.LoadAssetAsync<TextAsset>(addressDefaultLevelData).Completed += (_op) =>
            {
                // 캐싱 데이터가 있다면 이걸 기준으로 level data 를 넘긴다
                if (File.Exists(pathlevelDataCache))
                {
                    var jsonString = File.ReadAllText(pathlevelDataCache);
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        levels = JsonConvert.DeserializeObject<Dictionary<int, Level>>(jsonString);
                        _onCompleted?.Invoke(true);
                        return;
                    }
                }
                var jsonTextLevel = _op.Result.text;
                if (!string.IsNullOrEmpty(jsonTextLevel))
                {
                    levels = JsonConvert.DeserializeObject<Dictionary<int, Level>>(jsonTextLevel);
                    // 캐싱해 둔다
                    if (!File.Exists(pathlevelDataCache))
                    {
                        File.WriteAllText(pathlevelDataCache, jsonTextLevel);
                    }
                    _onCompleted?.Invoke(true);
                    return;
                }
            };
            _onCompleted?.Invoke(false);
        }
        
        public void SaveLevel(int _levelNumber, Level _level)
        {
            if (levels.ContainsKey(_levelNumber))
            {
                levels.Remove(_levelNumber);
            }
            levels.Add(_levelNumber, _level);
            _SaveLevelDataToFile();
            // if (ManageLevelData.Levels.ContainsKey(currentLevelNumber))
            // {
            //     ManageLevelData.Levels.Remove(currentLevelNumber);
            // }
            // ManageLevelData.Levels.Add(currentLevelNumber, level);
            // ManageLevelData.SaveToFile();
        }
        
        private void _SaveLevelDataToFile()
        {
            if (!File.Exists(pathlevelDataCache))
            {
                Debug.Log("No Level Data so Create");
            }
            var jsonInfo = JObject.FromObject(levels);
            File.WriteAllText(pathlevelDataCache, jsonInfo.ToString());
#if UNITY_EDITOR
            File.WriteAllText(pathDefaultLevelData, jsonInfo.ToString());
#endif
        }
        
        public bool TryGetLevel(int _levelNumber, out Level _level)
        {
            return levels.TryGetValue(_levelNumber, out _level);
        }

    }
}
