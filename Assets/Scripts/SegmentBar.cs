using System.Collections.Generic; using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems; namespace SpaceShooter {public class SegmentBar:MonoBehaviour {
 public Image template; readonly List<Image> items=new List<Image>();
 public void Set(int total,int filled,Color on,Color off,int current=-1,Color currentColor=default) {
  while(items.Count<total){var i=Instantiate(template,template.transform.parent);i.gameObject.SetActive(true);items.Add(i);}
  for(int i=0;i<items.Count;i++) {
   bool visible=i<total;
   if(items[i].gameObject.activeSelf!=visible)items[i].gameObject.SetActive(visible);
   if(visible)items[i].color=i==current?currentColor:i<filled?on:off;
  }
 }
}}
