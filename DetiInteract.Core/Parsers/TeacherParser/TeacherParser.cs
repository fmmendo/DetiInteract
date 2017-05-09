using System.Collections.Generic;
using System.ComponentModel;
using DetiInteract.DSDBroker.DSDWS;
using System;

namespace DetiInteract.DSDBroker.Parsers
{
    public sealed class TeacherParser : IParser
    {
        #region Fields

        /// <summary>
        /// Changed event 
		/// </summary>
        public event ProgressChangedEventHandler Changed;

        /// <summary>
        /// Background worker to deal with the data on a separate thread .
		/// </summary>
        private BackgroundWorker Worker;

		List<TeacherItem> teachers = new List<TeacherItem>();
        
        #endregion

		/// <summary>
		/// Constructor
		/// </summary>
        public TeacherParser()
        {
            Worker = null;
        }

        #region IParser Methods

        /// <summary>
        /// Activates the Changed event
        /// </summary>
        /// <param name="e">ProgressChangedEventArgs</param>
        public void SetChanged(ProgressChangedEventArgs e)
        {
            if (Changed != null) Changed(this, e);
        }

		/// <summary>
		/// Configures the worker and begins work.
		/// </summary>
        public void Start()
        {
            ConfigWorker();

            Worker.RunWorkerAsync();
        }

		/// <summary>
		/// Pauses the worker. Unused.
		/// </summary>
        public void Pause()
        {
            return;
        }

		/// <summary>
		/// Stops the worker. Unused
		/// </summary>
        public void Stop()
        {
            return;
        }
        #endregion

        #region BackgroundWorker Events

		/// <summary>
		/// Configures the Worker.
		/// </summary>
		private void ConfigWorker()
		{
			Worker = new BackgroundWorker();
			Worker.WorkerReportsProgress = true;
			Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
			Worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
			Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
		}

		/// <summary>
		/// Starts working on the data.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
			// Using ensures DSD reference is destroyd in the end.
            using (DSDContentProvider DSD = new DSDContentProvider())
            {
                // Get the teacher list from DSD Content Provider
                List<Docente> docentes = DSD.GetDocentes(DSD.IDDept);

				teachers.Clear();

				if (docentes != null)
				{
					foreach (Docente d in docentes)
					{
                        if (d.VisivelLista)
                        {
                            if (String.IsNullOrEmpty(d.Foto)) continue;

                            TeacherItem ti = new TeacherItem(d.Pessoa.Nome, String.Format("{0}{1}", DSDContentProvider.DSDFilePath, d.Foto), d.Gabinete, d.Extensao, d.Webpage);
                            teachers.Add(ti);
                        }
					}
				}
				else
				{
					//Add an item with a warning for bad connection
					TeacherItem ti = new TeacherItem("Erro ao carregar docentes!", DSDContentProvider.DSDFilePath, "", "", "");

					teachers.Add(ti);
				}

				Worker.ReportProgress(100);
            }
        }

		/// <summary>
		/// Handles a ProgressChanged evend. Extracts a TeacherItem and sends 
		/// it to the Presentation layer.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
            {
				// Fire the Changed event and send the List of TeacherItem to its handler.
                SetChanged(new ProgressChangedEventArgs(1, teachers));
            }
        }

		/// <summary>
		/// Disposes of the worker.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Worker.Dispose();

				teachers.Clear();
            }
        }

        #endregion
	}
}
