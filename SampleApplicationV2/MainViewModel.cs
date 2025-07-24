
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;


namespace SampleApplicationV2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MyData> Items { get; set; }

        public MainViewModel()
        {
            Columns = new Columns();

            Items = new ObservableCollection<MyData>(RandomDataGenerator.Generate(1));
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += (s, e) =>
            {

                foreach (var item in RandomDataGenerator.Generate(50))
                {
                    if (Items.Count >= 150)
                    {
                       // Items.RemoveAt(Items.Count - 1);
                    }
                    item.Description = null;
                    Items.Insert(0, item);
                }
                
            };
            timer.Start();
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
                item.Name = "Item 1";
                Items.Insert(0, item);
            }
            
        });

        public ICommand UpdateItemsValues => new RelayCommand(() =>
        {
            var item = RandomDataGenerator.Generate(100);
            Random rand = new();
            foreach (var items in Items.Where(x=>x.Name == "Item 1"))
            {
                var randomnumber = rand.Next(1, 100);
                items.Id = item[randomnumber].Id;
                items.Price = item[randomnumber].Price;
                items.Rating = item[randomnumber].Rating;
                items.CreatedAt = item[randomnumber].CreatedAt;
                items.Description = item[randomnumber].Description;
                items.Quantity = item[randomnumber].Quantity;
                items.Discount = item[randomnumber].Discount;
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
