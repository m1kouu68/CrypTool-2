using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


        private string _distinctLValue;
        public string DistinctLValue
        {
            get { return _distinctLValue; }
            set
            {
                _distinctLValue = value;
                OnPropertyChanged("DistinctLValue");
            }
        }



        private string _entropyLValue;
        public string EntropyLValue
        {

            get { return _entropyLValue; }
            set
            {
                _entropyLValue = value;
                OnPropertyChanged("EntropyLValue");
            }


        }


        private string _recursiveLValue;
        public string RecursiveLValue
        {
            get { return _recursiveLValue; }
            set
            {
                _recursiveLValue = value;
                OnPropertyChanged("RecursiveLValue");
            }
        }


        private string _simpleAlphaK;
        public string SimpleAlphaK
        {

            get { return _simpleAlphaK; }
            set
            {
                _simpleAlphaK = value;
                OnPropertyChanged("SimpleAlphaK");
            }

        }




        private string _generalAlphaK;
        public string GeneralAlphaK
        {
            get { return _generalAlphaK; }
            set
            {
                _generalAlphaK = value;
                OnPropertyChanged("GeneralAlphaK");
            }
        }



        private string _distinctSensitive;
        public string DistinctSensitive
        {
            get { return _distinctSensitive; }
            set
            {
                _distinctSensitive = value;
                OnPropertyChanged("DistinctSensitive");
            }
        }



        private string _tCloseness;
        public string TCloseness
        {

            get { return _tCloseness; }
            set
            {
                _tCloseness = value;
                OnPropertyChanged("TCloseness");
            }



        }





        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
