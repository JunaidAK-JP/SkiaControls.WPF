using SkiaSharp;
using SkiaSharpControls.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            MyDataCollection.CollectionChanged += MyDataCollection_CollectionChanged;
            ContextMHeader = new();
            ContextMHeader.Items.Add(new MenuItem { Header = "Bilal" });
            ContextM = new();
            ContextM.Items.Add(new MenuItem { Header = "Bilal1" });
            ContextM.Items.Add(new MenuItem { Header = "Bilal2" });
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

            new() { Id = 1, Name = "Alice", Age = 25 },
            new() { Id = 2, Name = "Alice 1", Age = 26 },
            new() { Id = 3, Name = "Alice 2", Age = 27 },
            new() { Id = 4, Name = "Alice 3", Age = 28 },
            new() { Id = 5, Name = "Alice 4", Age = 29 },
            new() { Id = 6, Name = "Alice 5", Age = 30 },
            new() { Id = 7, Name = "Alice 6", Age = 35 },

            new() { Id = 1, Name = "Alice", Age = 25 },
            new() { Id = 2, Name = "Alice 1", Age = 26 },
            new() { Id = 3, Name = "Alice 2", Age = 27 },
            new() { Id = 4, Name = "Alice 3", Age = 28 },
            new() { Id = 5, Name = "Alice 4", Age = 29 },
            new() { Id = 6, Name = "Alice 5", Age = 30 },
            new() { Id = 7, Name = "Alice 6", Age = 35 },

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

        public IEnumerable<SkGridViewColumn> Columns { get; set; } =
        [
            new SkGridViewColumn() { Header = "Id" },
            new SkGridViewColumn() { Header = "Name" },
            new SkGridViewColumn() { Header = "Trend" },
            new SkGridViewColumn() { Header = "Trend2" },
            new SkGridViewColumn() { Header = "Age" },
            new SkGridViewColumn() { Header = "IsToggle" },
            new SkGridViewColumn() { Header = "Highlighted" }
        ];

        public Action<object> RowClick => (object data) =>
        {

            if (data is MyData myData)
            {
                var r= SelectedItems.Count();
                //if (SelectedItems.Count == 1)
                //{
                //    SelectedItems[0] = myData;
                //}
                //else {
                //    SelectedItems.Add(myData);
                //}
            //    if (SelectedItems.Any(x => x.Equals(data)))
            //    {
            //        SelectedItems.Remove(myData);
            //    }
            //    else
            //        SelectedItems.Add(myData);
            }
        };

        public Action<double> VerticalScrollbarChanged => (double data) =>
        {

           
        };
        public Action<double> HorizontalScrollbarChanged => (double data) =>
        {


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
                _ => SKColors.Black,
            };
        };

        public Func<object, SKColor> RowBorderSelector => row =>
        {
            MyData? myObject = row as MyData;

            return myObject?.Age switch
            {
                28 => SKColors.Green,
                29 => SKColors.Green,
                30 => SKColors.Green,
                _ => SKColors.Transparent
            };
        };

        public static SKFont font = new SKFont() { Size = 11, Typeface = SKTypeface.FromFamilyName("Arial") };
        public static SKPaint TextForeground = new SKPaint { Color = SKColors.White, IsAntialias = true };
        public static SKPaint LineBackground = new SKPaint { Color = SKColors.Red, StrokeWidth = 1 };

        private static readonly SKPaint RedPaint = new SKPaint
        {
            Color = new SKColor(0xFF, 0x00, 0x00),
            StrokeWidth = 1,
            IsAntialias = true
        };

        private static readonly SKPaint GreenPaint = new SKPaint
        {
            Color = new SKColor(0x00, 0xFF, 0x00),
            StrokeWidth = 1,
            IsAntialias = true
        };
        private static readonly SKPaint BackgroundBrushHighlighting = new SKPaint { Color = SKColors.LightGreen, StrokeWidth = 1, IsAntialias = true };
        private static readonly SKPaint BorderBrushHighlighting = new SKPaint { Color = SKColors.Blue, StrokeWidth = 1, IsAntialias = true };

        public Func<object, string, SkCellTemplate> CellTemplateSelector { get; set; } = (row, column) =>
            {
                SkCellTemplate template = new();

                if (row is MyData myData)
                {
                    template.RendererProperties = new SkRendererProperties()
                    {
                        TextFont = font,
                        TextForeground = TextForeground,
                        LineBackground = LineBackground
                    };

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

                        case "Trend":
                            template.CustomDrawing = new Action<SKCanvas, float, float>((canvas, x, y) =>
                            {
                                x += 5;
                                y += 2;
                                var rect = new SKRect();
                                for (int i = 0; i < 10; i++)
                                {
                                    rect.Left = x;
                                    rect.Top = y;
                                    rect.Right = x + 10;
                                    rect.Bottom = y + 12;

                                    canvas.DrawRect(rect, i % 2 == 0 ? RedPaint : GreenPaint);
                                    x += 10;
                                }
                                //for (int i = 0; i < 10; i++)
                                //{
                                //    var rect = SKRect.Create(x, y, 10, 12);
                                //    canvas.DrawRect(rect, new SKPaint() { Color = SKColor.Parse(i % 2 == 0 ? "#FF0000" : "#00FF00"), StrokeWidth = 1, IsAntialias = true });
                                //    x += 10;
                                //}
                            });
                            break;

                        case "Trend2":
                            template.CustomDrawing = new Action<SKCanvas, float, float>((canvas, x, y) =>
                            {
                                // Initial adjustments
                                float initialX = x;
                                x += 5;
                                y += 2;

                                // Precreate SKPaint objects
                                var redPaint = new SKPaint { Color = new SKColor(0xFF, 0x00, 0x00), StrokeWidth = 1, IsAntialias = true };
                                var greenPaint = new SKPaint { Color = new SKColor(0x00, 0xFF, 0x00), StrokeWidth = 1, IsAntialias = true };

                                // Loop 1 (Red and Green rectangles)
                                for (int i = 0; i < 10; i++)
                                {
                                    var rect = new SKRect(x, y, x + 10, y + 6);
                                    canvas.DrawRect(rect, i % 2 == 0 ? redPaint : greenPaint);
                                    x += 10;
                                }

                                // Reset x and adjust y for second row
                                x = initialX + 5;
                                y += 6;

                                // Loop 2 (Green and Red rectangles)
                                for (int i = 0; i < 10; i++)
                                {
                                    var rect = new SKRect(x, y, x + 10, y + 6);
                                    canvas.DrawRect(rect, i % 2 != 0 ? redPaint : greenPaint);
                                    x += 10;
                                }
                            });
                            //template.CustomDrawing = new Action<SKCanvas, float, float>((canvas, x, y) =>
                            //{
                            //    float initialX = x;
                            //    x += 5;
                            //    y += 2;

                            //    for (int i = 0; i < 10; i++)
                            //    {
                            //        var rect = SKRect.Create(x, y, 10, 6);
                            //        canvas.DrawRect(rect, new SKPaint() { Color = SKColor.Parse(i % 2 == 0 ? "#FF0000" : "#00FF00"), StrokeWidth = 1, IsAntialias = true });
                            //        x += 10;
                            //    }

                            //    x = initialX + 5;
                            //    y += 6;

                            //    for (int i = 0; i < 10; i++)
                            //    {
                            //        var rect = SKRect.Create(x, y, 10, 6);
                            //        canvas.DrawRect(rect, new SKPaint() { Color = SKColor.Parse(i % 2 != 0 ? "#FF0000" : "#00FF00"), StrokeWidth = 1, IsAntialias = true });
                            //        x += 10;
                            //    }
                            //});
                            break;

                        case "Highlighted":
                            template.RendererProperties.BackgroundBrush = BackgroundBrushHighlighting;
                            template.CellContent = "hi11";
                            template.RendererProperties.BorderBrush = BorderBrushHighlighting;
                            template.CellContentAlignment = SkiaSharpControls.Enum.CellContentAlignment.Center;
                            break;
                    }
                }

                return template;
            };

        private bool showGridLines;

        public bool ShowGridLines
        {
            get { return showGridLines; }
            set
            {
                showGridLines = value;
                OnPropertyChanged(nameof(ShowGridLines));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ContextMenu ContextMHeader { get; set; }
        public ContextMenu ContextM { get; set; }
        public ObservableCollection<object> SelectedItems { get; set; } = new();
    }
}