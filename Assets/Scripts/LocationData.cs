using UnityEngine;
namespace SpaceShooter {
[CreateAssetMenu(menuName="Space Shooter/Location")]
public class LocationData : ScriptableObject {
 public string title; public int waves=5; public float strength=1; public Color background, accent;
 public EnemyData[] enemies; public EnemyData miniBoss, boss; public Sprite backgroundSprite;
 public AudioClip music; public int bossPattern;
}
}
