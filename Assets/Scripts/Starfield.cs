using UnityEngine;
namespace SpaceShooter {
public class Starfield : MonoBehaviour {
 SpriteRenderer[] stars; GameObject backdrop; int previous=-1;
 void Start() {
  stars=new SpriteRenderer[100];
  for(int i=0;i<stars.Length;i++){
   var g=new GameObject("Pixel star");g.transform.parent=transform;g.transform.position=new Vector3(Random.Range(-4.8f,4.8f),Random.Range(-6f,6f),5);
   g.transform.localScale=Vector3.one*Random.Range(.025f,.075f);stars[i]=g.AddComponent<SpriteRenderer>();stars[i].sprite=Game.I.Shape;stars[i].sortingOrder=-5;
  }
  backdrop=new GameObject("Location background");var sr=backdrop.AddComponent<SpriteRenderer>();sr.sortingOrder=-10;
 }
 void Update(){
  int current=Game.I.run.location;var loc=Game.I.Location;
  if(previous!=current){previous=current;var sr=backdrop.GetComponent<SpriteRenderer>();sr.sprite=loc.backgroundSprite;backdrop.transform.localScale=new Vector3(9,12,1);}
  foreach(var s in stars){s.color=loc.accent;var p=s.transform.position;p.y-=Time.deltaTime*.5f;if(p.y< -6)p.y=6;s.transform.position=p;}
 }
}
}
