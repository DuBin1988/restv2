using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.ComponentModel;

namespace Com.Aote.ObjectTools
{
    public class ParamObjectList : BaseObjectList
    {
        #region ParamName 参数名称
        public string paramName = null;
        public string ParamName
        {
            get { return paramName; }
            set
            {
                paramName = value;
            }
        }
        #endregion

        public override void Load()
        {
            
        }

        #region Count 总共数据个数
        override public int Count
        {
            get { return objects.Count; }
            set { throw new NotImplementedException(); }
        }
        #endregion

        
    }
}
