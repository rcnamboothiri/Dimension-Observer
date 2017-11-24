using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;


namespace DimensionObserver
{    
    public partial class MainForm : Form
    {
        public const String startMonitoring = "Start Monitoring";
        public const String stopMonitoring = "Stop Monitoring";

        public MainForm()
        {
            InitializeComponent();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest_1(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Create FolderBrowserDialog object.
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            // Show a button to create a new folder.
            folderBrowserDialog.ShowNewFolderButton = true;
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();
            // Get selected path from FolderBrowserDialog control.
            if (dialogResult == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog.SelectedPath;
                Environment.SpecialFolder root = folderBrowserDialog.RootFolder;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Create a new FileSystemWatcher object.
            FileSystemWatcher fsWatcher = new FileSystemWatcher();
            switch (button2.Text)
            {
                // Start Monitoring…
                case startMonitoring:
                    if (!textBox1.Text.Equals(String.Empty))
                    {
                        listBox1.Items.Add("Started FileSystemWatcher Service…");
                        fsWatcher.Path = textBox1.Text;
                        // Set Filter.
                        fsWatcher.Filter = (textBox2.Text.Equals(String.Empty)) ? "*.*" : textBox2.Text;
                        // Monitor files and subdirectories.
                        fsWatcher.IncludeSubdirectories = true;
                        // Monitor all changes specified in the NotifyFilters.
                        fsWatcher.NotifyFilter = NotifyFilters.Attributes |
                                                 NotifyFilters.CreationTime |
                                                 NotifyFilters.DirectoryName |
                                                 NotifyFilters.FileName |
                                                 NotifyFilters.LastAccess |
                                                 NotifyFilters.LastWrite |
                                                 NotifyFilters.Security |
                                                 NotifyFilters.Size;
                        fsWatcher.EnableRaisingEvents = true;
                        // Raise Event handlers.
                        // fsWatcher.Changed += new FileSystemEventHandler(OnChanged);
                        fsWatcher.Created += new FileSystemEventHandler(OnCreated);
                       // fsWatcher.Deleted += new FileSystemEventHandler(OnDeleted);
                       // fsWatcher.Renamed += new RenamedEventHandler(OnRenamed);
                       // fsWatcher.Error += new ErrorEventHandler(OnError);
                        button2.Text = stopMonitoring;
                        textBox1.Enabled = false;
                        textBox2.Enabled = false;
                    }
                    else
                    {
                        listBox1.Items.Add("Please select folder to monitor….");
                    }
                    break;
                // Stop Monitoring…
                case stopMonitoring:
                default:
                    fsWatcher.EnableRaisingEvents = false;
                    fsWatcher = null;
                    button2.Text = startMonitoring;
                    textBox1.Enabled = true;
                    textBox2.Enabled = true;
                    listBox1.Items.Add("Stopped FileSystemWatcher Service…");
                    break;
            }

        }
        // FileSystemWatcher – OnCreated Event Handler
        public void OnCreated(object sender, FileSystemEventArgs e)
        {
            // Add event details in listbox.
            this.Invoke((MethodInvoker)delegate { listBox1.Items.Add("--------------------------------"); });
            this.Invoke((MethodInvoker)delegate { listBox1.Items.Add(String.Format("Received Filename: {1}", e.FullPath,e.Name, e.ChangeType));});
           
            UpdateDatabase(sender,e);
        }

        public void UpdateDatabase(object sender, FileSystemEventArgs e)
        {
            // Add event details in listbox.
             this.Invoke((MethodInvoker)delegate { listBox1.Items.Add(String.Format("Processing Filename: {1}", e.FullPath, e.Name, e.ChangeType)); });
            SqlConnection con;

            string sqlconn;

            sqlconn = ConfigurationManager.ConnectionStrings["SqlCom"].ConnectionString;
            con = new SqlConnection(sqlconn);

            //Creating object of datatable  
            DataTable tblcsv = new DataTable();
            //creating columns  
            tblcsv.Columns.Add("sl no");
            tblcsv.Columns.Add("Dimension");

            //getting full file path of Uploaded file  
            string CSVFilePath = e.FullPath;
            this.Invoke((MethodInvoker)delegate { listBox1.Items.Add(String.Format("FullPath: {0}", e.FullPath, e.Name, e.ChangeType)); });
            //Reading All text  
            string ReadCSV = File.ReadAllText(CSVFilePath);
            //spliting row after new line  
            foreach (string csvRow in ReadCSV.Split('\n'))
            {
                if (!string.IsNullOrEmpty(csvRow))
                {
                    //Adding each row into datatable  
                    tblcsv.Rows.Add();
                    int count = 0;
                    foreach (string FileRec in csvRow.Split(','))
                    {
                        tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec;
                        count++;
                    }
                }


            }
            //Calling insert Functions  
            //creating object of SqlBulkCopy    
            SqlBulkCopy objbulk = new SqlBulkCopy(con);
            //assigning Destination table name    
            objbulk.DestinationTableName = "Dimension";
            //Mapping Table column    
            objbulk.ColumnMappings.Add("sl no", "sl no");
            objbulk.ColumnMappings.Add("Dimension", "Dimension");

            //inserting Datatable Records to DataBase    
            con.Open();
            objbulk.WriteToServer(tblcsv);
            con.Close();
            this.Invoke((MethodInvoker)delegate { listBox1.Items.Add(String.Format("--------------------------------")); });
        }
    }
}
