using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace graph_eiler
{
    public partial class Form1 : Form
    {
        // Матрица смежности для графа
        public double[,] graph = new double[8, 8]
        {
            { 0, 3, 0, 5, 6, 4, 0, 6 },
            { 3, 0, 3, 0, 0, 4, 0, 5 },
            { 0, 3, 0, 2, 0, 0, 0, 0 },
            { 5, 0, 2, 0, 2, 0, 0, 0 },
            { 6, 0, 0, 2, 0, 1, 0, 7 },
            { 4, 4, 0, 0, 1, 0, 1, 0 },
            { 0, 0, 0, 0, 0, 1, 0, 5 },
            { 6, 5, 0, 0, 7, 0, 5, 0 }
        };

        private Dictionary<int, Point> nodeCoordinates = new Dictionary<int, Point>
        {
            { 1, new Point(200, 50) },
            { 2, new Point(350, 100) },
            { 3, new Point(400, 200) },
            { 4, new Point(350, 300) },
            { 5, new Point(200, 370) },
            { 6, new Point(100, 300) },
            { 7, new Point(70, 200) },
            { 8, new Point(100, 100) }
        };
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            pictureBox1.Paint += pictureBox1_Paint;
            DrawGraph(pictureBox1.CreateGraphics());
        }
        private void DrawGraph(Graphics g)
        {
            // Очистим pictureBox перед рисованием
            g.Clear(Color.White);
            // Радиус вершин
            int radius = 20;

            // Рисуем вершины
            foreach (var kvp in nodeCoordinates)
            {
                int node = kvp.Key;
                Point center = kvp.Value;
                g.FillEllipse(Brushes.LightBlue, center.X - radius, center.Y - radius, 2 * radius, 2 * radius);
                g.DrawEllipse(Pens.Black, center.X - radius, center.Y - radius, 2 * radius, 2 * radius);
                g.DrawString(node.ToString(), new Font("Arial", 12), Brushes.Black, center.X - 8, center.Y - 8);
            }

            // Рисуем рёбра
            for (int i = 0; i < graph.GetLength(0); i++)
            {
                for (int j = i + 1; j < graph.GetLength(1); j++)
                {
                    if (graph[i, j] > 0)
                    {
                        Point start = nodeCoordinates[i + 1];
                        Point end = nodeCoordinates[j + 1];
                        g.DrawLine(Pens.Black, start, end);
                        // Рисуем вес ребра
                        g.DrawString(graph[i, j].ToString(), new Font("Arial", 10), Brushes.Black, (start.X + end.X) / 2, (start.Y + end.Y) / 2);
                    }
                }
            }
        }
        private List<int> FindEulerPath()
        {
            // Список для хранения пути
            List<int> path = new List<int>();
            // Стек для обхода графа
            Stack<int> stack = new Stack<int>();
            // Создание копии матрицы смежности для графа
            double[,] tempGraph = (double[,])graph.Clone();
            // Стартовая вершина
            int startNode = 0;
            stack.Push(startNode);

            // Подсчет степени каждой вершины
            int[] degrees = new int[graph.GetLength(0)];
            for (int i = 0; i < graph.GetLength(0); i++)
            {
                for (int j = 0; j < graph.GetLength(1); j++)
                {
                    if (graph[i, j] > 0)
                    {
                        degrees[i]++;
                    }
                }
            }

            while (stack.Count > 0)
            {
                int currentNode = stack.Peek();
                int i;

                for (i = 0; i < graph.GetLength(0); i++)
                {
                    if (tempGraph[currentNode, i] > 0)
                    {
                        // Удаляем ребро из графа
                        tempGraph[currentNode, i] = 0;
                        tempGraph[i, currentNode] = 0;
                        stack.Push(i);
                        break;
                    }
                }

                if (i == graph.GetLength(0))
                {
                    // Если у вершины больше нет инцидентных рёбер, добавляем её в путь
                    path.Add(currentNode);
                    stack.Pop();
                }
            }

            // Проверка на чётность степени каждой вершины
            for (int i = 0; i < degrees.Length; i++)
            {
                if (degrees[i] % 2 != 0)
                {
                    MessageBox.Show($"Граф не имеет эйлерова пути, так как у вершины {i + 1} нечётная степень.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            // Путь обходит каждое ребро ровно один раз, поэтому нужно перевернуть путь и прибавить 1 к каждой вершине
            path.Reverse();
            for (int i = 0; i < path.Count; i++)
            {
                path[i]++;
            }

            // Проверяем, что путь замкнутый (начальная и конечная вершины совпадают)
            if (path[0] != path[path.Count - 1])
            {
                MessageBox.Show("Граф не является замкнутым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return path;
        }

        private void DrawGraphWithDelay(List<int> eulerPath)
        {
            Graphics g = pictureBox1.CreateGraphics();
            DrawGraph(g); // Начальное отображение графа

            // Получаем количество вершин в графе
            int numVertices = nodeCoordinates.Count;

            // Словарь для отслеживания цветов вершин
            Dictionary<int, Color> nodeColors = new Dictionary<int, Color>();

            // Список перьев для рёбер
            List<Pen> edgePens = new List<Pen>();

            // Определяем количество различных цветов, которые будут использованы для раскраски вершин и рёбер
            int numNodeColors = (numVertices % 2 == 0) ? numVertices / 2 : (numVertices + 1) / 2;
            int numEdgeColors = (numVertices % 2 == 0) ? numVertices / 2 : (numVertices - 1) / 2;

            // Генерируем цвета для вершин
            List<Color> nodeColorList = GenerateColors(numNodeColors);
            // Генерируем цвета для рёбер
            List<Color> edgeColorList = GenerateColors(numEdgeColors * 4);


            // Создаем перья для всех возможных рёбер в графе
            foreach (Color color in edgeColorList)
            {
                edgePens.Add(new Pen(color, 5));
            }

            // Радиус вершин
            int radius = 20;

            // Отрисовка с задержкой
            for (int i = 0; i < eulerPath.Count - 1; i++)
            {
                int node1 = eulerPath[i] - 1;
                int node2 = eulerPath[i + 1] - 1;

                // Помечаем текущее ребро
                if (i < edgePens.Count) // Проверяем, что есть соответствующее перо для текущего ребра
                {
                    DrawEdge(g, node1, node2, edgePens[i]);

                    // Задержка в 1 секунду
                    Thread.Sleep(1000);

                    // Убираем пометку с ребра
                    DrawEdge(g, node1, node2, Pens.Black);
                }
                else
                {
                    MessageBox.Show("Ошибка: количество рёбер в графе превышает количество цветов рёбер. Пожалуйста, увеличьте количество цветов или уменьшите количество рёбер.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверяем, был ли уже окрашен данный узел
                if (!nodeColors.ContainsKey(node1))
                {
                    // Окрашиваем вершину
                    g.FillEllipse(new SolidBrush(nodeColorList[node1 % nodeColorList.Count]), nodeCoordinates[node1 + 1].X - radius, nodeCoordinates[node1 + 1].Y - radius, 2 * radius, 2 * radius);
                    nodeColors.Add(node1, nodeColorList[node1 % nodeColorList.Count]);
                }

                if (!nodeColors.ContainsKey(node2))
                {
                    // Окрашиваем вершину
                    g.FillEllipse(new SolidBrush(nodeColorList[node2 % nodeColorList.Count]), nodeCoordinates[node2 + 1].X - radius, nodeCoordinates[node2 + 1].Y - radius, 2 * radius, 2 * radius);
                    nodeColors.Add(node2, nodeColorList[node2 % nodeColorList.Count]);
                }


            }

            // Освобождаем ресурсы перьев рёбер
            foreach (Pen pen in edgePens)
            {
                pen.Dispose();
            }
        }
        private void DrawEdge(Graphics g, int node1, int node2, Pen pen)
        {
            Point start = nodeCoordinates[node1 + 1];
            Point end = nodeCoordinates[node2 + 1];
            g.DrawLine(pen, start, end);
            // Рисуем вес ребра
            g.DrawString(graph[node1, node2].ToString(), new Font("Arial", 10), Brushes.Black, (start.X + end.X) / 2, (start.Y + end.Y) / 2);
        }

        private List<Color> GenerateColors(int numColors)
        {
            // Генерируем указанное количество различных случайных цветов
            List<Color> colors = new List<Color>();
            Random rand = new Random();
            for (int i = 0; i < numColors; i++)
            {
                colors.Add(Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)));
            }
            return colors;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Находим эйлеров путь
            List<int> eulerPath = FindEulerPath();
            txtResult.Text = string.Join(" -> ", eulerPath);

            // Отрисовываем граф с задержкой
            DrawGraphWithDelay(eulerPath);
        }
        private void Form1_Load(object sender, EventArgs e) { DrawGraph(pictureBox1.CreateGraphics()); }
        private void pictureBox1_Paint(object sender, PaintEventArgs e) { DrawGraph(e.Graphics); }
        private void button2_Click(object sender, EventArgs e) { Application.Exit(); }
    }
}
