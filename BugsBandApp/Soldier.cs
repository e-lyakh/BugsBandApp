using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;

namespace BugsBandApp
{
    class Soldier
    {
        private SoldierUnit item;
        private Point currentPosition;
        private Grid grid;
        private double gridWidth;
        private double gridHeight;
        private Button bBase;
        private Point basePosition;

        private DispatcherTimer timer;
        private Random rnd;
        private const double rndStep = 5;
        private const double stepToEnemy = 10;
        private const int timerInterval = 50;

        private List<Enemy> enemyList;
        public List<Enemy> EnemyList
        {
            set
            {
                enemyList = value;
                FindNearestEnemy();
            }
        }
        private Enemy nearestEnemy;
        private Point nearestEnemyPos;
        private double nearestEnemyDist;
        private const double pieceToEat = 0.2;
        private List<Soldier> soldierList;
        public List<Soldier> SoldierList
        {
            set { soldierList = value; }
        }

        public SoldierUnit Item
        {
            get { return item; }
        }
        public Point CurrentPosition
        {
            get { return currentPosition; }
        }

        public Soldier(MainWindow window)
        {
            grid = window.gMainGrid;
            gridWidth = window.gMainGrid.Width;
            gridHeight = window.gMainGrid.Height;
            bBase = window.bBase;
            basePosition = bBase.TransformToAncestor(grid).Transform(new Point(0, 0));

            item = new SoldierUnit();
            currentPosition.X = gridWidth / 2 - item.Width / 2;
            currentPosition.Y = gridHeight / 2 - bBase.Height / 2 - item.Height;
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
            while (enemyList.Count == 0)
            {
                MoveRandomly(null);
                return;
            }

            if (nearestEnemy.Item.Height == 0)
            {
                nearestEnemy.StopTimer(null,null);
                enemyList.Remove(nearestEnemy);
                FindNearestEnemy();
            }
            else if (currentPosition.X != nearestEnemy.CurrentPosition.X)
            {
                MoveToEnemy();
            }
            else if (nearestEnemy.Item.Height < pieceToEat)
            {
                nearestEnemy.Item.Height = 0;
                nearestEnemy.Item.Width = 0;
            }
            else
            {
                nearestEnemy.Item.Height -= pieceToEat;
                nearestEnemy.Item.Width -= pieceToEat;
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
            double newPositionX = currentPosition.X + GetRandomDirection() * rndStep;
            if (
                newPositionX > 0 &&
                newPositionX < (gridHeight - item.Height) &&
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

            double newPositionY = currentPosition.Y + GetRandomDirection() * rndStep;
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

        public void FindNearestEnemy()
        {
            if (enemyList.Count != 0)
            {
                Enemy ce;
                Enemy ne;
                Point cePos;
                Point nePos;
                double nDist;
                double cDist;
                ce = enemyList[0];
                cePos = ce.Item.TransformToAncestor(grid).Transform(new Point(0, 0));
                cDist = Math.Sqrt(Math.Pow((cePos.X - currentPosition.X), 2) + Math.Pow((cePos.Y - currentPosition.Y), 2));
                ne = ce;
                nePos = cePos;
                nDist = cDist;
                foreach (Enemy en in enemyList)
                {
                    ce = en;
                    cePos = en.Item.TransformToAncestor(grid).Transform(new Point(0, 0));
                    cDist = Math.Sqrt(Math.Pow((cePos.X - currentPosition.X), 2) + Math.Pow((cePos.Y - currentPosition.Y), 2));
                    if (cDist < nDist)
                    {
                        ne = ce;
                        nePos = cePos;
                        nDist = cDist;
                    }
                }
                nearestEnemy = ne;
                nearestEnemyPos = nePos;
                nearestEnemyDist = nDist;
            }
        }

        public void MoveToEnemy()
        {
            if ((Math.Abs(currentPosition.X - nearestEnemy.CurrentPosition.X) < stepToEnemy) &&
                (Math.Abs(currentPosition.Y - nearestEnemy.CurrentPosition.Y) < stepToEnemy))
            {
                currentPosition.X = nearestEnemy.CurrentPosition.X;
                currentPosition.Y = nearestEnemy.CurrentPosition.Y;
                item.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
                FindNearestEnemy();
                return;
            }

            if (nearestEnemyDist == 0)
            {
                FindNearestEnemy();
                return;
            }

            if (currentPosition.X > nearestEnemy.CurrentPosition.X)
            {
                currentPosition.X -= stepToEnemy * (currentPosition.X - nearestEnemy.CurrentPosition.X) / nearestEnemyDist;
            }
            else
            {
                currentPosition.X += stepToEnemy * (nearestEnemy.CurrentPosition.X - currentPosition.X) / nearestEnemyDist;
            }

            if (currentPosition.Y > nearestEnemy.CurrentPosition.Y)
            {
                currentPosition.Y -= stepToEnemy * (currentPosition.Y - nearestEnemy.CurrentPosition.Y) / nearestEnemyDist;
            }
            else
            {
                currentPosition.Y += stepToEnemy * (nearestEnemy.CurrentPosition.Y - currentPosition.Y) / nearestEnemyDist;
            }

            item.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
            FindNearestEnemy();
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
            soldierList.Remove(this);
        }
    }

    class SoldierUnit : Button
    {
        private const int height = 12;
        private const int width = 12;

        public SoldierUnit()
        {
            this.Height = height;
            this.Width = width;
            this.Background = Brushes.LimeGreen;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
        }
    }
}
