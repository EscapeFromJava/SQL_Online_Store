using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace SQL_Online_Store
{
    internal class Program
    {

        private static string connectString = ConfigurationManager.ConnectionStrings["OnlineStore.dbo"].ConnectionString;
        private static SqlConnection sqlConnection = null;
        static void Main(string[] args)
        {
            sqlConnection = new SqlConnection(connectString);
            sqlConnection.Open();

            Console.WriteLine("Online Store");

            SqlDataReader sqlDataReader = null;

            string command = string.Empty;

            while (true)
            {

                Console.WriteLine("\n[1] - поиск товара по названию;");
                Console.WriteLine("[2] - выбрать все товары в категории;");
                Console.WriteLine("[3] - просмотр заказов по имени покупателя;");
                Console.WriteLine("[4] - создать новый заказ;");
                Console.WriteLine("[5] - создать группу заказов;");
                Console.WriteLine("[6] - изменить статус заказа.");
                Console.Write("\nВыберите вариант запроса: ");
                var index = Console.ReadLine();

                string idProduct = string.Empty;

                while (true)
                {

                    switch (index)
                    {

                        case "1":
                            Console.Clear();
                            Console.Write("Введите наименование товара> ");
                            command = Console.ReadLine();

                            SqlCommand sqlCommand = new SqlCommand($"select product.id, product.price, product.volume, brand.brand, category.category " +
                                                                   $"from product join brand on product.brand = brand.id " +
                                                                                  $"join category on product.category = category.id " +
                                                                   $"where product.name = '{command.ToLower()}'", sqlConnection);

                            sqlDataReader = sqlCommand.ExecuteReader();

                            while (sqlDataReader.Read())
                            {
                                Console.WriteLine($"ID товара: {sqlDataReader["id"]} | Цена: {sqlDataReader["price"]}$ " +
                                                  $"| Количество: {sqlDataReader["volume"]} | Производитель: {sqlDataReader["brand"]} " +
                                                  $"| Категория: {sqlDataReader["category"]}");
                            }

                            if (sqlDataReader != null)
                            {
                                sqlDataReader.Close();
                            }
                            break;

                        case "2":
                            Console.Clear();
                            sqlCommand = new SqlCommand($"select product.id, product.name, product.price, product.volume, brand.brand, category.category " +
                                                        $"from product join brand on product.brand = brand.id " +
                                                                     $"join category on product.category = category.id", sqlConnection);

                            sqlDataReader = sqlCommand.ExecuteReader();

                            while (sqlDataReader.Read())
                            {
                                Console.WriteLine($"ID товара: {sqlDataReader["Id"]} | Наименование: {sqlDataReader["Name"]} " +
                                                  $"| Цена: {sqlDataReader["Price"]}$ | Количество: {sqlDataReader["Volume"]} " +
                                                  $"| Производитель: {sqlDataReader["Brand"]} | Категория: {sqlDataReader["Category"]}");
                            }

                            if (sqlDataReader != null)
                            {
                                sqlDataReader.Close();
                            }
                            break;

                        case "3":
                            Console.Clear();
                            Console.WriteLine("Список покупателей: ");
                            sqlCommand = new SqlCommand($"select client.id, client.client, count(orders.id)  as 'volume' " +
                                                        $"from orders join client on orders.id_client = client.id " +
                                                        $"group by client.id, client.client", sqlConnection);
                            sqlDataReader = sqlCommand.ExecuteReader();

                            while (sqlDataReader.Read())
                            {
                                Console.WriteLine($"ID покупателя: {sqlDataReader["id"]} | Имя: {sqlDataReader["client"]} | Количество заказов: {sqlDataReader["volume"]}");
                            }

                            sqlDataReader.Close();

                            Console.Write("\nВведите имя покупателя> ");

                            command = Console.ReadLine();

                            sqlCommand = new SqlCommand($"select orders.id, product.name, date_create, status.status " +
                                                        $"from orders join client on orders.id_client = client.id " +
                                                                    $"join status on orders.status = status.id " +
                                                                    $"join product on orders.id_product = product.id " +
                                                        $"Where Client.Client = '{command}'", sqlConnection);

                            sqlDataReader = sqlCommand.ExecuteReader();

                            Console.WriteLine();

                            if (sqlDataReader.HasRows)
                            {

                                while (sqlDataReader.Read())
                                {
                                    Console.WriteLine($"ID заказа: {sqlDataReader["Id"]} | Продукт: {sqlDataReader["Name"]} " +
                                                      $"| Дата создания заказа: {sqlDataReader["Date_Create"].ToString().Substring(0, 10)} " +
                                                      $"| Статус: {sqlDataReader["Status"]}");
                                }

                                if (sqlDataReader != null)
                                {
                                    sqlDataReader.Close();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Заказы отсутствуют");
                                sqlDataReader.Close();
                            }
                            break;

                        case "4":
                            Console.Clear();
                            Console.Write("Введите имя покупателя> ");
                            string nameClient = Console.ReadLine();

                            sqlCommand = new SqlCommand($"select * from Client where Client.Client = '{nameClient}'", sqlConnection);

                            sqlDataReader = sqlCommand.ExecuteReader();

                            if (sqlDataReader.HasRows)
                            {
                                sqlDataReader.Close();
                                Console.Write("Введите ID товара> ");
                                idProduct = Console.ReadLine();

                                sqlCommand = new SqlCommand($"select * from product where id = '{idProduct}'", sqlConnection);

                                sqlDataReader = sqlCommand.ExecuteReader();

                                if (sqlDataReader.HasRows)
                                {
                                    sqlDataReader.Close();
                                    sqlCommand = new SqlCommand($"declare @nameClient varchar(50) = (select id from Client where Client.Client = '{nameClient}') " +
                                                                $"exec addorder @nameClient, {idProduct}", sqlConnection);

                                    sqlDataReader = sqlCommand.ExecuteReader();

                                    if (sqlDataReader != null)
                                    {
                                        Console.WriteLine("Операция выполнена успешно");
                                        sqlDataReader.Close();
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Товар с ID {idProduct} отсутствует в БД");
                                    sqlDataReader.Close();
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Покупатель {nameClient} отсутствует в БД");
                                sqlDataReader.Close();
                            }
                            break;

                        case "5":
                            Console.Clear();
                            Console.Write("Введите имя покупателя> ");
                            nameClient = Console.ReadLine();

                            sqlCommand = new SqlCommand($"select * from Client where Client.Client = '{nameClient}'", sqlConnection);

                            sqlDataReader = sqlCommand.ExecuteReader();

                            if (sqlDataReader.HasRows)
                            {
                                sqlDataReader.Close();
                                Console.Write("Введите количество добавляемых товаров> ");
                                int temp = Convert.ToInt32(Console.ReadLine());
                                for (int i = 0; i < temp; i++)
                                {

                                    Console.Write("Введите ID товара> ");
                                    idProduct = Console.ReadLine();

                                    sqlCommand = new SqlCommand($"select * from product where id = '{idProduct}'", sqlConnection);

                                    sqlDataReader = sqlCommand.ExecuteReader();

                                    if (sqlDataReader.HasRows)
                                    {
                                        sqlDataReader.Close();
                                        sqlCommand = new SqlCommand($"declare @nameClient varchar(50) = (select id from Client where Client.Client = '{nameClient}') " +
                                                                    $"exec addorder @nameClient, {idProduct}", sqlConnection);

                                        sqlDataReader = sqlCommand.ExecuteReader();
                                        Console.WriteLine($"Товар с ID {idProduct} добавлен");
                                        sqlDataReader.Close();
                                        if (sqlDataReader != null)
                                        {
                                            Console.WriteLine("Операция выполнена успешно");
                                            sqlDataReader.Close();
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Товар с ID {idProduct} отсутствует в БД");
                                        sqlDataReader.Close();
                                        temp++;
                                    }
                                }

                            }
                            else
                            {
                                Console.WriteLine($"Покупатель {nameClient} отсутствует в БД");
                                sqlDataReader.Close();
                            }
                            break;

                        case "6":
                            Console.Clear();
                            Console.Write("Введите ID заказа> ");
                            idProduct = Console.ReadLine();
                            Console.WriteLine("\n[1] - новый;");
                            Console.WriteLine("[2] - в обработке;");
                            Console.WriteLine("[3] - отправлен;");
                            Console.WriteLine("[4] - доставлен;");
                            Console.WriteLine("[5] - отменен;");
                            Console.Write("\nВыберите статус заказа> ");
                            string statusOrder = Console.ReadLine();

                            sqlCommand = new SqlCommand($"update orders " +
                                                        $"set status = {statusOrder} " +
                                                        $"where id = {idProduct}", sqlConnection);

                            sqlDataReader = sqlCommand.ExecuteReader();

                            if (sqlDataReader != null)
                            {
                                Console.WriteLine("Операция выполнена успешно");
                                sqlDataReader.Close();
                            }
                            break;

                        default:
                            break;
                    }

                    break;
                }
            }
            Console.WriteLine("Для продолжения нажмите любую клавишу...");
        }
    }

}
