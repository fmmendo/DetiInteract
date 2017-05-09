using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System;
using System.Windows.Threading;

namespace DataVirtualization
{
    /// <summary>
    /// Derived VirtualizatingCollection, performing loading asychronously.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    public class AsyncVirtualizingCollection<T> : VirtualizingCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
		BackgroundWorker bwCount = new BackgroundWorker();
		BackgroundWorker bwPage = new BackgroundWorker();

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider)
            : base(itemsProvider)
        {
            //_synchronizationContext = SynchronizationContext.Current;

			ConfigureWorkers();
        }

		private void ConfigureWorkers()
		{
			bwPage.WorkerReportsProgress = true;
			bwPage.DoWork += new DoWorkEventHandler(bwPage_DoWork);
			bwPage.ProgressChanged += new ProgressChangedEventHandler(bwPage_ProgressChanged);
			bwPage.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwPage_RunWorkerCompleted);

			bwCount.WorkerReportsProgress = true;
			bwCount.DoWork += new DoWorkEventHandler(bwCount_DoWork);
			bwCount.ProgressChanged += new ProgressChangedEventHandler(bwCount_ProgressChanged);
			bwCount.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwCount_RunWorkerCompleted);
		}



        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize)
            : base(itemsProvider, pageSize)
        {
            //_synchronizationContext = SynchronizationContext.Current;

			ConfigureWorkers();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageTimeout">The page timeout.</param>
        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize, int pageTimeout)
            : base(itemsProvider, pageSize, pageTimeout)
        {

			ConfigureWorkers();
        }

        #endregion

        #region INotifyCollectionChanged

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler h = CollectionChanged;
            if (h != null)
                h(this, e);
        }

        /// <summary>
        /// Fires the collection reset event.
        /// </summary>
        private void FireCollectionReset()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void FirePropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(e);
        }

        #endregion

        #region IsLoading

        private bool _isLoading;

        /// <summary>
        /// Gets or sets a value indicating whether the collection is loading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this collection is loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if ( value != _isLoading )
                {
                    _isLoading = value;
                }
                FirePropertyChanged("IsLoading");
            }
        }

        #endregion

        #region Load overrides

        /// <summary>
        /// Asynchronously loads the count of items.
        /// </summary>
		protected override void LoadCount()
        {
            Count = 0;
            IsLoading = true;

			if (!bwCount.IsBusy)
				bwCount.RunWorkerAsync();
        }


		/// <summary>
		/// Performed on background thread.
		/// </summary>
		/// <param name="args">None required.</param>
		void bwCount_DoWork(object sender, DoWorkEventArgs e)
		{
			bwCount.ReportProgress(100);
		}

		void bwCount_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			Count = FetchCount();
			
			IsLoading = false;
			FireCollectionReset();
		}

		
		/// <summary>
		/// Performed on UI-thread after LoadCountWork.
		/// </summary>
		/// <param name="args">Number of items returned.</param>
		void bwCount_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{

		}


        /// <summary>
        /// Asynchronously loads the page.
        /// </summary>
        /// <param name="index">The index.</param>
		protected override void LoadPage(int index)
        {
			IsLoading = true;

			if (!bwPage.IsBusy)
				bwPage.RunWorkerAsync(index);
        }


		/// <summary>
		/// Performed on background thread.
		/// </summary>
		/// <param name="args">Index of the page to load.</param>
		void bwPage_DoWork(object sender, DoWorkEventArgs e)
		{
			bwPage.ReportProgress(100, e.Argument);
		}

		void bwPage_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			int pageIndex = (int)e.UserState;
			IList<T> page = FetchPage(pageIndex);

			PopulatePage(pageIndex, page);
			IsLoading = false;
			FireCollectionReset();
		}

		/// <summary>
		/// Performed on UI-thread after LoadPageWork.
		/// </summary>
		/// <param name="args">object[] { int pageIndex, IList(T) page }</param>
		void bwPage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{

		}

        #endregion
    }
}
