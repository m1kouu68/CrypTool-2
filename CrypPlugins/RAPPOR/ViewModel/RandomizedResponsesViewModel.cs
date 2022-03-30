﻿using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CrypTool.Plugins.RAPPOR.ViewModel;
using RAPPOR.Helper;
using RAPPOR.Helper.ArrayDrawer;

namespace RAPPOR.ViewModel
{
    /// <summary>
    /// This class is used to create the view of the randomized response of the rr view tab. 
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.RAPPOR.Properties.Resources")]
    public class RandomizedResponsesViewModel : ObservableObject, IViewModelBase
    {
        /// <summary>
        /// The different array drawers which are being utilized.
        /// </summary>
        private readonly ArrayDrawer arrayDrawer;
        private readonly ArrayDrawerRR arrayDrawerRR;
        private readonly ArrayDrawerHeatMaps arrayDrawerHM;
        /// <summary>
        /// Instance of the current rappor class.
        /// </summary>
        public CrypTool.Plugins.RAPPOR.RAPPOR rappor;

        /// <summary>
        /// Sets up the current RandomizedResponseViewModel. 
        /// </summary>
        /// <param name="rAPPOR">The current rappor instance</param>
        public RandomizedResponsesViewModel(CrypTool.Plugins.RAPPOR.RAPPOR rAPPOR)
        {
            rappor = rAPPOR;

            arrayDrawer = new ArrayDrawer();
            arrayDrawerRR = new ArrayDrawerRR();
            arrayDrawerHM = new ArrayDrawerHeatMaps();
            Canvas canvas = new Canvas();
            RandomizedResponsesCanvas = canvas;
            DrawCanvas();
            OnPropertyChanged("RandomizedResponsesCanvas");
        }
        /// <summary>
        /// This class is used to create the randomized response view model of the component.
        /// </summary>
        public void DrawCanvas()
        {
            RandomizedResponsesCanvas.Children.Clear();
            rappor.RunRappor();
            //Drawing boxes
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(10, 10, 180, 185, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(10, 205, 180, 185, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(200, 10, 400, 380, "#FFFFFF"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(610, 100, 180, 140, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(610, 250, 180, 140, "#F2F2F2"));

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(610, 10, 85, 35, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(705, 10, 85, 35, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(610, 55, 85, 35, "#F2F2F2"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(705, 55, 85, 35, "#F2F2F2"));

            //Text for the variables
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 15, 17, "h: " + rappor.GetRAPPORSettings().GetAmountOfHashFunctions(), "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(710, 15, 17, "f: " + rappor.GetRAPPORSettings().GetFVariable() + " %", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 60, 17, "q: " + rappor.GetRAPPORSettings().GetQVariable() + " %", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(710, 60, 17, "p: " + rappor.GetRAPPORSettings().GetPVariable() + " %", "#000000"));


            //Drawing divider lines
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddStrokedLine(200, 137, 600, 137, 2, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddStrokedLine(200, 274, 600, 274, 2, "#808080"));

            //Drawing Tree
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(400, 50, 320, 182));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(400, 50, 440, 227));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(400, 50, 520, 227));

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(320, 182, 280, 227));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(320, 182, 360, 227));

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(280, 227, 245, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(280, 227, 290, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(360, 227, 335, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(360, 227, 380, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(440, 227, 425, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(440, 227, 470, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(520, 227, 515, 350));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddLine(520, 227, 560, 350));

            int radius = 3;
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(400, 50, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(320, 182, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(440, 227, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(520, 227, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(280, 227, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(360, 227, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(245, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(290, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(335, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(380, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(425, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(470, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(515, 350, radius, "#808080"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddCircle(560, 350, radius, "#808080"));

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(202, 115, 20, "B"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(202, 252, 20, "B'"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(202, 368, 20, "S"));

            //Adding text to tree //Right side
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(405, 38, 10, "B", "#000000"));//400,50
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(413, 44, 4, "i", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(365, 215, 10, "B", "#000000"));//360,227
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(373, 221, 4, "i", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(377, 215, 10, "= 1", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(445, 215, 10, "0", "#000000"));//440,227
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(525, 215, 10, "1", "#000000"));//520,227

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(295, 338, 10, "1", "#000000"));//290,350
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(385, 338, 10, "B", "#000000"));//380,350
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(393, 344, 4, "i", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(475, 338, 10, "1", "#000000"));//470,350
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(565, 338, 10, "1", "#000000"));//560,350

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(425, 138, 10, "f / 2", "#000000"));//420,139
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(475, 138, 10, "f / 2", "#000000"));//470,139
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(290, 287, 10, "p", "#000000"));//285,289
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(375, 287, 10, "q", "#000000"));//370,289
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(460, 287, 10, "p", "#000000"));//455,289
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(545, 287, 10, "q", "#000000"));//540,289

            //Left side
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(303, 170, 10, "B", "#000000"));//320,182
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(311, 176, 4, "i", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(260, 215, 10, "= 0", "#000000"));//280,227
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(256, 221, 4, "i", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(248, 215, 10, "B", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(237, 344, 10, "i", "#000000"));//245,350
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(229, 338, 10, "B", "#000000"));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(325, 338, 10, "0", "#000000"));//335,350
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(415, 338, 10, "0", "#000000"));//425,350
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(505, 338, 10, "0", "#000000"));//515,350

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(320, 138, 10, "1 - f", "#000000"));//360,116
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(233, 287, 10, "1 - p", "#000000"));//263,289
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(318, 287, 10, "1 - q", "#000000"));//348,289
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(403, 287, 10, "1 - p", "#000000"));//433,289
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(488, 287, 10, "1 - q", "#000000"));//518,289

