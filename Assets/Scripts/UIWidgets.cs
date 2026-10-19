using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceShooter {

// Палитра: синий «Sega» + красный «Nintendo» + жёлтый/зелёный акценты
public static class Pal {
 public static Color C(int r,int g,int b,int a=255)=>new Color32((byte)r,(byte)g,(byte)b,(byte)a);
 public static readonly Color
  Navy=C(10,16,52), Panel=C(20,34,102), PanelLight=C(34,56,150), Ink=C(8,12,36),
  Blue=C(30,100,240), BlueDark=C(14,52,160),
  Red=C(232,10,28), RedDark=C(140,4,18),
  Yellow=C(255,206,0), YellowDark=C(176,126,0),
  Green=C(46,196,96), GreenDark=C(18,110,50),
  Cyan=C(70,220,255), CyanDark=C(20,120,170),
  Text=C(220,234,255), Muted=C(130,155,215);
 public static Color HpColor(float t)=>t>.5f?Green:t>.25f?Yellow:Red;
}

// Короткая «пружинка» масштаба при изменении значения


// Появление окна: масштаб с перелётом + fade


// Объёмная кнопка: тень-«бортик» снизу, при нажатии лицевая часть вдавливается


// Ряд сегментов (пипсы уровней, прогресс волн)


// Полоса здоровья/щита с «шлейфом» урона (как в файтингах Sega)


// Карточка характеристики в HUD


// Карточка улучшения в ангаре
[System.Serializable]
public class UpgradeCard {
 public Text preview; public SegmentBar pips; public ChunkyButton buy; public Color accent;
}

}

