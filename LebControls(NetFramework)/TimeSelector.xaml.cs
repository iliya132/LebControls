using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LebControls_NetFramework_
{
    public partial class TimeSelector : UserControl
    {
        private readonly DependencyProperty MinimumTimeSelectedProperty = DependencyProperty.Register("MinTimeSelected", typeof(TimeSpan), typeof(TimeSelector));
        private readonly DependencyProperty MaximumTimeSelectedProperty = DependencyProperty.Register("MaxTimeSelected", typeof(TimeSpan), typeof(TimeSelector));
        public TimeSpan StartTime = new TimeSpan(7, 0, 0);
        public TimeSpan EndTime = new TimeSpan(19, 15, 0);
        public int Step = 15;
        public new double FontSize = 12;
        private TimeSpan InitBlockTimeStart;
        private TimeSpan InitBlockEnd;
        private TimeSpan EndBlockTimeStart;
        private TimeSpan EndBlockTimeEnd;
        private int currentHour = 0;

        private readonly List<TimedBlock> AllBlocks = new List<TimedBlock>();
        public TimeSpan MinTimeSelected
        {
            get { return (TimeSpan)GetValue(MinimumTimeSelectedProperty); }
            set { SetValue(MinimumTimeSelectedProperty, value); }
        }
        public TimeSpan MaxTimeSelected
        {
            get { return (TimeSpan)GetValue(MaximumTimeSelectedProperty); }
            set { SetValue(MaximumTimeSelectedProperty, value); }
        }

        public TimeSelector()
        {
            InitializeComponent();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            double columnsNeeded = (EndTime - StartTime).TotalMinutes / Step;
            TimeSpan _currentTime = StartTime;
            for (int i = 0; i < columnsNeeded; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                MainGrid.ColumnDefinitions.Add(column);
                Border border = new Border();
                border.Height = 5;
                border.VerticalAlignment = VerticalAlignment.Bottom;
                border.Margin = new Thickness(0, 0, 0, 15);
                border.BorderThickness = new Thickness(1, 0, 0, 1);
                border.BorderBrush = new SolidColorBrush(Colors.Black);
                TimedBlock timeBlock = new TimedBlock();
                AllBlocks.Add(timeBlock);
                timeBlock.start = _currentTime;
                timeBlock.end = _currentTime + new TimeSpan(0, Step, 0);
                Grid grid = new Grid();
                timeBlock.Block = grid;
                Rectangle CollisionRectagle = new Rectangle();
                CollisionRectagle.MouseMove += Slider_MouseMove;
                CollisionRectagle.MouseDown += Slider_MouseDown;
                CollisionRectagle.Height = 60;
                CollisionRectagle.Tag = "CollisionRectagle";
                CollisionRectagle.VerticalAlignment = VerticalAlignment.Bottom;
                Grid.SetZIndex(CollisionRectagle, 1);
                CollisionRectagle.Fill = new SolidColorBrush(Colors.Red);
                CollisionRectagle.Opacity = 0;
                Rectangle selectionRectangle = new Rectangle();
                selectionRectangle.Height = 10;
                selectionRectangle.Fill = new SolidColorBrush(Colors.Blue);
                selectionRectangle.Opacity = 0.4;
                selectionRectangle.Visibility = Visibility.Hidden;
                selectionRectangle.VerticalAlignment = VerticalAlignment.Bottom;
                selectionRectangle.Margin = new Thickness(0, 0, 0, 16);
                grid.Children.Add(CollisionRectagle);
                grid.Children.Add(selectionRectangle);
                Grid.SetColumn(grid, i + 1);
                if (currentHour != _currentTime.Hours)
                {
                    currentHour = _currentTime.Hours;
                    TextBlock hourText = new TextBlock();
                    hourText.Text = _currentTime.ToString("hh");
                    hourText.TextAlignment = TextAlignment.Center;
                    hourText.FontSize = FontSize;
                    hourText.VerticalAlignment = VerticalAlignment.Top;
                    hourText.Margin = new Thickness(0, 10, 0, 0);
                    hourText.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetColumn(hourText, i == 0 ? 0 : i);
                    Grid.SetColumnSpan(hourText, 2);
                    MainGrid.Children.Add(hourText);
                    border.Height = 15;
                }

                if (i % 2 == 0 || i == 0)
                {
                    TextBlock text = new TextBlock();
                    text.Text = _currentTime.ToString("mm");
                    text.TextAlignment = TextAlignment.Center;
                    text.FontSize = FontSize;
                    text.VerticalAlignment = VerticalAlignment.Bottom;
                    Grid.SetColumn(text, i == 0 ? 0 : i);
                    Grid.SetColumnSpan(text, 2);
                    MainGrid.Children.Add(text);
                }
                if (i == columnsNeeded - 1)
                {
                    border.BorderThickness = new Thickness(1, 0, 1, 1);
                }
                Grid.SetColumn(border, i + 1);
                MainGrid.Children.Add(border);
                MainGrid.Children.Add(grid);
                _currentTime += new TimeSpan(0, Step, 0);

            }
            ColumnDefinition column2 = new ColumnDefinition();
            MainGrid.ColumnDefinitions.Add(column2);
        }

        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainGrid.Focus();
            Rectangle colidedRectangle = sender as Rectangle;
            TimedBlock colidedBlock = AllBlocks.FirstOrDefault(i => i.Block == (colidedRectangle.Parent as Grid));
            InitBlockTimeStart = colidedBlock.start;
            InitBlockEnd = colidedBlock.end;
            MinTimeSelected = colidedBlock.start;
            MaxTimeSelected = colidedBlock.end;
            UnselectAll();
            Select(colidedBlock);
        }

        private void UnselectAll()
        {
            foreach (TimedBlock timeBlock in AllBlocks)
            {
                timeBlock.Block.Children[1].Visibility = Visibility.Hidden;
            }
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Rectangle overRectangle = sender as Rectangle;
                TimedBlock currentBlock = AllBlocks.FirstOrDefault(i => i.Block == overRectangle.Parent as Grid);
                EndBlockTimeStart = currentBlock.start;
                EndBlockTimeEnd = currentBlock.end;
                MinTimeSelected = EndBlockTimeStart < InitBlockTimeStart ? EndBlockTimeStart : InitBlockTimeStart;
                MaxTimeSelected = EndBlockTimeEnd > InitBlockEnd ? EndBlockTimeEnd : InitBlockEnd;
                UnselectAll();
                Select(AllBlocks.Where(b => b.start >= MinTimeSelected && b.end <= MaxTimeSelected));
            }
        }

        private void Select(TimedBlock block)
        {
            block.Block.Children[1].Visibility = Visibility.Visible;
        }

        private void Select(IEnumerable<TimedBlock> blocks)
        {
            foreach (TimedBlock block in blocks)
            {
                block.Block.Children[1].Visibility = Visibility.Visible;
            }
        }

        private class TimedBlock
        {
            internal TimeSpan start { get; set; }
            internal TimeSpan end { get; set; }
            internal Grid Block { get; set; }
        }

        private void OffGrid_LostFocus(object sender, RoutedEventArgs e) => UnselectAll();
    }
}
