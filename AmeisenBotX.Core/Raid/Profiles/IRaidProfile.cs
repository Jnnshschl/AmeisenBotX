namespace AmeisenBotX.Core.Jobs.Profiles
{
    public interface IRaidProfile
    {
        public string Author { get; }

        public string Description { get; }

        public string Name { get; }
    }
}