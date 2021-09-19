namespace AmeisenBotX.Core.Logic.Routines
{
    public static class TrainAllSpellsRoutine
    {
        public static void Run(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            // this can fail for myriad of reasons like not having enough money to buy service, or npc getting rekt/trainerFrame bugging out
            // this basically assumes unlimited cash supply and stable trainer frame open while executing
            bot.Wow.ClickOnTrainButton();
        }
    }
}