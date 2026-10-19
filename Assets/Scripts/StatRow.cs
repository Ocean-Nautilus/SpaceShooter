using System.Collections.Generic; using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems; namespace SpaceShooter {public class StatRow:MonoBehaviour {
 public Text value,level; public SegmentBar pips; public Punch punch; public Color accent;
 int last=-1;
 public void Set(string v,int lvl,int total,bool max) {
  value.text=v;level.text=max?"МАКС":"УР. "+lvl;
  pips.Set(total,Mathf.Min(lvl,total),accent,Pal.Ink);
  if(last!=lvl){if(last>=0)punch.Hit(.3f);last=lvl;}
 }
}}
