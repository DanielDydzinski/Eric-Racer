namespace EricRacer.UI
{
    public static class RaceText
    {
        public static string Ordinal(int place)
        {
            if (place <= 0)
                return string.Empty;
            int lastTwo = place % 100;
            if (lastTwo >= 11 && lastTwo <= 13)
                return place + "th";
            switch (place % 10)
            {
                case 1: return place + "st";
                case 2: return place + "nd";
                case 3: return place + "rd";
                default: return place + "th";
            }
        }

        public static string Time(float seconds)
        {
            int minutes = (int)(seconds / 60f);
            float rest = seconds - minutes * 60f;
            return $"{minutes}:{rest:00.00}";
        }
    }
}
