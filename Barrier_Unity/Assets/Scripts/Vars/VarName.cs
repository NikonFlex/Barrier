using UnityEngine;
using System;

public enum VarName
{
   UNDEFINED,
   [VarName(DisplayText = "КУ цели", Persist = true, Units = "Град", DefaultValue = 180f)]
   StartBearingToTarget,
   [VarName(DisplayText = "Расстояние до цели", Persist = true, Units = "м", DefaultValue = 7000f)]
   StartDistanceToTarget,
   [VarName(DisplayText = "Использовать МПЦ", Persist = false, DefaultValue = true)]
   MPC_USE,
   [VarName(DisplayText = "Дистанция МПЦ", Persist = true, DefaultValue = 800f)]
   MPC_DISTANCE,
   [VarName(DisplayText = "Дальность обнаружения МПЦ", Persist = true, Units = "км", DefaultValue = 10f)]
   MPC_RANGE,
   [VarName(DisplayText = "Погода", Persist = true, 
      Vt = VarType.Enum, Variants = new string[] { "Штиль", "Шторм" }, DefaultValue = 0)]
   Weather,
   [VarName(DisplayText = "Количество буев", Persist = true,
      Vt = VarType.Enum, Variants = new string[] { "1", "2", "3", "4", "5", "6" }, DefaultValue = 0)]
   NumBuoys,
   [VarName(DisplayText = "Дальность обнаружения буями", Persist = true, Units = "м", DefaultValue = 5000f)]
   BuoysDetectRange,
   [VarName(DisplayText = "Дальность полета буев", Persist = true, Units = "м", DefaultValue = 5000f)]
   BuoysShootRange,
   [VarName(DisplayText = "Разность дальности полета буев", Persist = true, Units = "м", DefaultValue = 1000f)]
   BuoysShootRangeDiff,
   [VarName(DisplayText = "Расстояние между буями", Persist = true, Units = "м", DefaultValue = 1000f)]
   BuoysDistanceBetween,
   [VarName(DisplayText = "Высота открытия парашюта", Persist = false, Units = "м", DefaultValue = 200f)]
   BuoysOpenConeHeight,
   [VarName(DisplayText = "Высота начала торможения буя", Persist = true, Units = "м", DefaultValue = 50f)]
   BuoyBreakStartAltitude,
   [VarName(DisplayText = "Погрешность постановки буя", Persist = false, Units = "м", DefaultValue = 500f)]
   BuoysSettingPostionError,
   [VarName(DisplayText = "Погрешность пеленга буя", Persist = true, Units = "град", DefaultValue = 5f)]
   BuoysBearingError,
   [VarName(DisplayText = "Время готовности буя", Persist = true, Units = "сек", DefaultValue = 3f)]
   BuoyReadyTime,
   [VarName(DisplayText = "Временной сдвиг пуска ракет", Persist = true, HideInInspector = true, Units = "сек", DefaultValue = 3f)]
   RocketPauseDuration,
   [VarName(DisplayText = "Количество ракет", Persist = true, HideInInspector = true,
      Vt = VarType.Enum, Variants = new string[] { "1", "2", "3", "4", "5", "6" }, DefaultValue = 0)]
   RocketNum,
   [VarName(DisplayText = "Расстояние между ракетами при ударе", Persist = true, HideInInspector = true, Units = "м", DefaultValue = 100f)]
   RocketDistance,
   [VarName(DisplayText = "Радиус поражения ракет", Persist = true, HideInInspector = true, Units = "м", DefaultValue = 500f)]
   RocketDestroyRadius,
   [VarName(DisplayText = "Множитель погрешности обнаружения буя при шторме", Persist = true, DefaultValue = 2f)]
   BuoysBearingMultplier,
   [VarName(DisplayText = "Диаметр зеленой зоны", Persist = true, DefaultValue = 1000f)]
   GreenZoneD,
   [VarName(DisplayText = "Диаметр желтой зоны", Persist = true, DefaultValue = 1500f)]
   YellowZoneD,

   // runtime vars
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 2f)]
   CurrentTime,
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 1f)]
   TargetBearing,
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 1f)]
   TargetDistance,
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 1f)]
   TargetTCPA,
   [VarName(Persist = false, DefaultValue = "--")]
   ScenarioPhaseName,
   [VarName(Persist = false, DefaultValue = -25f, FormatPrecision = 1f)]
   TargetDetectionError,
   //[VarName(DisplayText = "Максимальная погрешность обнаружения для выстрела", Persist = true, Units = "м", DefaultValue = 600f)]
   //MaxTargetDetectionError,

   [VarName(HideInInspector = true)]
   VARSYNC_LAST
};

[Serializable]
public class VarNameSelector
{
   public string Name;
};