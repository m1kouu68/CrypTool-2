using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CrypTool.Plugins.Anonymity
{
    public class ViewModel : INotifyPropertyChanged
    {

        private string _minGroupSize;
        public string MinGroupSize
        {
            get { return _minGroupSize; }
            set
            {
                _minGroupSize = value;
                OnPropertyChanged("MinGroupSize");
            }


        }


        private string _amountEquiClass;
        public string AmountEquiClass
        {
            get { return _amountEquiClass; }
            set
            {
                _amountEquiClass = value;
                OnPropertyChanged("AmountEquiClass");
            }


        }




        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
