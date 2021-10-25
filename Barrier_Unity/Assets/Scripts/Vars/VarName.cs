using UnityEngine;

public enum VarName
{
   // ===============================
   // ДОБАВЛЯТЬ В КОНЕЦ!!!!!!!!!!!!!!
   // ===============================
   [VarName(HideInInspector = true)]
   UNDEFINED,
   [VarName(DisplayText = "Пеленг на цель", Persist = true, HideInInspector = true, Units = "Град", DefaultValue = 180f)]
   StartBearingToTarget,
   [VarName(DisplayText = "Использовать МСЦ", Persist = true, HideInInspector = true, DefaultValue = true)]
   MSC_USE,
   [VarName(DisplayText = "Дистанция МСЦ", Persist = true, HideInInspector = true, DefaultValue = 800f)]
   MSC_DISTANCE,
   [VarName(DisplayText = "Погода", Persist = true, HideInInspector = true, 
      Vt = VarType.Enum, Variants = new string[] { "Штиль", "Шторм" })]
   Weather,
   [VarName(DisplayText = "Количество буев", Persist = true, HideInInspector = true,
      Vt = VarType.Enum, Variants = new string[] { "1", "2", "3", "4", "5", "6" })]
   NumBuoys,
   [VarName(DisplayText = "Дальность полета буев", Persist = true, HideInInspector = true, Units = "м", DefaultValue = 5000)]
   BuoysShootRange,
   [VarName(DisplayText = "Расстояние между буями", Persist = true, HideInInspector = true, Units = "м", DefaultValue = 1000)]
   BouysDistanceBetween,
   [VarName(DisplayText = "Высота открытия парашюта", Persist = true, HideInInspector = true, Units = "м", DefaultValue = 200)]
   BuoysOpenConeHeight,
   [VarName(DisplayText = "Погрешность постановки буя", Persist = true, HideInInspector = true, Units = "м", DefaultValue = 500)]
   BuoysSettingPostionError,
   [VarName(DisplayText = "Погрешность пеленга буя", Persist = true, HideInInspector = true, Units = "град", DefaultValue = 10)]
   BuoysBearingError,
   [VarName(DisplayText = "Время готовности буя", Persist = true, HideInInspector = true, Units = "сек", DefaultValue = 3)]
   BuoyReadyTime,

   // runtime vars
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 2)]
   CurrentTime,
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 1)]
   TargetBearing,
   [VarName(Persist = false, DefaultValue = 0f, FormatPrecision = 1)]
   TargetDistance,
   [VarName(Persist = false, DefaultValue = "")]
   ScenarioPhaseName,

   // TODO: перенести наверх, когда будет сделано https://bitbucket.org/blurman/barrier/issues/11
   [VarName(DisplayText = "Расстояние до цели", Persist = true, HideInInspector = true, Units = "м", DefaultValue = 7000f)]
   StartDistanceToTarget,

   VARSYNC_LAST
};
