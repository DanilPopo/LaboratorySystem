using System.Windows;
using System.Windows.Controls;

namespace LaboratorySystem
{
    public partial class ResearchLabAssistantWindow : UserWindow
    {
        public ResearchLabAssistantWindow(int userId) : base(userId)
        {
            InitializeComponent();
            LoadMenu();
        }

        private void LoadMenu()
        {
            StackPanel menuPanel = new StackPanel();
            menuPanel.Orientation = Orientation.Vertical;
            menuPanel.Margin = new Thickness(10);

            Button btnWorkWithAnalyzer = new Button();
            btnWorkWithAnalyzer.Content = "Работа с анализатором";
            btnWorkWithAnalyzer.Style = (Style)FindResource("MenuButtonStyle");
            btnWorkWithAnalyzer.Click += (s, e) => { ShowStatusMessage("Работа с анализатором"); };
            menuPanel.Children.Add(btnWorkWithAnalyzer);

            Grid.SetRow(menuPanel, 1);
            ((Grid)this.Content).Children.Add(menuPanel);
        }
    }
}
