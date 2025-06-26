using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace demoex
{
    public partial class Form2 : Form
    {
        private Panel scrollablePanel;

        public Form2()
        {
            
            InitializeControls();//инициализация панели с прокруткой
            LoadDataAsBlocks();//загрузка данных партнёра из бд и их отображение
            InitializeComponent();//инициализация остальных компонентов формы
           // this.Text = "Список партнёров";
          this.Size = new Size(1000, 600); // Размер формы
        }

        private void InitializeControls()
        {
            // Создаем прокручиваемую панель, на которой находятся блоки
            scrollablePanel = new Panel
            {
                AutoScroll = true,//включается прокрутка если элементы не помещаются
                Location = new Point(10,50),
                Size=new Size(1300,600)
            };

            this.Controls.Add(scrollablePanel);
        }

       
        private void LoadDataAsBlocks()
        {
            scrollablePanel.Controls.Clear(); // очистка панели перед загрузкой, чтобы избежать дублирования

            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres"; //подключение к бд
            string query = @"
                    SELECT 
                        p.id AS partner_id,
                        np.naimenovanie_partnera AS partner_name,
                        tp.tip_part AS partner_type,
                        p.ur_adress AS ur_adress,
                        p.phone AS phone,
                        p.reiting AS rating,
                        COALESCE(SUM(pp.kolichestvo_produc), 0) AS total_sales,
                        CASE
                            WHEN COALESCE(SUM(pp.kolichestvo_produc), 0) > 300000 THEN '15%'
                            WHEN COALESCE(SUM(pp.kolichestvo_produc), 0) > 50000 THEN '10%'
                            WHEN COALESCE(SUM(pp.kolichestvo_produc), 0) > 10000 THEN '5%'
                            ELSE '0%'
                        END AS discount
                    FROM 
                        public.partner p
                    LEFT JOIN 
                        public.partner_products pp ON pp.naim_part = p.naim_part
                    JOIN 
                        public.naimen_partner np ON p.naim_part = np.id
                    JOIN 
                        public.tip_partner tp ON p.tip_partn = tp.id
                    GROUP BY 
                        p.id, np.naimenovanie_partnera, tp.tip_part, p.director, p.phone, p.reiting;";
             //запрос получает данные о партнёре, имя, тип, директор, теефон,рейтинг, общий объём продаж и рассчитывет скидку, группировка.
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    { //инициализация позиций для блоков
                        int xPosition = 10;
                        int yPosition = 10;
                        int columnWidth = 400; // Увеличиваем ширину блока
                        int blockHeight = 150; // Увеличиваем высоту блока
                        int maxBlocksPerColumn = 10; 
                        int currentBlockCount = 0;
                       
                        //для каждой записи создаётся пэйнел с данными
                        while (reader.Read())
                        {
                            Panel blockPanel = new Panel
                            {
                                Size = new Size(columnWidth - 10, blockHeight - 10),
                                Location = new Point(xPosition, yPosition),
                                BorderStyle = BorderStyle.FixedSingle
                            };

                            // Тип и Наименование
                            Label typeLabel = new Label
                            {
                                Text = $"{reader["partner_type"]} | {reader["partner_name"]}",
                                Location = new Point(10, 10),
                                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                                AutoSize = true
                            };
                            blockPanel.Controls.Add(typeLabel);

                            // Должность
                            Label positionLabel = new Label
                            {
                                Text = $"Юридический адрес: {reader["ur_adress"]}",
                                Location = new Point(10, 30),
                                Font = new Font("Segoe UI", 10),
                                AutoSize = true
                            };
                            blockPanel.Controls.Add(positionLabel);

                            // Телефон
                            Label phoneLabel = new Label
                            {
                                Text = $"Телефон: {reader["phone"]}",
                                Location = new Point(10, 50),
                                Font = new Font("Segoe UI", 9),
                                AutoSize = true
                            };
                            blockPanel.Controls.Add(phoneLabel);

                            // Рейтинг
                            Label ratingLabel = new Label
                            {
                                Text = $"Рейтинг: {reader["rating"]}",
                                Location = new Point(10, 70),
                                Font = new Font("Segoe UI", 9),
                                AutoSize = true
                            };
                            blockPanel.Controls.Add(ratingLabel);

                            // Скидка
                            Label discountLabel = new Label
                            {
                                Text = $"Скидка: {reader["discount"]}",
                                Location = new Point(300, 10),
                                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                                ForeColor = Color.Green,
                                AutoSize = true
                            };
                            blockPanel.Controls.Add(discountLabel);

                            // Добавляем кнопку "Редактировать"
                            Button editButton = new Button
                            {
                                Text = "Редактировать",
                                Tag = reader["partner_id"], // Привязываем partner_id к кнопке
                                Location = new Point(10, 100), // Расположение кнопки в блоке
                                Size = new Size(120, 30)
                            };
                            editButton.Click += EditButton_Click; // Обработчик события для редактирования
                            blockPanel.Controls.Add(editButton);

                            // Добавляем кнопку "История продаж"
                            Button historyButton = new Button
                            {
                                Text = "История продаж",
                                Tag = reader["partner_id"], // Привязываем partner_id к кнопке
                                Location = new Point(140, 100), // Расположение кнопки
                                Size = new Size(120, 30)
                            };
                            historyButton.Click += HistoryButton_Click; // Обработчик события для истории продаж
                            blockPanel.Controls.Add(historyButton);

                            scrollablePanel.Controls.Add(blockPanel);
                            //прибавление блоков 
                            currentBlockCount++;
                            if (currentBlockCount < maxBlocksPerColumn)
                            {
                                yPosition += blockHeight;
                            }
                            else
                            {
                                currentBlockCount = 0;
                                xPosition += columnWidth;
                                yPosition = 10;
                            }
                        }
                    }
                }
            }
        }
        private void HistoryButton_Click(object sender, EventArgs e)
        {
            // Получаем partnerId из Tag кнопки
            Button historyButton = sender as Button;
            int partnerId = Convert.ToInt32(historyButton.Tag);

            // Открываем Form4 и передаём partnerId
            Form4 historyForm = new Form4(partnerId);
            historyForm.Show();
            this.Hide();
        }

        

        private void EditButton_Click(object sender, EventArgs e)
        {
            Button editButton = sender as Button;
            int partnerId = Convert.ToInt32(editButton.Tag); //получаем айди партнёра

            // Открываем форму для редактирования партнёра
            using (Form3 editForm = new Form3(partnerId))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // После закрытия формы редактирования обновляем данные
                    LoadDataAsBlocks();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Show();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}