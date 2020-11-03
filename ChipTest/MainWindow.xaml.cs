using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.IO.Ports;
using System.Threading;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Threading;

namespace ChipTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //参数声明
        SerialPort myPort = new SerialPort();//声明一个串口
        private bool myPortIsOpen = false;//COM口开启状态字
        public string com;
        List<byte> Receive_buf;         //存储从串口读取的数据
        Byte commandNum;                 //命令号
        int result = 0;
        int data1 = 0, data2=0;
        int state = 1;
        int re_length, re_comm;
        int data_l, data_h;
        string dataValue;
        int group = 1000;  //界面内显示的点数
        double ymax = 0, ymax1 = 0, ymin = 100000, ymin1 = 100000;
        double xaxis = 0;  //界面x坐标值起点
        int i = 0, flag = 0;
        long k = 0;
        List<int> al = new List<int>();
        List<int> alStore = new List<int>();

        private ObservableDataSource<Point> dataSource = new ObservableDataSource<Point>();
        private ObservableDataSource<Point> dataSource1 = new ObservableDataSource<Point>();
        private LineGraph graph = new LineGraph();
        private LineGraph graph1 = new LineGraph();
        private DispatcherTimer timer = new DispatcherTimer();

        Detection dete;


        public MainWindow()
        {
            InitializeComponent();
            myPortInitalize();            //串口初始化

            dataBinding();    //控件绑定
        }


        //------------------串口--------------------------
        private void myPortInitalize()
        {
            List<USBDeviceInfo> device = getCom.GetUSBDevices();
            if (device.Count == 0)
            {
                return;
            }
            com = getCom.GetComNumber(device[0].Name);

            myPort.BaudRate = 19200;
            myPort.DataBits = 8;
            myPort.PortName = com;
            myPort.Open();
            myPortIsOpen = true;
        }



        //-------------------命令发送-------------------------
        //阻抗检测
        private void impedance_Test(object sender, RoutedEventArgs e)   
        {
            try
            {
                //阻抗检测命令
                commandNum = 13;
                byte[] impedComm = new byte[14];
                impedComm[0] = 0x5A;                          //首字节
                impedComm[1] = 14;                           //命令数据长度
                impedComm[2] = commandNum;
                //和校验
                for (int i = 1; i < 12; i++)
                {
                    impedComm[12] += impedComm[i];
                }

                impedComm[13] = 0xA5;                        //尾字节

                //发送命令
                myPort.Write(impedComm, 0, impedComm.Length);
                Thread.Sleep(500);

                //接收命令
                byte[] Redata = new byte[myPort.BytesToRead];
                myPort.Read(Redata, 0, Redata.Length);

                //分析并显示数据
                impedanceData_Show(Redata);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }


        //单次刺激
        private void single_Stimulate(object sender, RoutedEventArgs e)      //发出单次刺激
        {
            try
            {
                //单次刺激命令
                commandNum = 9;
                byte[] sinStidata = new byte[14];
                sinStidata[0] = 0x5A;                          //首字节
                sinStidata[1] = 14;                           //命令数据长度
                sinStidata[2] = commandNum;
                //和校验
                for (int i = 1; i < 12; i++)
                {
                    sinStidata[12] += sinStidata[i];
                }

                sinStidata[13] = 0xA5;                        //尾字节

                //发送命令
                myPort.Write(sinStidata, 0, sinStidata.Length);
                Thread.Sleep(500);

                myPort.DataReceived += myPort_DataReceived;   //校验返回的结果
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }


        }

        private void channel_Select(object sender, RoutedEventArgs e)
        {
            //得到通道数据
            byte[] channel_Data = Get_Channeldata();

            //发送数据
            myPort.Write(channel_Data, 0, channel_Data.Length);
            Thread.Sleep(500);
            
            //校验
            myPort.DataReceived += myPort_DataReceived;   //校验返回的结果

        }

        //实时采样开始
        private void realTSample_Start(object sender, RoutedEventArgs e)      //发出单次刺激
        {
            //清空画布
            plotter.Children.Remove(graph);
            plotter1.Children.Remove(graph1);
            plotter.Legend.Remove();
            plotter1.Legend.Remove();

            //数据源与绘图数据关联
            graph = plotter.AddLineGraph(dataSource, Colors.Green, 2);
            graph1 = plotter.AddLineGraph(dataSource1, Colors.Green, 2);


            try
            {
                //实时采样命令开始
                commandNum = 12;
                byte[] realTdata = new byte[14];
                realTdata[0] = 0x5A;                          //首字节
                realTdata[1] = 14;                           //命令数据长度
                realTdata[2] = commandNum;
                realTdata[3] = 1;
                //和校验
                for (int i = 1; i < 12; i++)
                {
                    realTdata[12] += realTdata[i];
                }

                realTdata[13] = 0xA5;                        //尾字节

                //发送命令
                myPort.Write(realTdata, 0, realTdata.Length);
                Thread.Sleep(50);                                

                //接收数据
                timer.Tick += MyPort_sampleDataReceived;
                timer.Interval = TimeSpan.FromMilliseconds(5);   //隔5ms采样一次
                timer.IsEnabled = true;

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }


        }

        //实时采样停止
        private void realTSample_Stop(object sender, RoutedEventArgs e)
        {

            Thread.Sleep(10);
            if (myPort.IsOpen)
            {
                //实时采样命令开始
                commandNum = 12;
                byte[] realTdata = new byte[14];
                realTdata[0] = 0x5A;                          //首字节
                realTdata[1] = 14;                           //命令数据长度
                realTdata[2] = commandNum;
                realTdata[3] = 0;
                //和校验
                for (int i = 1; i < 12; i++)
                {
                    realTdata[12] += realTdata[i];
                }

                realTdata[13] = 0xA5;                        //尾字节

                myPort.Write(realTdata, 0, realTdata.Length);
                myPort.Close();

            }
            else
            {
                myPort.Close();
            }

            timer.Stop();

        }


        //-----------------------接收数据处理----------------------
        //接收数据并校验（同时清空Receive_buf）
        private void myPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {

                byte[] Redata = new byte[myPort.BytesToRead];
                myPort.Read(Redata, 0, Redata.Length);
                Receive_buf.AddRange(Redata);

                Received_Check(commandNum);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }


        }

        //接收信号校验命令号，弹框显示
        private void Received_Check(int status)
        {

            if (Receive_buf[0] == 0xA5 && Receive_buf[2] == status && Receive_buf[Receive_buf.Count-1] == 0x5A)
                {
                    Receive_buf.Clear();
                    MessageBox.Show("操作成功！");
                }
            
        }

        //接收实时采样数据
        private void MyPort_sampleDataReceived(object sender, EventArgs e)
        {
            if (!myPort.IsOpen)
            {
                return;
            }

            try
            {
                //读取44个字节
                byte[] Redata = new byte[44];
                myPort.Read(Redata, 0, 44);

                //提取数据，并赋给数据源
                Point point = new Point();
                Point point1 = new Point();
                for (i = 3; i < 22; i+=2)       //第3位到42位为采样数据,每次递增2
                {
                    data1 = (Redata[i] << 8) + Redata[i + 1];
                    data2 = (Redata[i+20] << 8) + Redata[i + 21];

                    point = new Point(k, data1);
                    point1 = new Point(k, data2);
                    dataSource.AppendAsync(base.Dispatcher, point);
                    dataSource1.AppendAsync(base.Dispatcher, point1);
                    k++;
                }


                if (k - group > 0)     //x轴移动
                {
                    xaxis = k - group;
                }
                else
                {
                    xaxis = 0;
                }

                //X轴动态显示，Y轴范围在全局变量定义中设置
                plotter.Viewport.Visible = new System.Windows.Rect(xaxis, ymin - 0.2 * Math.Abs(ymax - ymin), group, 1.4 * (ymax - ymin));
                plotter1.Viewport.Visible = new System.Windows.Rect(xaxis, ymin1 - 0.2 * Math.Abs(ymax1 - ymin1), group, 1.4 * (ymax1 - ymin1));

                //删除数据源中1000个以前的历史数据
                int m = dataSource.Collection.Count;

                if (m / 1000 > 0)
                {

                   for (int ii = (m - 1000); ii >= 0; ii--)
                   {
                       dataSource.Collection.RemoveAt(ii);
                       dataSource1.Collection.RemoveAt(ii);
                    }

                }
               
            }
            catch (Exception err1)
            {
                MessageBox.Show(err1.Message);
            }


        }



        //-----------------------------数据打包----------------------------

        //通道数据打包
        private byte[] Get_Channeldata()
        {
            commandNum = 23;
            byte[] channelData = new byte[14];

            channelData[0] = 0x5A;                          //首字节
            channelData[1] = 14;                           //命令数据长度
            channelData[2] = commandNum;
            channelData[3] = dete.DetectLHip1;
            channelData[4] = dete.DetectLHip2;
            channelData[5] = dete.DetectLHip3;
            channelData[6] = dete.DetectLHip4;
            channelData[7] = dete.DetectRHip1;
            channelData[8] = dete.DetectRHip2;
            channelData[9] = dete.DetectRHip3;
            channelData[10] = dete.DetectRHip4;
            channelData[11] = dete.DetectCan;

            //和校验
            for (int i = 1; i < 12; i++)
            {
                channelData[12] += channelData[i];
            }

            channelData[13] = 0xA5;                          //尾字节

            return channelData;
        }





        //------------------------界面显示---------------------------
        //在界面上显示阻抗检测的数值
        private void impedanceData_Show(byte[] impedanceReaddata)
        {
            if (impedanceReaddata.Length == 70 && impedanceReaddata[2] == 13)
            {
                //通道1
                impedance112.Text = ((impedanceReaddata[3] << 8) + impedanceReaddata[4]).ToString();   //触点1和触点2                    
                impedance113.Text = ((impedanceReaddata[5] << 8) + impedanceReaddata[6]).ToString();   //触点1和触点3
                impedance114.Text = ((impedanceReaddata[7] << 8) + impedanceReaddata[8]).ToString();   //触点1和触点4
                impedance115.Text = ((impedanceReaddata[9] << 8) + impedanceReaddata[10]).ToString();   //触点1和壳体
                impedance121.Text = ((impedanceReaddata[11] << 8) + impedanceReaddata[12]).ToString();   //触点2和触点1                    
                impedance123.Text = ((impedanceReaddata[13] << 8) + impedanceReaddata[14]).ToString();   //触点2和触点3
                impedance124.Text = ((impedanceReaddata[15] << 8) + impedanceReaddata[16]).ToString();   //触点2和触点4
                impedance125.Text = ((impedanceReaddata[17] << 8) + impedanceReaddata[18]).ToString();   //触点2和壳体
                impedance131.Text = ((impedanceReaddata[19] << 8) + impedanceReaddata[20]).ToString();   //触点3和触点1                    
                impedance132.Text = ((impedanceReaddata[21] << 8) + impedanceReaddata[22]).ToString();   //触点3和触点2
                impedance134.Text = ((impedanceReaddata[23] << 8) + impedanceReaddata[24]).ToString();   //触点3和触点4
                impedance135.Text = ((impedanceReaddata[25] << 8) + impedanceReaddata[26]).ToString();   //触点3和壳体
                impedance141.Text = ((impedanceReaddata[27] << 8) + impedanceReaddata[28]).ToString();   //触点4和触点1                    
                impedance142.Text = ((impedanceReaddata[29] << 8) + impedanceReaddata[30]).ToString();   //触点4和触点2
                impedance143.Text = ((impedanceReaddata[31] << 8) + impedanceReaddata[32]).ToString();   //触点4和触点3
                impedance145.Text = ((impedanceReaddata[33] << 8) + impedanceReaddata[34]).ToString();   //触点4和壳体

                //通道2
                impedance212.Text = ((impedanceReaddata[35] << 8) + impedanceReaddata[36]).ToString();   //触点1和触点2                    
                impedance213.Text = ((impedanceReaddata[37] << 8) + impedanceReaddata[38]).ToString();   //触点1和触点3
                impedance214.Text = ((impedanceReaddata[39] << 8) + impedanceReaddata[40]).ToString();   //触点1和触点4
                impedance215.Text = ((impedanceReaddata[41] << 8) + impedanceReaddata[42]).ToString();   //触点1和壳体
                impedance221.Text = ((impedanceReaddata[43] << 8) + impedanceReaddata[44]).ToString();   //触点2和触点1                    
                impedance223.Text = ((impedanceReaddata[45] << 8) + impedanceReaddata[46]).ToString();   //触点2和触点3
                impedance224.Text = ((impedanceReaddata[47] << 8) + impedanceReaddata[48]).ToString();   //触点2和触点4
                impedance225.Text = ((impedanceReaddata[49] << 8) + impedanceReaddata[50]).ToString();   //触点2和壳体
                impedance231.Text = ((impedanceReaddata[51] << 8) + impedanceReaddata[52]).ToString();   //触点3和触点1                    
                impedance232.Text = ((impedanceReaddata[53] << 8) + impedanceReaddata[54]).ToString();   //触点3和触点2
                impedance234.Text = ((impedanceReaddata[55] << 8) + impedanceReaddata[56]).ToString();   //触点3和触点4
                impedance235.Text = ((impedanceReaddata[57] << 8) + impedanceReaddata[58]).ToString();   //触点3和壳体
                impedance241.Text = ((impedanceReaddata[59] << 8) + impedanceReaddata[60]).ToString();   //触点4和触点1                    
                impedance242.Text = ((impedanceReaddata[61] << 8) + impedanceReaddata[62]).ToString();   //触点4和触点2
                impedance243.Text = ((impedanceReaddata[63] << 8) + impedanceReaddata[64]).ToString();   //触点4和触点3
                impedance245.Text = ((impedanceReaddata[65] << 8) + impedanceReaddata[66]).ToString();   //触点4和壳体

            }
            else
            {
                MessageBox.Show("接收的返回命令不正确。");
            }

        }

        //-----------------控件与数据绑定--------------------
        private void dataBinding()
        {
           
            //绑定检测参数
            dete = new Detection();
            this.cbDetectLHip1.SetBinding(ComboBox.SelectedIndexProperty, new Binding("DetectLHip1") { Source = dete });
            this.cbDetectLHip2.SetBinding(ComboBox.SelectedIndexProperty, new Binding("DetectLHip2") { Source = dete });
            this.cbDetectLHip3.SetBinding(ComboBox.SelectedIndexProperty, new Binding("DetectLHip3") { Source = dete });
            this.cbDetectLHip4.SetBinding(ComboBox.SelectedIndexProperty, new Binding("DetectLHip4") { Source = dete });
            this.cbDetectRHip1.SetBinding(ComboBox.SelectedIndexProperty, new Binding("DetectRHip1") { Source = dete });
            this.cbDetectRHip2.SetBinding(ComboBox.SelectedIndexProperty, new Binding("DetectRHip2") { Source = dete });
            this.cbDetectRHip3.SetBinding(ComboBox.SelectedIndexProperty, new Binding("DetectRHip3") { Source = dete });
            this.cbDetectRHip4.SetBinding(ComboBox.SelectedIndexProperty, new Binding("DetectRHip4") { Source = dete });

            this.cbDetectCan.SetBinding(ComboBox.SelectedIndexProperty, new Binding("DetectCan") { Source = dete });;


        }



    }
}
