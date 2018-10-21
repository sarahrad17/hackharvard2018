using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceTutorial
{
    public partial class MainWindow : Window
    {
        // Replace <SubscriptionKey> with your valid subscription key.
        // For example, subscriptionKey = "0123456789abcdef0123456789ABCDEF"
        private const string subscriptionKey = "97e7f0c3351d4554a1548983e08300f9";

        // Replace or verify the region.
        //
        // You must use the same region as you used to obtain your subscription
        // keys. For example, if you obtained your subscription keys from the
        // westus region, replace "westcentralus" with "westus".
        //
        // NOTE: Free trial subscription keys are generated in the westcentralus
        // region, so if you are using a free trial subscription key, you should
        // not need to change this region.
        
        private const string faceEndpoint =
            "https://westus.api.cognitive.microsoft.com";
        private string gender = "yeet";
        private string age = "yeet";
        private string emotion = "yeet";
        private string survive = "yeet";
        private string sloshed = "yeet";
        private string cheese = "yeet";
        private string prog = "yeet";
        private string phone = "yeet";
        private string veg = "yeet";
        private string feeling = "yeet";
        private string book = "yeet";
        public string final = "";
        private readonly IFaceClient faceClient = new FaceClient(
            new ApiKeyServiceClientCredentials(subscriptionKey),
            new System.Net.Http.DelegatingHandler[] { });

        IList<DetectedFace> faceList;   // The list of detected faces.
        String[] faceDescriptions;      // The list of descriptions for the detected faces.
        double resizeFactor;            // The resize factor for the displayed image.
        public string output = "";

        public MainWindow()
        {
            InitializeComponent();
            yeetus.Text = "Upload a picture of you and your friends";
            yeetus2.Text = "then click a face to see what FaceFeed has to say about you!";

            if (Uri.IsWellFormedUriString(faceEndpoint, UriKind.Absolute))
            {
                faceClient.Endpoint = faceEndpoint;
            }
            else
            {
                MessageBox.Show(faceEndpoint,
                    "Invalid URI", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }
        private async void Titanic_Click(object sender, RoutedEventArgs e)
        {
            faceDescriptionStatusBar.Text = survive;
        }
        private async void Feeling_Click(object sender, RoutedEventArgs e)
        {
            faceDescriptionStatusBar.Text = feeling;
        }
        private async void Sloshed_Click(object sender, RoutedEventArgs e)
        {
            faceDescriptionStatusBar.Text = sloshed;
        }
        private async void Veg_Click(object sender, RoutedEventArgs e)
        {
            faceDescriptionStatusBar.Text = veg;
        }
        private async void Phone_Click(object sender, RoutedEventArgs e)
        {
            faceDescriptionStatusBar.Text = phone;
        }
        private async void Cheese_Click(object sender, RoutedEventArgs e)
        {
            faceDescriptionStatusBar.Text = cheese;
        }
        private async void Prog_Click(object sender, RoutedEventArgs e)
        {
            faceDescriptionStatusBar.Text = prog;
        }
        private async void Book_Click(object sender, RoutedEventArgs e)
        {
            faceDescriptionStatusBar.Text = book;
        }
        // Displays the image and calls UploadAndDetectFaces.
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the image file to scan from the user.
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog(this);

            // Return if canceled.
            if (!(bool)result)
            {
                return;
            }
            BrowseButton.Visibility = Visibility.Collapsed;
            // Display the image file.
            string filePath = openDlg.FileName;

            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();

            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();

            FacePhoto.Source = bitmapSource;

            // Detect any faces in the image.
            Title = "Detecting...";
            faceList = await UploadAndDetectFaces(filePath);
            Title = String.Format(
                "Detection Finished. {0} face(s) detected", faceList.Count);

            if (faceList.Count > 0)
            {
                // Prepare to draw rectangles around the faces.
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                resizeFactor = (dpi > 0) ? 96 / dpi : 1;
                faceDescriptions = new String[faceList.Count];

                for (int i = 0; i < faceList.Count; ++i)
                {
                    DetectedFace face = faceList[i];

                    // Draw a rectangle on the face.
                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red, 2),
                        new Rect(
                            face.FaceRectangle.Left * resizeFactor,
                            face.FaceRectangle.Top * resizeFactor,
                            face.FaceRectangle.Width * resizeFactor,
                            face.FaceRectangle.Height * resizeFactor
                            )
                    );

                    // Store the face description.
                    faceDescriptions[i] = FaceDescription(face);
                }

                drawingContext.Close();

                // Display the image with the rectangle around the face.
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(visual);
                FacePhoto.Source = faceWithRectBitmap;

                // Set the status bar text.
                faceDescriptionStatusBar.Text =
                    "";
            }
        }

        // Displays the face description when the mouse is over a face rectangle.
        private void FacePhoto_MouseMove(object sender, MouseEventArgs e)
        {
            // If the REST call has not completed, return.
            if (faceList == null)
                return;

            // Find the mouse position relative to the image.
            Point mouseXY = e.GetPosition(FacePhoto);

            ImageSource imageSource = FacePhoto.Source;
            BitmapSource bitmapSource = (BitmapSource)imageSource;

            // Scale adjustment between the actual size and displayed size.
            var scale = FacePhoto.ActualWidth / (bitmapSource.PixelWidth / resizeFactor);

            // Check if this mouse position is over a face rectangle.
            bool mouseOverFace = false;

            for (int i = 0; i < faceList.Count; ++i)
            {
                FaceRectangle fr = faceList[i].FaceRectangle;
                double left = fr.Left * scale;
                double top = fr.Top * scale;
                double width = fr.Width * scale;
                double height = fr.Height * scale;

                // Display the face description if the mouse is over this face rectangle.
                if (mouseXY.X >= left && mouseXY.X <= left + width &&
                    mouseXY.Y >= top && mouseXY.Y <= top + height)
                {
                    faceDescriptionStatusBar.Text = faceDescriptions[i];
                    mouseOverFace = true;
                    break;
                }
            }

            // String to display when the mouse is not over a face rectangle.
            if (!mouseOverFace)
                faceDescriptionStatusBar.Text =
                    " ";
        }
        // Uploads the image file and calls DetectWithStreamAsync.
        private async Task<IList<DetectedFace>> UploadAndDetectFaces(string imageFilePath)
        {
            // The list of Face attributes to return.
            IList<FaceAttributeType> faceAttributes =
                new FaceAttributeType[]
                {
            FaceAttributeType.Gender, FaceAttributeType.Age,
            FaceAttributeType.Smile, FaceAttributeType.Emotion,
            FaceAttributeType.Glasses, FaceAttributeType.Hair
                };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    // The second argument specifies to return the faceId, while
                    // the third argument specifies not to return face landmarks.
                    IList<DetectedFace> faceList =
                        await faceClient.Face.DetectWithStreamAsync(
                            imageFileStream, true, false, faceAttributes);
                    return faceList;
                }
            }
            // Catch and display Face API errors.
            catch (APIErrorException f)
            {
                MessageBox.Show(f.Message);
                return new List<DetectedFace>();
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                return new List<DetectedFace>();
            }
        }
        // Creates a string out of the attributes describing the face.
        private string FaceDescription(DetectedFace face)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Face: ");

            // Add the gender, age, and smile.
            sb.Append(face.FaceAttributes.Gender);
            sb.Append(", ");
            sb.Append(face.FaceAttributes.Age);
            sb.Append(", ");
            sb.Append(String.Format("smile {0:F1}%, ", face.FaceAttributes.Smile * 100));

            // Add the emotions. Display all emotions over 10%.
            sb.Append("Emotion: ");
            Emotion emotionScores = face.FaceAttributes.Emotion;
            if (emotionScores.Anger >= 0.1f)
                sb.Append(String.Format("anger {0:F1}%, ", emotionScores.Anger * 100));
            if (emotionScores.Contempt >= 0.1f)
                sb.Append(String.Format("contempt {0:F1}%, ", emotionScores.Contempt * 100));
            if (emotionScores.Disgust >= 0.1f)
                sb.Append(String.Format("disgust {0:F1}%, ", emotionScores.Disgust * 100));
            if (emotionScores.Fear >= 0.1f)
                sb.Append(String.Format("fear {0:F1}%, ", emotionScores.Fear * 100));
            if (emotionScores.Happiness >= 0.1f)
                sb.Append(String.Format("happiness {0:F1}%, ", emotionScores.Happiness * 100));
            if (emotionScores.Neutral >= 0.1f)
                sb.Append(String.Format("neutral {0:F1}%, ", emotionScores.Neutral * 100));
            if (emotionScores.Sadness >= 0.1f)
                sb.Append(String.Format("sadness {0:F1}%, ", emotionScores.Sadness * 100));
            if (emotionScores.Surprise >= 0.1f)
                sb.Append(String.Format("surprise {0:F1}%, ", emotionScores.Surprise * 100));

            // Add glasses.
            sb.Append(face.FaceAttributes.Glasses);
            sb.Append(", ");

            // Add hair.
            sb.Append("Hair: ");

            // Display baldness confidence if over 1%.
            if (face.FaceAttributes.Hair.Bald >= 0.01f)
                sb.Append(String.Format("bald {0:F1}% ", face.FaceAttributes.Hair.Bald * 100));

            // Display all hair color attributes over 10%.
            IList<HairColor> hairColors = face.FaceAttributes.Hair.HairColor;
            foreach (HairColor hairColor in hairColors)
            {
                if (hairColor.Confidence >= 0.1f)
                {
                    sb.Append(hairColor.Color.ToString());
                    sb.Append(String.Format(" {0:F1}% ", hairColor.Confidence * 100));
                }
            }
            // Return the built string.
            output = sb.ToString();
            //parse string for gender
            if (output.ToLower().Contains("fem")) {
                gender = "fem";
            }
            else if (output.ToLower().Contains("male"))
            {
                gender = "male";
            }
            //parse string for age
            string[] vs= output.Split(',');
            int a = Int32.Parse(vs[1]);
            if(a <= 14)
            {
                age = "young";
            }
            if ((a > 14) && (a <= 29))
            {
                age = "mid";
            }
            if ((a > 29))
            {
                age = "old";
            }
            //parse emotion
            if (output.ToLower().Contains("fear"))
            {
                emotion = "fear";
            }
            if (output.ToLower().Contains("anger"))
            {
                emotion = "anger";
            }
            if (output.ToLower().Contains("disgust"))
            {
                emotion = "disgust";
            }
            if (output.ToLower().Contains("sadness"))
            {
                emotion = "disgust";
            }
            if (output.ToLower().Contains("sadness"))
            {
                emotion = "sadness";
            }
            if (output.ToLower().Contains("neutral"))
            {
                emotion = "neutral";
            }
            if (output.ToLower().Contains("happiness"))
            {
                emotion = "happiness";
            }
            if (output.ToLower().Contains("surprise"))
            {
                emotion = "suprise";
            }
            if (output.ToLower().Contains("contempt"))
            {
                emotion = "contempt";
            }
            //BRUTE FORCE BABY
            //
            //
            //
            StringBuilder slosh = new StringBuilder();
            if (a >= 21) {
                slosh.Append("Yes!");
            }
            else {
                slosh.Append("No.");
            }
            sloshed = slosh.ToString();
            //TITANIC BABY
            StringBuilder willYouSurvive = new StringBuilder();
            if (string.Equals(emotion, "fear") && string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You Survive!");
            }
            else if (string.Equals(emotion, "fear") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You Survive!");
            }
            else if (string.Equals(emotion, "sadness") && string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You survive!");
            }
            else if (string.Equals(emotion, "sadness") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You survive!");
            }
            else if (string.Equals(emotion, "surprise") && string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You survive!");
            }
            else if (string.Equals(emotion, "surprise") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You survive!");
            }
            else if (string.Equals(emotion, "contempt") && string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You survive!");
            }
            else if (string.Equals(emotion, "contempt") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You survive!");
            }
            else if (string.Equals(emotion, "disgust") && string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "disgust") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You survive!");
            }
            else if (string.Equals(emotion, "anger") && string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You survive!");
            }
            else if (string.Equals(emotion, "anger") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "neutral") && string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "neutral") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "fear") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "fear") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "sadness") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "sadness") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("You probably survive!");
            }
            else if (string.Equals(emotion, "surprise") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you might survive.");
            }
            else if (string.Equals(emotion, "surprise") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("you might survive.");
            }
            else if (string.Equals(emotion, "contempt") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you might survive.");
            }
            else if (string.Equals(emotion, "contempt") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("you might survive.");
            }
            else if (string.Equals(emotion, "disgust") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you might survive.");
            }
            else if (string.Equals(emotion, "disgust") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("you might survive.");
            }
            else if (string.Equals(emotion, "anger") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you might survive, but probably not.");
            }
            else if (string.Equals(emotion, "anger") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("you might survive, but probably not.");
            }
            else if (string.Equals(emotion, "neutral") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you might survive, but probably not.");
            }
            else if (string.Equals(emotion, "neutral") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("you probably die.");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you probably die.");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("you probably die.");
            }
            else if (string.Equals(emotion, "fear") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you probably die.");
            }
            else if (string.Equals(emotion, "fear") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("you probably die.");
            }
            else if (string.Equals(emotion, "sadness") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you probably die");
            }
            else if (string.Equals(emotion, "sadness") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("you probably die");
            }
            else if (string.Equals(emotion, "surprise") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you probably die");
            }
            else if (string.Equals(emotion, "surprise") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("you probably die");
            }
            else if (string.Equals(emotion, "contempt") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("you probably die");
            }
            else if (string.Equals(emotion, "contempt") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("You might not make it friend.");
            }
            else if (string.Equals(emotion, "disgust") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("You might not make it friend.");
            }
            else if (string.Equals(emotion, "disgust") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("You might not make it friend.");
            }
            else if (string.Equals(emotion, "anger") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("You might not make it friend.");
            }
            else if (string.Equals(emotion, "anger") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("You might not make it friend.");
            }
            else if (string.Equals(emotion, "neutral") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("You might not make it friend.");
            }
            else if (string.Equals(emotion, "neutral") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("You die.");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                willYouSurvive.Append("You die.");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                willYouSurvive.Append("You die.");
            }
            else
            {
                willYouSurvive.Append("You probably die");
            }
            survive = willYouSurvive.ToString();
            StringBuilder spiritCheese = new StringBuilder();
            if (string.Equals(emotion, "happiness") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                spiritCheese.Append("provolone.");
            }
            else if (string.Equals(emotion, "angry") && string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                spiritCheese.Append("parmigiano.");
            }
            else if (string.Equals(emotion, "neutral") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                spiritCheese.Append("Swiss.");
            }
            else if (string.Equals(emotion, "disgusted") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                spiritCheese.Append("Gouda.");
            }
            else if (string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                spiritCheese.Append("Cheddar.");
            }
            else if (string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                spiritCheese.Append("Mozzarella.");
            }
            else if (string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                spiritCheese.Append("Brie.");
            }
            else if (string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                spiritCheese.Append("Feta.");
            }
            else if (string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                spiritCheese.Append("Blue Cheese.");
            }
            else if (string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                spiritCheese.Append("Gorgonzola.");
            }
            else if (string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                spiritCheese.Append("Cottage Cheese.");
            }
            else
            {
                spiritCheese.Append("Cream Cheese");
            }
            cheese = spiritCheese.ToString();
            StringBuilder progLang = new StringBuilder();
            if (string.Equals(emotion, "neutral") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                progLang.Append("JavaScript");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                progLang.Append("Swift");
            }
            else if (string.Equals(emotion, "contempt") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                progLang.Append("Scratch");
            }
            else if (string.Equals(emotion, "sadness") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                progLang.Append("Kafka");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                progLang.Append("Pascal");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(age, "mid"))
            {
                progLang.Append("C++");
            }
            else if (string.Equals(emotion, "surprise") && string.Equals(age, "old"))
            {
                progLang.Append("Html");
            }
            else if (string.Equals(emotion, "surprise") && string.Equals(gender, "fem"))
            {
                progLang.Append("Php");
            }
            else if (string.Equals(emotion, "fear") && string.Equals(age, "mid"))
            {
                progLang.Append("SQL");
            }
            else if (string.Equals(emotion, "happy") && string.Equals(age, "young"))
            {
                progLang.Append("Python");
            }
            else if (string.Equals(emotion, "sadness"))
            {
                progLang.Append("C");
            }
            else if (string.Equals(emotion, "disgust"))
            {
                progLang.Append("C#");
            }
            else if (string.Equals(age, "young"))
            {
                progLang.Append("R");
            }
            else
            {
                progLang.Append("Java");
            }
            prog = progLang.ToString();
            StringBuilder greenVeg = new StringBuilder();
            if (string.Equals(emotion, "sadness") && string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                greenVeg.Append("Broccoli Rabe");
            }
            else if (string.Equals(emotion, "sadness") && string.Equals(gender, "fem") && string.Equals(age, "old"))
            {
                greenVeg.Append("Turnip");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                greenVeg.Append("Spinach");
            }
            else if (string.Equals(emotion, "happiness") && string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                greenVeg.Append("Iceberg Lettuce");
            }
            else if (string.Equals(emotion, "surprised") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                greenVeg.Append("Cabbage");
            }
            else if (string.Equals(emotion, "disgust") && string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                greenVeg.Append("Asparagus");
            }
            else if (string.Equals(emotion, "anger") && string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                greenVeg.Append("Zucchini");
            }
            else if (string.Equals(gender, "fem") && string.Equals(age, "mid"))
            {
                greenVeg.Append("Kale");
            }
            else if (string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                greenVeg.Append("Swiss Chard");
            }
            else if (string.Equals(gender, "male") && string.Equals(emotion, "surprise"))
            {
                greenVeg.Append("Arugala");
            }
            else if (string.Equals(gender, "male") && string.Equals(age, "old"))
            {
                greenVeg.Append("Green Beans");
            }
            else if (string.Equals(gender, "male") && string.Equals(age, "mid"))
            {
                greenVeg.Append("Cucumber");
            }
            else if (string.Equals(gender, "male") && string.Equals(age, "young"))
            {
                greenVeg.Append("Peas");
            }
            else if(string.Equals(gender, "fem") && string.Equals(age, "young"))
            {
                greenVeg.Append("Celery");
            }
            else
            {
                greenVeg.Append("Swiss Chard");
            }
            veg = greenVeg.ToString();
            StringBuilder bookRec = new StringBuilder();
            if (string.Equals(age, "young") && string.Equals(emotion, "fear"))
            {
                bookRec.Append("Percy Jackson the Lightning Thief, by Rick Riordan");
            }
            else if (string.Equals(age, "young") && string.Equals(gender, "fem"))
            {
                bookRec.Append("Harry Potter and the Philosopher’s Stone, by J.K. Rowling");
            }
            else if (string.Equals(age, "young") && string.Equals(emotion, "anger"))
            {
                bookRec.Append("Artemis Fowl, by Eoin Colfer");
            }
            else if (string.Equals(age, "young") && string.Equals(emotion, "surprise"))
            {
                bookRec.Append("Eragon, by Christopher Paolini");
            }
            else if (string.Equals(age, "young") && string.Equals(gender, "male"))
            {
                bookRec.Append("Lord of the Rings, by J.R.R. Tolkien");
            }
            else if (string.Equals(age, "young") && string.Equals(gender, "fem"))
            {
                bookRec.Append("Harry Potter and the Philosopher’s Stone, by J.K. Rowling");
            }
            if (string.Equals(age, "mid") && string.Equals(emotion, "anger"))
            {
                bookRec.Append("The Girl with the Dragon Tattoo, Stieg Larsson");
            }
            else if (string.Equals(age, "mid") && string.Equals(emotion, "sadness"))
            {
                bookRec.Append("The Road, Cormac McCarthy ");
            }
            else if (string.Equals(age, "mid") && string.Equals(emotion, "happiness"))
            {
                bookRec.Append("Ready Player One, by Ernest Cline");
            }
            else if (string.Equals(age, "mid") && string.Equals(gender, "fem"))
            {
                bookRec.Append("Yes Please, Amy Poehler ");
            }
            else if (string.Equals(age, "mid") && string.Equals(gender, "male"))
            {
                bookRec.Append("Fight Club, Chuck Palahniuk ");
            }
            if (string.Equals(age, "old") && string.Equals(emotion, "anger"))
            {
                bookRec.Append("The Great Gatsby, by F. Scott Fitzgerald");
            }
            else if (string.Equals(age, "old") && string.Equals(emotion, "sadness"))
            {
                bookRec.Append("Infinite Jest, by David Foster Wallace ");
            }
            else if (string.Equals(age, "old") && string.Equals(emotion, "happiness"))
            {
                bookRec.Append("Wonder, by RJ Palacio");
            }
            else if (string.Equals(age, "mid") && string.Equals(gender, "female"))
            {
                bookRec.Append("Eat Pray Love, by Elizabeth Gilbert ");
            }
            else if (string.Equals(age, "mid") && string.Equals(gender, "male"))
            {
                bookRec.Append("The Fight, by Norman Mailer");
            }
            book = bookRec.ToString();
            //emotions
            StringBuilder howUFeel = new StringBuilder();
            if (string.Equals(emotion, "contempt"))
            {
                howUFeel.Append("Full of contempt");
            }
            else if (string.Equals(emotion, "surprise"))
            {
                howUFeel.Append("Surprised");
            }
            else if (string.Equals(emotion, "happiness"))
            {
                howUFeel.Append("Happy");
            }
            else if (string.Equals(emotion, "neutral"))
            {
                howUFeel.Append("Neutral");
            }
            else if (string.Equals(emotion, "sadness"))
            {
                howUFeel.Append("Sad");
            }
            else if (string.Equals(emotion, "disgust"))
            {
                howUFeel.Append("Disgusted");
            }
            else if (string.Equals(emotion, "anger"))
            {
                howUFeel.Append("Angry");
            }
            else if (string.Equals(emotion, "fear"))
            {
                howUFeel.Append("Afraid");
            }
            feeling = howUFeel.ToString();

            //phone usage
            StringBuilder mostPhone = new StringBuilder();
            if (string.Equals(age, "young"))
            {
                mostPhone.Append("Playing Games");
            }
            if (string.Equals(age, "mid"))
            {
                mostPhone.Append("Mobile Chatting");
            }
            if (string.Equals(age, "old"))
            {
                mostPhone.Append("Calling and Texting");
            }
            phone = mostPhone.ToString();
            return final;
        }
    }
}
