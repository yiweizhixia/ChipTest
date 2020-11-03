using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChipTest
{
    class Detection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //检测通道1
        private byte detectLHip1;              //电极1触点1的值
        public byte DetectLHip1
        {
            get { return detectLHip1; }
            set
            {
                detectLHip1 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectLHip1"));
                }
            }
        }

        private byte detectLHip2;              //电极1触点2的值
        public byte DetectLHip2
        {
            get { return detectLHip2; }
            set
            {
                detectLHip2 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectLHip2"));
                }
            }
        }

        private byte detectLHip3;              //电极1触点3的值
        public byte DetectLHip3
        {
            get { return detectLHip3; }
            set
            {
                detectLHip3 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectLHip3"));
                }
            }
        }

        private byte detectLHip4;              //电极1触点4的值
        public byte DetectLHip4
        {
            get { return detectLHip4; }
            set
            {
                detectLHip4 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectLHip4"));
                }
            }
        }

        //检测通道2
        private byte detectRHip1;              //电极2触点1的值
        public byte DetectRHip1
        {
            get { return detectRHip1; }
            set
            {
                detectRHip1 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectRHip1"));
                }
            }
        }

        private byte detectRHip2;              //电极2触点2的值
        public byte DetectRHip2
        {
            get { return detectRHip2; }
            set
            {
                detectRHip2 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectRHip2"));
                }
            }
        }

        private byte detectRHip3;              //电极2触点3的值
        public byte DetectRHip3
        {
            get { return detectRHip3; }
            set
            {
                detectRHip3 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectRHip3"));
                }
            }
        }

        private byte detectRHip4;              //电极2触点4的值
        public byte DetectRHip4
        {
            get { return detectRHip4; }
            set
            {
                detectRHip4 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectRHip4"));
                }
            }
        }


        private byte detectCan;              //壳体
        public byte DetectCan
        {
            get { return detectCan; }
            set
            {
                detectCan = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectCan"));
                }
            }
        }

    }
}
