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

namespace CrypTool.Plugins.Anonymity
{
    /// <summary>
    /// Interaction logic for AnonymityPresentation.xaml
    /// </summary>
    public partial class AnonymityPresentation : UserControl
    {

        private List<ComboBox> hiddenComboboxes = new List<ComboBox>();
        private List<ComboBox> labelComboboxes = new List<ComboBox>();
        private bool isDragging = false;
        private Point startPoint;
        private DataTable initialTable;
        private List<Button> _buttonList = new List<Button>();







        public AnonymityPresentation()
        {
            InitializeComponent();
        }



        public void ClearComboboxList()
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



            foreach (StackPanel child in categoricNumeric.Children.OfType<StackPanel>())
            {
                child.Children.Clear();
            }

            categoricNumeric.Children.Clear();

        }


        public void CreateDataTable(string InputCsv, string RowSeperator, string ColumnSeperator)
        {
            DataTable dt = new DataTable();

            // split the input string into an array of lines

            string[] lines = InputCsv.Split(new[] { "\r\n", RowSeperator }, StringSplitOptions.RemoveEmptyEntries);


            // split the first line into an array of headers
            string[] headers = lines[0].Split(ColumnSeperator.ToArray(), StringSplitOptions.RemoveEmptyEntries);




            // add columns to the table
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }



            // create labels and combobox according to the amount of columns
            foreach (string headervalue in headers)
            {
                Label label = new Label();
                label.Content = headervalue;
                ComboBox comboBox = new ComboBox();
                comboBox.Items.Add("Identifier");
                comboBox.Items.Add("Quasi-Identifier");
                comboBox.Items.Add("Sensitive Attribute");
                comboBox.SelectionChanged += ComboBoxSelectionQuasiIdentifier;
                comboBox.Margin = new Thickness(5);
                headerlabels.Children.Add(label);
                labelComboboxes.Add(comboBox);
                headerlabels.Children.Add(comboBox);

            }


