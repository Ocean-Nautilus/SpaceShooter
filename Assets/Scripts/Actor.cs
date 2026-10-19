using UnityEngine;
namespace SpaceShooter {
public class Actor : MonoBehaviour {
 public EnemyData data; public bool boss; public float hp, maxHp, fire, age, flash; public Vector2 origin;
 SpriteRenderer visual; float smoke;
 public void Init(EnemyData d, bool b) {
  data=d; boss=b; hp=maxHp=d.health*Game.I.Location.strength*(b?1+Game.I.difficulty*.45f:1+Game.I.difficulty*.15f);
  origin=transform.position; visual=gameObject.AddComponent<SpriteRenderer>();
  visual.sprite=d.sprite?d.sprite:Game.I.Shape; visual.color=d.color;
  transform.localScale=b?new Vector3(2.2f,2.2f,1):d.motion==Motion.Strafe?new Vector3(1,.6f,1):new Vector3(.65f,.65f,1); if(d.sprite){var size=d.sprite.bounds.size;var target=transform.localScale;transform.localScale=Vector3.one*Mathf.Min(target.x/size.x,target.y/size.y);} if(!d.sprite && !b && d.motion==Motion.Dive)transform.rotation=Quaternion.Euler(0,0,45);fire=1;
 }
 void Update() {
  if(!Game.I.Playing) return;
  age+=Time.deltaTime; flash-=Time.deltaTime; visual.color=flash>0?new Color(1,.3f,.3f):data.color;
  Vector2 p=transform.position;
  if(p.y>origin.y-1) p.y-=Time.deltaTime*data.speed;
  if(data.motion==Motion.Strafe || boss) p.x=origin.x+Mathf.Sin(age*data.speed)* (boss?2.5f:1.1f);
  if(data.motion==Motion.Dive && age>2) p=Vector2.MoveTowards(p,Game.I.Player,Time.deltaTime*data.speed);
  if(data.motion==Motion.Sweep) {p.x=origin.x+Mathf.Sin(age*data.speed)*2; p.y-=Time.deltaTime*.12f;}
  if(boss)p.y=Mathf.Max(3.1f,p.y);p.x=Mathf.Clamp(p.x,-4.3f,4.3f); transform.position=p;
  fire-=Time.deltaTime;
  if(fire<=0) { fire=data.fireInterval/(1+Game.I.run.location*.12f+Game.I.difficulty*.12f); Shoot(); }
  if(Vector2.Distance(p,Game.I.Player)<(boss?1.2f:.65f)) {Game.I.Hurt(data.damage*Game.I.Location.strength); if(!boss) Hit(9999);}
  if(p.y< -6) Hit(9999,false);
  if(boss && hp<maxHp*.5f && (smoke-=Time.deltaTime)<=0) {smoke=.2f;Game.I.Burst(p,Color.gray,5);}
 }
 void Shoot() {
  Vector2 aim=(Game.I.Player-(Vector2)transform.position).normalized;
  int count=data.pellets+(boss?Game.I.Location.bossPattern:0);
  if(boss && Mathf.FloorToInt(age/5)%2==1) {
   for(int i=0;i<12+count;i++) Game.I.Bullet(transform.position,Quaternion.Euler(0,0,i*360f/(12+count)+age*15)*Vector2.down,false,data.damage);
  } else for(int i=0;i<count;i++) Game.I.Bullet(transform.position,Quaternion.Euler(0,0,(i-(count-1)*.5f)*data.spread)*aim,false,data.damage);
 }
 public void Hit(float damage,bool reward=true) {
  if(hp<=0)return;hp-=damage;flash=.09f;if(hp>0)return;
  if(reward){Game.I.run.score+=Mathf.RoundToInt(data.score*Game.I.Location.strength);Game.I.run.credits+=Mathf.RoundToInt(data.score*.5f);}
  Game.I.Burst(transform.position,data.color,25);Game.I.Sound(Game.I.campaign.explosion,95,.16f);Destroy(gameObject);
 }
}
public class Shot : MonoBehaviour {
 public Vector2 velocity; public bool friendly; public float damage;
 void Update() {
  if(!Game.I.Playing)return;
  transform.position+=(Vector3)(velocity*Time.deltaTime);
  Vector2 p=transform.position;
  if(Mathf.Abs(p.x)>5.5f || Mathf.Abs(p.y)>6.4f){Destroy(gameObject);return;}
  if(friendly) {
   foreach(var enemy in FindObjectsByType<Actor>())
    if(Vector2.Distance(p,enemy.transform.position)<(enemy.boss?1.05f:.45f)){enemy.Hit(damage);Destroy(gameObject);return;}
  } else if(Vector2.Distance(p,Game.I.Player)<.33f){Game.I.Hurt(damage*Game.I.Location.strength);Destroy(gameObject);}
 }
}
}






