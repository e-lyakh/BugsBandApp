using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BugsBandApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private List<Food> foodList;
        private List<Worker> workerList;
        private List<Enemy> enemyList;
        private List<Soldier> soldierList;

        public MainWindow()
        {
            InitializeComponent();           
            foodList = new List<Food>();
            workerList = new List<Worker>();
            enemyList = new List<Enemy>();
            soldierList = new List<Soldier>();            
        }        

        private void newFood_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Food food = new Food(this);            
            foodList.Add(food);
            food.FoodList = foodList;
            foreach (Worker w in workerList)
            {
                w.FindNearestFood();
            }
        }

        private void newWorker_Click(object sender, RoutedEventArgs e)
        {
            Worker worker = new Worker(this);
            worker.FoodList = foodList;
            workerList.Add(worker);
            worker.WorkerList = workerList;
            foreach (Enemy en in enemyList)
            {
                en.FindNearestWorker();
            }
        }        

        private void newEnemy_Click(object sender, RoutedEventArgs e)
        {
            Enemy enemy = new Enemy(this);
            enemy.WorkerList = workerList;
            enemyList.Add(enemy);
            enemy.EnemyList = enemyList;
            foreach (Soldier s in soldierList)
            {
                s.FindNearestEnemy();
            }            
        }

        private void newSoldier_Click(object sender, RoutedEventArgs e)
        {
            Soldier soldier = new Soldier(this);
            soldier.EnemyList = enemyList;
            soldierList.Add(soldier);
            soldier.SoldierList = soldierList;
        }
    }
}
