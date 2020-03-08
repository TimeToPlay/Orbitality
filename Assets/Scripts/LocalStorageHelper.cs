using System;
using System.Collections;
using System.IO;
using ProjectUtils;
using UnityEngine;

public class LocalStorageHelper
{
    public IEnumerator ReaderStringFileAsync(string file, Action<string> onSuccess, Action<string> onFailure)
    {
        string result = "";
        string failure = "";
        bool isSuccess = false;
        yield return new WaitForThreadedTask(() =>
        {
            if (File.Exists(file))
            {
                FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    result = sr.ReadToEnd();
                    sr.Close();
                    fileStream.Close();
                    if (result != "")
                    {
                        result = result.Remove(result.Length - 1, 1);
                        isSuccess = true;
                    }
                }
            }
            else
            {
                failure = "File not found";
            }
        });
        if (isSuccess)
        {
            onSuccess(result);
        }
        else
        {
            onFailure(failure);
        }
    }

    public IEnumerator WriteString(string path, string s)
    {
        yield return new WaitForThreadedTask(() =>
        {
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(s);
            writer.Close();
        });
        Debug.Log("save file done");
    }
}