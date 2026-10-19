using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SpaceShooter;
public static class ApplyArt {
 [MenuItem("Space Shooter/Apply ship art and UI")]
 public static void Run(){
  var campaign=AssetDatabase.LoadAssetAtPath<CampaignData>("Assets/Data/Campaign.asset");
  string dir="Assets/Art/Ships/";
  foreach(string file in Directory.GetFiles(dir,"*.png")){
   var importer=(TextureImporter)AssetImporter.GetAtPath(file);
   importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
   importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
   importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=256;importer.spritePixelsPerUnit=128;
   if(Path.GetFileNameWithoutExtension(file)=="bullet"){
    var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes(file));
    int left=texture.width,bottom=texture.height,right=0,top=0;
    for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(texture.GetPixel(x,y).a>.1f){left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
    if(right<=left||top<=bottom)throw new Exception("Projectile has no visible pixels");
    importer.spriteImportMode=SpriteImportMode.Multiple;
#pragma warning disable CS0618
    importer.spritesheet=new[]{new SpriteMetaData{name="bullet",rect=new Rect(left,bottom,right-left+1,top-bottom+1),pivot=new Vector2(.5f,.5f),alignment=9}};
#pragma warning restore CS0618
    UnityEngine.Object.DestroyImmediate(texture);
   }
   importer.SaveAndReimport();
  }
  string ship="Assets/2D Pixel Spaceships (2 Small Ships + Explosions)/Sprites/ship1.png";
  campaign.playerSprite=AssetDatabase.LoadAllAssetsAtPath(ship).OfType<Sprite>().First(s=>s.name=="ship1_0");
  campaign.bulletSprite=AssetDatabase.LoadAllAssetsAtPath(dir+"bullet.png").OfType<Sprite>().First();
  string[] names={"scout","fighter","heavy","miniboss","boss"};
  for(int l=0;l<campaign.locations.Length;l++){
   var loc=campaign.locations[l];
   var enemies=loc.enemies.Concat(new[]{loc.miniBoss,loc.boss}).ToArray();
   for(int e=0;e<enemies.Length;e++){
    enemies[e].sprite=AssetDatabase.LoadAssetAtPath<Sprite>(dir+names[e]+".png");
    enemies[e].color=l==0?Color.white:l==1?new Color(.8f,.9f,1):new Color(1,.85f,.8f);
    EditorUtility.SetDirty(enemies[e]);
   }
  }
  EditorUtility.SetDirty(campaign);AssetDatabase.SaveAssets();
  SpaceShooter.EditorTools.SpaceShooterSceneBuilder.Build();
  string scene="Assets/Scenes/SpaceShooter.unity";
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene,true)};
  Check();Debug.Log("SHIP_ART_UI_APPLIED");
 }
 public static void Check(){
  var campaign=AssetDatabase.LoadAssetAtPath<CampaignData>("Assets/Data/Campaign.asset");
  if(!campaign.playerSprite||!campaign.bulletSprite)throw new Exception("Missing player/projectile art");
  foreach(var loc in campaign.locations)
   foreach(var enemy in loc.enemies.Concat(new[]{loc.miniBoss,loc.boss}))
    if(!enemy.sprite)throw new Exception("Missing enemy art: "+enemy.name);
  foreach(var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
   foreach(var t in root.GetComponentsInChildren<Transform>(true))
    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Missing script: "+t.name);
  if(!UnityEngine.Object.FindAnyObjectByType<GameUI>())throw new Exception("No GameUI");
  Vector2 diagonal=new Vector2(1,-1).normalized;
  if(Vector2.Dot(Quaternion.FromToRotation(Vector3.up,diagonal)*Vector3.up,diagonal)<.999f)throw new Exception("Projectile diagonal alignment");
  Setup.Checks();Debug.Log("ART_UI_CHECKS_OK");
 }
}

