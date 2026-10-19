using System.Collections.Generic; using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems; namespace SpaceShooter {public class Punch:MonoBehaviour {
 float s;
 public void Hit(float amount=.25f){s=amount;}
 void Update() {
  if(s>.001f){s=Mathf.Lerp(s,0,1-Mathf.Exp(-12*Time.unscaledDeltaTime));transform.localScale=Vector3.one*(1+s);}
  else if(s!=0){s=0;transform.localScale=Vector3.one;}
 }
}}
