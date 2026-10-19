using System.Collections.Generic; using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems; namespace SpaceShooter {[RequireComponent(typeof(CanvasGroup))]
public class PopIn:MonoBehaviour {
 public float duration=.32f; float t=1; CanvasGroup g;
 void OnEnable(){g=GetComponent<CanvasGroup>();t=0;Apply(0);}
 void Update(){if(t>=duration)return;t+=Time.unscaledDeltaTime;Apply(Mathf.Clamp01(t/duration));}
 void Apply(float k){float x=k-1,c1=1.70158f,c3=c1+1;float e=1+c3*x*x*x+c1*x*x;transform.localScale=Vector3.one*(.82f+.18f*e);g.alpha=Mathf.Clamp01(k*3f);}
}}
