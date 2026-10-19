using System.Collections.Generic; using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems; namespace SpaceShooter {public class GameBar:MonoBehaviour {
 public RectTransform fill,trail; public Image fillImage; public Text text,label;
 float trailValue=1,lastValue=1,delay;
 public void Set(float value,float max,Color color,string caption,bool urgent=false) {
  float t=max>0?Mathf.Clamp01(value/max):0,dt=Time.unscaledDeltaTime;
  if(t<lastValue-.0005f)delay=.45f;
  lastValue=t;
  if(t>=trailValue)trailValue=t;
  else if(delay>0)delay-=dt;
  else trailValue=Mathf.MoveTowards(trailValue,t,dt*.6f);
  Place(fill,t);Place(trail,trailValue);
  fillImage.color=urgent?Color.Lerp(color,Color.white,(Mathf.Sin(Time.unscaledTime*12)*.5f+.5f)*.55f):color;
  if(text)text.text=caption;
 }
 static void Place(RectTransform r,float t){r.anchorMax=new Vector2(t,1);r.gameObject.SetActive(t>.002f);}
}}
