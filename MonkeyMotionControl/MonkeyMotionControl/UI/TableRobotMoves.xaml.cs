using Microsoft.Win32;
using MonkeyMotionControl.Properties;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MonkeyMotionControl.UI
{
    // Use CommandsMoveType

    public class TableRobotMovesPoint
    {
        public double X = 0;
        public double Y = 0;
        public double Z = 0;
        public double RX = 0;
        public double RY = 0;
        public double RZ = 0;
        public double Focus = 0;
        public double Iris = 0;
        public double Zoom = 0;
        public double Aux = 0;
        

        public TableRobotMovesPoint()
        {
        }
        
        public TableRobotMovesPoint(double x, double y, double z, double rx, double ry, double rz, double focus, double iris, double zoom, double aux)
        {
            X = x;
            Y = y;
            Z = z;
            RX = rx;
            RY = ry;
            RZ = rz;
            Focus = focus;
            Iris = iris;
            Zoom = zoom;
            Aux = aux;
        }
    }  

    public class TableRobotMovesRow
    {
        public int Id = 0;
        public string Name = "";
        public TableRobotMovesPoint StartPoint;
        public TableRobotMovesPoint MidPoint;
        public TableRobotMovesPoint EndPoint;
        public CommandsMoveType MoveType;
        public double Vel = 0;
        public double Acc = 0;
        public double Dec = 0;
        public double Leave = 0;
        public double Reach = 0;
        public TableRobotMovesRow (TableRobotMovesPoint startPt, TableRobotMovesPoint midPt, TableRobotMovesPoint endPt, int id, string name, CommandsMoveType movetype, double vel, double acc, double dec, double leave, double reach)
        {
            StartPoint = startPt;
            MidPoint = midPt;
            EndPoint = endPt;
            Id = id;
            Name = name;
            MoveType = movetype;
            Acc = acc;
            Dec = dec;
            Vel = vel;
            Leave = leave;
            Reach = reach;
        }
    }
    
    /// <summary>
    /// Interaction logic for TableCommands.xaml
    /// </summary>
    public partial class TableRobotMoves : UserControl, INotifyPropertyChanged
    {

        #region EVENTS

        public delegate void LogEventHandler(object sender, LogEventArgs args);
        public event LogEventHandler LogEvent;
        public delegate void ErrorLogEventHandler(object sender, LogEventArgs args);
        public event ErrorLogEventHandler ErrorLogEvent;
        public delegate void AddLinearMoveEventHandler(object sender, RoutedEventArgs args);
        public event AddLinearMoveEventHandler AddLinearMoveEvent;
        public delegate void EditMoveEventHandler(object sender, RoutedEventArgs args);
        public event EditMoveEventHandler EditMoveEvent;
        public delegate void AddCircularMoveEventHandler(object sender, RoutedEventArgs args);
        public event AddCircularMoveEventHandler AddCircularMoveEvent;
        public delegate void TableLoadedEventHandler(object sender, RoutedEventArgs args);
        public event TableLoadedEventHandler TableLoadedEvent;
        
        #endregion

        #region PROPERTIES
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Console.WriteLine($"State Changed: {propertyName}");
        }

        public int Count
        {
            get
            {
                return datatable.Rows.Count;
            }
        }

        public bool IsEmpty
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

        public TableRobotMoves()
        {
            InitializeComponent();

            // Initialize DataTable
            datatable = new DataTable("RobotMoves");
            datatable.Columns.Add("ID", System.Type.GetType("System.Int32"));
            datatable.Columns.Add("Name", System.Type.GetType("System.String"));
            datatable.Columns.Add("Move", System.Type.GetType("System.String"));
            
            datatable.Columns.Add("StartX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("StartY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("StartZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("StartRX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("StartRY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("StartRZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("StartFocus", System.Type.GetType("System.Double"));
            datatable.Columns.Add("StartIris", System.Type.GetType("System.Double"));
            datatable.Columns.Add("StartZoom", System.Type.GetType("System.Double"));
            datatable.Columns.Add("StartAuxMot", System.Type.GetType("System.Double"));

            datatable.Columns.Add("MidX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidRX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidRY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidRZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidFocus", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidIris", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidZoom", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidAuxMot", System.Type.GetType("System.Double"));
            
            datatable.Columns.Add("EndX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("EndY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("EndZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("EndRX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("EndRY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("EndRZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("EndFocus", System.Type.GetType("System.Double"));
            datatable.Columns.Add("EndIris", System.Type.GetType("System.Double"));
            datatable.Columns.Add("EndZoom", System.Type.GetType("System.Double"));
            datatable.Columns.Add("EndAuxMot", System.Type.GetType("System.Double"));
            
            datatable.Columns.Add("Vel", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Acc", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Dec", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Leave", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Reach", System.Type.GetType("System.Double"));
            
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
            DataGrid_RobotMoves.DataContext = datatable.DefaultView;
            DataGrid_RobotMoves.MinColumnWidth = 40;
            pointCounter = 1;
            isDatatableInitialized = true;
            IsDatatableChanged = false;
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

        public int GetSelectedRowIndex()
        {
            try
            {
                return DataGrid_RobotMoves.SelectedIndex;
            }
            catch(Exception ex)
            {
                ErrorLog($"Selected Row Index Error: {ex}");
                return -1;
            }
        }

        private DataRow DataTable_GetSelected_DataRow()
        {
            try
            {
                var index = DataGrid_RobotMoves.SelectedIndex;
                return datatable.Rows[index];
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private DataRowView DataTable_GetSelected_DataRowView()
        {
            return (DataRowView)DataGrid_RobotMoves.SelectedItem;
        }

        private DataRowCollection DataTable_GetRows()
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
        
        public TableRobotMovesRow GetRowData(int rowNumber)
        {
            try
            {
                DataRow row = datatable.Rows[rowNumber];
                if (row == null) return null;

                CommandsMoveType moveType;
                if (string.Compare((string)row["Move"], "LINEAR") == 0)
                    moveType = CommandsMoveType.LINEAR;
                else if (string.Compare((string)row["Move"], "CIRCULAR") == 0)
                    moveType = CommandsMoveType.CIRCULAR;
                else
                {
                    ErrorLog("Indefined Move Type");
                    return null;
                }
                //Console.WriteLine($"RowID: {row.Field<int>("ID")}");
                return new TableRobotMovesRow(
                    new TableRobotMovesPoint(
                        row.Field<double>("StartX"),
                        row.Field<double>("StartY"),
                        row.Field<double>("StartZ"),
                        row.Field<double>("StartRX"),
                        row.Field<double>("StartRY"),
                        row.Field<double>("StartRZ"),
                        row.Field<double>("StartFocus"),
                        row.Field<double>("StartIris"),
                        row.Field<double>("StartZoom"),
                        row.Field<double>("StartAuxMot")
                    ),
                    new TableRobotMovesPoint(
                        row.Field<double>("MidX"),
                        row.Field<double>("MidY"),
                        row.Field<double>("MidZ"),
                        row.Field<double>("MidRX"),
                        row.Field<double>("MidRY"),
                        row.Field<double>("MidRZ"),
                        row.Field<double>("MidFocus"),
                        row.Field<double>("MidIris"),
                        row.Field<double>("MidZoom"),
                        row.Field<double>("MidAuxMot")
                    ),
                    new TableRobotMovesPoint(
                        row.Field<double>("EndX"),
                        row.Field<double>("EndY"),
                        row.Field<double>("EndZ"),
                        row.Field<double>("EndRX"),
                        row.Field<double>("EndRY"),
                        row.Field<double>("EndRZ"),
                        row.Field<double>("EndFocus"),
                        row.Field<double>("EndIris"),
                        row.Field<double>("EndZoom"),
                        row.Field<double>("EndAuxMot")
                    ),
                    row.Field<int>("ID"),
                    row.Field<string>("Name"),
                    moveType,
                    row.Field<double>("Vel"),
                    row.Field<double>("Acc"),
                    row.Field<double>("Dec"),
                    row.Field<double>("Leave"),
                    row.Field<double>("Reach")
                ); ;

            }
            catch (Exception e)
            {
                return null;
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
                ErrorLog($"Save Robot Moves DataTable Error: {ex.Message}");
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
                    ErrorLog($"Load Robot Moves DataTable Error.");
                    UIPopUp_LoadDatatableError();
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Load Robot Moves DataTable Error: {ex.Message}");
                UIPopUp_LoadDatatableError();
                return false;
            }
        }

        public void Save()
        {
            SaveFile($"robot_moves.xml");
        }

        public void Load()
        {
            LoadFile($"robot_moves.xml");
        }

        public void Close()
        {
            Save();
        }

        private bool DataTable_AddRow(CommandsMoveType moveType, TableRobotMovesPoint startPt, TableRobotMovesPoint midPt, TableRobotMovesPoint endPt, double vel, double acc, double dec, double leave, double reach)
        {
            try
            {
                var name = "Move" + pointCounter.ToString(); // TODO Check repeat
                datatable.Rows.Add(
                    new object[] {
                        pointCounter,               /// ID
                        name,                       /// Name
                        moveType,                   /// Move type

                        Math.Round(startPt.X,4),    /// Start X
                        Math.Round(startPt.Y,4),    /// Start Y
                        Math.Round(startPt.Z,4),    /// Start Z
                        Math.Round(startPt.RX,4),   /// Start RX
                        Math.Round(startPt.RY,4),   /// Start RY
                        Math.Round(startPt.RZ,4),   /// Start RZ
                        Math.Round(startPt.Focus,2),/// Start Focus
                        Math.Round(startPt.Iris,2), /// Start Iris
                        Math.Round(startPt.Zoom,2), /// Start Zoom
                        Math.Round(startPt.Aux,2),  /// Start Ext Motor

                        Math.Round(midPt.X,4),      /// Mid X
                        Math.Round(midPt.Y,4),      /// Mid Y
                        Math.Round(midPt.Z,4),      /// Mid Z
                        Math.Round(midPt.RX,4),     /// Mid RX
                        Math.Round(midPt.RY,4),     /// Mid RY
                        Math.Round(midPt.RZ,4),     /// Mid RZ
                        Math.Round(midPt.Focus,2),  /// Mid Focus
                        Math.Round(midPt.Iris,2),   /// Mid Iris
                        Math.Round(midPt.Zoom,2),   /// Mid Zoom
                        Math.Round(midPt.Aux,2),    /// Mid Ext Motor
                        
                        Math.Round(endPt.X,4),      /// End X
                        Math.Round(endPt.Y,4),      /// End Y
                        Math.Round(endPt.Z,4),      /// End Z
                        Math.Round(endPt.RX,4),     /// End RX
                        Math.Round(endPt.RY,4),     /// End RY
                        Math.Round(endPt.RZ,4),     /// End RZ
                        Math.Round(endPt.Focus,2),  /// End Focus
                        Math.Round(endPt.Iris,2),   /// End Iris
                        Math.Round(endPt.Zoom,2),   /// End Zoom
                        Math.Round(endPt.Aux,2),    /// End Ext Motor
                        
                        Math.Round(vel,2),          /// Velocity
                        Math.Round(acc,2),          /// Acceleration
                        Math.Round(dec,2),          /// Deceleration
                        Math.Round(leave,1),        /// Leave
                        Math.Round(reach,1)         /// Reach
                    }
                );
                pointCounter++;
                IsDatatableChanged = true;
                UIState_TableNotEmpty();
                Log($"Added {moveType} move row '{name}'({pointCounter}).");
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Add {moveType} move row error: {ex.Message}");
                return false;
            }
        }

        private bool DataTable_EditRow(int rowIndex, TableRobotMovesPoint startPt, TableRobotMovesPoint midPt, TableRobotMovesPoint endPt, double vel, double acc, double dec, double leave, double reach)
        {
            try
            {
                datatable.Rows[rowIndex].SetField("StartX", Math.Round(startPt.X, 4));
                datatable.Rows[rowIndex].SetField("StartY", Math.Round(startPt.Y, 4));
                datatable.Rows[rowIndex].SetField("StartZ", Math.Round(startPt.Z, 4));
                datatable.Rows[rowIndex].SetField("StartRX", Math.Round(startPt.RX, 4));
                datatable.Rows[rowIndex].SetField("StartRY", Math.Round(startPt.RY, 4));
                datatable.Rows[rowIndex].SetField("StartRZ", Math.Round(startPt.RZ, 4));
                datatable.Rows[rowIndex].SetField("StartFocus", Math.Round(startPt.Focus, 2));
                datatable.Rows[rowIndex].SetField("StartIris", Math.Round(startPt.Iris, 2));
                datatable.Rows[rowIndex].SetField("StartZoom", Math.Round(startPt.Zoom, 2));
                datatable.Rows[rowIndex].SetField("StartAuxMot", Math.Round(startPt.Aux, 2));
                
                datatable.Rows[rowIndex].SetField("MidX", Math.Round(midPt.X, 4));
                datatable.Rows[rowIndex].SetField("MidY", Math.Round(midPt.Y, 4));
                datatable.Rows[rowIndex].SetField("MidZ", Math.Round(midPt.Z, 4));
                datatable.Rows[rowIndex].SetField("MidRX", Math.Round(midPt.RX, 4));
                datatable.Rows[rowIndex].SetField("MidRY", Math.Round(midPt.RY, 4));
                datatable.Rows[rowIndex].SetField("MidRZ", Math.Round(midPt.RZ, 4));
                datatable.Rows[rowIndex].SetField("MidFocus", Math.Round(midPt.Focus, 2));
                datatable.Rows[rowIndex].SetField("MidIris", Math.Round(midPt.Iris, 2));
                datatable.Rows[rowIndex].SetField("MidZoom", Math.Round(midPt.Zoom, 2));
                datatable.Rows[rowIndex].SetField("MidAuxMot", Math.Round(midPt.Aux, 2));
                
                datatable.Rows[rowIndex].SetField("EndX", Math.Round(endPt.X, 4));
                datatable.Rows[rowIndex].SetField("EndY", Math.Round(endPt.Y, 4));
                datatable.Rows[rowIndex].SetField("EndZ", Math.Round(endPt.Z, 4));
                datatable.Rows[rowIndex].SetField("EndRX", Math.Round(endPt.RX, 4));
                datatable.Rows[rowIndex].SetField("EndRY", Math.Round(endPt.RY, 4));
                datatable.Rows[rowIndex].SetField("EndRZ", Math.Round(endPt.RZ, 4));
                datatable.Rows[rowIndex].SetField("EndFocus", Math.Round(endPt.Focus, 2));
                datatable.Rows[rowIndex].SetField("EndIris", Math.Round(endPt.Iris, 2));
                datatable.Rows[rowIndex].SetField("EndZoom", Math.Round(endPt.Zoom, 2));
                datatable.Rows[rowIndex].SetField("EndAuxMot", Math.Round(endPt.Aux, 2));

                datatable.Rows[rowIndex].SetField("Vel", Math.Round(vel, 2));
                datatable.Rows[rowIndex].SetField("Acc", Math.Round(acc, 2));
                datatable.Rows[rowIndex].SetField("Dec", Math.Round(dec, 2));
                datatable.Rows[rowIndex].SetField("Leave", Math.Round(leave, 1));
                datatable.Rows[rowIndex].SetField("Reach", Math.Round(reach, 1));

                Log($"Edit {datatable.Rows[rowIndex].Field<string>("Move")} move row {rowIndex}: '{datatable.Rows[rowIndex].Field<string>("Name")}'.");
                IsDatatableChanged = true;
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Error editing {datatable.Rows[rowIndex].Field<string>("Move")} move row {rowIndex}: {ex.Message}");
                return false;
            }
        }
        
        private bool DataTable_DeleteSelectedRow()
        {
            try
            {
                if (DataGrid_RobotMoves.SelectedItem != null)
                {
                    DataRowView row = (DataRowView)DataGrid_RobotMoves.SelectedItem;
                    datatable.Rows.Remove(row.Row);
                    IsDatatableChanged = true;
                    return true;
                }
                else
                {
                    ErrorLog($"Delete Selected Row Error: Invalid Selection");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Delete Selected Row Error: {ex.Message}");
                return false;
            }
        }

        private void DataTable_Clear()
        {
            datatable.Clear();
            pointCounter = 1;
            IsDatatableChanged = false;
            UIState_TableEmpty();
        }

        public void Add_LinearMove(TableRobotMovesPoint startPt, TableRobotMovesPoint endPt, double vel, double acc, double dec, double leave, double reach)
        {
            UI_ExitAddEditMode(); // Unlock Table 
            DataTable_AddRow(CommandsMoveType.LINEAR, startPt, new TableRobotMovesPoint(0, 0, 0, 0, 0, 0, 0, 0, 0, 0), endPt, vel, acc, dec, leave, reach);
        }

        public void Edit_LinearMove(int rowIndex, TableRobotMovesPoint startPt, TableRobotMovesPoint endPt, double vel, double acc, double dec, double leave, double reach)
        {
            UI_ExitAddEditMode(); // Unlock Table
            DataTable_EditRow(rowIndex, startPt, new TableRobotMovesPoint(0, 0, 0, 0, 0, 0, 0, 0, 0, 0), endPt, vel, acc, dec, leave, reach);
        }

        public void Add_CircularMove(TableRobotMovesPoint startPt, TableRobotMovesPoint midPt, TableRobotMovesPoint endPt, double vel, double acc, double dec, double leave, double reach)
        {
            UI_ExitAddEditMode(); // Unlock Table
            DataTable_AddRow(CommandsMoveType.CIRCULAR, startPt, midPt, endPt, vel, acc, dec, leave, reach);
        }

        public void Edit_CircularMove(int rowIndex, TableRobotMovesPoint startPt, TableRobotMovesPoint midPt, TableRobotMovesPoint endPt, double vel, double acc, double dec, double leave, double reach)
        {
            UI_ExitAddEditMode(); // Unlock Table
            DataTable_EditRow(rowIndex, startPt, midPt, endPt, vel, acc, dec, leave, reach);
        }

        public double GetFocusValue(int index)
        {
            DataRow row = GetRow(index);
            return row.Field<double>("StartFocus");
        }
        
        public double GetIrisValue(int index)
        {
            DataRow row = GetRow(index);
            return row.Field<double>("StartIris");
        }
        
        public double GetZoomValue(int index)
        {
            DataRow row = GetRow(index);
            return row.Field<double>("StartZoom");
        }
        
        public double GetAuxValue(int index)
        {
            DataRow row = GetRow(index);
            return row.Field<double>("StartAuxMot");
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

        private void Btn_Add_Linear_Click(object sender, RoutedEventArgs e)
        {
            AddLinearMoveEvent?.Invoke(this, new RoutedEventArgs());
        }

        private void Btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEmpty)
            {
                UIState_AddEditMode();
                EditMoveEvent?.Invoke(this, new RoutedEventArgs());
            }
        }

        private void Btn_Add_Circular_Click(object sender, RoutedEventArgs e)
        {
            AddCircularMoveEvent?.Invoke(this, new RoutedEventArgs());
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            DataTable_DeleteSelectedRow();
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

        private void Btn_ClearTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsEmpty)
                {
                    if (UIPopUp_ClearDatatableWarning())
                    {
                        DataTable_Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Clear Table Error: {ex.Message}");
            }
        }

        private bool UIPopUp_ClearDatatableWarning()
        {
            return CustomMessageBox.ShowWarning("Are you sure you want to CLEAR table?", "CLEAR COMMANDS DATATABLE");
        }

        private bool UIPopUp_LoadDatatableWarning()
        {
            return CustomMessageBox.ShowWarning("Are you sure you want to LOAD new data to table?", "LOAD COMMANDS DATATABLE");
        }
        
        private bool UIPopUp_LoadDatatableError()
        {
            return CustomMessageBox.ShowError("Error Loading Datatable from file.", "LOAD COMMANDS DATATABLE ERROR");
        }

        private void UIState_TableEmpty()
        {
            Btn_Clear.IsEnabled = false;
            Btn_Delete.IsEnabled = false;
            Btn_SaveFile.IsEnabled = false;
            Btn_Edit.IsEnabled = false;
        }

        private void UIState_TableNotEmpty()
        {
            Btn_Clear.IsEnabled = true;
            Btn_Delete.IsEnabled = true;
            Btn_SaveFile.IsEnabled = true;
            Btn_Edit.IsEnabled = true;
        }

        private void UIState_AddEditMode()
        {
            DataGrid_RobotMoves.IsEnabled = false;
        }

        private void UIState_AddEditModeReturn()
        {
            DataGrid_RobotMoves.IsEnabled = true;
        }

        public void UI_AddEditMode()
        {
            UIState_AddEditMode(); // Locks Table
        }

        public void UI_ExitAddEditMode()
        {
            UIState_AddEditModeReturn(); // Unlocks Table
        }

        #endregion

    }
}
