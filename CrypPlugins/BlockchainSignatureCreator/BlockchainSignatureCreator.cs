﻿/*
   Copyright Eduard Scherf, 2021

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
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.BlockchainSignatureCreator
{
    [Author("Eduard Scherf", "eduard-scherf@gmx.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.Plugins.BlockchainSignatureCreator.Properties.Resources","BlockchainSignatureCreatorCaption", "BlockchainSignatureCreatorTooltip", "BlockchainSignatureCreator/userdoc.xml", "BlockchainSignatureCreator/icon.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Blockchain_Transaction_Signature_Creator : ICrypComponent
    {
        #region Private Variables

        private readonly BlockchainSignatureCreatorSettings settings = new BlockchainSignatureCreatorSettings();

        string senderName;
        string senderN;
        string senderE;
        string senderD;

        string recipientName;
        string recipientN;
        string recipientE;
        string recipientD;

        string amount;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "SenderCaption", "SenderTooltip")]
        public string Sender
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "RecipientCaption", "RecipientTooltip")]
        public string Recipient
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "AmountCaption", "AmountTooltip")]
        public string Amount
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "SignatureCaption", "SignatureOutputTooltip")]
        public string Signature
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            try { 

            ProgressChanged(0, 1);
            string sender = Sender;
            string recipient = Recipient;
            ReadInput();
            var sr3 = new StringBuilder();

            BigInteger sig = createSignature(senderName, recipientName, amount, senderN, senderD);
                sr3.Append(senderName);
                sr3.Append(",");
                sr3.Append(recipientName);
                sr3.Append(",");
                sr3.Append(amount);
                sr3.Append(",");
                sr3.Append(sig);
                Signature = sr3.ToString();
            OnPropertyChanged("Signature");               

            ProgressChanged(1, 1);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(BlockchainSignatureCreator.Properties.Resources.ExceptionMessage, ex.Message), NotificationLevel.Error);
            }


        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            Signature = String.Empty;
        }

        public void Initialize()
        {
        }

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

        #region Helper methods

        public void ReadInput()
        {
            string sender = Sender;
            string recipient = Recipient;

            var sr1 = new StringReader(sender);
            var sr2 = new StringReader(recipient);
            string line1;
            string line2;

            amount = Amount;

            while ((line1 = sr1.ReadLine()) != null)
            {
                if (!line1.StartsWith("#"))
                {
                    string[] data1 = line1.Split(',');
                    senderName = data1[0];
                    senderN = data1[1];
                    senderE = data1[2];
                    senderD = data1[3];
                }
            }

            while ((line2 = sr2.ReadLine()) != null)
            {
                if (!line2.StartsWith("#"))
                {
                    string[] data2 = line2.Split(',');
                    recipientName = data2[0];
                    recipientN = data2[1];
                    recipientE = data2[2];
                    recipientD = data2[3];
                }
            }
        }

        public BigInteger createSignature(string real_from, string real_to, string real_amount, string real_N, string real_d)
        {    
            string from = real_from;
            string to = real_to;
            double amount = double.Parse(real_amount, CultureInfo.InvariantCulture.NumberFormat);
            BigInteger N = BigInteger.Parse(real_N);
            BigInteger d = BigInteger.Parse(real_d);

            var preImage = Encoding.UTF8.GetBytes(from + to + amount);          
            SHA256 sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(preImage);
            BigInteger hashBigInt = new BigInteger(hash);
            
            if (hashBigInt < BigInteger.Zero)
            {
                hashBigInt = hashBigInt * BigInteger.MinusOne;
            }
            hashBigInt = BigInteger.ModPow(hashBigInt, BigInteger.One, N);
            return BigInteger.ModPow(hashBigInt , d , N);    
        }

        public string calculateHash(string stringToHash)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
        #endregion
    }
}
