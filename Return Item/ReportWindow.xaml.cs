using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Return_Item
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {

        private int itemID;
        private int borrowedQuantity;
        private string connectionString = "Data Source=MSI\\SQLEXPRESS;Initial Catalog=AvailableItems;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";
        public event Action ReportSubmitted;
        public ReportWindow(int itemID, int borrowedQuantity)
        {
            InitializeComponent();
            this.itemID = itemID;
            this.borrowedQuantity = borrowedQuantity;
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string ReportStatus = ReportStatusTextBox.Text;
            int quantityToReport;

            // Validate input
            if (string.IsNullOrWhiteSpace(ReportStatus))
            {
                MessageBox.Show("Please fill in all fields before submitting.");
                return;
            }

            if (!int.TryParse(QuantityTextbox.Text, out quantityToReport) || quantityToReport <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            if (quantityToReport > borrowedQuantity)
            {
                MessageBox.Show("Reported quantity cannot be greater than borrowed quantity.");
                return;
            }

            // Generate a random 5-digit Reported_ID
            Random rand1 = new Random();
            int reportedID = rand1.Next(10000, 99999);

            // Generate a random 5-digit Reported_ID
            Random rand2 = new Random();
            int ActivityID = rand2.Next(10000, 99999);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 🔹 1️⃣ Insert into ReportedItems
                    string insertQuery = @"
                INSERT INTO ReportedItems (Reported_ID, Item_ID, Report_Status, Activity_ID, Item_Quantity)
                VALUES (@ReportedID, @ItemID, @ReportStatus, @ActivityID, @ReportedQuantity)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ReportedID", reportedID);
                        cmd.Parameters.AddWithValue("@ItemID", itemID);
                        cmd.Parameters.AddWithValue("@ReportStatus", ReportStatus);
                        cmd.Parameters.AddWithValue("@ActivityID", ActivityID);
                        cmd.Parameters.AddWithValue("@ReportedQuantity", quantityToReport);
                        cmd.ExecuteNonQuery();
                    }

                    // 🔹 2️⃣ Calculate new borrowed quantity
                    int newBorrowedQuantity = borrowedQuantity - quantityToReport;

                    if (newBorrowedQuantity == 0)
                    {
                        // 🔹 3️⃣ Delete BorrowedItem if quantity is 0
                        string deleteQuery = "DELETE FROM BorrowedItems WHERE Item_ID = @ItemID";

                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ItemID", itemID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // 🔹 4️⃣ Update BorrowedQuantity
                        string updateQuery = @"
                    UPDATE BorrowedItems
                    SET Borrowed_Quantity = @NewQuantity
                    WHERE Item_ID = @ItemID";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NewQuantity", newBorrowedQuantity);
                            cmd.Parameters.AddWithValue("@ItemID", itemID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    // 🔹 Notify MainWindow that a report was submitted
                    ReportSubmitted?.Invoke();

                    MessageBox.Show("Report submitted successfully.");
                    this.Close(); // Close the report window
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
