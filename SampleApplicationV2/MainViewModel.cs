
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;


namespace SampleApplicationV2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MyData> Items { get; set; }

        public MainViewModel()
        {
            Columns = new Columns();

            Items = new ObservableCollection<MyData>(RandomDataGenerator.Generate(100));

           
        }
     


        public Columns Columns { get; set; }

        #region Commands
        public ICommand ChangeColumns1 => new RelayCommand(() =>
        {
            Columns.Price.BackColor = null;
            Columns.Name.IsVisible = true;
            Columns.Quantity.IsVisible = false;
            Columns.Id.IsVisible = false;
        });
        public ICommand ChangeColumns2 => new RelayCommand(() =>
        {
            Columns.Id.IsVisible = true;
            Columns.Price.BackColor = "Red";
            Columns.IsActive.IsVisible = false;
            Columns.IsDeleted.IsVisible = false;
            Columns.UpdatedAt.IsVisible = false;
            Columns.Category.IsVisible = false;
            Columns.Category.IsVisible = false;
            Columns.Id.Width = 50;

        });

        public ICommand ChangeColumns3 => new RelayCommand(() =>
        {
            Columns = new();
            
            OnPropertyChanged(nameof(Columns));

        });
        public ICommand InsertNewItems => new RelayCommand(() =>
        {
            foreach (var item in RandomDataGenerator.Generate(5))
            {
                Items.Insert(0, item);
            }
            

        });
        #endregion 


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
