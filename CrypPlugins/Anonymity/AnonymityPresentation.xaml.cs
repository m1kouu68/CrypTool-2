using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.Security.Cryptography;

namespace CrypTool.Plugins.Anonymity
{
    /// <summary>
    /// Interaction logic for AnonymityPresentation.xaml
    /// </summary>
    public partial class AnonymityPresentation : UserControl
    {
        public delegate void DataTableChangedEventHandler(object sender, EventArgs e);
        public event DataTableChangedEventHandler DataTableChanged;

        private List<ComboBox> hiddenComboboxes = new List<ComboBox>();
        private List<ComboBox> labelComboboxes = new List<ComboBox>();
        private bool isDragging = false;
        private Point startPoint;
        private DataTable initialTable;
        private List<Button> _buttonList = new List<Button>();
        private List<string> _headerList = new List<string>();
        private DataTable dt;
        private string[] headers;
        private ViewModel view;
        private List<Label> identMessageLabels = new List<Label>();




        public AnonymityPresentation()
        {
            InitializeComponent();
            this.view = new ViewModel();
            this.DataContext = this.view;
        }


        // clear presentation, remove all elements in the class lists and stackpanels
        public void ClearPresentation()
        {
            hiddenComboboxes.Clear();
            labelComboboxes.Clear();
            headerlabels.Children.Clear();
            _buttonList.Clear();
            foreach (Button btn in _buttonList)
            {
                btn.IsEnabled = true;
            }
            foreach (StackPanel childStackPanel in functionContainer.Children.OfType<StackPanel>())
            {
                childStackPanel.Children.Clear();
            }
            functionContainer.Children.Clear();
            foreach (StackPanel childStackPanel in numericContainer.Children.OfType<StackPanel>())
            {
                childStackPanel.Children.Clear();
            }
            numericContainer.Children.Clear();
        }

        // create the datatable in the presentation
        public void CreateDataTable(string InputCsv, string RowSeperator, string ColumnSeperator)
        {

            dt = new DataTable();

            // split the input string into an array of lines
            InputCsv = InputCsv.Replace(" ", "");
            string[] lines = InputCsv.Split(new[] { "\r\n", RowSeperator }, StringSplitOptions.RemoveEmptyEntries);


            // split the first line into an array of headers
            headers = lines[0].Split(ColumnSeperator.ToArray(), StringSplitOptions.RemoveEmptyEntries);




            // add columns to the table
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }


            /* create comboboxes containing Identifier, Quasi-Identifier and Sensitive Attribute values
             * create comboboxes containing Numeric and Categoric values
             * create message that identifiers are removed from the table
             */

            for (int i = 0; i < headers.Length; i++)
            {
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                Label label = new Label() { Content = headers[i] };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                ComboBox box1 = new ComboBox();
                box1.Items.Add("Identifier");
                box1.Items.Add("Quasi-Identifier");
                box1.Items.Add("Sensitive Attribute");
                box1.SelectedValue = "Quasi-Identifier";
                box1.Margin = new Thickness(5);
                box1.SelectionChanged += ComboBoxSelectionQuasiIdentifier;
                box1.SelectionChanged += HandleItemCreation;
                Grid.SetRow(box1, 1);
                Grid.SetColumn(box1, 0);
                grid.Children.Add(box1);
                labelComboboxes.Add(box1);
                ComboBox box2 = new ComboBox();
                box2.Items.Add("Categoric");
                box2.Items.Add("Numeric");
                Label idMessage = new Label();
                idMessage.Content = "Identifier is removed from table";
                idMessage.Visibility = Visibility.Collapsed;
                identMessageLabels.Add(idMessage);
                box2.Margin = new Thickness(5);
                box2.IsEnabled = false;
                Grid.SetRow(box2, 1);
                Grid.SetColumn(box2, 1);
                Grid.SetRow(idMessage, 1);
                Grid.SetColumn(idMessage, 1);
                grid.Children.Add(box2);
                grid.Children.Add(idMessage);
                hiddenComboboxes.Add(box2);
                headerlabels.Children.Add(grid);
            }
            // add data rows to the table
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(ColumnSeperator.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                dt.Rows.Add(values);

            }
            // set default data type values for comboboxes
            SetDataTypeForColumns(hiddenComboboxes, dt);
            // datatable bind to datagrid in presentation
            table.ItemsSource = dt.DefaultView;
            table.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            table.CanUserSortColumns = false;
            table.CanUserAddRows = false;
            table.CanUserReorderColumns = false;
            table.CanUserResizeColumns = false;
            table.CanUserAddRows = false;
            table.CanUserDeleteRows = false;

            //initial state of table is copied
            initialTable = dt.Copy();
            GenerateNumericItems();
            GenerateCategoricItems();
        }




        // data table changed event
        public virtual void OnDataTableChanged()
        {
            DataTableChanged?.Invoke(this, EventArgs.Empty);
        }



