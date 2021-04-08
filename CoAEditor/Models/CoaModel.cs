using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Text.Json.Serialization;

namespace CoAEditor.Models {
    public class CoaModel : INotifyPropertyChanged {
        private string imagePath;
        private BitmapImage imgComponent;
        private Bitmap originalImage;
        private CustomColor[] colors;
        private float scale;
        private float width;
        private float height;
        private float xpos;
        private float ypos;
        private float sliderXpos;
        private float sliderYpos;

        public event PropertyChangedEventHandler PropertyChanged;

        public CoaModel() { 
            
        }

        // Normalize values for them to work in CK3 (75 -> 0.75, 25 -> 0.25, etc)
        public CoaModel(string image_url, float scale, float xpos, float ypos) {
            //if (CoaComponentsLoader.GeneratePngFromUrl(image_url))
            //    System.Windows.MessageBox.Show(image_url);
            //else
            //    System.Windows.MessageBox.Show("FFMPEG Failure");

            imagePath = image_url;
            imgComponent = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            originalImage = new Bitmap(Image.FromFile(imagePath), 256, 256);
            colors = new CustomColor[] { new CustomColor(Color.Orange),  new CustomColor(Color.HotPink), new CustomColor(Color.CornflowerBlue) };
            this.scale = scale;
            width = height = 256 * scale / 100;
            sliderXpos = xpos;
            sliderYpos = ypos;
            this.xpos = 256 * (sliderXpos / 100) - (width / 2);
            this.ypos = 256 * (sliderYpos / 100) - (height / 2);
            ApplyNewColorsToImage();
        }

