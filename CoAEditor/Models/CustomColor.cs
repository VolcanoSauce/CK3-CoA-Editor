using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using System.ComponentModel;

namespace CoAEditor.Models {
    public class CustomColor : INotifyPropertyChanged {
        private byte a;
        private byte r;
        private byte g;
        private byte b;

        private float h;
        private float s;
        private float l;

        public event PropertyChangedEventHandler PropertyChanged;

        public CustomColor() {

        }

        public CustomColor(byte r, byte g, byte b, byte a = 255) {
            SetFromRGBColor(r, g, b, a);
        }

        public CustomColor(float h, float s, float l) {
            SetFromHSLColor(h, s, l);
        }

        public CustomColor(Color color) {
            SetFromDrawingColor(color);
        }

        public byte R {
            get { return r; }
            set {
                r = value;
                OnPropertyChanged();
            }
        }

        public byte G {
            get { return g; }
            set {
                g = value;
                OnPropertyChanged();
            }
        }

        public byte B {
            get { return b; }
            set {
                b = value;
                OnPropertyChanged();
            }
        }

        public byte A {
            get { return a; }
            set { 
                a = value;
                OnPropertyChanged();
            }
        }

        public float H {
            get { return h; }
            set {
                h = value;
                OnPropertyChanged();
            }
        }

        public float S {
            get { return s; }
            set {
                s = value;
                OnPropertyChanged();
            }
        }

        public float L { 
            get { return l; }
            set {
                l = value;
                OnPropertyChanged();
            }
        }

        public string FormattedStringColor {
            get { return "( R: " + R + " G: " + G + " B: " + B + " ) ( H: " + H + " S: " + S + " L: " + L + " )"; }
        }

        public void SetFromDrawingColor(Color color) {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;

            h = s = l = 0;
            UpdateHSL();
        }

        public void SetFromRGBColor(byte r, byte g, byte b, byte a = 255) {
            R = r;
            G = g;
            B = b;
            A = a;

            h = s = l = 0;
            UpdateHSL();
        }

        public void SetFromHSLColor(float h, float s, float l) {
            H = h;
            S = s;
            L = l;

            r = g = b = a = 0;
            UpdateRGB();
        }

        public void SetFromCustomColor(CustomColor customColor) {
            SetFromRGBColor(customColor.R, customColor.G, customColor.B, customColor.A);
        }

        public Color DrawingColor {
            get { return Color.FromArgb(A, R, G, B); }
        }

        public bool Equals(CustomColor customColor) {
            return (R == customColor.R) && (G == customColor.G) && (B == customColor.B) && (A == customColor.A) && (H == customColor.H) && (S == customColor.S) && (L == customColor.L);
        }

        public bool CheckColorRepresentationsEquality() {
            return Color.FromArgb(a, r, g, b).Equals(HSLToRGB(h, s, l));
        }

        private void UpdateRGB() {
            if(!CheckColorRepresentationsEquality()) {
                Color tmpColor = HSLToRGB(h, s, l);
                R = tmpColor.R;
                G = tmpColor.G;
                B = tmpColor.B;
                A = tmpColor.A;

                OnPropertyChanged("GetDrawingColor");
                OnPropertyChanged("FormattedStringColor");
            }
        }

        private void UpdateHSL() {
            if (!CheckColorRepresentationsEquality()) {
                Color tmpColor = Color.FromArgb(a, r, g, b);
                H = tmpColor.GetHue();
                S = tmpColor.GetSaturation();
                L = tmpColor.GetBrightness();

                OnPropertyChanged("GetDrawingColor");
                OnPropertyChanged("FormattedStringColor");
            }
        }

        //https://www.programmingalgorithms.com/algorithm/hsl-to-rgb/
        public static Color HSLToRGB(float hue, float sat, float li) {
            byte r, g, b;

            if (sat == 0) {
                r = g = b = (byte)(li * 255);
            } else {
                float v1, v2;
                float h = (float)hue / 360;

                v2 = (li < 0.5) ? (li * (1 + sat)) : (li + sat) - (li * sat);
                v1 = 2 * li - v2;

                r = (byte)(255 * HueToRGB(v1, v2, h + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(v1, v2, h));
                b = (byte)(255 * HueToRGB(v1, v2, h - (1.0f / 3)));
            }

            return Color.FromArgb(255, r, g, b);
        }

        private static float HueToRGB(float v1, float v2, float vH) {
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

            if (6 * vH < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if (2 * vH < 1)
                return v2;

            if (3 * vH < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
