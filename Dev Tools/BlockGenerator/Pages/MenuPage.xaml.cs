using System.Windows;
using System.Windows.Controls;

namespace BlockGenerator.Pages
{
    /// <summary>
    /// Interaction logic for MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Page
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        private async void DetailBlock_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new DetailBlockPage() );
        }

        private async void TypeScriptViewModelsButton_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new TypeScriptViewModelsPage() );
        }
    }
}
