using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BugsBandApp
{
    class Worker
    {
        private WorkerUnit item;
        private Point currentPosition;
        private Grid grid;
        private double gridWidth;
        private double gridHeight;
        private Button bBase;
        private Point basePosition;

        private DispatcherTimer timer;        
        private Random rnd;
        private const double rndStep = 5;
        private const double stepToFood = 10;
        private const int timerInterval = 50;

        private List<Food> foodList;
        public List<Food> FoodList
        {            
            set
            {
                foodList = value;
                FindNearestFood();
            }
        }
        private Food nearestFood;
        private Point nearestFoodPos;
        private double nearestFoodDist;
        private const double pieceToEat = 0.2;
        private List<Worker> workerList;
        public List<Worker> WorkerList
        {
            set { workerList = value; }
        }

        public WorkerUnit Item
        {
            get { return item; }
        }
        public Point CurrentPosition
        {
            get { return currentPosition; }
        }

        public Worker(MainWindow window)
        {                     
            grid = window.gMainGrid;
            gridWidth = window.gMainGrid.Width;
            gridHeight = window.gMainGrid.Height;
            bBase = window.bBase;
            basePosition = bBase.TransformToAncestor(grid).Transform(new Point(0, 0));            

            item = new WorkerUnit();            
            currentPosition.X = gridWidth/2 - item.Width/2;
            currentPosition.Y = gridHeight/2 - bBase.Height/2 - item.Height;
            item.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
            grid.Children.Add(item);            

            rnd = new Random();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            timer.Tick += Timer_Tick;
            timer.Start();

            item.MouseEnter += StopTimer;
            item.MouseLeave += StartTimer;
            item.Click += Delete;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            while (foodList.Count == 0)
            {
                MoveRandomly(null);
                return;
            }
            
            if (nearestFood.Item.Height == 0)
            {
                foodList.Remove(nearestFood);                
                FindNearestFood();
            }                
            else if (currentPosition.X != nearestFood.CurrentPosition.X)
            {                
                MoveToFood();
            }              
            else if (nearestFood.Item.Height < pieceToEat)
            {
                nearestFood.Item.Height = 0;
                nearestFood.Item.Width = 0;
            }
            else
            {
                nearestFood.Item.Height -= pieceToEat;
                nearestFood.Item.Width -= pieceToEat;
            }
        }

        private int GetRandomDirection()
        {
            int rd;
            rd = rnd.Next(-1, 2);
            return rd;
        }

        private void MoveRandomly(object state)
        {            
            double newPositionX = currentPosition.X + GetRandomDirection()*rndStep;
            if (
                newPositionX > 0 &&
                newPositionX < (gridHeight - item.Height) && // Why does it go under the bottom line?!
                !(
                newPositionX > (basePosition.X - item.Height) &&
                newPositionX < (basePosition.X + bBase.Height) &&
                currentPosition.Y > (basePosition.Y - item.Width) &&
                currentPosition.Y < (basePosition.Y + bBase.Width)
                )
               )
            {                
                currentPosition.X = newPositionX;
            }
            
            double newPositionY = currentPosition.Y + GetRandomDirection()*rndStep;
            if (
                newPositionY > 0 &&
                newPositionY < (gridWidth - item.Width) &&
                !(
                currentPosition.X > (basePosition.X - item.Height) &&
                currentPosition.X < (basePosition.X + bBase.Height) &&
                newPositionY > (basePosition.Y - item.Width) &&
                newPositionY < (basePosition.Y + bBase.Width)
                )                
               )
            {                
                currentPosition.Y = newPositionY;
            }            
            
            item.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);            
        }              

        public void FindNearestFood()
        {
            if (foodList.Count != 0)
            {
                Food cf;
                Food nf;                
                Point cfPos;
                Point nfPos;
                double nDist;
                double cDist;
                cf = foodList[0];
                cfPos = cf.Item.TransformToAncestor(grid).Transform(new Point(0, 0));
                cDist = Math.Sqrt( Math.Pow((cfPos.X-currentPosition.X),2) + Math.Pow((cfPos.Y-currentPosition.Y),2) );
                nf = cf;
                nfPos = cfPos;
                nDist = cDist;
                foreach (Food f in foodList)
                {
                    cf = f;
                    cfPos = f.Item.TransformToAncestor(grid).Transform(new Point(0, 0));
                    cDist = Math.Sqrt(Math.Pow((cfPos.X - currentPosition.X), 2) + Math.Pow((cfPos.Y - currentPosition.Y), 2));
                    if (cDist < nDist)
                    {
                        nf = cf;
                        nfPos = cfPos;
                        nDist = cDist;
                    }
                }
                nearestFood = nf;
                nearestFoodPos = nfPos;
                nearestFoodDist = nDist;
            }                        
        }

        public void MoveToFood()
        {
            if ((Math.Abs(currentPosition.X - nearestFood.CurrentPosition.X) < stepToFood) &&
                (Math.Abs(currentPosition.Y - nearestFood.CurrentPosition.Y) < stepToFood) )
            {
                currentPosition.X = nearestFood.CurrentPosition.X;
                currentPosition.Y = nearestFood.CurrentPosition.Y;
                item.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
                FindNearestFood();
                return;
            }

            if (currentPosition.X > nearestFood.CurrentPosition.X)
            {
                currentPosition.X -= stepToFood * (currentPosition.X - nearestFood.CurrentPosition.X) / nearestFoodDist;
            }
            else
            {
                currentPosition.X += stepToFood * (nearestFood.CurrentPosition.X - currentPosition.X) / nearestFoodDist;
            }
            
            if (currentPosition.Y > nearestFood.CurrentPosition.Y)
            {
                currentPosition.Y -= stepToFood * (currentPosition.Y - nearestFood.CurrentPosition.Y) / nearestFoodDist;
            }
            else
            {
                currentPosition.Y += stepToFood * (nearestFood.CurrentPosition.Y - currentPosition.Y) / nearestFoodDist;
            }

            item.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
            FindNearestFood();
        }

        public void StopTimer(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void StartTimer(object sender, EventArgs e)
        {
            timer.Start();
        }

        private void Delete(object sender, EventArgs e)
        {            
            timer = null;
            item.MouseLeave -= StartTimer;
            grid.Children.Remove(item);
            workerList.Remove(this);
        }
    }

    class WorkerUnit : Button
    {
        private const int height = 10;
        private const int width = 10;

        public WorkerUnit() 
        {
            this.Height = height;
            this.Width = width;
            this.Background = Brushes.Yellow;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
        }
    }
}
