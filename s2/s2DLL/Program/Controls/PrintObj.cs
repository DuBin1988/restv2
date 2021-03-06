﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Printing;
using Com.Aote.ObjectTools;
using System.Windows.Browser;
using System.ComponentModel;
using System.Windows.Navigation;

namespace Com.Aote.Controls
{
    /**
     * 单页打印对象
     */
    public class PrintObj : FrameworkElement, IAsyncObject
    {
        //打印区域
        public UIElement Area { set; get; }

        #region UseDefaultPrinter 是否选择默认打印机
        private bool userDefaultPrinter = false;
        public bool UseDefaultPrinter
        {
            get
            {
                return this.userDefaultPrinter;
            }
            set
            {
                if (this.userDefaultPrinter == value)
                {
                    return;
                }
                this.userDefaultPrinter = value;
            }
        }
        #endregion

      

        #region State 打印状态
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(State), typeof(PrintObj), null);

        public State State
        {
            get { return (State)GetValue(StateProperty); }
            set
            {
                SetValue(StateProperty, value);
            }
        }
        #endregion

        #region Message 打印提示信息
        private string message = "是否打印？";
        public string Message
        {
            get { return message; }
            set
            {
                if (message == value)
                {
                    return;
                }
                message = value;
            }
        }
        #endregion



        #region Printing 开始打印事件，在打印前触发
        /// <summary>
        /// 开始保存事件，在开始保存数据时触发
        /// </summary>
        public event EventHandler Printing;
        public void OnPrinting()
        {
            if (Printing != null)
            {
                Printing(this, null);
            }
        }
        #endregion

        #region IsPrint 是否执行打印
        public static readonly DependencyProperty IsPrintProperty =
            DependencyProperty.Register("IsPrint", typeof(bool), typeof(PrintObj),
            new PropertyMetadata(new PropertyChangedCallback(OnIsPrintChanged)));

        private static void OnIsPrintChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            PrintObj go = (PrintObj)dp;
            if (go.IsPrint)
            {
                go.TipPrint();
            }
            go.IsPrint = false;
        }

 
        public bool IsPrint
        {
            get { return (bool)GetValue(IsPrintProperty); }
            set { SetValue(IsPrintProperty, value); }
        }
        #endregion

        #region IsDirect 是否直接打印
        private bool isDirect = false;

        public bool IsDirect
        {
            get { return isDirect; }
            set
            {
                if (isDirect != value)
                {
                    isDirect = value;
                }
            }
        }
        #endregion


        #region MesswindowName 自定义弹出界面路径
        private string messWindowName = "";
        public string MessWindowName
        {
            get
            {
                return this.messWindowName;
            }
            set
            {
                if (this.messWindowName == value)
                {
                    return;
                }
                this.messWindowName = value;
            }
        }
        #endregion

      
        #region MesswindowName 自定义弹出界面路径
        private object paramObj = "";
        public object ParamObj
        {
            get
            {
                return this.paramObj;
            }
            set
            {
                if (this.paramObj == value)
                {
                    return;
                }
                this.paramObj = value;
            }
        }
        #endregion


        //提示方法
        public void TipPrint()
        {
            State = State.Start;
            //自定义界面,加载自定义界面
            if (this.messWindowName != null && !this.messWindowName.Equals(""))
            {
                this.CustomPagePrint();
            }
            //默认提示打印
            else
            {
                ShowMessage tipPage = new ShowMessage();
                ((ShowMessage)tipPage).Message = this.Message;
                //注册关闭事件获取返回值
                tipPage.Closed += (o, e) =>
                {
                    ShowMessage cw = (ShowMessage)o;
                    if (cw.ReturnValue != null && (bool)cw.ReturnValue)
                    {
                        Print();
                    }
                    else
                    {
                        State = State.Cancle;
                        AsyncCompletedEventArgs args1 = new AsyncCompletedEventArgs(null, true, State.Cancle);
                        OnCompleted(args1);
                    }
                };
                tipPage.Show();
            }
        }

        /// <summary>
        /// 提示方法，提示完发送通知，手动调用打印
        /// </summary>
        public void TipNoPrint()
        {
            State = State.Start;
            //自定义界面,加载自定义界面
            if (this.messWindowName != null && !this.messWindowName.Equals(""))
            {
                this.CustomPagePrint();
            }
            //默认提示打印
            else
            {
                ShowMessage tipPage = new ShowMessage();
                ((ShowMessage)tipPage).Message = this.Message;
                //注册关闭事件获取返回值
                tipPage.Closed += (o, e) =>
                {
                    ShowMessage cw = (ShowMessage)o;
                    if (cw.ReturnValue != null && (bool)cw.ReturnValue)
                    {
                        AsyncCompletedEventArgs args1 = new AsyncCompletedEventArgs(null, true, State.Start);
                        OnTipCompleted(args1);
                    }
                    else
                    {
                        State = State.Cancle;
                        AsyncCompletedEventArgs args1 = new AsyncCompletedEventArgs(null, true, State.Cancle);
                        OnCompleted(args1);
                    }
                };
                tipPage.Show();
            }
        }

        //自定义界面打印
        private void CustomPagePrint()
        {
            CustomChildWindow tipPage = null;
            PageResourceContentLoader load = new PageResourceContentLoader();
            load.BeginLoad(new Uri(messWindowName + ".xaml", UriKind.Relative), null, new AsyncCallback(r =>
            {
                LoadResult ui = load.EndLoad(r);
                tipPage = (CustomChildWindow)ui.LoadedContent;
                if (this.ParamObj != null)
                {
                    tipPage.ParamValue = this.ParamObj;
                    //注册关闭事件获取返回值
                    tipPage.Closed += (o, e) =>
                    {
                        CustomChildWindow cw = (CustomChildWindow)o;
                        if (cw.ReturnValue != null && (bool)cw.ReturnValue)
                        {
                            Print();
                        }
                        else
                        {
                            State = State.Cancle;
                            AsyncCompletedEventArgs args1 = new AsyncCompletedEventArgs(null, true, State.Cancle);
                            OnCompleted(args1);
                        }
                    };
                    tipPage.Show();
                }
            }), 1);
        }

        //打印方法
        public void Print()
        {
            //触发开始打印事件
            OnPrinting();
            PrintDocument pd = new PrintDocument();
            //pd.BeginPrint += (o, e) =>
            //{
            //    State = State.Start;
            //};
            pd.PrintPage += (o, e) =>
            {
                e.PageVisual = Area;
            };
            pd.EndPrint += (o, e) =>
            {
                State = State.End;
                AsyncCompletedEventArgs args1 = new AsyncCompletedEventArgs(null, true, State.End);
                OnCompleted(args1);
            };
            //默认打印机
            if (userDefaultPrinter || isDirect)
            {
                pd.Print("", null, userDefaultPrinter);
            }
            else
            {
                pd.Print("");
            }
        }

        /// <summary>
        /// 是否正忙于工作
        /// </summary>
        public bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        #region Error 单条错误信息
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(string), typeof(FileLoad),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion

        public event AsyncCompletedEventHandler TipCompleted;

        public void OnTipCompleted(AsyncCompletedEventArgs args)
        {
            if (TipCompleted != null)
            {
                TipCompleted(this, args);
            }
        }

        public event AsyncCompletedEventHandler Completed;

        public void OnCompleted(AsyncCompletedEventArgs args)
        {
            if (Completed != null)
            {
                Completed(this, args);
            }
        }

        #region PropertyChanged事件
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}
