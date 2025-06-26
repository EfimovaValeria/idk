using Npgsql;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace demoex
{
    public partial class Form4 : Form
    {
        // Поле для строки подключения
        private readonly string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";
        private readonly int partnerId; //айди партнрёра данные которого отобрражаются в форме
        private string partnerName; // Название партнёра
        private Panel scrollablePanel;

        public Form4(int partnerId)
        {
            this.partnerId = partnerId;

            LoadPartnerName(); // Загрузка имени партнёра

            InitializeComponent();
            this.Text = "История продаж";
            this.Size = new Size(1000, 600); // Размер формы

            InitializeHeader(); // Заголовок формы
            InitializeScrollablePanel(); // Прокручиваемая панель
            LoadHistory(); // Загрузка истории продаж
        }

        // Инициализация заголовка
        private void InitializeHeader()
        {
            Label headerLabel = new Label
            {
                Text = $"История продаж - {partnerName}", //заголовок плюс партнер из базы
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(10, 10),
                AutoSize = true
            };
            this.Controls.Add(headerLabel);
        }

        // Инициализация прокручиваемой панели
        private void InitializeScrollablePanel()
        {
            scrollablePanel = new Panel 
            {
                AutoScroll = true,//прокрутка
                Location = new Point(10, 50), // Под заголовком
                Size = new Size(760, 500),    // Размер панели
                BorderStyle = BorderStyle.None//убираю рамку
            };
            this.Controls.Add(scrollablePanel);
        }

        // Загрузка имени партнёра в заголовок
        private void LoadPartnerName()
        {
            try
            {//команда для выполнения запроса
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT np.naimenovanie_partnera FROM public.partner p JOIN public.naimen_partner np ON p.naim_part = np.id WHERE p.id = @partnerId;";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {//передаём айди партнера в запрос
                        command.Parameters.AddWithValue("@partnerId", partnerId);
                        object result = command.ExecuteScalar();//результат -  название партнра
                        partnerName = result != null ? result.ToString() : "Неизвестный партнёр";//проверка результата и присваивание значения переменной
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки имени партнёра: {ex.Message}");
                partnerName = "Неизвестный партнёр";
            }
        }

        // Загрузка данных об истории продаж
        private void LoadHistory()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            np.naim_product AS product_name,
                            pp.kolichestvo_produc AS quantity,
                            pp.data_prodazhi AS sale_date
                        FROM 
                            public.partner_products pp
                        JOIN 
                            public.naimen_produc np ON pp.product = np.id
                        WHERE 
                            pp.naim_part = @partnerId
                        ORDER BY 
                            pp.data_prodazhi DESC;";//запрос на полуаение истоии продаж

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))//команда для выполнения запроса
                    {
                        command.Parameters.AddWithValue("@partnerId", partnerId); //айди партнёра передаю в запрос
                        using (NpgsqlDataReader reader = command.ExecuteReader())//считываю данные
                        {
                            int yPosition = 10;// начальная позиция для блоков с данными по у
                            while (reader.Read())
                            {
                                // Создаём блок
                                Panel blockPanel = new Panel
                                {
                                    Size = new Size(740, 100),
                                    Location = new Point(10, yPosition),
                                    BorderStyle = BorderStyle.FixedSingle,
                                    BackColor = Color.FromArgb(240, 248, 255)
                                };

                                // метка с название продукта
                                Label productLabel = new Label
                                {
                                    Text = $"Продукт: {reader["product_name"]}",
                                    Location = new Point(10, 10),
                                    Font = new Font("Arial", 10, FontStyle.Bold),
                                    AutoSize = true
                                };
                                blockPanel.Controls.Add(productLabel);

                                // Количество
                                Label quantityLabel = new Label
                                {
                                    Text = $"Количество: {reader["quantity"]}",
                                    Location = new Point(10, 40),
                                    Font = new Font("Arial", 10),
                                    AutoSize = true
                                };
                                blockPanel.Controls.Add(quantityLabel);

                                // Дата продажи
                                Label dateLabel = new Label
                                {
                                    Text = $"Дата: {Convert.ToDateTime(reader["sale_date"]).ToShortDateString()}",
                                    Location = new Point(10, 70),
                                    Font = new Font("Arial", 10),
                                    AutoSize = true
                                };
                                blockPanel.Controls.Add(dateLabel);

                                // Добавляем блок в прокручиваемую панель
                                scrollablePanel.Controls.Add(blockPanel);

                                yPosition += 110; // Смещаемся вниз для следующего блока
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}");
            }
        }
    
public int CalculateMaterial(int prodTypeId, string materialType, int productCount, double param1, double param2)
    { 
            //проверяем корректность данных
            if (productCount <= 0  ||param1 <= 0||  param2 <= 0)
            {
                return -1; // Проверка на невалидные входные данные
            }

            try
            {
                double koefTipa;
                double procentBraka;

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Получаем коэффициент типа продукции
                    string koefQuery = "SELECT koef_tipa FROM public.prod_type_koef WHERE id = @prodTypeId;";
                    using (NpgsqlCommand koefCommand = new NpgsqlCommand(koefQuery, connection))
                    {
                        koefCommand.Parameters.AddWithValue("@prodTypeId", prodTypeId);//передаём айди типа как параметр
                        object result = koefCommand.ExecuteScalar();
                        if (result == null) return -1; // Тип продукции не найден, если результат пустой
                        koefTipa = Convert.ToDouble(result);//преобразуем результат в дабл
                    }

                    // Получаем процент брака
                    string brakQuery = "SELECT procent_braka FROM public.brak WHERE tip_mater = @materialType;";
                    using (NpgsqlCommand brakCommand = new NpgsqlCommand(brakQuery, connection))
                    {
                        brakCommand.Parameters.AddWithValue("@materialType", materialType);//передаём тип материала как параметр
                        object result = brakCommand.ExecuteScalar();
                        if (result == null) return -1; // Тип материала не найден, если нет результата
                        procentBraka = Convert.ToDouble(result);
                    }
                }

                // Расчёт количества материала
                double requiredMaterial = (param1 * param2 * koefTipa * productCount) * (1 + procentBraka / 100);
                return (int)Math.Ceiling(requiredMaterial); // Округляем до целого числа
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при расчёте материала: {ex.Message}");
                return -1; // В случае ошибки возвращаем -1
            }
        
     }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
            this.Hide();

        }
    }
}