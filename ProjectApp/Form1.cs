using ProjectApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace ProjectApp
{
    public partial class ProjectApp : Form
    {
        private DataTable projectApp = new DataTable();
        private SQLiteConnection sqliteConnection;
        public ProjectApp()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Set your SQLite database file path
            string dbFilePath = "C:\\Users\\Fabrice ELONO PISETH\\source\\repos\\ProjectApp\\ProjectApp\\projects.db";

            // Connection string for SQLite
            string connectionString = $"Data Source={dbFilePath};Version=3;";

            // Initialize SQLite connection
            sqliteConnection = new SQLiteConnection(connectionString);

            // Create the projects table if it doesn't exist
            using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS projects (Title TEXT, Description TEXT, StartDate DATE, EndDate DATE)", sqliteConnection))
            {
                sqliteConnection.Open();
                command.ExecuteNonQuery();
                sqliteConnection.Close();
            }
        }

        //Creating our data tabe I will use SQLite later (it will store our data)
        /*DataTable projectApp = new DataTable();*/
        bool isEditing = false;

        private void ProjectApp_Load(object sender, EventArgs e)
        {
            //Now creating some columns for title and description
            projectApp.Columns.Add("Id");
            projectApp.Columns.Add("Title");
            projectApp.Columns.Add("Description");
            projectApp.Columns.Add("StartDate");
            projectApp.Columns.Add("EndDate");

            //Now pointing the datagridview to display our data source
            projectAppView.DataSource = projectApp;
            // Load data from the SQLite database when the form loads
            LoadDataFromDatabase();
        }
        private void LoadDataFromDatabase()
        {
            // Clear existing data in the DataTable
            projectApp.Rows.Clear();

            // Retrieve data from the SQLite database and populate the DataTable
            using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM projects", sqliteConnection))
            {
                sqliteConnection.Open();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["Id"]);
                        string title = reader["Title"].ToString();
                        string description = reader["Description"].ToString();
                        // Check if "StartDate" is DBNull or null
                        DateTime startDate = (reader["StartDate"] == DBNull.Value || reader["StartDate"] == null) ? DateTime.MinValue : Convert.ToDateTime(reader["StartDate"]);

                        // Check if "EndDate" is DBNull or null
                        DateTime endDate = (reader["EndDate"] == DBNull.Value || reader["EndDate"] == null) ? DateTime.MinValue : Convert.ToDateTime(reader["EndDate"]);

                        projectApp.Rows.Add(id, title, description, startDate, endDate);
                    
                    // Concatenate "Project" to the Id for display
                    // string displayId = "Project " + id;

                    // Add the retrieved values to the DataTable
                    //projectApp.Rows.Add( id,title, description);
                    }
                }
                sqliteConnection.Close();
            }
        }

        private void newButton_Click(object sender, EventArgs e)
        {//new text box
            titleTextBox.Text = "";  // Deleting or erasing the data in the title text box to be able to add a new one
            descriptonTextBox.Text = "";
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            //extracting a past item (title and description)
            isEditing = true; //to be able to track if it is editing
            //Use text fields with data from the table
            //retreive data from our database created at projectApp
            titleTextBox.Text = projectApp.Rows[projectAppView.CurrentCell.RowIndex].ItemArray[1].ToString();
            descriptonTextBox.Text = projectApp.Rows[projectAppView.CurrentCell.RowIndex].ItemArray[2].ToString();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                int rowIndex = projectAppView.CurrentCell.RowIndex;

                // Check if the selected cell is valid
                if (rowIndex >= 0 && rowIndex < projectApp.Rows.Count)
                {
                    // Get values from the selected row
                    string title = projectApp.Rows[rowIndex]["Title"].ToString();
                    string description = projectApp.Rows[rowIndex]["Description"].ToString();

                    // Delete the row from the DataTable
                    projectApp.Rows[rowIndex].Delete();

                    // Delete the corresponding record from the database
                    DeleteRecordFromDatabase(title, description);

                    // Refresh the DataGridView
                    projectApp.AcceptChanges();
                    projectAppView.Refresh();

                    ShowPopup("Successfully Deleted!");
                }
                else
                {
                    Console.WriteLine($"Invalid row index: {rowIndex}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }

        private void DeleteRecordFromDatabase(string title, string description)
        {
            try
            {
                using (SQLiteCommand command = new SQLiteCommand("DELETE FROM projects WHERE Title = @Title AND Description = @Description", sqliteConnection))
                {
                    sqliteConnection.Open();
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Description", description);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting record from database: " + ex.Message);
            }
            finally
            {
                sqliteConnection.Close();
            }
        }


        private void saveButton_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                projectApp.Rows[projectAppView.CurrentCell.RowIndex]["Title"] = titleTextBox.Text;
                projectApp.Rows[projectAppView.CurrentCell.RowIndex]["Description"] = descriptonTextBox.Text;
                // Update existing record in the SQLite database
                UpdateRecordInDatabase();
            }
            else
            {
                // Add a new row with the correct order of columns
                projectApp.Rows.Add(null, titleTextBox.Text, descriptonTextBox.Text);
                // Insert a new record into the SQLite database
                InsertRecordIntoDatabase();
            }
            //Clear fields
            titleTextBox.Text = "";
            descriptonTextBox.Text = "";
            startDatePicker.Value = DateTime.Now; // Reset to default value
            endDatePicker.Value = DateTime.Now;   // Reset to default value
            endDatePicker.Checked = false;        // Reset Checked property
            isEditing = false; //since done with editing
                               // Show a success message
                               // Show a pop-up message
            ShowPopup("Successfully saved!");
        }
        //To be able to be in synchrony with my SQLite database I will add a refresh method
        private void RefreshData()
        {
            // Clear existing data in the DataTable
            projectApp.Rows.Clear();

            // Retrieve data from the SQLite database and populate the DataTable
            using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM projects", sqliteConnection))
            {
                sqliteConnection.Open();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["Id"]);
                        string title = reader["Title"].ToString();
                        string description = reader["Description"].ToString();

                        // Check if "StartDate" is DBNull or null
                        DateTime startDate = (reader["StartDate"] == DBNull.Value || reader["StartDate"] == null) ? DateTime.MinValue : Convert.ToDateTime(reader["StartDate"]);

                        // Check if "EndDate" is DBNull or null
                        DateTime endDate = (reader["EndDate"] == DBNull.Value || reader["EndDate"] == null) ? DateTime.MinValue : Convert.ToDateTime(reader["EndDate"]);

                        // Add the retrieved values to the DataTable
                        projectApp.Rows.Add(id, title, description, startDate, endDate);
                    }
                }
                sqliteConnection.Close();
            }

            // Refresh the DataGridView
            projectApp.AcceptChanges();
            projectAppView.Refresh();
        }

        private void InsertRecordIntoDatabase()
        {
            using (SQLiteCommand command = new SQLiteCommand("INSERT INTO projects (Title, Description, StartDate, EndDate) VALUES (@Title, @Description, @StartDate, @EndDate)", sqliteConnection))
            {
                sqliteConnection.Open();
                command.Parameters.AddWithValue("@Title", titleTextBox.Text);
                command.Parameters.AddWithValue("@Description", descriptonTextBox.Text);
                command.Parameters.AddWithValue("@StartDate", startDatePicker.Value);
                command.Parameters.AddWithValue("@EndDate", (endDatePicker.Checked) ? (object)endDatePicker.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Id", projectApp.Rows[projectAppView.CurrentCell.RowIndex]["Id"]);
                command.ExecuteNonQuery();
                sqliteConnection.Close();
            }
            // Refresh data after making changes
            RefreshData();
        }

        private void UpdateRecordInDatabase()
        {
            using (SQLiteCommand command = new SQLiteCommand("UPDATE projects SET Title = @Title, Description = @Description, StartDate = @StartDate, EndDate = @EndDate WHERE Id = @Id", sqliteConnection))
            {
                sqliteConnection.Open();
                command.Parameters.AddWithValue("@Title", titleTextBox.Text);
                command.Parameters.AddWithValue("@Description", descriptonTextBox.Text);
                command.Parameters.AddWithValue("@StartDate", startDatePicker.Value);
                command.Parameters.AddWithValue("@EndDate", (endDatePicker.Checked) ? (object)endDatePicker.Value : DBNull.Value);


                command.Parameters.AddWithValue("@Id", projectApp.Rows[projectAppView.CurrentCell.RowIndex]["Id"]);
                command.ExecuteNonQuery();
                sqliteConnection.Close();
            }
            // Refresh data after making changes
            RefreshData();
        }

        private async void ShowPopup(string message)
        {
            //first method
            /*  // Use a ToolTip to show a pop-up message for a short duration
              ToolTip tooltip = new ToolTip();
              tooltip.Show(message, this, 0, 0, 2000); // Display for 2000 milliseconds (2 seconds)

              // Wait for the duration of the tooltip to disappear
              await Task.Delay(2000);

              // Dispose the tooltip after it's no longer needed
              tooltip.Dispose();*/



            using (var popupForm = new Form())
            {
                popupForm.FormBorderStyle = FormBorderStyle.None;
                popupForm.StartPosition = FormStartPosition.Manual;
                popupForm.BackColor = Color.Green;
                popupForm.Size = new Size(200, 100);
                popupForm.Location = new Point(this.Left + (this.Width - popupForm.Width) / 2, this.Top + (this.Height - popupForm.Height) / 2);

                var label = new Label
                {
                    Text = message,
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font(Font.FontFamily, 12, FontStyle.Bold)
                };

                popupForm.Controls.Add(label);

                popupForm.Show();

                await Task.Delay(2000); // Display for 2000 milliseconds (2 seconds)

                popupForm.Close();
            }
        }

       
    }
}
