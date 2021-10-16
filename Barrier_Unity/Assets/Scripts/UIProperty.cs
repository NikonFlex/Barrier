public enum PropCategory
{
   Scenario,
   Common
}

public class UIProperty
{
   public string Title;
};

public class NumericProperty : UIProperty
{
   public NumericProperty(string t, string u)
   {
      Title = t;
      Units = u;
   }
   public string Units;
}

public class ToggleProperty : UIProperty
{
   public ToggleProperty(string t)
   {
      Title = t;
   }
}

public class DropdownProperty : UIProperty
{
   public DropdownProperty(string t, string[] options)
   {
      Title = t;
      Options = options;
   }
   public string[] Options;
}

static public class ScenarioProperties
{
   static public UIProperty[] Props = new UIProperty[]
   {
         new NumericProperty( "Расстояние до цели", "м"),
         new NumericProperty( "Пеленг на цель", "м"),
         new ToggleProperty( "МСЦ, использовать"),
         new NumericProperty( "МСЦ, дальность", "м"),
         new DropdownProperty( "Погода", new string[] { "Штиль", "Шторм" }),
         new DropdownProperty( "Количество буев", new string[] { "1", "2", "3", "4", "5", "6"}),
         new NumericProperty( "Погрешность постановки буя", "м"),
         new NumericProperty( "Погрешность пеленга буя", "град")
   };
};


static public class CommonProperties
{
   static public UIProperty[] Props = new UIProperty[]
   {
         new NumericProperty( "Точность пеленга антенны", "град"),
         new NumericProperty( "Высота раскрытия парашюта", "м")
   };
}
