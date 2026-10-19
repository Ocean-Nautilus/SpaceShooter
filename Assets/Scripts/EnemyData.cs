using UnityEngine;
namespace SpaceShooter {
public enum Motion { Stationary, Strafe, Dive, Sweep }
[CreateAssetMenu(menuName="Space Shooter/Enemy")]
public class EnemyData : ScriptableObject {
 public string title; public Motion motion; public float health=3, speed=1, fireInterval=2, damage=10;
 public int score=100, pellets=1; public float spread=18; public Color color=Color.red; public Sprite sprite;
}
}
