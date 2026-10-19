using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpaceShooter {

// Управляет всеми экранами интерфейса. Ссылки расставляет Editor-билдер сцены.
public class GameUI : MonoBehaviour {
 [Header("Корневые объекты")]
 public GameObject backdrop,hud,dim,menuPanel,settingsPanel,pausePanel,upgradePanel,defeatPanel,victoryPanel;
 public RectTransform stars,logo;

 [Header("HUD")]
 public GameBar hpBar,shieldBar,bossBar;
 public StatRow[] statRows=new StatRow[4];          // 0 урон, 1 скорострельность, 2 щит, 3 здоровье
 public Text locationTitle,waveText,scoreText,creditsText;
 public SegmentBar waveBar; public Punch scorePunch,creditsPunch;
 public GameObject bossRoot; public Button pauseButton;

 [Header("Главное меню")] public Button newGameBtn,continueBtn,settingsBtn,quitBtn;
 [Header("Настройки")] public Slider musicSlider,effectsSlider; public Text musicValue,effectsValue; public ChunkyButton[] difficultyBtns; public Button settingsBackBtn;
 [Header("Пауза")] public Button resumeBtn,pauseSettingsBtn,pauseMenuBtn;
 [Header("Ангар")] public UpgradeCard[] cards=new UpgradeCard[4]; public Text upgradeCredits; public Button nextWaveBtn;
 [Header("Поражение")] public Button restartBtn,defeatMenuBtn;
 [Header("Победа")] public Text victoryNote,victoryScore; public Button nextLocBtn,victoryMenuBtn;
 [Header("Уведомление")] public GameObject toast; public Text toastText;

 string current="";
 Actor boss; float bossTimer,toastTimer; int noticeSeen;
 Vector2 logoBase;
 float scoreShown,creditsShown; int scoreLast=-1,creditsLast=-1;
 struct Star{public RectTransform r;public float x,y,speed;}
 Star[] starField;
 static readonly Color[] diffFace={Pal.Green,Pal.Yellow,Pal.Red}, diffShadow={Pal.GreenDark,Pal.YellowDark,Pal.RedDark};

 static void Hook(Button b,UnityAction a){b.onClick.AddListener(()=>{Game.I.Click();a();});}

 void Start() {
  logoBase=logo.anchoredPosition; MakeStars();
  Hook(newGameBtn,()=>Game.I.NewGame());
  Hook(continueBtn,()=>Game.I.ContinueGame());
  Hook(settingsBtn,()=>Game.I.OpenSettings());
  Hook(quitBtn,()=>Game.I.QuitGame());
  Hook(pauseButton,()=>Game.I.Pause());
  Hook(resumeBtn,()=>Game.I.Resume());
  Hook(pauseSettingsBtn,()=>Game.I.OpenSettings());
  Hook(pauseMenuBtn,()=>Game.I.ToMenu());
  Hook(settingsBackBtn,()=>Game.I.CloseSettings());
  Hook(nextWaveBtn,()=>Game.I.NextWave());
  Hook(restartBtn,()=>Game.I.RestartLocation());
  Hook(defeatMenuBtn,()=>Game.I.ToMenu());
  Hook(nextLocBtn,()=>Game.I.NextLocation());
  Hook(victoryMenuBtn,()=>Game.I.ToMenu());
  for(int i=0;i<cards.Length;i++) {
   int type=i;
   cards[i].buy.button.onClick.AddListener(()=>{Game.I.Buy(type);Game.I.Sound(null,900,.08f);});
  }
  for(int i=0;i<difficultyBtns.Length;i++) {
   int d=i;
   Hook(difficultyBtns[i].button,()=>{Game.I.SetDifficulty(d);RefreshDifficulty();});
  }
  musicSlider.onValueChanged.AddListener(v=>{Game.I.SetMusicVolume(v);musicValue.text=Mathf.RoundToInt(v*100)+"%";});
  effectsSlider.onValueChanged.AddListener(v=>{Game.I.SetEffectsVolume(v);effectsValue.text=Mathf.RoundToInt(v*100)+"%";});
 }

 void Update() {
  var g=Game.I; if(g==null)return;
  if(g.Screen!=current)Show(g.Screen);
  if(backdrop.activeSelf){MoveStars();logo.anchoredPosition=logoBase+new Vector2(0,Mathf.Sin(Time.unscaledTime*2f)*8f);}
  if(current=="menu")continueBtn.interactable=g.HasSave;
  if(hud.activeSelf)UpdateHud(g);
  if(current=="upgrade")UpdateUpgrade(g);
  if(g.NoticeId!=noticeSeen) {
   noticeSeen=g.NoticeId;
   if(!string.IsNullOrEmpty(g.Notice)){toastText.text=g.Notice;toast.SetActive(true);toastTimer=5f;}
  }
  if(toastTimer>0){toastTimer-=Time.unscaledDeltaTime;if(toastTimer<=0)toast.SetActive(false);}
 }

 // ---------------------------------------------------------------- экраны
 void Show(string s) {
  current=s; var g=Game.I;
  bool settingsFromRun=s=="settings"&&g.Back!="menu";
  bool menuBg=s=="menu"||s=="complete"||(s=="settings"&&!settingsFromRun);
  backdrop.SetActive(menuBg);
  hud.SetActive(!menuBg);
  dim.SetActive(s=="pause"||s=="upgrade"||s=="defeat"||s=="victory"||settingsFromRun);
  menuPanel.SetActive(s=="menu");
  settingsPanel.SetActive(s=="settings");
  pausePanel.SetActive(s=="pause");
  upgradePanel.SetActive(s=="upgrade");
  defeatPanel.SetActive(s=="defeat");
  victoryPanel.SetActive(s=="victory"||s=="complete");
  if(s=="settings")SyncSettings(g);
  if(s=="victory"||s=="complete")SyncVictory(g);
 }

 void SyncSettings(Game g) {
  musicSlider.SetValueWithoutNotify(g.musicVolume);musicValue.text=Mathf.RoundToInt(g.musicVolume*100)+"%";
  effectsSlider.SetValueWithoutNotify(g.effectsVolume);effectsValue.text=Mathf.RoundToInt(g.effectsVolume*100)+"%";
  RefreshDifficulty();
 }
 void RefreshDifficulty() {
  for(int i=0;i<difficultyBtns.Length;i++) {
   bool sel=i==Game.I.difficulty;
   difficultyBtns[i].SetColors(sel?diffFace[i]:Pal.PanelLight,sel?diffShadow[i]:Pal.Panel);
   difficultyBtns[i].label.color=sel&&i==1?Pal.Ink:Color.white;
  }
 }
 void SyncVictory(Game g) {
  bool all=!g.HasNextLocation;
  victoryNote.text=all?"Все локации пройдены!":"Локация пройдена";
  victoryScore.text=g.CheckpointScore.ToString();
  nextLocBtn.gameObject.SetActive(!all);
 }

 // ---------------------------------------------------------------- HUD
 static string Fmt(int type,float v)=>type<2?v.ToString("0.0"):Mathf.RoundToInt(v).ToString();

 void Count(ref float shown,ref int last,int value,Text t,Punch p) {
  if(last<0)shown=value; else if(value>last)p.Hit(.2f);
  last=value;
  shown=Mathf.Lerp(shown,value,1-Mathf.Exp(-9*Time.unscaledDeltaTime));
  if(Mathf.Abs(shown-value)<.6f)shown=value;
  t.text=Mathf.RoundToInt(shown).ToString();
 }

 void UpdateHud(Game g) {
  var loc=g.HudLocation;
  float hpT=g.MaxHp>0?g.Hp/g.MaxHp:0;
  hpBar.Set(g.Hp,g.MaxHp,Pal.HpColor(hpT),Mathf.CeilToInt(g.Hp)+" / "+Mathf.CeilToInt(g.MaxHp),hpT<.25f&&g.Hp>0);
  bool broken=g.Shield<=0;
  shieldBar.Set(g.Shield,g.MaxShield,broken?Pal.Muted:Pal.Cyan,broken?"РАЗРУШЕН":Mathf.CeilToInt(g.Shield)+" / "+Mathf.CeilToInt(g.MaxShield));
  for(int i=0;i<statRows.Length;i++) {
   int lvl=g.UpgradeLevel(i);
   statRows[i].Set(Fmt(i,g.StatValue(i,lvl)),lvl+1,i==1?Game.MaxRateLevel+1:10,g.IsMaxed(i));
  }
  locationTitle.text=loc.title.ToUpper();
  bool cleared=g.Screen=="victory", bossWave=g.Wave>loc.waves;
  waveText.text=cleared?"ГОТОВО":bossWave?"БОСС":Mathf.Clamp(g.Wave,0,loc.waves)+" / "+loc.waves;
  waveText.color=cleared?Pal.Green:bossWave?Pal.Red:Pal.Yellow;
  int cur=g.Wave>=1&&!bossWave&&!cleared?g.Wave-1:-1;
  waveBar.Set(loc.waves,cleared||bossWave?loc.waves:Mathf.Max(0,g.Wave-1),Pal.Green,Pal.Ink,cur,Pal.Yellow);
  Count(ref scoreShown,ref scoreLast,g.Score,scoreText,scorePunch);
  Count(ref creditsShown,ref creditsLast,g.Credits,creditsText,creditsPunch);
  pauseButton.interactable=g.Playing;
  // босс-бар (поиск раз в 0.2 с, а не каждый кадр)
  bossTimer-=Time.unscaledDeltaTime;
  if(bossTimer<=0){bossTimer=.2f;boss=null;foreach(var a in FindObjectsByType<Actor>())if(a.boss){boss=a;break;}}
  bool showBoss=boss;
  if(bossRoot.activeSelf!=showBoss)bossRoot.SetActive(showBoss);
  if(showBoss){bossBar.label.text=boss.data.title.ToUpper();bossBar.Set(boss.hp,boss.maxHp,Pal.Red,Mathf.CeilToInt(boss.hp)+" / "+Mathf.CeilToInt(boss.maxHp));}
 }

 // ---------------------------------------------------------------- ангар
 void UpdateUpgrade(Game g) {
  upgradeCredits.text=g.Credits.ToString();
  for(int i=0;i<cards.Length;i++) {
   var c=cards[i]; int lvl=g.UpgradeLevel(i);
   bool max=g.IsMaxed(i),locked=g.IsLocked(i),can=g.CanBuy(i);
   float now=g.StatValue(i,lvl);
   c.preview.text=max?Fmt(i,now):Fmt(i,now)+"  →  "+Fmt(i,g.StatValue(i,lvl+1));
   c.pips.Set(i==1?Game.MaxRateLevel+1:10,Mathf.Min(lvl+1,i==1?Game.MaxRateLevel+1:10),c.accent,Pal.Ink);
   c.buy.label.text=max?"МАКСИМУМ":locked?"ЩИТ РАЗРУШЕН":can?"КУПИТЬ  •  "+g.UpgradeCost(i):"НУЖНО  "+g.UpgradeCost(i);
   c.buy.button.interactable=can;
  }
 }

 // ---------------------------------------------------------------- звёзды на фоне меню
 void MakeStars() {
  starField=new Star[80];
  for(int i=0;i<starField.Length;i++) {
   var go=new GameObject("Star",typeof(RectTransform),typeof(Image));
   var r=(RectTransform)go.transform; r.SetParent(stars,false);
   float size=Random.Range(3f,8f); r.sizeDelta=new Vector2(size,size);
   var im=go.GetComponent<Image>(); im.raycastTarget=false;
   float a=Random.Range(.25f,.9f);
   im.color=Random.value<.15f?new Color(1,.85f,.3f,a):new Color(.75f,.88f,1,a);
   starField[i]=new Star{r=r,x=Random.value,y=Random.value,speed=Mathf.Lerp(.02f,.12f,size/8f)};
  }
 }
 void MoveStars() {
  float dt=Time.unscaledDeltaTime;
  for(int i=0;i<starField.Length;i++) {
   var s=starField[i]; s.y-=s.speed*dt;
   if(s.y<-.02f){s.y=1.02f;s.x=Random.value;}
   s.r.anchorMin=s.r.anchorMax=new Vector2(s.x,s.y); s.r.anchoredPosition=Vector2.zero;
   starField[i]=s;
  }
 }
}
}
