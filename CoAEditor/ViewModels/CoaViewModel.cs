using CoAEditor.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace CoAEditor.ViewModels {
    public class CoaViewModel : INotifyPropertyChanged {
        public ObservableCollection<CoaModel> coaList;
        public List<string> coaComponents;
        public CoaModel selectedCoa;
        public string selectedComponent;
        public CustomColor selectedCustomColor;
        public CoaComponentsLoader coaCompFilesLoader;
        public bool coaCompFilesBtnVisibility;

        public MyICommand DeleteCommand { get; set; }
        public MyICommand DuplicateCommand { get; set; }
        public MyICommand MoveUpCommand { get; set; }
        public MyICommand MoveDownCommand { get; set; }
        public MyICommand SetCk3RootCommand { get; set; }
        public MyICommand AddCoaComponentCommand { get; set; }
        public MyICommand ExportCk3FormatCommand { get; set; }
        public MyICommandWithParameter<string> PickColorCommand { get; set; }
        public MyICommandWithParameter<string> ChangeSL { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public CoaViewModel() {
            coaList = new ObservableCollection<CoaModel>();

            coaCompFilesLoader = new CoaComponentsLoader();
            coaCompFilesBtnVisibility = CanSetCk3Root();

            DeleteCommand = new MyICommand(OnDelete, CanDelete);
            DuplicateCommand = new MyICommand(OnDuplicate, CanDelete);
            MoveUpCommand = new MyICommand(OnMoveUp, CanMoveUp);
            MoveDownCommand = new MyICommand(OnMoveDown, CanMoveDown);
            SetCk3RootCommand = new MyICommand(OnSetCk3Root, CanSetCk3Root);
            AddCoaComponentCommand = new MyICommand(OnAddComponent, CanAddComponent);
            ExportCk3FormatCommand = new MyICommand(OnExportCk3Format, CanDelete);
            PickColorCommand = new MyICommandWithParameter<string>(OnPickColor, CanPickColor);
            ChangeSL = new MyICommandWithParameter<string>(OnChangeSL, CanPickColor);

            InitCoaComponents();

        }

        public ObservableCollection<CoaModel> CoaList {
            get { return coaList; }
            set { coaList = value; }
        }

        public CoaModel SelectedCoa {
            get {
                return selectedCoa;
            }
            set {
                selectedCoa = value;
                if (selectedCoa != null)
                    SelectedCustomColor = selectedCoa.Colors[0];
                else
                    SelectedCustomColor = new CustomColor(0, 0, 0);
                DeleteCommand.OnCanExecuteChanged();
                DuplicateCommand.OnCanExecuteChanged();
                PickColorCommand.OnCanExecuteChanged();
                ChangeSL.OnCanExecuteChanged();
                ExportCk3FormatCommand.OnCanExecuteChanged();
                OnPropertyChanged();
                OnPropertyChanged("CustomColors");
                OnPropertyChanged("SelectedCustomColor");
                OnPropertyChanged("ColorHue");
                OnPropertyChanged("ColorSaturation");
                OnPropertyChanged("ColorLightness");
            }
        }

        public List<string> CoaComponents {
            get { return coaComponents; }
            set {
                coaComponents = value;
                OnPropertyChanged();
            }
        }

        public CustomColor[] CustomColors {
            get {
                if (SelectedCoa != null)
                    return SelectedCoa.Colors;
                else
                    return null;
            }
        }

        public string SelectedComponent {
            get { return selectedComponent; }
            set {
                selectedComponent = value;
                AddCoaComponentCommand.OnCanExecuteChanged();
            }
        }

        public CustomColor SelectedCustomColor {
            get { return selectedCustomColor; }
            set {
                selectedCustomColor = value;
                OnPropertyChanged();
                OnPropertyChanged("CustomColors");
                OnPropertyChanged("ColorHue");
                OnPropertyChanged("ColorSaturation");
                OnPropertyChanged("ColorLightness");
            }
        }

        public CoaComponentsLoader CoaCompFilesLoader {
            get {
                return coaCompFilesLoader;
            }
            set {
                coaCompFilesLoader = value;
            }
        }

        public bool CoaCompFilesBtnVisibility {
            get { return coaCompFilesBtnVisibility; }
            set { 
                coaCompFilesBtnVisibility = value;
                OnPropertyChanged();
            }
        }

        public float ColorHue {
            get {
                if (SelectedCustomColor != null)
                    return SelectedCustomColor.H;
                else
                    return 0;
            }
            set {
                if(SelectedCoa != null) {
                    SelectedCustomColor.SetFromHSLColor(value, SelectedCustomColor.S, SelectedCustomColor.L);
                    SelectedCoa.ApplyNewColorsToImage();
                    OnPropertyChanged();
                    OnPropertyChanged("CustomColors");
                }
            }
        }

        public float ColorSaturation {
            get {
                if (SelectedCustomColor != null)
                    return SelectedCustomColor.S;
                else
                    return 0;
            }
            set {
                if (SelectedCoa != null) {
                    float newSat = value;
                    if (value >= 1.0f)
                        newSat = 1.0f;
                    else if (value <= 0)
                        newSat = 0.0f;
                    SelectedCustomColor.SetFromHSLColor(SelectedCustomColor.H, newSat, SelectedCustomColor.L);
                    SelectedCoa.ApplyNewColorsToImage();
                    OnPropertyChanged();
                    OnPropertyChanged("CustomColors");
                }
            }
        }

        public float ColorLightness {
            get {
                if (SelectedCustomColor != null)
                    return SelectedCustomColor.L;
                else
                    return 0;
            }
            set {
                if (SelectedCoa != null) {
                    float newLi = value;
                    if (value >= 1.0f)
                        newLi = 1.0f;
                    else if (value <= 0)
                        newLi = 0.0f;
                    SelectedCustomColor.SetFromHSLColor(SelectedCustomColor.H, SelectedCustomColor.S, newLi);
                    SelectedCoa.ApplyNewColorsToImage();
                    OnPropertyChanged();
                    OnPropertyChanged("CustomColors");
                }
            }
        }

        private void InitCoaComponents() {
            coaComponents = new List<string>();
            if (coaCompFilesLoader.ck3RootDir != null) {
                string[] files = CoaComponentsLoader.GetFileNames(coaCompFilesLoader.ck3RootDir + "\\game\\gfx\\coat_of_arms\\patterns", "*.dds");
                foreach (string file in files) {
                    coaComponents.Add("pa:"+ file.Substring(0, file.LastIndexOf('.')) + ".png");
                }

                files = CoaComponentsLoader.GetFileNames(coaCompFilesLoader.ck3RootDir + "\\game\\gfx\\coat_of_arms\\colored_emblems", "*.dds");
                foreach (string file in files) {
                    coaComponents.Add("ce:"+file.Substring(0, file.LastIndexOf('.')) + ".png");
                }
            }
        }

        private void OnDelete() {
            int currIdx = coaList.IndexOf(SelectedCoa);

            SelectedCustomColor = new CustomColor(0, 0, 0);
            coaList.Remove(selectedCoa);

            if (currIdx > 0) {
                SelectedCoa = coaList[currIdx - 1];
            }
        }

        private bool CanDelete() {
            return selectedCoa != null;
        }

        private void OnDuplicate() {
            CoaModel newCoa = new CoaModel(SelectedCoa.ImagePath, SelectedCoa.Scale, SelectedCoa.SliderXpos, SelectedCoa.SliderYpos);
            newCoa.Colors = new CustomColor[3] { new CustomColor(SelectedCoa.Colors[0].DrawingColor), new CustomColor(SelectedCoa.Colors[1].DrawingColor), new CustomColor(SelectedCoa.Colors[2].DrawingColor)};
            newCoa.ApplyNewColorsToImage();
            coaList.Add(newCoa);
            SelectedCoa = coaList[coaList.Count - 1];
        }

        private void OnMoveUp() {
            int currIdx = coaList.IndexOf(SelectedCoa);
            if(currIdx > 0) {
                coaList.Move(currIdx, currIdx - 1);
            }
        }

        private bool CanMoveUp() {
            return true;
            //int currIdx = coaList.IndexOf(SelectedCoa);
            //return currIdx > 0;
        }

        private void OnMoveDown() {
            int currIdx = coaList.IndexOf(SelectedCoa);
            if (currIdx < coaList.Count - 1) {
                coaList.Move(currIdx, currIdx + 1);
            }
        }

        private bool CanMoveDown() {
            return true;
            //int currIdx = coaList.IndexOf(SelectedCoa);
            //return (currIdx < coaList.Count - 1);
        }

        private async void OnSetCk3Root() {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog()) {
                System.Windows.Forms.DialogResult result = dlg.ShowDialog();

                if (result.ToString().Equals("OK")) {
                    string dir = dlg.SelectedPath;
                    string currDir = dir.Substring(dir.LastIndexOf("\\") + 1);

                    if(currDir.Equals("Crusader Kings III")) {
                        coaCompFilesLoader.ck3RootDir = dir;
                        await coaCompFilesLoader.WriteCk3DirToFile();

                        // FIX THIS TRASH: MAKE BTN DISAPPEAR WHEN DIR HAS BEEN SET
                        CoaCompFilesBtnVisibility = false;
                        InitCoaComponents();
                        CoaComponentsLoader.ImportCoaComponents(coaCompFilesLoader.ck3RootDir + "\\game\\gfx\\coat_of_arms\\patterns", "./coat_of_arms/patterns", ".dds", ".png");
                        CoaComponentsLoader.ImportCoaComponents(coaCompFilesLoader.ck3RootDir + "\\game\\gfx\\coat_of_arms\\colored_emblems", "./coat_of_arms/colored_emblems", ".dds", ".png");
                        OnPropertyChanged("CoaCompFilesBtnVisibility");
                        OnPropertyChanged("CoaComponents");
                        System.Windows.MessageBox.Show("CK3 root dir set:\r\n" + coaCompFilesLoader.ck3RootDir);
                    } else {
                        System.Windows.MessageBox.Show("Incorrect Path");
                    }
                }

                dlg.Dispose();
            }
        }

        private bool CanSetCk3Root() {
            return coaCompFilesLoader.ck3RootDir == null;
        }

        private void OnAddComponent() {
            string[] auxStr = selectedComponent.Split(":");
            string componentType = auxStr[0].Equals("ce") ? "colored_emblems" : "patterns";
            coaList.Add(new CoaModel("./coat_of_arms/" + componentType + "/" + auxStr[1], 100, 50, 50));
            SelectedCoa = coaList[coaList.Count - 1];
            SelectedCustomColor = SelectedCoa.Colors[0];
        }

        private bool CanAddComponent() {
            return selectedComponent != null;
        }

        private void OnExportCk3Format() {
            var options = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var coaJson = JsonSerializer.Serialize(SelectedCoa, options);
            //System.Windows.MessageBox.Show(coaJson);

            StringBuilder sb = new StringBuilder("custom_coat_of_arms = {\r\n");
            foreach (CoaModel coa in CoaList) {
                string coaName = coa.ImagePath.Remove(coa.ImagePath.LastIndexOf('.')).Substring(coa.ImagePath.LastIndexOf('/') + 1);
                string c0 = string.Format(" {0} {1} {2} ", coa.Colors[0].R.ToString(), coa.Colors[0].G.ToString(), coa.Colors[0].B.ToString());
                string c1 = string.Format(" {0} {1} {2} ", coa.Colors[1].R.ToString(), coa.Colors[1].G.ToString(), coa.Colors[1].B.ToString());
                string c2 = string.Format(" {0} {1} {2} ", coa.Colors[2].R.ToString(), coa.Colors[2].G.ToString(), coa.Colors[2].B.ToString());

                if (coa.IsPattern) {
                    sb.AppendLine(string.Format("\tpattern = \"{0}.dds\"", coaName));
                    sb.AppendLine(string.Format("\tcolor1 = rgb {{{0}}}", c0));
                    sb.AppendLine(string.Format("\tcolor2 = rgb {{{0}}}", c1));
                    sb.AppendLine(string.Format("\tcolor3 = rgb {{{0}}}", c2));
                    sb.AppendLine("");
                } else {
                    sb.AppendLine("\tcolored_emblem = {");
                    sb.AppendLine(string.Format("\t\tcolor1 = rgb {{{0}}}", c0));
                    sb.AppendLine(string.Format("\t\tcolor2 = rgb {{{0}}}", c1));
                    sb.AppendLine(string.Format("\t\tcolor3 = rgb {{{0}}}", c2));
                    sb.AppendLine(string.Format("\t\ttexture = \"{0}.dds\"", coaName));
                    sb.AppendLine("\t\tinstance = {");
                    sb.AppendLine(string.Format("\t\t\tposition = {{ {0:F} {1:F} }}", coa.SliderXpos / 100, coa.SliderYpos / 100));
                    sb.AppendLine(string.Format("\t\t\tscale = {{ {0:F} {1:F} }}", coa.Scale / 100, coa.Scale / 100));
                    sb.AppendLine("\t\t}");
                    sb.AppendLine("\t}");
                    sb.AppendLine("");
                }
            }
            sb.AppendLine("}");
            System.Windows.Clipboard.SetText(sb.ToString());
            System.Windows.MessageBox.Show(sb.ToString(), "Coat of Arms");
        }

        private void OnPickColor(string numColor) {
            int colorIdx = short.Parse(numColor);
            System.Windows.Forms.ColorDialog pickColor = new System.Windows.Forms.ColorDialog();
            pickColor.AllowFullOpen = true;
            pickColor.AnyColor = true;
            pickColor.Color = SelectedCustomColor.DrawingColor;

            if(pickColor.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                SelectedCustomColor.SetFromDrawingColor(pickColor.Color);
                SelectedCoa.ApplyNewColorsToImage();

                OnPropertyChanged("CustomColors");
                OnPropertyChanged("ColorHue");
                OnPropertyChanged("ColorSaturation");
                OnPropertyChanged("ColorLightness");
            }
        }

        private bool CanPickColor(string parm) {
            return selectedCoa != null;
        }

        private void OnChangeSL(string settings) {
            string[] aux = settings.Split(":");
            float changeVal = short.Parse(aux[1]) * 0.05f;
            
            if (aux[0].Equals("S")) {
                ColorSaturation += changeVal;
            } else {
                ColorLightness += changeVal;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
