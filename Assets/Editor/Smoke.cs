using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using SpaceShooter;
[InitializeOnLoad]
public static class Smoke {
 static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static string savePath;static byte[] originalSave;static double deadline;static bool started,menuCaptured,playCaptured;
 static int captureFrame;static int victories,lastWave=-1;static float visualStart;
 static Smoke(){EditorApplication.update+=Tick;}
 public static void Run(){
  Setup.Checks();UnityEditor.SceneManagement.EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
  ApplyArt.Check();SessionState.SetBool("ShooterSmoke",true);
  EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Show();EditorApplication.isPlaying=true;
 }
 static void Require(bool condition,string label){if(!condition)throw new Exception(label);}
 static void UiChecks(Game game,GameUI ui){
  Require(ui!=null,"UI missing");
  float mv=game.musicVolume,ev=game.effectsVolume;int difficulty=game.difficulty;
  ui.settingsBtn.onClick.Invoke();Require(game.Screen=="settings","Settings button");
  ui.musicSlider.value=.17f;ui.effectsSlider.value=.23f;
  Require(Mathf.Abs(game.musicVolume-.17f)<.001f&&Mathf.Abs(game.effectsVolume-.23f)<.001f,"Volume sliders");
  ui.difficultyBtns[2].button.onClick.Invoke();Require(game.difficulty==2,"Difficulty buttons");
  ui.musicSlider.value=mv;ui.effectsSlider.value=ev;ui.difficultyBtns[difficulty].button.onClick.Invoke();
  ui.settingsBackBtn.onClick.Invoke();Require(game.Screen=="menu","Settings back");
  ui.newGameBtn.onClick.Invoke();Require(game.Playing,"New game");
  ui.pauseButton.onClick.Invoke();Require(game.Screen=="pause","Pause button");
  ui.resumeBtn.onClick.Invoke();Require(game.Playing,"Resume button");
  game.Hurt(100000);Require(game.Screen=="defeat","Defeat state");
  ui.restartBtn.onClick.Invoke();Require(game.Playing&&game.Hp==game.MaxHp,"Restart button");
  game.run.credits=1000;int cost=game.UpgradeCost(0);
  ui.cards[0].buy.button.onClick.Invoke();Require(game.run.damage==1&&game.Credits==1000-cost,"Purchase button");
  game.ToMenu();ui.newGameBtn.onClick.Invoke();Require(game.run.damage==0&&game.Credits==0,"Checkpoint rollback");
  game.Bullet(Vector2.zero,new Vector2(1,-1).normalized,false,1);
  var shots=UnityEngine.Object.FindObjectsByType<Shot>();bool diagonal=false;
  foreach(var shot in shots)if(Vector2.Dot(shot.transform.up,new Vector2(1,-1).normalized)>.999f){diagonal=true;Require(shot.GetComponent<SpriteRenderer>().sprite==game.campaign.bulletSprite,"Projectile art");}
  Require(diagonal,"Diagonal projectile");
  Debug.Log("UI_BUTTONS_PURCHASE_ROLLBACK_PROJECTILES_OK");
 }
 static void Tick(){
  if(!SessionState.GetBool("ShooterSmoke",false))return;
  if(deadline==0)deadline=EditorApplication.timeSinceStartup+240;
  if(EditorApplication.timeSinceStartup>deadline){Finish(false,"Timed out");return;}
  EditorApplication.isPaused=false;EditorApplication.QueuePlayerLoopUpdate();
  if(!EditorApplication.isPlaying||!Game.I||Time.frameCount<8)return;
  try {
   var game=Game.I;var type=typeof(Game);var ui=UnityEngine.Object.FindAnyObjectByType<GameUI>();
   if(!menuCaptured){System.IO.Directory.CreateDirectory("Previews");ScreenCapture.CaptureScreenshot("Previews/menu.png");menuCaptured=true;return;}
   if(Time.frameCount<12)return;
   if(!started){
    started=true;savePath=(string)type.GetProperty("SavePath",Flags).GetValue(game);
    if(System.IO.File.Exists(savePath))originalSave=System.IO.File.ReadAllBytes(savePath);
    UiChecks(game,ui);game.effectsVolume=0;game.difficulty=2;visualStart=Time.unscaledTime;
   }
   type.GetField("invincible",Flags).SetValue(game,100f);
   if(!playCaptured){
    if(Time.unscaledTime-visualStart<4)return;
    ScreenCapture.CaptureScreenshot("Previews/gameplay.png");playCaptured=true;captureFrame=Time.frameCount;return;
   }
   if(Time.frameCount<=captureFrame+2)return;Time.timeScale=40;
   int wave=game.Wave;if(wave!=lastWave){lastWave=wave;Debug.Log("SMOKE_LOCATION "+game.run.location+" WAVE "+wave);}
   string screen=game.Screen;
   foreach(var enemy in UnityEngine.Object.FindObjectsByType<Actor>())enemy.Hit(100000);
   if(screen=="upgrade")ui.nextWaveBtn.onClick.Invoke();
   if(screen=="defeat"){Finish(false,"Unexpected defeat");return;}
   if(screen=="victory"){
    victories++;
    if(game.run.location<game.campaign.locations.Length)ui.nextLocBtn.onClick.Invoke();
    else {
     Require(victories==3&&game.run.location==3&&game.Score>0,"Campaign result");
     game.ToMenu();ui.continueBtn.onClick.Invoke();Require(game.Screen=="complete","Save/load button");
     Finish(true,"UI buttons, purchases, rollback, sprites, diagonal shots, three locations and save/load passed");
    }
   }
  }catch(Exception e){Finish(false,e.ToString());}
 }
 static void Finish(bool ok,string message){
  SessionState.SetBool("ShooterSmoke",false);Time.timeScale=1;
  if(savePath!=null){if(originalSave!=null)System.IO.File.WriteAllBytes(savePath,originalSave);else if(System.IO.File.Exists(savePath))System.IO.File.Delete(savePath);}
  Debug.Log((ok?"SPACE_SHOOTER_SMOKE_OK: ":"SPACE_SHOOTER_SMOKE_FAIL: ")+message);EditorApplication.Exit(ok?0:1);
 }
}