            //Adding Images
            //Better quality with this perhaps: https://stackoverflow.com/questions/87753/resizing-an-image-without-losing-any-quality
            Image prrImage = new Image
            {
                Source = new BitmapImage(new Uri("..\\Images\\PermanentRandomizedResponse.png", UriKind.Relative)),
                Width = 166
            };//10, 10, 180, 185//901,288
            Canvas.SetLeft(prrImage, 17);
            Canvas.SetTop(prrImage, 41);

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(15, 36, 170, 53 + 4 + 4, "#FFFFFF"));
            RandomizedResponsesCanvas.Children.Add(prrImage);
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 16, 16, "B \u2192 B'"));

            Image irrImage = new Image
            {
                Source = new BitmapImage(new Uri("..\\Images\\InstantaneousRandomizedResponse.png", UriKind.Relative)),
                Width = 166
            };//10, 103, 180, 92,//865,247
            Canvas.SetLeft(irrImage, 17);
            Canvas.SetTop(irrImage, 139);

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(15, 134, 170, 47 + 4 + 4, "#FFFFFF"));
            RandomizedResponsesCanvas.Children.Add(irrImage);
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 114, 16, "B' \u2192 S"));

            int y = 90;
            Image eInfImage = new Image
            {
                Source = new BitmapImage(new Uri("..\\Images\\epsilonInfinity.png", UriKind.Relative)),
                Width = 166
            }; //610, 10, 180, 185,//237,105
            Canvas.SetLeft(eInfImage, 617);//image placement x
            Canvas.SetTop(eInfImage, 41 + y);//image placement y

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(615, 36 + y, 170, 74 + 4 + 4, "#FFFFFF"));//image rectangle
            RandomizedResponsesCanvas.Children.Add(eInfImage);
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 16 + y, 14, CrypTool.Plugins.RAPPOR.Properties.Resources.DifferentialPrivacyLevel + "\u03B5")); //top text
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(765, 22 + y, 10, "\u221E")); //top text infinity symbol

            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 123 + y, 20, "\u03B5  = " + string.Format("{0:0.##########}", calculateEpsilonInfinity(rappor.GetRAPPORSettings().GetAmountOfHashFunctions(), (double)rappor.GetRAPPORSettings().GetFVariable() / 100))));//bottom calc
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(623, 130 + y, 14, "\u221E"));//bottom epsilon


            y = y - 45;

            Image epsilonOne = new Image
            {
                Source = new BitmapImage(new Uri("..\\Images\\epsilonOne.png", UriKind.Relative)),
                Width = 166
            };//610, 205, 180, 185,//262,105
            Canvas.SetLeft(epsilonOne, 617);//image placement x
            Canvas.SetTop(epsilonOne, 232 + y);//image placement y

            RandomizedResponsesCanvas.Children.Add(arrayDrawerRR.AddRectangle(615, 230 + y, 170, 67 + 4, "#FFFFFF"));//image rectangle
            RandomizedResponsesCanvas.Children.Add(epsilonOne);
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 212 + y, 14, CrypTool.Plugins.RAPPOR.Properties.Resources.DifferentialPrivacyLevel + " \u03B5\u2081"));//top text
            //RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 306, 20, "\u03B5\u2081: " + calculateEpsilonOne((double)rappor.GetRAPPORSettings().GetAmountOfHashFunctions(), (double)rappor.GetRAPPORSettings().GetFVariable() / 100, (double)rappor.GetRAPPORSettings().GetQVariable() / 100, (double)rappor.GetRAPPORSettings().GetPVariable() / 100)));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(615, 306 + y, 20, "\u03B5\u2081= " + string.Format("{0:0.##########}", calculateEpsilonOne(rappor.GetRAPPORSettings().GetAmountOfHashFunctions(), (double)rappor.GetRAPPORSettings().GetFVariable() / 100, (double)rappor.GetRAPPORSettings().GetQVariable() / 100, (double)rappor.GetRAPPORSettings().GetPVariable() / 100)))); //bottom text



            //10, 205, 180, 185,
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 210, 14, CrypTool.Plugins.RAPPOR.Properties.Resources.qStarText));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 272, 14, "q*= " + string.Format("{0:0.##########}", calculateQStar((double)rappor.GetRAPPORSettings().GetFVariable() / 100, (double)rappor.GetRAPPORSettings().GetQVariable() / 100, (double)rappor.GetRAPPORSettings().GetPVariable() / 100))));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 300, 14, CrypTool.Plugins.RAPPOR.Properties.Resources.pStarText));
            RandomizedResponsesCanvas.Children.Add(arrayDrawerHM.AddText(15, 362, 14, "p*= " + string.Format("{0:0.##########}", calculatePStar((double)rappor.GetRAPPORSettings().GetFVariable() / 100, (double)rappor.GetRAPPORSettings().GetQVariable() / 100, (double)rappor.GetRAPPORSettings().GetPVariable() / 100))));

            OnPropertyChanged("RandomizedResponsesCanvas");
        }
        /// <summary>
        /// Probability of observing 1 given that the underlying Bloom filer bit was set is given by this formular.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="q"></param>
        /// <param name="p"></param>
        /// <returns>The q star value</returns>
        public double calculateQStar(double f, double q, double p)
        {
            return 0.5 * f * (p + q) + (1 - f) * q;
        }
        /// <summary>
        /// Probability of observing 1 given that the underlying Bloom filer bit was not set is given by this formular.
        /// </summary>
        /// <param name="f">User tunable parameter f</param>
        /// <param name="q">User tunable parameter q</param>
        /// <param name="p">User tunable parameter p</param>
        /// <returns>The p star value</returns>
        public double calculatePStar(double f, double q, double p)
        {
            return 0.5 * f * (p + q) + (1 - f) * p;
        }
        /// <summary>
        /// Provided differential privacy value against an attack who is able to collect all instantaneous randomized responses or the permanent randomized response.
        /// </summary>
        /// <param name="h">User tunable parameter h</param>
        /// <param name="f">User tunable parameter f</param>
        /// <returns>Epsilon infinity value</returns>
        public double calculateEpsilonInfinity(double h, double f)
        {
            if (f == 0)
            {
                return double.NaN;
            }
            //Math.Log is log with base e.
            return 2 * h * Math.Log((1 - 0.5 * f) / (0.5 * f));
        }
        /// <summary>
        /// Provided differential privacy value against an attack who is able to collect one instantaneous randomized response.
        /// </summary>
        /// <param name="h">User tunable parameter  h</param>
        /// <param name="f">User tunable parameter  f</param>
        /// <param name="q">User tunable parameter  q</param>
        /// <param name="p">User tunable parameter  p</param>
        /// <returns>epsilon one value</returns>
        public double calculateEpsilonOne(double h, double f, double q, double p)
        {
            double qStar = calculateQStar(f, q, p);
            double pStar = calculatePStar(f, q, p);
            return Math.Abs(h * Math.Log(qStar * (1 - pStar) / (pStar * (1 - qStar)), 2));
        }

        /// <summary>
        /// The randomized response canvas.
        /// </summary>
        private Canvas _randomizedResponsesCanvas;

        /// <summary>
        /// Getter and setter for the randomized response canvas
        /// </summary>
        public Canvas RandomizedResponsesCanvas
        {
            get
            {
                if (_randomizedResponsesCanvas == null)
                {
                    return null;
                }

                return _randomizedResponsesCanvas;
            }
            set
            {
                _randomizedResponsesCanvas = value;
                OnPropertyChanged("RandomizedResponsesCanvas");
            }
        }
    }
}
