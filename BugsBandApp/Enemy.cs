using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BugsBandApp
{
    class Enemy
    {
        private EnemyUnit item;
        private Point currentPosition;
        private Grid grid;
        private double gridWidth;
        private double gridHeight;
        private Button bBase;
        private Point basePosition;        

        private DispatcherTimer timer;
        private Random rnd;
        private const double rndStep = 5;
        private const double stepToWorker = 10;
        private const int timerInterval = 50;

        private List<Worker> workerList;
        public List<Worker> WorkerList
        {
            set
            {
                workerList = value;
                FindNearestWorker();
            }
        }
        private Worker nearestWorker;
        private Point nearestWorkerPos;
        private double nearestWorkerDist;
        private const double pieceToEat = 0.2;
        private List<Enemy> enemyList;
        public List<Enemy> EnemyList
        {
            set { enemyList = value; }
        }

        public EnemyUnit Item
        {
            get { return item; }
        }
        public Point CurrentPosition
        {
            get { return currentPosition; }
        }

        public Enemy(MainWindow window)
        {
            grid = window.gMainGrid;
            gridWidth = window.gMainGrid.Width;
            gridHeight = window.gMainGrid.Height;
            bBase = window.bBase;
            basePosition = bBase.TransformToAncestor(grid).Transform(new Point(0, 0));

            item = new EnemyUnit();
            currentPosition = Mouse.GetPosition(grid);
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
            while (workerList.Count == 0)
            {
                MoveRandomly(null);
                return;
            }

            if (nearestWorker.Item.Height == 0)
            {
                nearestWorker.StopTimer(null, null);
                workerList.Remove(nearestWorker);
                FindNearestWorker();
            }
            else if (currentPosition.X != nearestWorker.CurrentPosition.X)
            {
                MoveToWorker();
            }
            else if (nearestWorker.Item.Height < pieceToEat)
            {
                nearestWorker.Item.Height = 0;
                nearestWorker.Item.Width = 0;
            }
            else
            {
                nearestWorker.Item.Height -= pieceToEat;
                nearestWorker.Item.Width -= pieceToEat;
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

        public void FindNearestWorker()
        {
            if (workerList.Count != 0)
            {
                Worker cw;
                Worker nw;                
                Point cwPos;
                Point nwPos;
                double nDist;
                double cDist;
                cw = workerList[0];
                cwPos = cw.Item.TransformToAncestor(grid).Transform(new Point(0, 0));
                cDist = Math.Sqrt(Math.Pow((cwPos.X - currentPosition.X), 2) + Math.Pow((cwPos.Y - currentPosition.Y), 2));
                nw = cw;
                nwPos = cwPos;
                nDist = cDist;
                foreach (Worker w in workerList)
                {
                    cw = w;
                    cwPos = w.Item.TransformToAncestor(grid).Transform(new Point(0, 0));
                    cDist = Math.Sqrt(Math.Pow((cwPos.X - currentPosition.X), 2) + Math.Pow((cwPos.Y - currentPosition.Y), 2));
                    if (cDist < nDist)
                    {
                        nw = cw;
                        nwPos = cwPos;
                        nDist = cDist;
                    }
                }
                nearestWorker = nw;
                nearestWorkerPos = nwPos;
                nearestWorkerDist = nDist;
            }
        }

        public void MoveToWorker()
        {
            if ((Math.Abs(currentPosition.X - nearestWorker.CurrentPosition.X) < stepToWorker) &&
                (Math.Abs(currentPosition.Y - nearestWorker.CurrentPosition.Y) < stepToWorker))
            {
                currentPosition.X = nearestWorker.CurrentPosition.X;
                currentPosition.Y = nearestWorker.CurrentPosition.Y;
                item.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
                FindNearestWorker();
                return;
            }

            if (nearestWorkerDist == 0)
            {
                FindNearestWorker();
                return;
            }                

            if (currentPosition.X > nearestWorker.CurrentPosition.X)
            {
                currentPosition.X -= stepToWorker * (currentPosition.X - nearestWorker.CurrentPosition.X) / nearestWorkerDist;
            }
            else
            {
                currentPosition.X += stepToWorker * (nearestWorker.CurrentPosition.X - currentPosition.X) / nearestWorkerDist;
            }

            if (currentPosition.Y > nearestWorker.CurrentPosition.Y)
            {
                currentPosition.Y -= stepToWorker * (currentPosition.Y - nearestWorker.CurrentPosition.Y) / nearestWorkerDist;
            }
            else
            {
                currentPosition.Y += stepToWorker * (nearestWorker.CurrentPosition.Y - currentPosition.Y) / nearestWorkerDist;
            }

            item.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);                            
            FindNearestWorker();
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
            enemyList.Remove(this);
        }
    }

    class EnemyUnit : Button
    {
        private const int height = 12;
        private const int width = 12;

        public EnemyUnit()
        {
            this.Height = height;
            this.Width = width;
            this.Background = Brushes.Red;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
        }
    }
}
