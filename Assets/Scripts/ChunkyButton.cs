using System.Collections.Generic; using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems; namespace SpaceShooter {public class ChunkyButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
 public Button button; public Image face,shadow; public Text label; public float edge=8;
 public Color faceColor=Color.white,shadowColor=Color.black;
 bool pressed,lastInteractable=true;
 void OnEnable(){if(!button||!face||!shadow)return;pressed=false;Apply();lastInteractable=button.interactable;Refresh();}
 void Update(){if(button.interactable!=lastInteractable){lastInteractable=button.interactable;pressed=false;Apply();Refresh();}}
 public void SetColors(Color f,Color s){faceColor=f;shadowColor=s;face.color=f;Refresh();}
 void Refresh(){
  shadow.color=button.interactable?shadowColor:Color.Lerp(shadowColor,Pal.Ink,.6f);
  if(label)label.CrossFadeAlpha(button.interactable?1f:.5f,0,true);
 }
 void Apply(){var r=face.rectTransform;float down=pressed&&button.interactable?edge*.75f:0;r.offsetMin=new Vector2(0,edge-down);r.offsetMax=new Vector2(0,-down);}
 public void OnPointerDown(PointerEventData e){pressed=true;Apply();}
 public void OnPointerUp(PointerEventData e){pressed=false;Apply();}
 public void OnPointerExit(PointerEventData e){pressed=false;Apply();}
}}

