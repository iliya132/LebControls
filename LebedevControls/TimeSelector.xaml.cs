using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LebedevControls
{
    public partial class TimeSelector : UserControl
    {
        private static DependencyProperty MinimumTimeSelectedProperty = DependencyProperty.Register("MinTimeSelected", typeof(TimeSpan), typeof(TimeSelector), new PropertyMetadata(new TimeSpan(9, 0, 0), OnSelectedTimeChanged));
        private static DependencyProperty MaximumTimeSelectedProperty = DependencyProperty.Register("MaxTimeSelected", typeof(TimeSpan), typeof(TimeSelector), new PropertyMetadata(new TimeSpan(9, 15, 0), OnSelectedTimeChanged));
        private static DependencyProperty StartTimeProperty = DependencyProperty.Register("StartTime", typeof(TimeSpan), typeof(TimeSelector), new PropertyMetadata(new TimeSpan(7, 0, 0), OnTimeLimitChanged));
        private static DependencyProperty EndTimeProperty = DependencyProperty.Register("EndTime", typeof(TimeSpan), typeof(TimeSelector), new PropertyMetadata(new TimeSpan(19, 15, 0), OnTimeLimitChanged));
        private static DependencyProperty BusyTimeProperty = DependencyProperty.Register("BusyTime", typeof(List<TimeSpan[]>), typeof(TimeSelector), new PropertyMetadata(null, OnBusyTimeChanged));

        private static void OnBusyTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeSelector control = (TimeSelector)d;
            if (e.NewValue != null)
            {
                control.UnselectAll();
                control.MarkBusyTime();
            }
        }

        public List<TimeSpan[]> BusyTime
        {
            get { return (List<TimeSpan[]>)GetValue(BusyTimeProperty); }
            set { SetValue(BusyTimeProperty, value); }
        }

        private static void OnTimeLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeSelector control = (TimeSelector)d;
            if (e.NewValue != null)
            {
                control.ClearGrid();
                control.NewInitializeGrid();
            }
        }

        private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeSelector control = (TimeSelector)d;
            if (e.NewValue != null)
            {

            }
        }

        public int Step = 15;
        public new double FontSize = 12;
        private TimeSpan InitBlockTimeStart;
        private TimeSpan InitBlockEnd;
        private TimeSpan EndBlockTimeStart;
        private TimeSpan EndBlockTimeEnd;
        private int currentHour = 0;
        private int currentMinute = 0;
        private SolidColorBrush selectBrush = new SolidColorBrush(Colors.Green);
        private SolidColorBrush busyBrush = new SolidColorBrush(Colors.Blue);
        private SolidColorBrush SelectedBusyBrush = new SolidColorBrush(Colors.Navy);

        private readonly List<TimedBlock> AllBlocks = new List<TimedBlock>();
        public TimeSpan Min
        {
            get { return (TimeSpan)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }
        public TimeSpan Max
        {
            get { return (TimeSpan)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }
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
            NewInitializeGrid();
        }

        private void ClearGrid()
        {
            while (MainGrid.Children.Count > 0)
            {
                MainGrid.Children.RemoveAt(0);
            }
        }
        private void NewInitializeGrid()
        {
            double columnsNeeded = (Max - Min).TotalMinutes / Step;
            TimeSpan _currentTime = Min;
            for (int i = 0; i < columnsNeeded + 2; i++)
            {
                AddColumn();
            }
            for(int i = 0; i < columnsNeeded; i++)
            {
                currentMinute = _currentTime.Minutes;
                TimedBlock timeBlock = new TimedBlock();
                timeBlock.start = _currentTime;
                timeBlock.end = _currentTime + new TimeSpan(0, Step, 0);
                AllBlocks.Add(timeBlock);
                timeBlock.Block = GenerateBlock();
                Grid.SetColumn(timeBlock.Block, i + 1);

                Border border = GenerateDefaultBorder();
                Grid.SetColumn(border, i+1);
                if (currentHour != _currentTime.Hours)
                {
                    currentHour = _currentTime.Hours;
                    Border borderBig = GenerateDefaultBorder();
                    borderBig.Height = 7.5;
                    borderBig.BorderThickness = new Thickness(0, 0, 1, 0);
                    TextBlock hourText = GenerateTextBlock(currentHour.ToString());
                    hourText.VerticalAlignment = VerticalAlignment.Top;
                    Grid.SetColumn(hourText, i);
                    Grid.SetColumnSpan(hourText, 2);
                    Grid.SetColumn(borderBig, i);
                    MainGrid.Children.Add(hourText);
                    MainGrid.Children.Add(borderBig);
                }
                if(i%2==0 || i == 0)
                {
                    TextBlock minutesText = GenerateTextBlock(currentMinute.ToString());
                    minutesText.VerticalAlignment = VerticalAlignment.Bottom;
                    minutesText.FontSize = FontSize > 4 ? FontSize - 4 : 4;
                    Grid.SetColumn(minutesText, i);
                    Grid.SetColumnSpan(minutesText, 2);
                    MainGrid.Children.Add(minutesText);
                }
                MainGrid.Children.Add(border);
                MainGrid.Children.Add(timeBlock.Block);
                _currentTime += new TimeSpan(0, Step, 0);
            }
            MarkBusyTime();
        }



        private Border GenerateDefaultBorder()
        {
            Border border = new Border();
            border.Height = 5;
            border.VerticalAlignment = VerticalAlignment.Bottom;
            border.Margin = new Thickness(0, 0, 0, 15);
            border.BorderThickness = new Thickness(0.5, 0, 0.5, 1);
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            return border;
        }

        private Grid GenerateBlock()
        {
            Grid grid = new Grid();
            grid.VerticalAlignment = VerticalAlignment.Bottom;
            Rectangle CollisionRectagle = new Rectangle();
            CollisionRectagle.MouseMove += Slider_MouseMove;
            CollisionRectagle.MouseDown += Slider_MouseDown;
            CollisionRectagle.Height = 60;
            CollisionRectagle.VerticalAlignment = VerticalAlignment.Bottom;
            Grid.SetZIndex(CollisionRectagle, 2);
            CollisionRectagle.Fill = new SolidColorBrush(Colors.Black);
            CollisionRectagle.Opacity = 0;
            Rectangle selectionRectangle = new Rectangle();
            selectionRectangle.Height = 20;
            selectionRectangle.Fill = selectBrush;
            selectionRectangle.StrokeThickness = 1;
            selectionRectangle.Opacity = 0.2;
            selectionRectangle.Visibility = Visibility.Hidden;
            selectionRectangle.VerticalAlignment = VerticalAlignment.Bottom;
            selectionRectangle.Margin = new Thickness(0, 0, 0, 15);
            grid.Children.Add(CollisionRectagle);
            grid.Children.Add(selectionRectangle);
            return grid;
        }

        private TextBlock GenerateTextBlock(string Text)
        {
            TextBlock newTextBlock = new TextBlock();
            newTextBlock.Text = Text;
            newTextBlock.TextAlignment = TextAlignment.Center;
            newTextBlock.FontSize = FontSize;
            newTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            return newTextBlock;
        }

        private void AddColumn()
        {
            ColumnDefinition column = new ColumnDefinition();
            MainGrid.ColumnDefinitions.Add(column);
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
                Rectangle rectangle = timeBlock.Block.Children[1] as Rectangle;
                rectangle.Fill = selectBrush;
                rectangle.Opacity = 0.2;
                rectangle.Visibility = Visibility.Hidden;
            }
            MarkBusyTime();
            Select(AllBlocks.Where(i => (i.start < MaxTimeSelected && i.start >= MinTimeSelected) ||
                                            (i.end <= MaxTimeSelected && i.end > MinTimeSelected)));
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
            Rectangle rectangle = block.Block.Children[1] as Rectangle;
            if(rectangle.Fill == busyBrush)
            {
                rectangle.Fill = SelectedBusyBrush;
                rectangle.Opacity = 0.4;
            } 
            rectangle.Visibility = Visibility.Visible;

        }

        private void Select(IEnumerable<TimedBlock> blocks) => blocks.ToList().ForEach(Select);

        private void MarkBusyTime()
        {
            if (BusyTime == null)
                return;
            foreach(TimeSpan[] item in BusyTime)
            {
                List<TimedBlock> blocks = AllBlocks.Where(i => (i.start < item[1] && i.start >= item[0]) ||
                                                                (i.end <= item[1] && i.end > item[0])).
                                                                ToList();

                foreach (TimedBlock block in blocks)
                {
                    Rectangle rectangle = block.Block.Children[1] as Rectangle;
                    rectangle.Fill = busyBrush;
                    rectangle.Visibility = Visibility.Visible;
                }
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
