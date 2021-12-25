using Plugin.Media;
using Xamarin.Forms;
using EDPlayer.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.IO;

namespace EDPlayer
{
    public partial class MainPage : ContentPage
    {
        public string subscriptionKey = "bf70c4160d4c404cb31ad6d8dbe706de";

        public string uriBase = "https://centralindia.api.cognitive.microsoft.com/face/v1.0/detect";

        public MainPage()
        {
            InitializeComponent();
        }

        async void btnPick_Clicked(object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (file == null) return;
                imgSelected.Source = ImageSource.FromStream(() => {
                    var stream = file.GetStream();
                    return stream;
                });
                MakeAnalysisRequest(file.Path);
            }
            catch (Exception ex)
            {
                string test = ex.Message;
            }

        }


        public async void MakeAnalysisRequest(string imageFilePath)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            string uri = uriBase + "?" + requestParameters;
            HttpResponseMessage response;
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);

                string contentString = await response.Content.ReadAsStringAsync();

                List<ResponseModel> faceDetails = JsonConvert.DeserializeObject<List<ResponseModel>>(contentString);
                if (faceDetails.Count != 0)
                {
                    double Happiness = faceDetails[0].faceAttributes.emotion.happiness;
                    double Sadness = faceDetails[0].faceAttributes.emotion.sadness;
                    double Anger = faceDetails[0].faceAttributes.emotion.anger;
                    double Fear = faceDetails[0].faceAttributes.emotion.fear;
                    double Neutral = faceDetails[0].faceAttributes.emotion.neutral;
                    double Surprise = faceDetails[0].faceAttributes.emotion.surprise;
                    double Disgust = faceDetails[0].faceAttributes.emotion.disgust;
                    double Contempt = faceDetails[0].faceAttributes.emotion.contempt;

                    double Highest = Happiness;
                    string HighestMood = "Happiness";
                    if (Highest < Sadness) { Highest = Sadness; HighestMood = "Sadness"; }
                    if (Highest < Anger) { Highest = Anger; HighestMood = "Anger"; }
                    if (Highest < Fear) { Highest = Fear; HighestMood = "Fear"; }
                    if (Highest < Neutral) { Highest = Neutral; HighestMood = "Neutral"; }
                    if (Highest < Surprise) { Highest = Surprise; HighestMood = "Surprise"; }
                    if (Highest < Disgust) { Highest = Disgust; HighestMood = "Disgust"; }
                    if (Highest < Contempt) { Highest = Contempt; HighestMood = "Contempt"; }

                    lblMood.Text = "You are feeling : " + HighestMood;     
                }

            }
        }
        public byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}