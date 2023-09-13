using UnityEditor;
using UnityEngine;

class ShowOnStart
{
    [InitializeOnLoadMethod]
    static void OnProjectLoadedInEditor()
    {
        if (EditorApplication.isPlaying)
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorApplication.update += ShowWelcomeWindow;
    }

    public static void ShowWelcomeWindow()
    {
        if (EditorApplication.isCompiling)
            return;

        if (EditorApplication.isUpdating)
            return;

        EditorApplication.update -= ShowWelcomeWindow;

        if (EditorPrefs.GetBool("DoNotShowSetup"))
            return;

        SetupWindow.ShowWindow();
    }
}