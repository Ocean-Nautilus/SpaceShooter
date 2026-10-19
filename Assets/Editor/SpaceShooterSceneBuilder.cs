using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceShooter.EditorTools {

// Меню: Space Shooter → Build UI Scene
// Создаёт Assets/SpaceShooter/Scenes/SpaceShooter.unity: камеры, Game, Canvas со всем интерфейсом, EventSystem.
public static class SpaceShooterSceneBuilder {
 const string Dir="Assets/SpaceShooter";
 static Font font;
 static Sprite R,RP,RS,IcHeart,IcShield,IcBolt,IcStar,IcCoin;

 // ======================================================================= точка входа
 [MenuItem("Space Shooter/Build UI Scene")]
 public static void Build() {
  if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  Directory.CreateDirectory(Dir+"/UI");Directory.CreateDirectory(Dir+"/Scenes");AssetDatabase.Refresh();
  font=AssetDatabase.LoadAssetAtPath<Font>(Dir+"/UI/Font.ttf");           // свой шрифт: положите Font.ttf в Assets/SpaceShooter/UI
  if(!font)font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
  if(!font)font=Resources.GetBuiltinResource<Font>("Arial.ttf");
  MakeSprites();

  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  MakeCameras(); MakeGame();

  var cv=new GameObject("UI Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
  var canvas=cv.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=10;
  var sc=cv.GetComponent<CanvasScaler>(); sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution=new Vector2(1920,1080);
  sc.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; sc.matchWidthOrHeight=0f;   // по ширине: боковые панели всегда = 20% ширины
  var ui=cv.AddComponent<GameUI>(); var t=cv.transform;

  BuildBackdrop(t,ui); BuildHud(t,ui);
  var dim=Img(t,"Dim",null,new Color(0,0,0,.6f)); dim.rectTransform.anchorMin=new Vector2(.2f,0); dim.rectTransform.anchorMax=new Vector2(.8f,1);
  dim.rectTransform.offsetMin=dim.rectTransform.offsetMax=Vector2.zero; ui.dim=dim.gameObject;
  BuildMenu(t,ui); BuildSettings(t,ui); BuildPause(t,ui); BuildUpgrade(t,ui); BuildDefeat(t,ui); BuildVictory(t,ui); BuildToast(t,ui);

  new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));

  // начальная видимость в редакторе: фон + главное меню
  foreach(var go in new[]{ui.hud,ui.dim,ui.settingsPanel,ui.pausePanel,ui.upgradePanel,ui.defeatPanel,ui.victoryPanel,ui.toast})go.SetActive(false);

  string path="Assets/Scenes/SpaceShooter.unity";
  EditorSceneManager.SaveScene(scene,path);
  var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
  if(!list.Exists(s=>s.path==path)){list.Insert(0,new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=list.ToArray();}
  Debug.Log("Сцена создана: "+path);
 }

 // ======================================================================= сцена: камеры и Game
 static void MakeCameras() {
  var main=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)); main.tag="MainCamera";
  var c=main.GetComponent<Camera>(); c.orthographic=true; c.orthographicSize=6f; c.clearFlags=CameraClearFlags.SolidColor;
  c.backgroundColor=new Color(.02f,.03f,.1f); c.depth=0; main.transform.position=new Vector3(0,0,-10);
  // Game.Awake ужимает основную камеру до центральных 60% экрана; эта камера закрашивает остальное
  var back=new GameObject("Backdrop Camera",typeof(Camera)); var b=back.GetComponent<Camera>();
  b.orthographic=true; b.clearFlags=CameraClearFlags.SolidColor; b.backgroundColor=Pal.Navy; b.cullingMask=0; b.depth=-10;
  back.transform.position=new Vector3(0,0,-10);
 }
 static void MakeGame() {
  var go=new GameObject("Game"); var game=go.AddComponent<Game>();go.AddComponent<Starfield>();
  var so=new SerializedObject(game); var prop=so.FindProperty("campaign");
  if(prop!=null&&prop.propertyType==SerializedPropertyType.ObjectReference) {
   var guids=AssetDatabase.FindAssets("t:CampaignData");
   if(guids.Length>0){prop.objectReferenceValue=AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]),typeof(UnityEngine.Object));so.ApplyModifiedPropertiesWithoutUndo();}
   else Debug.LogWarning("CampaignData не найден в проекте — назначьте поле Campaign на объекте Game вручную.");
  } else Debug.LogWarning("Поле Campaign на объекте Game заполните вручную.");
 }

 // ======================================================================= спрайты
 static void MakeSprites() {
  R=Rounded("ui_round_l",96,28,32); RP=Rounded("ui_pill",32,16,16); RS=Rounded("ui_round_s",16,4,5);
  IcHeart=Icon("ic_heart",new[]{".##..##.","########","########","########",".######.","..####..","...##...","........"});
  IcShield=Icon("ic_shield",new[]{"########","########","########","########",".######.",".######.","..####..","...##..."});
  IcBolt=Icon("ic_bolt",new[]{"....###.","...###..","..###...",".######.","...###..","..###...",".##.....",".#......"});
  IcStar=Icon("ic_star",new[]{"...##...","...##...","..####..","########","########","..####..","...##...","...##..."});
  IcCoin=Icon("ic_coin",new[]{"..####..",".######.","########","########","########","########",".######.","..####.."});
 }
 static Sprite Rounded(string name,int size,int radius,int border) {
  var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
  for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
   float px=x+.5f,py=y+.5f,cx=Mathf.Clamp(px,radius,size-radius),cy=Mathf.Clamp(py,radius,size-radius);
   float d=Vector2.Distance(new Vector2(px,py),new Vector2(cx,cy));
   tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(radius-d+.5f)));
  }
  tex.Apply(); return Save(name,tex,border,100,FilterMode.Bilinear);
 }
 static Sprite Icon(string name,string[] rows) {
  var tex=new Texture2D(8,8,TextureFormat.RGBA32,false);
  for(int y=0;y<8;y++)for(int x=0;x<8;x++)tex.SetPixel(x,7-y,rows[y][x]=='#'?Color.white:Color.clear);
  tex.Apply(); return Save(name,tex,0,8,FilterMode.Point);
 }
 static Sprite Save(string name,Texture2D tex,int border,float ppu,FilterMode fm) {
  string path=Dir+"/UI/"+name+".png";
  File.WriteAllBytes(path,tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex);
  AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
  var ti=(TextureImporter)AssetImporter.GetAtPath(path);
  ti.textureType=TextureImporterType.Sprite; ti.spriteImportMode=SpriteImportMode.Single; ti.spriteBorder=new Vector4(border,border,border,border);
  ti.spritePixelsPerUnit=ppu; ti.filterMode=fm; ti.mipmapEnabled=false; ti.alphaIsTransparency=true; ti.textureCompression=TextureImporterCompression.Uncompressed;
  ti.SaveAndReimport();
  return AssetDatabase.LoadAssetAtPath<Sprite>(path);
 }

 // ======================================================================= помощники
 static Vector2 V(float x,float y)=>new Vector2(x,y);
 static RectTransform NewRect(string name,Transform p){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(p,false);return (RectTransform)go.transform;}
 static void Stretch(RectTransform r,float l=0,float b=0,float rt=0,float t=0){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=V(l,b);r.offsetMax=V(-rt,-t);}
 static void Place(RectTransform r,Vector2 anchor,Vector2 pos,Vector2 size,Vector2? pivot=null){r.anchorMin=r.anchorMax=anchor;r.pivot=pivot??anchor;r.anchoredPosition=pos;r.sizeDelta=size;}
 static void TopStretch(RectTransform r,float l,float rt,float top,float h){r.anchorMin=V(0,1);r.anchorMax=V(1,1);r.pivot=V(.5f,1);r.offsetMin=V(l,-(top+h));r.offsetMax=V(-rt,-top);}
 static LayoutElement LE(Component c,float w=-1,float h=-1,float fw=-1,float fh=-1) {
  var le=c.GetComponent<LayoutElement>(); if(!le)le=c.gameObject.AddComponent<LayoutElement>();
  if(w>=0)le.preferredWidth=w; if(h>=0){le.preferredHeight=h;le.minHeight=h;} if(fw>=0)le.flexibleWidth=fw; if(fh>=0)le.flexibleHeight=fh; return le;
 }
 static void Spacer(Transform p,float flex=1){var r=NewRect("Spacer",p);LE(r,-1,-1,-1,flex);}
 static VerticalLayoutGroup VBox(RectTransform r,int space,int l,int t,int rt,int b,TextAnchor a=TextAnchor.UpperCenter) {
  var v=r.gameObject.AddComponent<VerticalLayoutGroup>(); v.spacing=space; v.padding=new RectOffset(l,rt,t,b); v.childAlignment=a;
  v.childControlWidth=true; v.childControlHeight=true; v.childForceExpandWidth=true; v.childForceExpandHeight=false; return v;
 }
 static HorizontalLayoutGroup HBox(RectTransform r,int space,int l,int t,int rt,int b,TextAnchor a=TextAnchor.MiddleCenter,bool expandW=true) {
  var h=r.gameObject.AddComponent<HorizontalLayoutGroup>(); h.spacing=space; h.padding=new RectOffset(l,rt,t,b); h.childAlignment=a;
  h.childControlWidth=true; h.childControlHeight=true; h.childForceExpandWidth=expandW; h.childForceExpandHeight=true; return h;
 }
 static Image Img(Transform p,string name,Sprite s,Color c) {
  var r=NewRect(name,p); var i=r.gameObject.AddComponent<Image>(); i.sprite=s; i.color=c; i.raycastTarget=false;
  i.type=s!=null&&s.border!=Vector4.zero?Image.Type.Sliced:Image.Type.Simple; return i;
 }
 static Text Txt(Transform p,string name,string s,int size,Color c,TextAnchor a=TextAnchor.MiddleCenter,FontStyle st=FontStyle.Bold) {
  var r=NewRect(name,p); var t=r.gameObject.AddComponent<Text>();
  t.font=font; t.text=s; t.fontSize=size; t.color=c; t.alignment=a; t.fontStyle=st; t.raycastTarget=false;
  t.horizontalOverflow=HorizontalWrapMode.Wrap; t.verticalOverflow=VerticalWrapMode.Truncate;
  t.resizeTextForBestFit=true; t.resizeTextMinSize=Mathf.Max(8,size/2); t.resizeTextMaxSize=size; return t;
 }
 static void Drop(Text t,Color c,float d=3){var s=t.gameObject.AddComponent<Shadow>();s.effectColor=c;s.effectDistance=V(0,-d);}
 static void Stroke(Text t,Color c,float d){var o=t.gameObject.AddComponent<Outline>();o.effectColor=c;o.effectDistance=V(d,-d);}

 // «объёмная» плашка: рамка (опц.) + тень-бортик + лицо
 static RectTransform Card(Transform p,string name,Color face,Color shadow,float edge,bool border=false) {
  var root=NewRect(name,p);
  if(border){var b=Img(root,"Border",R,new Color(1,1,1,.92f));Stretch(b.rectTransform,-5,-5,-5,-5);}
  var sh=Img(root,"Shadow",R,shadow); Stretch(sh.rectTransform);
  var f=Img(root,"Face",R,face); Stretch(f.rectTransform,0,edge,0,0);
  return root;
 }
 static RectTransform Face(RectTransform card)=>(RectTransform)card.Find("Face");

 static ChunkyButton Btn(Transform p,string name,string text,Color face,Color shadow,float w,float h,int size=34,Color? textColor=null) {
  float edge=Mathf.Min(10,h*.14f);
  var root=Card(p,name,face,shadow,edge); var f=Face(root); var fi=f.GetComponent<Image>(); fi.raycastTarget=true;
  var lbl=Txt(f,"Label",text,size,textColor??Color.white,TextAnchor.MiddleCenter,FontStyle.BoldAndItalic); Stretch(lbl.rectTransform,12,4,12,4);
  if(!textColor.HasValue)Drop(lbl,new Color(0,0,0,.35f),3);
  var btn=root.gameObject.AddComponent<Button>(); btn.targetGraphic=fi;
  var cb=btn.colors; cb.normalColor=new Color(.92f,.92f,.92f); cb.highlightedColor=Color.white; cb.pressedColor=new Color(.8f,.8f,.8f);
  cb.selectedColor=new Color(.92f,.92f,.92f); cb.disabledColor=new Color(.5f,.5f,.55f); cb.colorMultiplier=1; cb.fadeDuration=.08f; btn.colors=cb;
  var ch=root.gameObject.AddComponent<ChunkyButton>(); ch.button=btn; ch.face=fi; ch.shadow=root.Find("Shadow").GetComponent<Image>(); ch.label=lbl;
  ch.edge=edge; ch.faceColor=face; ch.shadowColor=shadow;
  LE(root,w,h,w<=0?1:-1); return ch;
 }
 static void Pill(Transform p,string text,Color c,Color d,float h) {
  var card=Card(p,"Header",c,d,5); LE(card,-1,h,1);
  var t=Txt(Face(card),"T",text,32,Color.white,TextAnchor.MiddleCenter,FontStyle.BoldAndItalic); Stretch(t.rectTransform,8,0,8,0); Drop(t,new Color(0,0,0,.4f),3);
 }
 static SegmentBar Segments(Transform p,string name,float h,int spacing) {
  var r=NewRect(name,p); LE(r,-1,h,1);
  var hl=r.gameObject.AddComponent<HorizontalLayoutGroup>(); hl.spacing=spacing; hl.childControlWidth=true; hl.childControlHeight=true; hl.childForceExpandWidth=true; hl.childForceExpandHeight=true;
  var sb=r.gameObject.AddComponent<SegmentBar>(); var tpl=Img(r,"Template",RS,Pal.Ink); tpl.gameObject.SetActive(false); sb.template=tpl; return sb;
 }
 static Image Badge(Transform p,string name,Color c,Sprite icon,Color iconColor,float size,float iconSize) {
  var b=Img(p,name,R,c); var ic=Img(b.transform,"Icon",icon,iconColor); ic.preserveAspect=true;
  Place(ic.rectTransform,V(.5f,.5f),Vector2.zero,V(iconSize,iconSize)); return b;
 }

 // полоса с рамкой и шлейфом урона
 static GameBar BarBlock(Transform p,string name,string label,Sprite icon,Color iconColor,Color fill,float h) {
  var root=NewRect(name,p); LE(root,-1,h,1); Text lab=null;
  var top=NewRect("Top",root); top.anchorMin=V(0,1); top.anchorMax=V(1,1); top.pivot=V(.5f,1); top.offsetMin=V(0,-36); top.offsetMax=Vector2.zero;
  float x=0;
  if(icon){var ic=Img(top,"Icon",icon,iconColor);ic.preserveAspect=true;Place(ic.rectTransform,V(0,.5f),Vector2.zero,V(32,32));x=42;}
  lab=Txt(top,"Label",label,24,Pal.Text,TextAnchor.MiddleLeft); Stretch(lab.rectTransform,x,0,0,0);
  var frame=Img(root,"Frame",RP,new Color(1,1,1,.9f)); var fr=frame.rectTransform;
  fr.anchorMin=V(0,0); fr.anchorMax=V(1,0); fr.pivot=V(.5f,0); fr.offsetMin=Vector2.zero; fr.offsetMax=V(0,46);
  var track=Img(frame.transform,"Track",RP,Pal.Ink); Stretch(track.rectTransform,3,3,3,3);
  var trail=Img(track.transform,"Trail",RP,new Color(1,.93f,.6f)); Stretch(trail.rectTransform);
  var f=Img(track.transform,"Fill",RP,fill); Stretch(f.rectTransform);
  var vt=Txt(track.transform,"Value","",24,Color.white); Stretch(vt.rectTransform); Drop(vt,new Color(0,0,0,.6f),2);
  var bar=root.gameObject.AddComponent<GameBar>(); bar.fill=f.rectTransform; bar.trail=trail.rectTransform; bar.fillImage=f; bar.text=vt; bar.label=lab; return bar;
 }

 static StatRow StatCard(Transform p,string title,Sprite icon,Color accent) {
  var card=Card(p,title,Pal.Panel,Pal.Ink,6); LE(card,-1,104,1); var f=Face(card);
  var badge=Badge(f,"Badge",accent,icon,Pal.Ink,62,38); Place(badge.rectTransform,V(0,.5f),V(14,0),V(62,62));
  var name=Txt(f,"Name",title,20,Pal.Muted,TextAnchor.MiddleLeft); TopStretch(name.rectTransform,90,14,8,28);
  var lvl=Txt(f,"Level","",20,accent,TextAnchor.MiddleLeft); TopStretch(lvl.rectTransform,90,130,38,38);
  var val=Txt(f,"Value","",36,Color.white,TextAnchor.MiddleRight); Place(val.rectTransform,V(1,1),V(-14,-38),V(120,38)); Drop(val,new Color(0,0,0,.5f),2);
  var pips=Segments(f,"Pips",14,3); var pr=pips.GetComponent<RectTransform>(); pr.anchorMin=V(0,0); pr.anchorMax=V(1,0); pr.pivot=V(.5f,0); pr.offsetMin=V(90,10); pr.offsetMax=V(-14,24);
  var row=card.gameObject.AddComponent<StatRow>(); row.value=val; row.level=lvl; row.pips=pips; row.accent=accent; row.punch=val.gameObject.AddComponent<Punch>(); return row;
 }

 // окно: рамка + «лента» с заголовком, поверх верхнего края
 static (RectTransform root,RectTransform face) Modal(Transform p,string name,float w,float h,string title,Color rib,Color ribDark) {
  var root=Card(p,name,Pal.Panel,Pal.Ink,10,true); Place(root,V(.5f,.5f),Vector2.zero,V(w,h));
  root.gameObject.AddComponent<CanvasGroup>(); root.gameObject.AddComponent<PopIn>();
  var rb=Card(root,"Ribbon",rib,ribDark,8); Place(rb,V(.5f,1),V(0,8),V(Mathf.Min(w-100,620),88));
  var t=Txt(Face(rb),"Title",title,50,Color.white,TextAnchor.MiddleCenter,FontStyle.BoldAndItalic); Stretch(t.rectTransform,10,0,10,0); Drop(t,new Color(0,0,0,.4f),4);
  return (root,Face(root));
 }
 static void Keycap(Transform p,string text,float w,Color? face=null,Color? textColor=null,int size=26) {
  var c=Card(p,"Key "+text,face??Pal.C(240,244,255),Pal.C(110,130,200),6); LE(c,w,52);
  var t=Txt(Face(c),"T",text,size,textColor??Pal.Ink); Stretch(t.rectTransform,2,0,2,0);
 }
 static Slider MakeSlider(Transform p,string name,Color fillColor) {
  var root=NewRect(name,p); LE(root,-1,52,1);
  var slider=root.gameObject.AddComponent<Slider>();
  var bg=Img(root,"Background",RP,Pal.Ink); bg.rectTransform.anchorMin=V(0,.5f); bg.rectTransform.anchorMax=V(1,.5f); bg.rectTransform.offsetMin=V(14,-15); bg.rectTransform.offsetMax=V(-14,15);
  var area=NewRect("Fill Area",root); area.anchorMin=V(0,.5f); area.anchorMax=V(1,.5f); area.offsetMin=V(22,-11); area.offsetMax=V(-22,11);
  var fill=Img(area,"Fill",RP,fillColor); fill.rectTransform.anchorMin=V(0,0); fill.rectTransform.anchorMax=V(0,1); fill.rectTransform.offsetMin=fill.rectTransform.offsetMax=Vector2.zero;
  var slide=NewRect("Handle Slide Area",root); Stretch(slide,22,0,22,0);
  var handle=Img(slide,"Handle",RP,Color.white); handle.raycastTarget=true; handle.rectTransform.anchorMin=V(0,0); handle.rectTransform.anchorMax=V(0,1); handle.rectTransform.sizeDelta=V(44,0);
  slider.fillRect=fill.rectTransform; slider.handleRect=handle.rectTransform; slider.targetGraphic=handle; slider.direction=Slider.Direction.LeftToRight; slider.minValue=0; slider.maxValue=1; slider.value=.5f;
  return slider;
 }

 // ======================================================================= фон главного меню
 static void BuildBackdrop(Transform p,GameUI ui) {
  var root=NewRect("Backdrop",p); Stretch(root); ui.backdrop=root.gameObject;
  var bg=Img(root,"Background",null,Pal.Navy); Stretch(bg.rectTransform);
  for(int i=0;i<5;i++) {
   var c=i%2==0?Pal.Blue:Pal.Red; var s=Img(root,"Stripe"+i,null,new Color(c.r,c.g,c.b,i%2==0?.22f:.16f));
   Place(s.rectTransform,V(.5f,.5f),V(-900+i*450,0),V(120+i*20,2600)); s.rectTransform.localRotation=Quaternion.Euler(0,0,20);
  }
  var stars=NewRect("Stars",root); Stretch(stars); ui.stars=stars;
  var red=Img(root,"BandRed",null,Pal.Red); red.rectTransform.anchorMin=V(0,0); red.rectTransform.anchorMax=V(1,0); red.rectTransform.pivot=V(.5f,0); red.rectTransform.offsetMin=Vector2.zero; red.rectTransform.offsetMax=V(0,14);
  var blue=Img(root,"BandBlue",null,Pal.Blue); blue.rectTransform.anchorMin=V(0,0); blue.rectTransform.anchorMax=V(1,0); blue.rectTransform.pivot=V(.5f,0); blue.rectTransform.offsetMin=V(0,14); blue.rectTransform.offsetMax=V(0,28);
 }

 // ======================================================================= HUD
 static RectTransform Side(Transform p,string name,bool left) {
  var r=NewRect(name,p); r.anchorMin=V(left?0:.8f,0); r.anchorMax=V(left?.2f:1,1); r.offsetMin=r.offsetMax=Vector2.zero;
  var bg=Img(r,"BG",null,Pal.Navy); Stretch(bg.rectTransform);
  float x=left?1:0;
  var e1=Img(r,"EdgeRed",null,Pal.Red); e1.rectTransform.anchorMin=V(x,0); e1.rectTransform.anchorMax=V(x,1); e1.rectTransform.pivot=V(x,.5f); e1.rectTransform.sizeDelta=V(8,0); e1.rectTransform.anchoredPosition=Vector2.zero;
  var e2=Img(r,"EdgeBlue",null,Pal.Blue); e2.rectTransform.anchorMin=V(x,0); e2.rectTransform.anchorMax=V(x,1); e2.rectTransform.pivot=V(x,.5f); e2.rectTransform.sizeDelta=V(8,0); e2.rectTransform.anchoredPosition=V(left?-8:8,0);
  var content=NewRect("Content",r); Stretch(content); VBox(content,12,24,24,left?32:24,24); if(!left)content.GetComponent<VerticalLayoutGroup>().padding=new RectOffset(32,24,24,24);
  return content;
 }
 static RectTransform InfoCard(Transform p,string name,string label,float h) {
  var card=Card(p,name,Pal.Panel,Pal.Ink,6); LE(card,-1,h,1); var f=Face(card);
  var l=Txt(f,"Label",label,18,Pal.Muted,TextAnchor.MiddleLeft); TopStretch(l.rectTransform,16,16,8,24); return f;
 }
 static void BuildHud(Transform p,GameUI ui) {
  var hud=NewRect("HUD",p); Stretch(hud); ui.hud=hud.gameObject;

  // ---- левая панель: пилот
  var L=Side(hud,"LeftPanel",true);
  Pill(L,"ПИЛОТ",Pal.Red,Pal.RedDark,52);
  ui.hpBar=BarBlock(L,"HP","ЗДОРОВЬЕ",IcHeart,Pal.Red,Pal.Green,84);
  ui.shieldBar=BarBlock(L,"Shield","ЩИТ",IcShield,Pal.Cyan,Pal.Cyan,84);
  var sec=Txt(L,"Section","ХАРАКТЕРИСТИКИ",22,Pal.Yellow,TextAnchor.MiddleLeft,FontStyle.BoldAndItalic); LE(sec,-1,30,1);
  ui.statRows=new StatRow[4];
  ui.statRows[0]=StatCard(L,"УРОН",IcStar,Pal.Red);
  ui.statRows[1]=StatCard(L,"СКОРОСТРЕЛЬНОСТЬ",IcBolt,Pal.Yellow);
  ui.statRows[2]=StatCard(L,"ПРОЧНОСТЬ ЩИТА",IcShield,Pal.Cyan);
  ui.statRows[3]=StatCard(L,"ЗДОРОВЬЕ",IcHeart,Pal.Green);
  ui.statRows[0].accent=Pal.Red; ui.statRows[1].accent=Pal.Yellow; ui.statRows[2].accent=Pal.Cyan; ui.statRows[3].accent=Pal.Green;

  // ---- правая панель: миссия
  var Rt=Side(hud,"RightPanel",false);
  Pill(Rt,"МИССИЯ",Pal.Blue,Pal.BlueDark,52);
  var lf=InfoCard(Rt,"Location","ЛОКАЦИЯ",130);
  ui.locationTitle=Txt(lf,"Title","",38,Color.white,TextAnchor.UpperLeft,FontStyle.BoldAndItalic); TopStretch(ui.locationTitle.rectTransform,16,16,34,80);
  var wf=InfoCard(Rt,"Wave","ВОЛНА",120);
  ui.waveText=Txt(wf,"Value","",34,Pal.Yellow,TextAnchor.MiddleRight); Place(ui.waveText.rectTransform,V(1,1),V(-16,-4),V(180,40));
  ui.waveBar=Segments(wf,"Segments",22,4); var wr=ui.waveBar.GetComponent<RectTransform>(); wr.anchorMin=V(0,0); wr.anchorMax=V(1,0); wr.pivot=V(.5f,0); wr.offsetMin=V(16,16); wr.offsetMax=V(-16,38);
  var sf=InfoCard(Rt,"Score","ОЧКИ",120);
  ui.scoreText=Txt(sf,"Value","0",60,Pal.Yellow,TextAnchor.MiddleLeft,FontStyle.BoldAndItalic); TopStretch(ui.scoreText.rectTransform,16,16,30,70); Drop(ui.scoreText,new Color(0,0,0,.5f),3);
  ui.scorePunch=ui.scoreText.gameObject.AddComponent<Punch>();
  var cf=InfoCard(Rt,"Credits","",100);
  var coin=Badge(cf,"Coin",Pal.Yellow,IcCoin,Pal.YellowDark,62,40); Place(coin.rectTransform,V(0,.5f),V(14,0),V(62,62));
  var cl=Txt(cf,"CreditsLabel","КРЕДИТЫ",18,Pal.Muted,TextAnchor.MiddleLeft); TopStretch(cl.rectTransform,90,16,8,26);
  ui.creditsText=Txt(cf,"Value","0",46,Color.white,TextAnchor.MiddleLeft,FontStyle.BoldAndItalic); TopStretch(ui.creditsText.rectTransform,90,16,34,56); Drop(ui.creditsText,new Color(0,0,0,.5f),3);
  ui.creditsPunch=ui.creditsText.gameObject.AddComponent<Punch>();
  Spacer(Rt);
  var pause=Btn(Rt,"Pause","ПАУЗА",Pal.Red,Pal.RedDark,0,84,40); ui.pauseButton=pause.button;

  // ---- босс-бар по центру сверху
  var boss=NewRect("BossBar",hud); boss.anchorMin=V(.2f,1); boss.anchorMax=V(.8f,1); boss.pivot=V(.5f,1); boss.offsetMin=V(40,-124); boss.offsetMax=V(-40,-24);
  ui.bossBar=BarBlock(boss,"Bar","БОСС",null,Color.white,Pal.Red,100); Stretch(ui.bossBar.GetComponent<RectTransform>()); ui.bossRoot=boss.gameObject;
  ui.bossBar.label.fontStyle=FontStyle.BoldAndItalic; ui.bossRoot.SetActive(false);
 }

 // ======================================================================= главное меню
 static void BuildMenu(Transform p,GameUI ui) {
  var root=NewRect("MenuPanel",p); Stretch(root); ui.menuPanel=root.gameObject;
  var logo=NewRect("Logo",root); Place(logo,V(.5f,1),V(0,-160),V(1500,260)); ui.logo=logo;
  var tag=Card(logo,"Tag",Pal.Red,Pal.RedDark,6); Place(tag,V(.5f,1),V(0,-10),V(420,60));
  var tt=Txt(Face(tag),"T","SPACE SHOOTER",30,Color.white,TextAnchor.MiddleCenter,FontStyle.BoldAndItalic); Stretch(tt.rectTransform); Drop(tt,new Color(0,0,0,.35f),3);
  var title=Txt(logo,"Title","ЗВЁЗДНЫЙ РУБЕЖ",100,Color.white,TextAnchor.MiddleCenter,FontStyle.BoldAndItalic);
  Place(title.rectTransform,V(.5f,0),V(0,40),V(1500,200)); Stroke(title,Pal.Blue,6); Drop(title,Pal.Red,14);
  var box=NewRect("Buttons",root); Place(box,V(.5f,.5f),V(0,-150),V(640,500)); VBox(box,18,0,0,0,0);
  ui.newGameBtn=Btn(box,"NewGame","НОВАЯ ИГРА",Pal.Red,Pal.RedDark,0,110,48).button;
  ui.continueBtn=Btn(box,"Continue","ЗАГРУЗИТЬ СОХРАНЕНИЕ",Pal.Blue,Pal.BlueDark,0,96,32).button;
  ui.settingsBtn=Btn(box,"Settings","НАСТРОЙКИ",Pal.Yellow,Pal.YellowDark,0,96,34,Pal.Ink).button;
  ui.quitBtn=Btn(box,"Quit","ВЫХОД",Pal.PanelLight,Pal.Panel,0,84,30).button;
 }

 // ======================================================================= настройки
 static Text LabelValue(Transform p,string label) {
  var row=NewRect("Row",p); LE(row,-1,40,1); HBox(row,10,0,0,0,0,TextAnchor.MiddleCenter);
  var l=Txt(row,"Label",label,26,Pal.Text,TextAnchor.MiddleLeft); LE(l,-1,-1,1);
  var v=Txt(row,"Value","",30,Pal.Yellow,TextAnchor.MiddleRight); LE(v,140); return v;
 }
 static void BuildSettings(Transform p,GameUI ui) {
  var m=Modal(p,"SettingsPanel",860,680,"НАСТРОЙКИ",Pal.Blue,Pal.BlueDark); ui.settingsPanel=m.root.gameObject;
  VBox(m.face,14,56,84,56,36);
  ui.musicValue=LabelValue(m.face,"ГРОМКОСТЬ МУЗЫКИ"); ui.musicSlider=MakeSlider(m.face,"MusicSlider",Pal.Blue);
  ui.effectsValue=LabelValue(m.face,"ГРОМКОСТЬ ЭФФЕКТОВ"); ui.effectsSlider=MakeSlider(m.face,"EffectsSlider",Pal.Red);
  var dl=Txt(m.face,"DiffLabel","СЛОЖНОСТЬ",26,Pal.Text,TextAnchor.MiddleLeft); LE(dl,-1,40,1);
  var row=NewRect("Difficulty",m.face); LE(row,-1,84,1); HBox(row,16,0,0,0,0);
  ui.difficultyBtns=new[]{
   Btn(row,"Easy","ЛЕГКО",Pal.Green,Pal.GreenDark,0,84,28),
   Btn(row,"Normal","НОРМАЛЬНО",Pal.Yellow,Pal.YellowDark,0,84,28,Pal.Ink),
   Btn(row,"Hard","СЛОЖНО",Pal.Red,Pal.RedDark,0,84,28)};
  Spacer(m.face);
  ui.settingsBackBtn=Btn(m.face,"Back","НАЗАД",Pal.Red,Pal.RedDark,0,90,40).button;
 }

 // ======================================================================= пауза + подсказки управления
 static void BuildPause(Transform p,GameUI ui) {
  var m=Modal(p,"PausePanel",1060,600,"ПАУЗА",Pal.Blue,Pal.BlueDark); ui.pausePanel=m.root.gameObject;
  HBox(m.face,32,44,96,44,44,TextAnchor.UpperCenter);
  var left=NewRect("Buttons",m.face); LE(left,380,-1,0); VBox(left,20,0,0,0,0);
  ui.resumeBtn=Btn(left,"Resume","ПРОДОЛЖИТЬ",Pal.Green,Pal.GreenDark,0,104,38).button;
  ui.pauseSettingsBtn=Btn(left,"Settings","НАСТРОЙКИ",Pal.Blue,Pal.BlueDark,0,104,38).button;
  ui.pauseMenuBtn=Btn(left,"Menu","В ГЛАВНОЕ МЕНЮ",Pal.Red,Pal.RedDark,0,104,32).button;

  var cc=Card(m.face,"Controls",Pal.Navy,Pal.Ink,6); LE(cc,-1,-1,1,1); var cf=Face(cc); VBox(cf,12,28,22,28,22);
  var title=Txt(cf,"Title","УПРАВЛЕНИЕ",34,Pal.Yellow,TextAnchor.MiddleLeft,FontStyle.BoldAndItalic); LE(title,-1,44,1);

  var l1=Txt(cf,"L1","ДВИЖЕНИЕ",20,Pal.Muted,TextAnchor.MiddleLeft); LE(l1,-1,26,1);
  var r1=NewRect("Move",cf); LE(r1,-1,52,1); HBox(r1,8,0,0,0,0,TextAnchor.MiddleLeft,false);
  foreach(var k in new[]{"W","A","S","D"})Keycap(r1,k,50);
  var or=Txt(r1,"Or","или",20,Pal.Muted); LE(or,48);
  foreach(var k in new[]{"↑","←","↓","→"})Keycap(r1,k,50);

  var l2=Txt(cf,"L2","СТРЕЛЬБА",20,Pal.Muted,TextAnchor.MiddleLeft); LE(l2,-1,26,1);
  var r2=NewRect("Fire",cf); LE(r2,-1,52,1); HBox(r2,14,0,0,0,0,TextAnchor.MiddleLeft,false);
  Keycap(r2,"АВТО",110,Pal.Red,Color.white,22);
  var d2=Txt(r2,"Desc","Корабль стреляет сам",24,Pal.Text,TextAnchor.MiddleLeft,FontStyle.Normal); LE(d2,-1,-1,1);

  var l3=Txt(cf,"L3","ПАУЗА",20,Pal.Muted,TextAnchor.MiddleLeft); LE(l3,-1,26,1);
  var r3=NewRect("Pause",cf); LE(r3,-1,52,1); HBox(r3,14,0,0,0,0,TextAnchor.MiddleLeft,false);
  Keycap(r3,"Esc",90,null,null,24);
  var d3=Txt(r3,"Desc","Пауза / продолжить",24,Pal.Text,TextAnchor.MiddleLeft,FontStyle.Normal); LE(d3,-1,-1,1);
 }

 // ======================================================================= ангар
 static UpgradeCard UpgradeTile(Transform p,string title,string desc,Sprite icon,Color accent) {
  var card=Card(p,title,Pal.Panel,Pal.Ink,8); var f=Face(card);
  card.GetComponent<RectTransform>();
  var badge=Badge(f,"Badge",accent,icon,Pal.Ink,84,52); Place(badge.rectTransform,V(0,1),V(16,-16),V(84,84));
  var tt=Txt(f,"Title",title,30,Color.white,TextAnchor.MiddleLeft,FontStyle.BoldAndItalic); TopStretch(tt.rectTransform,116,16,14,38);
  var dd=Txt(f,"Desc",desc,20,Pal.Muted,TextAnchor.MiddleLeft,FontStyle.Normal); TopStretch(dd.rectTransform,116,16,52,28);
  var pv=Txt(f,"Preview","",32,Pal.Yellow,TextAnchor.MiddleLeft,FontStyle.BoldAndItalic); TopStretch(pv.rectTransform,116,16,84,40); Drop(pv,new Color(0,0,0,.5f),2);
  var pips=Segments(f,"Pips",14,3); var pr=pips.GetComponent<RectTransform>(); pr.anchorMin=V(0,1); pr.anchorMax=V(1,1); pr.pivot=V(.5f,1); pr.offsetMin=V(116,-148); pr.offsetMax=V(-16,-134);
  var buy=Btn(f,"Buy","КУПИТЬ",Pal.Green,Pal.GreenDark,0,60,28); var br=buy.GetComponent<RectTransform>();
  br.anchorMin=V(0,0); br.anchorMax=V(1,0); br.pivot=V(.5f,0); br.offsetMin=V(16,14); br.offsetMax=V(-16,74);
  return new UpgradeCard{preview=pv,pips=pips,buy=buy,accent=accent};
 }
 static void BuildUpgrade(Transform p,GameUI ui) {
  var m=Modal(p,"UpgradePanel",1080,770,"АНГАР",Pal.Red,Pal.RedDark); ui.upgradePanel=m.root.gameObject;
  var sub=Txt(m.face,"Sub","Улучшения до следующей волны",26,Pal.Muted,TextAnchor.MiddleLeft,FontStyle.Normal); TopStretch(sub.rectTransform,44,330,60,40);
  var cr=Img(m.face,"Credits",RP,Pal.Ink); Place(cr.rectTransform,V(1,1),V(-36,-56),V(260,58),V(1,1));
  var coin=Img(cr.transform,"Coin",IcCoin,Pal.Yellow); coin.preserveAspect=true; Place(coin.rectTransform,V(0,.5f),V(14,0),V(34,34));
  ui.upgradeCredits=Txt(cr.transform,"Value","0",34,Pal.Yellow,TextAnchor.MiddleRight); Stretch(ui.upgradeCredits.rectTransform,60,0,20,0);
  var grid=NewRect("Grid",m.face); TopStretch(grid,40,40,112,490);
  var g=grid.gameObject.AddComponent<GridLayoutGroup>(); g.cellSize=V(490,236); g.spacing=V(20,18); g.constraint=GridLayoutGroup.Constraint.FixedColumnCount; g.constraintCount=2; g.childAlignment=TextAnchor.UpperCenter;
  ui.cards=new UpgradeCard[4];
  ui.cards[0]=UpgradeTile(grid,"УРОН","Урон за выстрел",IcStar,Pal.Red);
  ui.cards[1]=UpgradeTile(grid,"СКОРОСТРЕЛЬНОСТЬ","Выстрелов в секунду",IcBolt,Pal.Yellow);
  ui.cards[2]=UpgradeTile(grid,"ПРОЧНОСТЬ ЩИТА","Максимум щита (+25)",IcShield,Pal.Cyan);
  ui.cards[3]=UpgradeTile(grid,"ЗДОРОВЬЕ","Максимум HP (+25) и лечение",IcHeart,Pal.Green);
  var next=Btn(m.face,"NextWave","СЛЕДУЮЩАЯ ВОЛНА",Pal.Red,Pal.RedDark,0,90,40); var nr=next.GetComponent<RectTransform>();
  Place(nr,V(.5f,0),V(0,26),V(620,90),V(.5f,0)); ui.nextWaveBtn=next.button;
 }

 // ======================================================================= поражение / победа
 static void BuildDefeat(Transform p,GameUI ui) {
  var m=Modal(p,"DefeatPanel",780,520,"ПОРАЖЕНИЕ",Pal.Red,Pal.RedDark); ui.defeatPanel=m.root.gameObject;
  VBox(m.face,16,48,84,48,36);
  var t=Txt(m.face,"Note","Прогресс этой локации откатывается.",30,Color.white,TextAnchor.MiddleCenter,FontStyle.Normal); LE(t,-1,90,1);
  Spacer(m.face);
  ui.restartBtn=Btn(m.face,"Restart","НАЧАТЬ ЛОКАЦИЮ ЗАНОВО",Pal.Yellow,Pal.YellowDark,0,96,32,Pal.Ink).button;
  ui.defeatMenuBtn=Btn(m.face,"Menu","В МЕНЮ",Pal.Blue,Pal.BlueDark,0,96,36).button;
 }
 static void BuildVictory(Transform p,GameUI ui) {
  var m=Modal(p,"VictoryPanel",780,620,"ПОБЕДА!",Pal.Yellow,Pal.YellowDark); ui.victoryPanel=m.root.gameObject;
  VBox(m.face,14,48,84,48,36);
  ui.victoryNote=Txt(m.face,"Note","",32,Color.white,TextAnchor.MiddleCenter,FontStyle.BoldAndItalic); LE(ui.victoryNote,-1,50,1);
  var l=Txt(m.face,"ScoreLabel","ОЧКИ",24,Pal.Muted); LE(l,-1,30,1);
  ui.victoryScore=Txt(m.face,"Score","0",84,Pal.Yellow,TextAnchor.MiddleCenter,FontStyle.BoldAndItalic); LE(ui.victoryScore,-1,110,1); Drop(ui.victoryScore,new Color(0,0,0,.5f),4);
  Spacer(m.face);
  ui.nextLocBtn=Btn(m.face,"NextLocation","СЛЕДУЮЩАЯ ЛОКАЦИЯ",Pal.Green,Pal.GreenDark,0,96,36).button;
  ui.victoryMenuBtn=Btn(m.face,"Menu","В МЕНЮ",Pal.Blue,Pal.BlueDark,0,96,36).button;
 }

 static void BuildToast(Transform p,GameUI ui) {
  var toast=Card(p,"Toast",Pal.Red,Pal.RedDark,6,true); Place(toast,V(.5f,0),V(0,40),V(900,70),V(.5f,0));
  ui.toastText=Txt(Face(toast),"T","",26,Color.white); Stretch(ui.toastText.rectTransform,20,0,20,0); ui.toast=toast.gameObject;
 }
}
}


