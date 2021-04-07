using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace CoAEditor.VML {
    public class ViewModelLocator {
        public static bool GetAutoHookedUpViewModel(DependencyObject obj) {
            return (bool)obj.GetValue(AutoHookedUpViewModelProperty);
        }

        public static void SetAutoHookedUpViewModel(DependencyObject obj, bool value) {
            obj.SetValue(AutoHookedUpViewModelProperty, value);
        }

        public static readonly DependencyProperty AutoHookedUpViewModelProperty = DependencyProperty.RegisterAttached("AutoHookedUpViewModel", typeof(bool), typeof(ViewModelLocator), new PropertyMetadata(false, AutoHookedUpViewModelChanged));

        private static void AutoHookedUpViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (DesignerProperties.GetIsInDesignMode(d)) 
                return;

            var viewType = d.GetType();

            string str = viewType.FullName;
            str = str.Replace(".Views.", ".ViewModels.");

            var viewTypeName = str;
            var viewModelTypeName = viewTypeName + "Model";
            var viewModelType = Type.GetType(viewModelTypeName);
            var viewModel = Activator.CreateInstance(viewModelType);

            ((FrameworkElement)d).DataContext = viewModel;
        }
    }
}
