/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.Anonymity
{
    // HOWTO: Plugin developer HowTo can be found here: https://github.com/CrypToolProject/CrypTool-2/wiki/Developer-HowTo

    // HOWTO: Change author name, email address, organization and URL.
    [Author("Mikail Sarier", "mikail.sarier@students.uni-mannheim.de", "Universität Mannheim", "")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Anonymity", "Applies Anonymity methods to DataSet", "Anonymity/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Anonymity : ICrypComponent
    {
        #region Private Variables

        public string _csv;
        public string _output;
        private readonly AnonymitySettings _settings = new AnonymitySettings();
        private AnonymityPresentation _presentation = new AnonymityPresentation();
        


        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Input CSV", "Text in CSV-format")]
        public string InputCSV
        {
            get
            {
                return _csv;
            }
            set
            {
                _csv = value;
                OnPropertyChanged("InputCSV");
            }
        }




        [PropertyInfo(Direction.OutputData, "Output Data", "Anonymized DataSet")]
        public  string OutputData
        {
            get
            {
                return _output;
            }
            set
            {
                _output = value;
                OnPropertyChanged("OutputData");

            }
        }






        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>


        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return _presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {


           
        }

        public void Execute()
        {
            if (string.IsNullOrEmpty(_csv))
            {
                GuiLogMessage("Empty CSV. Can not process.", NotificationLevel.Warning);
                return;
            }



            try
            {

               
                string rowSeperator = ProcessEscapeSymbols(_settings.RowSeparator);
                string columnSeperator = ProcessEscapeSymbols(_settings.ColumnSeparator);
                OutputData = _csv;


                if (!_csv.Contains(rowSeperator) || !_csv.Contains(columnSeperator))
                {
                    GuiLogMessage("CSV does not contain the specified row or column separator. Cannot process.", NotificationLevel.Warning);
                    return;
                }

                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    _presentation.ClearPresentation();
                    _presentation.CreateDataTableAndComboboxes(_csv, rowSeperator, columnSeperator);    
                    _presentation.DataTableChanged += Presentation_DataTableChanged;
                

                }, null);


            }
            catch (Exception ex)
            {

                GuiLogMessage(string.Format("Exception occured during processing of CSV: {0}", ex.Message), NotificationLevel.Error);

            }

            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.

            OnPropertyChanged("");

            // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.

            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
        }






        private void Presentation_DataTableChanged(object sender, EventArgs e)
        {
            var dataGrid = _presentation.table;
            StringBuilder csv = new StringBuilder();

            foreach (DataGridColumn column in dataGrid.Columns)
            {
                if (column.Visibility == Visibility.Visible)
                {
                    csv.Append(column.Header);
                    csv.Append(",");
                }
            }

            csv.AppendLine();

            foreach (DataRowView rowView in dataGrid.Items)
            {
                DataRow row = rowView.Row;
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (column.Ordinal < dataGrid.Columns.Count && dataGrid.Columns[column.Ordinal].Visibility == Visibility.Visible)
                    {
                        csv.Append(row[column.ColumnName]);
                        csv.Append(",");
                    }
                }

                csv.AppendLine();
            }

            OutputData = csv.ToString();
        }










        private string ProcessEscapeSymbols(string p)
        {
            return p.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\b", "\b").Replace("\\t", "\t").Replace("\\v", "\v").Replace("\\", "\\");
        }







        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {


            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {


                _presentation.table.Columns.Clear();
                _presentation.ClearPresentation();
             

            }, null);


        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }



   




        #endregion
    }
}
