using Microsoft.Win32;
using MonkeyMotionControl.Properties;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MonkeyMotionControl.UI
{

    public class RobotPresetsTableRow
    {
        public int Id = 0;
        public string Name = "";
        public string Config = "";
        public double J1 = 0;
        public double J2 = 0;
        public double J3 = 0;
        public double J4 = 0;
        public double J5 = 0;
        public double J6 = 0;
        public double Focus = 0;
        public double Iris = 0;
        public double Zoom = 0;
        public double Aux = 0;

        public RobotPresetsTableRow(int id, string name, string config, double j1, double j2, double j3, double j4, double j5, double j6, double focus, double iris, double zoom, double aux)
        {
            Id = id;
            Name = name;
            Config = config;
            J1 = j1;
            J2 = j2;
            J3 = j3;
            J4 = j4;
            J5 = j5;
            J6 = j6;
            Focus = focus;
            Iris = iris;
            Zoom = zoom;
            Aux = aux;
        }
    }
    
    /// <summary>
    /// Interaction logic for TableCommands.xaml
    /// </summary>
    public partial class TableRobotPresets : UserControl, INotifyPropertyChanged
    {

        #region EVENTS

        public delegate void LogEventHandler(object sender, LogEventArgs args);
        public event LogEventHandler LogEvent;
        public delegate void ErrorLogEventHandler(object sender, LogEventArgs args);
        public event ErrorLogEventHandler ErrorLogEvent;
        public delegate void AddEventHandler(object sender, RoutedEventArgs args);
        public event AddEventHandler AddEvent;
        public delegate void GotoSelectedEventHandler(object sender, RoutedEventArgs args);
        public event GotoSelectedEventHandler GotoSelectedEvent;
        public delegate void TableLoadedEventHandler(object sender, RoutedEventArgs args);
        public event TableLoadedEventHandler TableLoadedEvent;
        public delegate void ClearEventHandler(object sender, RoutedEventArgs args);
        public event ClearEventHandler ClearEvent;
        public delegate void DeleteEventHandler(object sender, RoutedEventArgs args);
        public event DeleteEventHandler DeleteEvent;

        #endregion

        #region PROPERTIES
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Console.WriteLine($"State Changed: {propertyName}");
        }

        private int Count
        {
            get
            {
                return datatable.Rows.Count;
            }
        }
        private bool IsEmpty
        {
            get
            {
                return Count > 0 ? false : true;
            }

        }
        private DataTable datatable { get; set; }
        private bool isDatatableInitialized { get; set; } = false;
        private int pointCounter { get; set; } = 1;
        
        #endregion

        public TableRobotPresets()
        {
            InitializeComponent();

            // Initialize DataTable
            datatable = new DataTable("CommandsSequence");
            datatable.Columns.Add("ID", System.Type.GetType("System.Int32"));
            datatable.Columns.Add("Name", System.Type.GetType("System.String"));
            datatable.Columns.Add("Config", System.Type.GetType("System.String")); // Camera configuration Front, Under, etc
            datatable.Columns.Add("J1", System.Type.GetType("System.Double"));
            datatable.Columns.Add("J2", System.Type.GetType("System.Double"));
            datatable.Columns.Add("J3", System.Type.GetType("System.Double"));
            datatable.Columns.Add("J4", System.Type.GetType("System.Double"));
            datatable.Columns.Add("J5", System.Type.GetType("System.Double"));
            datatable.Columns.Add("J6", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Focus", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Iris", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Zoom", System.Type.GetType("System.Double"));
            datatable.Columns.Add("AuxMot", System.Type.GetType("System.Double"));
            for (int i = 0; i < datatable.Columns.Count; i++)
            {
                datatable.Columns[i].AllowDBNull = false; // Make all columns required.
            }
            DataColumn[] unique_cols =
            {
                datatable.Columns["ID"],
                datatable.Columns["Name"]
            };
            datatable.Constraints.Add(new UniqueConstraint(unique_cols));
            datatable.RowChanged += TableRowChanged;
            DataGrid_RobotPresets.DataContext = datatable.DefaultView;
            DataGrid_RobotPresets.MinColumnWidth = 40;
            isDatatableInitialized = true;
            UIState_TableEmpty();
        }

        private void Log(string msg)
        {
            LogEvent?.Invoke(this, new LogEventArgs(msg));
        }

        private void ErrorLog(string msg)
        {
            ErrorLogEvent?.Invoke(this, new LogEventArgs(msg));
        }

        private DataRow GetRow(int row)
        {
            try
            {
                return datatable.Rows[row];
            }
            catch (Exception e)
            {
                return null;
            }

        }

        private DataRow GetLastRow()
        {
            try
            {
                return datatable.Rows[datatable.Rows.Count - 1];
            }
            catch (Exception e)
            {
                return null;
            }

        }
        
        private DataRow GetSelected_DataRow()
        {
            try
            {
                var index = DataGrid_RobotPresets.SelectedIndex;
                return datatable.Rows[index];
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private DataRowView GetSelected_DataRowView()
        {
            return (DataRowView)DataGrid_RobotPresets.SelectedItem;
        }

        private DataRowCollection GetRows()
        {
            try
            {
                return datatable.Rows;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private RobotPresetsTableRow GetRowData(int rowNumber)
        {
            try
            {
                DataRow row = datatable.Rows[rowNumber];
                if (row == null) return null;

                //if (string.Compare((string)row["Config"], "FRONT") == 0)
                //    RobotConfig = "FRONT";
                //Console.WriteLine($"RowID: {row.Field<int>("ID")}");
                
                return new RobotPresetsTableRow(
                        row.Field<int>("ID"),
                        row.Field<string>("Name"),
                        row.Field<string>("Config"),
                        row.Field<double>("J1"),
                        row.Field<double>("J2"),
                        row.Field<double>("J3"),
                        row.Field<double>("J4"),
                        row.Field<double>("J5"),
                        row.Field<double>("J6"),
                        row.Field<double>("Focus"),
                        row.Field<double>("Iris"),
                        row.Field<double>("Zoom"),
                        row.Field<double>("AuxMot")
                ); 
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private bool LoadFile(string filename)
        {
            try
            {
                datatable.Clear();
                datatable.ReadXml(filename);
                pointCounter = Count + 1;
                if (!IsEmpty)
                {
                    TableLoadedEvent?.Invoke(this, new RoutedEventArgs());
                    UIState_TableNotEmpty();
                    IsDatatableChanged = false;
                    return true;
                }
                else
                {
                    ErrorLog($"Load DataTable Error.");
                    UIPopUp_LoadDatatableError();
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Load DataTable Error: {ex.Message}");
                UIPopUp_LoadDatatableError();
                return false;
            }
        }

        private bool IsDatatableChanged { get; set; } = false;

        private void SaveFile(string filename)
        {
            try
            {
                if (IsDatatableChanged && !IsEmpty && Count >= 1)
                {
                    datatable.WriteXml(filename, XmlWriteMode.WriteSchema);
                    Log("Saved Datatable.");
                    IsDatatableChanged = false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Save DataTable Error: {ex.Message}");
            }
        }
        
        private bool AddRow(RobotPresetsTableRow rowData)
        {
            try
            {
                var name = "Preset" + pointCounter.ToString();
                datatable.Rows.Add(
                    new object[] {
                        pointCounter,               /// ID
                        name,                       /// Name
                        rowData.Config,             /// Config
                        Math.Round(rowData.J1,2),   /// J1
                        Math.Round(rowData.J2,2),   /// J2
                        Math.Round(rowData.J3,2),   /// J3
                        Math.Round(rowData.J4,2),   /// J4
                        Math.Round(rowData.J5,2),   /// J5
                        Math.Round(rowData.J6,2),   /// J6
                        Math.Round(rowData.Focus,2),/// Focus
                        Math.Round(rowData.Iris,2), /// Iris
                        Math.Round(rowData.Zoom,2), /// Zoom
                        Math.Round(rowData.Aux,2)  /// Ext Motor
                    }
                );
                pointCounter++;
                Log($"Added preset row '{name}'.");
                IsDatatableChanged = true;
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Add Preset Row Error: {ex.Message}");
                return false;
            }
        }

        private bool EditRow(int rowIndex, RobotPresetsTableRow rowData)
        {
            try
            {
                datatable.Rows[rowIndex].SetField("Name", rowData.Name);
                datatable.Rows[rowIndex].SetField("Config", rowData.Config);
                datatable.Rows[rowIndex].SetField("J1", Math.Round(rowData.J1, 2));
                datatable.Rows[rowIndex].SetField("J2", Math.Round(rowData.J2, 2));
                datatable.Rows[rowIndex].SetField("J3", Math.Round(rowData.J3, 2));
                datatable.Rows[rowIndex].SetField("J4", Math.Round(rowData.J4, 2));
                datatable.Rows[rowIndex].SetField("J5", Math.Round(rowData.J5, 2));
                datatable.Rows[rowIndex].SetField("J6", Math.Round(rowData.J6, 2));
                datatable.Rows[rowIndex].SetField("Focus", rowData.Focus);
                datatable.Rows[rowIndex].SetField("Iris", rowData.Iris);
                datatable.Rows[rowIndex].SetField("Zoom", rowData.Zoom);
                datatable.Rows[rowIndex].SetField("AuxMot", rowData.Aux);
                Log($"Edit Preset Row {rowIndex}: '{datatable.Rows[rowIndex].Field<string>("Name")}'.");
                IsDatatableChanged = true;
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Edit Preset Row {rowIndex} Error: {ex.Message}");
                return false;
            }
        }

        private bool DeleteRow()
        {
            try
            {
                if (!IsEmpty)
                {
                    if (DataGrid_RobotPresets.SelectedItem != null)
                    {
                        DataRowView row = (DataRowView)DataGrid_RobotPresets.SelectedItem;
                        datatable.Rows.Remove(row.Row);
                        IsDatatableChanged = true;
                        if (IsEmpty)
                        {
                            UIState_TableEmpty();
                        }
                        return true;
                    }
                    else
                    {
                        ErrorLog($"Delete Row Error: Invalid Selection");
                        return false;
                    }
                }
                else
                {
                    ErrorLog($"Delete Row Error: Table Empty.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Delete Row Error: {ex.Message}");
                return false;
            }
        }

        private void Clear()
        {
            datatable.Clear();
            pointCounter = 1;
            UIState_TableEmpty();
            IsDatatableChanged = false;
            ClearEvent?.Invoke(this, new RoutedEventArgs());
        }

        public RobotPresetsTableRow GetSelectedRowData()
        {
            var selectedIndex = DataGrid_RobotPresets.SelectedIndex;
            if (selectedIndex < 0) return null;
            return GetRowData(selectedIndex);
        }

        public void AddPreset(string name, string config, double j1, double j2, double j3, double j4, double j5, double j6, double focus, double iris, double zoom, double aux)
        {
            if(AddRow(new RobotPresetsTableRow(-1, name, config, j1, j2, j3, j4, j5,j6, focus, iris, zoom, aux)))
            {
                UIState_TableNotEmpty();
            }
        }

        public void EditPreset(int rowIndex, string name, string config, double j1, double j2, double j3, double j4, double j5, double j6, double focus, double iris, double zoom, double aux)
        {
            EditRow(rowIndex, new RobotPresetsTableRow(-1, name, config, j1, j2, j3, j4, j5, j6, focus, iris, zoom, aux));
        }

        public void Close()
        {
            Save();
        }

        public void Save()
        {
            SaveFile($"robot_presets.xml");
        }

        public void Load()
        {
            LoadFile($"robot_presets.xml");
        }

        public bool ClearTable()
        {
            if (!IsEmpty)
            {
                if (UIPopUp_ClearDatatableWarning())
                {
                    Clear();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #region BUTTON HANDLERS & UI METHODS

        private void TableRowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (isDatatableInitialized)
            {
                //Check each column for correct data input and range
                //Log($"Table row {e.Row["ID"]} ({e.Row["Name"]}) changed.");
            }
        }

        private void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            AddEvent?.Invoke(this, new RoutedEventArgs());
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteRow();
        }

        private void Btn_SaveFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                ErrorLog($"Save File Error: {ex.Message}");
            }
        }

        private void Btn_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool proceed = false;
                if (!IsEmpty)
                    proceed = UIPopUp_LoadDatatableWarning();
                else 
                    proceed = true;

                if (proceed)
                {
                    Load();
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Open File Error: {ex.Message}");
            }
        }

        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearTable();
            }
            catch (Exception ex)
            {
                ErrorLog($"Clear Table Error: {ex.Message}");
            }
        }

        private void Btn_GotoSelected_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEmpty)
            { 
                GotoSelectedEvent?.Invoke(this, new RoutedEventArgs());
            }
        }

        private bool UIPopUp_ClearDatatableWarning()
        {
            return CustomMessageBox.ShowWarning("Are you sure you want to CLEAR table?", "CLEAR PRESETS DATATABLE");
        }

        private bool UIPopUp_LoadDatatableWarning()
        {
            return CustomMessageBox.ShowWarning("Are you sure you want to LOAD new data to table?", "LOAD PRESETS DATATABLE");
        }
        
        private bool UIPopUp_LoadDatatableError()
        {
            return CustomMessageBox.ShowError("Error Loading Datatable from file.", "LOAD PRESETS DATATABLE ERROR");
        }

        private void UIState_TableEmpty()
        {
            Btn_Clear.IsEnabled = false;
            Btn_Delete.IsEnabled = false;
            Btn_SaveFile.IsEnabled = false;
            Btn_GotoSelected.IsEnabled = false;
        }

        private void UIState_TableNotEmpty()
        {
            Btn_Clear.IsEnabled = true;
            Btn_Delete.IsEnabled = true;
            Btn_SaveFile.IsEnabled = true;
            Btn_GotoSelected.IsEnabled = true;            
        }

        #endregion

    }
}
