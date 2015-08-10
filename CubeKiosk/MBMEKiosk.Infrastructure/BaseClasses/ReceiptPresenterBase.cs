using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBMEKiosk.Infrastructure.Interfaces;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows.Markup;
using MBMEKiosk.Infrastructure.ObjectModel;
using System.ComponentModel;

namespace MBMEKiosk.Infrastructure.BaseClasses
{
    public class ReceiptPresenterBase<T> : IReceiptPresenter, INotifyPropertyChanged where T : TransactionContextBase
    {
        private FrameworkElement viewGrid;
        private T transaction;

        public ReceiptPresenterBase(T transaction)
        {
            this.Transaction = transaction;
        }

        public virtual FrameworkElement LoadReceiptXaml(string xamlPath)
        {
            try
            {
                FrameworkElement root = LoadXamlFile(xamlPath);
                root.BeginInit();
                try
                {
                    root.DataContext = this;
                }
                catch (Exception ex)
                {
                    // KS TODO: Add log messages.
                    throw ex;
                }
                finally
                {
                    root.EndInit();
                    ViewGrid = root;
                    root.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception details: \nSource: {0}\nMessage: {1}\nTrace:", ex.Source, ex.Message, ex.StackTrace);
                throw ex;
            }

            return ViewGrid;
        }

        protected FrameworkElement LoadXamlFile(string filePath)
        {
            FrameworkElement root = null;
            try
            {
                FileStream s = new FileStream(filePath, FileMode.Open);
                root = XamlReader.Load(s) as FrameworkElement;
                s.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception details: \nSource: {0}\nMessage: {1}\nTrace:", ex.Source, ex.Message, ex.StackTrace);
                MessageBox.Show(ex.Message);
            }

            return root;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public FrameworkElement ViewGrid
        {
            get
            {
                return viewGrid;
            }

            set
            {
                if (viewGrid != value)
                {
                    viewGrid = value;
                    OnPropertyChanged("ViewGrid");
                }
            }
        }

        public T Transaction
        {
            get
            {
                return transaction;
            }

            protected set
            {
                if (transaction != value)
                {
                    transaction = value;
                    OnPropertyChanged("Transaction");
                }
            }
        }
    }
}
