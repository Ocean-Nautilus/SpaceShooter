using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace SpaceShooter {
public class Game : MonoBehaviour {
 public static Game I; public CampaignData campaign; public Progress run=new Progress(); Progress checkpoint=new Progress();
 public int difficulty=1; public float musicVolume=.3f,effectsVolume=.45f;
 public LocationData Location=>campaign.locations[Mathf.Min(run.location,campaign.locations.Length-1)];
 public bool Playing=>screen=="play"; public Vector2 Player=>ship? (Vector2)ship.transform.position:Vector2.zero;
 public Sprite Shape {get;private set;}
 string screen="menu", back="menu", notice=""; int wave,noticeId; float hp,shield,shotTimer,invincible,transition;
 GameObject ship; AudioSource music,effects,engine; Coroutine waves;
 string SavePath=>Path.Combine(Application.persistentDataPath,"campaign.json");

 // =====================================================================
 //  API для GameUI (uGUI). Вся игровая логика осталась прежней.
 // =====================================================================
 public const int MaxRateLevel=7;
 public string Screen=>screen;
 public string Back=>back;
 public float Hp=>Mathf.Max(0,hp);
 public float Shield=>Mathf.Max(0,shield);
 public float MaxHp=>100+run.health*25;
 public float MaxShield=>60+run.shield*25;
 public int Wave=>wave;
 public int Credits=>(int)run.credits;
 public int Score=>(int)run.score;
 public int CheckpointScore=>(int)checkpoint.score;
 public string Notice=>notice;
 public int NoticeId=>noticeId;
 public bool HasSave=>File.Exists(SavePath);
 public bool HasNextLocation=>checkpoint.location<campaign.locations.Length;
 public float FireInterval=>Mathf.Max(.09f,.32f-run.rate*.035f);
 public LocationData HudLocation=>screen=="victory"?campaign.locations[Mathf.Max(0,run.location-1)]:Location;
 // type: 0 урон, 1 скорострельность, 2 щит, 3 здоровье
 public int UpgradeLevel(int type)=>type==0?run.damage:type==1?run.rate:type==2?run.shield:run.health;
 public int UpgradeCost(int type)=>(int)Rules.Cost(UpgradeLevel(type));
 public bool IsMaxed(int type)=>type==1&&run.rate>=MaxRateLevel;
 public bool IsLocked(int type)=>type==2&&shield<=0;
 public bool CanBuy(int type)=>!IsMaxed(type)&&!IsLocked(type)&&run.credits>=Rules.Cost(UpgradeLevel(type));
 // Значение характеристики на заданном уровне: урон, выстр./сек, макс. щит, макс. HP
 public float StatValue(int type,int level)=>type==0?1+level*.6f:type==1?1f/Mathf.Max(.09f,.32f-level*.035f):type==2?60+level*25:100+level*25;

 public void Click()=>Sound(campaign.click,600);
 public void NewGame(){checkpoint=new Progress();StartLocation();}
 public void ContinueGame()=>Load();
 public void OpenSettings(){back=screen;screen="settings";}
 public void CloseSettings(){PlayerPrefs.SetInt("difficulty",difficulty);PlayerPrefs.SetFloat("music",musicVolume);PlayerPrefs.SetFloat("effects",effectsVolume);PlayerPrefs.Save();screen=back;}
 public void Pause(){if(Playing)screen="pause";}
 public void Resume(){if(screen=="pause")screen="play";}
 public void NextWave(){if(screen=="upgrade")screen="play";}
 public void RestartLocation()=>StartLocation();
 public void NextLocation()=>StartLocation();
 public void ToMenu()=>Menu();
 public void QuitGame()=>Application.Quit();
 public void SetMusicVolume(float v){musicVolume=v;if(music)music.volume=v;}
 public void SetEffectsVolume(float v){effectsVolume=v;}
 public void SetDifficulty(int d){difficulty=d;}
 public void Buy(int type) {
  if(!CanBuy(type))return;
  run.credits-=Rules.Cost(UpgradeLevel(type));
  if(type==0)run.damage++; else if(type==1)run.rate++; else if(type==2){run.shield++;shield+=25;} else {run.health++;hp+=25;}
 }
 void Notify(string text){notice=text;noticeId++;}
 // =====================================================================

 void Awake() {
  I=this; Application.targetFrameRate=60;Camera.main.rect=new Rect(.2f,0,.6f,1);
  Texture2D tex=new Texture2D(8,8);tex.filterMode=FilterMode.Point;
  for(int y=0;y<8;y++)for(int x=0;x<8;x++)tex.SetPixel(x,y,Color.white);tex.Apply();
  Shape=Sprite.Create(tex,new Rect(0,0,8,8),new Vector2(.5f,.5f),8);
  music=gameObject.AddComponent<AudioSource>();music.loop=true;
  effects=gameObject.AddComponent<AudioSource>(); engine=gameObject.AddComponent<AudioSource>();engine.loop=true;
  difficulty=PlayerPrefs.GetInt("difficulty",1);musicVolume=PlayerPrefs.GetFloat("music",.3f);effectsVolume=PlayerPrefs.GetFloat("effects",.45f);
  Music(campaign.menuMusic,0);
 }
 readonly System.Collections.Generic.Dictionary<string,AudioClip> tones=new System.Collections.Generic.Dictionary<string,AudioClip>(); Material particleMaterial;
 AudioClip Tone(float frequency,float duration) {
  string key=frequency+":"+duration;if(tones.TryGetValue(key,out var cached))return cached;
  int n=(int)(22050*duration);float[] samples=new float[n];
  for(int i=0;i<n;i++)samples[i]=Mathf.Sign(Mathf.Sin(i*frequency*2*Mathf.PI/22050))*.12f*(1-i/(float)n);
  AudioClip clip=AudioClip.Create("Synth placeholder",n,1,22050,false);clip.SetData(samples,0);tones[key]=clip;return clip;
 }
 AudioClip[] tracks=new AudioClip[4];
 void Music(AudioClip clip,int index) {
  if(!clip && !tracks[index]) {
   int n=22050*8;float[] samples=new float[n];int[] notes={0,7,12,3,10,7,5,3};
   for(int i=0;i<n;i++) {float t=i/22050f;float f=110*Mathf.Pow(2,(notes[(int)(t*2)%8]+index*3)/12f);samples[i]=(Mathf.Sin(t*f*6.28318f)>.0f?.045f:-.045f)*(1-(t*2%1));}
   tracks[index]=AudioClip.Create("Replaceable chiptune "+index,n,1,22050,false);tracks[index].SetData(samples,0);
  }
  music.clip=clip?clip:tracks[index];music.volume=musicVolume;music.Play();
 }
 public void Sound(AudioClip clip,float freq=440,float duration=.07f) {effects.PlayOneShot(clip?clip:Tone(freq,duration),effectsVolume);}
 void Clear() {
  if(waves!=null)StopCoroutine(waves);
  foreach(var a in FindObjectsByType<Actor>())Destroy(a.gameObject);
  foreach(var b in FindObjectsByType<Shot>())Destroy(b.gameObject);
  if(ship)Destroy(ship);engine.Stop();
 }
 void StartLocation() {
  Clear(); run=checkpoint.Copy(); wave=0;hp=100+run.health*25;shield=60+run.shield*25;invincible=0;
  Camera.main.backgroundColor=Location.background;
  ship=Visual("Player ship",new Vector2(0,-4),new Vector2(.85f,.85f),campaign.playerSprite?Color.white:Color.cyan,campaign.playerSprite);

  engine.clip=campaign.engine?campaign.engine:Tone(55,2);engine.volume=effectsVolume*.15f;engine.Play();
  screen="play";Music(Location.music,run.location+1);waves=StartCoroutine(Waves());
 }
 IEnumerator WaitPlaying(float seconds) {while(seconds>0){if(Playing)seconds-=Time.deltaTime;yield return null;}}
 IEnumerator Waves() {
  for(wave=1;wave<=Location.waves;wave++) {
   int count=Rules.Count(wave,difficulty);
   for(int i=0;i<count;i++) {
    while(!Playing)yield return null;
    int unlocked=Mathf.Min(Location.enemies.Length,1+(wave-1)/2);
    Spawn(Location.enemies[UnityEngine.Random.Range(0,unlocked)],false);
    yield return WaitPlaying(i%3==2?1.7f:.35f);
   }
   if(wave==Rules.MiniWave(Location.waves))Spawn(Location.miniBoss,true);
   yield return WaitPlaying(.5f);
   while(FindObjectsByType<Actor>().Length>0)yield return null;
   ClearShots();screen="upgrade";
   while(screen=="upgrade"||screen=="pause"||screen=="settings")yield return null;
  }
  Spawn(Location.boss,true);
  while(FindObjectsByType<Actor>().Length>0)yield return null;
  if(screen=="defeat")yield break;
  ClearShots();run.location++;checkpoint=run.Copy();Save();screen="victory";transition=0;engine.Stop();Sound(campaign.victory,880,.6f);
 }
 void Spawn(EnemyData data,bool boss) {
  var g=new GameObject(data.title);g.transform.position=new Vector2(boss?0:UnityEngine.Random.Range(-3.3f,3.3f),boss?4.9f:UnityEngine.Random.Range(4f,5.4f));g.AddComponent<Actor>().Init(data,boss);
 }
 void ClearShots(){foreach(var b in FindObjectsByType<Shot>())Destroy(b.gameObject);}
 GameObject Visual(string name,Vector2 p,Vector2 scale,Color c,Sprite sprite=null) {
  var g=new GameObject(name);g.transform.position=p;g.transform.localScale=sprite?Vector3.one*Mathf.Min(scale.x/sprite.bounds.size.x,scale.y/sprite.bounds.size.y):(Vector3)scale;var sr=g.AddComponent<SpriteRenderer>();sr.sprite=sprite?sprite:Shape;sr.color=c;return g;
 }
 public void Bullet(Vector2 p,Vector2 direction,bool friendly,float damage) {
  var g=Visual(friendly?"Player shot":"Enemy shot",p,new Vector2(.18f,.48f),friendly?Color.cyan:new Color(1,.35f,.4f),campaign.bulletSprite);
  g.transform.rotation=Quaternion.FromToRotation(Vector3.up,direction);
  var b=g.AddComponent<Shot>();b.velocity=direction*(friendly?10:3.1f+run.location*.35f+difficulty*.25f);b.friendly=friendly;b.damage=damage;
 }
 public void Burst(Vector2 p,Color color,int count) {
  var g=new GameObject("Explosion particles");g.transform.position=p;
  var ps=g.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.startColor=color;main.startSize=.09f;main.startLifetime=.35f;main.startSpeed=2;main.loop=false;main.duration=.4f;main.maxParticles=40;
  var emission=ps.emission;emission.rateOverTime=0;
  var renderer=ps.GetComponent<ParticleSystemRenderer>();if(!particleMaterial)particleMaterial=new Material(Shader.Find("Sprites/Default"));renderer.sharedMaterial=particleMaterial;
  ps.Play();ps.Emit(count);Destroy(g,1);
 }
 public void Hurt(float damage) {
  if(!Playing||invincible>0)return;
  hp-=Rules.Absorb(ref shield,damage);invincible=.65f;Burst(Player,Color.cyan,12);Sound(null,140,.15f);
  if(hp<=0){screen="defeat";transition=0;engine.Stop();Sound(campaign.defeat,110,.6f);}
 }
 void Update() {
  transition+=Time.unscaledDeltaTime;
  if(Input.GetKeyDown(KeyCode.Escape)){if(Playing)screen="pause";else if(screen=="pause")screen="play";else if(screen=="settings")CloseSettings();}
  if(!Playing)return;
  invincible-=Time.deltaTime;
  var move=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0)).normalized;
  var p=Player+move*5*Time.deltaTime;p.x=Mathf.Clamp(p.x,-4.3f,4.3f);p.y=Mathf.Clamp(p.y,-5.1f,1.1f);ship.transform.position=p;
  ship.GetComponent<SpriteRenderer>().color=invincible>0 && Mathf.Sin(Time.time*40)>0?new Color(1,.35f,.35f):campaign.playerSprite?Color.white:Color.cyan;
  shotTimer-=Time.deltaTime;
  if(shotTimer<=0){shotTimer=FireInterval;Bullet(p+Vector2.up*.5f,Vector2.up,true,1+run.damage*.6f);Sound(campaign.shot,760,.035f);}
 }
 void Save() {
  try {string temp=SavePath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(checkpoint,true));if(File.Exists(SavePath))File.Replace(temp,SavePath,null);else File.Move(temp,SavePath);}
  catch(Exception e){Notify("Не удалось сохранить: "+e.Message);Debug.LogException(e);}
 }
 void Load() {
  try {
   var p=JsonUtility.FromJson<Progress>(File.ReadAllText(SavePath));
   if(p==null||p.location<0||p.location>campaign.locations.Length||p.score<0||p.credits<0||p.damage<0||p.rate<0||p.shield<0||p.health<0||p.damage>1000||p.rate>1000||p.shield>1000||p.health>1000)throw new Exception("Некорректные данные");
   checkpoint=p;if(p.location==campaign.locations.Length){screen="complete";return;}StartLocation();
  }catch(Exception e){Notify("Не удалось загрузить: "+e.Message);}
 }
 void Menu(){Clear();screen="menu";Music(campaign.menuMusic,0);}
}
}


