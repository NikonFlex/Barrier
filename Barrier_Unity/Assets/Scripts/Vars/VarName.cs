using UnityEngine;
using System;

public enum VarName
{
   UNDEFINED,
   [VarName(DisplayText = "Пеленг на цель", Persist = true, Units = "Град", DefaultValue = 180f)]
   StartBearingToTarget,
   [VarName(DisplayText = "Расстояние до цели", Persist = true, Units = "м", DefaultValue = 7000f)]
   StartDistanceToTarget,
   [VarName(DisplayText = "Использовать МСЦ", Persist = true, DefaultValue = true)]
   MSC_USE,
   [VarName(DisplayText = "Дистанция МСЦ", Persist = true, DefaultValue = 800f)]
   MSC_DISTANCE,
   [VarName(DisplayText = "Погода", Persist = true, 
      Vt = VarType.Enum, Variants = new string[] { "Штиль", "Шторм" })]
   Weather,
   [VarName(DisplayText = "Количество буев", Persist = true,
      Vt = VarType.Enum, Variants = new string[] { "1", "2", "3", "4", "5", "6" })]
   NumBuoys,
   [VarName(DisplayText = "Дальность обнаружения буями", Persist = true, Units = "м", DefaultValue = 5000f)]
   BuoysDetectRange,
   [VarName(DisplayText = "Дальность полета буев", Persist = true, Units = "м", DefaultValue = 5000f)]
   BuoysShootRange,
   [VarName(DisplayText = "Расстояние между буями", Persist = true, Units = "м", DefaultValue = 1000f)]
   BouysDistanceBetween,
   [VarName(DisplayText = "Высота открытия парашюта", Persist = true, Units = "м", DefaultValue = 200f)]
   BuoysOpenConeHeight,
   [VarName(DisplayText = "Погрешность постановки буя", Persist = true, Units = "м", DefaultValue = 500f)]
   BuoysSettingPostionError,
   [VarName(DisplayText = "Погрешность пеленга буя", Persist = true, Units = "град", DefaultValue = 10f)]
   BuoysBearingError,
   [VarName(DisplayText = "Время готовности буя", Persist = true, Units = "сек", DefaultValue = 3f)]
   BuoyReadyTime,

   // runtime vars
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 2f)]
   CurrentTime,
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 1f)]
   TargetBearing,
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 1f)]
   TargetDistance,
   [VarName(Persist = false, DefaultValue = "")]
   ScenarioPhaseName,

   
   [VarName(HideInInspector = true)]
   VARSYNC_LAST
};

[Serializable]
public class VarNameSelector
{
   public string Name;
};