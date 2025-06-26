using Npgsql;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace demoex
{
    public partial class Form3 : Form
    {
        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";
        private int? partnerId; //идентификатор партнёра(если нул то добавление, если задан то редактирование)

        //поля ввода и элементы управления
        private ComboBox typeComboBox;
        private TextBox nameTextBox;
        private TextBox positionTextBox;
        private TextBox phoneTextBox;
        private NumericUpDown ratingNumericUpDown;
        private Button saveButton;
        private Button cancelButton;
        private TextBox addressTextBox;
        private TextBox emailTextBox;

        public Form3(int? partnerId = null)
        {
          
            InitializeComponent();
            this.partnerId = partnerId; //сохраняем айди если есть

            InitializeFormComponents();
            LoadPartnerTypes();//загрузка типов партнёров в выпадающий список

            
            if (partnerId.HasValue)//если есть айди, то загружаем данные партнёра для редактирования
            {
                LoadPartnerData(partnerId.Value);
            }
            this.Size = new Size(1000, 600); // Размер формы
        }

        private void InitializeFormComponents()
        {
            this.Text = partnerId.HasValue ? "Редактировать партнёра" : "Добавить партнёра"; //заголовок формы
            this.Size = new Size(400, 400); // размер формы

            //метки и поля для ввода данных
            Label typeLabel = new Label { Text = "Тип партнёра", Location = new Point(10, 20), AutoSize = true };
            typeComboBox = new ComboBox { Location = new Point(150, 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            Label nameLabel = new Label { Text = "Наименование", Location = new Point(10, 60), AutoSize = true };
            nameTextBox = new TextBox { Location = new Point(150, 60), Width = 200 };

            Label positionLabel = new Label { Text = "Должность", Location = new Point(10, 100), AutoSize = true };
            positionTextBox = new TextBox { Location = new Point(150, 100), Width = 200 };

            Label phoneLabel = new Label { Text = "Телефон", Location = new Point(10, 140), AutoSize = true };
            phoneTextBox = new TextBox { Location = new Point(150, 140), Width = 200 };

            Label ratingLabel = new Label { Text = "Рейтинг", Location = new Point(10, 180), AutoSize = true };
            ratingNumericUpDown = new NumericUpDown
            {
                Location = new Point(150, 180),
                Maximum = 10,
                Minimum = 1,
                Width = 50
            };
            // Поля для адреса и email
            Label addressLabel = new Label { Text = "Адрес", Location = new Point(10, 220), AutoSize = true };
            addressTextBox = new TextBox { Location = new Point(150, 220), Width = 200 };

            Label emailLabel = new Label { Text = "Email", Location = new Point(10, 260), AutoSize = true };
            emailTextBox = new TextBox { Location = new Point(150, 260), Width = 200 };

            saveButton = new Button { Text = "Сохранить", Location = new Point(150, 300), Size = new Size(100, 30) };
            saveButton.Click += SaveButton_Click;//обработчик нажатия

            cancelButton = new Button { Text = "Отмена", Location = new Point(260, 300), Size = new Size(100, 30) };
            cancelButton.Click += (s, e) => this.Close();

            //добавление элементов на форму
            this.Controls.Add(typeLabel);
            this.Controls.Add(typeComboBox);
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(positionLabel);
            this.Controls.Add(positionTextBox);
            this.Controls.Add(phoneLabel);
            this.Controls.Add(phoneTextBox);
            this.Controls.Add(ratingLabel);
            this.Controls.Add(ratingNumericUpDown);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(addressLabel);
            this.Controls.Add(addressTextBox);
            this.Controls.Add(emailLabel);
            this.Controls.Add(emailTextBox);
        }

        private void LoadPartnerTypes()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id, tip_part FROM public.tip_partner;";//запрос на получение типов партнёра
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())//выполняем запрос, результаты возвращаем  в объект
                        {
                            typeComboBox.Items.Clear();//очищаем список, предотвращаем дублирование
                            while (reader.Read())//перебор строк результата, для каждой строки айтем
                            {
                                //добавление элементов в комбобокс
                                typeComboBox.Items.Add(new ComboBoxItem
                                {
                                    Text = reader["tip_part"].ToString(),
                                    Value = Convert.ToInt32(reader["id"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов партнёров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private int Clamp(int value, int min, int max)//ограничение рейтинга в заданых пределах
        {
            if (value < min) return min;//если значение меньше минимального, то возвращаю минимальное
            if (value > max) return max;
            return value;//если в пределах возвращаю значение  
        }
        private void LoadPartnerData(int partnerId)//метод загрузки данные партнёра из бд для редактирования
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            p.tip_partn,
                            np.naimenovanie_partnera,
                            p.director,
                            p.phone,
                            p.email,
                            p.ur_adress,
                            p.reiting
                        FROM public.partner p
                        JOIN public.naimen_partner np ON p.naim_part = np.id
                        WHERE p.id = @partnerId;";//выбираем данные для указанного айди партнёра

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))//объект команды для выполнения запроса
                    {
                        command.Parameters.AddWithValue("@partnerId", partnerId);//передаём значение айди партнёра в запрос
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())//заполняем поля ввода значениями из бызы, если найдена строка с данными
                            {
                                nameTextBox.Text = reader["naimenovanie_partnera"].ToString();
                                positionTextBox.Text = reader["director"].ToString();
                                phoneTextBox.Text = reader["phone"].ToString();
                                emailTextBox.Text = reader["email"].ToString(); // Загружаем email
                                addressTextBox.Text = reader["ur_adress"].ToString(); // Загружаем адрес
                                ratingNumericUpDown.Value = Clamp(Convert.ToInt32(reader["reiting"]), 1, 10);//рейтинг с ограничениями

                                int typeId = Convert.ToInt32(reader["tip_partn"]);//опред тип партнера по айди
                                foreach (ComboBoxItem item in typeComboBox.Items)
                                {
                                    if ((int)item.Value == typeId)//если текущий элемент бокса соответствует айди типа
                                    {
                                        typeComboBox.SelectedItem = item;//устанавливаем выбранный элемент
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных партнёра: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (typeComboBox.SelectedItem == null) //проверяем выбран ли тип партнёра, если нет то завершение  метода
            {
                MessageBox.Show("Выберите тип партнёра!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedType = (ComboBoxItem)typeComboBox.SelectedItem;//получаем выбранный тип, преобразует в объект комбо бокса для текст и валью

            try//для обработки возможных исключений
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Вставка наименования в таблицу naimen_partner
                    string insertNameQuery = @"
                INSERT INTO public.naimen_partner (naimenovanie_partnera)
                VALUES (@naimenovanie_partnera)
                ON CONFLICT (naimenovanie_partnera) DO NOTHING;";//конфликт для избежания дублирования записей при одинаковых именах

                    using (NpgsqlCommand insertNameCommand = new NpgsqlCommand(insertNameQuery, connection))//команда для выполнения запроса
                    {
                        insertNameCommand.Parameters.AddWithValue("@naimenovanie_partnera", nameTextBox.Text);//параметр наименования заполняется текстом из текст бокса
                        insertNameCommand.ExecuteNonQuery();//ExecuteNonQuery спользуется для закпросов без возвращаемого результата
                    }

                    // запрос для добавления/обновления партнёра
                    string query;

                    if (partnerId.HasValue)//если есть айди редактируем
                    {
                        query = @"
                    UPDATE public.partner
                    SET 
                        tip_partn = @tip_partn,
                        naim_part = (SELECT id FROM public.naimen_partner WHERE naimenovanie_partnera = @naim_part),
                        director = @director,
                        phone = @phone,
                        email = @email,
                        ur_adress = @ur_adress,
                        reiting = @reiting
                    WHERE id = @partnerId;";
                    }
                    else//если нет добавляем нового
                    {
                        query = @"
                    INSERT INTO public.partner (tip_partn, naim_part, director, phone, email, ur_adress, reiting)
                    VALUES (
                        @tip_partn, 
                        (SELECT id FROM public.naimen_partner WHERE naimenovanie_partnera = @naim_part), 
                        @director, 
                        @phone, 
                        @email, 
                        @ur_adress, 
                        @reiting
                    );";
                    }

                    //команда для выполнения сформированного запроса
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {//параметры запролняются значениями из полей формы
                        command.Parameters.AddWithValue("@tip_partn", selectedType.Value);//тип партнера берется из selectedType.Value
                        command.Parameters.AddWithValue("@naim_part", nameTextBox.Text);
                        command.Parameters.AddWithValue("@director", positionTextBox.Text);
                        command.Parameters.AddWithValue("@phone", phoneTextBox.Text);
                        command.Parameters.AddWithValue("@email", emailTextBox.Text);
                        command.Parameters.AddWithValue("@ur_adress", addressTextBox.Text);
                        command.Parameters.AddWithValue("@reiting", (int)ratingNumericUpDown.Value);

                        if (partnerId.HasValue)
                        {
                            command.Parameters.AddWithValue("@partnerId", partnerId.Value);//если обновляется существующий партнёр то обнавляется паараметр партнёр айди
                        }

                        command.ExecuteNonQuery();//запрос на сохранение данных
                    }
                }

                MessageBox.Show(partnerId.HasValue ? "Данные партнёра обновлены!" : "Партнёр добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;//возвращаем результат
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения партнёра: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ComboBoxItem//класс для представления элемента в комбобокс
        {
            public string Text { get; set; }//текст  для отображения
            public object Value { get; set; }//айди соотв тексту
            public override string ToString() => Text;//метод тустринг отображает в комбобокс только текст
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
          
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}