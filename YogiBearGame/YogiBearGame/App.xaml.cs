using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using YogiBearGame.Model;
using YogiBearGame.ViewModel;
using YogiBearGame.Persistence;
using System.ComponentModel;
using Microsoft.Win32;
using YogiBearGame.View;
using System.Diagnostics;

namespace YogiBearGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private YogiBearGameModel _model;
        private YogiBearViewModel _viewModel;
        private MainWindow _view;
        private DispatcherTimer _timer;

        #endregion

        #region Application event handlers
        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // modell létrehozása
            _model = new YogiBearGameModel(new YogiBearFileDataAccess());

            // nézemodell létrehozása
            _viewModel = new YogiBearViewModel(_model);

            //eseményekre feliratkozás
            _model.GameOver += new EventHandler<YogiBearEventArgs>(Model_GameOver);
            _viewModel.NewGame += new EventHandler(ViewModel_NewGame);
            _viewModel.ExitGame += new EventHandler(ViewModel_ExitGame);
            _viewModel.LoadGameTables += new EventHandler(ViewModel_LoadGame);
            _viewModel.StartStop += new EventHandler(ViewModel_StartStop);
            
            // nézet létrehozása
            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.newGameMenuItem.IsEnabled = false;
            _view.stopMenuItem.IsEnabled = false;
            _view.startMenuItem.IsEnabled = false;
            _view.Closing += new System.ComponentModel.CancelEventHandler(View_Closing); // eseménykezelés a bezáráshoz
            _view.Show();

            // időzítő létrehozása
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += new EventHandler(Timer_Tick);

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _model.AdvanceTime();
        }

        #endregion

        #region View event handlers

        /// <summary>
        /// Nézet bezárásának eseménykezelője.
        /// </summary>
        private void View_Closing(object sender, CancelEventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;

            _timer.Stop();

            if (MessageBox.Show("Are you sure to exit from game?", "YogiBearGame", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; // töröljük a bezárást

                if (restartTimer) // ha szükséges, elindítjuk az időzítőt
                    _timer.Start();
            }
        }

        #endregion

        #region ViewModel event handlers

        private void ViewModel_StartStop(object sender, EventArgs e)
        {
            if (_timer.IsEnabled)
            {
                _model.GameIsOn = false;
                _timer.Stop();
                _view.stopMenuItem.IsEnabled = false;
                _view.startMenuItem.IsEnabled = true;
            }
            else
            {
                _model.GameIsOn = true;
                _timer.Start();
                _view.stopMenuItem.IsEnabled = true;
                _view.startMenuItem.IsEnabled = false;
            }
        }

        /// <summary>
        /// Új játék indításának eseménykezelője.
        /// </summary>
        private void ViewModel_NewGame(object sender, EventArgs e)
        {
            _model.NewGame();
            _timer.Start();
            _view.stopMenuItem.IsEnabled = true;

            switch (_model.GameTable)
            {
                case GameTable.Small: _view.Height = 580; _view.Width = 500; break;
                case GameTable.Medium: _view.Height = 745; _view.Width = 665; break;
                case GameTable.Large: _view.Height = 1000; _view.Width = 930; break;
            }
        }

        /// <summary>
        /// Játék betöltésének eseménykezelője.
        /// </summary>
        private async void ViewModel_LoadGame(object sender, System.EventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;

            _timer.Stop();
            _model.GameIsOn = false;
            // innen meg lehet hianyzik a jatektabla beallitasa
            OpenFileDialog openFileDialog = new OpenFileDialog(); // dialógusablak
            openFileDialog.Title = "Loading YogiBear gametables";
            openFileDialog.Filter = "YogiBear table|*.txt";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true) { 
                foreach (String file in openFileDialog.FileNames)
                {
                    try
                    {
                        await _model.LoadGameAsync(file);
                    }
                    catch (YogiBearDataException)
                    {
                        MessageBox.Show("Gametables loading failed!" + Environment.NewLine + "The access path or the file format is wrong.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                _view.newGameMenuItem.IsEnabled = true;
            }

            if (restartTimer) // ha szükséges, elindítjuk az időzítőt
                _timer.Start();
            _model.GameIsOn = true;
        }

        /// <summary>
        /// Játékból való kilépés eseménykezelője.
        /// </summary>
        private void ViewModel_ExitGame(object sender, System.EventArgs e)
        {
            _view.Close(); // ablak bezárása
        }

        #endregion

        #region Model event handlers

        /// <summary>
        /// Játék végének eseménykezelője.
        /// </summary>
        private void Model_GameOver(object sender, YogiBearEventArgs e)
        {
            _timer.Stop();
            if (e.IsWon) // győzelemtől függő üzenet megjelenítése
            {
                MessageBox.Show("Congratulation, you win!" + Environment.NewLine +
                                "Game time: " + TimeSpan.FromSeconds(e.GameTime).ToString("g"),
                                "Yogi Bear Game",
                                MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
            }
            else
            {
                MessageBox.Show("A ranger saw you." +
                                "Sorry, you lose!",
                                "Yogi Bear Game",
                                MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
            }
        }

        #endregion
    }
}
