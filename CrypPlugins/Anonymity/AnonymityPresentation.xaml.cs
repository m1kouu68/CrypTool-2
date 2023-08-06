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
 
        private List<Label> identMessageLabels = new List<Label>();




        public AnonymityPresentation()
        {
            InitializeComponent();

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
                box1.SelectionChanged += EnforceSingleSensitive;
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


            OnDataTableChanged();
        }




        // data table changed event
        public virtual void OnDataTableChanged()
        {
            DataTableChanged?.Invoke(this, EventArgs.Empty);
        }





        private void EnforceSingleSensitive(object sender, SelectionChangedEventArgs e)
        {
            ComboBox currentBox = sender as ComboBox;
            if (currentBox.SelectedItem.ToString() == "Sensitive Attribute")
            {
                foreach (ComboBox box in labelComboboxes)
                {
                    if (box != currentBox && box.SelectedItem.ToString() == "Sensitive Attribute")
                    {
                        box.SelectedItem = "Quasi-Identifier";
                    }
                }
            }
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


            for (int a = 0; a < labelComboboxes.Count; a++)
            {
                if (labelComboboxes[a].SelectedItem != null && labelComboboxes[a].SelectedItem.ToString() == "Identifier")
                {
                    CenterColumnData(a);
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

                    SortNumericValues(distinctValues);


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
                        numTextblock.Padding = new Thickness(10); // Padding hinzufügen
                        numTextblock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        double numTextblockHeight = numTextblock.DesiredSize.Height;
                        double topMargin = (canvas.Height - numTextblockHeight) / 2;

                        Canvas.SetLeft(numTextblock, margin);
                        Canvas.SetTop(numTextblock, topMargin);
                        canvas.Children.Add(numTextblock);

                        margin += (int)numTextblock.DesiredSize.Width + 10;

                        if (j != distinctValues.Count - 1)
                        {
                            var rectangle = new Rectangle
                            {
                                Width = 3,
                                Height = canvas.Height,
                                Fill = Brushes.Black,
                            };

                            rectangle.MouseLeftButtonDown += (sender, e) => Rect_MouseLeftButtonDown(sender, e, canvas, index);
                            Canvas.SetLeft(rectangle, margin);
                            canvas.Children.Add(rectangle);
                            margin += 10;
                        }
                    }

                    canvas.Width = margin;
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

        public void SortNumericValues(List<string> values)
        {

            decimal max = values.Max(val => decimal.Parse(val));

      
            if (max <= int.MaxValue)
            {
                values.Sort((a, b) => int.Parse(a).CompareTo(int.Parse(b)));
            }
            else if (max <= long.MaxValue)
            {
                values.Sort((a, b) => long.Parse(a).CompareTo(long.Parse(b)));
            }
            else
            {
                values.Sort((a, b) => decimal.Parse(a).CompareTo(decimal.Parse(b)));
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


            RectangleColoring(rect);
            HandleGrouping(rect, canvas, columnIndex);

        }


        /* handle rectangle coloring
         * 
         */

        private void RectangleColoring(Rectangle rect)
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


            int first = int.MaxValue;
            int second = int.MinValue;


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
                            first = Math.Min(first, value);
                            second = Math.Max(second, value);
                        }
                    }
                    else
                    {
                        if (inGroup && prevTextBlock != null)
                        {

                            int value = int.Parse(prevTextBlock.Text);
                            second = Math.Max(second, value);


                            foreach (var row in rows)
                            {
                                int cellValue;
                                if (int.TryParse(row[columnIndex].ToString(), out cellValue))
                                {
                                    if (cellValue >= first && cellValue <= second)
                                    {
                                        row[columnIndex] = $"[{first} - {second}]";
                                    }
                                }
                            }


                            inGroup = false;
                            first = int.MaxValue;
                            second = int.MinValue;
                        }
                    }
                }
            }


            if (inGroup && prevTextBlock != null)
            {

                int value = int.Parse(prevTextBlock.Text);
                second = Math.Max(second, value);


                foreach (var row in rows)
                {
                    int cellValue;
                    if (int.TryParse(row[columnIndex].ToString(), out cellValue))
                    {
                        if (cellValue >= first && cellValue <= second)
                        {
                            row[columnIndex] = $"[{first} - {second}]";
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

                        var text = new TextBlock() { Text = col, Foreground = Brushes.White, FontSize = 14 };
                        text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                        double textWidth = text.DesiredSize.Width;

                        border.Child = text;
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

            SortNumericValues(distinctValues);

            var textblock = new TextBlock();
            textblock.FontWeight = FontWeights.DemiBold;
            textblock.FontSize = 14.0;
            textblock.Foreground = Brushes.Black;
            textblock.Margin = new Thickness(0, 10, 10, 10);

            textblock.Text = dt.Columns[index].ColumnName;
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            numericContainer.Children.Add(textblock);

            for (int j = 0; j < distinctValues.Count; j++)
            {
                var col = distinctValues[j];
                var numTextblock = new TextBlock();
                numTextblock.FontWeight = FontWeights.DemiBold;
                numTextblock.FontSize = 14.0;
                numTextblock.Foreground = Brushes.Black;
                numTextblock.VerticalAlignment = VerticalAlignment.Center;
                numTextblock.Padding = new Thickness(10);
                numTextblock.Text = col;
                numTextblock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                double textBlockHeight = numTextblock.DesiredSize.Height;
                double topMargin = (canvas.Height - textBlockHeight) / 2;
                Canvas.SetLeft(numTextblock, margin);
                Canvas.SetTop(numTextblock, topMargin);
                canvas.Children.Add(numTextblock);

                margin += (int)numTextblock.DesiredSize.Width + 10;

                if (j != distinctValues.Count - 1)
                {
                    var rectangle = new Rectangle
                    {
                        Width = 3,
                        Height = canvas.Height,
                        Fill = Brushes.Black,
                    };

                    rectangle.MouseLeftButtonDown += (sender, e) => Rect_MouseLeftButtonDown(sender, e, canvas, index);
                    Canvas.SetLeft(rectangle, margin);
                    canvas.Children.Add(rectangle);

                    margin += 10;
                }
            }

            canvas.Width = margin;

            var scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Margin = new Thickness(0, 0, 0, 10);
            stackPanel.Children.Add(canvas);
            stackPanel.Children.Add(new ListView { VerticalAlignment = VerticalAlignment.Center });

            var btn = new Button { Content = "Inverse", Background = Brushes.SkyBlue, FontSize = 14, Margin = new Thickness(10, 0, 0, 0), Foreground = Brushes.White, Width = 60, Height = 30 };
            btn.Click += (sender, e) => InverseGrouping(canvas, index);

            stackPanel.Children.Add(btn);
            scrollViewer.Content = stackPanel;

            if (index == 0)
            {
                numericContainer.Children.Insert(index, scrollViewer);
            }
            else if (index > 0 && index < numericContainer.Children.Count)
            {
                numericContainer.Children.Insert(index, scrollViewer);
            }
            else
            {
                numericContainer.Children.Add(scrollViewer);
            }
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

                var text = new TextBlock() { Text = col, Foreground = Brushes.White, FontSize = 14 };
                text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                double textWidth = text.DesiredSize.Width;

                border.Child = text;
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
                Style center = new Style(typeof(DataGridCell));
                center.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                table.Columns[columnIndex].CellStyle = center;
            }
        }

        private void CenterColumnDataByName(string columnName)
        {
            var column = table.Columns.FirstOrDefault(c => c.Header != null && c.Header.ToString() == columnName);

            if (column != null)
            {
                Style center = new Style(typeof(DataGridCell));
                center.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                column.CellStyle = center;
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

            int maxGroupSize = groupedRows.Max(group => group.Count());

            if (!quasiIdentifierIndexes.Any())
            {
                minGroupSize = 1;
            }

            int amountEquiClass = groupedRows.Count;

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

            if (!newTable.Columns.Contains("k-Value"))
            {
                newTable.Columns.Add("k-Value", typeof(int));
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
                    newRow["k-Value"] = 1;
                    newTable.Rows.Add(newRow);
                }
            }
            else
            {
                int groupID = 0;
                foreach (var group in groupedRows)
                {
                    var groupRows = group.ToList();
                    int middleIndex = groupRows.Count / 2;
                    if (groupRows.Count % 2 == 0) 
                    {
                        middleIndex -= 1;
                    }

                    for (int i = 0; i < groupRows.Count; i++)
                    {
                        DataRow newRow = newTable.NewRow();
                        newRow.ItemArray = groupRows[i].ItemArray;
                        newRow["GroupID"] = groupID;
                        newRow["IsGroupStart"] = i == 0;
                        newRow["IsGroupEnd"] = i == groupRows.Count - 1;
                        if (i == middleIndex) 
                        {
                            newRow["k-Value"] = groupRows.Count;
                        }
                        else
                        {
                            newRow["k-Value"] = (object)DBNull.Value;
                        }
                        newTable.Rows.Add(newRow);
                    }
                    groupID++;
                }
            }
            dt = newTable;
            table.ItemsSource = dt.DefaultView;
            OnDataTableChanged();
            

        }
        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            var selectedTab = tabControl.SelectedItem as TabItem;

            if (selectedTab.Header.ToString() == "Distinct l-Diversity")
            {
                DistinctLDiversityTable();
                ColumnKValueCollapsed();
                OnDataTableChanged();
            }
            else if (selectedTab.Header.ToString() == "k-Anonymity")
            {
                KAnonymityTable();
                ColumnKValueVisible();
                OnDataTableChanged();
            }
            else if (selectedTab.Header.ToString() == "t-Closeness")
            {
                TClosenessTable();
                ColumnKValueCollapsed();
                OnDataTableChanged();
            }
            else if (selectedTab.Header.ToString() == "Entropy l-Diversity")
            {
                EntropyLDiversityTable();
                ColumnKValueCollapsed();
                OnDataTableChanged();
            }
            else if (selectedTab.Header.ToString() == "Recursive (c,l)-Diversity")
            {
                RecursiveLDiversityTable();
                ColumnKValueCollapsed();
                OnDataTableChanged();
            }
        }

        private void KAnonymityTable()
        {

            table.ItemsSource = dt.DefaultView;
            for (int a = 0; a < labelComboboxes.Count; a++)
            {
                if (labelComboboxes[a].SelectedItem != null && labelComboboxes[a].SelectedItem.ToString() == "Identifier")
                {
                    CenterColumnData(a);
                }
            }
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

            int maxGroupSize = groupedRows.Max(group => group.Count());

            int amountEquiClass = groupedRows.Count;
            if (minGroupSize == 1)
            {
                tableMessage.Text = "The table only meets the criteria of 1-Anonymity, where each row represents a distinct equivalence class. Please modify the data to make multiple rows identical with respect to the columns designated as Quasi-Identifiers.";

            }
            else if (minGroupSize == maxGroupSize)
            {
                tableMessage.Text = "The table complies with k=" + minGroupSize + "-anonymity, as there is one equivalence class with a k-value of " + minGroupSize + ", meaning that there are " + minGroupSize + " rows that are identical with respect to the Quasi-Identifier columns.";

            }


            else
            {
                tableMessage.Text = "The table meets the criteria of k=" + minGroupSize + "-anonymity, as the smallest k-value for any equivalence class is " + minGroupSize + ". In total, there are " + amountEquiClass + " equivalence classes that are identical with respect to the Quasi-Identifier columns.";

            }
        }
        private void DistinctLDiversityTable()
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

                tableMessage.Text = "No sensitive column has been assigned, so l-Diversity cannot be calculated. Please mark a column as sensitive so that l-Diversity can be determined";
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
            var lValue = dt.AsEnumerable()
                .GroupBy(row => row["GroupID"])
                .Select(group => new
                {
                    GroupId = group.Key,
                    LValue = group.Select(row => row[sensitiveIndex]).Distinct().Count()
                })
                .ToList();

            DataTable newTable = dt.Clone();
            if (!newTable.Columns.Contains("Frequency"))
            {
                newTable.Columns.Add("Frequency", typeof(int));
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
                    tableMessage.Text = "The table only meets the criteria for 1-Diversity, as there is an equivalence class where every row has the same sensitive value. Please modify the data to ensure that different sensitive values are present within each equivalence class";
                }else if(equiClass.Count == 1)
                {
                    tableMessage.Text = "The table meets " + l.DistinctValuesCount + "-Diversity, as there are at least " + l.DistinctValuesCount + " different sensitive values in the equivalence class";

                }
                else
                {
                    tableMessage.Text = "The table meets " + l.DistinctValuesCount + "-Diversity, as there are at least " + l.DistinctValuesCount + " different sensitive values in every equivalence class";
                }

                var groupsWithRows = groupedRows
                 .GroupBy(g => g.GroupId)
                   .ToList();
                foreach (var groupWithRows in groupsWithRows)
                {
                    var rowsInGroup = groupWithRows.ToList();
                    int middleIndex = (rowsInGroup.Count - 1) / 2;  
                    var middleRowGroup = rowsInGroup[middleIndex];

                    foreach (var rowGroup in rowsInGroup)
                    {
                        DataRow groupRow = dt.AsEnumerable().First(row => row["GroupID"].Equals(rowGroup.GroupId) && row[sensitiveIndex].Equals(rowGroup.SensitiveValue));

                        DataRow newRow = newTable.NewRow();
                        newRow.ItemArray = groupRow.ItemArray;
                        newRow["Frequency"] = rowGroup.Count;

                        if (rowGroup == middleRowGroup)
                        {
                            var lValueGroup = lValue.First(g => g.GroupId.Equals(rowGroup.GroupId));
                            newRow["l-value"] = lValueGroup.LValue;
                        }
                        else
                        {
                            newRow["l-value"] = DBNull.Value;
                        }

                        newTable.Rows.Add(newRow);
                    }
                }
            }
            else
            {

                foreach (DataRow row in dt.Rows)
                {
                    DataRow newRow = newTable.NewRow();
                    newRow.ItemArray = row.ItemArray;
                    newRow["l-value"] = 1;
                    newRow["Frequency"] = 1;
                    newTable.Rows.Add(newRow);
                }

                tableMessage.Text = "The table only satisfies 1-Diversity since each row represents a distinct equivalence class. Please modify the data to ensure that multiple rows are identical with respect to the Quasi-Identifier columns";

            }
            table.ItemsSource = newTable.DefaultView;

            for (int a = 0; a < labelComboboxes.Count; a++)
            {
                if (labelComboboxes[a].SelectedItem != null && labelComboboxes[a].SelectedItem.ToString() == "Identifier")
                {
                    CenterColumnData(a);
                }
            }

            CenterColumnDataByName("l-value");
            OnDataTableChanged();
        }
        private void ColumnKValueCollapsed()
        {

            DataGridColumn kValue = table.Columns.FirstOrDefault(col => col.Header.ToString() == "k-Value");
            if (kValue != null)
            {
                kValue.Visibility = Visibility.Collapsed;
            }


        }
        private void ColumnKValueVisible()
        {

            DataGridColumn kValue = table.Columns.FirstOrDefault(col => col.Header.ToString() == "k-Value");
            if (kValue != null)
            {
                kValue.Visibility = Visibility.Visible;
            }


            CenterColumnDataByName("k-Value");

        }
        private void EntropyLDiversityTable()
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

            var equiClass = dt.AsEnumerable()
                .GroupBy(row => string.Join("|", quasiIdentifierIndexes.Select(index => row[index])))
                .ToList();

            int lValue = equiClass.Min(group => group.Select(row => row[sensitiveIndex]).Distinct().Count());

            List<double> entropyList = new List<double>();

            int minGroupSize = equiClass.Min(group => group.Count());

            DataTable newTable = dt.Clone();

            if (!newTable.Columns.Contains("Frequency (p)"))
            {
                newTable.Columns.Add("Frequency (p)", typeof(string));
            }

            if (!newTable.Columns.Contains("-p * log (p)"))
            {
                newTable.Columns.Add("-p * log (p)", typeof(double));
            }

            if (!newTable.Columns.Contains("Entropy"))
            {
                newTable.Columns.Add("Entropy", typeof(double));
            }

            if (minGroupSize > 1)
            {
                var groupedRows = dt.AsEnumerable()
         .GroupBy(row => new { GroupId = row["GroupID"], SensitiveValue = row[sensitiveIndex] })
         .Select(group => new
         {
             group.Key.GroupId,
             group.Key.SensitiveValue,
             Count = group.Count(),
             Total = dt.AsEnumerable().Count(row => row["GroupID"].Equals(group.Key.GroupId))
         })
         .ToList();

               

                var groupsByGroupId = groupedRows
                    .GroupBy(row => row.GroupId)
                    .ToList();

                foreach (var groupSet in groupsByGroupId)
                {
                    int middleIndex = (groupSet.Count() - 1) / 2;
                    double entropy = groupSet.Sum(g => -g.Count / (double)g.Total * Math.Log(g.Count / (double)g.Total, 2));
                    if (entropy * 1000 != Math.Floor(entropy * 1000))
                    {

                        entropy = Math.Round(entropy, 3, MidpointRounding.AwayFromZero);
                    }

                    entropyList.Add(entropy);


                    for (int i = 0; i < groupSet.Count(); i++)
                    {
                        var group = groupSet.ElementAt(i);
                        DataRow groupRow = dt.AsEnumerable().First(row => row["GroupID"].Equals(group.GroupId) && row[sensitiveIndex].Equals(group.SensitiveValue));
                        DataRow newRow = newTable.NewRow();
                        newRow.ItemArray = groupRow.ItemArray;

                        double relativeFrequency = (double)group.Count / group.Total;
                        newRow["Frequency (p)"] = $"{group.Count}/{group.Total}";
                        double entropyValue = -relativeFrequency * Math.Log(relativeFrequency, 2);
                        if (entropyValue * 1000 != Math.Floor(entropyValue * 1000))
                        {

                            entropyValue = Math.Round(entropyValue, 3, MidpointRounding.AwayFromZero);
                        }



                        newRow["-p * log (p)"] = entropyValue;



                        if (i == middleIndex)
                        {
                            newRow["Entropy"] = entropy;
                        }
                        else
                        {
                            newRow["Entropy"] = DBNull.Value;
                        }

                        newTable.Rows.Add(newRow);
                    }
                }


                bool b = false;

        foreach (double e in entropyList)
                {


                    if(e >= Math.Log(lValue, 2))
                    {
                        b = true;
                    }
                    else
                    {
                        b = false;
                    }
                }

        if(b == true)
                {
                    tableMessage.Text = "The table meets Entropy-l-Diversity as the entropy of every equivalence class is greater or equal to l=" + lValue;

                }
                else
                {
                    tableMessage.Text = "Entropy-l-Diversity is not given as the condition that the entropy of every equivalence class is greater or equal to l=" + lValue + " is not fulfilled";

                }


            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    DataRow newRow = newTable.NewRow();
                    newRow.ItemArray = row.ItemArray;
                    newRow["Frequency (p)"] = "1/1";
                    double relativeFrequency = 1.0;
                    double entropyValue = -relativeFrequency * Math.Log(relativeFrequency, 2);
                    newRow["-p * log (p)"] = entropyValue;
                    newRow["Entropy"] = entropyValue;

                    newTable.Rows.Add(newRow);
                }


                tableMessage.Text = "Every row represents a single equivalence class right now. Please modify the data to ensure that multiple rows are identical with respect to the Quasi-Identifiers";

            }

            table.ItemsSource = newTable.DefaultView;

            for (int a = 0; a < labelComboboxes.Count; a++)
            {
                if (labelComboboxes[a].SelectedItem != null && labelComboboxes[a].SelectedItem.ToString() == "Identifier")
                {
                    CenterColumnData(a);
                }
            }




            CenterColumnDataByName("Entropy");

        }


        private void TClosenessTable()
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

            var distinctSensitiveValues = dt.AsEnumerable().Select(row => row[sensitiveIndex]).Distinct().ToList();

            var equiClass = dt.AsEnumerable()
                .GroupBy(row => string.Join("|", quasiIdentifierIndexes.Select(index => row[index])))
                .ToList();

            int minGroupSize = equiClass.Min(group => group.Count());

            DataTable newTable = dt.Clone();

            if (!newTable.Columns.Contains("Frequency"))
            {
                newTable.Columns.Add("Frequency", typeof(string));
            }

            if (!newTable.Columns.Contains("Overall Distribution"))
            {
                newTable.Columns.Add("Overall Distribution", typeof(string));
            }

            if (!newTable.Columns.Contains("t-Value"))
            {
                newTable.Columns.Add("t-Value", typeof(string));
            }

            if (minGroupSize > 1 && hiddenComboboxes[sensitiveIndex].SelectedItem.ToString() == "Categoric")
            {
                foreach (var group in equiClass)
                {
                    var groupedRows = group
                        .GroupBy(row => row[sensitiveIndex])
                        .Select(grp => new
                        {
                            SensitiveValue = grp.Key,
                            Count = grp.Count(),
                            Total = group.Count()
                        })
                        .ToList();

                    double tValue = 0;
                    int initialRowCount = newTable.Rows.Count; 
                    foreach (var grpRow in groupedRows)
                    {
                        DataRow groupRow = group.First(row => row[sensitiveIndex].Equals(grpRow.SensitiveValue));

                        DataRow newRow = newTable.NewRow();
                        newRow.ItemArray = groupRow.ItemArray;

                        double relativeFrequency = (double)grpRow.Count / grpRow.Total;
                        newRow["Frequency"] = $"{grpRow.Count}/{grpRow.Total}";

                        int overallCount = dt.AsEnumerable().Count(row => row[sensitiveIndex].Equals(grpRow.SensitiveValue));
                        newRow["Overall Distribution"] = overallCount == 0 ? "0" : overallCount + "/" + dt.Rows.Count;

                        double overallDistribution = overallCount / (double)dt.Rows.Count;
                        tValue = CalculateTValueCategoric(relativeFrequency, overallDistribution, tValue);

                        newTable.Rows.Add(newRow);
                    }

                    var existingSensitive = group.Select(row => row[sensitiveIndex]).Distinct().ToList();
                    var missingSensitive = distinctSensitiveValues.Except(existingSensitive);

                    foreach (var missingSensitiveValue in missingSensitive)
                    {
                        DataRow missingRow = newTable.NewRow();
                        missingRow.ItemArray = group.First().ItemArray;

                        missingRow[sensitiveIndex] = missingSensitiveValue;
                        missingRow["Frequency"] = "0";

                        int overallCount = dt.AsEnumerable().Count(row => row[sensitiveIndex].Equals(missingSensitiveValue));
                        missingRow["Overall Distribution"] = overallCount == 0 ? "0" : overallCount + "/" + dt.Rows.Count;

                        double overallDistribution = overallCount / (double)dt.Rows.Count;
                        tValue = CalculateTValueCategoric(0, overallDistribution, tValue);

                        newTable.Rows.Add(missingRow);
                    }
                    int middleRowIndex = initialRowCount + ((newTable.Rows.Count - initialRowCount) - 1) / 2;
                    if (newTable.Rows.Count > initialRowCount)
                    {
                        newTable.Rows[middleRowIndex]["t-Value"] = tValue;
                    }
                }
                List<double> tValues = newTable.AsEnumerable()
                    .Where(row => row["t-Value"] != DBNull.Value)
                    .Select(row => double.Parse(row["t-Value"].ToString()))
                    .ToList();
                double maxTValue;
                if (tValues.Any())
                {
                    maxTValue = tValues.Max();
                }
                else
                {
                    maxTValue = 0;
                }


                tableMessage.Text = "The table meets t-Closeness with a t-value of " + maxTValue + ". The highest t-value found in any equivalence class determines the overall t-value for the datatable.";

            }
            else if (minGroupSize > 1 && hiddenComboboxes[sensitiveIndex].SelectedItem.ToString() == "Numeric")
            {
                foreach (var group in equiClass)
                {
                    var orderedSensitive = dt.AsEnumerable()
                        .Select(row => double.Parse(row[sensitiveIndex].ToString()))
                        .Distinct()
                        .OrderBy(val => val)
                        .ToList();

                    var sensitiveValuesInGroup = group.Select(row => double.Parse(row[sensitiveIndex].ToString())).Distinct().ToList();
                    var missingSensitiveValues = orderedSensitive.Except(sensitiveValuesInGroup);

                    double tValue = CalculateTValueNumeric(orderedSensitive, group, dt, sensitiveIndex);

                    int initialRowCount = newTable.Rows.Count; 

                    foreach (var value in sensitiveValuesInGroup)
                    {
                        DataRow existingRow = newTable.NewRow();
                        existingRow.ItemArray = group.First(row => double.Parse(row[sensitiveIndex].ToString()) == value).ItemArray;

                        int groupCount = group.Count(row => double.Parse(row[sensitiveIndex].ToString()) == value);
                        existingRow["Frequency"] = $"{groupCount}/{group.Count()}";

                        int overallCount = dt.AsEnumerable().Count(row => double.Parse(row[sensitiveIndex].ToString()) == value);
                        existingRow["Overall Distribution"] = overallCount == 0 ? "0" : overallCount + "/" + dt.Rows.Count;

                        newTable.Rows.Add(existingRow);
                    }

  
                    foreach (var value in missingSensitiveValues)
                    {
                        DataRow missingRow = newTable.NewRow();
                        missingRow.ItemArray = group.First().ItemArray;

                        missingRow[sensitiveIndex] = value;
                        missingRow["Frequency"] = "0";

                        int overallCount = dt.AsEnumerable().Count(row => double.Parse(row[sensitiveIndex].ToString()) == value);
                        missingRow["Overall Distribution"] = overallCount == 0 ? "0" : overallCount + "/" + dt.Rows.Count;

                        newTable.Rows.Add(missingRow);
                    }
                    int middleRowIndex = initialRowCount + ((newTable.Rows.Count - initialRowCount) - 1) / 2;
                    if (newTable.Rows.Count > initialRowCount)
                    {
                        newTable.Rows[middleRowIndex]["t-Value"] = tValue;
                    }
                }
                List<double> tValues = newTable.AsEnumerable()
                   .Where(row => row["t-Value"] != DBNull.Value)
                   .Select(row => double.Parse(row["t-Value"].ToString()))
                   .ToList();


                double maxTValue;
                if (tValues.Any())
                {
                    maxTValue = tValues.Max();
                }
                else
                {
                    maxTValue = 0;
                }
                tableMessage.Text = "The table meets t-Closeness with a t-value of " + maxTValue + ". The highest t-value found in any equivalence class determines the overall t-value for the datatable.";
            }

            else
            {
                foreach (DataRow dtRow in dt.Rows)
                {
                    DataRow newRow = newTable.NewRow();
                    newRow.ItemArray = dtRow.ItemArray;

                    newRow["Frequency"] = "1/1";

                    int overallCount = dt.AsEnumerable().Count(rowData => rowData[sensitiveIndex].Equals(dtRow[sensitiveIndex]));
                    newRow["Overall Distribution"] = overallCount == 0 ? "0" : overallCount + "/" + dt.Rows.Count;
                    if (hiddenComboboxes[sensitiveIndex].SelectedItem.ToString() == "Categoric")
                    {
                        double tValue = CalculateTValueCategoricSingleRowGroup(dt, sensitiveIndex, distinctSensitiveValues, dtRow);
                        newRow["t-Value"] = tValue;
                    }
                    else if (hiddenComboboxes[sensitiveIndex].SelectedItem.ToString() == "Numeric")
                    {

                        var orderedSensitiveValues = dt.AsEnumerable()
                            .Select(rowData => double.Parse(rowData[sensitiveIndex].ToString()))
                            .Distinct()
                            .OrderBy(val => val)
                            .ToList();

                        int overallSum = dt.AsEnumerable().Count(rowData => double.Parse(rowData[sensitiveIndex].ToString()) == double.Parse(dtRow[sensitiveIndex].ToString()));
                        newRow["Overall Distribution"] = overallSum == 0 ? "0" : overallSum + "/" + dt.Rows.Count;

                        double tValue = CalculateTValueNumericSingleRowGroup(dtRow, dt, sensitiveIndex, orderedSensitiveValues);
                        newRow["t-Value"] = tValue;
                    }

                    newTable.Rows.Add(newRow);
                }
                tableMessage.Text = "Every row represents a single equivalence class right now. Please modify the data to ensure that multiple rows are identical with respect to the Quasi-Identifiers";

            }

            // Implement the "IsGroupStart" and "IsGroupEnd" logic
            if (minGroupSize > 1)
            {
                var groups = newTable.AsEnumerable()
                    .GroupBy(row => string.Join("|", quasiIdentifierIndexes.Select(index => row[index])))
                    .ToList();

                foreach (var group in groups)
                {
                    bool isFirst = true;
                    foreach (var row in group)
                    {
                        row["IsGroupStart"] = isFirst;
                        row["IsGroupEnd"] = false;
                        isFirst = false;
                    }
                    if (group.Any())
                    {
                        group.Last()["IsGroupEnd"] = true;
                    }
                }
            }

            table.ItemsSource = newTable.DefaultView;

            for (int a = 0; a < labelComboboxes.Count; a++)
            {
                if (labelComboboxes[a].SelectedItem != null && labelComboboxes[a].SelectedItem.ToString() == "Identifier")
                {
                    CenterColumnData(a);
                }
            }


            CenterColumnDataByName("t-Value");
        }






        private double CalculateTValueCategoric(double relativeFrequency, double overallDistribution, double tValue)
        {
            tValue += Math.Abs(relativeFrequency - overallDistribution) / 2;

            if (tValue * 1000 != Math.Floor(tValue * 1000))
            {
                tValue = Math.Round(tValue, 3, MidpointRounding.AwayFromZero);
            }

            return tValue;
        }


        private double CalculateTValueNumeric(List<double> orderedSensitiveValues, IEnumerable<DataRow> group, DataTable dt, int sensitiveIndex)
        {
            double tValue = 0;
            double r = 0;
            for (int i = 0; i < orderedSensitiveValues.Count - 1; i++)
            {
                double currentValue = orderedSensitiveValues[i];
                double groupCount = group.Count(row => double.Parse(row[sensitiveIndex].ToString()) == currentValue);
                double overallCount = dt.AsEnumerable().Count(row => double.Parse(row[sensitiveIndex].ToString()) == currentValue);

                double groupDist = groupCount / (double)group.Count();
                double overallDist = overallCount / (double)dt.Rows.Count;

                r += groupDist - overallDist;
                tValue += Math.Abs(r);
            }
            tValue /= (orderedSensitiveValues.Count - 1);

            if (tValue * 1000 != Math.Floor(tValue * 1000))
            {
                tValue = Math.Round(tValue, 3, MidpointRounding.AwayFromZero);
            }


            return tValue;
        }

        public double CalculateTValueNumericSingleRowGroup(DataRow currentRow, DataTable dt, int sensitiveIndex, List<double> orderedSensitiveValues)
        {
            double tValue = 0;
            double r = 0;
            double currentValue = double.Parse(currentRow[sensitiveIndex].ToString());

            for (int i = 0; i < orderedSensitiveValues.Count - 1; i++)
            {
                double orderedValue = orderedSensitiveValues[i];
                double groupDist = orderedValue == currentValue ? 1 : 0;
                double overallDist = dt.AsEnumerable().Count(rowData => double.Parse(rowData[sensitiveIndex].ToString()) == orderedValue) / (double)dt.Rows.Count;
                r += groupDist - overallDist;
                tValue += Math.Abs(r);
            }
            tValue /= (orderedSensitiveValues.Count - 1);
            if (tValue * 1000 != Math.Floor(tValue * 1000))
            {
                tValue = Math.Round(tValue, 3, MidpointRounding.AwayFromZero);
            }



            return tValue;
        }
        private double CalculateTValueCategoricSingleRowGroup(DataTable dt, int sensitiveIndex, List<object> distinctSensitiveValues, DataRow dtRow)
        {
            double tValue = 0;
            foreach (var sensitiveValue in distinctSensitiveValues)
            {
                int countSensitive = dt.AsEnumerable().Count(rowData => rowData[sensitiveIndex].Equals(sensitiveValue));
                double overallDistribution = countSensitive / (double)dt.Rows.Count;
                if (sensitiveValue.Equals(dtRow[sensitiveIndex]))
                {
                    tValue += 1 - overallDistribution;
                }
                else
                {
                    tValue += overallDistribution;
                }
            }
            tValue = tValue / 2;
            if (tValue * 1000 != Math.Floor(tValue * 1000))
            {

                tValue = Math.Round(tValue, 3, MidpointRounding.AwayFromZero);
            }

            return tValue;
        }

        private void RecursiveLDiversityTable()
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
                tableMessage.Text = "No sensitive value is selected, therefore Recursive (c,l)-Diversity cannot be calculated";
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

            DataTable newTable = dt.Clone();

            if (!newTable.Columns.Contains("Frequency"))
            {
                newTable.Columns.Add("Frequency", typeof(int));
            }

            if (!newTable.Columns.Contains("c-value"))
            {
                newTable.Columns.Add("c-value", typeof(double));
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
               tableMessage.Text = "Recursive (c,l)-Diversity is fulfilled for any c with n1 < c * (nl + ... nm)";
                
                foreach (var group in groupedRows)
                {
                    DataRow groupRow = dt.AsEnumerable().First(row => row["GroupID"].Equals(group.GroupId) && row[sensitiveIndex].Equals(group.SensitiveValue));

                    DataRow newRow = newTable.NewRow();
                    newRow.ItemArray = groupRow.ItemArray;
                    newRow["Frequency"] = group.Count;

                    newTable.Rows.Add(newRow);
                }

                var groupedNewTable = newTable.AsEnumerable()
                    .GroupBy(row => row["GroupID"])
                    .ToList();

                DataTable sortedNewTable = newTable.Clone();

                foreach (var group in groupedNewTable)
                {
                    var orderedGroupRows = group.OrderByDescending(row => row.Field<int>("Frequency")).ToList();
                    bool firstRow = true;
                    DataRow lastRow = null;
                    int maxFrequency = orderedGroupRows[0].Field<int>("Frequency");
                    for (int i = 0; i < orderedGroupRows.Count; i++)
                    {
                        var row = orderedGroupRows[i];
                        if (firstRow)
                        {
                            row["IsGroupStart"] = true;
                            firstRow = false;
                        }
                        else
                        {
                            row["IsGroupStart"] = false;
                        }
                        if (lastRow != null)
                        {
                            lastRow["IsGroupEnd"] = false;
                        }

                        int remainingTotalFrequency = orderedGroupRows.Skip(i).Sum(r => r.Field<int>("Frequency"));
                       
                        var cvalue = (double)maxFrequency / remainingTotalFrequency;
                        if (cvalue * 1000 != Math.Floor(cvalue * 1000))
                        {
                            cvalue = Math.Round(cvalue, 3, MidpointRounding.AwayFromZero);
                        }

                        row["c-value"] = cvalue;

                        lastRow = row;
                        sortedNewTable.ImportRow(row);
                    }
                    if (lastRow != null)
                    {
                        lastRow["IsGroupEnd"] = true;
                    }
                }
                table.ItemsSource = sortedNewTable.DefaultView;
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    DataRow newRow = newTable.NewRow();
                    newRow.ItemArray = row.ItemArray;
                    newRow["c-value"] = 1.0;
                    newRow["Frequency"] = 1;
                    newTable.Rows.Add(newRow);
                }

                tableMessage.Text = "Every row represents a single equivalence class right now. Please modify the data to ensure that multiple rows are identical with respect to the Quasi-Identifiers";

                table.ItemsSource = newTable.DefaultView;
            }

            for (int a = 0; a < labelComboboxes.Count; a++)
            {
                if (labelComboboxes[a].SelectedItem != null && labelComboboxes[a].SelectedItem.ToString() == "Identifier")
                {
                    CenterColumnData(a);
                }
            }


            CenterColumnDataByName("c-value");
        }





        private void CheckSelectedTab(TabControl tabControl)
        {
            var selectedTab = tabControl.SelectedItem as TabItem;

            if (selectedTab != null)
            {
                if (selectedTab.Header.ToString() == "Distinct l-Diversity")
                {
                    DistinctLDiversityTable();
                    ColumnKValueCollapsed();
                    OnDataTableChanged();
                }
                else if (selectedTab.Header.ToString() == "k-Anonymity")
                {


                    KAnonymityTable();
                    ColumnKValueVisible();
                    OnDataTableChanged();

                }
                else if (selectedTab.Header.ToString() == "t-Closeness")
                {
                    TClosenessTable();
                    ColumnKValueCollapsed();
                    OnDataTableChanged();

                }
                else if (selectedTab.Header.ToString() == "Entropy l-Diversity")
                {
                    EntropyLDiversityTable();
                    ColumnKValueCollapsed();
                    OnDataTableChanged();
                }
                else if (selectedTab.Header.ToString() == "Recursive (c,l)-Diversity")
                {

                    RecursiveLDiversityTable();
                    ColumnKValueCollapsed();
                    OnDataTableChanged();
                }
            }



            for (int a = 0; a < labelComboboxes.Count; a++)
            {
                if (labelComboboxes[a].SelectedItem != null && labelComboboxes[a].SelectedItem.ToString() == "Identifier")
                {
                    CenterColumnData(a);
                }
            }


        }


    }

}