            // create comboboxes with the items categoric and numeric
            foreach (string combo in headers)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.Visibility = Visibility.Hidden;
                comboBox.Items.Add("Categoric");
                comboBox.Items.Add("Numeric");
                comboBox.Margin = new Thickness(0, 33.5, 0, 0);
                comboBox.Width = 100;
                categoricNumeric.Children.Add(comboBox);
                hiddenComboboxes.Add(comboBox);
            }




            // add data rows to the table
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(ColumnSeperator.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                dt.Rows.Add(values);

            }



            Button applyButton = new Button();
            applyButton.Width = 100;
            applyButton.HorizontalAlignment = HorizontalAlignment.Center;
            applyButton.Background = new SolidColorBrush(Colors.SkyBlue);
            applyButton.Content = "Apply";
            applyButton.Foreground = new SolidColorBrush(Colors.White);
            applyButton.Margin = new Thickness(0, 10, 0, 0);
            applyButton.Click += new RoutedEventHandler(Apply_Click);
            _buttonList.Add(applyButton);
            headerlabels.Children.Add(applyButton);




            // bind the DataTable to a DataGrid
            table.ItemsSource = dt.DefaultView;
            table.CanUserSortColumns = false;
            table.CanUserAddRows = false;
            table.CanUserReorderColumns = false;
            table.CanUserResizeColumns = false;
            table.CanUserAddRows = false;
            table.CanUserDeleteRows = false;
            initialTable = dt.Copy();




            resetButton.Click += ResetButton_Click;

        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ComboBox comboBox in hiddenComboboxes)
            {
                comboBox.SelectedItem = null;
                comboBox.Visibility = Visibility.Hidden;
            }

            foreach (ComboBox comboBox in labelComboboxes)
            {
                comboBox.SelectedItem = null;

            }

            functionContainer.Children.Clear();
            foreach (Button btn in _buttonList)
            {
                btn.IsEnabled = true;
            }
            DataTable currentTable = initialTable.Copy();
            table.ItemsSource = currentTable.DefaultView;
        }

        private string ProcessEscapeSymbols(string p)
        {
            return p.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\b", "\b").Replace("\\t", "\t").Replace("\\v", "\v").Replace("\\", "\\");
        }



        // if combobox next to the column headers is set to Quasi-Identifier, the comboboxes with the items categoric and numeric appear
        private void ComboBoxSelectionQuasiIdentifier(object sender, SelectionChangedEventArgs e)
        {

            ComboBox combobox = sender as ComboBox;
            int selectedIndex = labelComboboxes.IndexOf(combobox);
            if (combobox.SelectedItem != null && combobox.SelectedItem.ToString() == "Quasi-Identifier")
            {
                hiddenComboboxes[selectedIndex].Visibility = Visibility.Visible;
            }

            else
            {
                hiddenComboboxes[selectedIndex].Visibility = Visibility.Hidden;
            }


        }
        // code to execute when the apply button is clicked
        private void Apply_Click(object sender, RoutedEventArgs e)
        {

            Button btn = sender as Button;


            if (CheckInput() == true)
            {
                ColumnVisibility();
                GenerateCategoricItems();
                GenerateNumericItems();
                btn.IsEnabled = false;
                CalculateKValue();
            }
            else
            {
                Console.WriteLine("Check Input is NOT True");
            }

        }



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
                    canvas.Margin = new Thickness(0, 5, 0, 0);
                    var margin = 5;
                    canvas.Background = Brushes.Gray;
                    canvas.Height = 55;
                    canvas.HorizontalAlignment = HorizontalAlignment.Left;
                    canvas.VerticalAlignment = VerticalAlignment.Top;
                    distinctValues.Sort((a, b) => int.Parse(a).CompareTo(int.Parse(b)));
                    var textblock = new TextBlock(); // textblock to display cell value
                    textblock.FontWeight = FontWeights.DemiBold;
                    textblock.FontSize = 14.0;
                    textblock.Foreground = Brushes.Black;
                    textblock.Margin = new Thickness(0, 10, 10, 10);
                    textblock.Text = "Numeric Column: " + table.Columns[i].Header;
                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    functionContainer.Children.Add(textblock);






                    foreach (var col in distinctValues)
                    {
                        var numTextblock = new TextBlock();
                        numTextblock.FontWeight = FontWeights.DemiBold;
                        numTextblock.FontSize = 14.0;
                        numTextblock.Foreground = Brushes.Black;

                        numTextblock.VerticalAlignment = VerticalAlignment.Center;
                        numTextblock.Text = col;
                        numTextblock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        double textBlockHeight = numTextblock.DesiredSize.Height;
                        double topMargin = (canvas.Height - textBlockHeight) / 2;
                        Canvas.SetLeft(numTextblock, margin + 10);
                        Canvas.SetTop(numTextblock, topMargin);
                        canvas.Children.Add(numTextblock);
                        margin += 100;

                    }


                    canvas.Width = margin + 100;
                    stackPanel.Children.Add(canvas);
                    functionContainer.Children.Add(stackPanel);

                    var rangeStackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                    var rangeTextBlock = new TextBlock();
                    rangeTextBlock.FontWeight = FontWeights.DemiBold;
                    rangeTextBlock.FontSize = 14.0;
                    rangeTextBlock.Foreground = Brushes.Black;
                    rangeTextBlock.Margin = new Thickness(0, 10, 10, 10);
                    rangeTextBlock.Text = "Split the values in ranges ";
                    rangeStackPanel.Children.Add(rangeTextBlock);


                    var rangeComboBox = new ComboBox();
                    rangeComboBox.Margin = new Thickness(0, 10, 10, 10);
                    rangeComboBox.Width = 150;

                    rangeStackPanel.Children.Add(rangeComboBox);

                    // Populate the combobox with numbers

                    for (int j = 2; j < distinctValues.Count; j++)
                    {
                        rangeComboBox.Items.Add(j);
                    }

                    var btn = new Button { Content = "Group", Background = Brushes.SkyBlue, FontSize = 14, Margin = new Thickness(10, 0, 0, 0), Foreground = Brushes.White, Width = 60, Height = 30 };
                    btn.Click += (s, evt) => NumericGroupBtnClick(s, evt, canvas, index, rangeComboBox);
                    var debtn = new Button { Content = "Degroup", Background = Brushes.MediumPurple, FontSize = 14, Margin = new Thickness(10, 0, 0, 0), Foreground = Brushes.White, Width = 60, Height = 30 };
                    debtn.Click += (s, evt) => NumericDegroupBtnClick(s, evt, btn, rangeComboBox, canvas, index);
                    rangeStackPanel.Children.Add(btn);
                    rangeStackPanel.Children.Add(debtn);
                    rangeComboBox.SelectionChanged += (sender, e) =>
                    {

                        if (rangeComboBox.SelectedItem != null)
                        {
                            int selectedValue = int.Parse(rangeComboBox.SelectedItem.ToString());
                            int redBarCount = selectedValue - 1;



                            // Remove any existing redbars from the canvas
                            var existingRedBars = canvas.Children.OfType<Rectangle>().ToList();
                            foreach (var redBar in existingRedBars)
                            {
                                canvas.Children.Remove(redBar);
                            }

                            // Add the new red bars
                            double initialLeftPosition = 8;
                            for (int j = 0; j < redBarCount; j++)
                            {
                                var redBar = CreateRedBar(canvas, initialLeftPosition);
                                Canvas.SetLeft(redBar, initialLeftPosition);
                                Canvas.SetTop(redBar, 10);
                                canvas.Children.Add(redBar);
                                initialLeftPosition += 100;
                            }
                        }
                    };
                    functionContainer.Children.Add(rangeStackPanel);
                }
            }
        }

        private void NumericDegroupBtnClick(object s, RoutedEventArgs evt, Button button, ComboBox comboBox, Canvas canvas, int columnIndex)
        {


            List<Rectangle> rectanglesToRemove = new List<Rectangle>();
            button.IsEnabled = true;
            comboBox.IsEnabled = true;
            comboBox.SelectedItem = null;
            foreach (UIElement element in canvas.Children)
            {
                if (element is Rectangle rectangle)
                {
                    rectanglesToRemove.Add(rectangle);
                }
            }

            foreach (Rectangle rectangle in rectanglesToRemove)
            {
                canvas.Children.Remove(rectangle);
            }

            DataTable currentTable = (table.ItemsSource as DataView).ToTable();

            for (int rowIndex = 0; rowIndex < initialTable.Rows.Count; rowIndex++)
            {
                currentTable.Rows[rowIndex][columnIndex] = initialTable.Rows[rowIndex][columnIndex];
            }

            table.ItemsSource = currentTable.DefaultView;
            ColumnVisibility();

            CalculateKValue();

        }





        private string GetRangeForValue(int value, List<(int First, int Last)> ranges)
        {
            foreach (var range in ranges)
            {
                if (value >= range.First && value <= range.Last)
                {
                    return $"[{range.First} - {range.Last}]";
                }
            }
            return null;
        }



        private bool IsSingleValueRange(string range)
        {
            var regex = new Regex(@"\[(\d+) - (\d+)\]");
            var match = regex.Match(range);

            if (match.Success)
            {
                int first = int.Parse(match.Groups[1].Value);
                int last = int.Parse(match.Groups[2].Value);

                return first == last;
            }

            return false;
        }






        private void NumericGroupBtnClick(object sender, RoutedEventArgs e, Canvas canvas, int columnIndex, ComboBox combo)
        {
            canvas.Children.ReorderElementsByLeftPosition();

            var btn = sender as Button;

            List<TextBlock> leftTextBlocks = new List<TextBlock>();
            List<TextBlock> rightTextBlocks = new List<TextBlock>();
            int rectanglesCount = 0;

            List<(int First, int Last)> leftRanges = new List<(int First, int Last)>();
            List<(int First, int Last)> rightRanges = new List<(int First, int Last)>();

            for (int i = 0; i < canvas.Children.Count; i++)
            {
                UIElement child = canvas.Children[i];
                if (child is TextBlock textBlock)
                {
                    if (rectanglesCount > 0)
                        rightTextBlocks.Add(textBlock);
                    else
                        leftTextBlocks.Add(textBlock);
                }
                else if (child is Rectangle)
                {
                    rectanglesCount++;

                    if (leftTextBlocks.Count > 0)
                    {
                        leftRanges.Add((int.Parse(leftTextBlocks.First().Text), int.Parse(leftTextBlocks.Last().Text)));
                        leftTextBlocks.Clear();
                    }

                    if (rectanglesCount > 1)
                    {
                        rightRanges.Add((int.Parse(rightTextBlocks.First().Text), int.Parse(rightTextBlocks.Last().Text)));
                        rightTextBlocks.Clear();
                    }
                }
            }

            if (rightTextBlocks.Count > 0)
            {
                rightRanges.Add((int.Parse(rightTextBlocks.First().Text), int.Parse(rightTextBlocks.Last().Text)));
            }

            var rows = table.Items.Cast<DataRowView>().ToList(); // get all rows from the datagrid
            foreach (var row in rows)
            {
                var cellValueStr = row.Row[columnIndex].ToString(); // get the value of the cell in the column
                int.TryParse(cellValueStr, out int cellValue);

                string leftRange = GetRangeForValue(cellValue, leftRanges);
                string rightRange = GetRangeForValue(cellValue, rightRanges);

                if (leftRange != null)
                {
                    if (IsSingleValueRange(leftRange))
                        row.Row[columnIndex] = cellValueStr;
                    else
                        row.Row[columnIndex] = leftRange;
                }
                else if (rightRange != null)
                {
                    if (IsSingleValueRange(rightRange))
                        row.Row[columnIndex] = cellValueStr;
                    else
                        row.Row[columnIndex] = rightRange;
                }
            }

            if (combo.SelectedItem != null)
            {
                combo.IsEnabled = false;
                btn.IsEnabled = false;
            }
            CalculateKValue();
        }




        private Rectangle CreateRedBar(Canvas canvas, double initialLeftPosition)
        {
            var redBar = new Rectangle
            {
                Width = 5,
                Height = 55 - 20,
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Fill = Brushes.Red,
                Cursor = Cursors.Hand
            };

            redBar.PreviewMouseLeftButtonDown += (sender, e) =>
            {
                redBar.CaptureMouse();
                e.Handled = true;
            };

            redBar.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                redBar.ReleaseMouseCapture();


            };

            redBar.PreviewMouseMove += (sender, e) =>
            {
                if (redBar.IsMouseCaptured)
                {
                    var position = e.GetPosition(canvas);
                    var newLeft = Math.Max(0, Math.Min(position.X - (redBar.Width / 2), canvas.ActualWidth - redBar.Width));

                    var textBlocks = canvas.Children.OfType<TextBlock>().ToList();
                    var canDrop = false;
                    const double sideDropTolerance = 5.0;

                    foreach (var textBlock in textBlocks)
                    {
                        double textBlockLeft = Canvas.GetLeft(textBlock) - 10;
                        double textBlockRight = textBlockLeft + textBlock.ActualWidth + 10;

                        if (newLeft >= textBlockLeft - redBar.Width - sideDropTolerance &&
                            newLeft <= textBlockLeft - redBar.Width + sideDropTolerance)
                        {
                            canDrop = true;
                            newLeft = textBlockLeft - redBar.Width;
                            break;
                        }
                        else if (newLeft >= textBlockRight - sideDropTolerance &&
                                 newLeft <= textBlockRight + sideDropTolerance)
                        {
                            canDrop = true;
                            newLeft = textBlockRight;
                            break;
                        }
                    }

                    if (canDrop)
                    {
                        Canvas.SetLeft(redBar, newLeft);
                    }
                }
            };

            return redBar;
        }

        private void GenerateCategoricItems()
        {

            for (int i = 0; i < hiddenComboboxes.Count(); i++)
            {
                if (hiddenComboboxes[i].SelectedItem != null && hiddenComboboxes[i].SelectedItem.ToString() == "Categoric" && hiddenComboboxes[i].Visibility == Visibility.Visible)
                {

                    int index = i;

                    List<string> distinctValues = table.Items.Cast<DataRowView>()
                                    .Select(row => row.Row.ItemArray[i].ToString())
                                    .Distinct().ToList();

                    var canvas = new Canvas();
                    var margin = 0.0;
                    canvas.Background = Brushes.Gray;
                    canvas.Height = 55;
                    canvas.HorizontalAlignment = HorizontalAlignment.Left;
                    canvas.VerticalAlignment = VerticalAlignment.Top;
                    var textblock = new TextBlock();
                    textblock.FontWeight = FontWeights.DemiBold;
                    textblock.FontSize = 14.0;
                    textblock.Foreground = Brushes.Black;
                    textblock.Margin = new Thickness(0, 10, 10, 10);
                    textblock.Text = "Categoric Column: " + table.Columns[i].Header;

                    foreach (var col in distinctValues)
                    {
                        var border = new Border();
                        var converter = new BrushConverter();
                        var brush = (Brush)converter.ConvertFromString("#3F48CC");
                        border.Background = brush;
                        border.Tag = col;
                        border.Cursor = Cursors.Hand;
                        border.CornerRadius = new CornerRadius(5);
                        border.Padding = new Thickness(10);
                        border.Child = new TextBlock() { Text = col, Foreground = Brushes.White, FontSize = 14 };

                        // events to handle border drag and drop
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
                                double deltaX = currentPosition.X - startPoint.X;
                                double deltaY = currentPosition.Y - startPoint.Y;
                                double newLeft = Canvas.GetLeft(border) + deltaX;
                                double newTop = Canvas.GetTop(border) + deltaY;
                                if (newLeft >= 0 && newLeft <= canvas.ActualWidth - border.ActualWidth && newTop >= 0 && newTop <= canvas.ActualHeight - border.ActualHeight)
                                {

                                    Canvas.SetLeft(border, newLeft);
                                    Canvas.SetTop(border, newTop);

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
                        };
                        Canvas.SetLeft(border, margin);
                        Canvas.SetTop(border, 0);
                        canvas.Children.Add(border);
                        margin += 100;
                    }
                    canvas.Width = margin + 100;
                    var btn = new Button { Content = "Group", Background = Brushes.SkyBlue, FontSize = 14, Margin = new Thickness(10, 0, 0, 0), Foreground = Brushes.White, Width = 60, Height = 30 };
                    var btn2 = new Button { Content = "Degroup", Background = Brushes.MediumPurple, FontSize = 14, Margin = new Thickness(10, 0, 0, 0), Foreground = Brushes.White, Width = 60, Height = 30 };
                    btn.Click += (s, e) => CategoricGroupBtnClick(s, e, index);
                    btn2.Click += (s, e) => CategoricDeGroupBtnClick(s, e, index);
                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    stackPanel.Children.Add(canvas);
                    stackPanel.Children.Add(btn);
                    stackPanel.Children.Add(btn2);
                    stackPanel.Children.Add(new ListView { VerticalAlignment = VerticalAlignment.Center });
                    functionContainer.Children.Add(textblock);
                    functionContainer.Children.Add(stackPanel);
                }
            }
            CalculateKValue();
        }

        private void CategoricDeGroupBtnClick(object sender, RoutedEventArgs e, int columnIndex)
        {
            var btn = sender as Button;
            var container = btn.Parent as StackPanel;
            var canvas = container.Children[0] as Canvas;
            var groupBtn = container.Children[1] as Button;
            groupBtn.IsEnabled = true;
            canvas.Children.Clear();
            DataTable currentTable = (table.ItemsSource as DataView).ToTable();

            for (int rowIndex = 0; rowIndex < initialTable.Rows.Count; rowIndex++)
            {
                currentTable.Rows[rowIndex][columnIndex] = initialTable.Rows[rowIndex][columnIndex];
            }

            table.ItemsSource = currentTable.DefaultView;
            ColumnVisibility();
            List<string> distinctValues = table.Items.Cast<DataRowView>()
                                    .Select(row => row.Row.ItemArray[columnIndex].ToString())
                                    .Distinct().ToList();
            var margin = 0.0;

            foreach (var col in distinctValues)
            {
                var border = new Border();
                var converter = new BrushConverter();
                var brush = (Brush)converter.ConvertFromString("#3F48CC");
                border.Background = brush;
                border.Tag = col;
                border.Cursor = Cursors.Hand;
                border.CornerRadius = new CornerRadius(5);
                border.Padding = new Thickness(10);
                border.Child = new TextBlock() { Text = col, Foreground = Brushes.White, FontSize = 14 };

                border.MouseLeftButtonDown += (senderObject, ev) =>
                {
                    var element = senderObject as UIElement;
                    if (element != null)
                    {
                        element.CaptureMouse();
                        isDragging = true;
                        startPoint = ev.GetPosition(canvas);
                    }
                };

                border.MouseMove += (senderObject, ev) =>
                {
                    if (isDragging)
                    {
                        Point currentPosition = ev.GetPosition(canvas);
                        double deltaX = currentPosition.X - startPoint.X;
                        double deltaY = currentPosition.Y - startPoint.Y;
                        double newLeft = Canvas.GetLeft(border) + deltaX;
                        double newTop = Canvas.GetTop(border) + deltaY;
                        if (newLeft >= 0 && newLeft <= canvas.ActualWidth - border.ActualWidth && newTop >= 0 && newTop <= canvas.ActualHeight - border.ActualHeight)
                        {
                            // Set the new position of the border
                            Canvas.SetLeft(border, newLeft);
                            Canvas.SetTop(border, newTop);
                            // Update the click position
                            startPoint = currentPosition;
                        }
                    }
                };
                border.MouseLeftButtonUp += (senderObject, ev) =>
                {
                    var element = senderObject as UIElement;
                    if (element != null)
                    {
                        element.ReleaseMouseCapture();
                        isDragging = false;
                    }
                };
                Canvas.SetLeft(border, margin);
                Canvas.SetTop(border, 0);
                canvas.Children.Add(border);
                margin += 100;
            }
            CalculateKValue();

        }

        private void CategoricGroupBtnClick(object sender, RoutedEventArgs e, int index)
        {

            var btn = sender as Button;
            var container = btn.Parent as StackPanel;
            var canvas = container.Children[0] as Canvas;
            var groups = new List<List<string>>();
            List<string> categoricOutput = new List<string>();
            // loop over canvas children and find UI elements that intersect
            for (int i = 0; i < canvas.Children.Count; ++i)
            {
                var item1 = canvas.Children[i] as Border;
                var currentGroup = new List<string>();
                currentGroup.Add((string)item1.Tag);

                for (int j = i + 1; j < canvas.Children.Count; ++j)
                {
                    var item2 = canvas.Children[j] as Border;

                    var point1 = item1.TranslatePoint(new Point(0, 0), canvas);
                    var point2 = item2.TranslatePoint(new Point(0, 0), canvas);

                    Rect rect1 = new Rect(point1.X, point1.Y, item1.RenderSize.Width, item1.RenderSize.Height);
                    Rect rect2 = new Rect(point2.X, point2.Y, item2.RenderSize.Width, item2.RenderSize.Height);
                    if (rect1.IntersectsWith(rect2))
                    {
                        currentGroup.Add((string)item2.Tag);
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
            foreach (var group in groups)
            {

                var line = string.Join(" | ", group);
                categoricOutput.Add(line);

            }

            int columnIndex = index;
            Console.WriteLine("Column Index: " + columnIndex);
            var rows = table.Items.Cast<DataRowView>().ToList(); // get all rows from the datagrid
            foreach (var row in rows)
            {

                var cellValue = row.Row[columnIndex].ToString(); // get the value of the cell in the column
                Console.WriteLine("Cell value: " + cellValue);
                foreach (var value in categoricOutput)
                {

                    if (value != null && value.Contains(cellValue) && cellValue != "|" && cellValue != "")
                    {
                        row.Row[columnIndex] = value; // set the new value for the cell in the column
                    }
                    else
                    {
                        row.Row[columnIndex] = cellValue;

                    }


                }

                btn.Opacity = 0.9;
                btn.IsEnabled = false;

            }
            CalculateKValue();
        }

        private bool CheckInput()
        {
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem == null)
                {
                    Console.WriteLine("All comboboxes have to be selected");
                    return false;
                }
                else if (labelComboboxes[i].SelectedItem.ToString() == "Quasi-Identifier" && hiddenComboboxes[i].SelectedItem == null)
                {
                    Console.WriteLine("Quasi-Identifier have to be either numeric or categoric");
                    return false;
                }
            }
            return true;
        }
        private void ColumnVisibility()
        {
            for (int i = 0; i < labelComboboxes.Count; i++)
            {
                if (labelComboboxes[i].SelectedItem != null && labelComboboxes[i].SelectedItem.ToString() == "Identifier")
                {
                    table.Columns[i].Visibility = Visibility.Collapsed;
                }
                else
                {
                    table.Columns[i].Visibility = Visibility.Visible;
                }
            }

            foreach (DataGridColumn column in table.Columns)
            {
                if (column.Header != null && column.Header.ToString() == "GroupID")
                {
                    column.Visibility = Visibility.Collapsed;
                    break;
                }
            }
        }


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

            // Group rows based on the quasi-identifier column values
            DataTable currentTable = (table.ItemsSource as DataView).ToTable();
            var groupedRows = currentTable.AsEnumerable()
                .GroupBy(row => string.Join("|", quasiIdentifierIndexes.Select(index => row[index])))
                .ToList();

            // Find the group with the least number of rows
            int minGroupSize = groupedRows.Min(group => group.Count());

            // Update kLabel
            kLabel.Content = minGroupSize.ToString() + " Anonymity";

            // Create a new DataTable with the same schema and add grouped rows
            DataTable newTable = currentTable.Clone();

            // Check if the GroupID column already exists, if not, add it
            if (!newTable.Columns.Contains("GroupID"))
            {
                newTable.Columns.Add("GroupID", typeof(int));
            }

            int groupID = 0;
            foreach (var group in groupedRows)
            {
                foreach (var row in group)
                {
                    // Create a new row with the same item array and additional GroupID column
                    DataRow newRow = newTable.NewRow();
                    newRow.ItemArray = row.ItemArray;
                    newRow["GroupID"] = groupID;
                    newTable.Rows.Add(newRow);
                }
                groupID++;
            }

            // Set the DataGrid's ItemsSource to the new table
            table.ItemsSource = newTable.DefaultView;
            ColumnVisibility();
        }






    }

}
