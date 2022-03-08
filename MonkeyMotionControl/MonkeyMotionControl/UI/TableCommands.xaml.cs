using Microsoft.Win32;
using MonkeyMotionControl.Properties;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MonkeyMotionControl.UI
{
    public enum CommandsMoveType
    {
        START = 0,
        LINEAR = 1,
        CIRCULAR = 2
    };


    public class CommandsTablePoint
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
        

        public CommandsTablePoint()
        {
        }
        
        public CommandsTablePoint(double x, double y, double z, double rx, double ry, double rz, double focus, double iris, double zoom, double aux)
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

    public class CommandsTableRow
    {
        public int Id = 0;
        public string Name = "";
        public CommandsTablePoint TargetPoint;
        public CommandsTablePoint MidPoint;
        public CommandsMoveType MoveType;
        public double Vel = 0;
        public double Acc = 0;
        public double Dec = 0;
        public double Leave = 0;
        public double Reach = 0;
        public CommandsTableRow (CommandsTablePoint target, CommandsTablePoint mid, int id, string name, CommandsMoveType movetype, double vel, double acc, double dec, double leave, double reach)
        {
            TargetPoint = target;
            MidPoint = mid;
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
    public partial class TableCommands : UserControl, INotifyPropertyChanged
    {

        #region EVENTS

        public delegate void LogEventHandler(object sender, LogEventArgs args);
        public event LogEventHandler LogEvent;
        public delegate void ErrorLogEventHandler(object sender, LogEventArgs args);
        public event ErrorLogEventHandler ErrorLogEvent;
        public delegate void SetStartPointEventHandler(object sender, RoutedEventArgs args);
        public event SetStartPointEventHandler SetStartPointEvent;
        public delegate void AddLinearMoveEventHandler(object sender, RoutedEventArgs args);
        public event AddLinearMoveEventHandler AddLinearMoveEvent;
        public delegate void EditMoveEventHandler(object sender, RoutedEventArgs args);
        public event EditMoveEventHandler EditMoveEvent;
        public delegate void AddCircularMoveEventHandler(object sender, RoutedEventArgs args);
        public event AddCircularMoveEventHandler AddCircularMoveEvent;
        public delegate void GotoSelPointEventHandler(object sender, RoutedEventArgs args);
        public event GotoSelPointEventHandler GotoSelPointEvent;
        public delegate void TableLoadedEventHandler(object sender, RoutedEventArgs args);
        public event TableLoadedEventHandler TableLoadedEvent;
        public delegate void ClearEventHandler(object sender, RoutedEventArgs args);
        public event ClearEventHandler ClearEvent;
        public delegate void DeleteLastEventHandler(object sender, RoutedEventArgs args);
        public event DeleteLastEventHandler DeleteLastEvent;
        public delegate void NavCurrentRowChangedEventHandler(object sender, RoutedEventArgs args);
        public event NavCurrentRowChangedEventHandler NavCurrentRowChangedEvent;
        public delegate void NavNextEventHandler(object sender, RoutedEventArgs args);
        public event NavNextEventHandler NavNextEvent;
        public delegate void NavPrevEventHandler(object sender, RoutedEventArgs args);
        public event NavPrevEventHandler NavPrevEvent;
        public delegate void NavStartEventHandler(object sender, RoutedEventArgs args);
        public event NavStartEventHandler NavStartEvent;
        public delegate void NavEndEventHandler(object sender, RoutedEventArgs args);
        public event NavEndEventHandler NavEndEvent;

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

        private int NavCurrentRow // TODO Convert to Full DP
        {
            get { return _navCurrentRow;  }
            set 
            { 
                if(value >= 0 && value < Count)
                {
                    _navCurrentRow = value;
                    DataGrid_SequenceCommands.SelectedIndex = _navCurrentRow;
                    Tb_NavCurPoint.Text = $"{_navCurrentRow+1}";
                    NavCurrentRowChangedEvent?.Invoke(this, new RoutedEventArgs());
                }
            }
        } int _navCurrentRow = 0;

        private DataTable datatable { get; set; }
        private bool isDatatableInitialized { get; set; } = false;
        private int pointCounter { get; set; } = 1;
        public bool IsStartPointSet { get; private set; } = false;

        #endregion

        public TableCommands()
        {
            InitializeComponent();

            // Initialize DataTable
            datatable = new DataTable("CommandsSequence");
            datatable.Columns.Add("ID", System.Type.GetType("System.Int32"));
            datatable.Columns.Add("Name", System.Type.GetType("System.String"));
            datatable.Columns.Add("Move", System.Type.GetType("System.String"));
            datatable.Columns.Add("PX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("PY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("PZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("PRX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("PRY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("PRZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidRX", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidRY", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidRZ", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Vel", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Acc", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Dec", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Leave", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Reach", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Focus", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Iris", System.Type.GetType("System.Double"));
            datatable.Columns.Add("Zoom", System.Type.GetType("System.Double"));
            datatable.Columns.Add("AuxMot", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidFocus", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidIris", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidZoom", System.Type.GetType("System.Double"));
            datatable.Columns.Add("MidAuxMot", System.Type.GetType("System.Double"));
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
            DataGrid_SequenceCommands.DataContext = datatable.DefaultView;
            DataGrid_SequenceCommands.MinColumnWidth = 40;
            pointCounter = 1;
            NavCurrentRow = 0;
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
        
        public int GetCurrentRowIndex()
        {
            return NavCurrentRow;
        }

        private DataRow DataTable_GetSelected_DataRow()
        {
            try
            {
                var index = DataGrid_SequenceCommands.SelectedIndex;
                return datatable.Rows[index];
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private DataRowView DataTable_GetSelected_DataRowView()
        {
            return (DataRowView)DataGrid_SequenceCommands.SelectedItem;
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

        private bool DataTable_Load(string filename)
        {
            try
            {
                datatable.Clear();
                datatable.ReadXml(filename);
                if (!IsEmpty)
                {
                    pointCounter = Count + 1;
                    IsStartPointSet = true;
                    NavCurrentRow = 0;
                    TableLoadedEvent?.Invoke(this, new RoutedEventArgs());
                    UIState_TableNotEmpty();
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

        private void DataTable_SaveFile(string filename)
        {
            try
            {
                datatable.WriteXml(filename, XmlWriteMode.WriteSchema);
            }
            catch (Exception ex)
            {
                ErrorLog($"Sve DataTable Error: {ex.Message}");
            }
        }

        private bool DataTable_AddRow_StartPoint(CommandsTablePoint startPoint)
        {
            try
            {
                var name = "Point " + pointCounter.ToString();
                datatable.Rows.Add(
                    new object[] {
                        pointCounter,            /// ID
                        name,                           /// Name
                        CommandsMoveType.START,         /// Move type
                        Math.Round(startPoint.X,4),     /// P X
                        Math.Round(startPoint.Y,4),     /// P Y
                        Math.Round(startPoint.Z,4),     /// P Z
                        Math.Round(startPoint.RX,4),    /// P RX
                        Math.Round(startPoint.RY,4),    /// P RY
                        Math.Round(startPoint.RZ,4),    /// P RZ
                        -1,                             /// Mid X
                        -1,                             /// Mid Y
                        -1,                             /// Mid Z
                        -1,                             /// Mid RX
                        -1,                             /// Mid RY
                        -1,                             /// Mid RZ
                        -1,                             /// Velocity
                        -1,                             /// Acceleration
                        -1,                             /// Deceleration
                        -1,                             /// Leave
                        -1,                             /// Reach
                        Math.Round(startPoint.Focus,2), /// Focus
                        Math.Round(startPoint.Iris,2),  /// Iris
                        Math.Round(startPoint.Zoom,2),  /// Zoom
                        Math.Round(startPoint.Aux,2),   /// Ext Motor
                        -1,                             /// Mid Focus
                        -1,                             /// Mid Iris
                        -1,                             /// Mid Zoom
                        -1                              /// Mid Ext Motor
                    }
                );
                pointCounter++;
                IsStartPointSet = true;
                NavCurrentRow = 0;
                Log($"Added START move row '{name}'.");
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Start Point Add Row Error: {ex.Message}");
                return false;
            }
        }
        
        private bool DataTable_AddRow_LinearMove(CommandsTablePoint pointB, double vel, double acc, double dec, double leave, double reach)
        {
            try
            {
                var name = "Point " + pointCounter.ToString();
                datatable.Rows.Add(
                    new object[] {
                        pointCounter,        /// ID
                        name,                       /// Name
                        CommandsMoveType.LINEAR,    /// Move type
                        Math.Round(pointB.X,4),     /// P X
                        Math.Round(pointB.Y,4),     /// P Y
                        Math.Round(pointB.Z,4),     /// P Z
                        Math.Round(pointB.RX,4),    /// P RX
                        Math.Round(pointB.RY,4),    /// P RY
                        Math.Round(pointB.RZ,4),    /// P RZ
                        -1,                         /// Mid X
                        -1,                         /// Mid Y
                        -1,                         /// Mid Z
                        -1,                         /// Mid RX
                        -1,                         /// Mid RY
                        -1,                         /// Mid RZ
                        Math.Round(vel,2),          /// Velocity
                        Math.Round(acc,2),          /// Acceleration
                        Math.Round(dec,2),          /// Deceleration
                        Math.Round(leave,1),        /// Leave
                        Math.Round(reach,1),        /// Reach
                        Math.Round(pointB.Focus,2), /// Focus
                        Math.Round(pointB.Iris,2),  /// Iris
                        Math.Round(pointB.Zoom,2),  /// Zoom
                        Math.Round(pointB.Aux,2),   /// Ext Motor
                        -1,                         /// Mid Focus
                        -1,                         /// Mid Iris
                        -1,                         /// Mid Zoom
                        -1                          /// Mid Ext Motor
                    }
                );
                pointCounter++;
                NavCurrentRow++;
                UIState_NavControlsEnable();
                Log($"Added LINEAR move row '{name}'.");
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Linear Move Add Row Error: {ex.Message}");
                return false;
            }
        }

        private bool DataTable_EditRow_StartPoint(CommandsTablePoint startpt)
        {
            try
            {
                datatable.Rows[0].SetField("PX", Math.Round(startpt.X, 4));
                datatable.Rows[0].SetField("PY", Math.Round(startpt.Y, 4));
                datatable.Rows[0].SetField("PZ", Math.Round(startpt.Z, 4));
                datatable.Rows[0].SetField("PRX", Math.Round(startpt.RX, 4));
                datatable.Rows[0].SetField("PRY", Math.Round(startpt.RY, 4));
                datatable.Rows[0].SetField("PRZ", Math.Round(startpt.RZ, 4));
                datatable.Rows[0].SetField("MidX", -1);
                datatable.Rows[0].SetField("MidY", -1);
                datatable.Rows[0].SetField("MidZ", -1);
                datatable.Rows[0].SetField("MidRX", -1);
                datatable.Rows[0].SetField("MidRY", -1);
                datatable.Rows[0].SetField("MidRZ", -1);
                datatable.Rows[0].SetField("Vel", -1);
                datatable.Rows[0].SetField("Acc", -1);
                datatable.Rows[0].SetField("Dec", -1);
                datatable.Rows[0].SetField("Leave", -1);
                datatable.Rows[0].SetField("Reach", -1);
                datatable.Rows[0].SetField("Focus", Math.Round(startpt.Focus, 2));
                datatable.Rows[0].SetField("Iris", Math.Round(startpt.Iris, 2));
                datatable.Rows[0].SetField("Zoom", Math.Round(startpt.Zoom, 2));
                datatable.Rows[0].SetField("AuxMot", Math.Round(startpt.Aux, 2));
                datatable.Rows[0].SetField("MidFocus", -1);
                datatable.Rows[0].SetField("MidIris", -1);
                datatable.Rows[0].SetField("MidZoom", -1);
                datatable.Rows[0].SetField("MidAuxMot", -1);
                Log($"Edit Row Start Point.");
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Edit Start Point Error: {ex.Message}");
                return false;
            }
        }

        private bool DataTable_EditRow_LinearMove(int rowIndex, CommandsTablePoint endpt, double vel, double acc, double dec, double leave, double reach)
        {
            try
            {
                datatable.Rows[rowIndex].SetField("PX", Math.Round(endpt.X, 4));    
                datatable.Rows[rowIndex].SetField("PY", Math.Round(endpt.Y, 4));  
                datatable.Rows[rowIndex].SetField("PZ", Math.Round(endpt.Z, 4));  
                datatable.Rows[rowIndex].SetField("PRX", Math.Round(endpt.RX, 4));  
                datatable.Rows[rowIndex].SetField("PRY", Math.Round(endpt.RY, 4));   
                datatable.Rows[rowIndex].SetField("PRZ", Math.Round(endpt.RZ, 4));  
                datatable.Rows[rowIndex].SetField("MidX", -1);
                datatable.Rows[rowIndex].SetField("MidY", -1);
                datatable.Rows[rowIndex].SetField("MidZ", -1);
                datatable.Rows[rowIndex].SetField("MidRX", -1);
                datatable.Rows[rowIndex].SetField("MidRY", -1);
                datatable.Rows[rowIndex].SetField("MidRZ", -1);
                datatable.Rows[rowIndex].SetField("Vel", Math.Round(vel, 2));
                datatable.Rows[rowIndex].SetField("Acc", Math.Round(acc, 2));
                datatable.Rows[rowIndex].SetField("Dec", Math.Round(dec, 2));
                datatable.Rows[rowIndex].SetField("Leave", Math.Round(leave, 1));
                datatable.Rows[rowIndex].SetField("Reach", Math.Round(reach, 1));
                datatable.Rows[rowIndex].SetField("Focus", Math.Round(endpt.Focus, 2));
                datatable.Rows[rowIndex].SetField("Iris", Math.Round(endpt.Iris, 2));
                datatable.Rows[rowIndex].SetField("Zoom", Math.Round(endpt.Zoom, 2));
                datatable.Rows[rowIndex].SetField("AuxMot", Math.Round(endpt.Aux, 2));
                datatable.Rows[rowIndex].SetField("MidFocus", -1);
                datatable.Rows[rowIndex].SetField("MidIris", -1);
                datatable.Rows[rowIndex].SetField("MidZoom", -1);
                datatable.Rows[rowIndex].SetField("MidAuxMot", -1);
                Log($"Linear Move Edit Row {rowIndex}: '{datatable.Rows[rowIndex].Field<string>("Name")}'.");
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Linear Move Edit Row {rowIndex} Error: {ex.Message}");
                return false;
            }
        }
        
        private bool DataTable_AddRow_CircularMove(CommandsTablePoint midpt, CommandsTablePoint endpt, double vel, double acc, double dec, double leave, double reach)
        {
            try
            {
                var name = "Point " + pointCounter.ToString();
                datatable.Rows.Add(
                    new object[] {
                        pointCounter,        /// ID
                        name,                       /// Name
                        CommandsMoveType.CIRCULAR,  /// Move type
                        Math.Round(endpt.X,4),      /// P X
                        Math.Round(endpt.Y,4),      /// P Y
                        Math.Round(endpt.Z,4),      /// P Z
                        Math.Round(endpt.RX,4),     /// P RX
                        Math.Round(endpt.RY,4),     /// P RY
                        Math.Round(endpt.RZ,4),     /// P RZ
                        Math.Round(midpt.X,4),      /// Mid X
                        Math.Round(midpt.Y,4),      /// Mid Y
                        Math.Round(midpt.Z,4),      /// Mid Z
                        Math.Round(midpt.RX,4),     /// Mid RX
                        Math.Round(midpt.RY,4),     /// Mid RY
                        Math.Round(midpt.RZ,4),     /// Mid RZ
                        Math.Round(vel,2),          /// Velocity
                        Math.Round(acc,2),          /// Acceleration
                        Math.Round(dec,2),          /// Deceleration
                        Math.Round(leave,1),        /// Leave
                        Math.Round(reach,1),        /// Reach
                        Math.Round(endpt.Focus,2),  /// Focus
                        Math.Round(endpt.Iris,2),   /// Iris
                        Math.Round(endpt.Zoom,2),   /// Zoom
                        Math.Round(endpt.Aux,2),    /// Ext Motor
                        Math.Round(midpt.Focus,2),  /// Mid Focus
                        Math.Round(midpt.Iris,2),   /// Mid Iris
                        Math.Round(midpt.Zoom,2),   /// Mid Zoom
                        Math.Round(midpt.Aux,2)     /// Mid Ext Motor
                    }
                );
                pointCounter++;
                NavCurrentRow++;
                UIState_NavControlsEnable();
                Log($"Added CIRCULAR move row '{name}'.");
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Circular Move Add Row Error: {ex.Message}");
                return false;
            }
        }

        private bool DataTable_EditRow_CircularMove(int rowIndex, CommandsTablePoint midpt, CommandsTablePoint endpt, double vel, double acc, double dec, double leave, double reach)
        {
            try
            {
                datatable.Rows[rowIndex].SetField("PX", Math.Round(endpt.X, 4));
                datatable.Rows[rowIndex].SetField("PY", Math.Round(endpt.Y, 4));
                datatable.Rows[rowIndex].SetField("PZ", Math.Round(endpt.Z, 4));
                datatable.Rows[rowIndex].SetField("PRX", Math.Round(endpt.RX, 4));
                datatable.Rows[rowIndex].SetField("PRY", Math.Round(endpt.RY, 4));
                datatable.Rows[rowIndex].SetField("PRZ", Math.Round(endpt.RZ, 4));
                datatable.Rows[rowIndex].SetField("MidX", Math.Round(midpt.X, 4));
                datatable.Rows[rowIndex].SetField("MidY", Math.Round(midpt.Y, 4));
                datatable.Rows[rowIndex].SetField("MidZ", Math.Round(midpt.Z, 4));
                datatable.Rows[rowIndex].SetField("MidRX", Math.Round(midpt.RX, 4));
                datatable.Rows[rowIndex].SetField("MidRY", Math.Round(midpt.RY, 4));
                datatable.Rows[rowIndex].SetField("MidRZ", Math.Round(midpt.RZ, 4));
                datatable.Rows[rowIndex].SetField("Vel", Math.Round(vel, 2));
                datatable.Rows[rowIndex].SetField("Acc", Math.Round(acc, 2));
                datatable.Rows[rowIndex].SetField("Dec", Math.Round(dec, 2));
                datatable.Rows[rowIndex].SetField("Leave", Math.Round(leave, 1));
                datatable.Rows[rowIndex].SetField("Reach", Math.Round(reach, 1));
                datatable.Rows[rowIndex].SetField("Focus", endpt.Focus);
                datatable.Rows[rowIndex].SetField("Iris", endpt.Iris);
                datatable.Rows[rowIndex].SetField("Zoom", endpt.Zoom);
                datatable.Rows[rowIndex].SetField("AuxMot", endpt.Aux);
                datatable.Rows[rowIndex].SetField("MidFocus", midpt.Focus);
                datatable.Rows[rowIndex].SetField("MidIris", midpt.Iris);
                datatable.Rows[rowIndex].SetField("MidZoom", midpt.Zoom);
                datatable.Rows[rowIndex].SetField("MidAuxMot", midpt.Aux);
                Log($"Circular Move Edit Row {rowIndex}: '{datatable.Rows[rowIndex].Field<string>("Name")}'.");
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog($"Circular Move Edit Row {rowIndex} Error: {ex.Message}");
                return false;
            }
        }

        private bool DataTable_DeleteLastRow()
        {
            try
            {
                if (!IsEmpty)
                {
                    datatable.Rows.Remove(GetLastRow());
                    NavCurrentRow--;
                    if (IsEmpty)
                    {
                        IsStartPointSet = false;
                        UIState_TableEmpty();
                    }
                    UIState_NavControlsDisable();
                    DeleteLastEvent?.Invoke(this, new RoutedEventArgs());
                    return true;
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

        private bool DataTable_DeleteSelectedRow()
        {
            try
            {
                if (DataGrid_SequenceCommands.SelectedItem != null)
                {
                    DataRowView row = (DataRowView)DataGrid_SequenceCommands.SelectedItem;
                    datatable.Rows.Remove(row.Row);
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
            IsStartPointSet = false;
            UIState_TableEmpty();
            ClearEvent?.Invoke(this, new RoutedEventArgs());
        }

        public DataTable CopyDataTable()
        {
            return datatable.Copy();
        }

        public DataTable CloneDataTable()
        {
            return datatable.Clone();
        }

        public int GetSelectedRowIndex()
        {
            return DataGrid_SequenceCommands.SelectedIndex;
        }

        public CommandsTableRow GetRowData(int rowNumber)
        {
            try
            {
                DataRow row = datatable.Rows[rowNumber];
                if (row == null) return null;

                CommandsMoveType moveType = CommandsMoveType.START;

                if (string.Compare((string)row["Move"], "START") == 0)
                    moveType = CommandsMoveType.START;
                else if (string.Compare((string)row["Move"], "LINEAR") == 0)
                    moveType = CommandsMoveType.LINEAR;
                else if (string.Compare((string)row["Move"], "CIRCULAR") == 0)
                    moveType = CommandsMoveType.CIRCULAR;
                //Console.WriteLine($"RowID: {row.Field<int>("ID")}");
                return new CommandsTableRow(
                    new CommandsTablePoint(
                        row.Field<double>("PX"),
                        row.Field<double>("PY"),
                        row.Field<double>("PZ"),
                        row.Field<double>("PRX"),
                        row.Field<double>("PRY"),
                        row.Field<double>("PRZ"),
                        row.Field<double>("Focus"),
                        row.Field<double>("Iris"),
                        row.Field<double>("Zoom"),
                        row.Field<double>("AuxMot")
                    ),
                    new CommandsTablePoint(
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

        public void SetStartPoint(CommandsTablePoint startPoint)
        {
            bool emptyFlag;
            if (IsEmpty)
            {
                emptyFlag = true;
            }
            else
            {
                if (!UIPopUp_ClearDatatableWarning())
                {
                    return;
                }
                else
                {
                    DataTable_Clear();
                    emptyFlag = true;
                }
            }
            if (emptyFlag)
            {
                if (DataTable_AddRow_StartPoint(startPoint))
                {
                    UIState_TableNotEmpty();
                    Log($"Added start point({startPoint.X}, {startPoint.Y}, {startPoint.Z}, {startPoint.RX}, {startPoint.RY}, {startPoint.RZ}) | focus({startPoint.Focus}) iris({startPoint.Iris}) zoom({startPoint.Zoom}) AuxMot({startPoint.Aux})");
                }
                else
                    Log($"Error adding start point.");
            }
            else
            {
                Log($"Error adding start point.");
            }
        }

        public void AddLinearMove(CommandsTablePoint destPoint, double vel, double acc, double dec, double leave, double reach)
        {
            UI_ExitAddEditMode(); // Unlock Table
            DataTable_AddRow_LinearMove(destPoint, vel, acc, dec, leave, reach);
        }

        public void EditLinearMove(int rowIndex, CommandsTablePoint destPoint, double vel, double acc, double dec, double leave, double reach)
        {
            UI_ExitAddEditMode(); // Unlock Table
            DataTable_EditRow_LinearMove(rowIndex, destPoint, vel, acc, dec, leave, reach);
        }

        public void AddCircularMove(CommandsTablePoint midPoint, CommandsTablePoint destPoint, double vel, double acc, double dec, double leave, double reach)
        {
            UI_ExitAddEditMode(); // Unlock Table
            DataTable_AddRow_CircularMove(midPoint, destPoint, vel, acc, dec, leave, reach);
        }

        public void EditCircularMove(int rowIndex, CommandsTablePoint midPoint, CommandsTablePoint destPoint, double vel, double acc, double dec, double leave, double reach)
        {
            UI_ExitAddEditMode(); // Unlock Table
            DataTable_EditRow_CircularMove(rowIndex, midPoint, destPoint, vel, acc, dec, leave, reach);
        }
        
        public void EditStart(CommandsTablePoint startPoint)
        {
            UI_ExitAddEditMode(); // Unlock Table
            DataTable_EditRow_StartPoint(startPoint);
        }

        public bool ClearTable()
        {
            if (UIPopUp_ClearDatatableWarning())
            {
                DataTable_Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        public CommandsTablePoint GetLastMovePoint()
        {
            try
            {
                if (!IsEmpty)
                {
                    DataRow row = GetRow(Count - 1);
                    return new CommandsTablePoint(
                        row.Field<double>("PX"),
                        row.Field<double>("PY"),
                        row.Field<double>("PZ"),
                        row.Field<double>("PRX"),
                        row.Field<double>("PRY"),
                        row.Field<double>("PRZ"),
                        row.Field<double>("Focus"),
                        row.Field<double>("Iris"),
                        row.Field<double>("Zoom"),
                        row.Field<double>("AuxMot")
                    );
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Get Last Move Point Error: {ex.Message}");
                return null;
            }
        }

        public CommandsTablePoint GetPreviousMovePoint(int curRow)
        {
            try
            {
                //Console.WriteLine($"curRow={curRow}");
                if (curRow > 0)
                {
                    DataRow row = GetRow(curRow - 1);
                    return new CommandsTablePoint(
                        row.Field<double>("PX"),
                        row.Field<double>("PY"),
                        row.Field<double>("PZ"),
                        row.Field<double>("PRX"),
                        row.Field<double>("PRY"),
                        row.Field<double>("PRZ"),
                        row.Field<double>("Focus"),
                        row.Field<double>("Iris"),
                        row.Field<double>("Zoom"),
                        row.Field<double>("AuxMot")
                    );
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Get Previous Move Point Error: {ex.Message}");
                return null;
            }
        }

        public double GetFocusValue(int index)
        {
            DataRow row = GetRow(index);
            return row.Field<double>("Focus");
        }
        
        public double GetIrisValue(int index)
        {
            DataRow row = GetRow(index);
            return row.Field<double>("Iris");
        }
        
        public double GetZoomValue(int index)
        {
            DataRow row = GetRow(index);
            return row.Field<double>("Zoom");
        }
        
        public double GetAuxValue(int index)
        {
            DataRow row = GetRow(index);
            return row.Field<double>("AuxMot");
        }

        public int NavStart()
        {
            try
            {
                NavCurrentRow = 0;
                return NavCurrentRow;
            }
            catch (Exception e)
            {
                ErrorLog($"Commands Table Navigate Start Point Error: {e}");
                return -2;
            }
        }

        public int NavPrev()
        {
            try
            {
                if ((NavCurrentRow - 1) >= 0)
                {
                    NavCurrentRow--;
                    return NavCurrentRow;
                }
                else
                    return -1;
            }
            catch (Exception e)
            {
                ErrorLog($"Commands Table Navigate Previous Point Error: {e}");
                return -2;
            }
        }
        
        public int NavNext()
        {
            try
            {
                if ((NavCurrentRow + 1) < Count)
                {
                    NavCurrentRow++;
                    return NavCurrentRow; 
                }
                else
                    return -1;
            }
            catch (Exception e)
            {
                ErrorLog($"Commands Table Navigate Next Point Error: {e}");
                return -2;
            }
        }
        
        public int NavEnd()
        {
            try
            {
                NavCurrentRow = Count - 1;
                return NavCurrentRow;
            }
            catch (Exception e)
            {
                ErrorLog($"Commands Table Navigate End/Last Point Error: {e}");
                return -2;
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

        private void Btn_Set_Start_Click(object sender, RoutedEventArgs e)
        {
            SetStartPointEvent?.Invoke(this, new RoutedEventArgs());
        }

        private void Btn_Add_Linear_Click(object sender, RoutedEventArgs e)
        {
            if (IsStartPointSet)
            {
                AddLinearMoveEvent?.Invoke(this, new RoutedEventArgs());
            }
            else
            {
                UIPopUp_SetStartPointWarning();
                return;
            }
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
            if (IsStartPointSet)
            {
                AddCircularMoveEvent?.Invoke(this, new RoutedEventArgs());
            }
            else
            {
                UIPopUp_SetStartPointWarning();
                return;
            }
        }

        private void Btn_DeleteLast_Click(object sender, RoutedEventArgs e)
        {
            DataTable_DeleteLastRow();
        }

        private void Btn_SaveFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsEmpty && Count > 1)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    string time = DateTime.Now.ToString("hhmmss"); // includes leading zeros
                    string date = DateTime.Now.ToString("yyMMdd"); // includes leading zeros
                    saveFileDialog.FileName = $"commands_sequence_{date}_{time}.xml";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        DataTable_SaveFile(saveFileDialog.FileName);
                    }
                }
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
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        DataTable_Load(openFileDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog($"Open File Error: {ex.Message}");
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

        private void Btn_GotoSelPoint_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEmpty)
            { 
                GotoSelPointEvent?.Invoke(this, new RoutedEventArgs());
            }
        }

        private void Btn_NavStart_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEmpty)
            {
                NavStartEvent?.Invoke(this, new RoutedEventArgs());
            }
        }
        
        private void Btn_NavPrev_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEmpty)
            {
                NavPrevEvent?.Invoke(this, new RoutedEventArgs());
            }
        }
        
        private void Btn_NavNext_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEmpty)
            {
                NavNextEvent?.Invoke(this, new RoutedEventArgs());
            }
        }
        
        private void Btn_NavEnd_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEmpty)
            {
                NavEndEvent?.Invoke(this, new RoutedEventArgs());
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

        private void UIPopUp_SetStartPointWarning()
        {
            CustomMessageBox.ShowInfo("Please set Start Point before adding moves.", "START POINT NOT SET");
        }

        private void UIState_TableEmpty()
        {
            Btn_Clear.IsEnabled = false;
            Btn_DeleteLast.IsEnabled = false;
            Btn_SaveFile.IsEnabled = false;
            Btn_Edit.IsEnabled = false;
            Btn_GotoSelPoint.IsEnabled = false;
            Btn_SetStart.IsEnabled = true;
            UIState_NavControlsDisable();
        }

        private void UIState_TableNotEmpty()
        {
            Btn_Clear.IsEnabled = true;
            Btn_DeleteLast.IsEnabled = true;
            Btn_SaveFile.IsEnabled = true;
            Btn_Edit.IsEnabled = true;
            Btn_GotoSelPoint.IsEnabled = true;
            Btn_SetStart.IsEnabled = false;
            if (Count > 1)
            {
                stackpanel_function_navigation.IsEnabled = true;
            }
        }

        private void UIState_NavControlsEnable()
        {
            if (Count > 1)
            {
                stackpanel_function_navigation.IsEnabled = true;
            }
        }

        private void UIState_NavControlsDisable()
        {
            if (Count < 2)
            {
                stackpanel_function_navigation.IsEnabled = false;
            }
        }

        private void UIState_AddEditMode()
        {
            stackpanel_function_buttons.IsEnabled = false;
            DataGrid_SequenceCommands.IsEnabled = false;
        }

        private void UIState_AddEditModeReturn()
        {
            stackpanel_function_buttons.IsEnabled = true;
            DataGrid_SequenceCommands.IsEnabled = true;
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
