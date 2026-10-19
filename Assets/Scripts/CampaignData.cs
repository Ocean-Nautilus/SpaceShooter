using UnityEngine; namespace SpaceShooter {[CreateAssetMenu(menuName="Space Shooter/Campaign")]
public class CampaignData : ScriptableObject {
 public LocationData[] locations; public Sprite playerSprite, bulletSprite;
 public AudioClip menuMusic, shot, explosion, engine, click, victory, defeat;
}
}


