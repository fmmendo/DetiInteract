using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DetiInteract.DSDBroker.DSDWS;
using System.Text;

namespace DetiInteract.DSDBroker.Parsers
{
	public sealed class TimetableParser : IParser
	{
		#region Fields

		/// <summary>
		/// Changed event sends data to the Presentation Layer.
		/// </summary>
		public event ProgressChangedEventHandler Changed;

		/// <summary>
		/// BackgroundWorker to process data on a separate thread.
		/// </summary>
		private BackgroundWorker _worker;

		private const int _idDept = 1;
		private int _idSem;
		private int _year;
		private int _month;
		private int _semester;

		private int[] _courses = new int[] { 11, 15, 18 };
		private int _selectedYear = 1;
		private int _selectedCourseIndex = 0;
		private int _subjectCount;

		private List<TimetableItem>[,] _coursesyear = new List<TimetableItem>[3, 5];
		private string[,] _headers = new string[3, 5];

		/// <summary>
		/// Checks if worker Data has already been retrieved, so we're not calling
		/// the DSD server everytime.
		/// </summary>
		private Boolean _gotData = false;

		#endregion

		/// <summary>
		/// Construcor
		/// </summary>
		public TimetableParser()
		{
			_worker = null;
		}

		#region IParser Methods

		/// <summary>
		/// Triggers the Changed event.
		/// </summary>
		/// <param name="e"></param>
		public void SetChanged(ProgressChangedEventArgs e)
		{
			if (Changed != null) Changed(this, e);
		}

		/// <summary>
		/// Configures and starts the worker
		/// </summary>
		public void Start()
		{
			ConfigWorker();

			_worker.RunWorkerAsync();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public void Stop()
		{
		}

		#endregion

		#region BackgroundWorker Methods

		/// <summary>
		/// Configure the BackgroundWorker
		/// </summary>
		private void ConfigWorker()
		{
			_worker = new BackgroundWorker();
			_worker.WorkerReportsProgress = true;
			_worker.WorkerSupportsCancellation = true;
			_worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
			_worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
			_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
		}

		/// <summary>
		/// Starts the worker.
		/// Calculates the semester, fetches data, and processes the results.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			if (_gotData == false)
			{
				List<Curso> _cursos;
				List<DisciplinaAno> _disciplinaAno;
				List<Aula> _aulas;
				List<Sala> _salas;
				List<Turma> _turmas;
				List<TurmaHora> _turmasHora;
				List<Disciplina> _disciplinas;
				List<Disciplina> _selected_disciplinas;
				List<Semestre> _semestres;

				using (DSDContentProvider DSD = new DSDContentProvider())
				{
					// Fetch all the necessary information from the DSD Content Provider
					_cursos = DSD.GetCursos(DSD.IDDept);
					_salas = DSD.GetSalas(DSD.IDDept);
					_disciplinas = DSD.GetDisciplinas(DSD.IDDept);
					_semestres = DSD.GetSemestres();

					_month = DateTime.Now.Month;
					_year = DateTime.Now.Year;

					// If there has been an error with fetching, get out quickly!
					if (_cursos == null || _salas == null || _disciplinas == null || _semestres == null) return;

					//Set the present semester
					SetCurrentSemester(_semestres);

					_disciplinaAno = DSD.GetDisciplinasAno(DSD.IDDept, DSD.IDSem);
					_aulas = DSD.GetAulas(DSD.IDDept, DSD.IDSem);
					_turmas = DSD.GetTurmas(DSD.IDDept, DSD.IDSem);
					_turmasHora = DSD.GetTurmaHora(DSD.IDDept, DSD.IDSem);

					// If there has been an error with fetching, get out quickly!
					if (_disciplinaAno == null || _aulas == null || _turmas == null || _turmasHora == null) return;
				}

				// for each course
				for (int i = 0; i < 3; i++)
				{
					// for each year
					for (int j = 0; j < 5; j++)
					{
						_selected_disciplinas = GetDisciplinaCursoAno(_disciplinas, _courses[i], j + 1);

						_coursesyear[i, j] = new List<TimetableItem>();

						// If all data checks out, query the results to find each timetable item, and send it to the ProgressChanged handler.
						if (_cursos != null && _disciplinaAno != null && _aulas != null && _salas != null && _turmas != null && _turmasHora != null && _selected_disciplinas.Count > 0)
						{
							// Generates the header for each timetable containing course name, grade, semester and year.
							_headers[i, j] = GetTimetableHeader(_cursos ,i, j);

							//_worker.ReportProgress(0, sb.ToString());

							// Parse the info to isolate classes for a given course and grade
							IEnumerable<TimetableItem> query = from d in _selected_disciplinas
															   join da in _disciplinaAno on d.IDDisciplina equals da.RefIDDisciplina
															   join a in _aulas on da.IDDisciplinaAno equals a.RefIDDisciplinaAno
															   join t in _turmas on a.IDAula equals t.RefIDAula
															   join th in _turmasHora on t.IDTurma equals th.RefIDTurma
															   select new TimetableItem(d.IDDisciplina,
																						(a.Duracao * 2) / 60,
																						2 + (th.HoraInicio.Hour - 8) * 2 + (th.HoraInicio.Minute / 30),
																						th.Dia,
																						String.Format("{0}-{1} ({2})", d.Sigla, t.Nome, GetSala(_salas, _idDept, th.Salas)));

							// Reset the subject counter 
							_subjectCount = 0;
							// list to check for unique subject IDs
							List<int> ids = new List<int>();

							// For each unique subject, increment subject count (for colouring purposes)
							foreach (TimetableItem tti in query)
							{
								if (!ids.Contains(tti.IDSubject))
								{
									ids.Add(tti.IDSubject);
								}

								_subjectCount = ids.IndexOf(tti.IDSubject);

								while (_subjectCount > 9)
								{
									_subjectCount -= 10;
								}

								tti.Color = _subjectCount;

								// Send the TimeTableData to the ReportProgress method
								//_worker.ReportProgress(1, tti);
								_coursesyear[i, j].Add(tti);
							}
						}
					}
				}

				_gotData = true;

				_cursos.Clear();
				_disciplinaAno.Clear();
				_aulas.Clear();
				_salas.Clear();
				_turmas.Clear();
				_turmasHora.Clear();
				_disciplinas.Clear();
				_semestres.Clear();
			} // got data, only runs once

			_worker.ReportProgress(0, _headers[_selectedCourseIndex, _selectedYear]);

			foreach (TimetableItem tti in _coursesyear[_selectedCourseIndex, _selectedYear])
			{
				_worker.ReportProgress(1, tti);
			}
		}

