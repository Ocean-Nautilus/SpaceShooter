using Motion = SpaceShooter.Motion; using Progress = SpaceShooter.Progress;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SpaceShooter;
public static class Setup {
 [MenuItem("Space Shooter/Create default game")]
 public static void Create() {
  Directory.CreateDirectory("Assets/Data");Directory.CreateDirectory("Assets/Scenes");
  var campaign=AssetDatabase.LoadAssetAtPath<CampaignData>("Assets/Data/Campaign.asset");
  if(!campaign){
   campaign=ScriptableObject.CreateInstance<CampaignData>();campaign.locations=new LocationData[3];
   string[] names={"Солнечная система","Млечный путь","Стрелец A"};
   for(int l=0;l<3;l++){
    var loc=ScriptableObject.CreateInstance<LocationData>();loc.title=names[l];loc.waves=new[]{5,7,10}[l];loc.strength=1+l*.65f;loc.bossPattern=l;
    loc.background=new[]{new Color(.015f,.025f,.09f),new Color(.05f,.015f,.1f),new Color(.06f,.005f,.025f)}[l];
    loc.accent=new[]{new Color(.3f,.65f,1),new Color(.7f,.4f,1),new Color(1,.5f,.2f)}[l];loc.enemies=new EnemyData[3];
    string[] labels={"Разведчик","Истребитель","Тяжёлый корабль","Мини-босс","Босс"};
    for(int e=0;e<5;e++){
     var d=ScriptableObject.CreateInstance<EnemyData>();d.title=labels[e];d.motion=new[]{Motion.Stationary,Motion.Dive,Motion.Strafe,Motion.Sweep,Motion.Strafe}[e];
     d.health=new[]{2,4,8,35,100}[e];d.speed=e==1?1.15f:.7f;d.score=new[]{100,160,250,800,2500}[e];
     d.damage=8+e*3;d.fireInterval=Mathf.Max(.65f,2.7f-e*.4f);d.pellets=1+e;d.color=Color.HSVToRGB((l*.23f+e*.11f)%1,.65f,1);
     AssetDatabase.CreateAsset(d,$"Assets/Data/L{l}_{e}.asset");
     if(e<3)loc.enemies[e]=d;else if(e==3)loc.miniBoss=d;else loc.boss=d;
    }
    AssetDatabase.CreateAsset(loc,$"Assets/Data/Location{l}.asset");campaign.locations[l]=loc;
   }
   AssetDatabase.CreateAsset(campaign,"Assets/Data/Campaign.asset");
  }
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var camera=new GameObject("Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=6;camera.transform.position=new Vector3(0,0,-10);camera.backgroundColor=new Color(.01f,.02f,.07f);camera.clearFlags=CameraClearFlags.SolidColor;camera.gameObject.AddComponent<AudioListener>();
  var game=new GameObject("Space Shooter").AddComponent<Game>();game.campaign=campaign;game.gameObject.AddComponent<Starfield>();
  PlayerSettings.companyName="Independent";PlayerSettings.productName="SpaceShooter";PlayerSettings.defaultScreenWidth=960;PlayerSettings.defaultScreenHeight=720;PlayerSettings.runInBackground=true;
  EditorSceneManager.SaveScene(scene,"Assets/Scenes/Main.unity");
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Main.unity",true)};
  AssetDatabase.SaveAssets();Checks();Debug.Log("SPACE_SHOOTER_SETUP_OK");
 }
 [MenuItem("Space Shooter/Run rule checks")]
 public static void Checks(){
  if(Rules.MiniWave(5)!=4||Rules.MiniWave(7)!=5||Rules.MiniWave(10)!=7)throw new Exception("Mini-boss timing");
  if(Rules.Count(3,2)<=Rules.Count(2,2)||Rules.Count(2,2)<=Rules.Count(2,0))throw new Exception("Difficulty progression");
  float shield=5;float damage=Rules.Absorb(ref shield,10);
  if(shield!=0||damage!=5||Rules.Absorb(ref shield,10)!=10)throw new Exception("Shield durability");
  var p=new Progress{credits=200,damage=2};var copy=p.Copy();copy.damage++;if(p.damage!=2)throw new Exception("Checkpoint isolation");
  Debug.Log("SPACE_SHOOTER_CHECKS_OK");
 }
 [MenuItem("Space Shooter/Build Windows")]
 public static void Build(){
  Checks();var r=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Build/SpaceShooter.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
  if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
 }
}

