namespace AmeisenBotX.Utils
{
    public class IdleActionWrapper
    {
        public bool IsEnabled { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}