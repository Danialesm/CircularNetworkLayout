using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace LanguageAnalyzer
{
    public partial class Form1 : Form
    {
        bool Fig5 = true;
        bool Fig6 = false;
        double R = 0;
        const double rad = 355;
        int[] prime_numbers = new int[] {2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127};
        List<int> used_numbers = new List<int>();
        Dictionary<string, Color> FamilyColors = new Dictionary<string, Color>();
        List<Color> defined_colors = new List<Color>() { Color.Sienna, Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Navy, Color.Pink, Color.Magenta, Color.Lime, Color.LightSteelBlue, Color.Khaki, Color.Honeydew, Color.GreenYellow, Color.Gold, Color.Fuchsia, Color.Azure, Color.Wheat, Color.Turquoise, Color.Tomato, Color.Tan, Color.Teal, Color.Thistle, Color.Silver };
        Dictionary<string, double> Sizes = new Dictionary<string, double>();
        Dictionary<string, double> NumberOfConnections = new Dictionary<string, double>();
        Dictionary<Point, double> Links = new Dictionary<Point, double>();
        Dictionary<int, double> Balance = new Dictionary<int, double>();
        Dictionary<int, Color> LinksColors = new Dictionary<int, Color>();
        Dictionary<int, int> Mapping = new Dictionary<int, int>();
        Dictionary<PointF, double> Occupied_Spaces = new Dictionary<PointF, double>();
        public Form1()
        {
            InitializeComponent();
            for (int i = prime_numbers.Length - 1; i >= 0; i--)
            {
                used_numbers.Add(prime_numbers[i]);
            }
        }
        double MinimumPosition = 0;
        double MaximumPosition = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            string[] lines = File.ReadAllLines(@"..\..\..\..\Sizes.txt");
            Dictionary<string, int> PositionsOnMatrix = new Dictionary<string,int>(); //for sparsity
            foreach (var line in lines)
            {
                string[] splitted = line.Split('\t');
                Sizes.Add(splitted[0].Trim(), double.Parse(splitted[1].Trim()));
                PositionsOnMatrix.Add(splitted[0].Trim(), PositionsOnMatrix.Count);

                Mapping.Add(Sizes.Last().Key.GetHashCode(), Sizes.Keys.Count);
                if (!Balance.Keys.Contains(Sizes.Last().Key.GetHashCode()))
                    Balance.Add(Sizes.Last().Key.GetHashCode(), 0);
                if (!NumberOfConnections.Keys.Contains(Sizes.Last().Key))
                    NumberOfConnections.Add(Sizes.Last().Key, 0);

            }

            lines = File.ReadAllLines(@"..\..\..\..\Data.txt");
            int[,] Sparsity = new int[100, 100];

            foreach (var line in lines)
            {
                string[] splitted = line.Split('\t');

                int Index = splitted[0].IndexOf(" to ");
                string from = splitted[0].Substring(0, Index).Trim();
                string to = splitted[0].Substring(Index + 4).Trim();
                Sparsity[PositionsOnMatrix[from], PositionsOnMatrix[to]] = 1;
                double volume = double.Parse(splitted[1].Trim());
                NumberOfConnections[from]++;
                NumberOfConnections[to]++;
                Balance[from.GetHashCode()] -= volume;
                Balance[to.GetHashCode()] += volume;

                Links.Add(new Point(Mapping[from.GetHashCode()], Mapping[to.GetHashCode()]), double.Parse(splitted[1].Trim()));
            }
            
            string str_Sparsity = "[";
            for (int i = 0; i < 100; i++)
            {
                str_Sparsity += "[";
                for (int j = 0; j < 100; j++)
                {
                    str_Sparsity += Sparsity[i, j].ToString() + (j < 100 ? "," : "");
                }
                str_Sparsity += "];";
            }
            str_Sparsity += "];";


            if (Fig5 == false)
            {
                for (int i = 0; i < NumberOfConnections.Count; i++)
                {
                    if (!Fig6)
                        NumberOfConnections[NumberOfConnections.ElementAt(i).Key] = (double)(NumberOfConnections.ElementAt(i).Value / Sizes[NumberOfConnections.ElementAt(i).Key]);  //For Fig6 and Fig 7
                    if (NumberOfConnections.ElementAt(i).Value < MinimumPosition)
                        MinimumPosition = NumberOfConnections.ElementAt(i).Value;
                    if (NumberOfConnections.ElementAt(i).Value > MaximumPosition)
                        MaximumPosition = NumberOfConnections.ElementAt(i).Value;
                }
            }
            else
            {
                foreach (var item in Balance)
                {
                    if (item.Value < MinimumPosition)
                        MinimumPosition = item.Value;
                    if (item.Value > MaximumPosition)
                        MaximumPosition = item.Value;
                }
            }

            Random randonGen = new Random();
            int color_counter = 0;
            lines = File.ReadAllLines(@"..\..\..\..\Families.txt");
            foreach (var line in lines)
            {
                string[] splitted = line.Split('\t');
                Color randomColor = defined_colors.First(); defined_colors.Remove(randomColor);
                string[] splitted_language = splitted[1].Split(',');
                foreach (var lang in splitted_language)
                {
                    FamilyColors.Add(lang.Trim().ToLower(), randomColor);
                }
                color_counter++;
            }

            if (!Fig5)
            {
                if (Fig6)
                    WriteToXML(@"..\..\..\..\NOC.xml");
                else
                    WriteToXML(@"..\..\..\..\NC-NS.xml");
            }
            else
                WriteToXML(@"..\..\..\..\BL.xml");

        }

        public static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, ascending, 0);
                case 1:
                    return Color.FromArgb(255, descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, ascending);
                case 3:
                    return Color.FromArgb(255, 0, descending, 255);
                case 4:
                    return Color.FromArgb(255, ascending, 0, 255);
                default:
                    return Color.FromArgb(255, 255, 0, descending);
            }
        }

        public void WriteToXML(string fileName)
        {
            int previous_angle = 0;
            double ratio = 0; ;
            using (XmlWriter writer = XmlWriter.Create(fileName))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("DATA_XML");
                {
                    #region "Save BG"
                    writer.WriteStartElement("BG");
                        writer.WriteElementString("COLOR", Color.White.ToArgb().ToString());
                        writer.WriteElementString("COLOR2", Color.White.ToArgb().ToString());
                        writer.WriteElementString("Direction", "Vertical");
                        writer.WriteElementString("isGradient", "True");
                        writer.WriteElementString("showIDCard", "False");
                        writer.WriteElementString("Paralax", "False");
                        writer.WriteElementString("isRadial", "False");
                        writer.WriteElementString("Version", "10");
                        writer.WriteElementString("DesignTime_W", "1348");
                        writer.WriteElementString("DesignTime_H", "785");
                        writer.WriteElementString("RotateCanvas", "False");
                        writer.WriteElementString("BGMusic", "0");
                    writer.WriteEndElement();
                    #endregion

                    writer.WriteStartElement("FilledRectangles");
                    writer.WriteEndElement();
                    
                    #region "Save Filled Ellipses"
                    float progress = 0;
                    writer.WriteStartElement("FilledEllipses"); 
                    foreach (var item in Sizes)
                    {
                        writer.WriteStartElement("FilledEllipse");
                            writer.WriteElementString("Key", Mapping[item.Key.GetHashCode()].ToString());
                            writer.WriteElementString("ID", Mapping[item.Key.GetHashCode()].ToString());
                            progress += 0.01f;
                            LinksColors.Add(Mapping[item.Key.GetHashCode()], Rainbow(progress));

                            bool isOccupied = true;
                            double diameter = (10 * item.Value);
                            while (isOccupied)
                            {
                                int tmp_angle = used_numbers.First();
                                previous_angle += used_numbers.First();
                                used_numbers.Remove(tmp_angle);
                                used_numbers.Add(tmp_angle);
                                isOccupied = false;
                                if (Fig5)
                                {
                                    ratio = ((Balance[item.Key.GetHashCode()] - MinimumPosition) / (MaximumPosition - MinimumPosition));
                                    R = rad * Math.Pow(ratio, 2);
                                }
                                else
                                {
                                    ratio = ((NumberOfConnections[item.Key] - MaximumPosition) / (MaximumPosition - MinimumPosition));
                                    R = rad * Math.Pow(ratio, 1);
                                }

                                double tmp_x = ((R) * Math.Cos((previous_angle + used_numbers.First()) * Math.PI / 180.0));
                                double tmp_y = ((R) * Math.Sin((previous_angle + used_numbers.First()) * Math.PI / 180.0));

                                foreach (var range in Occupied_Spaces)
                                {
                                    double distance = Math.Sqrt((tmp_x - range.Key.X) * (tmp_x - range.Key.X) + (tmp_y - range.Key.Y) * (tmp_y - range.Key.Y));
                                    if (distance<range.Value/1.2)
                                    {
                                        isOccupied = true;
                                        break;
                                    }
                                }
                            }
                            double x, y, xx, yy;
                            if (Fig5)
                            {
                                ratio = ((Balance[item.Key.GetHashCode()] - MinimumPosition) / (MaximumPosition - MinimumPosition));
                                R = rad * Math.Pow(ratio, 2);
                            }
                            else
                            {
                                ratio = ((NumberOfConnections[item.Key] - MaximumPosition) / (MaximumPosition - MinimumPosition));
                                R = rad * Math.Pow(ratio, 1);
                            }
                            x = (R) * Math.Cos((previous_angle + used_numbers.First()) * Math.PI / 180.0);
                            y = (R) * Math.Sin((previous_angle + used_numbers.First()) * Math.PI / 180.0);
                            xx = (x + (diameter / 2));
                            yy = (y + (diameter / 2));
                            double fromAngle = Math.Atan2(y, x) * 180 / Math.PI;
                            double toAngle = Math.Atan2(yy, xx) * 180 / Math.PI;

                            
                            Occupied_Spaces.Add(new PointF((float)x,(float)y), diameter);

                            writer.WriteElementString("Left", (x - diameter / 2.0).ToString());
                            writer.WriteElementString("Top", (y - diameter / 2.0).ToString());
                            writer.WriteElementString("Width", diameter.ToString());
                            writer.WriteElementString("Height", diameter.ToString());
                            writer.WriteElementString("Opacity", "1");
                            try
                            {
                                writer.WriteElementString("ForeColor", FamilyColors[item.Key.ToLower()].ToArgb().ToString());
                            }
                            catch
                            {
                                writer.WriteElementString("ForeColor", Color.White.ToArgb().ToString());
                            }
                            writer.WriteElementString("BorderColor", Color.Black.ToArgb().ToString());
                            writer.WriteElementString("TextColor", Color.Black.ToArgb().ToString());
                            writer.WriteElementString("BoarderWidth", "0.01");
                            writer.WriteElementString("String", "\r\n\r\n\r\n" + item.Key + "\r\n" + (Fig5 ? Balance[item.Key.GetHashCode()].ToString() : (Fig6 ? NumberOfConnections[item.Key].ToString() : NumberOfConnections[item.Key].ToString("#0.##"))));
                            writer.WriteElementString("Effect", "");
                            writer.WriteElementString("FontName", "Tahoma");
                            writer.WriteElementString("FontWeight", "Regular");
                            writer.WriteElementString("FontStyle", "Normal");

                            double fontSize = 1 * item.Value;
                            writer.WriteElementString("FontSize", fontSize.ToString());
                            writer.WriteElementString("TextAlignment", "Center");
                            double angle = Math.Atan2(y, x) * 180 / Math.PI;
                            writer.WriteElementString("Angle", "0");//angle.ToString());

                            writer.WriteElementString("inGroup", "false");
                            writer.WriteElementString("WhichGroup", "");
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    #endregion

                    writer.WriteStartElement("Texts");
                    writer.WriteEndElement();
                    int KeysStarts = Mapping.Values.Max() + 1;

                    #region "Save Lines"
                    writer.WriteStartElement("Lines");
                    foreach (var item in Links)
                    {
                        writer.WriteStartElement("Line");
                        KeysStarts++;
                        writer.WriteElementString("Key", KeysStarts.ToString());
                        writer.WriteElementString("ID", KeysStarts.ToString());
                        writer.WriteElementString("SrcPointX", "0");
                        writer.WriteElementString("SrcPointY", "0");
                        writer.WriteElementString("DstPointX", "2");
                        writer.WriteElementString("DstPointY", "2");
                        writer.WriteElementString("Opacity", "1");
                        writer.WriteElementString("ForeColor", LinksColors[item.Key.X].ToArgb().ToString());

                        writer.WriteElementString("Dst_Anchor", "0");
                        writer.WriteElementString("Src_Anchor", "0");
                        writer.WriteElementString("Src_ID", item.Key.X.ToString());
                        writer.WriteElementString("Dst_ID", item.Key.Y.ToString());
                        writer.WriteElementString("Src_topleftX", "1");
                        writer.WriteElementString("Src_topleftY", "1");
                        writer.WriteElementString("Src_bottomrightX", "2");
                        writer.WriteElementString("Src_bottomrightY", "2");

                        writer.WriteElementString("Dst_topleftX", "3");
                        writer.WriteElementString("Dst_topleftY", "3");
                        writer.WriteElementString("Dst_bottomrightX", "4");
                        writer.WriteElementString("Dst_bottomrightY", "4");

                        writer.WriteElementString("Src_Type", "Filled Ellipse");
                        writer.WriteElementString("Dst_Type", "Filled Ellipse");
                        if (item.Value > 100)
                            writer.WriteElementString("LineWidth", (0.000001 * (item.Value)).ToString());
                        else
                            writer.WriteElementString("LineWidth", (0.02 * Math.Log10(item.Value)).ToString());
                        writer.WriteElementString("ShowArrow", "True");
                        if (item.Value > 100)
                            writer.WriteElementString("Effect", "DashType|Solid|ArrowAtSrc|False");
                        else
                            writer.WriteElementString("Effect", "DashType|Solid|ArrowAtSrc|False");

                        writer.WriteElementString("inGroup", "false");
                        writer.WriteElementString("WhichGroup", "");
                        writer.WriteElementString("Curvature", "1");
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    #endregion

                    writer.WriteStartElement("Frames");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Images");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Sprites");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Movies");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Beziers");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Groups");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Environment");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            Application.Exit();
        }
    }
}
