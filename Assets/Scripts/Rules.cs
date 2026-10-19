using System;
using UnityEngine;
namespace SpaceShooter {
[Serializable] public class Progress {
 public int location, score, credits, damage, rate, shield, health;
 public Progress Copy() { return (Progress)MemberwiseClone(); }
}
public static class Rules {
 public static int Count(int wave, int difficulty) { return 5+wave*3+difficulty*3; }
 public static int MiniWave(int waves) { return Mathf.CeilToInt(waves*.7f); }
 public static int Cost(int level) { return 150+level*100; }
 public static float Absorb(ref float shield, float damage) {
  if(shield<=0) return damage;
  float absorbed=Mathf.Min(shield, damage*.65f); shield-=absorbed; return damage-absorbed;
 }
}
}

