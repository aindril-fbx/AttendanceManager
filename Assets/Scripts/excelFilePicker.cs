using System;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using SFB;
#endif

public class ExcelFilePicker : MonoBehaviour
{
    public void PickExcelFile()
    {
#if UNITY_ANDROID
        PickFileAndroid();

#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        PickFileWindows();

#else
        Debug.LogError("File picker is not implemented for this platform.");
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR

    private void PickFileWindows()
    {
        var extensions = new[]
        {
            new ExtensionFilter("Excel Files", "xlsx")
        };

        StandaloneFileBrowser.OpenFilePanelAsync(
            "Select Excel Timetable",
            "",
            extensions,
            false,
            paths =>
            {
                if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
                {
                    OnFileSelected(paths[0]);
                }
            }
        );
    }

#endif

#if UNITY_ANDROID

    private void PickFileAndroid()
    {
        try
        {
            using (AndroidJavaClass intentClass =
                   new AndroidJavaClass("android.content.Intent"))
            {
                using (AndroidJavaObject intent =
                       new AndroidJavaObject("android.content.Intent"))
                {
                    intent.Call<AndroidJavaObject>(
                        "setAction",
                        intentClass.GetStatic<string>("ACTION_OPEN_DOCUMENT")
                    );

                    intent.Call<AndroidJavaObject>(
                        "addCategory",
                        intentClass.GetStatic<string>("CATEGORY_OPENABLE")
                    );

                    intent.Call<AndroidJavaObject>(
                        "setType",
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    );

                    using (AndroidJavaClass unityPlayer =
                           new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        AndroidJavaObject activity =
                            unityPlayer.GetStatic<AndroidJavaObject>(
                                "currentActivity"
                            );

                        activity.Call(
                            "startActivityForResult",
                            intent,
                            1001
                        );
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Android file picker error: " + e);
        }
    }

#endif

    private void OnFileSelected(string path)
    {
        Debug.Log("Excel file selected:");
        Debug.Log(path);

        // We will connect this to TimetableReader later.
        //
        // Example:
        // FindObjectOfType<TimetableReader>().LoadExcel(path);
    }
}
