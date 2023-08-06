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

namespace CrypTool.Plugins.Anonymity
{
    // HOWTO: rename class (click name, press F2)
    public class AnonymitySettings : ISettings
    {
        #region Private Variables

        private string _rowSeparator = "\\n";
        private string _columnSeparator = ",";

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Row Seperator", "This is the Row Seperator", null, 0, false, ControlType.TextBox)]
        public string RowSeparator
        {
            get
            {
                return _rowSeparator;
            }
            set
            {
                if (_rowSeparator != value)
                {
                    _rowSeparator = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("_rowSeparator;");
                }
            }
        }

        [TaskPane("Column Seperator", "This is the Column Seperator", null, 1, false, ControlType.TextBox)]
        public string ColumnSeparator
        {

            get
            {
                return _columnSeparator;
            }
            set
            {
                if (_columnSeparator != value)
                {
                    _columnSeparator = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("ColumnSeparator");
                }
            }
        }


        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {

        }
    }

}