        public string ImagePath {
            get { return imagePath; }
            set {
                imagePath = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public BitmapImage ImgComponent {
            get { return imgComponent; }
            set {
                imgComponent = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public Bitmap OriginalImage {
            get { return originalImage; }
            set {
                originalImage = value;
                OnPropertyChanged();
            }
        }

        public CustomColor[] Colors {
            get { return colors; }
            set {
                value.CopyTo(colors, 0);
                OnPropertyChanged();
            }
        }

        public float Scale {
            get { return scale; }
            set {
                if(value > 100) {
                    value = 100;
                }
                scale = value;
                width = height = 256 * scale / 100;
                xpos = 256 * (sliderXpos / 100) - (width / 2);
                ypos = 256 * (sliderYpos / 100) - (height / 2);
                OnPropertyChanged();
                OnPropertyChanged("Width");
                OnPropertyChanged("Height");
                OnPropertyChanged("Xpos");
                OnPropertyChanged("Ypos");
                OnPropertyChanged("FullInfo");
            }
        }

        public float Width {
            get { return width; }
            set {
                width = value;
                OnPropertyChanged();
            }
        }
        public float Height {
            get { return height; }
            set {
                height = value;
                OnPropertyChanged();
            }
        }

        public float Xpos {
            get { return xpos; }
            set {
                xpos = 256 * (value / 100) - (width / 2);
                sliderXpos = (int)((value + width / 2) * 100 / 256);
                OnPropertyChanged();
                OnPropertyChanged("SliderXpos");
                OnPropertyChanged("FullInfo");
            }
        }

        public float Ypos {
            get { return ypos; }
            set {
                ypos = 256 * (value / 100) - (height / 2);
                sliderYpos = (int)((ypos + height / 2) * 100 / 256); ;
                OnPropertyChanged();
                OnPropertyChanged("SliderYpos");
                OnPropertyChanged("FullInfo");
            }
        }

        public float SliderXpos {
            get { return sliderXpos; }
            set {
                sliderXpos = value;
                xpos = 256 * (sliderXpos / 100) - (width / 2);
                OnPropertyChanged();
                OnPropertyChanged("Xpos");
                OnPropertyChanged("FullInfo");
            }
        }

        public float SliderYpos {
            get { return sliderYpos; }
            set {
                sliderYpos = value;
                ypos = 256 * (sliderYpos / 100) - (height / 2);
                OnPropertyChanged();
                OnPropertyChanged("Ypos");
                OnPropertyChanged("FullInfo");
            }
        }

        public string FullInfo {
            get {
                return ImagePath.Substring(ImagePath.LastIndexOf('/') + 1) + " | " + scale.ToString() + " | " + xpos.ToString() + " | " + ypos.ToString();
            }
        }

        public bool IsPattern {
            get {
                return imagePath.Split("/").Reverse().ElementAt(1).ToString().Equals("patterns");
            }
        }

        public void SetDrawingColorByIndex(Color color, int index) {
            if (index < Colors.Length) {
                Colors[index].SetFromDrawingColor(color);
                ApplyNewColorsToImage();
            }
        }

        public void SetHSLColorByIndex(float hue, float sat, float li, int index) {
            if(index < Colors.Length) {
                Colors[index].SetFromHSLColor(hue, sat, li);
                ApplyNewColorsToImage();
            }
        }

        public void SetRGBColorByIndex(byte r, byte g, byte b, int index) {
            if (index < Colors.Length) {
                Colors[index].SetFromRGBColor(r, g, b);
                ApplyNewColorsToImage();
            }
        }

        public void SetCustomColorByIndex(CustomColor customColor, int index) {
            if(index < Colors.Length) {
                Colors[index].SetFromCustomColor(customColor);
                ApplyNewColorsToImage();
            }
        }

        public void ApplyNewColorsToImage() {
            Color[] replacedColors;
            Color currentColor;
            bool skip;

            Bitmap newBmp = new Bitmap(originalImage);

            if (IsPattern) {
                replacedColors = new Color[] { Color.Red, Color.Yellow, Color.White };
            } else {
                replacedColors = new Color[] { Color.FromArgb(255,0,0,128), Color.FromArgb(255,0,255,128), Color.FromArgb(255, 255, 0, 128) };
            }

            for (int i = 0; i < originalImage.Width; i++) {
                for(int j = 0; j < originalImage.Height; j++) {
                    skip = true;
                    currentColor = originalImage.GetPixel(i, j);

                    if (currentColor.A > 16) {
                        for (int k = 0; k < colors.Length && skip; k++) {
                            if (currentColor.GetHue() <= replacedColors[k].GetHue() + 50 && currentColor.GetHue() >= replacedColors[k].GetHue() - 50) {
                                skip = false;
                                newBmp.SetPixel(i, j, IsPattern ? colors[k].DrawingColor : CustomColor.HSLToRGB(colors[k].H, (currentColor.GetSaturation() + colors[k].S) / 2.0f, (currentColor.GetBrightness() + colors[k].L) / 2.0f));
                            }

                            // White is Hue 0, Red is 0 and 360
                            if(IsPattern && currentColor.R == 255 && currentColor.G == 255 && currentColor.B == 255) {
                                newBmp.SetPixel(i, j,colors[2].DrawingColor);
                            }
                        }
                    }
                }
            }
            ImgComponent = SlowBitmap2BitmapImage(newBmp);

            newBmp.Save("testOutput.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        // https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage) {
            using (MemoryStream outStream = new MemoryStream()) {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bmp = new Bitmap(outStream);

                return new Bitmap(bmp);
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapImage Bitmap2BitmapImage(Bitmap bmp) {
            return (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private static BitmapImage SlowBitmap2BitmapImage(Bitmap bmp) {
            using (var memory = new MemoryStream()) {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.StreamSource = memory;
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.EndInit();
                bmpImage.Freeze();

                return bmpImage;
            }
        }

        //private BitmapImage Bitmap2BitmapImage(Bitmap bmp) {
        //    IntPtr hBitmap = bmp.GetHbitmap();
        //    BitmapImage rVal;

        //    try {
        //        rVal = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
        //            hBitmap,
        //            IntPtr.Zero,
        //            System.Windows.Int32Rect.Empty,
        //            BitmapSizeOptions.FromEmptyOptions());
        //    } finally {
        //        DeleteObject(hBitmap);
        //    }

        //    return rVal;
        //}

        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
