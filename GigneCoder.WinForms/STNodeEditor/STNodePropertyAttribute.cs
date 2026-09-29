using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ST.Library.UI.NodeEditor
{
    /// <summary>
    /// STNode node attribute characteristics
    /// Used to describe STNode node attribute information and behavior on the attribute editor
    /// </summary>
    public class STNodePropertyAttribute : Attribute
    {
        private string _Name;
        /// <summary>
        /// Get the name of the property that needs to be displayed on the property editor
        /// </summary>
        public string Name {
            get { return _Name; }
        }

        private string _Description;
        /// <summary>
        /// Get the description of the property that needs to be displayed on the property editor
        /// </summary>
        public string Description {
            get { return _Description; }
        }

        private Type _ConverterType = typeof(STNodePropertyDescriptor);
        /// <summary>
        /// Get the property descriptor type
        /// </summary>
        public Type DescriptorType {
            get { return _ConverterType; }
            set { _ConverterType = value; }
        }

        /// <summary>
        /// Constructs an STNode property attribute
        /// </summary>
        /// <param name="strKey">Name to be displayed</param>
        /// <param name="strDesc">Description information to be displayed</param>
        public STNodePropertyAttribute(string strKey, string strDesc) {
            this._Name = strKey;
            this._Description = strDesc;
        }
        //private Type m_descriptor_type_base = typeof(STNodePropertyDescriptor);
    }
    /// <summary>
    /// STNode property descriptor
    /// Used to determine how to interact with the property's value on the property editor and to determine how the property's value will be drawn and interacted on the property editor
    /// </summary>
    public class STNodePropertyDescriptor
    {
        /// <summary>
        /// Get the target node
        /// </summary>
        public STNode Node { get; internal set; }
        /// <summary>
        /// Get the node attribute editor control to which it belongs
        /// </summary>
        public STNodePropertyGrid Control { get; internal set; }
        /// <summary>
        /// Get the area where the option is located
        /// </summary>
        public Rectangle Rectangle { get; internal set; }
        /// <summary>
        /// Get the area where the option name is located
        /// </summary>
        public Rectangle RectangleL { get; internal set; }
        /// <summary>
        /// Get the area where the option value is located
        /// </summary>
        public Rectangle RectangleR { get; internal set; }
        /// <summary>
        /// Get the name of the option that needs to be displayed
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Get the description information corresponding to the attribute
        /// </summary>
        public string Description { get; internal set; }
        /// <summary>
        /// Get attribute information
        /// </summary>
        public PropertyInfo PropertyInfo { get; internal set; }

        private static Type m_t_int = typeof(int);
        private static Type m_t_float = typeof(float);
        private static Type m_t_double = typeof(double);
        private static Type m_t_string = typeof(string);
        private static Type m_t_bool = typeof(bool);

        private StringFormat m_sf;

        /// <summary>
        /// Construct a descriptor
        /// </summary>
        public STNodePropertyDescriptor() {
            m_sf = new StringFormat();
            m_sf.LineAlignment = StringAlignment.Center;
            m_sf.FormatFlags = StringFormatFlags.NoWrap;
        }

        /// <summary>
        /// Occurs when determining the position of the STNode property on the property editor
        /// </summary>
        protected internal virtual void OnSetItemLocation() { }
        /// <summary>
        /// Converts the property value in the form of a string to the value of the property target type
        /// By default, only int float double string bool and Array of the above types are supported
        /// If the target type is not in the above, please rewrite this function to convert it yourself
        /// </summary>
        /// <param name="strText">Attribute value in string form</param>
        /// <returns>The value of the real target type of the attribute</returns>
        protected internal virtual object GetValueFromString(string strText) {
            Type t = this.PropertyInfo.PropertyType;
            if (t == m_t_int) return int.Parse(strText);
            if (t == m_t_float) return float.Parse(strText);
            if (t == m_t_double) return double.Parse(strText);
            if (t == m_t_string) return strText;
            if (t == m_t_bool) return bool.Parse(strText);
            if (t.IsEnum) {
                return Enum.Parse(t, strText);
            } else if (t.IsArray) {
                var t_1 = t.GetElementType();
                if (t_1 == m_t_string) return strText.Split(',');
                string[] strs = strText.Trim(new char[] { ' ', ',' }).Split(',');//add other place trim()
                if (t_1 == m_t_int) {
                    int[] arr = new int[strs.Length];
                    for (int i = 0; i < strs.Length; i++) arr[i] = int.Parse(strs[i].Trim());
                    return arr;
                }
                if (t_1 == m_t_float) {
                    float[] arr = new float[strs.Length];
                    for (int i = 0; i < strs.Length; i++) arr[i] = float.Parse(strs[i].Trim());
                    return arr;
                }
                if (t_1 == m_t_int) {
                    double[] arr = new double[strs.Length];
                    for (int i = 0; i < strs.Length; i++) arr[i] = double.Parse(strs[i].Trim());
                    return arr;
                }
                if (t_1 == m_t_int) {
                    bool[] arr = new bool[strs.Length];
                    for (int i = 0; i < strs.Length; i++) arr[i] = bool.Parse(strs[i].Trim());
                    return arr;
                }
            }
            throw new InvalidCastException("Unable to complete the conversion of [string] to [" + t.FullName + "], please overload [STNodePropertyDescriptor.GetValueFromString(string)]");
        }
        /// <summary>
        /// Converts the value of the attribute target type to a value in the form of a string
        /// ToString() operation on type value by default
        /// If you need special processing, please rewrite this function to convert it yourself
        /// </summary>
        /// <returns>String form of attribute value</returns>
        protected internal virtual string GetStringFromValue() {
            var v = this.PropertyInfo.GetValue(this.Node, null);
            var t = this.PropertyInfo.PropertyType;
            if (v == null) return null;
            if (t.IsArray) {
                List<string> lst = new List<string>();
                foreach (var item in (Array)v) lst.Add(item.ToString());
                return string.Join(",", lst.ToArray());
            }
            return v.ToString();
        }
        /// <summary>
        /// Convert the attribute value in binary form to the value of the attribute target type for restoring the attribute value from the data in the file store
        /// Convert it to a string by default and then call GetValueFromString(string)
        /// This function corresponds to GetBytesFromValue(). If the function needs to be rewritten, the two functions should be rewritten together
        /// </summary>
        /// <param name="byData">Binary data</param>
        /// <returns>The value of the real target type of the attribute</returns>
        protected internal virtual object GetValueFromBytes(byte[] byData) {
            if (byData == null) return null;
            string strText = Encoding.UTF8.GetString(byData);
            return this.GetValueFromString(strText);
        }
        /// <summary>
        /// Called when converting the value of the attribute target type to a binary value for file storage
        /// Calls GetStringFromValue() by default and converts the string to binary data
        /// For special handling, please rewrite this function to convert it yourself and rewrite GetValueFromBytes()
        /// </summary>
        /// <returns>Binary form of attribute value</returns>
        protected internal virtual byte[] GetBytesFromValue() {
            string strText = this.GetStringFromValue();
            if (strText == null) return null;
            return Encoding.UTF8.GetBytes(strText);
        }
        /// <summary>
        /// This function corresponds to System.Reflection.PropertyInfo.GetValue()
        /// </summary>
        /// <param name="index">The optional index value of the indexed attribute should be null for non-indexed attributes</param>
        /// <returns>Attribute value</returns>
        protected internal virtual object GetValue(object[] index) {
            return this.PropertyInfo.GetValue(this.Node, index);
        }
        /// <summary>
        /// This function corresponds to System.Reflection.PropertyInfo.SetValue()
        /// </summary>
        /// <param name="value">Attribute value to be set</param>
        protected internal virtual void SetValue(object value) {
            this.PropertyInfo.SetValue(this.Node, value, null);
        }
        /// <summary>
        /// This function corresponds to System.Reflection.PropertyInfo.SetValue()
        /// GetValueFromString(strValue) will be processed by default before calling
        /// </summary>
        /// <param name="strValue">The value of the attribute string that needs to be set</param>
        protected internal virtual void SetValue(string strValue) {
            this.PropertyInfo.SetValue(this.Node, this.GetValueFromString(strValue), null);
        }
        /// <summary>
        /// This function corresponds to System.Reflection.PropertyInfo.SetValue()
        /// GetValueFromBytes(byte[]) will be processed by default before calling
        /// </summary>
        /// <param name="byData">Attribute binary data to be set</param>
        protected internal virtual void SetValue(byte[] byData) {
            this.PropertyInfo.SetValue(this.Node, this.GetValueFromBytes(byData), null);
        }
        /// <summary>
        /// This function corresponds to System.Reflection.PropertyInfo.SetValue()
        /// </summary>
        /// <param name="value">Attribute value to be set</param>
        /// <param name="index">The optional index value of the indexed attribute should be null for non-indexed attributes</param>
        protected internal virtual void SetValue(object value, object[] index) {
            this.PropertyInfo.SetValue(this.Node, value, index);
        }
        /// <summary>
        /// This function corresponds to System.Reflection.PropertyInfo.SetValue()
        /// GetValueFromString(strValue) will be processed by default before calling
        /// </summary>
        /// <param name="strValue">The value of the attribute string that needs to be set</param>
        /// <param name="index">The optional index value of the indexed attribute should be null for non-indexed attributes</param>
        protected internal virtual void SetValue(string strValue, object[] index) {
            this.PropertyInfo.SetValue(this.Node, this.GetValueFromString(strValue), index);
        }
        /// <summary>
        /// This function corresponds to System.Reflection.PropertyInfo.SetValue()
        /// GetValueFromBytes(byte[]) will be processed by default before calling
        /// </summary>
        /// <param name="byData">Attribute binary data to be set</param>
        /// <param name="index">The optional index value of the indexed attribute should be null for non-indexed attributes</param>
        protected internal virtual void SetValue(byte[] byData, object[] index) {
            this.PropertyInfo.SetValue(this.Node, this.GetValueFromBytes(byData), index);
        }
        /// <summary>
        /// Occurs when there is an error setting the property value
        /// </summary>
        /// <param name="ex">Exception information</param>
        protected internal virtual void OnSetValueError(Exception ex) {
            this.Control.SetErrorMessage(ex.Message);
        }
        /// <summary>
        /// Occurs when drawing the property's value area on the property editor
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        protected internal virtual void OnDrawValueRectangle(DrawingTools dt) {
            Graphics g = dt.Graphics;
            SolidBrush brush = dt.SolidBrush;
            STNodePropertyGrid ctrl = this.Control;
            //STNodePropertyItem item = this._PropertyItem;
            brush.Color = ctrl.ItemValueBackColor;

            g.FillRectangle(brush, this.RectangleR);
            Rectangle rect = this.RectangleR;
            rect.Width--; rect.Height--;
            brush.Color = this.Control.ForeColor;
            g.DrawString(this.GetStringFromValue(), ctrl.Font, brush, this.RectangleR, m_sf);

            if (this.PropertyInfo.PropertyType.IsEnum || this.PropertyInfo.PropertyType == m_t_bool) {
                g.FillPolygon(Brushes.Gray, new Point[]{
                        new Point(rect.Right - 13, rect.Top + rect.Height / 2 - 2),
                        new Point(rect.Right - 4, rect.Top + rect.Height / 2 - 2),
                        new Point(rect.Right - 9, rect.Top + rect.Height / 2 + 3)
                    });
            }
        }
        /// <summary>
        /// Occurs when the mouse enters the area where the property value is located
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected internal virtual void OnMouseEnter(EventArgs e) { }
        /// <summary>
        /// Occurs when the mouse is clicked in the area where the property value is located
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected internal virtual void OnMouseDown(MouseEventArgs e) {
        }
        /// <summary>
        /// Occurs when the mouse moves in the area where the property value is located
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected internal virtual void OnMouseMove(MouseEventArgs e) { }
        /// <summary>
        /// Occurs when the mouse is raised in the area where the property value is located
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected internal virtual void OnMouseUp(MouseEventArgs e) { }
        /// <summary>
        /// Occurs when the mouse leaves the area where the property value is located
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected internal virtual void OnMouseLeave(EventArgs e) { }
        /// <summary>
        /// Occurs when the mouse is clicked in the area where the property value is located
        /// </summary>
        /// <param name="e">Event parameters</param>
        protected internal virtual void OnMouseClick(MouseEventArgs e) {
            Type t = this.PropertyInfo.PropertyType;
            if (t == m_t_bool || t.IsEnum) {
                new FrmSTNodePropertySelect(this).Show(this.Control);
                return;
            }
            Rectangle rect = this.Control.RectangleToScreen(this.RectangleR);
            new FrmSTNodePropertyInput(this).Show(this.Control);
        }
        /// <summary>
        /// Redraw the options area
        /// </summary>
        public void Invalidate() {
            Rectangle rect = this.Rectangle;
            rect.X -= this.Control.ScrollOffset;
            this.Control.Invalidate(rect);
        }
    }
}