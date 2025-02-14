using System.Windows;
using Gwent_Release.Models;
using Gwent_Release.ViewModels;

namespace Gwent_Release.Views
{
    /// <summary>
    /// Логика взаимодействия для Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();

            MenuViewModel menuViewModel = new MenuViewModel();

            DataContext = menuViewModel;
        }
    }
}