        // make the columns GroupID, IsGroupStart, IsGroupEnd always invisible
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "GroupID")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
            else if (e.PropertyName == "IsGroupStart")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
            else if (e.PropertyName == "IsGroupEnd")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }


        //  remove / generate categoric and numeric items after combobox value is selected by user  
        private void HandleItemCreation(object sender, SelectionChangedEventArgs e)
        {

            ComboBox combo = sender as ComboBox;
            DataTable init = initialTable.Copy();
            var rows = dt.AsEnumerable().ToList();
            var initialRows = init.AsEnumerable().ToList();
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i] == combo && combo.SelectedItem.ToString() != "Quasi-Identifier" && hiddenComboboxes[i].SelectedItem.ToString() == "Categoric")
                {
                    for (int j = 0; j < rows.Count; j++)
                    {
                        rows[j][i] = initialRows[j][i];
                    }

                    SpecificColumnVisibility(i);
                    RemoveSpecificCategoricItem(headers[i]);
                    CheckSelectedTab(tabSelec);

                    if(combo.SelectedItem.ToString() == "Identifier")
                    {
                        CenterColumnData(i);
                    }



            
                }
                else if (labelComboboxes[i] == combo && combo.SelectedItem.ToString() != "Quasi-Identifier" && hiddenComboboxes[i].SelectedItem.ToString() == "Numeric")
                {
                    for (int j = 0; j < rows.Count; j++)
                    {
                        rows[j][i] = initialRows[j][i];
                    }
                    SpecificColumnVisibility(i);
                    RemoveSpecificNumericItem(headers[i]);
                    CheckSelectedTab(tabSelec);


                    if (combo.SelectedItem.ToString() == "Identifier")
                    {
                        CenterColumnData(i);
                    }


                }
                else if (labelComboboxes[i] == combo && combo.SelectedItem.ToString() == "Quasi-Identifier" && hiddenComboboxes[i].SelectedItem.ToString() == "Categoric")
                {
                    SpecificColumnVisibility(i);
                    GenerateSpecificCategoricItem(i);
                    CheckSelectedTab(tabSelec);
                }
                else if (labelComboboxes[i] == combo && combo.SelectedItem.ToString() == "Quasi-Identifier" && hiddenComboboxes[i].SelectedItem.ToString() == "Numeric")
                {
                    SpecificColumnVisibility(i);
                    GenerateSpecificNumericItem(i);
                    CheckSelectedTab(tabSelec);
                }


            }

        }

        // set default data type values for comboboxes to determine if they are categoric or numeric
        public void SetDataTypeForColumns(List<ComboBox> hiddenComboboxes, DataTable dt)
        {
            for (int index = 0; index < hiddenComboboxes.Count; index++)
            {
                List<string> columnValues = new List<string>();


                for (int row = 1; row < dt.Rows.Count; row++)
                {
                    string value = dt.Rows[row][index].ToString();
                    columnValues.Add(value);
                }
                bool isNumeric = columnValues.All(value => double.TryParse(value, out _));
                string defaultValue = isNumeric ? "Numeric" : "Categoric";
                hiddenComboboxes[index].SelectedItem = defaultValue;
            }
        }




        // if combobox next to the column headers is set to Quasi-Identifier, the comboboxes with the items categoric and numeric appear
        private void ComboBoxSelectionQuasiIdentifier(object sender, SelectionChangedEventArgs e)
        {

            ComboBox combobox = sender as ComboBox;
            int selectedIndex = labelComboboxes.IndexOf(combobox);
            if (combobox.SelectedItem != null && combobox.SelectedItem.ToString() == "Quasi-Identifier")
            {
                identMessageLabels[selectedIndex].Visibility = Visibility.Hidden;
                hiddenComboboxes[selectedIndex].Visibility = Visibility.Visible;

            }
            else if (combobox.SelectedItem != null && combobox.SelectedItem.ToString() == "Identifier")
            {

                identMessageLabels[selectedIndex].Visibility = Visibility.Visible;
                hiddenComboboxes[selectedIndex].Visibility = Visibility.Hidden;

            }

            else
            {
                identMessageLabels[selectedIndex].Visibility = Visibility.Hidden;
                hiddenComboboxes[selectedIndex].Visibility = Visibility.Hidden;
            }
        }




        // numeric items created, according to index of hiddencomboboxes the corresponding column values are used
        private void GenerateNumericItems()
        {

            for (int i = 0; i < hiddenComboboxes.Count(); i++)
            {


                if (hiddenComboboxes[i].SelectedItem != null && hiddenComboboxes[i].SelectedItem.ToString() == "Numeric" && hiddenComboboxes[i].Visibility == Visibility.Visible)
                {
                    int index = i;


                    // get the distinct values of column and order ascending
                    List<string> distinctValues = table.Items.Cast<DataRowView>()
                                        .Select(row => row.Row.ItemArray[i].ToString())
                                        .Distinct().ToList();


                    var canvas = new Canvas();
                    canvas.Margin = new Thickness(0, 1, 0, 0);
                    var margin = 5;
                    canvas.Background = Brushes.Gray;
                    canvas.Height = 55;
                    canvas.HorizontalAlignment = HorizontalAlignment.Left;
                    canvas.VerticalAlignment = VerticalAlignment.Top;

                    distinctValues.Sort((a, b) => int.Parse(a).CompareTo(int.Parse(b)));

                    var textblock = new TextBlock();
                    textblock.FontWeight = FontWeights.DemiBold;
                    textblock.FontSize = 14.0;
                    textblock.Foreground = Brushes.Black;
                    textblock.Margin = new Thickness(0, 10, 10, 10);



                    textblock.Text = dt.Columns[i].ColumnName;
                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    numericContainer.Children.Add(textblock);


                    // values in the column are added to the canvas
                    for (int j = 0; j < distinctValues.Count; j++)
                    {

                        var col = distinctValues[j];
                        var numTextblock = new TextBlock();
                        numTextblock.FontWeight = FontWeights.DemiBold;
                        numTextblock.FontSize = 14.0;
                        numTextblock.Foreground = Brushes.Black;

                        numTextblock.VerticalAlignment = VerticalAlignment.Center;
                        numTextblock.Text = col;
                        numTextblock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        double textBlockHeight = numTextblock.DesiredSize.Height;
                        double topMargin = (canvas.Height - textBlockHeight) / 2;
                        Canvas.SetLeft(numTextblock, margin + 15);
                        Canvas.SetTop(numTextblock, topMargin);
                        canvas.Children.Add(numTextblock);
                        margin += 50;

                        if (j != distinctValues.Count - 1)
                        {
                            var rectangle = new Rectangle
                            {
                                Width = 3,
                                Height = canvas.Height,
                                Fill = Brushes.Black,

                            };


                            rectangle.MouseLeftButtonDown += (sender, e) => Rect_MouseLeftButtonDown(sender, e, canvas, index);
                            Canvas.SetLeft(rectangle, margin + 10);
                            canvas.Children.Add(rectangle);
                            margin += 5;



                        }
                    }

                    canvas.Width = margin + 30;
                    var scrollViewer = new ScrollViewer();
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.Margin = new Thickness(0, 0, 0, 10);
                    stackPanel.Children.Add(canvas);
                    stackPanel.Children.Add(new ListView { VerticalAlignment = VerticalAlignment.Center });

                    // inverse button
                    var btn = new Button { Content = "Inverse", Background = Brushes.SkyBlue, FontSize = 14, Margin = new Thickness(10, 0, 0, 0), Foreground = Brushes.White, Width = 60, Height = 30 };
                    btn.Click += (sender, e) => InverseGrouping(canvas, index);

                    stackPanel.Children.Add(btn);
                    scrollViewer.Content = stackPanel;
                    numericContainer.Children.Add(scrollViewer);

                }
            }


        }

        private void InverseGrouping(Canvas canvas, int columnIndex)
        {
            // Invert the color of rectangles
            foreach (UIElement child in canvas.Children)
            {
                if (child is Rectangle rectangle)
                {
                    if (rectangle.Fill == Brushes.Black)
                    {
                        rectangle.Fill = Brushes.LightGray;
                    }
                    else if (rectangle.Fill == Brushes.LightGray)
                    {
                        rectangle.Fill = Brushes.Black;
                    }
                }
            }

            HandleGrouping(null, canvas, columnIndex);
        }








        // rectangle mousleftbuttondown event handler
        private void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e, Canvas canvas, int columnIndex)
        {
            var rect = sender as Rectangle;


            HandleRectangleColoring(rect);
            HandleGrouping(rect, canvas, columnIndex);

        }




        /* handle rectangle coloring
         * 
         */

        private void HandleRectangleColoring(Rectangle rect)
        {
            if (rect.Fill == Brushes.LightGray)
            {
                rect.Fill = Brushes.Black;
            }
            else
            {
                rect.Fill = Brushes.LightGray;
            }
        }



        /* grouping for numeric items
         *
         * 
         * 
         * 
         */


        private void HandleGrouping(Rectangle clickedRect, Canvas canvas, int columnIndex)
        {
            DataTable init = initialTable.Copy();
            var rows = dt.AsEnumerable().ToList();
            var initialRows = init.AsEnumerable().ToList();


            for (int i = 0; i < rows.Count; i++)
            {
                rows[i][columnIndex] = initialRows[i][columnIndex];
            }


            int lowerBound = int.MaxValue;
            int upperBound = int.MinValue;


            bool inGroup = false;
            TextBlock prevTextBlock = null;

            foreach (UIElement child in canvas.Children)
            {
                if (child is TextBlock textBlock)
                {
                    prevTextBlock = textBlock;
                }
                else if (child is Rectangle rectangle)
                {
                    if (rectangle.Fill == Brushes.LightGray)
                    {
                        inGroup = true;
                        if (prevTextBlock != null)
                        {
                            int value = int.Parse(prevTextBlock.Text);
                            lowerBound = Math.Min(lowerBound, value);
                            upperBound = Math.Max(upperBound, value);
                        }
                    }
                    else
                    {
                        if (inGroup && prevTextBlock != null)
                        {

                            int value = int.Parse(prevTextBlock.Text);
                            upperBound = Math.Max(upperBound, value);


                            foreach (var row in rows)
                            {
                                int cellValue;
                                if (int.TryParse(row[columnIndex].ToString(), out cellValue)) 
                                {
                                    if (cellValue >= lowerBound && cellValue <= upperBound)
                                    {
                                        row[columnIndex] = $"[{lowerBound} - {upperBound}]"; 
                                    }
                                }
                            }


                            inGroup = false;
                            lowerBound = int.MaxValue;
                            upperBound = int.MinValue;
                        }
                    }
                }
            }


            if (inGroup && prevTextBlock != null)
            {

                int value = int.Parse(prevTextBlock.Text);
                upperBound = Math.Max(upperBound, value);


                foreach (var row in rows)
                {
                    int cellValue;
                    if (int.TryParse(row[columnIndex].ToString(), out cellValue))
                    {
                        if (cellValue >= lowerBound && cellValue <= upperBound)
                        {
                            row[columnIndex] = $"[{lowerBound} - {upperBound}]";
                        }
                    }
                }
            }

            CalculateKValue();

            CheckSelectedTab(tabSelec);
        }










        // categoric items are created
        private void GenerateCategoricItems()
        {
            for (int i = 0; i < hiddenComboboxes.Count(); i++)
            {
                if (hiddenComboboxes[i].SelectedItem != null && hiddenComboboxes[i].SelectedItem.ToString() == "Categoric" && hiddenComboboxes[i].Visibility == Visibility.Visible)
                {
                    int index = i;

                    // distinct values of the corresponding column in the table are taken
                    List<string> distinctValues = dt.AsEnumerable()
                            .Select(row => row.Field<string>(i))
                            .Distinct().ToList();

                    var canvas = new Canvas();
                    double currentLeft = 0.0;
                    canvas.Background = Brushes.Gray;
                    canvas.Height = 55;
                    canvas.HorizontalAlignment = HorizontalAlignment.Left;
                    canvas.VerticalAlignment = VerticalAlignment.Top;
                    var textblock = new TextBlock();
                    textblock.FontWeight = FontWeights.DemiBold;
                    textblock.FontSize = 14.0;
                    textblock.Foreground = Brushes.Black;
                    textblock.Margin = new Thickness(0, 10, 10, 10);

                    textblock.Text = dt.Columns[i].ColumnName;

                    // canvas is filled with the distinct values of the column
                    for (int j = 0; j < distinctValues.Count; j++)
                    {
                        var col = distinctValues[j];

                        var border = new Border();
                        var converter = new BrushConverter();
                        var brush = (Brush)converter.ConvertFromString("#3F48CC");
                        border.Background = brush;
                        border.Tag = col;
                        border.Cursor = Cursors.Hand;
                        border.CornerRadius = new CornerRadius(5);
                        border.Padding = new Thickness(10);

                        var textBlockInside = new TextBlock() { Text = col, Foreground = Brushes.White, FontSize = 14 };
                        textBlockInside.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                        double textWidth = textBlockInside.DesiredSize.Width;

                        border.Child = textBlockInside;
                        border.Width = textWidth + 20;

                        border.MouseLeftButtonDown += (sender, e) =>
                        {
                            var element = sender as UIElement;
                            if (element != null)
                            {
                                element.CaptureMouse();
                                isDragging = true;
                                startPoint = e.GetPosition(canvas);
                            }
                        };

                        border.MouseMove += (sender, e) =>
                        {
                            if (isDragging)
                            {
                                Point currentPosition = e.GetPosition(canvas);
                                // calculate change in position
                                double x = currentPosition.X - startPoint.X;
                                double y = currentPosition.Y - startPoint.Y;

                                double newX = Canvas.GetLeft(border) + x;
                                double newY = Canvas.GetTop(border) + y;
                                // if new position is within bounds, update position
                                if (newX >= 0 && newX <= canvas.ActualWidth - border.ActualWidth && newY >= 0 && newY <= canvas.ActualHeight - border.ActualHeight)
                                {

                                    Canvas.SetLeft(border, newX);
                                    Canvas.SetTop(border, newY);

                                    startPoint = currentPosition;
                                }
                            }
                        };

                        border.MouseLeftButtonUp += (sender, e) =>
                        {
                            var element = sender as UIElement;
                            if (element != null)
                            {
                                element.ReleaseMouseCapture();
                                isDragging = false;
                            }

                            UpdateCategoricColumn(canvas, index);
                        };

                        Canvas.SetLeft(border, currentLeft);
                        Canvas.SetTop(border, 0.0);
                        currentLeft += border.Width;


                        if (j < distinctValues.Count - 1)
                        {
                            currentLeft += 20;
                        }

                        canvas.Children.Add(border);
                    }

                    canvas.Width = currentLeft;

                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    var scrollViewer = new ScrollViewer();
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.Margin = new Thickness(0, 0, 0, 10);
                    stackPanel.Children.Add(canvas);
                    stackPanel.Children.Add(new ListView { VerticalAlignment = VerticalAlignment.Center });
                    functionContainer.Children.Add(textblock);
                    scrollViewer.Content = stackPanel;
                    functionContainer.Children.Add(scrollViewer);



                }
            }
            CalculateKValue();
        }




        // remove categoric item if it is not classified as Quasi-Identifier by the user
        private void RemoveSpecificCategoricItem(string columnName)
        {
            var childrenCopy = functionContainer.Children.Cast<UIElement>().ToList();
            for (int i = 0; i < childrenCopy.Count; i++)
            {


                if (childrenCopy[i] is TextBlock textBlock)
                {
                    string name = textBlock.Text;
                    if (name.Length > 1 && name == columnName)
                    {
                        functionContainer.Children.Remove(textBlock);
                        if (i < childrenCopy.Count - 1)
                        {
                            functionContainer.Children.Remove(childrenCopy[i + 1]);
                        }
                    }
                }
            }

            CalculateKValue();
        }



        // remove numeric item if it is not classified as Quasi-Identifier by the user
        private void RemoveSpecificNumericItem(string columnName)
        {
            var childrenCopy = numericContainer.Children.Cast<UIElement>().ToList();
            for (int i = 0; i < childrenCopy.Count; i++)
            {


                if (childrenCopy[i] is TextBlock textBlock)
                {
                    string name = textBlock.Text;
                    if (name.Length > 1 && name == columnName)
                    {
                        numericContainer.Children.Remove(textBlock);
                        if (i < childrenCopy.Count - 1)
                        {
                            numericContainer.Children.Remove(childrenCopy[i + 1]);
                        }
                    }
                }
            }

            CalculateKValue();
        }


        // generate numeric item if it is classified as Quasi-Identifier by the user
        private void GenerateSpecificNumericItem(int index)
        {
            List<string> distinctValues = dt.AsEnumerable()
                             .Select(row => row[index].ToString())
                             .Distinct().ToList();
            var canvas = new Canvas();
            canvas.Margin = new Thickness(0, 1, 0, 0);
            var margin = 5;
            canvas.Background = Brushes.Gray;
            canvas.Height = 55;
            canvas.HorizontalAlignment = HorizontalAlignment.Left;
            canvas.VerticalAlignment = VerticalAlignment.Top;

            distinctValues.Sort((a, b) => int.Parse(a).CompareTo(int.Parse(b)));

            var textblock = new TextBlock();
            textblock.FontWeight = FontWeights.DemiBold;
            textblock.FontSize = 14.0;
            textblock.Foreground = Brushes.Black;
            textblock.Margin = new Thickness(0, 10, 10, 10);



            textblock.Text = dt.Columns[index].ColumnName;
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };



            for (int j = 0; j < distinctValues.Count; j++)
            {

                var col = distinctValues[j];
                var numTextblock = new TextBlock();
                numTextblock.FontWeight = FontWeights.DemiBold;
                numTextblock.FontSize = 14.0;
                numTextblock.Foreground = Brushes.Black;
                numTextblock.VerticalAlignment = VerticalAlignment.Center;
                numTextblock.Text = col;
                numTextblock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double textBlockHeight = numTextblock.DesiredSize.Height;
                double topMargin = (canvas.Height - textBlockHeight) / 2;
                Canvas.SetLeft(numTextblock, margin + 15);
                Canvas.SetTop(numTextblock, topMargin);
                canvas.Children.Add(numTextblock);
                margin += 50;

                if (j != distinctValues.Count - 1)
                {
                    var rectangle = new Rectangle
                    {
                        Width = 3,
                        Height = canvas.Height,
                        Fill = Brushes.Black,

                    };


                    rectangle.MouseLeftButtonDown += (sender, e) => Rect_MouseLeftButtonDown(sender, e, canvas, index);


                    Canvas.SetLeft(rectangle, margin + 10);
                    canvas.Children.Add(rectangle);
                    margin += 5;



                }
            }
            canvas.Width = margin + 30;

            var scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Margin = new Thickness(0, 0, 0, 10);

            stackPanel.Children.Add(canvas);
            stackPanel.Children.Add(new ListView { VerticalAlignment = VerticalAlignment.Center });

            // inverse button
            var btn = new Button { Content = "Inverse", Background = Brushes.SkyBlue, FontSize = 14, Margin = new Thickness(10, 0, 0, 0), Foreground = Brushes.White, Width = 60, Height = 30 };
            btn.Click += (sender, e) => InverseGrouping(canvas, index);
            stackPanel.Children.Add(btn);

            scrollViewer.Content = stackPanel;

            if (index == 0)
            {
                numericContainer.Children.Insert(index, scrollViewer);
                numericContainer.Children.Insert(index, textblock);
            }



            else if (index >= numericContainer.Children.Count)
            {

                numericContainer.Children.Add(textblock);
                numericContainer.Children.Add(scrollViewer);
            }
            else
            {

                numericContainer.Children.Insert(index + 1, scrollViewer);
                numericContainer.Children.Insert(index + 1, textblock);



            }

            CalculateKValue();

        }



        // generate categoric item if it is classified as Quasi-Identifier by the user
        private void GenerateSpecificCategoricItem(int index)
        {
            List<string> distinctValues = dt.AsEnumerable()
                            .Select(row => row.Field<string>(index))
                            .Distinct().ToList();

            var canvas = new Canvas();
            double currentLeft = 0.0;
            canvas.Background = Brushes.Gray;
            canvas.Height = 55;
            canvas.HorizontalAlignment = HorizontalAlignment.Left;
            canvas.VerticalAlignment = VerticalAlignment.Top;
            var textblock = new TextBlock();
            textblock.FontWeight = FontWeights.DemiBold;
            textblock.FontSize = 14.0;
            textblock.Foreground = Brushes.Black;
            textblock.Margin = new Thickness(0, 10, 10, 10);

            textblock.Text = dt.Columns[index].ColumnName;

            for (int j = 0; j < distinctValues.Count; j++)
            {
                var col = distinctValues[j];

                var border = new Border();
                var converter = new BrushConverter();
                var brush = (Brush)converter.ConvertFromString("#3F48CC");
                border.Background = brush;
                border.Tag = col;
                border.Cursor = Cursors.Hand;
                border.CornerRadius = new CornerRadius(5);
                border.Padding = new Thickness(10);

                var textBlockInside = new TextBlock() { Text = col, Foreground = Brushes.White, FontSize = 14 };
                textBlockInside.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                double textWidth = textBlockInside.DesiredSize.Width;

                border.Child = textBlockInside;
                border.Width = textWidth + 20;

                border.MouseLeftButtonDown += (sender, e) =>
                {
                    var element = sender as UIElement;
                    if (element != null)
                    {
                        element.CaptureMouse();
                        isDragging = true;
                        startPoint = e.GetPosition(canvas);
                    }
                };

                border.MouseMove += (sender, e) =>
                {
                    if (isDragging)
                    {
                        Point currentPosition = e.GetPosition(canvas);
                        double x = currentPosition.X - startPoint.X;
                        double y = currentPosition.Y - startPoint.Y;
                        double newX = Canvas.GetLeft(border) + x;
                        double newY = Canvas.GetTop(border) + y;
                        if (newX >= 0 && newX <= canvas.ActualWidth - border.ActualWidth && newY >= 0 && newY <= canvas.ActualHeight - border.ActualHeight)
                        {
                            Canvas.SetLeft(border, newX);
                            Canvas.SetTop(border, newY);
                            startPoint = currentPosition;
                        }
                    }
                };

                border.MouseLeftButtonUp += (sender, e) =>
                {
                    var element = sender as UIElement;
                    if (element != null)
                    {
                        element.ReleaseMouseCapture();
                        isDragging = false;
                    }
                    UpdateCategoricColumn(canvas, index);
                };

                Canvas.SetLeft(border, currentLeft);
                Canvas.SetTop(border, 0);
                canvas.Children.Add(border);
                currentLeft += border.Width;

                if (j < distinctValues.Count - 1)
                {
                    currentLeft += 20;
                }
            }
            canvas.Width = currentLeft;

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Margin = new Thickness(0, 0, 0, 10);
            stackPanel.Children.Add(canvas);
            stackPanel.Children.Add(new ListView { VerticalAlignment = VerticalAlignment.Center });
            scrollViewer.Content = stackPanel;

            if (index == 0)
            {
                functionContainer.Children.Insert(index, scrollViewer);
                functionContainer.Children.Insert(index, textblock);
            }
            else if (index >= functionContainer.Children.Count)
            {
                functionContainer.Children.Add(textblock);
                functionContainer.Children.Add(scrollViewer);
            }
            else
            {
                functionContainer.Children.Insert(index + 1, scrollViewer);
                functionContainer.Children.Insert(index + 1, textblock);
            }




            CalculateKValue();
        }








        // categoric column in datatable is updated according to intersecting or non-intersecting of categoric items
        private void UpdateCategoricColumn(Canvas canvas, int columnIndex)
        {
            var groups = new List<List<string>>();
            DataTable init = initialTable.Copy();

            // loop over canvas children and find intersecting
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                var border1 = canvas.Children[i] as Border;
                var currentGroup = new List<string>();
                currentGroup.Add((string)border1.Tag);

                for (int j = 0; j < canvas.Children.Count; j++)
                {
                    if (i == j) continue;

                    var border2 = canvas.Children[j] as Border;

                    var point1 = border1.TranslatePoint(new Point(0, 0), canvas);
                    var point2 = border2.TranslatePoint(new Point(0, 0), canvas);

                    Rect rect1 = new Rect(point1.X, point1.Y, border1.RenderSize.Width, border1.RenderSize.Height);
                    Rect rect2 = new Rect(point2.X, point2.Y, border2.RenderSize.Width, border2.RenderSize.Height);
                    if (rect1.IntersectsWith(rect2))
                    {
                        currentGroup.Add((string)border2.Tag);
                    }
                }

                if (currentGroup.Count > 1)
                {
                    bool groupFound = false;
                    foreach (var group in groups)
                    {
                        if (currentGroup.Any(b => group.Contains(b)))
                        {
                            group.AddRange(currentGroup.Except(group));
                            groupFound = true;
                            break;
                        }
                    }
                    if (!groupFound)
                    {
                        groups.Add(currentGroup);
                    }
                }
            }
            var rows = dt.AsEnumerable().ToList();
            for (int i = 0; i < rows.Count; ++i)
            {
                rows[i][columnIndex] = init.Rows[i][columnIndex];

            }
            if (groups.Count > 0)
            {
                foreach (var group in groups)
                {
                    var groupValue = string.Join(" | ", group);
                    foreach (var row in rows)
                    {
                        var cellValue = row[columnIndex].ToString();

                        if (group.Contains(cellValue))
                        {
                            row[columnIndex] = groupValue;

                        }
                    }
                }
            }

              CalculateKValue();


            CheckSelectedTab(tabSelec);
        }



        private void SpecificColumnVisibility(int index)
        {
            DataTable init = initialTable.Copy();

            if (labelComboboxes[index].SelectedItem != null && table.Columns.Count != 0 && labelComboboxes[index].SelectedItem.ToString() == "Identifier")
            {
                AsteriskColumnData(index);
                CenterColumnData(index);
            }
            else if (labelComboboxes[index].SelectedItem != null && table.Columns.Count != 0 && labelComboboxes[index].SelectedItem.ToString() != "Identifier")
            {
                // Restore column data
                RestoreColumnData(index, init);
            }
        }






        // This method replaces all the cell values in the given column with '*'
        private void AsteriskColumnData(int columnIndex)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row.Table.Columns.Count > columnIndex)
                {
                    row[columnIndex] = "*";
                }
            }

          
        }





        private void CenterColumnData(int columnIndex)
        {
            if (table.Columns.Count > columnIndex)
            {
                Style centeredStyle = new Style(typeof(DataGridCell));
                centeredStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                table.Columns[columnIndex].CellStyle = centeredStyle;
            }
        }






        // This method restores the original data in the given column
        private void RestoreColumnData(int columnIndex, DataTable init)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row.Table.Columns.Count > columnIndex)
                {
                    row[columnIndex] = init.Rows[i][columnIndex];
                }
            }
        }






        // calculation of k for k-Anonymity
        private void CalculateKValue()
        {
            // Find the column indexes with "Quasi-Identifier" selected
            List<int> quasiIdentifierIndexes = new List<int>();
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Quasi-Identifier")
                {
                    quasiIdentifierIndexes.Add(i);

                }
            }
            var groupedRows = dt.AsEnumerable()
            .GroupBy(row => string.Join("|", quasiIdentifierIndexes.Select(index => row[index])))
            .ToList();

            int minGroupSize = groupedRows.Min(group => group.Count());

            if (!quasiIdentifierIndexes.Any())
            {
                minGroupSize = 1;
            }

            view.MinGroupSize = minGroupSize + " Anonymity";


            int amountEquiClass = groupedRows.Count;

           
            if (minGroupSize == 1)
            {
                tableMessage.Text = "The table only fulfills 1-Anonymity. Each row represents a distinct equivalence class. Protection against reidentification is not given. Try to modify the data so that multiple rows are identical with regards to the Quasi-Identifiers";

            }
            else
            {
                tableMessage.Text = "The table fulfills " + minGroupSize + " Anonymity. There are " + amountEquiClass + " equivalence classes that contain rows which are identicial with regards to the Quasi-Identifiers";

            }


       

        

            if (minGroupSize > 1 && amountEquiClass != 1)
            {
                view.AmountEquiClass = amountEquiClass + " Equivalence Classes";

            }
            else if (minGroupSize > 1 && amountEquiClass == 1)
            {
                view.AmountEquiClass = amountEquiClass + " Equivalence Class";
            }
            else
            {
                view.AmountEquiClass = "No Equivalence Classes available";
            }


            DataTable newTable = dt.Clone();




            if (!newTable.Columns.Contains("GroupID"))
            {
                newTable.Columns.Add("GroupID", typeof(int));
            }

            if (!newTable.Columns.Contains("IsGroupStart"))
            {
                newTable.Columns.Add("IsGroupStart", typeof(bool));
            }

            if (!newTable.Columns.Contains("IsGroupEnd"))
            {
                newTable.Columns.Add("IsGroupEnd", typeof(bool));
            }

            if (minGroupSize == 1)
            {
                foreach (DataRow row in dt.Rows)
                {
                    DataRow newRow = newTable.NewRow();
                    newRow.ItemArray = row.ItemArray;
                    newRow["GroupID"] = -1;
                    newRow["IsGroupStart"] = false;
                    newRow["IsGroupEnd"] = false;
                    newTable.Rows.Add(newRow);
                }

              
            }
            else
            {
                int groupID = 0;
                foreach (var group in groupedRows)
                {
                    var groupRows = group.ToList();
                    for (int i = 0; i < groupRows.Count; i++)
                    {
                        DataRow newRow = newTable.NewRow();
                        newRow.ItemArray = groupRows[i].ItemArray;
                        newRow["GroupID"] = groupID;
                        newRow["IsGroupStart"] = i == 0;
                        newRow["IsGroupEnd"] = i == groupRows.Count - 1;
                        newTable.Rows.Add(newRow);
                    }
                    groupID++;
                }
            }



            dt = newTable;
            table.ItemsSource = dt.DefaultView;
            // ColumnVisibility();
            OnDataTableChanged();

          

            // other anonymization methods are called here
         //   CalculateDistinctLDiversity(minGroupSize);
         //   CalculateEntropyLDiversity(minGroupSize);
         //   CalculateRecursiveDiversity(minGroupSize);
         //   GeneralAlphaKAnonymity(minGroupSize);
         //   SimpleAlphaKAnonymity(minGroupSize);
         //   Tcloseness(minGroupSize);
        }


        // distinct l diversity calculation
        private void CalculateDistinctLDiversity(int minGroupSize)
        {

            int sensitiveIndex = -1;
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Sensitive Attribute")
                {
                    sensitiveIndex = i;
                    break;
                }
            }


            if (sensitiveIndex == -1)
            {

                view.DistinctLValue = "No";
                return;
            }


            if (minGroupSize > 1)
            {

                var groupedRows = dt.AsEnumerable()
                    .GroupBy(row => row["GroupID"])
                    .Select(group => new
                    {
                        GroupId = group.Key,
                        DistinctValuesCount = group
                            .Select(row => row[sensitiveIndex].ToString())
                            .Distinct()
                            .Count()
                    })
                    .ToList();


                var l = groupedRows.OrderBy(group => group.DistinctValuesCount).FirstOrDefault();


                if (l.DistinctValuesCount == 1)
                {
                    view.DistinctLValue = "No";
                }

                else
                {
                    view.DistinctLValue = "" + l.DistinctValuesCount + "- Diversity";
                }
            }
            else
            {
                view.DistinctLValue = "No";

            }
        }



        // entropy l diversity calculation
        private void CalculateEntropyLDiversity(int minGroupSize)
        {
            int sensitiveIndex = -1;
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Sensitive Attribute")
                {
                    sensitiveIndex = i;
                    break;
                }
            }

            if (sensitiveIndex == -1)
            {
                view.EntropyLValue = "No";
                return;
            }

            if (minGroupSize > 1)
            {
                var groupedRows = dt.AsEnumerable()
                    .GroupBy(row => row["GroupID"])
                    .ToList();

                double minEntropy = double.MaxValue;
                int l = int.MaxValue;
                foreach (var group in groupedRows)
                {
                    var valueCounts = group
                        .Select(row => row[sensitiveIndex].ToString())
                        .GroupBy(val => val)
                        .ToDictionary(g => g.Key, g => g.Count());

                    double total = valueCounts.Values.Sum();
                    double entropy = 0;
                    foreach (var count in valueCounts.Values)
                    {
                        double probability = count / total;
                        entropy -= probability * Math.Log(probability, 2.0);
                    }

                    if (entropy < minEntropy)
                    {
                        minEntropy = entropy;
                    }

                    int distinctValues = valueCounts.Count;
                    if (distinctValues < l)
                    {
                        l = distinctValues;
                    }


                }

                if (l == 1)
                {
                    view.EntropyLValue = "No";
                    return;

                }
                else if (minEntropy >= Math.Log(l, 2.0))
                {

                    view.EntropyLValue = "Datatable is Entropy-L-Divers with smallest Entropy being " + minEntropy;

                }
                else
                {

                    view.EntropyLValue = "No Entropy-L-Diversity with smallest Entropy being " + minEntropy;
                }
            }
            else
            {
                view.EntropyLValue = "No";
            }
        }


        // recursive (c,l) diversity calculation
        private void CalculateRecursiveDiversity(int minGroupSize)
        {
            int sensitiveIndex = -1;
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Sensitive Attribute")
                {
                    sensitiveIndex = i;
                    break;
                }
            }

            if (sensitiveIndex == -1)
            {
                view.RecursiveLValue = "No";
                return;
            }

            if (minGroupSize > 1)
            {
                var groupedRows = dt.AsEnumerable()
                    .GroupBy(row => row["GroupID"])
                    .ToList();

                int l = int.MaxValue;
                List<int> minGroupCounts = null;
                foreach (var group in groupedRows)
                {
                    var valueCounts = group
                        .Select(row => row[sensitiveIndex].ToString())
                        .GroupBy(val => val)
                        .Select(g => g.Count())
                        .OrderByDescending(count => count)
                        .ToList();

                    int distinctValues = valueCounts.Count;
                    if (distinctValues < l)
                    {
                        l = distinctValues;
                        minGroupCounts = valueCounts;
                    }
                }

                if (minGroupCounts != null)
                {
                    var nM = minGroupCounts.Skip(l - 1).ToList();  // new list in which the the highest amount in minGroupCounts is not available anymore
                    double c = (double)minGroupCounts.First() / nM.Sum();  // mingroupCounts.First() represents the highest amount


                    bool cSmall = true;
                    foreach (var group in groupedRows)
                    {
                        var valueCounts = group
                            .Select(row => row[sensitiveIndex].ToString())
                            .GroupBy(val => val)
                            .Select(g => g.Count())
                            .OrderByDescending(count => count)
                            .ToList();

                        if (valueCounts.First() / valueCounts.Skip(l - 1).Sum() < c)
                        {
                            cSmall = false;
                            break;
                        }
                    }

                    var message = "n1 < c * (";
                    for (int i = 0; i < nM.Count; i++)
                    {
                        if (i != 0)
                        {
                            message += " + ";
                        }
                        message += "n" + (i + l).ToString();
                    }
                    message += ")";

                    if (cSmall)
                    {
                        message += " with c > " + c.ToString();

                        view.RecursiveLValue = message;
                    }
                    else
                    {

                        view.RecursiveLValue = "No";
                    }
                }
            }
            else
            {
                view.RecursiveLValue = "No";
            }
        }





        // simple alpha k anonymity calculation
        private void SimpleAlphaKAnonymity(int minGroupSize)
        {


            view.SimpleAlphaK = "";

            int sensitiveIndex = -1;
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Sensitive Attribute")
                {
                    sensitiveIndex = i;
                    break;
                }
            }

            if (sensitiveIndex == -1)
            {
                view.SimpleAlphaK = "No";
                return;
            }

            if (minGroupSize > 1)
            {
                var groupedRows = dt.AsEnumerable()
                    .GroupBy(row => row["GroupID"])
                    .ToList();

                Dictionary<string, double> maxFrequency = new Dictionary<string, double>();

                foreach (var group in groupedRows)
                {
                    var valueCounts = group
                        .Select(row => row[sensitiveIndex].ToString())
                        .GroupBy(val => val)
                        .ToDictionary(g => g.Key, g => g.Count());

                    double total = valueCounts.Values.Sum();
                    foreach (var k in valueCounts)
                    {
                        double frequency = k.Value / total;
                        if (!maxFrequency.ContainsKey(k.Key) || maxFrequency[k.Key] < frequency)
                        {
                            maxFrequency[k.Key] = frequency;
                        }
                    }
                }

                foreach (var k in maxFrequency)
                {

                    string sensValue = k.Key;
                    double alphaValue = k.Value;
                    string message = sensValue + ": " + "(" + alphaValue + ", " + minGroupSize + ")- Anonymity \n";
                    view.SimpleAlphaK += message;
                }
            }
            else
            {
                view.SimpleAlphaK = "No";
            }
        }


        // general alpha k anonymity calculation
        private void GeneralAlphaKAnonymity(int minGroupSize)
        {
            int sensitiveIndex = -1;
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Sensitive Attribute")
                {
                    sensitiveIndex = i;
                    break;
                }
            }

            if (sensitiveIndex == -1)
            {

                view.GeneralAlphaK = "No";
                return;
            }

            if (minGroupSize > 1)
            {
                var groupedRows = dt.AsEnumerable()
                    .GroupBy(row => row["GroupID"])
                    .ToList();

                double maxFrequency = 0;
                foreach (var group in groupedRows)
                {
                    var valueCounts = group
                        .Select(row => row[sensitiveIndex].ToString())
                        .GroupBy(val => val)
                        .ToDictionary(g => g.Key, g => g.Count());

                    double total = valueCounts.Values.Sum();
                    foreach (var count in valueCounts.Values)
                    {
                        double frequency = count / total;
                        if (frequency > maxFrequency)
                        {
                            maxFrequency = frequency;
                        }
                    }
                }


                view.GeneralAlphaK = "(" + maxFrequency + " , " + minGroupSize + ")- Anonymity";


            }
            else
            {
                view.GeneralAlphaK = "No";
            }




        }


        // T closeness calculation
        private void Tcloseness(int minGroupSize)
        {
            int sensitiveIndex = -1;
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Sensitive Attribute")
                {
                    sensitiveIndex = i;
                    break;
                }
            }

            if (sensitiveIndex == -1)
            {
                view.TCloseness = "No";
                tableMessage.Text = "No sensitive value is selected, therefore t-Closeness cannot be calculated";
                return;
            }

            double total = dt.Rows.Count;

            var overallCounts = dt.AsEnumerable()
                .Select(row => row[sensitiveIndex].ToString())
                .GroupBy(val => val)
                .ToDictionary(g => g.Key, g => g.Count());

            var groupedRows = dt.AsEnumerable()
                .GroupBy(row => row["GroupID"])
                .ToList();

            double maxEmd = 0;

            foreach (var group in groupedRows)
            {
                double groupTotal = group.Count();

                var groupCounts = group
                    .Select(row => row[sensitiveIndex].ToString())
                    .GroupBy(val => val)
                    .ToDictionary(g => g.Key, g => g.Count());

                if (minGroupSize > 1 && hiddenComboboxes[sensitiveIndex].SelectedItem.ToString() == "Categoric")
                {
                    double emd = 0;

                    foreach (var overallCount in overallCounts)
                    {
                        double groupCount = groupCounts.ContainsKey(overallCount.Key) ? groupCounts[overallCount.Key] : 0;
                        emd += Math.Abs(groupCount / groupTotal - overallCount.Value / total);


                    }
                    emd /= 2;
                    maxEmd = Math.Max(maxEmd, emd);
                    view.TCloseness = maxEmd + " - Closeness";
                    tableMessage.Text = "The table fulfills t-Closeness with t equal to " + maxEmd + " The value of t represents a threshold for the deviation of the distribution of a sensitive value in an equivalence class from the distribution of that value in the overall table distribution";

                }
                else if (minGroupSize > 1 && hiddenComboboxes[sensitiveIndex].SelectedItem.ToString() == "Numeric")
                {
                    var orderedSensitiveValues = dt.AsEnumerable()
                        .Select(row => double.Parse(row[sensitiveIndex].ToString()))
                        .Distinct()
                        .OrderBy(val => val)
                        .ToList();

                    double r = 0;
                    double emd = 0;

                    int counter = 0;
                    foreach (var value in orderedSensitiveValues)
                    {
                        counter++;
                        if (counter == orderedSensitiveValues.Count)
                            break;

                        double overallCount = overallCounts.ContainsKey(value.ToString()) ? overallCounts[value.ToString()] : 0;
                        double groupCount = groupCounts.ContainsKey(value.ToString()) ? groupCounts[value.ToString()] : 0;

                        r += groupCount / groupTotal - overallCount / total;

                        emd += Math.Abs(r);
                    }
                    emd /= (orderedSensitiveValues.Count - 1);
                    maxEmd = Math.Max(maxEmd, emd);
                    view.TCloseness = maxEmd + " - Closeness";
                    tableMessage.Text = "The table fulfills t-Closeness with t equal to " + maxEmd + " The value of t represents a threshold for the deviation of the distribution of a sensitive value in an equivalence class from the distribution of that value in the overall table distribution";

                }
                else
                {
                    view.TCloseness = "No";
                    tableMessage.Text = "Each row represents currently a distinct equivalence class. Try to modify the data that multiple rows are identical with regards to the Quasi-Identifiers";

                }
            }


        }


        /*
        private void OnDataTransformationButtonClick(object sender, RoutedEventArgs e)
        {

            DataTransformationView.Visibility = Visibility.Visible;
            MetricsView.Visibility = Visibility.Collapsed;
       //     metricsButton.IsEnabled = true;
        }

        


        private void OnMetricsButtonClick(object sender, RoutedEventArgs e)
        {
            DataTransformationView.Visibility = Visibility.Collapsed;
            MetricsView.Visibility = Visibility.Visible;
         //   metricsButton.IsEnabled = false;

        }


        */

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as TabControl; 
            var selectedTab = tabControl.SelectedItem as TabItem; 

            if (selectedTab.Header.ToString() == "l-Diversity")
            {
                LDiversityTable();
            }else if(selectedTab.Header.ToString() == "k-Anonymity")
            {
                KAnonymityTable();

            }else if(selectedTab.Header.ToString() == "t-Closeness")
            {
                TclosenessTable();
            }
        }

        private void KAnonymityTable()
        {

            table.ItemsSource = dt.DefaultView;

            List<int> quasiIdentifierIndexes = new List<int>();
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Quasi-Identifier")
                {
                    quasiIdentifierIndexes.Add(i);

                }
            }
            var groupedRows = dt.AsEnumerable()
            .GroupBy(row => string.Join("|", quasiIdentifierIndexes.Select(index => row[index])))
            .ToList();

            int minGroupSize = groupedRows.Min(group => group.Count());

            if (!quasiIdentifierIndexes.Any())
            {
                minGroupSize = 1;
            }

            int amountEquiClass = groupedRows.Count;
            if (minGroupSize == 1)
            {
                tableMessage.Text = "The table only fulfills 1-Anonymity. Each row represents a distinct equivalence class. Protection against reidentification is not given. Try to modify the data so that multiple rows are identical with regards to the Quasi-Identifiers";

            }
            else
            {
                tableMessage.Text = "The table fulfills " + minGroupSize + " Anonymity. There are " + amountEquiClass + " equivalence classes that contain rows which are identicial with regards to the Quasi-Identifiers";

            }

        }


        private void LDiversityTable()
        {
            int sensitiveIndex = -1;
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Sensitive Attribute")
                {
                    sensitiveIndex = i;
                    break;
                }
            }

            if (sensitiveIndex == -1)
            {
                Console.WriteLine("No sensitive value");
                tableMessage.Text = "No sensitive value is selected, therefore l-Diversity cannot be calculated";
                return;
            }

            List<int> quasiIdentifierIndexes = new List<int>();
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Quasi-Identifier")
                {
                    quasiIdentifierIndexes.Add(i);

                }
            }

            var equivalenceClass = dt.AsEnumerable()
            .GroupBy(row => string.Join("|", quasiIdentifierIndexes.Select(index => row[index])))
            .ToList();

            int minGroupSize = equivalenceClass.Min(group => group.Count());




            var groupedRows = dt.AsEnumerable()
                .GroupBy(row => new { GroupId = row["GroupID"], SensitiveValue = row[sensitiveIndex] })
                .Select(group => new
                {
                    group.Key.GroupId,
                    group.Key.SensitiveValue,
                    Count = group.Count()
                })
                .ToList();

 

            var lValueGroups = dt.AsEnumerable()
                .GroupBy(row => row["GroupID"])
                .Select(group => new
                {
                    GroupId = group.Key,
                    LValue = group.Select(row => row[sensitiveIndex]).Distinct().Count()
                })
                .ToList();

            DataTable newTable = dt.Clone();


            if (!newTable.Columns.Contains("Sensitive Amount"))
            {
                newTable.Columns.Add("Sensitive Amount", typeof(int));
            }

            if (!newTable.Columns.Contains("l-value"))
            {
                newTable.Columns.Add("l-value", typeof(int));
            }



            if (minGroupSize > 1)
            {

                var equiClass = dt.AsEnumerable()
             .GroupBy(row => row["GroupID"])
             .Select(group => new
             {
                 GroupId = group.Key,
                 DistinctValuesCount = group
                     .Select(row => row[sensitiveIndex].ToString())
                     .Distinct()
                     .Count()
             })
             .ToList();


                var l = equiClass.OrderBy(group => group.DistinctValuesCount).FirstOrDefault();


                if (l.DistinctValuesCount == 1)
                {
                    tableMessage.Text = "The table only fulfills 1-Diversity as there exist equivalence classes in which the rows have the same sensitive value. This table is not sufficiently protected and sensitive information may be disclosed";
                }

                else
                {
                    tableMessage.Text = "The table fulfills " + l.DistinctValuesCount + "- Diversity. In every equivalence class there are at least " + l.DistinctValuesCount + " different sensitive values";
                }

                foreach (var group in groupedRows)
                {
                    DataRow groupRow = dt.AsEnumerable().First(row => row["GroupID"].Equals(group.GroupId) && row[sensitiveIndex].Equals(group.SensitiveValue));

                    DataRow newRow = newTable.NewRow();
                    newRow.ItemArray = groupRow.ItemArray;
                    newRow["Sensitive Amount"] = group.Count;

                    var lValueGroup = lValueGroups.First(g => g.GroupId.Equals(group.GroupId));
                    if (newTable.AsEnumerable().Any(row => row["GroupID"].Equals(group.GroupId)))
                    {
                        newRow["l-value"] = DBNull.Value;
                    }
                    else
                    {
                        newRow["l-value"] = lValueGroup.LValue;
                    }

                    newTable.Rows.Add(newRow);
                }
            }
            else
            {
               
                foreach (DataRow row in dt.Rows)
                {
                    DataRow newRow = newTable.NewRow();
                    newRow.ItemArray = row.ItemArray;
                    newRow["l-value"] = 1; 
                    newRow["Sensitive Amount"] = 1;
                    newTable.Rows.Add(newRow);
                }

                tableMessage.Text = "The table only fulfills 1-Diversity as every row represents a distinct equivalence class. Try to modify the data to make multiple rows identical with regards to the Quasi-Identifiers";



            }
            table.ItemsSource = newTable.DefaultView;
        }





        private void TclosenessTable()
        {
            int sensitiveIndex = -1;
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Sensitive Attribute")
                {
                    sensitiveIndex = i;
                    break;
                }
            }

            if (sensitiveIndex == -1)
            {
                Console.WriteLine("No sensitive value");
                tableMessage.Text = "No sensitive value is selected, therefore t-Closeness cannot be calculated";

                return;
            }

            List<int> quasiIdentifierIndexes = new List<int>();
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Quasi-Identifier")
                {
                    quasiIdentifierIndexes.Add(i);
                }
            }

            var equivalenceClass = dt.AsEnumerable()
                .GroupBy(row => string.Join("|", quasiIdentifierIndexes.Select(index => row[index])))
                .ToList();

            int minGroupSize = equivalenceClass.Min(group => group.Count());

            Tcloseness(minGroupSize);

            var groupedRows = dt.AsEnumerable()
                .GroupBy(row => new { GroupId = row["GroupID"], SensitiveValue = row[sensitiveIndex] })
                .Select(group => new
                {
                    group.Key.GroupId,
                    group.Key.SensitiveValue,
                    Count = group.Count()
                })
                .ToList();

            if (minGroupSize > 1)
            {
                DataTable newTable = new DataTable();

                newTable.Columns.Add("IsGroupStart", typeof(bool));
                newTable.Columns.Add("IsGroupEnd", typeof(bool));
                newTable.Columns.Add("GroupID", typeof(int));
                newTable.Columns.Add("Sensitive Values");

                for (int i = 0; i < equivalenceClass.Count; i++)
                {
                    newTable.Columns.Add("Distribution EC " + (i + 1));
                }

                newTable.Columns.Add("Overall Distribution");

                var distinctSensitiveValues = dt.AsEnumerable().Select(row => row[sensitiveIndex]).Distinct();

                foreach (var sensitiveValue in distinctSensitiveValues)
                {
                    DataRow newRow = newTable.NewRow();
                    newRow["IsGroupStart"] = false;
                    newRow["IsGroupEnd"] = false;
                    newRow["GroupID"] = -1;
                    newRow["Sensitive Values"] = sensitiveValue;

                    for (int i = 0; i < equivalenceClass.Count; i++)
                    {
                        int count = equivalenceClass[i].Count(row => row[sensitiveIndex].Equals(sensitiveValue));
                        newRow["Distribution EC " + (i + 1)] = count == 0 ? "0" : count + "/" + equivalenceClass[i].Count();
                    }

                    int overallCount = dt.AsEnumerable().Count(row => row[sensitiveIndex].Equals(sensitiveValue));
                    newRow["Overall Distribution"] = equivalenceClass.Count == 1
                        ? newRow["Distribution EC 1"]
                        : (overallCount == 0 ? "0" : overallCount + "/" + dt.Rows.Count);

                    newTable.Rows.Add(newRow);
                }

                table.ItemsSource = newTable.DefaultView;
            }
            else
            {
                DataTable newTable = new DataTable();

                newTable.Columns.Add("IsGroupStart", typeof(bool));
                newTable.Columns.Add("IsGroupEnd", typeof(bool));
                newTable.Columns.Add("GroupID", typeof(int));
                newTable.Columns.Add("Sensitive Values");
                newTable.Columns.Add("Overall Distribution");

                var distinctSensitiveValues = dt.AsEnumerable().Select(row => row[sensitiveIndex]).Distinct();

                foreach (var sensitiveValue in distinctSensitiveValues)
                {
                    DataRow newRow = newTable.NewRow();
                    newRow["IsGroupStart"] = false;
                    newRow["IsGroupEnd"] = false;
                    newRow["GroupID"] = -1;
                    newRow["Sensitive Values"] = sensitiveValue;

                    int overallCount = dt.AsEnumerable().Count(row => row[sensitiveIndex].Equals(sensitiveValue));
                    newRow["Overall Distribution"] = overallCount == 0 ? "0" : overallCount + "/" + dt.Rows.Count;

                    newTable.Rows.Add(newRow);
                }

                table.ItemsSource = newTable.DefaultView;
            }
        }




        private void CheckSelectedTab(TabControl tabControl)
        {
            var selectedTab = tabControl.SelectedItem as TabItem;

            if (selectedTab != null)
            {
                if (selectedTab.Header.ToString() == "l-Diversity")
                {
                    LDiversityTable();
                }
                else if (selectedTab.Header.ToString() == "k-Anonymity")
                {


                    KAnonymityTable();

                }else if (selectedTab.Header.ToString() == "t-Closeness")
                {
                    TclosenessTable();
                }
            }
        }


    }

}