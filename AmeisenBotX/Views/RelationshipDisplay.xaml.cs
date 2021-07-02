using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Objects;
using System.Windows;
using System.Windows.Controls;

namespace AmeisenBotX.Views
{
    public partial class RelationshipDisplay : UserControl
    {
        private const int MAX_MARGIN = 324;
        private const int MAX_REL = 5;
        private const int MIN_MARGIN = 6;
        private const int MIN_REL = -4;
        private readonly float relationshipToScale = (MAX_MARGIN - MIN_MARGIN) / (MAX_REL - MIN_REL);

        public RelationshipDisplay(AmeisenBotInterfaces wowInterface, ulong guid, int relationship)
        {
            Bot = wowInterface;
            Guid = guid;
            Relationship = relationship;

            InitializeComponent();
        }

        private AmeisenBotInterfaces Bot { get; }

        private ulong Guid { get; }

        private int Relationship { get; }

        public void Update(ulong guid, int relationship)
        {
            if (Bot.Db.GetUnitName(Bot.GetWowObjectByGuid<WowUnit>(guid), out string name))
            {
                labelName.Content = name;
            }
            else
            {
                labelName.Content = "unknown";
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Update(Guid, Relationship);
        }
    }
}