using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class SpikeWebGL {
  public static void BuildMarksman() {
    string[] scenes=AssetDatabase.FindAssets("t:Scene")
      .Select(AssetDatabase.GUIDToAssetPath)
      .Where(p=>Path.GetFileNameWithoutExtension(p)
        .IndexOf("Marksman",StringComparison.OrdinalIgnoreCase)>=0).ToArray();
    if(scenes.Length!=1) throw new Exception("MARKSMAN_SCENE_COUNT="+scenes.Length);
    PlayerSettings.WebGL.memorySize=256;
    PlayerSettings.WebGL.dataCaching=true;
    var opt=new BuildPlayerOptions {
      scenes=new []{scenes[0]},locationPathName="/spike/webgl",
      target=BuildTarget.WebGL,options=BuildOptions.None
    };
    BuildReport report=BuildPipeline.BuildPlayer(opt);
    if(report.summary.result!=BuildResult.Succeeded)
      throw new Exception("MARKSMAN_WEBGL_RESULT="+report.summary.result);
  }
}
