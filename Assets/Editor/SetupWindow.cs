using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

//[InitializeOnLoad]
public class SetupWindow : EditorWindow
{
    static readonly string tutorialUrl;
    static readonly string instructionsTxt;

    static SetupWindow()
    {
        // Load tutorial url
        try
        {
            string txt = File.ReadAllText("Assets/Editor/Tutorial.url");

            tutorialUrl = txt.Substring(txt.IndexOf("http"));
        }
        catch (FileNotFoundException)
        {
            tutorialUrl = "http://docs2x.smartfoxserver.com/ExamplesUnity/introduction";
        }

        // Load instructions text
        try
        {
            instructionsTxt = File.ReadAllText("Assets/Editor/Instructions.txt");
        }
        catch (FileNotFoundException)
        {
            instructionsTxt = "Instructions.txt file is missing.";
        }

        //// Show this editor window on load
        //if (!EditorPrefs.GetBool("Setup_Dismissed"))
        //    ShowWindow();

        //// Show this editor window when scene is changed
        //EditorSceneManager.sceneOpened += OnEditorSceneManagerSceneOpened;
    }

    //private static void OnEditorSceneManagerSceneOpened(Scene scene, OpenSceneMode mode)
    //{
    //    if (!EditorPrefs.GetBool("Setup_Dismissed"))
    //        ShowWindow();
    //}

    [MenuItem("SmartFoxServer/Demo Project Setup")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SetupWindow), true, "SmartFoxServer Demo Project Setup");
    }

    [MenuItem("SmartFoxServer/Download SmartFoxServer [↗]")]
    public static void DownloadSfs()
    {
        Application.OpenURL("https://www.smartfoxserver.com/download/sfs2x");
    }

    [MenuItem("SmartFoxServer/Tutorial [↗]")]
    public static void GoToTutorial()
    {
        Application.OpenURL(tutorialUrl);
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML (also references USS style sheet internally)
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SetupWindow.uxml");
        visualTree.CloneTree(root);
        
        if (!EditorGUIUtility.isProSkin)
            root.AddToClassList("light");

        // Add click listener to download button
        Button downloadBt = (Button)root.Query("downloadBt").First();
        downloadBt.RegisterCallback<ClickEvent>(HandleDownloadClickCallback);

        // Add click listener to tutorial button
        Button tutorialBt = (Button)root.Query("tutorialBt").First();
        tutorialBt.RegisterCallback<ClickEvent>(HandleTutorialClickCallback);

        // Add change listener to checkbox
        Toggle showTg = (Toggle)root.Query("showTg").First();
        showTg.value = !EditorPrefs.GetBool("DoNotShowSetup");
        showTg.RegisterValueChangedCallback(HandleShowToggleCallback);

        // Set instructions 
        TextElement instructionsLb = (TextElement)root.Query("instructionsLb").First();
        instructionsLb.text = instructionsTxt;

    }

    private void HandleDownloadClickCallback(ClickEvent evt)
    {
        DownloadSfs();
    }

    private void HandleTutorialClickCallback(ClickEvent evt)
    {
        GoToTutorial();
    }

    private void HandleShowToggleCallback(ChangeEvent<bool> evt)
    {
        EditorPrefs.SetBool("DoNotShowSetup", !evt.newValue);
    }
}