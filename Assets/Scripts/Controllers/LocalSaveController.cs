using System;
using System.IO;
using Models;
using UniRx;
using UnityEngine;

namespace Controllers
{
    /// <summary>
    /// Serialize/deserialize json into game model
    /// </summary>
    public class LocalSaveController
    {
        private string gameDataFileName = "saves.json";
        private string saveFileName = "saves";
        private LocalStorageHelper _localStorageHelper;

        public LocalSaveController(LocalStorageHelper localStorageHelper)
        {
            _localStorageHelper = localStorageHelper;
        }

        public void LoadSaveFile(Action<GameModel> onSuccess, Action<string> onFailure)
        {
            MainThreadDispatcher.StartUpdateMicroCoroutine(
                _localStorageHelper.ReaderStringFileAsync(GetCurrentSavePath(),
                    delegate(string s)
                    {
                        if (s == "")
                        {
                            onFailure("empty save file");
                            return;
                        }

                        var saveStoryModel = JsonUtility.FromJson<GameModel>(s);
                        onSuccess(saveStoryModel);
                    }, onFailure));
        }

        public void SaveProgress(string jsonToSave)
        {
            var path = GetCurrentSavePath();
            MainThreadDispatcher.StartUpdateMicroCoroutine(_localStorageHelper.WriteString(path, jsonToSave));
        }

        private string GetCurrentSavePath()
        {
            return Path.Combine(Application.persistentDataPath, saveFileName);
        }

        public bool IsSaveFileExists()
        {
            return File.Exists(GetCurrentSavePath());
        }
    }
}