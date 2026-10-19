using UnityEngine;
namespace SpaceShooter {
// Two geometric animation frames; replace these with artist sprites when available.
public class Exhaust : MonoBehaviour {
 SpriteRenderer visual; Sprite[] frames; float clock;
 void Start(){
  var texture=new Texture2D(4,8);texture.filterMode=FilterMode.Point;
  for(int y=0;y<8;y++)for(int x=0;x<4;x++)texture.SetPixel(x,y,new Color(0,0,0,0));
  for(int y=0;y<3;y++)for(int x=1;x<3;x++)texture.SetPixel(x,y,Color.white);
  for(int y=4;y<8;y++)for(int x=1;x<3;x++)texture.SetPixel(x,y,Color.white);
  texture.Apply();frames=new[]{Sprite.Create(texture,new Rect(0,0,4,4),new Vector2(.5f,1),4),Sprite.Create(texture,new Rect(0,4,4,4),new Vector2(.5f,1),4)};
  var child=new GameObject("Animated engine flame");child.transform.SetParent(transform,false);child.transform.localPosition=new Vector3(0,-.5f,0);
  visual=child.AddComponent<SpriteRenderer>();visual.color=new Color(.2f,.6f,1);visual.sortingOrder=-1;
 }
 void Update(){if(Game.I.Playing)clock+=Time.deltaTime;visual.sprite=frames[(int)(clock*12)%2];}
}
}