		/// <summary>
		/// Notifies the presentation layer of the workers progress, this will 
		/// either reset the timetable, or place a new item on it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			// If worker just started: set or reset the timetable
			if (e.ProgressPercentage == 0)
			{
				// Get the tect for the timetable header...
				string text = e.UserState as string;

				SetChanged(new ProgressChangedEventArgs(0, text));

				return;
			}
			// If worker finished
			else if (e.ProgressPercentage == 100)
			{
				// do nothing
			}
			// If progress is underway
			else
			{
				// Get the passes TimeTableData...
				TimetableItem ti = (TimetableItem)e.UserState;

				SetChanged(new ProgressChangedEventArgs(1, ti));
			}
		}

		/// <summary>
		/// Handles the end of the workers run and disposes of it.
		/// Cancellation may have been triggerd (selecting a new timetable 
		/// while the previous is still undergoing work, this restarts the 
		/// worker.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				_worker.RunWorkerAsync();
			}
			else if (e.Error != null)
			{
				_worker.Dispose();
			}
		}

		#endregion

		#region Timetable Construction Helpers
		/// <summary>
		/// Generates a list of all the subjects for the given course and year.
		/// </summary>
		/// <param name="id_curso">Course ID</param>
		/// <param name="ano">Year</param>
		/// <returns>List of Disciplina</returns>
		public List<Disciplina> GetDisciplinaCursoAno(List<Disciplina> disciplinas, int id_curso, int ano)
		{
			if (disciplinas == null) return null;

			List<Disciplina> disciplinasFinal = new List<Disciplina>();

			foreach (Disciplina d in disciplinas)
			{
				foreach (DisciplinaCurso dc in d.Cursos)
				{
					if ((dc.RefIDCurso == id_curso) && (dc.Ano == ano) && (dc.Semestre == _semester) && (!disciplinasFinal.Contains(d)))
					{
						disciplinasFinal.Add(d);
					}
				}
			}

			return disciplinasFinal;
		}

		/// <summary>
		/// Accesses the DSD Web Services and requests a list of departments.
		/// </summary>
		/// <returns>List of Docente</returns>
		public string GetSala(List<Sala> salas, int dept, ArrayOfInt num)
		{
			if (salas == null) return "N/D";
			if (num == null || num.Count <= 0) return "N/D";
			foreach (Sala s in salas)
			{
				if (s.IDSala == num[0])
				{
					return s.Numero.ToString();
				}
			}

			return "N/D";
		}

		/// <summary>
		/// Sets the current semester.
		/// </summary>
		private void SetCurrentSemester(List<Semestre> semestres)
		{
			foreach (Semestre s in semestres)
			{

				if (_month == 9 || _month == 10 || _month == 11 || _month == 12)
				{
					if ((s.NSemestre == 1) && (s.AnoCivil == _year))
					{
						_idSem = s.IDSemestre;
						_semester = s.NSemestre;
					}
				}
				else
				{
					if ((s.NSemestre == 2) && (s.AnoCivil == (DateTime.Now.Year - 1)))
					{
						_idSem = s.IDSemestre;
						_semester = s.NSemestre;
						_year -= 1;

					}
				}
			}
		}

		/// <summary>
		/// Gets the header to place at a certain timetable
		/// </summary>
		/// <returns></returns>
		private string GetTimetableHeader(List<Curso> cursos, int courseIndex, int year)
		{
			year = year + 1;

			StringBuilder sb = new StringBuilder();

			sb.Append(cursos.Find(delegate(Curso c) 
									{ return c.IDCurso == _courses[courseIndex]; }
								  ).Nome);
			sb.Append(" - ");
			sb.Append(year.ToString());
			sb.Append("º Ano\n");
			sb.Append(_semester.ToString());
			sb.Append("º Semestre de ");
			sb.Append(_year.ToString());
			sb.Append("-");
			sb.Append(_year + 1);

			return sb.ToString();
		}
		#endregion

		#region Timetable Management

		/// <summary>
		/// Redraws the timetable with the default course and year.
		/// </summary>
		public void ResetState()
		{
			_selectedCourseIndex = 0;
			_subjectCount = 0;

			TimeTable_Refresh();
		}

		/// <summary>
		/// Processes a TimeTable Down action: Next Course
		/// </summary>
		public void TimeTable_Down()
		{
			_selectedCourseIndex++;
			if (_selectedCourseIndex > 2)
			{
				_selectedCourseIndex = 0;
			}
			if (_selectedCourseIndex == 2)
			{
				if (_selectedYear > 3)
					_selectedYear = 3;
			}

			TimeTable_Refresh();
		}

		/// <summary>
		/// Processes a TimeTable Up action: Previous Course
		/// </summary>
		public void TimeTable_Up()
		{
			_selectedCourseIndex--;
			if (_selectedCourseIndex < 0)
			{
				_selectedCourseIndex = 2;
			}
			if (_selectedCourseIndex == 2)
			{
				if (_selectedYear > 3)
					_selectedYear = 3;
			}

			TimeTable_Refresh();
		}

		/// <summary>
		/// Processes a TimeTable Right action: Next Year
		/// </summary>
		public void TimeTable_Right()
		{
			_selectedYear++;
			if (_courses[_selectedCourseIndex] == 11 || _courses[_selectedCourseIndex] == 15)
			{
				if (_selectedYear >= 5)
				{
					_selectedYear = 0;
				}
			}
			else
			{
				if (_selectedYear >= 4)
				{
					_selectedYear = 0;
				}
			}

			TimeTable_Refresh();
		}

		/// <summary>
		/// Processes a TimeTable Left action: Previous Year
		/// </summary>
		public void TimeTable_Left()
		{
			_selectedYear--;
			if (_selectedYear < 0)
			{
				if (_courses[_selectedCourseIndex] == 11 || _courses[_selectedCourseIndex] == 15)
				{
					_selectedYear = 4;
				}
				else
				{
					_selectedYear = 3;
				}
			}

			TimeTable_Refresh();
		}

		/// <summary>
		/// Refreshes the time table by restarting the BackgroundWorker
		/// </summary>
		private void TimeTable_Refresh()
		{
			if (_worker.IsBusy)
				_worker.CancelAsync();
			else
				_worker.RunWorkerAsync();
		}

		#endregion
	}
}
