using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BugsBandApp
{
    class Food
    {
        private Grid grid;
        private Button bBase;
        private Point currentPosition;        
        private Point basePosition;
        private const int foodHeight = 16;
        private const int foodWidth = 16;
        private Button item;
        private List<Food> foodList;
        public List<Food> FoodList
        {
            set { foodList = value; }
        }

        public Button Item
        {
            get { return item; }
        }
        public Point CurrentPosition
        {
            get { return currentPosition; }
        }        

        public Food(MainWindow window)
        {
            grid = window.gMainGrid;
            bBase = window.bBase;            
            SetFood();

            item.Click += Delete;
        }

        public void SetFood()
        {
            currentPosition = Mouse.GetPosition(grid);
            basePosition = bBase.TransformToAncestor(grid).Transform(new Point(0, 0));

            if (currentPosition.X > (basePosition.X - foodHeight) &&
                currentPosition.X < (basePosition.X + bBase.Height) &&
                currentPosition.Y > (basePosition.Y - foodWidth) &&
                currentPosition.Y < (basePosition.Y + bBase.Width)
                )
            {
                return;
            }
            else
            {
                item = new Button();
                item.Height = foodHeight;
                item.Width = foodWidth;
                item.Background = Brushes.Green;
                item.HorizontalAlignment = HorizontalAlignment.Left;
                item.VerticalAlignment = VerticalAlignment.Top;
                item.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
                grid.Children.Add(item);
            }
        }

        private void Delete(object sender, EventArgs e)
        {            
            grid.Children.Remove(item);
            foodList.Remove(this);
        }

    }
}
