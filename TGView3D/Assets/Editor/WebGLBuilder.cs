using UnityEditor;
class WebGLBuilder
{
    static void Build()
    {

        // Place all your scenes here
        string[] scenes = { "Assets/Scenes/SampleScene.unity" };

        string pathToDeploy = "/Web/";

        BuildPipeline.BuildPlayer(scenes, pathToDeploy, BuildTarget.WebGL, BuildOptions.None);
    }
}


