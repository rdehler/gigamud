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
using System.Windows.Data;

namespace MMX.Infrastructure.Binding
{
    public class TextBoxBindingHelper
    {
        #region UpdateSourceOnChange

        public static readonly DependencyProperty UpdateSourceOnChange =
            DependencyProperty.RegisterAttached("UpdateSourceOnChange", typeof(bool), typeof(TextBoxBindingHelper), new PropertyMetadata(true, PropertyChangedCallback));

        static void PropertyChangedCallback(DependencyObject owner, DependencyPropertyChangedEventArgs e)
        {
            TextBox tbx = owner as TextBox;

            // validate the cast
            if (tbx != null)
            {
                // add/remove the handler as necessary
                if ((bool)e.NewValue)
                    tbx.TextChanged += new TextChangedEventHandler(TextChangedHandler);
                else
                    tbx.TextChanged -= new TextChangedEventHandler(TextChangedHandler);
            }
        }

        static void TextChangedHandler(object sender, TextChangedEventArgs e)
        {
            TextBox tbx = sender as TextBox;

            // validate the cast
            if (tbx != null)
            {
                BindingExpression expression = tbx.GetBindingExpression(TextBox.TextProperty);

                // if the text property is bound with a two way binding, update the source
                if (expression != null &&
                    expression.ParentBinding != null &&
                    expression.ParentBinding.Mode == BindingMode.TwoWay)
                    expression.UpdateSource();
            }
        }

        public static bool GetUpdateSourceOnChange(DependencyObject obj)
        {
            return (bool)obj.GetValue(UpdateSourceOnChange);
        }

        public static void SetUpdateSourceOnChange(DependencyObject obj, bool value)
        {
            obj.SetValue(UpdateSourceOnChange, value);
        }

        #endregion
    }
}
