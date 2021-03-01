using AmeisenBotX.Core;
using AmeisenBotX.Core.Personality.Objects;
using System;
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

        public RelationshipDisplay(WowInterface wowInterface, ulong guid, Relationship relationship)
        {
            WowInterface = wowInterface;
            Guid = guid;
            Relationship = relationship;

            InitializeComponent();
        }

        private ulong Guid { get; }

        private Relationship Relationship { get; }

        private WowInterface WowInterface { get; }

        public void Update(ulong guid, Relationship relationship)
        {
            if (WowInterface.Db.TryGetUnitName(guid, out string name))
            {
                labelName.Content = name;
            }
            else
            {
                labelName.Content = "unknown";
            }

            labelStatus.Content = relationship.Level;
            labelLastSeen.Content = relationship.LastSeen;

            double leftMargin = MIN_MARGIN + ((Math.Min(Math.Max(relationship.Score, MIN_REL), MAX_REL) + Math.Abs(MIN_REL)) * relationshipToScale);
            rectIndicator.Margin = new(leftMargin, rectIndicator.Margin.Top, rectIndicator.Margin.Right, rectIndicator.Margin.Bottom);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Update(Guid, Relationship);
        }
    }
}