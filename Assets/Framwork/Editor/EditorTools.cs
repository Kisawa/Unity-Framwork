using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Framwork
{
    public class EditorTools
    {
        [MenuItem("Tools/Clear local data")]
        static void clearLocalData()
        {
            if (EditorUtility.DisplayDialog("Clear local data", "Are you sure you wish to clear the local data?\n This action cannot be reversed.", "Clear", "Cancel"))
            {
                DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
                foreach (DirectoryInfo dir in di.GetDirectories())
                    dir.Delete(true);
                LocalSaveUtility.RefreshHasInjected();
            }
        }
    }
}