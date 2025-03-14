using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace Return_Item
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = "Data Source=MSI\\SQLEXPRESS;Initial Catalog=AvailableItems;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                DataTable dt = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM BorrowedItems", conn);
                adapter.Fill(dt);
                myDataGrid.ItemsSource = dt.DefaultView; // Refresh DataGrid
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView row)
            {
                string itemName = row["Item_Name"].ToString();
                int borrowedQuantity = Convert.ToInt32(row["Borrowed_Quantity"]);
                int borrowedID = Convert.ToInt32(row["Borrowed_ID"]);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // 🔹 1️⃣ Add Borrowed_Quantity to AvailableItems.Item_Quantity
                        string updateAvailableItems = @"
                            UPDATE AvailableItems
                            SET Item_Quantity = Item_Quantity + @BorrowedQuantity
                            WHERE Item_Name = @ItemName";

                        using (SqlCommand cmd = new SqlCommand(updateAvailableItems, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BorrowedQuantity", borrowedQuantity);
                            cmd.Parameters.AddWithValue("@ItemName", itemName);
                            cmd.ExecuteNonQuery();
                        }

                        // 🔹 2️⃣ Delete the Borrowed Item
                        string deleteBorrowedItem = "DELETE FROM BorrowedItems WHERE Borrowed_ID = @BorrowedID";

                        using (SqlCommand cmd = new SqlCommand(deleteBorrowedItem, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BorrowedID", borrowedID);
                            cmd.ExecuteNonQuery();
                        }

                        // 🔹 3️⃣ Commit Transaction
                        transaction.Commit();
                        MessageBox.Show("Item returned successfully!");

                        // Reload DataGrid
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }
    }
}