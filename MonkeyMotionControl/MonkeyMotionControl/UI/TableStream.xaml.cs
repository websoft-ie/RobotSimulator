using Microsoft.Win32;
using MonkeyMotionControl.Properties;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MonkeyMotionControl.UI
{
    /// <summary>
    /// Interaction logic for TableStream.xaml
    /// </summary>
    public partial class TableStream : UserControl, INotifyPropertyChanged
    {
        #region EVENTS

        public delegate void LogEventHandler(object sender, LogEventArgs args);
        public event LogEventHandler LogEvent;
        public delegate void AddPointEventHandler(object sender, RoutedEventArgs args);
        public event AddPointEventHandler AddPointEvent;
        public delegate void GotoSelPointEventHandler(object sender, RoutedEventArgs args);
        public event GotoSelPointEventHandler GotoSelPointEvent;
        public delegate void ClearStackEventHandler(object sender, RoutedEventArgs args);
        public event ClearStackEventHandler ClearStackEvent;
        public delegate void RunEventHandler(object sender, RoutedEventArgs args);
        public event RunEventHandler RunEvent;

        #endregion

        #region PROPERTIES

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Console.WriteLine($"State Changed: {propertyName}");
        }

        #endregion

        private DataTable datatable_Sequence;
        private int datatable_CurPointCounter = 1;
        private bool isDatatableInitialized = false;

        public TableStream()
        {
            InitializeComponent();
            datatable_Sequence = new DataTable("Sequence");
            datatable_Sequence.Columns.Add("ID", System.Type.GetType("System.Int32"));
            datatable_Sequence.Columns.Add("Name", System.Type.GetType("System.String"));
            datatable_Sequence.Columns.Add("Distance", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("Focus", System.Type.GetType("System.Int32"));
            datatable_Sequence.Columns.Add("Iris", System.Type.GetType("System.Int32"));
            datatable_Sequence.Columns.Add("Zoom", System.Type.GetType("System.Int32"));
            datatable_Sequence.Columns.Add("Aux", System.Type.GetType("System.Int32"));
            datatable_Sequence.Columns.Add("J1", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("J2", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("J3", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("J4", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("J5", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("J6", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("Vel", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("Acc", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("Dec", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("Leave", System.Type.GetType("System.Double"));
            datatable_Sequence.Columns.Add("Reach", System.Type.GetType("System.Double"));
            for (int i = 0; i < datatable_Sequence.Columns.Count; i++)
            {
                datatable_Sequence.Columns[i].AllowDBNull = false; // Make all columns required.
            }
            DataColumn[] unique_cols =
            {
                datatable_Sequence.Columns["ID"],
                datatable_Sequence.Columns["Name"]
            };
            datatable_Sequence.Constraints.Add(new UniqueConstraint(unique_cols));
            datatable_Sequence.RowChanged += TableRowChanged;
        
            DataGrid_SequenceStream.DataContext = datatable_Sequence.DefaultView;
            DataGrid_SequenceStream.MinColumnWidth = 40;
            isDatatableInitialized = true;
            
        }

        private void Log(string msg)
        {
            LogEvent?.Invoke(this, new LogEventArgs(msg));
        }

        private void TableRowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (isDatatableInitialized)
            {
                //Check each column for correct data input and range
                //Log($"Table row {e.Row["ID"]}-{e.Row["Name"]} changed.");
            }
        }

        public bool AddRow(double[] jointAngles, double focusDist, int focusPos, int irisPos, int zoomPos, int auxPos, double vel, double acc, double dec, double leave, double reach)
        {
            try
            {
                datatable_Sequence.Rows.Add(new object[] {
                    datatable_CurPointCounter,
                    "Point" + datatable_CurPointCounter.ToString(),
                    focusDist,      /// Distance
                    focusPos,       /// Focus
                    irisPos,        /// Iris
                    zoomPos,        /// Zoom
                    auxPos,         /// Aux
                    Math.Round(jointAngles[0],3),   /// J1
                    Math.Round(jointAngles[1],3),   /// J2
                    Math.Round(jointAngles[2],3),   /// J3
                    Math.Round(jointAngles[3],3),   /// J4
                    Math.Round(jointAngles[4],3),   /// J5
                    Math.Round(jointAngles[5],3),   /// J6
                    vel,            /// Velocity
                    acc,            /// Acceleration
                    dec,            /// Deceleration
                    leave,          /// Leave
                    reach           /// Reach
                });
                datatable_CurPointCounter++;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public bool UpdateRow(int nRow, double dist, int focus, int iris, int zoom, int aux, double[] joints, double vel, double acc, double dec, double leave, double reach)
        {
            try
            {
                DataRow row = datatable_Sequence.Rows[nRow];
                nRow++;
                row.ItemArray = new object[] {
                        nRow,
                        "Point " + nRow.ToString(),
                        dist,       /// Distance
                        focus,      /// Focus
                        iris,       /// Iris
                        zoom,       /// Zoom
                        aux,        /// Aux
                        joints[0],  /// J1
                        joints[1],  /// J2
                        joints[2],  /// J3
                        joints[3],  /// J4
                        joints[4],  /// J5
                        joints[5],  /// J6
                        vel,        /// Velocity
                        acc,        /// Acceleration
                        dec,        /// Deceleration
                        leave,      /// Leave
                        reach       /// Reach
                };
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void Clear()
        {
            datatable_Sequence.Clear();
            datatable_CurPointCounter = 0;
        }

        public bool ClearTable()
        {
            if (ClearDatatableWarning())
            {
                Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        public DataRow GetRow(int row)
        {
            return datatable_Sequence.Rows[row];
        }

        public DataRowCollection GetRows()
        {
            return datatable_Sequence.Rows;
        }

        public DataRowView GetSelectedRow()
        {
            return (DataRowView)DataGrid_SequenceStream.SelectedItem;
        }

        public int Count()
        {
            return datatable_Sequence.Rows.Count;
        }

        public bool IsTableEmpty()
        {
            return datatable_Sequence.Rows.Count > 0 ? false : true; 
        }

        //public void EnableMotionFunctions()
        //{
        //    grpbox_ActionButtons.IsEnabled = true;
        //}

        //public void DisableMotionFunctions()
        //{
        //    grpbox_ActionButtons.IsEnabled = false;
        //}

        private void load_DataTable(string filename)
        {
            try
            {
                datatable_Sequence.Clear();
                datatable_Sequence.ReadXml(filename);
                datatable_CurPointCounter = datatable_Sequence.Rows.Count;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void save_DataTable(string filename)
        {
            try
            {
                datatable_Sequence.WriteXml(filename, XmlWriteMode.WriteSchema);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //public void SetProgressBar(double total)
        //{
        //    progBar_RunProgress.Maximum = total;
        //    tb_RunItemsTotal.Text = total.ToString();
        //    progBar_RunProgress.Value = 0;
        //    tb_RunProgress.Text = "0.00";
        //}

        //public void UpdateProgressBar(double prog)
        //{
        //    progBar_RunProgress.Value = prog;
        //    tb_RunProgress.Text = Math.Round(prog,2).ToString();
        //}

        //public void UpdateTotalTime(TimeSpan totaltime)
        //{
        //    tb_totalTime.Text = totaltime.ToString(@"hh\:mm\:ss\.fff");
        //}

        private void Btn_AddPoint_Click(object sender, RoutedEventArgs e)
        {
            AddPointEvent?.Invoke(this, new RoutedEventArgs());
        }

        private void Btn_DeletePoint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGrid_SequenceStream.SelectedItem != null)
                {
                    DataRowView row = (DataRowView)DataGrid_SequenceStream.SelectedItem;
                    datatable_Sequence.Rows.Remove(row.Row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Loadto3dSim_Click(object sender, RoutedEventArgs e)
        {
            if (datatable_Sequence.Rows.Count > 0)
            {
                //Simulator.ViewPort3D_ClearSeqPoints();
                //foreach (DataRow row in datatable_Sequence.Rows)
                //{
                //    AddLog($"Sequence add 3D Point: ID({row["ID"]}) {row["Name"]}( {row["X"]}, {row["Y"]}, {row["Z"]}, {row["R-X"]}, {row["R-Y"]}, {row["R-Z"]} )");
                //    Simulator.ViewPort3D_AddSeqPoint(Convert.ToInt16(row["ID"]), new double[] { Convert.ToDouble(row["X"]), Convert.ToDouble(row["Y"]), Convert.ToDouble(row["Z"]), Convert.ToDouble(row["R-X"]), Convert.ToDouble(row["R-Y"]), Convert.ToDouble(row["R-Z"]) },
                //                        Convert.ToString(row["Name"]));
                //}
            }
        }

        private void Btn_Clear3dSim_Click(object sender, RoutedEventArgs e)
        {
           //Simulator.ViewPort3D_ClearSeqPoints();
        }

        private void Btn_SaveFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                string time = DateTime.Now.ToString("hhmmss"); // includes leading zeros
                string date = DateTime.Now.ToString("yyMMdd"); // includes leading zeros
                saveFileDialog.FileName = $"stream_sequence_{date}_{time}.xml";
                if (saveFileDialog.ShowDialog() == true)
                {
                    save_DataTable(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoadDatatableWarning())
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        load_DataTable(openFileDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_ClearTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_GotoSelPoint_Click(object sender, RoutedEventArgs e)
        {
            GotoSelPointEvent?.Invoke(this, new RoutedEventArgs());
        }

        private void Btn_ClearStack_Click(object sender, RoutedEventArgs e)
        {
            ClearStackEvent?.Invoke(this, new RoutedEventArgs());
        }

        private void Btn_SeqRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGrid_SequenceStream.Items.Count > 0)
                {
                    RunEvent?.Invoke(this, new RoutedEventArgs());
                }
                else
                {
                    CustomMessageBox.ShowError("Stream Mode Table Empty.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool ClearDatatableWarning()
        {
            return CustomMessageBox.ShowWarning("Are you sure you want to CLEAR table?", "CLEAR DATATABLE");
        }

        private bool LoadDatatableWarning()
        {
            return CustomMessageBox.ShowWarning("Are you sure you want to LOAD new data to table?", "LOAD DATATABLE");
        }

    }
}
