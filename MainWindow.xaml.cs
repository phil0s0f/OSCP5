using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OSCP5
{
    public partial class MainWindow : Window
    {
        List<Process> FCFSProcessesList = new List<Process>();
        ObservableCollection<string> FCFSProcessesNameList = new ObservableCollection<string>();
        ObservableCollection<Grid> FCFSProcessesStateList = new ObservableCollection<Grid>();

        List<Process> RRProcessesList = new List<Process>();
        ObservableCollection<string> RRProcessesNameList = new ObservableCollection<string>();
        ObservableCollection<Grid> RRProcessesStateList = new ObservableCollection<Grid>();

        Thread ProcessThread;

        int AutoC = 0;

        public MainWindow()
        {
            InitializeComponent();
            FCFSProcessesListBox.ItemsSource = FCFSProcessesNameList;
            FCFSProcessesStateListBox.ItemsSource = FCFSProcessesStateList;

            RRProcessesListBox.ItemsSource = RRProcessesNameList;
            RRProcessesStateListBox.ItemsSource = RRProcessesStateList;

            ProcessThread = new Thread(new ThreadStart(ProcessThreadFunc));
            ProcessThread.IsBackground = true;
            ProcessThread.Start();
        }
        void ProcessThreadFunc()
        {
            const int QuantSize = 50;
            int CurrentQuant = 0;

            Rectangle currentProcessRectangle;
            ProgressBar ExecutionProcessProgressBar;

            while (true)//RR
            {
                Thread.Sleep(100);

                int ExecutionProcessIndex = RRProcessesList.FindIndex(item => item.GetState() == ProcessStates.execution);
                if (RRProcessesList.Count != 0 && ExecutionProcessIndex == -1)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            for (int i = 0; i < RRProcessesList.Count; i++)
                            {
                                currentProcessRectangle = RRProcessesStateList[i].Children[1] as Rectangle;
                                RRProcessesList[i].SetState(ProcessStates.waiting);
                                currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                            }

                            currentProcessRectangle = RRProcessesStateList[0].Children[1] as Rectangle;
                            RRProcessesList[0].SetState(ProcessStates.execution);
                            currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));

                            if (RRProcessesList.Count > 1)
                            {
                                currentProcessRectangle = RRProcessesStateList[1].Children[1] as Rectangle;
                                RRProcessesList[1].SetState(ProcessStates.readiness);
                                currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                            }
                        });

                    ExecutionProcessIndex = 0;
                }

                if (ExecutionProcessIndex != -1)
                {
                    CurrentQuant++;

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            RRProcessesList[ExecutionProcessIndex].SetCompleteLevel(RRProcessesList[ExecutionProcessIndex].GetCompleteLevel() + 1);
                            ExecutionProcessProgressBar = RRProcessesStateList[ExecutionProcessIndex].Children[2] as ProgressBar;
                            ExecutionProcessProgressBar.Value = RRProcessesList[ExecutionProcessIndex].GetCompleteLevel();

                            lbl_RR_CurrentProcessProperties.Content = "Name:" + RRProcessesList[ExecutionProcessIndex].GetName() + "\n" +
                                                                   "ID:" + RRProcessesList[ExecutionProcessIndex].GetID() + "\n" +
                                                                   "State:" + RRProcessesList[ExecutionProcessIndex].GetState() + "\n" +
                                                                   "Runtime:" + RRProcessesList[ExecutionProcessIndex].GetRuntime() + "\n" +
                                                                   "CompleteLevel:" + RRProcessesList[ExecutionProcessIndex].GetCompleteLevel() + "\n" +
                                                                   "Priority:" + RRProcessesList[ExecutionProcessIndex].GetPriority() + "\n";

                            if (RRProcessesListBox.SelectedIndex != -1)
                            {
                                lbl_RR_SelectedProcessProperties.Content = "Name:" + RRProcessesList[RRProcessesListBox.SelectedIndex].GetName() + "\n" +
                                                                        "ID:" + RRProcessesList[RRProcessesListBox.SelectedIndex].GetID() + "\n" +
                                                                        "State:" + RRProcessesList[RRProcessesListBox.SelectedIndex].GetState() + "\n" +
                                                                        "Runtime:" + RRProcessesList[RRProcessesListBox.SelectedIndex].GetRuntime() + "\n" +
                                                                        "CompleteLevel:" + RRProcessesList[RRProcessesListBox.SelectedIndex].GetCompleteLevel() + "\n" +
                                                                        "Priority:" + RRProcessesList[RRProcessesListBox.SelectedIndex].GetPriority() + "\n";
                            }

                            if (RRProcessesList[ExecutionProcessIndex].GetCompleteLevel() == RRProcessesList[ExecutionProcessIndex].GetRuntime())
                            {
                                currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex].Children[1] as Rectangle;
                                RRProcessesList[ExecutionProcessIndex].SetState(ProcessStates.finished_execution);
                                currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                                RRProcessesNameList.Remove(RRProcessesList[ExecutionProcessIndex].GetName());
                                RRProcessesStateList.RemoveAt(ExecutionProcessIndex);
                                RRProcessesList.Remove(RRProcessesList[ExecutionProcessIndex]);

                                if (RRProcessesList.Count != 0)
                                {
                                    ExecutionProcessIndex = RRProcessesList.FindIndex(item => item.GetState() == ProcessStates.readiness);

                                    currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex].Children[1] as Rectangle;
                                    RRProcessesList[ExecutionProcessIndex].SetState(ProcessStates.execution);
                                    currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));

                                    if (RRProcessesList.Count > 1)
                                    {
                                        if (ExecutionProcessIndex == RRProcessesList.Count - 1)
                                        {
                                            currentProcessRectangle = RRProcessesStateList[0].Children[1] as Rectangle;
                                            RRProcessesList[0].SetState(ProcessStates.readiness);
                                            currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                                        }
                                        else
                                        {
                                            currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex + 1].Children[1] as Rectangle;
                                            RRProcessesList[ExecutionProcessIndex + 1].SetState(ProcessStates.readiness);
                                            currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                                        }
                                    }
                                }
                            }



                            if (CurrentQuant == QuantSize)
                            {
                                CurrentQuant = 0;

                                if (RRProcessesList.Count == 2)
                                {
                                    if (ExecutionProcessIndex == RRProcessesList.Count - 1)
                                    {
                                        currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex].Children[1] as Rectangle;
                                        RRProcessesList[ExecutionProcessIndex].SetState(ProcessStates.readiness);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));

                                        currentProcessRectangle = RRProcessesStateList[0].Children[1] as Rectangle;
                                        RRProcessesList[0].SetState(ProcessStates.execution);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                                    }
                                    else
                                    {
                                        currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex].Children[1] as Rectangle;
                                        RRProcessesList[ExecutionProcessIndex].SetState(ProcessStates.readiness);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));

                                        currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex + 1].Children[1] as Rectangle;
                                        RRProcessesList[ExecutionProcessIndex + 1].SetState(ProcessStates.execution);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                                    }
                                }
                                else if (RRProcessesList.Count > 2)
                                {
                                    if (ExecutionProcessIndex == RRProcessesList.Count - 1)
                                    {
                                        currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex].Children[1] as Rectangle;
                                        RRProcessesList[ExecutionProcessIndex].SetState(ProcessStates.waiting);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));

                                        currentProcessRectangle = RRProcessesStateList[0].Children[1] as Rectangle;
                                        RRProcessesList[0].SetState(ProcessStates.execution);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));

                                        currentProcessRectangle = RRProcessesStateList[1].Children[1] as Rectangle;
                                        RRProcessesList[1].SetState(ProcessStates.readiness);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                                    }
                                    else if (ExecutionProcessIndex == RRProcessesList.Count - 2)
                                    {
                                        currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex].Children[1] as Rectangle;
                                        RRProcessesList[ExecutionProcessIndex].SetState(ProcessStates.waiting);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));

                                        currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex + 1].Children[1] as Rectangle;
                                        RRProcessesList[ExecutionProcessIndex + 1].SetState(ProcessStates.execution);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));

                                        currentProcessRectangle = RRProcessesStateList[0].Children[1] as Rectangle;
                                        RRProcessesList[0].SetState(ProcessStates.readiness);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                                    }
                                    else
                                    {
                                        currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex].Children[1] as Rectangle;
                                        RRProcessesList[ExecutionProcessIndex].SetState(ProcessStates.waiting);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0));

                                        currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex + 1].Children[1] as Rectangle;
                                        RRProcessesList[ExecutionProcessIndex + 1].SetState(ProcessStates.execution);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));

                                        currentProcessRectangle = RRProcessesStateList[ExecutionProcessIndex + 2].Children[1] as Rectangle;
                                        RRProcessesList[ExecutionProcessIndex + 2].SetState(ProcessStates.readiness);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                                    }
                                }
                            }
                        });
                }
                else
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            lbl_RR_CurrentProcessProperties.Content = "";
                            lbl_RR_SelectedProcessProperties.Content = "";
                        });
                }

               
                
                if(RRProcessesList.Count == 0) //FCFS
                {

                    Thread.Sleep(100);

                    ExecutionProcessIndex = FCFSProcessesList.FindIndex(item => item.GetState() == ProcessStates.execution);



                    if (ExecutionProcessIndex != -1)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                try
                                {
                                    FCFSProcessesList[ExecutionProcessIndex].SetCompleteLevel(FCFSProcessesList[ExecutionProcessIndex].GetCompleteLevel() + 1);

                                    ExecutionProcessProgressBar = FCFSProcessesStateList[ExecutionProcessIndex].Children[2] as ProgressBar;
                                    ExecutionProcessProgressBar.Value = FCFSProcessesList[ExecutionProcessIndex].GetCompleteLevel();
                                    if (FCFSProcessesList.Count > 1)
                                    {
                                        currentProcessRectangle = FCFSProcessesStateList[ExecutionProcessIndex + 1].Children[1] as Rectangle;
                                        FCFSProcessesList[ExecutionProcessIndex + 1].SetState(ProcessStates.readiness);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                                    }

                                    lbl_FCFS_CurrentProcessProperties.Content = "Name:" + FCFSProcessesList[ExecutionProcessIndex].GetName() + "\n" +
                                                                                   "ID:" + FCFSProcessesList[ExecutionProcessIndex].GetID() + "\n" +
                                                                                   "State:" + FCFSProcessesList[ExecutionProcessIndex].GetState() + "\n" +
                                                                                   "Runtime:" + FCFSProcessesList[ExecutionProcessIndex].GetRuntime() + "\n" +
                                                                                   "CompleteLevel:" + FCFSProcessesList[ExecutionProcessIndex].GetCompleteLevel() + "\n" +
                                                                                   "Priority:" + FCFSProcessesList[ExecutionProcessIndex].GetPriority() + "\n";
                                    if (FCFSProcessesListBox.SelectedIndex != -1)
                                    {
                                        lbl_FCFS_SelectedProcessProperties.Content = "Name:" + FCFSProcessesList[FCFSProcessesListBox.SelectedIndex].GetName() + "\n" +
                                                                                "ID:" + FCFSProcessesList[FCFSProcessesListBox.SelectedIndex].GetID() + "\n" +
                                                                                "State:" + FCFSProcessesList[FCFSProcessesListBox.SelectedIndex].GetState() + "\n" +
                                                                                "Runtime:" + FCFSProcessesList[FCFSProcessesListBox.SelectedIndex].GetRuntime() + "\n" +
                                                                                "CompleteLevel:" + FCFSProcessesList[FCFSProcessesListBox.SelectedIndex].GetCompleteLevel() + "\n" +
                                                                                "Priority:" + FCFSProcessesList[FCFSProcessesListBox.SelectedIndex].GetPriority() + "\n";
                                    }

                                    if (FCFSProcessesList[ExecutionProcessIndex].GetCompleteLevel() == FCFSProcessesList[ExecutionProcessIndex].GetRuntime())
                                    {

                                        currentProcessRectangle = FCFSProcessesStateList[ExecutionProcessIndex].Children[1] as Rectangle;
                                        FCFSProcessesList[ExecutionProcessIndex].SetState(ProcessStates.finished_execution);
                                        currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                                        FCFSProcessesNameList.Remove(FCFSProcessesList[ExecutionProcessIndex].GetName());
                                        FCFSProcessesStateList.RemoveAt(ExecutionProcessIndex);
                                        FCFSProcessesList.Remove(FCFSProcessesList[ExecutionProcessIndex]); //удалем выполненый процесс
                                    if (ExecutionProcessIndex < FCFSProcessesList.Count)
                                        {
                                        FCFSProcessesList[ExecutionProcessIndex].SetState(ProcessStates.readiness);
                                        }
                                        if (FCFSProcessesList.Count != 0)
                                        {
                                            ExecutionProcessIndex = FCFSProcessesList.FindIndex(item => item.GetState() == ProcessStates.readiness);

                                            currentProcessRectangle = FCFSProcessesStateList[ExecutionProcessIndex].Children[1] as Rectangle;
                                            FCFSProcessesList[ExecutionProcessIndex].SetState(ProcessStates.execution);
                                            currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));

                                            if (FCFSProcessesList.Count > 1)
                                            {
                                                currentProcessRectangle = FCFSProcessesStateList[ExecutionProcessIndex + 1].Children[1] as Rectangle;
                                                FCFSProcessesList[ExecutionProcessIndex + 1].SetState(ProcessStates.readiness);
                                                currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                Console.WriteLine("Error was detected!");
                                }
                            });
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                lbl_FCFS_CurrentProcessProperties.Content = "";
                                lbl_FCFS_SelectedProcessProperties.Content = "";
                            });
                    }
                }

            }


        }

        void ProcessesStateListCreateFCFS()
        {
            FCFSProcessesStateList.Add(new Grid());

            FCFSProcessesStateList.Last().Width = 565;
            FCFSProcessesStateList.Last().Height = 20;
            FCFSProcessesStateList.Last().HorizontalAlignment = HorizontalAlignment.Center;
            FCFSProcessesStateList.Last().VerticalAlignment = VerticalAlignment.Stretch;

            Label temp_label = new Label();
            temp_label.Width = 175;
            temp_label.Height = 20;
            temp_label.HorizontalAlignment = HorizontalAlignment.Left;
            temp_label.VerticalAlignment = VerticalAlignment.Center;
            temp_label.Padding = new Thickness(5, 0, 5, 0);
            temp_label.BorderThickness = new Thickness(1);
            temp_label.Content = FCFSProcessesList.Last().GetName();
            temp_label.BorderBrush = new SolidColorBrush(Color.FromRgb(188, 188, 188));

            Rectangle temp_rectangle = new Rectangle();
            temp_rectangle.Width = 20;
            temp_rectangle.Height = 20;
            temp_rectangle.HorizontalAlignment = HorizontalAlignment.Left;
            temp_rectangle.VerticalAlignment = VerticalAlignment.Center;
            temp_rectangle.Margin = new Thickness(180, 0, 0, 0);
            temp_rectangle.StrokeThickness = 1;
            temp_rectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            temp_rectangle.Stroke = new SolidColorBrush(Color.FromRgb(188, 188, 188));

            ProgressBar temp_progressBar = new ProgressBar();
            temp_progressBar.Width = 360;
            temp_progressBar.Height = 20;
            temp_progressBar.VerticalAlignment = VerticalAlignment.Center;
            temp_progressBar.Margin = new Thickness(205, 0, 0, 0);
            temp_progressBar.BorderBrush = new SolidColorBrush(Color.FromRgb(188, 188, 188));
            temp_progressBar.Maximum = FCFSProcessesList.Last().GetRuntime();

            FCFSProcessesStateList.Last().Children.Add(temp_label);
            FCFSProcessesStateList.Last().Children.Add(temp_rectangle);
            FCFSProcessesStateList.Last().Children.Add(temp_progressBar);
        }
        void ProcessesStateListCreateRR()
        {
            RRProcessesStateList.Add(new Grid());

            RRProcessesStateList.Last().Width = 565;
            RRProcessesStateList.Last().Height = 20;
            RRProcessesStateList.Last().HorizontalAlignment = HorizontalAlignment.Center;
            RRProcessesStateList.Last().VerticalAlignment = VerticalAlignment.Stretch;

            Label temp_label = new Label();
            temp_label.Width = 175;
            temp_label.Height = 20;
            temp_label.HorizontalAlignment = HorizontalAlignment.Left;
            temp_label.VerticalAlignment = VerticalAlignment.Center;
            temp_label.Padding = new Thickness(5, 0, 5, 0);
            temp_label.BorderThickness = new Thickness(1);
            temp_label.Content = RRProcessesList.Last().GetName();
            temp_label.BorderBrush = new SolidColorBrush(Color.FromRgb(188, 188, 188));

            Rectangle temp_rectangle = new Rectangle();
            temp_rectangle.Width = 20;
            temp_rectangle.Height = 20;
            temp_rectangle.HorizontalAlignment = HorizontalAlignment.Left;
            temp_rectangle.VerticalAlignment = VerticalAlignment.Center;
            temp_rectangle.Margin = new Thickness(180, 0, 0, 0);
            temp_rectangle.StrokeThickness = 1;
            temp_rectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            temp_rectangle.Stroke = new SolidColorBrush(Color.FromRgb(188, 188, 188));

            ProgressBar temp_progressBar = new ProgressBar();
            temp_progressBar.Width = 360;
            temp_progressBar.Height = 20;
            temp_progressBar.VerticalAlignment = VerticalAlignment.Center;
            temp_progressBar.Margin = new Thickness(205, 0, 0, 0);
            temp_progressBar.BorderBrush = new SolidColorBrush(Color.FromRgb(188, 188, 188));
            temp_progressBar.Maximum = RRProcessesList.Last().GetRuntime();

            RRProcessesStateList.Last().Children.Add(temp_label);
            RRProcessesStateList.Last().Children.Add(temp_rectangle);
            RRProcessesStateList.Last().Children.Add(temp_progressBar);
        }

        void CreateNewProcess(string processName)
        {
            Rectangle currentProcessRectangle;

            Random random = new Random();

            int Priority = random.Next(0, 5); //0,1,2,3,4,5  Priority < 3 - низкий

            if (Priority < 3)
            {
                FCFSProcessesList.Add(new Process(processName, FCFSProcessesList, Priority));
                FCFSProcessesNameList.Add(FCFSProcessesList.Last().GetName());
                ProcessesStateListCreateFCFS();
            }
            else
            {
                RRProcessesList.Add(new Process(processName, RRProcessesList, Priority));
                RRProcessesNameList.Add(RRProcessesList.Last().GetName());
                ProcessesStateListCreateRR();
            }

            if (FCFSProcessesNameList.Count == 1)
            {
                FCFSProcessesListBox.SelectedIndex = 0;
                currentProcessRectangle = FCFSProcessesStateList.Last().Children[1] as Rectangle;
                FCFSProcessesList.First().SetState(ProcessStates.execution);
                currentProcessRectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));
            }
            else if (FCFSProcessesNameList.Count > 1)
            {
                FCFSProcessesListBox.SelectedIndex = FCFSProcessesNameList.Count - 1;
            }
        }

        private void btm_CreateProcess_Click(object sender, RoutedEventArgs e)
        {
            if (tb_NewProcessName.Text != "")
            {
                CreateNewProcess(tb_NewProcessName.Text.Replace(" ", "_"));
                tb_NewProcessName.Text = "";
            }
        }

        private void btm_CreateSomeProcess_Click(object sender, RoutedEventArgs e)
        {
            for (int j = 0; j < 10; j++, AutoC++)
                CreateNewProcess("Auto " + AutoC);
        }
    }
}