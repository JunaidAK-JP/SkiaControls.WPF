﻿using SkiaSharp;
using SkiaSharpControls.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace WpfApp1
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            MyDataCollection.CollectionChanged += MyDataCollection_CollectionChanged;
        }

        private ObservableCollection<MyData> myDataCollection =
        [
            new() { Id = 1, Name = "Alice", Age = 25 },
            new() { Id = 2, Name = "Alice 1", Age = 26 },
            new() { Id = 3, Name = "Alice 2", Age = 27 },
            new() { Id = 4, Name = "Alice 3", Age = 28 },
            new() { Id = 5, Name = "Alice 4", Age = 29 },
            new() { Id = 6, Name = "Alice 5", Age = 30 },
            new() { Id = 7, Name = "Alice 6", Age = 35 },
        ];

        public ObservableCollection<MyData> MyDataCollection
        {
            get { return myDataCollection; }
            set
            {
                myDataCollection = value;
                OnPropertyChanged(nameof(MyDataCollection));
            }
        }

        private void MyDataCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.RefreshSkia();
        }

        public IEnumerable<SKGridViewColumn> Columns { get; set; } =
        [
            new SKGridViewColumn() { Header = "Id" },
            new SKGridViewColumn() { Header = "Name" },
            new SKGridViewColumn() { Header = "Age" },
            new SKGridViewColumn() { Header = "IsToggle" }
        ];

        public Action<object> RowClick => (object data) =>
        {
            if (data is MyData myData)
            {
                //MessageBox.Show(myData.ToString());
            }
        };

        public Action<object, string> CellClick => (object data, string header) =>
        {
            if (data is MyData myData)
            {
                //MessageBox.Show(myData.ToString());
                //MessageBox.Show(header);

                if (header == "IsToggle")
                {
                    var index = MyDataCollection.IndexOf(myData);
                    myData.IsToggledOn = !myData.IsToggledOn;
                    (Application.Current.MainWindow as MainWindow)?.RefreshSkia();
                }
            }
        };

        public Func<object, SKColor> RowBackgroundSelector => row =>
        {
            MyData? myObject = row as MyData;

            return myObject?.Age switch
            {
                25 => SKColors.Blue,
                26 => SKColors.LightGreen,
                27 => SKColors.Pink,
                28 => SKColors.Orange,
                29 => SKColors.Yellow,
                30 => SKColors.Green,
                35 => SKColors.Red,
                _ => SKColors.White,
            };
        };

        public Func<object, string, SkCellTemplate> CellTemplateSelector { get; set; } = (row, column) =>
        {
            SkCellTemplate template = new();

            if (row is MyData myData)
            {
                switch (column)
                {
                    case "IsToggle":
                        template.IsToggleButton = true;
                        template.IsToggleButtonOn = myData.IsToggledOn;
                        break;

                    case "Id":
                        template.CellContent = myData.Id.ToString();
                        break;

                    case "Name":
                        template.CellContent = myData.Name;
                        break;

                    case "Age":
                        template.CellContent = myData.Age.ToString();
                        break;
                }
            }

            return template;
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}