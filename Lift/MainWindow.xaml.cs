using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


// COMMAND LINE PARAMETERS:

// -f <how many floors> - number of floors in the building (5 - 20). Type: int. Default value: 10.
// -h <floor height> - floor height (meters). Type: double. Default value: 3.
// -s <lift speed> - lift speed (meters per second). Type: double. Default value: 2.
// -o <openning time> - lift door openning time (seconds). Type: double. Default value: 1.
// -c <closing time> - lift door closing time (seconds). Type: double. Default value: 1.
// -w <waiting time when door opened> - waiting time lift door still opened (seconds). Type: double. Default value: 3.


namespace Lift
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        // Raised when a property on this object has a new value.
        public event PropertyChangedEventHandler PropertyChanged;

        // --------------------------------------------
        // FLOOR PARAMETERS

        private int _floorsTotal; // how many floor in building

        public int FloorsTotal
        {
            get
            {
                return _floorsTotal;
            }
            set
            {
                if (_floorsTotal != value)
                {
                    _floorsTotal = value;
                    OnPropertyChanged("FloorsTotal");
                }
            }
        }



        private double _floorHeight; // floor height in meters

        public double FloorHeight
        {
            get
            {
                return _floorHeight;
            }
            set
            {
                if (_floorHeight != value)
                {
                    _floorHeight = value;
                    OnPropertyChanged("FloorHeight");
                }
            }
        }


        private const int FLOOR_HEIGHT = 100; // floor height for schema painting (DIP)


        private double _floorLineStrokeThickness = 1;


        // FLOOR ITEM (floor panel to lift call)
        public class FloorItem
        {
            public int floorNumber { get; set; } // floor number (starts from 1)

            public bool callButtonPressed { get; set; }

        }

        // --------------------------------------------
        // DOORS PARAMETERS

        private double _doorsClosingTime; // lift doors closing time in seconds

        public double DoorsClosingTime
        {
            get
            {
                return _doorsClosingTime;
            }
            set
            {
                if (_doorsClosingTime != value)
                {
                    _doorsClosingTime = value;
                    OnPropertyChanged("DoorsClosingTime");
                }
            }
        }


        private double _doorsOpeningTime; // lift doors opening time in seconds

        public double DoorsOpeningTime
        {
            get
            {
                return _doorsOpeningTime;
            }
            set
            {
                if (_doorsOpeningTime != value)
                {
                    _doorsOpeningTime = value;
                    OnPropertyChanged("DoorsOpeningTime");
                }
            }
        }


        private double _doorsWaitOpenTime; // waiting time lift door still opened (seconds)


        public double DoorsWaitOpenTime
        {
            get
            {
                return _doorsWaitOpenTime;
            }
            set
            {
                if (_doorsWaitOpenTime != value)
                {
                    _doorsWaitOpenTime = value;
                    OnPropertyChanged("DoorsWaitOpenTime");
                }
            }
        }



        private const int DOOR_WIDTH = 60; // door rectangle width in DIP = (LIFT_WIDTH / 2)

        private Rectangle _leftDoorRect;

        private static BackgroundWorker _backgroundWorkerCloseDoor;

        private static BackgroundWorker _backgroundWorkerOpenDoor;

        private static BackgroundWorker _backgroundWorkerDoorWaitOpened;

        private enum DoorStateEnum
        {
            DOORS_CLOSED,
            DOORS_OPENED,
            DOORS_OPENING,
            DOORS_CLOSING
        }

        private static DoorStateEnum _doorState;

        // --------------------------------------------
        // LIFT PARAMETERS

        private double _liftSpeed; // lift speed (meters per second)

        public double LiftSpeed
        {
            get
            {
                return _liftSpeed;
            }
            set
            {
                if (_liftSpeed != value)
                {
                    _liftSpeed = value;
                    OnPropertyChanged("LiftSpeed");
                }
            }
        }


        private Rectangle _liftRect; // lift rectangle

        private static double _currentLiftPositionY; // current lift Y-position (lift height)

        private static int _currentLiftFloor; // current lift floor (value from 1 to _floorsTotal)

        private const int LIFT_HEIGHT = 80; // lift rectangle height

        private const int LIFT_WIDTH = 60; // lift rectangle width

        private static BackgroundWorker _backgroundWorkerLift;

        private enum LiftStateEnum
        {
            STOPPED,
            MOVE_UP,
            MOVE_DOWN
        }

        private static LiftStateEnum _liftState;


        private static bool _liftProcessing;


        // --------------------------------------------

        private static object _lockObject = new object();


        // lift tasks list
        private List<CallItem> _liftCallsList = new List<CallItem>();


        public class CallItem
        {
            public int floorNumber { get; set; } // floor number (starts from 1)

            public double floorPositionY { get; set; } // floor pos Y

        }


        private static ObservableCollection<FloorItem> _floorItemsList = new ObservableCollection<FloorItem>();

        public ObservableCollection<FloorItem> FloorItemsList
        {
            get
            {
                return _floorItemsList;
            }
            set
            {
                if (_floorItemsList != value)
                {
                    _floorItemsList = value;
                    OnPropertyChanged("FloorItemsList");
                }
            }
        }


        private int _selectedFloorIndex;


        public int SelectedFloorIndex
        {
            get
            {
                return _selectedFloorIndex;
            }

            set
            {
                if (_selectedFloorIndex != value)
                {
                    _selectedFloorIndex = value;
                    OnPropertyChanged("SelectedFloorIndex");
                }
            }
        }

        //private double _translateTransformY;


        //public double TranslateTransformY
        //{
        //    get
        //    {
        //        return _translateTransformY;
        //    }

        //    set
        //    {
        //        if (_translateTransformY != value)
        //        {
        //            _translateTransformY = value;
        //            OnPropertyChanged("TranslateTransformY");
        //        }
        //    }
        //}


        // --------------------------------------------
        // CONSTRUCTOR
        public MainWindow()
        {
            InitializeComponent();
        }


        private void SetDefaultParameters()
        {
            // SET DEFAULT VALUES

            // -f <how many floors> - number of floors in the building (5 - 20). Type: int. Default value: 10.
            // -h <floor height> - floor height (meters). Type: double. Default value: 3.
            // -s <lift speed> - lift speed (meters per second). Type: double. Default value: 2.
            // -o <openning time> - lift door openning time (seconds). Type: double. Default value: 1.
            // -c <closing time> - lift door closing time (seconds). Type: double. Default value: 1.
            // -w <waiting time when door opened> - waiting time lift door still opened (seconds). Type: double. Default value: 3.

            FloorsTotal = 10;      // -f

            FloorHeight = 3;       // -h

            LiftSpeed = 2;         // -s

            DoorsOpeningTime = 1;  // -o

            DoorsClosingTime = 1;  // -c

            DoorsWaitOpenTime = 3; // -w
        }


        private void Init()
        {
            // set schema canvas height
            showLift.Height = _floorsTotal * FLOOR_HEIGHT;

            //TranslateTransformY = _floorsTotal * FLOOR_HEIGHT;
            //OnPropertyChanged("TranslateTransformY");
            //TranslateTransform translateTransform = new TranslateTransform(0, 300);
            //ScaleTransform scaleTransform = new ScaleTransform(0, -1);

            //this.showLift.RenderTransform = scaleTransform;
            //this.showLift.RenderTransform = translateTransform;

            // Init
            //FloorItemsList = new ObservableCollection<FloorItem>();

            for (int i = _floorsTotal - 1; i >= 0; i--)
            {
                FloorItemsList.Add(new FloorItem { floorNumber = i + 1 });
            }


            floorListView.AddHandler(Button.ClickEvent, new RoutedEventHandler(buttonFloorCall_Click));

            //floorListView.ItemsSource = FloorItemsList;




            // set default lift state
            _liftState = LiftStateEnum.STOPPED;

            _doorState = DoorStateEnum.DOORS_OPENED;

            /// _liftPositionY = (_floorsTotal) * FLOOR_HEIGHT - LIFT_HEIGHT - 1; // lift on the 1 floor
            /// _liftPositionY = (_floorsTotal - 2 + 1) * FLOOR_HEIGHT - LIFT_HEIGHT - 1; // lift on the 2 floor

            _currentLiftFloor = 1; // lift on the FIRST floor !!

            _currentLiftPositionY = (_floorsTotal - _currentLiftFloor + 1) * FLOOR_HEIGHT - LIFT_HEIGHT - 1; // lift on the 1 floor !!


            //MessageBox.Show("_liftPositionY = " + _liftPositionY.ToString()); // 1919


            // ---------------------------------------------
            // LIFT backgroundWorker


            _backgroundWorkerLift = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            // Lift backgroundWorker events    
            //For the performing operation in the background
            _backgroundWorkerLift.DoWork += Lift_DoWork;

            //For the display of operation progress to UI
            _backgroundWorkerLift.ProgressChanged += Lift_ProgressChanged;

            //After the completation of operation
            _backgroundWorkerLift.RunWorkerCompleted += Lift_RunWorkerCompleted;
            // DOOR CLOSE backgroundWorker

            _backgroundWorkerCloseDoor = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };


            // ---------------------------------------------
            // DOOR CLOSE backgroundWorker

            //For the performing operation in the background
            _backgroundWorkerCloseDoor.DoWork += CloseDoor_DoWork;

            //For the display of operation progress to UI
            _backgroundWorkerCloseDoor.ProgressChanged += CloseDoor_ProgressChanged;

            //After the completation of operation
            _backgroundWorkerCloseDoor.RunWorkerCompleted += CloseDoor_RunWorkerCompleted;


            // ---------------------------------------------
            // DOOR OPEN backgroundWorker

            _backgroundWorkerOpenDoor = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };

            //For the performing operation in the background
            _backgroundWorkerOpenDoor.DoWork += OpenDoor_DoWork;

            //For the display of operation progress to UI
            _backgroundWorkerOpenDoor.ProgressChanged += OpenDoor_ProgressChanged;

            //After the completation of operation
            _backgroundWorkerOpenDoor.RunWorkerCompleted += OpenDoor_RunWorkerCompleted;


            // ---------------------------------------------
            // DOOR OPENED WAIT backgroundWorker

            _backgroundWorkerDoorWaitOpened = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };

            //For the performing operation in the background
            _backgroundWorkerDoorWaitOpened.DoWork += DoorWaitOpened_DoWork;

            //For the display of operation progress to UI
            /// _backgroundWorkerDoorWaitOpened.ProgressChanged += DoorWaitOpened_ProgressChanged;

            //After the completation of operation
            _backgroundWorkerDoorWaitOpened.RunWorkerCompleted += DoorWaitOpened_RunWorkerCompleted;


            // ---------------------------------------------
            // PAINT LIFT SCHEMA


            for (int i = _floorsTotal; i > 0; i--)
            {
                // --------------------------------------------------
                // create floor horizontal line

                Line floorLine = new Line()
                {
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = _floorLineStrokeThickness,
                    X1 = 0,
                    Y1 = i * FLOOR_HEIGHT - _floorLineStrokeThickness,
                    X2 = 300,
                    Y2 = i * FLOOR_HEIGHT - _floorLineStrokeThickness,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                /// myLine.VerticalAlignment = VerticalAlignment.Center;

                // add to parent control
                this.showLift.Children.Add(floorLine);


                // --------------------------------------------------
                // create TextBlock for display floor number

                TextBlock floorNumberTextBlock = new TextBlock
                {
                    Text = FloorItemsList[i - 1].floorNumber.ToString(),
                    Width = 100,
                    Height = 50,
                    FontSize = 20
                    //BorderThickness = new Thickness(1),
                    //BorderBrush = new SolidColorBrush(Color.FromRgb(5, 5, 5)),
                    //Margin = new Thickness(20, 20, 0, 0)
                };

                // add to parent control
                this.showLift.Children.Add(floorNumberTextBlock);

                // set TextBlock position
                Canvas.SetLeft(floorNumberTextBlock, 130); // X-position
                Canvas.SetTop(floorNumberTextBlock, i * FLOOR_HEIGHT - FLOOR_HEIGHT / 2); // Y-position
            }


            //TranslateTransform translateTransform = new TranslateTransform(0, 300);
            //ScaleTransform scaleTransform = new ScaleTransform(0, -1);

            //this.showLift.RenderTransform = scaleTransform;
            //this.showLift.RenderTransform = translateTransform;


            // --------------------------------------------------
            // paint LIFT

            _liftRect = new Rectangle
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                Width = LIFT_WIDTH,
                Height = LIFT_HEIGHT,
                Fill = new SolidColorBrush(Colors.White)
            };

            // set lift position
            Canvas.SetLeft(_liftRect, 20); // X
            Canvas.SetTop(_liftRect, _currentLiftPositionY); // Y

            // add to parent control
            this.showLift.Children.Add(_liftRect);


            // --------------------------------------------------
            // paint DOOR

            _leftDoorRect = new Rectangle
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Colors.Blue),
                Width = 0, // LIFT_WIDTH
                Height = LIFT_HEIGHT
            };

            // set door position
            Canvas.SetLeft(_leftDoorRect, 20);
            Canvas.SetTop(_leftDoorRect, _currentLiftPositionY);

            // add to parent control
            this.showLift.Children.Add(_leftDoorRect);


            // --------------------------------------------------
            // paint inner lift control panel (controlWrapPanel)

            for (int i = 0; i < _floorsTotal; i++)
            {
                Button button = new Button
                {
                    Width = 60,
                    Height = 60,
                    Content = i + 1,
                    Margin = new Thickness(10),
                    FontSize = 20
                };

                // add click event handler to every button
                button.AddHandler(Button.ClickEvent, new RoutedEventHandler(buttonLift_Click));

                // add to parent control
                this.controlWrapPanel.Children.Add(button);
            }


            // scroll all containers to bottom
            floorListView.SelectedItem = floorListView.Items[floorListView.Items.Count - 1];
            floorListView.ScrollIntoView(floorListView.SelectedItem);

            floorsScrollViewer.ScrollToBottom();

            /// controlPanelScrollViewer.ScrollToBottom();
        }


        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        // ---------------------------------------------
        // DOOR CLOSE

        // door closing animation
        private void CloseDoor_DoWork(object sender, DoWorkEventArgs e)
        {
            _liftProcessing = true;

            BackgroundWorker doorWorker = (BackgroundWorker)sender;

            // get parameter
            /// double targetFloorY = (double)e.Argument;
            double targetFloorY = _liftCallsList[0].floorPositionY;

            // calculate time delay to 1 pixel paint
            // result will be a little slower
            int delayMilliseconds = (int)(_doorsClosingTime / DOOR_WIDTH * 1000 * 0.96d);

            //MessageBox.Show(delayMilliseconds.ToString());

            for (double x = 0; x <= DOOR_WIDTH; x++)
            {
                doorWorker.ReportProgress(0, x); // animate door closing

                System.Threading.Thread.Sleep(delayMilliseconds); // time delay
            }

            _doorState = DoorStateEnum.DOORS_CLOSED;// set door state = closed

            // set data to "CloseDoor_RunWorkerCompleted" method
            /// e.Result = targetFloorY;
            e.Result = _liftCallsList[0].floorPositionY;
        }


        // door animation
        private void CloseDoor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            double x = (double)e.UserState;

            _leftDoorRect.Width = x;
        }


        // door closed
        private void CloseDoor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("_liftState = " + _liftState.ToString());

            // run lift move
            _backgroundWorkerLift.RunWorkerAsync(e.Result); // e.Result = target Y-coordinate
        }



        // ---------------------------------------------
        // LIFT

        // lift moving process
        private void Lift_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker liftWorker = (BackgroundWorker)sender;

            // get parameter: target Y-coordinate
            /// double endFloorHeight = (double)e.Argument;
            double endFloorHeight = _liftCallsList[0].floorPositionY;


            // calculate time delay to 1 pixel paint
            // result will be a little slower
            int delayMilliseconds = (int)(_floorHeight / _liftSpeed / (double)FLOOR_HEIGHT * 1000 * 0.95d);

            //MessageBox.Show(delayMilliseconds.ToString());

            /// MessageBox.Show("endFloorHeight = " + endFloorHeight + "  _liftPositionY = " + _liftPositionY);


            double start = _currentLiftPositionY;
            double end = endFloorHeight;

            if (endFloorHeight > _currentLiftPositionY) // moving DOWN
            {
                // set LIFT STATE = MOVING DOWN
                _liftState = LiftStateEnum.MOVE_DOWN;

                Debug.WriteLine("_liftState = " + _liftState.ToString());

                for (double height = start; height < end; height++)
                {
                    if (_backgroundWorkerLift.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    liftWorker.ReportProgress(0, height);


                    // lock threads common variables
                    lock (_lockObject)
                    {
                        _currentLiftPositionY = height;
                    }


                    System.Threading.Thread.Sleep(delayMilliseconds);
                }

                e.Result = _currentLiftPositionY;
            }
            else // moving UP
            {
                // set LIFT STATE = MOVING UP
                _liftState = LiftStateEnum.MOVE_UP;

                Debug.WriteLine("_liftState = " + _liftState.ToString());

                for (double height = start; height > end - 1; height--)
                {
                    if (_backgroundWorkerLift.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    liftWorker.ReportProgress(0, height);

                    _currentLiftPositionY = height;

                    System.Threading.Thread.Sleep(delayMilliseconds);
                }

                /// e.Result = _currentLiftPositionY;
                e.Result = _liftCallsList[0].floorPositionY;
            }
        }


        // lift moving animation
        private void Lift_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            double height = (double)e.UserState;

            // set lift position
            Canvas.SetTop(_liftRect, height);

            // set lift door position
            Canvas.SetTop(_leftDoorRect, height);
        }


        // lift stopped
        private void Lift_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // set LIFT STATE = STOPPED
            _liftState = LiftStateEnum.STOPPED;

            Debug.WriteLine("_liftState = " + _liftState.ToString());

            // run openning door
            _backgroundWorkerOpenDoor.RunWorkerAsync(e.Result); // =  _liftPositionY
        }


        // ---------------------------------------------
        // DOOR OPEN

        // door openning animation

        private void OpenDoor_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker doorWorker = (BackgroundWorker)sender;

            // get parameter
            double targetFloorY = (double)e.Argument;


            // calculate time delay to 1 pixel paint
            // result will be a little slower
            int delayMilliseconds = (int)(_doorsOpeningTime / DOOR_WIDTH * 1000 * 0.97);

            //MessageBox.Show(delayMilliseconds.ToString());

            for (double x = DOOR_WIDTH; x >= 0; x--)
            {
                doorWorker.ReportProgress(0, x);// animate door openning

                System.Threading.Thread.Sleep(delayMilliseconds); // time delay
            }

            _doorState = DoorStateEnum.DOORS_OPENED; // set door state = opened


            e.Result = targetFloorY;
        }


        // door openning animation
        private void OpenDoor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            double x = (double)e.UserState;

            _leftDoorRect.Width = x;
        }


        // when door opened
        private void OpenDoor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // run waiting opened door
            _backgroundWorkerDoorWaitOpened.RunWorkerAsync(e.Result); // =  _liftPositionY
        }

        // ---------------------------------------------

        // door opened waiting
        private void DoorWaitOpened_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < DoorsWaitOpenTime * 100; i++) // 100
            {
                if (_backgroundWorkerDoorWaitOpened.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                System.Threading.Thread.Sleep(10);
            }

            //System.Threading.Thread.Sleep((int)(DoorsWaitOpenTime * 1000)); // waiting when door open
        }


        // when opened door waiting ends
        private void DoorWaitOpened_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // remove completed task from lift task list
            _liftCallsList.RemoveAt(0);

            // _currentTargetFloor = _liftCallsList[i].floorNumber; // set closest target

            // task completed
            _liftProcessing = false;


            ShowTestData();


            // if call queque is empty then nothing to do
            if (_liftCallsList.Count == 0)
            {
                Debug.WriteLine("ALL TASKS DONE: _liftState = " + _liftState.ToString());
                return;
            }

            // another optimization
            SetClosestTarget();


            // run next task
            _backgroundWorkerCloseDoor.RunWorkerAsync(_liftCallsList[0].floorPositionY);
        }


        // ---------------------------------------------


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // get command line arguments array
            string[] argumentsArray = Environment.GetCommandLineArgs();


            SetDefaultParameters();

            //Init();


            // when no command line parameters found
            if (argumentsArray.Length == 1) // when only full path to application
            {
                //SetDefaultParameters();

                Init();

                return;
            }

            // convert to list
            List<string> argumentsList = argumentsArray.ToList();

            // remove first element
            argumentsList.RemoveAt(0);

            string allParameters = string.Join("", argumentsList).ToUpper();

            string[] parametersArray = allParameters.Split('-');

            bool parameterError = false;

            for (int i = 1; i < parametersArray.Length; i++) // without first empty string !!
            {
                string allParameter = parametersArray[i].Trim();

                string parameter = allParameter.Substring(0, 1).ToUpper();

                string paramValue = allParameter.Substring(1, allParameter.Length - 1).ToUpper();

                /*
                 
            FloorsTotal = 10;      // -f
            FloorHeight = 3;       // -h
            LiftSpeed = 2;         // -s
            DoorsOpeningTime = 1;  // -o
            DoorsClosingTime = 1;  // -c
            DoorsWaitOpenTime = 3; // -w
                 
                 */

                double dValue = 0;


                switch (parameter)
                {
                    case "F": // int value

                        int iValue = ConvertToInt(paramValue);

                        if (iValue == -1)
                        {
                            parameterError = true;
                        }

                        FloorsTotal = iValue;

                        break;

                    case "H": // double value

                        dValue = ConvertToDouble(paramValue);

                        if (dValue == -1)
                        {
                            parameterError = true;
                        }

                        FloorHeight = dValue;

                        break;

                    case "S": // double value

                        dValue = ConvertToDouble(paramValue);

                        if (dValue == -1)
                        {
                            parameterError = true;
                        }

                        LiftSpeed = dValue;

                        break;

                    case "O": // double value

                        dValue = ConvertToDouble(paramValue);

                        if (dValue == -1)
                        {
                            parameterError = true;
                        }

                        DoorsOpeningTime = dValue;

                        break;

                    case "C": // double value

                        dValue = ConvertToDouble(paramValue);

                        if (dValue == -1)
                        {
                            parameterError = true;
                        }

                        DoorsClosingTime = dValue;

                        break;

                    case "W": // double value

                        dValue = ConvertToDouble(paramValue);

                        if (dValue == -1)
                        {
                            parameterError = true;
                        }

                        DoorsWaitOpenTime = dValue;

                        break;

                    default: // error parameter value

                        parameterError = true; // wrong parameter letter

                        break;
                }
            }


            Init();

            // if parameters error then wrong parameters = default parameters
            if (parameterError)
            {
                // create error info window
                ParametersErrorWindow errorWindow = new ParametersErrorWindow();

                errorWindow.ShowDialog(); // show error info window

                return;
            }
        }



        // when floor lift call button clicked
        private void buttonFloorCall_Click(object sender, RoutedEventArgs e)
        {
            // get pressed button
            Button button = e.OriginalSource as Button;

            // get selected floor number (starts from 1)
            int selectedFloor = (int)button.Tag;

            // highlight listView row
            SelectedFloorIndex = _floorsTotal - selectedFloor;

            // add new task
            AddTask(selectedFloor);
        }


        // lift inner control panel button clicked
        private void buttonLift_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;

            int selectedFloor = 0;

            // https://msdn.microsoft.com/ru-ru/library/system.convert(v=vs.110).aspx
            try
            {
                selectedFloor = Convert.ToInt32(button.Content); // convert from object to int
            }
            catch
            {
                MessageBox.Show("Can`t convert to int");
                return;
            }

            // add new task
            AddTask(selectedFloor);
        }


        private void AddTask(int targetFloor)
        {
            // is such floor un the task list found?
            bool found = false;

            // if such target floor already has ??
            for (int i = 0; i < _liftCallsList.Count; i++)
            {
                if (_liftCallsList[i].floorNumber == targetFloor)
                {
                    //MessageBox.Show("already has");
                    found = true;
                    break;
                }
            }

            // if task already has then return
            if (found)
            {
                //MessageBox.Show("found");
                return;
            }

            double targetFloorPositionY = (_floorsTotal - targetFloor + 1) * FLOOR_HEIGHT - LIFT_HEIGHT - 1;

            // if current pos = target pos then DO NOTHING
            if (targetFloorPositionY == _currentLiftPositionY)
            {
                //MessageBox.Show("NOWHERE TO MOVE");
                return;
            }

            // add new task to tasks list
            _liftCallsList.Add(new CallItem
            {
                floorNumber = targetFloor,
                floorPositionY = targetFloorPositionY
            });


            // MAIN LIFT LOGIC
            SetClosestTarget();

            // run lift processing
            if (!_liftProcessing)
            {
                // move to target
                _backgroundWorkerCloseDoor.RunWorkerAsync(_liftCallsList[0].floorPositionY);
            }

            ShowTestData();
        }


        private void SetClosestTarget()
        {
            // initial minimal value
            double minValue = FLOOR_HEIGHT * _floorsTotal;

            int targetIndex = -1;

            for (int i = 0; i < _liftCallsList.Count; i++)
            {
                double newMin = Math.Abs(_currentLiftPositionY - _liftCallsList[i].floorPositionY);

                if (newMin < minValue)
                {
                    minValue = newMin; // set found min value

                    // find closest target
                    if (_liftState == LiftStateEnum.STOPPED)
                    {
                        targetIndex = i;
                    }
                    else if (_liftState == LiftStateEnum.MOVE_UP)
                    {
                        // if closest target upper then current position
                        if (_liftCallsList[i].floorPositionY < _currentLiftPositionY)
                        {
                            targetIndex = i;
                        }
                    }
                    else if (_liftState == LiftStateEnum.MOVE_DOWN)
                    {
                        // if closest target lower then current position
                        if (_liftCallsList[i].floorPositionY > _currentLiftPositionY)
                        {
                            targetIndex = i;
                        }
                    }
                }
            }

            // set target element to 0 position (swap elements)
            lock (_lockObject)
            {
                CallItem tempItem = _liftCallsList[0];
                _liftCallsList[0] = _liftCallsList[targetIndex];
                _liftCallsList[targetIndex] = tempItem;
            }


            ShowTestData();
        }


        private void ShowTestData()
        {
            testTextBox.Clear();

            for (int i = 0; i < _liftCallsList.Count; i++)
            {
                testTextBox.Text += "FLOOR = " + _liftCallsList[i].floorNumber.ToString()
                    + " TARGET Y = " + _liftCallsList[i].floorPositionY.ToString()
                    + "\n";
            }

            //Debug.WriteLine("_liftState = " + _liftState.ToString());
        }


        private double ConvertToDouble(string value)
        {
            double result = 0;

            if (double.TryParse(value, out result))
            {
                return result;
            }

            return -1; // error code (correct lift Y-position always > 0)
        }


        private int ConvertToInt(string value)
        {
            int result = 0;

            if (int.TryParse(value, out result))
            {
                return result;
            }

            return -1; // error code (correct floor number always > 0)
        }


        // lift control panel "START" button click
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_backgroundWorkerDoorWaitOpened.IsBusy)
            {
                _backgroundWorkerDoorWaitOpened.CancelAsync();
            }
        }
    }
}